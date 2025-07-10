using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class ArrivalNotificationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType() => AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);

		public void TestArrivalGoodsLocationUserControl() => AssertType<ZCodeFindBox>(control.ArrivalGoodsLocationCodeFindBox);

		public void TestRepresentativeTraderZDocAddressControl() => AssertType<ZDocAddressControl>(control.RepresentativeTraderZDocAddressControl);

		public void TestBrokerCodeFindBox() => AssertType<ZCodeFindBox>(control.BrokerCodeFindBox);

		public void TestCertificateDropEdit() => AssertType<ZDropEdit>(control.CertificateDropEdit);

		public void TestTrainingCheckBox()
		{
			var trainingCheckBox = control.TrainingCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", trainingCheckBox);
				AssertEquals("Caption", "Training Entry", trainingCheckBox.CaptionResourceString.Caption);
				AssertEquals("ShortCaption", "Training Entry", trainingCheckBox.CaptionResourceString.ShortCaption);
				AssertEquals("MediumCaption", "Training Entry", trainingCheckBox.CaptionResourceString.MediumCaption);
				AssertEquals("FullDescription", "When checked the declaration will be sent to Test", trainingCheckBox.CaptionResourceString.FullDescription);
				AssertEquals("GetBindingMember", nameof(NctsHeader.TrainingEntry), trainingCheckBox.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ArrivalNotificationDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ArrivalNotificationDetailsUserControl control;
	}
}
