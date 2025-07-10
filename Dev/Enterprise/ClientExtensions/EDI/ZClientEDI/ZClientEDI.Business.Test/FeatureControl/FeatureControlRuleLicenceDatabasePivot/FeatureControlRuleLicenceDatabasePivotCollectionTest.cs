using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	[TestedType(typeof(FeatureControlRuleLicenceDatabasePivotCollection))]
	public class FeatureControlRuleLicenceDatabasePivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<FeatureControlHeader>();
			var ruleGlobal = header.FeatureControlRules.AddNew();
			ruleGlobal.IsGlobalRule = true;
			ruleGlobal.FCR_Parameters = "{\"key1\":\"value1\"}";
			return new FeatureControlRuleLicenceDatabasePivotCollection(ruleGlobal);
		}
	}
}
