using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection))]
	class KoreaSouthEInvoicingDSBSummaryAppendingRuleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override bool SupportsAddNew => true;

		protected override KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection GetCollectionToTest()
		{
			return new KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new KoreaSouthEInvoicingDSBSummaryAppendingRule(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		public void TestAllowRemove()
		{
			AssertEquals("PreCondition", false, GetCollectionToTest().ReadOnly);
			AssertEquals(true, GetCollectionToTest().AllowRemove);
		}
	}
}
