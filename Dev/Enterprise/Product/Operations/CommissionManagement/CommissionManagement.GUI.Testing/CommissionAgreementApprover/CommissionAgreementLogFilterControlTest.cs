using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	class CommissionAgreementLogFilterControlTest : TestCaseWithFactory
	{
		public void TestAvailableColumns()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			using (var module = new CommissionAgreementLogFilterModule())
			{
				module.InitData(agreement.GetMainVersion());
				var control = module.EmbeddedControl as CommissionAgreementLogFilterControl;
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						StmALog.Schema.SL_EventTime, StmALog.Schema.SL_PostedTimeUtc, StmALog.Schema.SL_TableFriendlyNameForBinding, "SL_ReferenceForBinding", StmALog.Schema.SL_UserNameAndInitials
					},
					control.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).Select(x => x.ColumnName));
			}
		}
	}
}
