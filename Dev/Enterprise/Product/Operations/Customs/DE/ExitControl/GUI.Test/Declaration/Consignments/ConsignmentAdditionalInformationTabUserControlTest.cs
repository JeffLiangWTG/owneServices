using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.ExitControl.GUI.Testing
{
	sealed class ConsignmentAdditionalInformationTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals(typeof(ExitControlAdditionalInfoCollection), userControl.BindingSource.DataSourceType);
		}

		public void TestAdditionalInformationSplitContainer()
		{
			CombineAssertions(() =>
			{
				var splitContainer = userControl.AdditionalInformationSplitContainer;
				AssertEquals("Dock", DockStyle.Fill, splitContainer.Dock);
				AssertEquals("Orientation", Orientation.Horizontal, splitContainer.Orientation);
				AssertEquals("Panel2MinSize", 50, splitContainer.Panel2MinSize);
			});
		}

		public void TestConsignmentAdditionalInformationGridUserControl()
		{
			var gridUserControl = userControl.ConsignmentAdditionalInformationGridUserControl;
			CombineAssertions(() =>
			{
				AssertEquals("Dock", DockStyle.Fill, gridUserControl.Dock);
				AssertEquals("BindingMember", ".", gridUserControl.GetBindingMember());
				AssertEquals("Is within AdditionalDocumentsSplitContainer.Panel1", true, userControl.AdditionalInformationSplitContainer.Panel1.Contains(gridUserControl));
			});
		}

		public void TestFullTypeCodeFindBox()
		{
			var fullTypeCodeFindBox = userControl.FullTypeCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", fullTypeCodeFindBox);
				AssertEquals("BindTo", nameof(ExitControlAdditionalInfo.CSI_Code), fullTypeCodeFindBox.BindTo);
				AssertEquals("Is within AdditionalDocumentsSplitContainer.Panel2", true, userControl.AdditionalInformationSplitContainer.Panel2.Contains(fullTypeCodeFindBox));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ConsignmentAdditionalInformationTabUserControl();
		}
		ConsignmentAdditionalInformationTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
