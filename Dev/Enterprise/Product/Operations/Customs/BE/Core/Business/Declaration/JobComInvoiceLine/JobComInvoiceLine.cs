using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public class JobComInvoiceLine : AutoJobComInvoiceLine
	, Integration.Customs.BE.IJobComInvoiceLine
{
	public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.JobComInvoiceLine.Schema
	{
		public new const int JI_StateOrRegionOfOriginMaxLength = 1;
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

	public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

	public override ZInt MaxNumberOfAdditionalProcedureCode => Declaration != null ? (Declaration.IsImport || Declaration.IsExport ? 99 : 0) : 0;

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups() => new JobComInvoiceLineLookups(this);

	protected override System.Collections.Generic.IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		return result;
	}

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation() => IsImport ? new ImportJobComInvoiceLineValidation(this) : new JobComInvoiceLineValidation(this);

	protected override EU.Business.Declaration.AddInfoJobComInvoiceLine GetNewAddInfo() => new AddInfoJobComInvoiceLine(JI_AddInfoInfo);

	public new AddInfoJobComInvoiceLineLookups AddInfoLookups => (AddInfoJobComInvoiceLineLookups)base.AddInfoLookups;

	protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Belgium;

	protected override Type TypeOfPartUsedCore => typeof(MasterFiles.OrgSupplierPart);

	[ResourceStringData("BA2E98CC-3F6A-4403-AE55-18392C4AEC0F", Caption = "BTW")]
	public override ZString JI_ZZF_NKTaxType { get => base.JI_ZZF_NKTaxType; set => base.JI_ZZF_NKTaxType = value; }

	[ResourceStringData("E36BC71A-4199-4D24-9407-78D7814E8A3E", Caption = "[UCC 5/8] Dest.", FullDescription = "[UCC 5/8] Country of Destination")]
	[ResourceStringData("947B212A-98B8-44E9-93CE-40AFC0BEBBE8", Caption = "[UCC 5/8] Dest.", FullDescription = "[UCC 5/8] Country of Destination", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	public override ZString ZG_CountryOfDestination { get => base.ZG_CountryOfDestination; set => base.ZG_CountryOfDestination = value; }

	[ResourceStringData("08894DED-C2EA-489D-BA45-F32DC156A454", ShortCaption = "[UCC 5/14] Disp.", Caption = "[UCC 5/14] Dispatch", FullDescription = "[UCC 5/14] Country of Dispatch", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	public override ZString JI_RN_NKCountryOfExport { get => base.JI_RN_NKCountryOfExport; set => base.JI_RN_NKCountryOfExport = value; }

	[ResourceStringData("DF3D67E2-84E4-47D2-9632-04C4F2EEBB98", Caption = "Region of Dispatch")]
	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.RegionOfDispatchList))]
	[MaxLength(Schema.JI_StateOrRegionOfOriginMaxLength)]
	public override ZString JI_StateOrRegionOfOrigin { get => base.JI_StateOrRegionOfOrigin; set => base.JI_StateOrRegionOfOrigin = value; }

	public new ICusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine> CusAuthorizationUsages => (CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>)base.CusAuthorizationUsages;

	protected override ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.JobComInvoiceLine> GetCusAuthorizationUsages() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>(this, Factory);

	public override ZString JI_Procedure
	{
		get => base.JI_Procedure;
		set
		{
			var hasChanged = JI_Procedure != value;
			base.JI_Procedure = value;
			if (hasChanged && !IsCopying)
			{
				if (EntryInstruction is CusEntryInstruction entryInstruction)
				{
					var style = entryInstruction.CEI_Style;
					if (!style.IsEmpty && !JI_Procedure.IsEmpty)
					{
						entryInstruction.DefaultFiscalReferenceForEntryInstructionStyle(this, style, FiscalReferenceCodeList.Codes.FR2_Customer);
					}
				}
			}
		}
	}

	[ResourceStringData("74C8C8AF-4740-449B-B575-4592EDFF01DD", Caption = "UCR Reference")]
	public override ZString ZG_UCRReference
	{
		get { return base.ZG_UCRReference; }
		set { base.ZG_UCRReference = value; }
	}

	[ResourceStringData("10D4D680-B1A4-4942-90D9-0EA077119E5A", Caption = "Procedure", FullDescription = "[UCC 1/10] Procedure", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	public override ZString JI_FormattedProcedure
	{
		get => base.JI_FormattedProcedure;
		set => base.JI_FormattedProcedure = value;
	}

	[ResourceStringData("1A6C7ED9-4DFF-4DB1-A882-150A85102350", Caption = "Add. Procedures", FullDescription = "[UCC 1/11] Add. Procedures", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	public override ZString AdditionalProcedureCodesAsString => base.AdditionalProcedureCodesAsString;

	[ResourceStringData("B043F00E-BB1A-4BD0-A26C-8649ABC605AC", Caption = "[UCC 4/14] Price")]
	public override ZDecimal JI_LinePrice
	{
		get => base.JI_LinePrice;
		set => base.JI_LinePrice = value;
	}

	[ResourceStringData("190FFFF3-19DA-4FCC-8D42-B16F0CC31764", Caption = "Preference", FullDescription = "[UCC 4/17] Preference", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public override ZString JI_PrimaryPreference
	{
		get => base.JI_PrimaryPreference;
		set => base.JI_PrimaryPreference = value;
	}

	[ResourceStringData("E5562A79-259B-4144-904E-AE870FE19B2A", Caption = "Country of Origin", FullDescription = "[UCC 5/15] Country of Origin", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	[ResourceStringData("7B92A495-80F9-4D4F-98BA-C02D16E81342", Caption = "Pref. Orig.", FullDescription = "[UCC 5/16] Country of Preferential Origin", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public override ZString JI_CountryOfOrigin
	{
		get => base.JI_CountryOfOrigin;
		set => base.JI_CountryOfOrigin = value;
	}

	[ResourceStringData("0311544E-7553-44A4-9760-A1CC37F050E6", Caption = "Country of Origin", FullDescription = "[UCC 5/15] Country of Origin", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public override ZString ZG_CountryOfSupply
	{
		get => base.ZG_CountryOfSupply;
		set => base.ZG_CountryOfSupply = value;
	}

	[ResourceStringData("A41A1BA7-C9A1-4D3F-AE2B-4EC70BA4B6A9", ShortCaption = "[UCC 5/14] Disp.", Caption = "[UCC 5/14] Dispatch", FullDescription = "[UCC 5/14] Country of Dispatch")]
	public override ZString ZG_CountryOfDispatch
	{
		get => base.ZG_CountryOfDispatch;
		set => base.ZG_CountryOfDispatch = value;
	}

	[ResourceStringData("34AA50BF-C200-4054-8A58-07DB1C0BDBA1", Caption = "Gross Weight", FullDescription = "[UCC 6/5] Gross Weight")]
	public override ZDecimal JI_Weight
	{
		get => base.JI_Weight;
		set => base.JI_Weight = value;
	}

	[ResourceStringData("5DB6F23D-3A2E-41DC-839C-B3B3BE1846E2", Caption = "Net Weight", FullDescription = "[UCC 6/1] Net Weight")]
	public override ZDecimal JI_NetWeight
	{
		get => base.JI_NetWeight;
		set => base.JI_NetWeight = value;
	}

	[ResourceStringData("6CCEAA7C-A6D8-4F3B-B78B-86FE41258AB4", Caption = "Goods Description", FullDescription = "[UCC 6/8] Goods Description")]
	public override ZString JI_Description
	{
		get => base.JI_Description;
		set => base.JI_Description = value;
	}

	[ResourceStringData("5228F9A5-A5E4-4517-B275-67C643A4BAFC", Caption = "CUS Code", FullDescription = "[UCC 6/13] CUS Code")]
	public override ZString ZG_CusNumber
	{
		get => base.ZG_CusNumber;
		set => base.ZG_CusNumber = value;
	}

	[ResourceStringData("7304ABE9-431A-4C56-8CD1-20A87947D2F9", Caption = "[UCC 6/14] Tariff")]
	public override ZString JI_Tariff
	{
		get => base.JI_Tariff;
		set => base.JI_Tariff = value;
	}

	[ResourceStringData("F2446904-FEB9-4527-8BC6-831E23482B41", Caption = "[UCC 8/1] Quota")]
	public override ZString JI_ConcessionOrder
	{
		get => base.JI_ConcessionOrder;
		set => base.JI_ConcessionOrder = value;
	}

	[ResourceStringData("D28EB6A8-235E-406A-87D3-CE35C6EEDE83", Caption = "Tran. Nature", FullDescription = "[UCC 8/5] Transaction Nature")]
	public override ZString ZG_TransNature
	{
		get => base.ZG_TransNature;
		set => base.ZG_TransNature = value;
	}

	[ResourceStringData("14B745DC-0DA0-45BF-A245-0182E6203F0C", Caption = "Customs Qty.", FullDescription = "Customs Quantity")]
	public override ZDecimal JI_CustomsQuantity
	{
		get => base.JI_CustomsQuantity;
		set => base.JI_CustomsQuantity = value;
	}

	[ResourceStringData("3F36AB11-285F-4946-ABA0-598ACAADF47A", Caption = "Supply Qty.", FullDescription = "Supply Quantity")]
	public override ZDecimal JI_CustomsSecondQuantity
	{
		get => base.JI_CustomsSecondQuantity;
		set => base.JI_CustomsSecondQuantity = value;
	}

	[ResourceStringData("D1C552CB-EBEE-4C7C-8B84-C46847E76B2D", Caption = "Third Qty.", FullDescription = "Third Quantity")]
	public override ZDecimal JI_CustomsThirdQuantity
	{
		get => base.JI_CustomsThirdQuantity;
		set => base.JI_CustomsThirdQuantity = value;
	}

	[ResourceStringData("E22DA252-12E1-45CF-BB86-E7225EE2A0BD", Caption = "Valuation Method", FullDescription = "[UCC 4/16] Valuation Method")]
	public override ZString JI_ValuationCode
	{
		get => base.JI_ValuationCode;
		set => base.JI_ValuationCode = value;
	}

	protected override BaseCusLinkPackageCollection PackagesForInvoiceLinesCore() => new CusLinkPackageCollection(this);
}
