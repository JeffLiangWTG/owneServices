using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineExDocRexAcknowledgementLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var ack = Factory.New<QuarantineExDocRexAcknowledgement>();
			var list = ack.Lookups.CY_CodeList;
			AssertEquals(QuarantineExDocRexAcknowledgement.AcknowledgementCode, list.CodesAsString);

			var ack2 = Factory.New<QuarantineExDocRexAcknowledgement>();
			AssertSame(list, ack2.Lookups.CY_CodeList);
		}
	}
}
