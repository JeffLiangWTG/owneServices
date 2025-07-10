using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Scheduler.Testing
{
	[TestsSubclassesOf(typeof(ScheduleForm))]
	internal abstract class ScheduleFormTest<T> : ZFormBasherTest where T : ScheduleForm
	{
		public void TestClearButton()
		{
			Mock<T> moqForm = null;

			if (BusinessEntityType == typeof(AccPeriodSchedule))
			{
				var moqSchedule = new Mock<AccPeriodSchedule> { CallBase = true };
				moqForm = GetNewFormMoq(moqSchedule.Object);
				moqSchedule.Protected().Setup("ClearCore");
				moqSchedule.Setup(m => m.ClearDate());
				moqForm.Protected().Setup("OnClosing", ItExpr.IsAny<System.ComponentModel.CancelEventArgs>());
				using (T form = moqForm.Object)
				{
					form.Show();
					form.ClearButton.PerformClick();
				}

				AssertNoExceptionThrown(moqSchedule.VerifyAll);
				AssertNoExceptionThrown(moqForm.VerifyAll);
			}
			else if (BusinessEntityType == typeof(DateSchedule))
			{
				var moqSchedule = new Mock<DateSchedule> { CallBase = true };
				moqForm = GetNewFormMoq(moqSchedule.Object);
				moqSchedule.Protected().Setup("ClearCore");
				moqSchedule.Setup(m => m.ClearDate());
				moqForm.Protected().Setup("OnClosing", ItExpr.IsAny<System.ComponentModel.CancelEventArgs>());
				using (T form = moqForm.Object)
				{
					form.Show();
					form.ClearButton.PerformClick();
				}

				AssertNoExceptionThrown(moqSchedule.VerifyAll);
				AssertNoExceptionThrown(moqForm.VerifyAll);
			}
			else
			{
				// Given each type needs to be provided explicity, throw an exception if a type comes through which is not accounted for.
				throw new Exception("Schedule Type case not catered for.");
			}
		}

		[RequiresSTA]
		public void TestCloseButton()
		{
			var moqForm = GetNewFormMoq();
			using (T form = moqForm.Object)
			{
				form.Show();
				moqForm.Protected().Setup("OnClosing", ItExpr.IsAny<System.ComponentModel.CancelEventArgs>());
				form.CloseButton.PerformClick();
			}

			AssertNoExceptionThrown(moqForm.VerifyAll);
		}

		public void TestOKButton()
		{
			var schedule = GetNewSchedule();
			using (var form = new ScheduleForm(schedule))
			{
				form.Show();

				form.BusinessEntity.PeriodScope = "x";
				form.BusinessEntity.PeriodCount = 2;
				form.OKButton.PerformClick();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("originalSchedule.PeriodScope", PeriodScopeList.Codes.This, form.OriginalSchedule.PeriodScope);
				AssertEquals("originalSchedule.PeriodNumber", ZByte.Zero, form.OriginalSchedule.PeriodCount);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.BusinessEntity.FillWithValidTestData();
				form.BusinessEntity.PeriodScope = PeriodScopeList.Codes.Next;
				form.BusinessEntity.PeriodCount = 2;
				using (form.BusinessEntity.SuspendValidationTesting())
				{
					form.BusinessEntity.PeriodCountInfo.AddError("x");
				}

				form.OKButton.PerformClick();

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("originalSchedule.PeriodScope", PeriodScopeList.Codes.Next, form.OriginalSchedule.PeriodScope);
				AssertEquals("originalSchedule.PeriodNumber", (ZByte)2, form.OriginalSchedule.PeriodCount);
			}
		}

		protected sealed override Form GetFormToBashCore()
		{
			return (T)Activator.CreateInstance(typeof(T), GetNewSchedule());
		}

		Mock<T> GetNewFormMoq(Schedule schedule = null)
		{
			var scheduleToReturn = schedule;
			if (schedule == null)
			{
				scheduleToReturn = GetNewSchedule();
			}

			return new Mock<T>(new object[] { scheduleToReturn }) { CallBase = true };
		}

		protected virtual Schedule GetNewSchedule()
		{
			Schedule result = (Schedule)Activator.CreateInstance(BusinessEntityType);
			result.ScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			return result;
		}

		protected abstract Type BusinessEntityType { get; }
	}
}
