using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5UnloadingDifferencesPreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestUserControlDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), userControl.BindingSource.DataSourceType);
		}

		public void TestPreviousDocumentsUserControlGroupBox()
		{
			var groupBox = userControl.PreviousDocumentGroupBox;
			AssertEquals("Caption", "Previous Documents", groupBox.CaptionResourceString.Caption);
		}

		public void TestUnloadingDifferencesPreviousDocumentsGridUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			userControl.SetDataBinding(nctsHeader, "");

			var grid = userControl.PreviousDocumentGroupBox.FindSingle<Phase5UnloadingDifferencesPreviousDocumentsGridUserControl>();
			AssertEquals("BindingMember", "PreviousDocuments", grid.GetBindingMember());
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5UnloadingDifferencesPreviousDocumentsUserControl();
		}
		Phase5UnloadingDifferencesPreviousDocumentsUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
