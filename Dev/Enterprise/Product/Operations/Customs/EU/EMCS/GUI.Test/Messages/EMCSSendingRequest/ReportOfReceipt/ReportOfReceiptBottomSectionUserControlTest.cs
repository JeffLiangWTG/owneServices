using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class ReportOfReceiptBottomSectionUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ReportOfReceiptSendingActionParent), control.BindingSource.DataSourceType);
		}

		public void TestReceiptResultDropEdit()
		{
			AssertType<ZDropEdit>("Type", control.ReceiptResultDropEdit);
		}

		public void TestInformationTextBox()
		{
			var informationTextBox = control.ComplementaryInformationWrapTextBox;
			AssertType<WordWrappingTextBox>("Type", informationTextBox);
			AssertEquals("CharacterCasing", CharacterCasing.Normal, informationTextBox.CharacterCasing);
		}

		public void TestArrivalDate()
		{
			var arrivalDateEdit = control.ArrivalDate;
			AssertType<ZDateEdit>("Type", arrivalDateEdit);
			AssertEquals("ZDateTimePickerFormat", ZDateTimePickerFormat.Long, arrivalDateEdit.DateTimeFormat);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ReportOfReceiptBottomSectionUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ReportOfReceiptBottomSectionUserControl control;
	}
}
