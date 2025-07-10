using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class ExplanationOnDelayBottomSectionUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExplanationOnDelaySendingActionParent), control.BindingSource.DataSourceType);
		}

		public void TestExplanationCodeDropEdit()
		{
			AssertType<ZDropEdit>("Type", control.ExplanationCodeDropEdit);
		}

		public void TestInformationWordWrapTextBox()
		{
			var informationWordWrapTextBox = control.InformationWordWrapTextBox;
			AssertType<WordWrappingTextBox>("Type", informationWordWrapTextBox);
			AssertEquals("CharacterCasing", CharacterCasing.Normal, informationWordWrapTextBox.CharacterCasing);
		}

		public void TestMessageRoleDropEdit()
		{
			AssertType<ZDropEdit>("Type", control.MessageRoleDropEdit);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ExplanationOnDelayBottomSectionUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ExplanationOnDelayBottomSectionUserControl control;
	}
}
