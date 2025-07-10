using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	[TestedType(typeof(ScheduleDatePickerForm))]
	sealed class ScheduleDatePickerFormTest : ZFormBasherTest
	{
		public void TestOKButton()
		{
			var moqForm = new Mock<ScheduleDatePickerForm>(new object[] { Factory.NewWithValidTestData<ReportScheduleTask>() }) { CallBase = true };
			using (ScheduleDatePickerForm form = moqForm.Object)
			{
				form.Show();

				moqForm.Setup(m => m.FireSaveButton(null)).Returns(ContinueWithSave.No);
				form.OKButton.PerformClick();
				moqForm.Protected().Verify("OnClosing", Times.Never(), ItExpr.IsAny<System.ComponentModel.CancelEventArgs>());
				AssertEquals("DialogResult", DialogResult.None, form.DialogResult);

				moqForm.Invocations.Clear();
				moqForm.Setup(m => m.FireSaveButton(null)).Returns(ContinueWithSave.Yes);
				moqForm.Protected().Setup("OnClosing", ItExpr.IsAny<System.ComponentModel.CancelEventArgs>());
				form.OKButton.PerformClick();

				AssertEquals("DialogResult", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCloseButton()
		{
			var moqForm = new Mock<ScheduleDatePickerForm>(new object[] { Factory.NewWithValidTestData<ReportScheduleTask>() }) { CallBase = true };
			using (ScheduleDatePickerForm form = moqForm.Object)
			{
				form.Show();
				form.BusinessEntity.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
				moqForm.Protected().Setup("OnClosing", ItExpr.IsAny<System.ComponentModel.CancelEventArgs>());
				form.CloseButton.PerformClick();
				AssertEquals("DialogResult", DialogResult.Cancel, form.DialogResult);
				AssertEquals("BusinessEntity.IsInDatabase", false, ((ReportScheduleTask)form.LastDataSourceForTest).IsInDatabase);
				AssertEquals("BusinessEntity.S5_NextScheduledPrintRunTimeUtc", ZDateTime.Empty, ((ReportScheduleTask)form.LastDataSourceForTest).S5_NextScheduledPrintRunTimeUtc);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new ScheduleDatePickerForm(Factory.NewWithValidTestData<ReportScheduleTask>());
		}
	}
}

