using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business.Declaration;

public class JobComInvoiceLine : AutoJobComInvoiceLine
	, Integration.Customs.NL.IJobComInvoiceLine
{
	public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new JobComInvoiceHeader InvoiceHeader => (JobComInvoiceHeader)base.InvoiceHeader;

	public new JobComInvoiceLineLookups Lookups => (JobComInvoiceLineLookups)base.Lookups;

	public new JobComInvoiceLineValidation Validation => (JobComInvoiceLineValidation)base.Validation;

	protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
	{
		JobComInvoiceLineLookups result;
		if (IsImport)
		{
			result = new ImportJobComInvoiceLineLookups(this);
		}
		else if (IsExport)
		{
			result = new ExportJobComInvoiceLineLookups(this);
		}
		else
		{
			result = new JobComInvoiceLineLookups(this);
		}
		return result;
	}

	protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
	{
		JobComInvoiceLineValidation result;
		if (IsImport)
		{
			result = new ImportJobComInvoiceLineValidation(this);
		}
		else if (IsExport)
		{
			result = new ExportJobComInvoiceLineValidation(this);
		}
		else
		{
			result = new JobComInvoiceLineValidation(this);
		}
		return result;
	}

	public void PopulateFromSupplierLink()
	{
		if (InvoiceHeader != null)
		{
			if (JI_ValuationCode.IsEmpty && InvoiceHeader.SupplierBuyerLink != null)
			{
				JI_ValuationCode = InvoiceHeader.SupplierBuyerLink.OL_ValuationBasis.Trim();
			}
		}
	}

	[ResourceStringData("752A53F7-BE87-447C-B40E-E4419EF9E15E", Caption = "Consignor")]
	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ConsignorList))]
	public override ZGuid JI_OA_ExporterAddress
	{
		get => base.JI_OA_ExporterAddress;
		set => base.JI_OA_ExporterAddress = value;
	}

	[ResourceStringData("9366F8F0-7E59-45DB-B7E9-B21B7F083EE5", Caption = "Procedure", FullDescription = "[UCC 1/10] Procedure")]
	[ResourceStringData("51E0CBA1-591E-418A-B502-53C69DC163A3", Caption = "Procedure", FullDescription = "[UCC 1/10] Procedure", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	public override ZString JI_FormattedProcedure
	{
		get => base.JI_FormattedProcedure;
		set => base.JI_FormattedProcedure = value;
	}

	[ResourceStringData("605DDE20-8798-405B-802F-83DFE93EE445", Caption = "Add. Procedures", FullDescription = "[UCC 1/11] Add. Procedures")]
	[ResourceStringData("E53D4591-077F-4A59-B997-C1FD123F4094", Caption = "Add. Procedures", FullDescription = "[UCC 1/11] Add. Procedures", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	public override ZString AdditionalProcedureCodesAsString => base.AdditionalProcedureCodesAsString;

	[ResourceStringData("B26CECD9-FAE6-4E5C-8E88-EEF4F00580D2", Caption = "[UCC 4/14] Price")]
	public override ZDecimal JI_LinePrice
	{
		get => base.JI_LinePrice;
		set => base.JI_LinePrice = value;
	}

	[ResourceStringData("D202C3F4-9F2E-4F38-B5CA-1A1BCA8F15D6", Caption = "Preference", FullDescription = "[UCC 4/17] Preference", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public override ZString JI_PrimaryPreference
	{
		get => base.JI_PrimaryPreference;
		set => base.JI_PrimaryPreference = value;
	}

	[ResourceStringData("F74CECDB-DECE-4633-ADF5-B7130F92CA1C", Caption = "Country of Origin", FullDescription = "[UCC 5/15] Country of Origin", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	[ResourceStringData("1E8B164C-F432-48E4-905F-E8537D3079ED", Caption = "Pref. Orig.", FullDescription = "[UCC 5/16] Country of Preferential Origin", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public override ZString JI_CountryOfOrigin
	{
		get => base.JI_CountryOfOrigin;
		set => base.JI_CountryOfOrigin = value;
	}

	[ResourceStringData("1DD4F27D-F577-47AD-8836-8192D9B54851", Caption = "Country of Origin", FullDescription = "[UCC 5/15] Country of Origin", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public override ZString ZG_CountryOfSupply
	{
		get => base.ZG_CountryOfSupply;
		set => base.ZG_CountryOfSupply = value;
	}

	[ResourceStringData("A51FB709-6BF3-48B6-B590-AAD4227C506A", ShortCaption = "[UCC 5/14] Disp.", Caption = "[UCC 5/14] Dispatch", FullDescription = "[UCC 5/14] Country of Dispatch")]
	public override ZString ZG_CountryOfDispatch
	{
		get => base.ZG_CountryOfDispatch;
		set => base.ZG_CountryOfDispatch = value;
	}

	[ResourceStringData("7F1CEF65-3AED-4080-A066-084500BBC309", Caption = "[UCC 5/8] Dest.", FullDescription = "[UCC 5/8] Country of Destination")]
	[ResourceStringData("C30C536E-053E-4420-B758-C160509174F9", Caption = "[UCC 5/8] Dest.", FullDescription = "[UCC 5/8] Country of Destination", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	public override ZString ZG_CountryOfDestination
	{
		get => base.ZG_CountryOfDestination;
		set => base.ZG_CountryOfDestination = value;
	}

	[ResourceStringData("93A0BC76-6983-43EF-97BF-FC4472178A83", ShortCaption = "[UCC 5/14] Disp.", Caption = "[UCC 5/14] Dispatch", FullDescription = "[UCC 5/14] Country of Dispatch", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	public override ZString JI_RN_NKCountryOfExport { get => base.JI_RN_NKCountryOfExport; set => base.JI_RN_NKCountryOfExport = value; }

	[ResourceStringData("32178D1B-1062-43A4-8AF6-65BF680FABD8", Caption = "Gross Weight", FullDescription = "[UCC 6/5] Gross Weight")]
	public override ZDecimal JI_Weight
	{
		get => base.JI_Weight;
		set => base.JI_Weight = value;
	}

	[ResourceStringData("4D908224-165B-4639-80E8-DC0B728C0D15", Caption = "Net Weight", FullDescription = "[UCC 6/1] Net Weight")]
	public override ZDecimal JI_NetWeight
	{
		get => base.JI_NetWeight;
		set => base.JI_NetWeight = value;
	}

	[ResourceStringData("4FFC4C56-8D5B-42E6-A9C6-4C52F80221D0", Caption = "Goods Description", FullDescription = "[UCC 6/8] Goods Description")]
	public override ZString JI_Description
	{
		get => base.JI_Description;
		set => base.JI_Description = value;
	}

	[ResourceStringData("0C32674F-FA69-4C32-85C7-D09581C1B714", Caption = "CUS Code", FullDescription = "[UCC 6/13] CUS Code")]
	public override ZString ZG_CusNumber
	{
		get => base.ZG_CusNumber;
		set => base.ZG_CusNumber = value;
	}

	[ResourceStringData("D1358740-9175-4385-90E3-D9C4063BD7AB", Caption = "[UCC 6/14] Tariff")]
	public override ZString JI_Tariff
	{
		get => base.JI_Tariff;
		set => base.JI_Tariff = value;
	}

	[ResourceStringData("8264CEE9-9E73-4D6D-A09A-DDA2647808D8", Caption = "[UCC 8/1] Quota")]
	public override ZString JI_ConcessionOrder
	{
		get => base.JI_ConcessionOrder;
		set => base.JI_ConcessionOrder = value;
	}

	[ResourceStringData("DE4DE14F-6C1D-43AB-A061-B4E98F8E427D", Caption = "Tran. Nature", FullDescription = "[UCC 8/5] Transaction Nature")]
	public override ZString ZG_TransNature
	{
		get => base.ZG_TransNature;
		set => base.ZG_TransNature = value;
	}

	[ResourceStringData("08985A4A-F7FD-4CBA-9084-54D559D81AFA", Caption = "Customs Qty.", FullDescription = "Customs Quantity")]
	public override ZDecimal JI_CustomsQuantity
	{
		get => base.JI_CustomsQuantity;
		set => base.JI_CustomsQuantity = value;
	}

	[ResourceStringData("A1415F96-8A0E-4F03-879A-476343451075", Caption = "Supply Qty.", FullDescription = "Supply Quantity")]
	public override ZDecimal JI_CustomsSecondQuantity
	{
		get => base.JI_CustomsSecondQuantity;
		set => base.JI_CustomsSecondQuantity = value;
	}

	[ResourceStringData("A8F796A0-8DC7-4D09-A5AD-037FF29DB3B3", Caption = "Third Qty.", FullDescription = "Third Quantity")]
	public override ZDecimal JI_CustomsThirdQuantity
	{
		get => base.JI_CustomsThirdQuantity;
		set => base.JI_CustomsThirdQuantity = value;
	}

	public override ZInt MaxNumberOfAdditionalProcedureCode => 99;

	public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

	protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

	protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = base.GetCusSupportingInfoTypes();
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
		result[Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
		return result;
	}

	public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

	protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

	protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.Netherlands;
	protected override Type TypeOfPartUsedCore => typeof(MasterFiles.OrgSupplierPart);

	public new EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine> CusAuthorizationUsages => (EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>)base.CusAuthorizationUsages;

	protected override EU.Business.ICusAuthorizationUsageCollection<EU.Business.CusAuthorizationUsage, EU.Business.Declaration.JobComInvoiceLine> GetCusAuthorizationUsages() => new EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>(this, Factory);

	[ResourceStringData("018DD1EF-3AA0-4820-A311-A1AAE79A732A", Caption = "Valuation Method", FullDescription = "[UCC 4/16] Valuation Method")]
	public override ZString JI_ValuationCode
	{
		get => base.JI_ValuationCode;
		set
		{
			var oldValue = JI_ValuationCode;
			base.JI_ValuationCode = value;
			if (!IsCopying && oldValue != JI_ValuationCode)
			{
				Declaration.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString JI_Procedure
	{
		get => base.JI_Procedure;
		set
		{
			base.JI_Procedure = value;
			InvoiceHeader?.MarkAsNeedingValidation();
		}
	}

	public override ZGuid JI_JZ
	{
		get => base.JI_JZ;
		set
		{
			base.JI_JZ = value;
			InvoiceHeader?.MarkAsNeedingValidation();
		}
	}

	public ZString AdditionalProcedureCode
	{
		get
		{
			var additionalProcedure = ZString.Empty;
			if (JI_Procedure.Length >= 7)
			{
				additionalProcedure = JI_Procedure.SubstringSafe(4, 3);
			}
			else if (AdditionalProcedureCodes.Count > 0)
			{
				additionalProcedure = (ZString)(AdditionalProcedureCodes.FirstOrDefault()?.CY_Code.SubstringSafe(4, 3));
			}

			return additionalProcedure;
		}
	}

	public bool HasAtLeastOneRelatedIndicatorChecked => RelatedIndicator || RelatedIndicator2 || RelatedIndicator3 || RelatedIndicator4;

	public new CusEntryInstruction EntryInstruction => (CusEntryInstruction)base.EntryInstruction;

	public bool HasProcedureStartingWithAny(params ZString[] procedureCodes) => procedureCodes.Any(procedureCode => JI_Procedure.StartsWith(procedureCode));

	protected override ZBool ShouldCheckMissingPreviousDocumentsCore => !(IsExport && JI_Procedure.StartsWith(NLConstants.ProcedureCodes._10)) && base.ShouldCheckMissingPreviousDocumentsCore;

	[ResourceStringData("Enterprise.Customs.NL.Business.Declaration.JobComInvoiceLine|ECCNCodesAsString", Caption = "ECCN")]
	public ZString ECCNCodesAsString => ECCNCodes.AsString;

	[ChildEditable(true)]
	public ECCNCodeCollection ECCNCodes
	{
		get
		{
			if (eccnCodes == null)
			{
				eccnCodes = GetECCNCodeCollection();
				RegisterEditableChildObject(eccnCodes);
				eccnCodes.Load();
			}
			return eccnCodes;
		}
	}
	ECCNCodeCollection eccnCodes;

	protected ECCNCodeCollection GetECCNCodeCollection() => new ECCNCodeCollection(this);

	public ZPropertyInfo ECCNCodesAsStringInfo => GetZPropertyInfo(nameof(ECCNCodesAsString));

	protected override Dictionary<ZString, Type> GetCusCodeDataTypes()
	{
		var result = base.GetCusCodeDataTypes();
		result.Add(CusCodeDataTypeList.Codes.ExportControlClassificationNumber, typeof(ECCNCode));
		return result;
	}

	protected override BaseCusLinkPackageCollection PackagesForInvoiceLinesCore() => new CusLinkPackageCollection(this);
}
