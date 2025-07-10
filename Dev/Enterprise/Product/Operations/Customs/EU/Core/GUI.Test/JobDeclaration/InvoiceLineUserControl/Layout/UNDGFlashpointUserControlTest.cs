using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class UNDGFlashpointUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(UNDGDataItem), control.BindingSource.DataSourceType);
		}

		public void TestFlashPointCalcEdit()
		{
			var flashPointCalcEdit = control.FlashPointCalcEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcEdit>("Type", flashPointCalcEdit);
				AssertEquals("BindingMember", nameof(UNDGDataItem.DI_DGFlashPoint), flashPointCalcEdit.GetBindingMember());
			});
		}

		public void TestFlashPointDescLabel()
		{
			var flashPointDescLabel = control.FlashPointDescLabel;
			CombineAssertions(() =>
			{
				AssertType<ZLabel>("Type", flashPointDescLabel);
				AssertEquals("Caption", "(Manufacturer Specified)", flashPointDescLabel.CaptionResourceString.Caption);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new UNDGFlashpointUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		UNDGFlashpointUserControl control;
	}
}
