using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(PreviousDocumentLookups))]
sealed class PermitLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestPermitAuthorityCodeListExport() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertCodeList(lookups.PermitAuthorityCodeList, RefCusCodeTestHelper.ValidPermitAuthorityCode, RefCusCodeTestHelper.InvalidPermitAuthorityCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertCodeList(lookups.PermitAuthorityCodeList, RefCusCodeTestHelper.ValidPermitAuthorityCode, RefCusCodeTestHelper.InvalidPermitAuthorityCode);
	});

	public void TestPermitAuthorityCodeListImport() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertCodeList(lookups.PermitAuthorityCodeList, RefCusCodeTestHelper.ValidPermitAuthorityCode, RefCusCodeTestHelper.InvalidPermitAuthorityCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertCodeList(lookups.PermitAuthorityCodeList, RefCusCodeTestHelper.ValidPermitAuthorityCodePast, RefCusCodeTestHelper.ValidPermitAuthorityCode);
	});

	public void TestPermitTypeCodeListExport() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertCodeList(lookups.PermitTypeCodeList, RefCusCodeTestHelper.ValidPermitTypeCode, RefCusCodeTestHelper.InvalidPermitTypeCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertCodeList(lookups.PermitTypeCodeList, RefCusCodeTestHelper.ValidPermitTypeCode, RefCusCodeTestHelper.InvalidPermitTypeCode);
	});

	public void TestPermitTypeCodeListImport() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertCodeList(lookups.PermitTypeCodeList, RefCusCodeTestHelper.ValidPermitTypeCode, RefCusCodeTestHelper.InvalidPermitTypeCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertCodeList(lookups.PermitTypeCodeList, RefCusCodeTestHelper.ValidPermitTypeCodePast, RefCusCodeTestHelper.ValidPermitTypeCode);
	});

	void AssertCodeList(ZZRefCusCodeListCombinedCollection list, string validCode, string invalidCode)
	{
		list.Load();

		AssertEquals("Valid code", true, list.OfType<ZZRefCusCodeListCombined>().Any(c => c.ZZD_Code == validCode));
		AssertEquals("Invalid code", false, list.OfType<ZZRefCusCodeListCombined>().Any(c => c.ZZD_Code == invalidCode));
	}

	protected override void SetUp()
	{
		base.SetUp();

		RefCusCodeTestHelper.CreatePermitAuthorityCodeList(Factory);
		RefCusCodeTestHelper.CreatePermitTypeCodeList(Factory);

		declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		permit = invoiceLine.Permits.AddNew();
		lookups = permit.Lookups;
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	CusEntryInstruction entryInstruction;
	Permit permit;
	PermitLookups lookups;
}
