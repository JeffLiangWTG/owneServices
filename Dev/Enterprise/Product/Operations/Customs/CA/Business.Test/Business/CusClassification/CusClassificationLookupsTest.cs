using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusClassificationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestClassification()
		{
			var parent = Factory.New<CusClassification>();
			AssertEquals(parent.Lookups.Classification, parent);
			parent.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			AssertEquals(typeof(CACExportTariffCollection), parent.Lookups.Tariffs.GetType());
			parent.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			AssertEquals(typeof(CACClassCollection), parent.Lookups.Tariffs.GetType());
		}

		public void TestAuthorityNumberList()
		{
			var classification = Factory.New<CusClassification>();
			classification.CCA_AuthorityNumber = "C0001";

			AssertType<CACusRulingFindBoxCollection>(classification.Lookups.AuthorityNumberList);

			var classification2 = Factory.New<CusClassification>();
			classification2.CCA_AuthorityNumber = "C0001";

			AssertSame(classification.Lookups.AuthorityNumberList, classification2.Lookups.AuthorityNumberList);
		}

		public void TestManufacturers()
		{
			var classification = Factory.New<CusClassification>();
			AssertType<OrgHeaderCollection>(classification.Lookups.Manufacturers);
		}
	}
}
