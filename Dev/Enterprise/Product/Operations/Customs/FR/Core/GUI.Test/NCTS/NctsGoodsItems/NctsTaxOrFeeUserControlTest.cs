using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	public class NctsTaxOrFeeUserControlTest : TestCaseWithFactory
	{
		public void TestUserControl()
		{
			using (var form = new ZForm())
			using (var control = new NctsTaxOrFeeUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var feesGrid = control.FindSingle<ZGrid>("TaxOrFeesGrid");
				AssertNotNull(feesGrid);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "BFE_ChargeType", "BFE_ChargeAmount", "BFE_RateOverrideReasonCode" }, feesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}
	}
}
