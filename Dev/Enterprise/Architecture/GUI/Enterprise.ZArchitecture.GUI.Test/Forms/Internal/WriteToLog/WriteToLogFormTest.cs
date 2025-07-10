using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(WriteToLogForm))]
	class WriteToLogFormTest : ZFormBasherTest
	{
		public void TestCloseButton()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				form.FormClosed += new FormClosedEventHandler(form_FormClosed);

				try
				{
					AssertEquals("Precondition: Form should not be closed yet.", false, isFormClosed);
					form.CloseButton.PerformClick();
					AssertEquals("Form should be closed now.", true, isFormClosed);
				}
				finally
				{
					form.FormClosed -= new FormClosedEventHandler(form_FormClosed);
				}
			}
		}

		#region Write To Log

		public void TestWriteToLogFailed()
		{
			TestWriteToLog(false, "This record has been modified since the form was last loaded. Please close the form and re-open it to view the latest version of this record.");
		}

		public void TestWriteToLogSucceeded()
		{
			TestWriteToLog(true, null);
		}

		void TestWriteToLog(bool writeToLogResult, string expectedMessage)
		{
			using (var form = GetFormToBash())
			{
				Factory.Save();
				form.Show();
				form.FormClosed += new FormClosedEventHandler(form_FormClosed);

				AssertEquals("Precondition: logger.WriteToLog() should not be called yet.", false, Logger.isLoggingCalled);
				AssertEquals("Precondition: Form should not be closed yet.", false, isFormClosed);
				AssertNull("Precondition: There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);

				try
				{
					Logger.loggingResult = writeToLogResult;
					form.WriteToLogButton.PerformClick();
					AssertEquals("Logger.isLoggingCalled", true, Logger.isLoggingCalled);
					AssertEquals("LastMessage.Text", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Form should be closed.", true, isFormClosed);
					AssertEquals("LoggingSucceeded", writeToLogResult, form.LoggingSucceeded);
				}
				finally
				{
					form.FormClosed -= new FormClosedEventHandler(form_FormClosed);
				}
			}
		}

		public void TestWriteToLogWithObjectHasChanges()
		{
			var topLevelBusinessObject = Factory.New<DummyEnterpriseBusinessObject>();
			topLevelBusinessObject.IsTopLevel = true;

			var revisedBusinessObject = Factory.New<DummyEnterpriseBusinessObject>();
			var logger = new BusinessObjectLogger(topLevelBusinessObject, revisedBusinessObject);

			using (var form = new WriteToLogForm(logger, "Dummy"))
			{
				Factory.Save();
				form.Show();
				form.FormClosed += new FormClosedEventHandler(form_FormClosed);

				AssertEquals("Precondition: logger.WriteToLog() should not be called yet.", false, Logger.isLoggingCalled);
				AssertEquals("Precondition: Form should not be closed yet.", false, isFormClosed);
				AssertNull("Precondition: There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);

				revisedBusinessObject.Z0_Description = "Change it!";

				try
				{
					form.WriteToLogButton.PerformClick();
					AssertEquals("LastMessage.Text", "The revisedBusinessObject has changes. Writing to Log aborted.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Form should be closed.", true, isFormClosed);
				}
				finally
				{
					form.FormClosed -= new FormClosedEventHandler(form_FormClosed);
				}
			}

			isFormClosed = false;
			UnitTestUserNotification.Instance.ClearMessages();

			using (var form = new WriteToLogForm(logger, "Dummy"))
			{
				Factory.Save();
				form.Show();
				form.FormClosed += new FormClosedEventHandler(form_FormClosed);

				AssertEquals("Precondition: logger.WriteToLog() should not be called yet.", false, Logger.isLoggingCalled);
				AssertEquals("Precondition: Form should not be closed yet.", false, isFormClosed);
				AssertNull("Precondition: There should not be any messages.", UnitTestUserNotification.Instance.LastMessage.Text);

				topLevelBusinessObject.Z0_Description = "Change it!";

				try
				{
					form.WriteToLogButton.PerformClick();
					AssertEquals("LastMessage.Text", "The topLevelBusinessObject has changes. Writing to Log aborted.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Form should be closed.", true, isFormClosed);
				}
				finally
				{
					form.FormClosed -= new FormClosedEventHandler(form_FormClosed);
				}
			}
		}

		public void TestWhenThereIsAnError()
		{
			using (var form = GetFormToBash())
			{
				Factory.Save();
				form.Show();
				form.FormClosed += new FormClosedEventHandler(form_FormClosed);

				try
				{
					Logger.Reference = "~";
					AssertHasErrors(Logger.ReferenceInfo);

					form.WriteToLogButton.PerformClick();
					AssertEquals("Logger.isLoggingCalled", false, Logger.isLoggingCalled);
					AssertEquals("Form should not be closed.", false, isFormClosed);
					AssertEquals("Log should not proceed", false, form.LoggingSucceeded);
				}
				finally
				{
					form.FormClosed -= new FormClosedEventHandler(form_FormClosed);
				}
			}
		}

		#endregion

		#region Implementation

		protected new WriteToLogForm GetFormToBash()
		{
			return (WriteToLogForm)base.GetFormToBash();
		}

		protected override Form GetFormToBashCore()
		{
			return new WriteToLogForm(Logger, Res.GetString("b9949400-b599-415d-b9ac-d181a78dbb98", "Dummy"));
		}

		void form_FormClosed(object sender, FormClosedEventArgs e)
		{
			isFormClosed = true;
		}

		DummyBusinessObjectLogger Logger
		{
			get
			{
				if (logger == null)
				{
					revisedBusinessObject = Factory.New<DummyEnterpriseBusinessObject>();
					revisedBusinessObject.IsTopLevel = true;
					logger = new DummyBusinessObjectLogger(revisedBusinessObject);
				}
				return logger;
			}
		}

		bool isFormClosed;
		DummyBusinessObjectLogger logger;
		DummyEnterpriseBusinessObject revisedBusinessObject;

		#region class DummyBusinessObjectLogger

		class DummyBusinessObjectLogger : BusinessObjectLogger
		{
			public DummyBusinessObjectLogger(IStmALogParent revisedBusinessObject)
				: base(revisedBusinessObject)
			{
			}

			protected override bool WriteToLogCore()
			{
				isLoggingCalled = true;
				return loggingResult;
			}

			public bool loggingResult;
			public bool isLoggingCalled;
		}

		#endregion

		#endregion
	}
}
