using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusSeaManOBLHeaderLookupsTest : BaseCusSeaManOBLHeaderLookupsTest
	{
		[TestDate(2005, 10, 12)]
		public override void TestMethodsOfPayment()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			AssertEquals(typeof(CMRMethodsOfPayment), header.Lookups.MethodsOfPayment.GetType());
		}

		public override void TestCargoCodes()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			AssertEquals(typeof(CMRImportCargoCodes), header.Lookups.CargoCodes.GetType());
		}

		public void TestConsolidatedCargoStatuses()
		{
			CusSeaManOBLHeader header = Factory.New<CusSeaManOBLHeader>();
			CodeDescriptionPairList result = header.Lookups.CargoReportStatusList;
			AssertEquals("Count", 23, result.Count);
			Assert("CargoReportStatusList should be cached", ReferenceEquals(Factory.GetCachedValue<CMRStatuses>(), result));
		}
	}
}
