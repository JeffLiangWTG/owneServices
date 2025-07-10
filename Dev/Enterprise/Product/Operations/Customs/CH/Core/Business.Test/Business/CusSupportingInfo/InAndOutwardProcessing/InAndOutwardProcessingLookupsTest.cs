using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(InAndOutwardProcessingLookups))]
sealed class InAndOutwardProcessingLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestDirectionListExport() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInAndOutwardDirectionList(Factory);

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertLookupPairList(lookups.DirectionList, 1, RefCusCodeTestHelper.ValidSimpleCode, RefCusCodeTestHelper.InvalidSimpleCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertLookupPairList(lookups.DirectionList, 1, RefCusCodeTestHelper.ValidSimpleCode, RefCusCodeTestHelper.InvalidSimpleCode);
	});

	public void TestDirectionListImport() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInAndOutwardDirectionList(Factory);

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertLookupPairList(lookups.DirectionList, 1, RefCusCodeTestHelper.ValidSimpleCode, RefCusCodeTestHelper.InvalidSimpleCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertLookupPairList(lookups.DirectionList, 1, RefCusCodeTestHelper.ValidSimpleCodePast, RefCusCodeTestHelper.ValidSimpleCode);
	});

	public void TestRefinementTypeListExport() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInAndOutwardRefinementTypeList(Factory);

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertLookupPairList(lookups.RefinementTypeList, 1, RefCusCodeTestHelper.ValidSimpleCode, RefCusCodeTestHelper.InvalidSimpleCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertLookupPairList(lookups.RefinementTypeList, 1, RefCusCodeTestHelper.ValidSimpleCode, RefCusCodeTestHelper.InvalidSimpleCode);
	});

	public void TestRefinementTypeListImport() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInAndOutwardRefinementTypeList(Factory);

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertLookupPairList(lookups.RefinementTypeList, 1, RefCusCodeTestHelper.ValidSimpleCode, RefCusCodeTestHelper.InvalidSimpleCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertLookupPairList(lookups.RefinementTypeList, 1, RefCusCodeTestHelper.ValidSimpleCodePast, RefCusCodeTestHelper.ValidSimpleCode);
	});

	public void TestProcessTypeListExport() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInAndOutwardProcessTypeList(Factory);

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertLookupPairList(lookups.ProcessTypeList, 1, RefCusCodeTestHelper.ValidSimpleCode, RefCusCodeTestHelper.InvalidSimpleCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertLookupPairList(lookups.ProcessTypeList, 1, RefCusCodeTestHelper.ValidSimpleCode, RefCusCodeTestHelper.InvalidSimpleCode);
	});

	public void TestProcessTypeListImport() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInAndOutwardProcessTypeList(Factory);

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertLookupPairList(lookups.ProcessTypeList, 1, RefCusCodeTestHelper.ValidSimpleCode, RefCusCodeTestHelper.InvalidSimpleCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertLookupPairList(lookups.ProcessTypeList, 1, RefCusCodeTestHelper.ValidSimpleCodePast, RefCusCodeTestHelper.ValidSimpleCode);
	});

	public void TestBillingTypeListExport() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInAndOutwardBillingTypeList(Factory);

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertLookupPairList(lookups.BillingTypeList, 1, RefCusCodeTestHelper.ValidSimpleCode, RefCusCodeTestHelper.InvalidSimpleCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertLookupPairList(lookups.BillingTypeList, 1, RefCusCodeTestHelper.ValidSimpleCode, RefCusCodeTestHelper.InvalidSimpleCode);
	});

	public void TestBillingTypeListImport() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateInAndOutwardBillingTypeList(Factory);

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertLookupPairList(lookups.BillingTypeList, 1, RefCusCodeTestHelper.ValidSimpleCode, RefCusCodeTestHelper.InvalidSimpleCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertLookupPairList(lookups.BillingTypeList, 1, RefCusCodeTestHelper.ValidSimpleCodePast, RefCusCodeTestHelper.ValidSimpleCode);
	});

	public void TestNotifyCustomsOfficeListExport() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateNotifyCustomsOfficeList(Factory);

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		AssertLookupCombinedCollection(lookups.NotifyCustomsOfficeList, RefCusCodeTestHelper.ValidNotifyCustomsOfficeCode, RefCusCodeTestHelper.InvalidNotifyCustomsOfficeCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertLookupCombinedCollection(lookups.NotifyCustomsOfficeList, RefCusCodeTestHelper.ValidNotifyCustomsOfficeCode, RefCusCodeTestHelper.InvalidNotifyCustomsOfficeCode);
	});

	public void TestNotifyCustomsOfficeListImport() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateNotifyCustomsOfficeList(Factory);

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;

		AssertLookupCombinedCollection(lookups.NotifyCustomsOfficeList, RefCusCodeTestHelper.ValidNotifyCustomsOfficeCode, RefCusCodeTestHelper.InvalidNotifyCustomsOfficeCode);

		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);

		AssertLookupCombinedCollection(lookups.NotifyCustomsOfficeList, RefCusCodeTestHelper.ValidNotifyCustomsOfficeCodePast, RefCusCodeTestHelper.ValidNotifyCustomsOfficeCode);
	});

	void AssertLookupPairList(CodeDescriptionPairList list, int expectedNumberOfElements, string validCode, string inValidCode)
	{
		AssertEquals("Number of elements", expectedNumberOfElements, list.Count);
		Assert("Expected valid code", list.ContainsCode(validCode));
		Assert("Unexpected invalid code", !list.ContainsCode(inValidCode));
	}

	void AssertLookupCombinedCollection(ZZRefCusCodeListCombinedCollection list, string validCode, string invalidCode)
	{
		list.Load();

		AssertEquals("Valid code", true, list.OfType<ZZRefCusCodeListCombined>().Any(c => c.ZZD_Code == validCode));
		AssertEquals("Invalid code", false, list.OfType<ZZRefCusCodeListCombined>().Any(c => c.ZZD_Code == invalidCode));
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		inAndOutwardProcessing = invoiceLine.InAndOutwardProcessings.AddNew();
		lookups = inAndOutwardProcessing.Lookups;
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	CusEntryInstruction entryInstruction;
	InAndOutwardProcessing inAndOutwardProcessing;
	InAndOutwardProcessingLookups lookups;
}
