using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Testing
{
	internal class CusEntryPayInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransactionTypeList()
		{
			var lookups = Factory.New<CusEntryPayInfo>().Lookups;
			var transactionTypeList = (CodeDescriptionPairList)lookups.TransactionTypeList;
			AssertEquals("CAS, PVA", transactionTypeList.CodesAsString);
		}

		public void TestPaymentStatusList()
		{
			var lookups = Factory.New<CusEntryPayInfo>().Lookups;
			var paymentStatusList = (CodeDescriptionPairList)lookups.PaymentStatusList;
			AssertEquals("PAY, FUN, PUN", paymentStatusList.CodesAsString);
		}
	}
}
