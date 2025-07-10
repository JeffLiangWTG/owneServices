using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class JobComInvoiceHeaderCCNsLookupsTest : TestCaseWithFactory
	{
		public void TestCargoControlNumbersList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.Invoices.AddNew();
			var release = declaration.ReleaseStatuses.AddNew();
			release.RL_CargoControlNumber = "00001";
			var release2 = declaration.ReleaseStatuses.AddNew();
			release2.RL_CargoControlNumber = "00002";
			var headerCCN = header.CargoControlNumbersList.AddNew();
			AssertEquals("00001, 00002", headerCCN.Lookups.CargoControlNumbersList.CodesAsString);
		}

		public void TestCargoControlNumbersList_NullDeclaration()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			var headerCCN = header.CargoControlNumbersList.AddNew();
			AssertEquals(ZString.Empty, headerCCN.Lookups.CargoControlNumbersList.CodesAsString);
		}
	}
}
