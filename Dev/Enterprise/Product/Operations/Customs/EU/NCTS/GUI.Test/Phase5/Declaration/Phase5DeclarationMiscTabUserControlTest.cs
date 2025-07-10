using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5DeclarationMiscTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals("BindingSource DataSourceType", typeof(NctsHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("CaptionRenderingEnabled", true, userControl.CaptionRenderingEnabled);
		}

		public void TestMiscOptionsGroupBox()
		{
			CombineAssertions(() =>
			{
				AssertEquals("MiscOptionsGroupBox Caption", "Miscellaneous Options", userControl.MiscOptionsGroupBox.CaptionResourceString.Caption);
				AssertEquals("DynamicMiscOptionsPanel is within MiscOptionsGroupBox", true, userControl.MiscOptionsGroupBox.Controls.Contains(userControl.DynamicMiscOptionsPanel));
				AssertEquals("DynamicMiscOptionsPanel Dock", DockStyle.Fill, userControl.DynamicMiscOptionsPanel.Dock);
			});
		}

		[RequiresSTA]
		public void TestDynamicMiscOptionsPanelLayout()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			userControl.SetDataBinding(nctsHeader, "");

			var expectedControlsOrder = new string[]
			{
				nameof(MiscellaneousOptionsControlBag.BranchCodeFindBox),
			};
			DynamicLayoutPanelTest.AssertControlsOrder(userControl.DynamicMiscOptionsPanel, expectedControlsOrder);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5DeclarationMiscTabUserControl();
		}

		Phase5DeclarationMiscTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
