using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class DocCusEntryHeader : DocBaseCusEntryHeader
	{
		DocCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
			: base(cusEntryHeader, factoryToWrap)
		{
			languageCode = TranslationHelper.GetLanguageCodeForCountry(Core.Constants.CountryCodes.Brazil);
		}
		readonly ZString languageCode;

		public static DocCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
		{
			return cusEntryHeader == null ? null : new DocCusEntryHeader(cusEntryHeader, factoryToWrap);
		}

		#region Wrapped BizObj

		public DocDeclaration Declaration => DocDeclaration.New(CusEntryHeader.Declaration, Factory);

		#endregion

		#region Wrapped Fields

		public CodeAndDescriptionWrapper TypeOfOperationExport => BRPairListHelper.CreateCodeAndDescriptionWrapper(Factory.GetCachedValue<TypeOfOperationExportList>(), CusEntryHeader.Declaration.JE_DeclarantType, Factory);

		public CodeAndDescriptionWrapper SpecialCustomsClearance => BRPairListHelper.CreateCodeAndDescriptionWrapper(Factory.GetCachedValue<SpecialCustomsClearanceList>(), CusEntryHeader.EntryInstruction?.CEI_SpecialCustomsClearance ?? ZString.Empty, Factory);

		public CodeAndDescriptionWrapper LegalDocument => BRPairListHelper.CreateCodeAndDescriptionWrapper(Factory.GetCachedValue<LegalDocumentList>(), CusEntryHeader.EntryInstruction?.CEI_LegalDocument ?? ZString.Empty, Factory);

		public CodeAndDescriptionWrapper ClearanceOffice => BRPairListHelper.CreateCodeAndDescriptionWrapper(CustomsOfficeList, CusEntryHeader.Declaration.JE_CustomsOffice, Factory);

		public CodeAndDescriptionWrapper BoardingOffice => BRPairListHelper.CreateCodeAndDescriptionWrapper(CustomsOfficeList, CusEntryHeader.Declaration.BoardingOfficeCode, Factory);

		public CodeAndDescriptionWrapper ClearanceEnclosure => BRPairListHelper.CreateCodeAndDescriptionWrapper(CustomsEnclosureList, CusEntryHeader.Declaration.JE_LocationOfGoods, Factory);

		public CodeAndDescriptionWrapper EntranceOffice => BRPairListHelper.CreateCodeAndDescriptionWrapper(CustomsOfficeList, CusEntryHeader.Declaration.EntranceOfficeCode, Factory);

		public CodeAndDescriptionWrapper BoardingEnclosure => BRPairListHelper.CreateCodeAndDescriptionWrapper(CustomsEnclosureList, CusEntryHeader.Declaration.BoardingEnclosureCode, Factory);

		public CodeAndDescriptionWrapper CargoArrivalDocumentType => BRPairListHelper.CreateCodeAndDescriptionWrapper(Factory.GetCachedValue<BRCargoArrivalDocList>(), CusEntryHeader.Declaration.JE_CargoArrivalDocumentType, Factory);

		public CodeAndDescriptionWrapper CargoStatus => BRPairListHelper.CreateCodeAndDescriptionWrapper(Factory.GetCachedValue<BRCargoStatusList>(), CusEntryHeader.CH_CargoStatus, Factory);

		public CodeAndDescriptionWrapper AdministrativeStatus => BRPairListHelper.CreateCodeAndDescriptionWrapper(Factory.GetCachedValue<BRAdministrativeStatusList>(), CusEntryHeader.CH_AdministrativeStatus, Factory);

		public CodeAndDescriptionWrapper EntryStatus => BRPairListHelper.CreateCodeAndDescriptionWrapper(EntryStatusList, CusEntryHeader.CH_EntryStatus, Factory);

		public CodeAndDescriptionWrapper Sector => BRPairListHelper.CreateCodeAndDescriptionWrapper(SubLocationOfGoodsList, CusEntryHeader.Declaration.JE_SubLocationOfGoods, Factory);

		public JobDocAddress ClearanceLocalInvolvedParty => CusEntryHeader.Declaration.ClearanceLocalInvolvedParty;

		public JobDocAddress BoardingLocalAddress => CusEntryHeader.Declaration.BoardingLocalAddress;

		public DocCountry CargoProvenance => CusEntryHeader.Declaration.JE_GoodsOrigin.IsEmpty ? null : DocCountry.New(Factory, CusEntryHeader.Declaration.JE_GoodsOrigin);

		public DocOrganisation Declarant => DocOrganisation.New(CusEntryHeader.Declaration.DeclarantAddress?.Header, Factory);

		#endregion

		#region ZDecimal Fields

		public ZDecimal NetWeight => InvoiceLines.Cast<DocJobComInvoiceLine>().Sum(line => line.NetWeightInKG);

		public ZDecimal GrossWeight => WeightUnits.ConvertWeightToKilogramsIfRequired(CusEntryHeader.Declaration.JE_TotalWeight, CusEntryHeader.Declaration.JE_TotalWeightUnit);

		public ZDecimal TotalFreightInLocalCurrency => CusEntryHeader.CurrencyConverter.ConvertRounded(CusEntryHeader.OverseasFreight, CusEntryHeader.LocalCurrency).Amount;

		public ZDecimal TotalInsuranceInLocalCurrency => CusEntryHeader.CurrencyConverter.ConvertRounded(CusEntryHeader.OverseasInsurance, CusEntryHeader.LocalCurrency).Amount;

		public ZDecimal TotalFOBInLocalCurrency => CusEntryHeader.FOBInLocalCurrency.Amount;

		public ZDecimal TotalCustomsValue => CusEntryHeader.CustomsValue;

		public ZDecimal TotalDutyAmount => CusEntryHeader.MergedLines.Sum(s => s.Fees.GetAmount(Enterprise.Customs.Business.ChargeTypesList.Codes.DTY));

		public ZDecimal TotalIPIChargeAmount => CusEntryHeader.MergedLines.Sum(s => s.Fees.GetAmount(Constants.RateTypes.IPI));

		public ZDecimal TotalPISChargeAmount => CusEntryHeader.MergedLines.Sum(s => s.Fees.GetAmount(Constants.RateTypes.PIS));

		public ZDecimal TotalCofinsChargeAmount => CusEntryHeader.MergedLines.Sum(s => s.Fees.GetAmount(Constants.RateTypes.Cofins));

		public ZDecimal TotalAntidumpingChargeAmount => CusEntryHeader.MergedLines.Sum(s => s.Fees.GetAmount(Constants.RateTypes.Antidumping));

		public ZDecimal TotalICMSChargeAmount => CusEntryHeader.MergedLines.Sum(s => s.Fees.GetAmount(Constants.RateTypes.ICMS).Round(2));

		public ZDecimal TotalFOBInInvoiceHeaders => CusEntryHeader.FOB.Amount;

		public ZDecimal TotalCustomsValueInInvoiceHeaders => CusEntryHeader.CIF.Amount;

		public ZDecimal TotalFCPChargeAmount => CusEntryHeader.MergedLines.Sum(s => s.Fees.GetAmount(Constants.RateTypes.ICMSFCP).Round(2));

		public ZDecimal TotalNFeLinePrice => InvoiceLines.Cast<DocJobComInvoiceLine>().Sum(s => s.NFeLinePrice.Round(2));

		#endregion

		#region ZDateTime fields

		public ZDateTime EntryIssueDate => CusEntryHeader.MovementReferenceNumberIssueDate;

		public ZDateTime ValidityILShipmentDate => CusEntryHeader.CH_ValidityILShipmentDate;

		public ZDateTime ValidityILDispatchDate => CusEntryHeader.CH_ValidityILDispatchDate;

		#endregion

		#region ZString Fields

		public ZString UCRNumber => CusEntryHeader.UniqueConsignmentReference;

		public ZString NetWeightUQ => Core.Constants.Weight.Kilograms;

		public ZString GrossWeightUQ => Core.Constants.Weight.Kilograms;

		public ZString CargoArrivalDocumentNumber => CusEntryHeader.Declaration.JE_CargoArrivalDocumentNumber;

		public ZString WarehouseAreasConcatenated => CusEntryHeader.Declaration.WarehouseAreasConcatenated;

		public ZString AdditionalInformation => CusEntryHeader.EntryInstruction?.AdditionalInformation ?? ZString.Empty;

		public ZString LocalCurrency => GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		public ZString EntryAccessKey => CusEntryHeader.EntryAccessKey;

		public ZString InvoiceHeadersFOBCurrency => CusEntryHeader.FOB.Currency.Code;

		public ZString InvoiceHeadersCustomsValueCurrency => CusEntryHeader.CIF.Currency.Code;

		#endregion

		#region ZBool Fields

		public ZBool IsConsortedExport => CusEntryHeader.EntryInstruction?.CEI_IsConsortedExport ?? false;

		public ZBool BoardingOfficeIsCustomsEnclosure => CusEntryHeader.Declaration.BoardingOfficeIsCustomsEnclosure;

		public ZBool ClearanceOfficeIsCustomsEnclosure => CusEntryHeader.Declaration.ClearanceOfficeIsCustomsEnclosure;

		public ZBool ClearanceOfficeIsHomeDispatch => CusEntryHeader.Declaration.ClearanceOfficeIsHomeDispatch;

		#endregion

		#region Collections

		public DocCusEntryLineCollection EntryLines
		{
			get
			{
				if (fEntryLines == null)
				{
					fEntryLines = new DocCusEntryLineCollection(CusEntryHeader.MergedLines, Factory);
					fEntryLines.Sort("LineNumber", System.ComponentModel.ListSortDirection.Ascending);
				}
				return fEntryLines;
			}
		}
		DocCusEntryLineCollection fEntryLines;

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get
			{
				if (invoiceLines == null)
				{
					invoiceLines = new DocJobComInvoiceLineCollection(Factory);

					foreach (JobComInvoiceLine line in CusEntryHeader.InvoiceLines)
					{
						invoiceLines.Add(DocJobComInvoiceLine.New(line, Factory));
					}
				}
				return invoiceLines;
			}
		}
		protected DocJobComInvoiceLineCollection invoiceLines;

		public DocJobComInvoiceHeaderCollection InvoiceHeaders
		{
			get
			{
				if (invoiceHeaders == null)
				{
					invoiceHeaders = new DocJobComInvoiceHeaderCollection(Factory);

					foreach (var line in CusEntryHeader.InvoiceHeaders)
					{
						invoiceHeaders.Add(DocJobComInvoiceHeader.New(line, Factory));
					}
				}
				return invoiceHeaders;
			}
		}
		protected DocJobComInvoiceHeaderCollection invoiceHeaders;

		public DocPackageCollection Packs
		{
			get
			{
				if (packs == null)
				{
					packs = new DocPackageCollection(CusEntryHeader.Packages, Factory);
					packs.Sort(nameof(DocPackage.Unit), System.ComponentModel.ListSortDirection.Ascending);
				}
				return packs;
			}
		}
		protected DocPackageCollection packs;

		#endregion

		#region Implementation

		CusEntryHeader CusEntryHeader => (CusEntryHeader)WrappedObject;

		CodeDescriptionPairList CustomsOfficeList => BRRefCusCodeListTypes.GetCustomsOfficeList(Factory, languageCode);

		CodeDescriptionPairList CustomsEnclosureList => BRRefCusCodeListTypes.GetCustomsEnclosureList(Factory, languageCode);

		CodeDescriptionPairList EntryStatusList => BRRefCusCodeListTypes.GetEntryStatusList(Factory, languageCode);

		CodeDescriptionPairList SubLocationOfGoodsList => BRRefCusCodeListTypes.GetWarehousingSectorList(Factory, CusEntryHeader.Declaration.JE_CustomsOffice, CusEntryHeader.Declaration.JE_LocationOfGoods, languageCode);

		#endregion
	}
}
