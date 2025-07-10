using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ETHeaderSecurityBlockWrapperTest : TestCaseWithFactory
{
	public void TestSpecificCircumstanceIndicator()
	{
		AssertEquals("Field not wrapped only on a temporary basis.", ZString.Empty, etHeaderSecurityBlock.SpecificCircumstanceIndicator);
	}

	public void TestPlaceOfLoadingCode()
	{
		AssertEquals(ZString.Empty, etHeaderSecurityBlock.PlaceOfLoadingCode);
	}

	public void TestTransportChargesMethodOfPayment()
	{
		AssertEquals(ZString.Empty, etHeaderSecurityBlock.TransportChargesMethodOfPayment);

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;

		invoice1.ZG_TransportChargesMethodOfPayment = "X";
		invoice2.ZG_TransportChargesMethodOfPayment = "X";
		entryHeader.ResetInvoiceHeadersAndLines();
		AssertEquals("X", etHeaderSecurityBlock.TransportChargesMethodOfPayment);

		invoice2.ZG_TransportChargesMethodOfPayment = "A";
		AssertEquals(ZString.Empty, etHeaderSecurityBlock.TransportChargesMethodOfPayment);
	}

	public void TestCommercialReferenceNumber()
	{
		AssertEquals("Field not wrapped only on a temporary basis.", ZString.Empty, etHeaderSecurityBlock.CommercialReferenceNumber);
	}

	public void TestConveyanceReferenceNumber()
	{
		AssertEquals(ZString.Empty, etHeaderSecurityBlock.ConveyanceReferenceNumber);
	}

	public void TestPlaceOfUnloadingCode()
	{
		AssertEquals(ZString.Empty, etHeaderSecurityBlock.PlaceOfUnloadingCode);
	}

	public void TestTransitCountries()
	{
		CombineAssertions(() =>
		{
			var transitCountries = etHeaderSecurityBlock.TransitCountries;
			AssertNotNull(transitCountries);
			AssertEquals(0, transitCountries.Count());
		});
	}

	public void TestConsignor()
	{
		CombineAssertions(() =>
		{
			var consignor = etHeaderSecurityBlock.Consignor;
			AssertNotNull(consignor);
			AssertType<SADEmptyTraderWrapper>(consignor);
		});
	}

	public void TestConsignee()
	{
		CombineAssertions(() =>
		{
			var consignee = etHeaderSecurityBlock.Consignee;
			AssertNotNull(consignee);
			AssertType<SADEmptyTraderWrapper>(consignee);
		});
	}

	public void TestCarrier()
	{
		CombineAssertions(() =>
		{
			var carrier = etHeaderSecurityBlock.Carrier;
			AssertNotNull(carrier);
			AssertType<SADEmptyTraderWrapper>(carrier);
		});
	}

	public void TestSealsNumber()
	{
		entryInstruction.ZG_SealsCount = 99;
		AssertEquals("SealsNumber", 99, etHeaderSecurityBlock.SealsNumber);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderSecurityBlockWrapper(null));
		AssertExceptionThrown<ArgumentNullException>(() => new ETHeaderSecurityBlockWrapper(Factory.New<CusEntryHeader>()));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		etHeaderSecurityBlock = new ETHeaderSecurityBlockWrapper(entryHeader);
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryHeader entryHeader;

	ETHeaderSecurityBlockWrapper etHeaderSecurityBlock;
}
