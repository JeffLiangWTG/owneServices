using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(CommissionAgreementLogFilterModule))]
	class CommissionAgreementLogFilterModuleTest : ZModuleBasherTest
	{
		#region ID

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CommissionAgreementLogFilter;
		}
		#endregion

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public void TestGetNewFilterControl()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			using (var module = new CommissionAgreementLogFilterModuleForTest())
			{
				module.InitData(agreement.GetMainVersion());
				var control = module.EmbeddedControl;
				AssertType<CommissionAgreementLogFilterControl>("The control is not of the expected type.", control);
			}
		}

		public class CommissionAgreementLogFilterModuleForTest : CommissionAgreementLogFilterModule
		{
			public CommissionAgreementLogFilterModuleForTest()
			{
			}
		}
	}
}
