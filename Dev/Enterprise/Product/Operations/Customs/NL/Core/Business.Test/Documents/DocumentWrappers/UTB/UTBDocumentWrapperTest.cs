using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

class UTBDocumentWrapperTest : TestCaseWithFactory
{
	protected override void SetUp()
	{
		base.SetUp();
		var entryHeader = DocumentWrapperTestHelper.GetEntryHeaderForTest(Factory);
		wrapper = new UTBDocumentWrapper(entryHeader);
	}
	UTBDocumentWrapper wrapper;

	public void TestDatOfIssue()
	{
		var dateOfIssue = new ZDateTime(2021, 11, 18, 15, 00, 00);
		AssertEquals("DateOfIssue", dateOfIssue.ToString("yyyyMMdd"), wrapper.DateOfIssue.ToString("yyyyMMdd"));
	}

	[TestDate(2021, 11, 17, 14, 15, 30)]
	public void TestObjectionDate()
	{
		var dateOfIssue = new ZDateTime(2021, 11, 18, 15, 00, 00);
		var objectionDate = dateOfIssue.AddDays(UTBDocumentWrapper.ObjectionIntervalInDays);
		AssertEquals("Objection Date", objectionDate.ToString("yyyyMMdd"), wrapper.ObjectionDateTime.ToString("yyyyMMdd"));
	}

	public void TestMovementReferenceNumber()
	{
		AssertEquals("MRN", "MRN1234567890", wrapper.MovementReferenceNumber);
	}

	public void TestControllingAgent()
	{
		var controllingAgent = wrapper.Agent;
		AssertEquals("Agent name", "Chris the Controlling Agent", controllingAgent.Name);
		AssertEquals("Agent EORInumber", "123456789", controllingAgent.EORINumber);
		AssertEquals("Agent Address", "CAstreet 12", controllingAgent.Address);
		AssertEquals("Agent City", "Rotterdam", controllingAgent.City);
		AssertEquals("Agent PostCode", "1079CK", controllingAgent.PostCode);
		AssertEquals("Agent CountryCode", "NL", controllingAgent.CountryCode);
	}

	public void TestDeclarant()
	{
		var declarant = wrapper.Declarant;
		AssertEquals("Declarant name", "Delta the Declarant", declarant.Name);
		AssertEquals("Declarant EORInumber", "987654321", declarant.EORINumber);
		AssertEquals("Declarant Address", "Decstreet 12", declarant.Address);
		AssertEquals("Declarant City", "Brussel", declarant.City);
		AssertEquals("Declarant PostCode", "2010AB", declarant.PostCode);
		AssertEquals("Declarant CountryCode", "BE", declarant.CountryCode);
	}

	public void TestDutiesAndTaxCalculation()
	{
		var totalDuties = 96m + 24m + 48m + 1020m;
		var totalTax = 252m + 16800m + 3570m;
		var totalDutiesAndTax = totalDuties + totalTax;
		AssertEquals("Total duties and taxes", totalDutiesAndTax, wrapper.TotalDutiesAndTaxes);
	}

	public void TestUCR()
	{
		AssertEquals("UCR", "2-B00169514", wrapper.UCR);
	}

	public void TestNumberOfEntryLines()
	{
		AssertEquals("Number of entry lines", "3", wrapper.NumberOfEntryLines);
	}
}
