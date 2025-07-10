using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class AlertOrRejectBottomSectionUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(AlertOrRejectSendingActionParent), control.BindingSource.DataSourceType);
		}

		public void TestRejectedCheckBox()
		{
			AssertType<ZCheckBox>("Type", control.RejectedCheckBox);
		}

		public void TestAlertRejectionDate()
		{
			AssertType<ZDateEdit>("Type", control.AlertRejectionDate);
		}

		public void TestGridColumnDetails()
		{
			var reasonsGrid = control.ReasonGrid;
			CombineAssertions(() =>
			{
				var reasonColumnStyle = reasonsGrid.GetColumnStyle(nameof(AlertOrRejectReason.Reason));
				AssertEquals("Reason Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58), reasonColumnStyle.Width);
				AssertEquals("Reason Casing", CharacterCasing.Upper, reasonColumnStyle.CharacterCasing);
				var reasonInfoColumnStyle = reasonsGrid.GetColumnStyle(nameof(AlertOrRejectReason.Information));
				AssertEquals("Reason Info Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(233), reasonInfoColumnStyle.Width);
				AssertEquals("Reason Info Casing", CharacterCasing.Normal, reasonInfoColumnStyle.CharacterCasing);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new AlertOrRejectBottomSectionUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		AlertOrRejectBottomSectionUserControl control;
	}
}
