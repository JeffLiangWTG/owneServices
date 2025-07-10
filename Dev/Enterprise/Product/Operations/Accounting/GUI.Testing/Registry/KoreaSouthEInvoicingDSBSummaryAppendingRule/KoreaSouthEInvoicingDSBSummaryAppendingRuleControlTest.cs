using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.Environment;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingDSBSummaryAppendingRuleControl))]
	class KoreaSouthEInvoicingDSBSummaryAppendingRuleControlTest : RegistryZUserControlTestCase
	{
		public override void TestBoundListsAreNotLoadedOnAccess()
		{
			Assert(true);
		}

		protected override IBusiness GetNewBusinessEntity() => new KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory)
		{
			new KoreaSouthEInvoicingDSBSummaryAppendingRule(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory)
		};

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var koreaSouthEInvoicingDSBSummaryAppendingRuleControl = control as KoreaSouthEInvoicingDSBSummaryAppendingRuleControl;
			AssertNotNull(koreaSouthEInvoicingDSBSummaryAppendingRuleControl);

			var grid = koreaSouthEInvoicingDSBSummaryAppendingRuleControl.FindSingleOrDefault<ZGrid>("ConfigurationGrid");
			AssertNotNull(grid);

			return grid.ReadOnly;
		}
	}
}
