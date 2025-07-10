using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

internal class TransportChargesDataProviderTest : TestCaseWithFactory
{
	public void TestNew()
	{
		CombineAssertions(() =>
		{
			AssertNull("NctsHeader==null", TransportChargesDataProvider.New(null));

			AssertNull("MovementHeader != null, BM_MethodOfPayment empty", TransportChargesDataProvider.New(NctsHeader.MovementHeader));

			NctsHeader.MovementHeader.BM_MethodOfPayment = "A";
			AssertNotNull("MovementHeader != null, BM_MethodOfPayment not empty", TransportChargesDataProvider.New(NctsHeader.MovementHeader));
		});
	}

	public void TestProvider()
	{
		NctsHeader.MovementHeader.BM_MethodOfPayment = "A";

		AssertEquals("Method Of Payment", "A", TransportCharges.MethodOfPayment);
	}

	public void TestMethodOfPayment()
	{
		CombineAssertions(() =>
		{
			AssertNull("no Method of Payment specified", TransportCharges);

			NctsHeader.MovementHeader.BM_MethodOfPayment = "A";

			AssertEquals("Method Of Payment", "A", TransportCharges.MethodOfPayment);
		});
	}

	public void TestSequenceNumber()
	{
		NctsHeader.MovementHeader.BM_MethodOfPayment = "A";
		AssertNull("SequenceNumber not available", TransportCharges.SequenceNumber);
	}

	public void TestAmount()
	{
		NctsHeader.MovementHeader.BM_MethodOfPayment = "A";
		AssertNull("Amount not available", TransportCharges.Amount);
	}

	public void TestCurrency()
	{
		NctsHeader.MovementHeader.BM_MethodOfPayment = "A";
		AssertNull("Amount not available", TransportCharges.Currency);
	}

	public void TestType()
	{
		NctsHeader.MovementHeader.BM_MethodOfPayment = "A";
		AssertNull("Type not available", TransportCharges.Type);
	}

	public void TestIncludedInInvoice()
	{
		NctsHeader.MovementHeader.BM_MethodOfPayment = "A";
		AssertNull("IncludedInInvoice not available", TransportCharges.IncludedInInvoice);
	}

	NctsHeader CreateNctsHeader()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader;
	}

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	public ITransportCharges TransportCharges => transportCharges ?? (transportCharges = TransportChargesDataProvider.New(NctsHeader.MovementHeader));
	ITransportCharges transportCharges;
}
