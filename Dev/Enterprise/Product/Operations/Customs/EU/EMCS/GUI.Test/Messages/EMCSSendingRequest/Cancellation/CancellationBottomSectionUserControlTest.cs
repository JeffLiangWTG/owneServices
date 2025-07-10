using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class CancellationBottomSectionUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CancellationSendingActionParent), control.BindingSource.DataSourceType);
		}

		public void TestReasonDropEdit()
		{
			AssertType<ZDropEdit>("Type", control.ReasonDropEdit);
		}

		public void TestInformationTextBox()
		{
			var informationTextBox = control.InformationTextBox;
			AssertType<ZTextBox>("Type", informationTextBox);
			AssertEquals("CharacterCasing", CharacterCasing.Normal, informationTextBox.CharacterCasing);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new CancellationBottomSectionUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		CancellationBottomSectionUserControl control;
	}
}
