using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Export.Testing
{
	class DeclarationNFeInvoiceProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationNFeInvoiceProvider()
		{
			var cusEntryLine = Factory.New<CusEntryLine>();

			var invHeader = Factory.New<JobComInvoiceHeader>();
			invHeader.JZ_InvoiceAmount = 10m;
			invHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invHeader.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CL = cusEntryLine.PK;
			invLine.JI_NFeNumber = "00000000000000000000000000000000000000000000";
			invLine.JI_NFeItemNumber = "001";

			var goodsInvoice = new DeclarationNFeInvoiceProvider(invLine);

			AssertEquals("NFEKey should be", "00000000000000000000000000000000000000000000", goodsInvoice.NFEKey);
			AssertEquals("DestinationCountry should be", (ZShort)1, goodsInvoice.NFEItemNumber);
			AssertEquals("IntendedTerms should be", ZInt.Zero, goodsInvoice.NFEItemSequence);
			AssertEquals("NetWeight should be", ZDecimal.Zero, goodsInvoice.CustomsQuantityRelated);
			AssertEquals("TypeInvoice should be", ZString.Empty, goodsInvoice.Type);
		}
	}
}
