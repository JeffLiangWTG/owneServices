using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.H7.Business;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public class GbCDSH7ImportDeclarationWrapper : IImportDeclaration
	{
		public GbCDSH7ImportDeclarationWrapper(MessageSendingObject sendingObject)
		{
			this.sendingObject = sendingObject;
			this.bill = (AsycudaBill)sendingObject.Bill;
			this.header = bill.Header;
		}

		protected readonly MessageSendingObject sendingObject;
		protected readonly AsycudaBill bill;
		protected readonly AsycudaManifestHeader header;

		public ZString DeclarationTypeCode => EntryStyleListImport.Codes.ImportNormal + sendingObject.SubStyle;

		public ZString SpecificCircumstancesCodeCode => ZString.Empty;

		public ZInt GoodsItemQuantity => 1;

		public GbCDSH7ImportGoodsShipmentWrapper GoodsShipmentWrapper => goodsShipmentWrapper ?? (goodsShipmentWrapper = GetGoodsShipmentWrapper(bill));
		GbCDSH7ImportGoodsShipmentWrapper goodsShipmentWrapper;

		IGoodsShipment IDeclaration.GoodsShipment => GoodsShipmentWrapper;

		GbCDSH7ImportGoodsShipmentWrapper GetGoodsShipmentWrapper(AsycudaBill bill)
		{
			return new GbCDSH7ImportGoodsShipmentWrapper(bill, IsBIRDSMessage);
		}

		public ZString FunctionalReferenceID => bill.LocalReferenceNumber;

		public IEnumerable<IDecAdditionalDocument> DecAdditionalDocuments => null;

		public IOrganisation Exporter
		{
			get
			{
				IOrganisation result;
				var address = AddressWrapper.New(bill.ABL_ShipperStreet1 + bill.ABL_ShipperStreet2, bill.ABL_ShipperCity, bill.ABL_RN_NKShipperCountry, bill.ABL_ShipperPostcode);
				result = OrganisationWrapper.New(bill.ABL_ShipperName, ZString.Empty, address, false);

				return result;
			}
		}

		public IOrganisation ExporterNameAndAddress => null;

		public IOrganisation Declarant => header.Declarant != null ?
			OrganisationWrapper.New(header.Declarant, header.Declarant.Header.GetEuIdentificationNumber()) : null;

		public IAgent Agent => header.Representative != null
			? AgentWrapper.New(OrganisationWrapper.New(header.Representative, 70, 35), header.RepresentativeStatusCode)
			: AgentWrapper.New(null, header.RepresentativeStatusCode);

		public IEnumerable<IAuthorisationHolder> AuthorisationHolders
		{
			get
			{
				if (IsBIRDSMessage && header.BIRDSPermitHeader != null)
				{
					return [AuthorisationHolderWrapper.New(header.Declarant.Header.GetEuIdentificationNumber(), CDSAuthorisationHeaderTypeList.Codes.BulkImportReducedDataSetAuthorisation)];
				}

				return null;
			}
		}

		public IEnumerable<ICurrencyExchange> CurrencyExchanges => null;

		public ZString PresentationOffice => header.PresentationOffice;

		public ZString SupervisingOffice => header.SupervisingOffice?.LocalCustomsClientCode ?? ZString.Empty;

		public ZDecimal TotalPackageQuantity => CachedValueHelper.GetValue(ref totalPackageQuantity, CountPackageQuantity);
		CachedValue<ZDecimal> totalPackageQuantity;

		ZDecimal CountPackageQuantity()
		{
			var quantity = 0m;

			if (IsBIRDSMessage)
			{
				foreach (var bill in header.Bills)
				{
					foreach (var pack in bill.Packs.OfType<AsycudaPack>())
					{
						quantity += pack.APA_PackQty;
					}
				}
			}

			return quantity;
		}

		public ITransportMeans BorderTransportMeans => TransportMeansWrapper.New(string.Empty, GetTransportModeCode(header.AMA_TransportMode), string.Empty, string.Empty);

		public IEnumerable<IObligationGuarantee> ObligationGuarantees => null;

		public ZDateTime AcceptanceDateTime => ZDateTime.Empty;

		public ZDecimal TotalGrossMassMeasure => bill.MassInKilos;

		public IConsignment Consignment => null;

		public ZString ExitOfficeID => string.Empty;

		public IAmountAndCurrency InvoiceAmount => null;

		public IAmountAndCurrency FreightChargeAmount => AmountAndCurrencyWrapper.New(bill.ABL_TransportValue, bill.ABL_RX_NKTransportValueCurrency);

		public IEnumerable<EU.Integration.SadH.IStatement> AdditionalInformations => new List<EU.Integration.SadH.IStatement>();

		ZString GetTransportModeCode(ZString transportMode)
		{
			switch (transportMode)
			{
				case Constants.TransportModes.Sea:
					return "1";
				case Constants.TransportModes.Rail:
					return "2";
				case Constants.TransportModes.Road:
					return "3";
				case Constants.TransportModes.Air:
					return "4";
				case Constants.TransportModes.Mail:
					return "5";
				default:
					return transportMode;
			}
		}

		bool IsBIRDSMessage => !header.PortOfDischarge?.IsInNorthernIreland ?? false;
	}
}
