using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	[TestedType(typeof(FeatureControlSet))]
	public class FeatureControlSetTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDatabases()
		{
			var featureSet = Factory.NewWithValidTestData<FeatureControlSet>();

			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db3 = Factory.NewWithValidTestData<LicenceDatabase>();

			db1.LD_FCS_FeatureSet = featureSet.PK;
			db2.LD_FCS_FeatureSet = featureSet.PK;

			Factory.Save();

			AssertEquals(2, featureSet.Databases.Count);
			Assert(featureSet.Databases.Any(db => db.PK == db1.PK));
			Assert(featureSet.Databases.Any(db => db.PK == db2.PK));
			Assert(!featureSet.Databases.Any(db => db.PK == db3.PK));
		}

		public void TestFeatureRules()
		{
			var featureSet = Factory.NewWithValidTestData<FeatureControlSet>();

			var header1 = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule1 = header1.FeatureControlRules.AddNew();
			rule1.FCR_StartDateUtc = new ZDateTime(2024, 1, 1);

			var header2 = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule2 = header2.FeatureControlRules.AddNew();
			rule2.FCR_StartDateUtc = new ZDateTime(2024, 1, 1);

			var header3 = Factory.NewWithValidTestData<FeatureControlHeader>();
			var rule3 = header3.FeatureControlRules.AddNew();
			rule3.FCR_StartDateUtc = new ZDateTime(2024, 1, 1);

			rule1.IsFeatureSetRule = true;
			rule2.IsFeatureSetRule = true;
			rule1.FCR_FCS_FeatureSet = featureSet.PK;
			rule2.FCR_FCS_FeatureSet = featureSet.PK;

			Factory.Save();

			AssertEquals(2, featureSet.FeatureRules.Count);
			Assert(featureSet.FeatureRules.Any(rule => rule.PK == rule1.PK));
			Assert(featureSet.FeatureRules.Any(rule => rule.PK == rule2.PK));
			Assert(!featureSet.FeatureRules.Any(rule => rule.PK == rule3.PK));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Factory.New<FeatureControlSet>();
		}
	}

	[TestedType(typeof(FeatureControlSet))]
	public class FeatureControlSetAuditParentTest : AuditParentTest<FeatureControlSet>
	{
		protected override FeatureControlSet NewTestAuditParent()
		{
			return Factory.New<FeatureControlSet>();
		}
	}
}
