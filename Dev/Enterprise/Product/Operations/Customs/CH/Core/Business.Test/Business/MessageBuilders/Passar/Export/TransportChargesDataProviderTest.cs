using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class TransportChargesDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("EntryInstruction==null", ExporterDataProvider.New(null));

			var instruction = Factory.New<CusEntryInstruction>();
			AssertNull("EntryInstruction!=null, TransportChargesMethodOfPayment empty", TransportChargesDataProvider.New(instruction));
		});
	}

	public void TestMethodOfPayment()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		instruction.CEI_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
		AssertEquals("Method of Payment", TransportChargesModeOfPayment.Codes.Cash, TransportChargesDataProvider.New(instruction).MethodOfPayment);
	}

	public void TestSequenceNumber()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		instruction.CEI_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
		AssertNull("SequenceNumber not available", TransportChargesDataProvider.New(instruction).SequenceNumber);
	}

	public void TestAmount()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		instruction.CEI_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
		AssertNull("Amount not available", TransportChargesDataProvider.New(instruction).Amount);
	}

	public void TestCurrency()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		instruction.CEI_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
		AssertNull("Currency not available", TransportChargesDataProvider.New(instruction).Currency);
	}

	public void TestType()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		instruction.CEI_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
		AssertNull("Type not available", TransportChargesDataProvider.New(instruction).Type);
	}

	public void TestIncludedInInvoice()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		instruction.CEI_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
		AssertNull("IncludedInInvoice not available", TransportChargesDataProvider.New(instruction).IncludedInInvoice);
	}
}
