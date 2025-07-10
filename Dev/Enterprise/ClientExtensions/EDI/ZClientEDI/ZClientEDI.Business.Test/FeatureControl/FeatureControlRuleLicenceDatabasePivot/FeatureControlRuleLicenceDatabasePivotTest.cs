using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	[TestedType(typeof(FeatureControlRuleLicenceDatabasePivot))]
	public class FeatureControlRuleLicenceDatabasePivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<FeatureControlRuleLicenceDatabasePivot>();
		}

		[TestDate(2024, 1, 1)]
		public void TestDelete()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule1 = header.FeatureControlRules.AddNew();
			var pivot = rule1.LicenceDatabasePivots.AddNew();
			rule1.FCR_SystemLastEditTimeUtc = new ZDateTime(2000, 1, 1);

			pivot.Delete();
			AssertEquals(new ZDateTime(2024, 1, 1), rule1.FCR_SystemLastEditTimeUtc);

			AssertNoExceptionThrown(() =>
			{
				var pivot2 = Factory.New<FeatureControlRuleLicenceDatabasePivot>();
				pivot2.Delete();
			});
		}

		public void TestAudit()
		{
			var obj = Factory.NewWithValidTestData<FeatureControlRuleLicenceDatabasePivot>();
			Assert(!obj.IsAutoLogged);
		}
	}
}
