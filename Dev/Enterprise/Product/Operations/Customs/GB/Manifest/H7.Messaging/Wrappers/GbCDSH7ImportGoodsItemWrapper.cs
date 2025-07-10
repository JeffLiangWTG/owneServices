using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.H7.Business;
using static Enterprise.Customs.GB.CDS.Constants.Classification;
using IOrganisation = Enterprise.Customs.GB.CDS.Messaging.IOrganisation;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public class GbCDSH7ImportGoodsItemWrapper : IGovernmentAgencyGoodsItem, ICommodity
	{
		public GbCDSH7ImportGoodsItemWrapper(AsycudaPackedItem packedItem, bool isBIRDSMessage)
		{
			this.packedItem = packedItem;
			this.bill = packedItem.Bill;
			this.isBIRDSMessage = isBIRDSMessage;
		}

		readonly AsycudaPackedItem packedItem;
		readonly AsycudaBill bill;
		readonly bool isBIRDSMessage;

		public IEnumerable<CDS.Messaging.IPreviousDocument> PreviousDocuments =>
			bill.PreviousDocuments.Concat(packedItem.PreviousDocuments).Select(previousDocument => new GbCDSH7PreviousDocumentWrapper(previousDocument));

		public IEnumerable<IStatement> AdditionalInformations =>
			packedItem.AdditionalInfos
				.Select(additionalInfo => new Statement(additionalInfo.CSI_Code, additionalInfo.CSI_Description))
				.Concat(bill.AdditionalInfos.Select(additionalInfo => new Statement(additionalInfo.CSI_Code, additionalInfo.CSI_Description)));

		public IEnumerable<IAdditionalDocument> AdditionalDocuments =>
			packedItem.SupportingDocuments
				.Select(supportingDocument => new GbCDSH7AdditionalDocumentWrapper(supportingDocument))
				.Concat(bill.SupportingDocuments.Select(supportingDocument => new GbCDSH7AdditionalDocumentWrapper(supportingDocument)));

		public IEnumerable<IGovernmentProcedure> GovernmentProcedures
		{
			get
			{
				var procedures = new List<IGovernmentProcedure>();

				if (!isBIRDSMessage)
				{
					var loader = new Universal.RefCusProcedure.Loader(bill.Factory);
					var mainRefCusProcedure = loader.LoadTop1FromFullCodeCurrentPlusPreviousPlusConcession(bill.ABL_Procedure, "CDS", ZDateTime.Empty);

					if (mainRefCusProcedure != null)
					{
						procedures.Add(GovernmentProcedureWrapper.New(mainRefCusProcedure.ZZ6_ProcedureCode, mainRefCusProcedure.ZZ6_PreviousProcedureCode));
						procedures.Add(GovernmentProcedureWrapper.New(mainRefCusProcedure.ZZ6_Concession, string.Empty));
					}

					foreach (var additionalProcedureCode in bill.AdditionalProcedureCodes)
					{
						var additionalRefCusProcedure = loader.LoadTop1FromFullCodeCurrentPlusPreviousPlusConcession(additionalProcedureCode.CY_Code, "CDS", ZDateTime.Empty);

						if ((additionalRefCusProcedure != null) && (additionalRefCusProcedure.ZZ6_Concession != ZString.Empty))
						{
							procedures.Add(GovernmentProcedureWrapper.New(additionalRefCusProcedure.ZZ6_Concession, string.Empty));
						}
					}
				}
				else
				{
					const string procedureCode00 = "00";
					const string procedureCode20 = "20";
					const string procedureCode21V = "21V";
					const string procedureCode15F = "15F";

					procedures.Add(GovernmentProcedureWrapper.New(procedureCode00, procedureCode20));
					procedures.Add(GovernmentProcedureWrapper.New(procedureCode21V, string.Empty));

					if (bill.ABL_RL_NKOrigin.StartsWith(Constants.CountryCodes.Jersey) || bill.ABL_RL_NKOrigin.StartsWith(Constants.CountryCodes.Guernsey))
					{
						procedures.Add(GovernmentProcedureWrapper.New(procedureCode15F, string.Empty));
					}
				}

				return procedures;
			}
		}

		public IOrganisation Consignor => null;

		public IOrganisation Seller => bill.GetShipperOrg();

		public IOrganisation Buyer => bill.GetConsigneeOrg();

		public IEnumerable<IParty> AEOMutualRecognitionParties => null;

		public IEnumerable<IParty> DomesticDutyTaxParties => null;

		public ZString ValuationAdjustmentAdditionCode => null;

		public ICustomsValuation CustomsValuation => null;

		public ZString DestinationCountryCode => null;

		public IEnumerable<IPackaging> Packagings => isBIRDSMessage ? GetPackagingsForBIRDSMessage() : null;

		IEnumerable<IPackaging> GetPackagingsForBIRDSMessage()
		{
			return packedItem.AsycudaPackPackedItemLinks.Where(p => p.IsLinked).Select(p => GetPackaging(p.Package, p.PackQty));
		}

		IPackaging GetPackaging(EU.H7.Business.AsycudaPack pack, ZInt quantity)
		{
			return PackagingWrapper.New(pack.APA_PackUQ, (ZDecimal)quantity, pack.APA_MarksAndNumbers);
		}

		public ZString TransactionNatureCode => null;

		public IAmountAndCurrency StatisticalValue => null;

		public ICommodity Commodity => this;

		public IEnumerable<ICountry> Origins => null;

		public ZString TransportChargesMethodOfPayment => null;

		public ZString ExportCountryCode => null;

		#region ICommodity

		public ZString Description => packedItem.API_GoodsDescription;

		public IEnumerable<IClassification> Classifications => new[] { ClassificationWrapper.New(packedItem.API_Tariff.Left(6), IdentificationTypeCodes.TSP) };

		public ZString UNDGID => null;

		public IEnumerable<IDutyTaxFee> DutyTaxFees => null;

		public IAmountAndCurrency InvoiceLineItemCharge => AmountAndCurrencyWrapper.New(packedItem.API_GoodsValue, packedItem.API_RX_NKGoodsValueCurrency);

		public ZDecimal NetWeight => Constants.Weight.ConvertSafe(packedItem.API_NetWeight, packedItem.API_NetWeightUQ, Constants.Weight.Kilograms);

		public ZDecimal GrossWeight => Constants.Weight.ConvertSafe(packedItem.API_GrossWeight, packedItem.API_GrossWeightUQ, Constants.Weight.Kilograms);

		public ZDecimal TariffQuantity => packedItem.API_CustomsQty2.Normalize();

		public IEnumerable<ZString> TransportEquipmentIDs => null;

		public bool NoAdditionalProcedureCodesAreE01orE02 => false;

		#endregion
	}
}
