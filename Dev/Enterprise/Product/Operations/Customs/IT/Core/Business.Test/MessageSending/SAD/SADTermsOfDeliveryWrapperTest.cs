using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADTermsOfDeliveryWrapperTest : TestCaseWithFactory
{
	public void TestIncotermCode()
	{
		AssertEquals(ZString.Empty, termsOfDeliveryWrapper.IncotermCode);
		invoice.JZ_IncoTerm = "DAF";
		AssertEquals("DAF", termsOfDeliveryWrapper.IncotermCode);
	}

	public void TestComplementaryCode()
	{
		AssertEquals(ZString.Empty, termsOfDeliveryWrapper.ComplementaryCode);
		invoice.ZG_AgreedPlaceCode = "1";
		AssertEquals("1", termsOfDeliveryWrapper.ComplementaryCode);
	}

	public void TestComplementOfInfo()
	{
		AssertEquals(ZString.Empty, termsOfDeliveryWrapper.ComplementOfInfo);
		invoice.JZ_IncoTermPlace = "PLACE";
		AssertEquals("PLACE", termsOfDeliveryWrapper.ComplementOfInfo);
	}

	public void TestComplementOfInfoLng()
	{
		AssertEquals(ZString.Empty, termsOfDeliveryWrapper.ComplementOfInfoLng);
	}
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new SADTermsOfDeliveryWrapper(null));
		AssertNoExceptionThrown(() => new SADTermsOfDeliveryWrapper(entryHeader));
	}

	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		invoice = jobDeclaration.Invoices.AddNew();
		entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		termsOfDeliveryWrapper = new SADTermsOfDeliveryWrapper(entryHeader);
	}

	JobDeclaration jobDeclaration;
	JobComInvoiceHeader invoice;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;
	SADTermsOfDeliveryWrapper termsOfDeliveryWrapper;
}

