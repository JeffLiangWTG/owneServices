using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class ReasonForShortageBottomSectionUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ReasonForShortageSendingActionParent), control.BindingSource.DataSourceType);
		}

		public void TestGeneralExplanationTextBox()
		{
			var generalExplanationTextBox = control.GeneralExplanationTextBox;
			AssertType<WordWrappingTextBox>("Type", generalExplanationTextBox);
			AssertEquals("CharacterCasing", CharacterCasing.Normal, generalExplanationTextBox.CharacterCasing);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ReasonForShortageBottomSectionUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ReasonForShortageBottomSectionUserControl control;
	}
}
