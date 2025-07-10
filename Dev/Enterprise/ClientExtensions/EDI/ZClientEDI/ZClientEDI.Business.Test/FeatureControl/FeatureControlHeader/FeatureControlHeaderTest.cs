using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.Business.Testing
{
	[TestedType(typeof(FeatureControlHeader))]
	public class FeatureControlHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<FeatureControlHeader>();
		}

		public void TestAudit()
		{
			var obj = Factory.NewWithValidTestData<FeatureControlHeader>();
			Assert(!obj.IsAutoLogged);
			AssertNotNull(obj.RelatedAuditChildren.Single(x => x.KeyColumn == FeatureControlRuleSchema.FCR_FCM_FeatureControl && x.InfoColumn == FeatureControlRuleSchema.FCR_Description));
		}

		public void TestStatus()
		{
			var activeCode = "CR5RESWIZ";
			var inDevelopmentCode = "ACCRBKFTR";
			var sunsettingCode = "NOTEXIST";
			var obj = Factory.NewWithValidTestData<FeatureControlHeader>();

			obj.FCM_FeatureControlCode = activeCode;
			AssertEquals("Active", obj.Status);

			obj.FCM_FeatureControlCode = inDevelopmentCode;
			AssertEquals("In Development", obj.Status);

			obj.FCM_FeatureControlCode = sunsettingCode;
			AssertEquals("Sunsetting", obj.Status);
		}
	}

	[TestedType(typeof(FeatureControlHeader))]
	public class FeatureControlHeaderAuditParentTest : AuditParentTest<FeatureControlHeader>
	{
		protected override FeatureControlHeader NewTestAuditParent()
		{
			return Factory.New<FeatureControlHeader>();
		}
	}
}
