using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public sealed class JobComInvoiceHeaderValuePostProcessingStrategyTest : TestCaseWithFactory
	{
		public void TestOnZG_AgreedPlaceCodeChanges()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = "SEA";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_IncoTerm = "CIF";

			AssertEquals("Prerequiste: ZG_AgreedPlaceCode should be empty", ZString.Empty, invoice.ZG_AgreedPlaceCode);
			AssertEquals("Prerequiste: Number of charges should be 0", 0, declaration.TopGroupInvoice.Charges.Count);

			invoice.ZG_AgreedPlaceCode = "1";
			AssertEquals("Group charges should be updated based on ZG_AgreedPlaceCode", 2, declaration.TopGroupInvoice.Charges.Count);

			invoice.ZG_AgreedPlaceCode = "2";
			AssertEquals("Group charges should be updated based on ZG_AgreedPlaceCode", 4, declaration.TopGroupInvoice.Charges.Count);
		}
	}
}
