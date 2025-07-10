using System;
using System.Windows.Forms;
using CargoWise.BrandManager;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class ExceptionReportingFormTest : TestCase
	{
		public void TestShow()
		{
			using (ExceptionReportingForm form = new ExceptionReportingForm("", false))
			{
				form.Show();
				AssertEquals("Visible", true, form.Visible);
				AssertEquals("WindowState", FormWindowState.Normal, form.WindowState);
			}
		}

		public void TestShowFullMode()
		{
			using (ExceptionReportingForm form = new ExceptionReportingForm("", true))
			{
				form.Show();
				AssertEquals("Visible", true, form.Visible);
				AssertEquals("WindowState", FormWindowState.Maximized, form.WindowState);
			}
		}

		public void TestIsShutDownRequested()
		{
			using (ExceptionReportingForm form = new ExceptionReportingForm("", true))
			{
				AssertEquals("Initially", false, form.IsShutDownRequested);
				form.ShutdownEnterpriseCheckEditForTest.Checked = true;
				AssertEquals("After selecting the CheckEdit", true, form.IsShutDownRequested);
			}
		}

		public void TestSetException()
		{
			using (ExceptionReportingForm form = new ExceptionReportingForm("", true))
			{
				AssertEquals(null, form.ExceptionToReport);
				AssertEquals("", form.ErrorDescriptionTextBoxForTest.Text);
				AssertEquals("", form.FullDetailsTextBoxForTest.Text);

				Exception ex = new ArgumentException("The mice will play");
				const string Message = "When the cat's away";
				const string Key = "The cat's dead...";
				form.SetException(ex, Key, Message);
				AssertEquals(ex, form.ExceptionToReport);
				AssertEquals("Key", Key, form.ExceptionKey);
				AssertEquals(Message, form.ErrorDescriptionTextBoxForTest.Text);
				Assert("Full details textbox should be filled in", form.FullDetailsTextBoxForTest.Text.Length > 0);
			}
		}

		public void TestExceptionsDuringSetExceptionCharZero()
		{
			using (ExceptionReportingForm form = new ExceptionReportingForm("", true))
			{
				string myerror = "Error begin " + (char)0 + " Error end";
				Exception ex = new ArgumentException(myerror);
				const string Key = "The error report has error on its own!";
				AssertNoExceptionThrown(delegate { form.SetException(ex, Key, ""); });
				Assert(form.FullDetailsTextBoxForTest.Text.Contains("Error begin 0x00 [REPLACED \\0] Error end"));
			}
		}

		public void TestExceptionsDuringSetExceptionCharOne()
		{
			using (ExceptionReportingForm form = new ExceptionReportingForm("", true))
			{
				string myerror = "Error begin " + (char)1 + " Error end";
				Exception ex = new ArgumentException(myerror);
				const string Key = "The error report has error on its own!";
				AssertNoExceptionThrown(delegate { form.SetException(ex, Key, ""); });
				Assert(form.FullDetailsTextBoxForTest.Text.Contains("Some thing is wrong when parsing the original error report at line, position:"));
			}
		}

		public void TestErrorReportIdOnFormShouldBeSameAsSentToReport()
		{
			var errorReportId = string.Empty;
			using (var form = new ExceptionReportingForm("Test Error Id", true))
			{
				AssertEquals("Error ID: Test Error Id", form.ErrorIDLabelForTest.Text);
				form.ErrorReportSend += (e) => { errorReportId = e.ErrorReportID; };
				var ex = new ArgumentException("test error");
				var key = "The error report has error on its own!";
				AssertNoExceptionThrown(delegate { form.SetException(ex, key, "test error"); });
				form.SendButton_Click(this, null);
				AssertEquals("Test Error Id", errorReportId);
			}
		}

		public void TestSendButtonIsDisabledAfterErrorReportSent()
		{
			var errorReportId = string.Empty;
			using (var form = new ExceptionReportingForm("Test Error Id", true))
			{
				AssertEquals("Error ID: Test Error Id", form.ErrorIDLabelForTest.Text);
				form.ErrorReportSend += (e) => { errorReportId = e.ErrorReportID; };
				var ex = new ArgumentException("test error");
				var key = "The error report has error on its own!";

				form.SetException(ex, key, "");
				form.SendButton_Click(this, null);
				AssertEquals("", errorReportId);
				AssertEquals(true, form.SendButtonForTest.Enabled);

				form.SetException(ex, key, "test error");
				form.SendButton_Click(this, null);
				AssertEquals("Test Error Id", errorReportId);
				AssertEquals(false, form.SendButtonForTest.Enabled);
			}
		}

		public void TestDontThrowExceptionWhenProductLogoNotFound()
		{
			using (var form = new ExceptionReportingForm("Test Error Id", true))
			{
				//check to see the expected behavour still works as intended
				AssertNotNull(form.ProductLogoPictureBoxForTest.Image);
			}
			var mock = new Mock<IBranding>();
			mock.Setup(m => m.ProductLogo).Throws(new ArgumentException("Value of 'null' is not valid for 'stream'."));
			mock.Setup(m => m.CompanyName).Returns("Fake Company Name");
			mock.Setup(m => m.ProductName).Returns("Fake Product Name");
			using (BrandingFactory.ConfigureTemporary(() => mock.Object))
			{
				AssertNoExceptionThrown(() =>
				{
					using (var form = new ExceptionReportingForm("Test Error Id", true))
					{
						AssertNull(form.ProductLogoPictureBoxForTest.Image);
					}
				});
			}
		}
	}
}
