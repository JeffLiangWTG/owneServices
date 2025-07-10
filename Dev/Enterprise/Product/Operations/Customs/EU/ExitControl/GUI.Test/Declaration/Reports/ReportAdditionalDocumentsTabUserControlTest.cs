using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ReportAdditionalDocumentsTabUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals(typeof(IAdditionalInfoCollection<AdditionalInfo>), userControl.BindingSource.DataSourceType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ReportAdditionalDocumentsTabUserControl();
		}

		ReportAdditionalDocumentsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
