using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceLine))]
sealed class JobComInvoiceLineTest : EU.Business.Declaration.Testing.JobComInvoiceLineTest<JobComInvoiceLine>
{
	public void TestICusCodeDataTypeSupporter_ECN()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		ICusCodeDataTypeSupporter supporter = invoiceLine;
		AssertEquals(typeof(ECCNCode), supporter.GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.ExportControlClassificationNumber]);
	}

	public void TestECCNCodes()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		AssertType<ECCNCodeCollection>(invoiceLine.ECCNCodes);
	}

	public void TestGetCusSupportingInfoTypes() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var supportingInfoTypeSupporter = (ICusSupportingInfoTypeSupporter)invoiceLine;

		AssertEquals("AdditionalInfo", typeof(AdditionalInfo), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		AssertEquals("SupportingDocument", typeof(SupportingDocument), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
		AssertEquals("PreviousDocument", typeof(PreviousDocument), supportingInfoTypeSupporter.GetCusSupportingInfoTypes()[Customs.Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
	});

	public void TestShouldCheckMissingPreviousDocuments()
	{
		CombineAssertions(() =>
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export Declaration and JI_Procedure doesn't start with 10", true, InvoiceLine.ShouldCheckMissingPreviousDocuments);

			InvoiceLine.JI_Procedure = "1000";
			AssertEquals("Export Declaration and JI_Procedure starts with 10", false, InvoiceLine.ShouldCheckMissingPreviousDocuments);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import Declaration and JI_Procedure starts with 10", true, InvoiceLine.ShouldCheckMissingPreviousDocuments);
		});
	}

	public void TestCustomsCountryCode()
	{
		AssertEquals("CustomsCountryCodeCore should be NL", Core.Constants.CountryCodes.Netherlands, InvoiceLine.CustomsCountryCode);
	}

	public void TestIInvoiceLinePartDetailsMembers()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
		{
			IInvoiceLinePartDetails partDetails = Factory.New<JobComInvoiceLine>();
			AssertEquals(Core.Constants.CountryCodes.Netherlands, partDetails.CustomsCountryCode);
			AssertEquals(typeof(MasterFiles.OrgSupplierPart), partDetails.TypeOfPartUsed);
		}
	}

	public void TestLookups_Import()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobComInvoiceLineLookups>(InvoiceLine.Lookups);
	}

	public void TestLookups_Export()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobComInvoiceLineLookups>(InvoiceLine.Lookups);
	}

	public void TestLookups_MiscellaneousCustoms()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobComInvoiceLineLookups>(InvoiceLine.Lookups);
	}

	public void TestValidation_Import()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<ImportJobComInvoiceLineValidation>(InvoiceLine.Validation);
	}

	public void TestValidation_Export()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertType<ExportJobComInvoiceLineValidation>(InvoiceLine.Validation);
	}

	public void TestValidation_MiscellaneousCustoms()
	{
		Declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
		AssertType<JobComInvoiceLineValidation>(InvoiceLine.Validation);
	}

	public void TestAddInfo()
	{
		AssertType<AddInfoJobComInvoiceLine>(InvoiceLine.AddInfo);
	}

	public void TestDefaultValuesFromSupplierBuyerLink()
	{
		CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobDeclaration.JE_OH_Importer = ZGuid.Empty;
			invoiceHeader.JZ_OH_Supplier = ZGuid.Empty;
			var invoiceLine = declaration.InvoiceLines.AddNew();

			AssertEquals(ZString.Empty, invoiceLine.JI_ValuationCode);

			var importer = OrgHeader.New(Factory);
			var supplier = OrgHeader.New(Factory);
			var link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = importer.PK;
			link.OL_OH_Supplier = supplier.PK;
			link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Netherlands;
			link.OL_ValuationBasis = "5";

			invoiceHeader.JobDeclaration.JE_OH_Importer = importer.PK;
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();

			AssertEquals("5", invoiceLine.JI_ValuationCode);
			AssertEquals("5", invoiceLine2.JI_ValuationCode);

			var supplier2 = OrgHeader.New(Factory);
			link = Factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = importer.PK;
			link.OL_OH_Supplier = supplier2.PK;
			link.OL_RN_NKImporterCountry = Enterprise.Core.Constants.CountryCodes.Netherlands;
			link.OL_ValuationBasis = "3";

			invoiceHeader.JZ_OH_Supplier = supplier2.PK;

			AssertEquals("5", invoiceLine2.JI_ValuationCode);
		});
	}

	public void TestMaxNumberOfAdditionalProcedureCode()
	{
		AssertEquals("MaxNumberOfAdditionalProcedureCode", 99, InvoiceLine.MaxNumberOfAdditionalProcedureCode);
	}

	public void TestAdditionalInfo()
	{
		AssertType<AdditionalInfoCollection>(InvoiceLine.AdditionalInfos);
	}

	public void TestJI_OA_ExporterAddress_Caption()
	{
		AssertEquals("Consignor", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_OA_ExporterAddressInfo).Caption);
	}

	public void TestJI_OA_ConsigneeAddress_Caption()
	{
		AssertEquals("Consignee", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_OA_ConsigneeAddressInfo).Caption);
	}

	public void TestJI_FormattedProcedure_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_FormattedProcedureInfo, "Procedure", "[UCC 1/10] Procedure");
		AssertCaptionAndFullDescription(InvoiceLine.JI_FormattedProcedureInfo, "Procedure", "[UCC 1/10] Procedure", "EXP");
	}

	public void TestAdditionalProcedureCodesAsString_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.AdditionalProcedureCodesAsStringInfo, "Add. Procedures", "[UCC 1/11] Add. Procedures");
		AssertCaptionAndFullDescription(InvoiceLine.AdditionalProcedureCodesAsStringInfo, "Add. Procedures", "[UCC 1/11] Add. Procedures", "EXP");
	}

	public void TestJI_LinePrice_Caption()
	{
		AssertEquals("[UCC 4/14] Price", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_LinePriceInfo).Caption);
	}

	public void TestJI_ValuationCode_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_ValuationCodeInfo, "Valuation Method", "[UCC 4/16] Valuation Method");
	}

	public void TestJI_PrimaryPreference_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_PrimaryPreferenceInfo, "Preference", "[UCC 4/17] Preference", "IMP");
	}

	public void TestJI_CountryOfOrigin_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_CountryOfOriginInfo, "Country of Origin", "[UCC 5/15] Country of Origin", "EXP");
		AssertCaptionAndFullDescription(InvoiceLine.JI_CountryOfOriginInfo, "Pref. Orig.", "[UCC 5/16] Country of Preferential Origin", "IMP");
	}

	public void TestZG_CountryOfSupply_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.ZG_CountryOfSupplyInfo, "Country of Origin", "[UCC 5/15] Country of Origin", "IMP");
	}

	public void TestJI_RN_NKCountryOfExport_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_RN_NKCountryOfExportInfo, "[UCC 5/14] Dispatch", "[UCC 5/14] Country of Dispatch", "EXP", expectedShortCaption: "[UCC 5/14] Disp.");
	}

	public void TestZG_CountryOfDispatch_Caption()
	{
		AssertEquals("[UCC 5/14] Dispatch", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.ZG_CountryOfDispatchInfo).Caption);
	}

	public void TestZG_CountryOfDestination_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.ZG_CountryOfDestinationInfo, "[UCC 5/8] Dest.", "[UCC 5/8] Country of Destination", "IMP");
		AssertCaptionAndFullDescription(InvoiceLine.ZG_CountryOfDestinationInfo, "[UCC 5/8] Dest.", "[UCC 5/8] Country of Destination", "EXP");
	}

	public void TestJI_Weight_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_WeightInfo, "Gross Weight", "[UCC 6/5] Gross Weight");
	}

	public void TestJI_NetWeight_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_NetWeightInfo, "Net Weight", "[UCC 6/1] Net Weight");
	}

	public void TestJI_Description_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_DescriptionInfo, "Goods Description", "[UCC 6/8] Goods Description");
	}

	public new void TestZG_CusNumber_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.ZG_CusNumberInfo, "CUS Code", "[UCC 6/13] CUS Code");
	}

	public void TestJI_Tariff_Caption()
	{
		AssertEquals("[UCC 6/14] Tariff", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_TariffInfo).Caption);
	}

	public void TestJI_ConcessionOrder_Caption()
	{
		AssertEquals("[UCC 8/1] Quota", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.JI_ConcessionOrderInfo).Caption);
	}

	public void TestZG_TransNature_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.ZG_TransNatureInfo, "Tran. Nature", "[UCC 8/5] Transaction Nature");
	}

	public void TestJI_CustomsQuantity_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_CustomsQuantityInfo, "Customs Qty.", "Customs Quantity");
	}

	public void TestJI_CustomsSecondQuantity_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_CustomsSecondQuantityInfo, "Supply Qty.", "Supply Quantity");
	}

	public void TestJI_CustomsThirdQuantity_Caption()
	{
		AssertCaptionAndFullDescription(InvoiceLine.JI_CustomsThirdQuantityInfo, "Third Qty.", "Third Quantity");
	}

	public void TestECCNCodesAsString_Caption()
	{
		AssertEquals("ECCN", DataBoundResourceStrings.GetDataForProperty(InvoiceLine.ECCNCodesAsStringInfo).Caption);
	}

	public void TestSupportingDocumentCollectionType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.InvoiceLines.AddNew();
		AssertType<SupportingDocumentCollection>(invoiceLine.SupportingDocuments);
	}

	public void TestPreviousDocumentCollectionType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.InvoiceLines.AddNew();
		AssertType<PreviousDocumentCollection>(invoiceLine.PreviousDocuments);
	}

	public void TestCusAuthorizationUsages()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		AssertType<EU.Business.CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>>(invoiceLine.CusAuthorizationUsages);
	}
	protected override Type GetExpectedCusEntryLineType()
	{
		return typeof(CusEntryLine);
	}

	protected override Type GetExpectedEntryInstructionType()
	{
		return typeof(CusEntryInstruction);
	}

	public void TestHasAtLeastOneRelatedIndicatorChecked()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		AssertEquals("By Default an invoice line should not have any related indicators checked ", false, invoiceLine.HasAtLeastOneRelatedIndicatorChecked);

		invoiceLine.RelatedIndicator = true;
		invoiceLine.RelatedIndicator2 = false;
		invoiceLine.RelatedIndicator3 = false;
		invoiceLine.RelatedIndicator4 = false;

		AssertEquals("Related indicator 1 is checked ", true, invoiceLine.HasAtLeastOneRelatedIndicatorChecked);

		invoiceLine.RelatedIndicator = false;
		invoiceLine.RelatedIndicator2 = true;

		AssertEquals("Related indicator 2 is checked ", true, invoiceLine.HasAtLeastOneRelatedIndicatorChecked);

		invoiceLine.RelatedIndicator2 = false;
		invoiceLine.RelatedIndicator3 = true;

		AssertEquals("Related indicator 3 is checked ", true, invoiceLine.HasAtLeastOneRelatedIndicatorChecked);

		invoiceLine.RelatedIndicator3 = false;
		invoiceLine.RelatedIndicator4 = true;

		AssertEquals("Related indicator 4 is checked ", true, invoiceLine.HasAtLeastOneRelatedIndicatorChecked);
	}

	public void TestAdditionalProcedureCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, invoiceLine.AdditionalProcedureCode);

			invoiceLine.JI_Procedure = "XXXXF49";
			AssertEquals("F49", invoiceLine.AdditionalProcedureCode);

			invoiceLine.JI_Procedure = "XXXX";
			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedureCode.CY_Code = "XXXXF50";
			AssertEquals("F50", invoiceLine.AdditionalProcedureCode);
		});
	}

	public void TestHasProcedureStartingWithAny()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_Procedure = "000000";

		ZString[] checkCodes = new ZString[] { NLConstants.ProcedureCodes._51, NLConstants.ProcedureCodes._53, NLConstants.ProcedureCodes._71 };

		AssertEquals("There should not be a procedure starting with 51, 53 or 71.", false, invoiceLine.HasProcedureStartingWithAny(checkCodes));

		invoiceLine.JI_Procedure = "510000";

		AssertEquals("There should be a procedure starting with 51.", true, invoiceLine.HasProcedureStartingWithAny(checkCodes));
	}

	public void TestPackagesForInvoiceLines()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		AssertType<CusLinkPackageCollection>(invoiceLine.PackagesForInvoiceLinesForBindingOnly);
	}

	protected override Type GetExpectedPartType() => typeof(MasterFiles.OrgSupplierPart);

	new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		SetupDataEligibleForMerging(declaration);
		base.DoMerge(declaration);
	}

	void AssertCaptionAndFullDescription(ZPropertyInfo propertyInfo, string expectedCaption, string expectedFullDescription, string messageType = "", string expectedShortCaption = "")
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(propertyInfo, null, [messageType == "IMP" ? JobDeclaration.CaptionKeyImportUCC6 : messageType == "EXP" ? JobDeclaration.CaptionKeyExportUCC6 : null]);

		CombineAssertions(() =>
		{
			if (!string.IsNullOrEmpty(expectedShortCaption))
			{
				AssertEquals($"{propertyInfo.Name} {messageType} ShortCaption", expectedShortCaption, resourceStringData.ShortCaption);
			}
			AssertEquals($"{propertyInfo.Name} {messageType} Caption", expectedCaption, resourceStringData.Caption);
			AssertEquals($"{propertyInfo.Name} {messageType} FullDescription", expectedFullDescription, resourceStringData.FullDescription);
		});
	}

	void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
	}
}
