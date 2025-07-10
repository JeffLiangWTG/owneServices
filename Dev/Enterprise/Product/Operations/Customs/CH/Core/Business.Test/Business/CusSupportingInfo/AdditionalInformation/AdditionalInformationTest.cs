using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(AdditionalInformation))]
sealed class AdditionalInformationTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInformation>
{
	public void TestCSI_LineNoAssignedSequential() => CombineAssertions(() =>
	{
		var invoiceLine = AdditionalInformation.Parent as JobComInvoiceLine;
		AssertEquals("1 added -1st", 1, AdditionalInformation.CSI_LineNo);

		var additionalInformation2 = invoiceLine.AdditionalInformations.AddNew();
		AssertEquals("2 added - 1st", 1, AdditionalInformation.CSI_LineNo);
		AssertEquals("2 added - 2nd", 2, additionalInformation2.CSI_LineNo);

		var additionalInformation3 = invoiceLine.AdditionalInformations.AddNew();
		AssertEquals("3 added- 1st", 1, AdditionalInformation.CSI_LineNo);
		AssertEquals("3 added - 2nd", 2, additionalInformation2.CSI_LineNo);
		AssertEquals("3 added - 3rd", 3, additionalInformation3.CSI_LineNo);

		additionalInformation2.Delete();
		AssertEquals("2 deleted - 1st", 1, AdditionalInformation.CSI_LineNo);
		AssertEquals("2 deleted - 3rd", 2, additionalInformation3.CSI_LineNo);

		var additionalInformation4 = invoiceLine.AdditionalInformations.AddNew();
		AssertEquals("4 added - 1st", 1, AdditionalInformation.CSI_LineNo);
		AssertEquals("4 added - 3rd", 2, additionalInformation3.CSI_LineNo);
		AssertEquals("4 added - 4th", 3, additionalInformation4.CSI_LineNo);
	});

	public void TestCaptions() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateAdditionalInformationCodes(Factory);
		AssertEquals("Caption CSI_Code", "Code", AdditionalInformation.CSI_CodeInfo.Description);
		AssertEquals("Caption CSI_Description", "Text", AdditionalInformation.CSI_DescriptionInfo.Description);
		AssertEquals("Caption", "Value", AdditionalInformation.CSI_ReferenceNumberInfo.Description);
	});

	public void TestMaxLength_Export() => CombineAssertions(() =>
	{
		AdditionalInformation.Parent.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		AssertEquals("MaxLength CSI_Code Export", 5, AdditionalInformation.CSI_CodeInfo.MaxLength);
	});

	public void TestMaxLength() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateAdditionalInformationCodes(Factory);
		AssertEquals("MaxLength CSI_Code Import", 3, AdditionalInformation.CSI_CodeInfo.MaxLength);
		AssertEquals("MaxLength CSI_Description", 512, AdditionalInformation.CSI_DescriptionInfo.MaxLength);
		AssertEquals("MaxLength", 50, AdditionalInformation.CSI_ReferenceNumberInfo.MaxLength);
	});

	public void TestIsSamnaunFreeZoneTraffic() => CombineAssertions(() =>
	{
		AssertIsSamnaunFreeZoneTraffic(UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic, UniversalReferenceConstants.FreeZoneTradeCode.Hochsavoyen, false);
		AssertIsSamnaunFreeZoneTraffic(UniversalReferenceConstants.AdditionalInformationTypeCodes.BorderZoneTraffic, UniversalReferenceConstants.FreeZoneTradeCode.Samnaun, false);
		AssertIsSamnaunFreeZoneTraffic(UniversalReferenceConstants.AdditionalInformationTypeCodes.FreeZoneTraffic, UniversalReferenceConstants.FreeZoneTradeCode.Samnaun, true);
	});

	public void TestAdditionalInformationHumanReadableName() => AssertEquals("Additional Information", AdditionalInformation.HumanReadableName);

	public void TestLookups() => AssertType<AdditionalInformationLookups>(AdditionalInformation.Lookups);

	public void TestValidator() => AssertType<AdditionalInformationValidation>(AdditionalInformation.Validation);

	public void TestCSI_DescriptionFieldType() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var additionalInformation = invoiceLine.AdditionalInformations.AddNew();

		invoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoTaxRefund;
		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup;
		AssertEquals("if JI_RefundType = 6 and CSI_Code = A1402 field type should be", nameof(FieldType.TextDropEdit), additionalInformation.CSI_DescriptionFieldType);

		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductSubgroup;
		AssertEquals("if JI_RefundType = 6 and CSI_Code = A1403 field type should be", nameof(FieldType.TextDropEdit), additionalInformation.CSI_DescriptionFieldType);

		invoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoProductsExTaxWarehouse;
		AssertEquals("if HI_RefundType = 7 and CSI_Code = A1403 field type should be", nameof(FieldType.TextDropEdit), additionalInformation.CSI_DescriptionFieldType);

		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup;
		AssertEquals("if HI_RefundType = 7 and CSI_Code = A1402 field type should be", nameof(FieldType.TextDropEdit), additionalInformation.CSI_DescriptionFieldType);

		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ExportCodeMineralOil;
		AssertEquals("if JI_RefundType = 6 and CSI_Code = 27 field type should be", nameof(FieldType.TextMultiLine), additionalInformation.CSI_DescriptionFieldType);

		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup;
		invoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.Refund;
		AssertEquals("if HI_RefundType = 1 and CSI_Code = A1402 field type should be", nameof(FieldType.TextMultiLine), additionalInformation.CSI_DescriptionFieldType);
	});

	public void TestClearA1403DescriptionOnChangeOfA1402() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoTaxRefund;
		var additionalInformationA1402 = invoiceLine.AdditionalInformations.AddNew();
		var additionalInformationA1403 = invoiceLine.AdditionalInformations.AddNew();
		additionalInformationA1402.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup;
		additionalInformationA1403.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductSubgroup;

		additionalInformationA1403.CSI_Description = "Test";
		additionalInformationA1402.CSI_Description = "1";
		AssertEquals("if description change in A1402 Additional information the A1403 should be deleted", ZString.Empty, additionalInformationA1403.CSI_Description);

		additionalInformationA1403.CSI_Description = "Test";
		additionalInformationA1402.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.PartialShipmentNumber;
		AssertEquals("if description change in A1402 Additional information the A1403 should be deleted", ZString.Empty, additionalInformationA1403.CSI_Description);

		additionalInformationA1402.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup;
		invoiceLine.JI_RefundType = UniversalReferenceConstants.RefundType.TobaccoTaxRefund;
		additionalInformationA1403.CSI_Description = "Test";
		AssertEquals("if 1402 change before the A1403 description is written that should be not deleted", "Test", additionalInformationA1403.CSI_Description);
	});

	public void TestIsProductMainGroupOrSubgroup() => CombineAssertions(() =>
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var additionalInformation = invoiceLine.AdditionalInformations.AddNew();

		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductMainGroup;
		AssertEquals("if CSI_Code = A1402, true", true, additionalInformation.IsProductMainGroupOrSubgroup);

		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ProductSubgroup;
		AssertEquals("if CSI_Code = A1403, true", true, additionalInformation.IsProductMainGroupOrSubgroup);

		additionalInformation.CSI_Code = UniversalReferenceConstants.AdditionalInformationTypeCodes.ExportCodeMineralOil;
		AssertEquals("if CSI_Code = 27, false", false, additionalInformation.IsProductMainGroupOrSubgroup);
	});

	protected override BusinessObject GetNewBusinessObject() => GetNewAdditionalInformation();

	protected override IEnumerable<AdditionalInformation> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var additionalInformation = invoiceLine.AdditionalInformations.AddNew();
		yield return additionalInformation;
	}

	void AssertIsSamnaunFreeZoneTraffic(ZString code, ZString number, bool expectedIsSamnaunFreeZoneTraffic)
	{
		AdditionalInformation.CSI_Code = code;
		AdditionalInformation.CSI_ReferenceNumber = number;

		AssertEquals($"CSI_Code={code} CSI_ReferenceNumber={number}", expectedIsSamnaunFreeZoneTraffic, AdditionalInformation.IsSamnaunFreeZoneTraffic);
	}

	AdditionalInformation GetNewAdditionalInformation()
	{
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		return invoiceLine.AdditionalInformations.AddNew();
	}

	AdditionalInformation AdditionalInformation => additionalInformation ??= GetNewAdditionalInformation();
	AdditionalInformation additionalInformation;

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;
}
