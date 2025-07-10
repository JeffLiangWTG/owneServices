using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class TraderDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBrokerCodeFindBox()
		{
			var brokerFindBox = control.BrokerCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", brokerFindBox);
				AssertEquals("GetBindingMember", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_GS_NKCusAgent), brokerFindBox.GetBindingMember());
			});
		}

		public void TestCertificateDropEdit()
		{
			var certificateDropEdit = control.CertificateDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", certificateDropEdit);
				AssertEquals("GetBindingMember", nameof(NctsHeader.BH_CustomsProfile), certificateDropEdit.GetBindingMember());
			});
		}

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
			control = new TraderDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TraderDetailsUserControl control;
	}
}
