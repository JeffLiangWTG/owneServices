using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NonCustomsLawLookups))]
sealed class NonCustomsLawLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTypeCodesList() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateNonCustomsLawTypeCodesList(Factory);

		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var nonCustomsLaw = invoiceLine.NonCustomsLaws.AddNew();

		var expectedCodes = new string[] { "26", "30", "44", "66", "669" };
		var expectedCodesImpPast = new string[] { "30", "44", "66", "669" };

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		AssertContainsExactElementsInAnyOrder("EXP Type codes list content", expectedCodes, ((CodeDescriptionPairList)nonCustomsLaw.Lookups.CodeList).GetAllCodes());

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		AssertContainsExactElementsInAnyOrder("IMP Type codes list content", expectedCodes, ((CodeDescriptionPairList)nonCustomsLaw.Lookups.CodeList).GetAllCodes());

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 12, 31);
		invoiceLine.JI_CEI = entryInstruction.PK;

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
		AssertContainsExactElementsInAnyOrder("EXP Type codes list content - Past", expectedCodes, ((CodeDescriptionPairList)nonCustomsLaw.Lookups.CodeList).GetAllCodes());

		declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
		AssertContainsExactElementsInAnyOrder("IMP Type codes list content - Past", expectedCodesImpPast, ((CodeDescriptionPairList)nonCustomsLaw.Lookups.CodeList).GetAllCodes());
	});
}
