using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5ArrivalNotificationTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals("Phase5ArrivalNotificationTabUserControl BindingSource DataSourceType", typeof(NctsHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestArrivalDetailsGroupBox()
		{
			var arrivalDetailsGroupBox = userControl.ArrivalDetailsGroupBox;
			var dynamicArrivalDetailsPanel = userControl.DynamicArrivalDetailsPanel;
			CombineAssertions(() =>
			{
				AssertEquals("ArrivalDetailsGroupBox Caption", "Arrival Details", arrivalDetailsGroupBox.CaptionResourceString.Caption);
				AssertEquals("ArrivalDetailsGroupBox TabIndex", 0, arrivalDetailsGroupBox.TabIndex);
				AssertEquals("ArrivalDetailsGroupBox Anchor", AnchorStyles.Top | AnchorStyles.Left, arrivalDetailsGroupBox.Anchor);
				AssertEquals("DynamicArrivalDetailsPanel is within ArrivalDetailsGroupBox", true, arrivalDetailsGroupBox.Controls.Contains(dynamicArrivalDetailsPanel));
				AssertEquals("DynamicArrivalDetailsPanel Dock", DockStyle.Fill, dynamicArrivalDetailsPanel.Dock);
			});
		}

		public void TestDeclarationDetailsGroupBox()
		{
			var declarationDetailsGroupBox = userControl.DeclarationDetailsGroupBox;
			var dynamicDeclarationDetailsPanel = userControl.DynamicDeclarationDetailsPanel;
			CombineAssertions(() =>
			{
				AssertEquals("DeclarationDetailsGroupBox Caption", "Declaration Details", declarationDetailsGroupBox.CaptionResourceString.Caption);
				AssertEquals("DeclarationDetailsGroupBox TabIndex", 2, declarationDetailsGroupBox.TabIndex);
				AssertEquals("DeclarationDetailsGroupBox Anchor", AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, declarationDetailsGroupBox.Anchor);
				AssertEquals("DynamicDeclarationDetailsPanel is within DeclarationDetailsGroupBox", true, declarationDetailsGroupBox.Controls.Contains(dynamicDeclarationDetailsPanel));
				AssertEquals("DynamicDeclarationDetailsPanel Dock", DockStyle.Fill, dynamicDeclarationDetailsPanel.Dock);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5ArrivalNotificationTabUserControl();
		}
		Phase5ArrivalNotificationTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
