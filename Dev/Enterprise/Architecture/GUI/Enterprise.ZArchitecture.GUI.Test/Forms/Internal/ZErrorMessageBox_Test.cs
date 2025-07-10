using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class ZErrorMessageBox_Test : TestCaseWithFactory
	{
		public void TestDetailsTextWithColumnNames()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (dummy.SuspendValidationTesting())
			{
				dummy.Z0_CodeInfo.AddError("error1");
				dummy.Z0_CodeInfo.AddError("error2");
				dummy.Z0_CodeInfo.AddWarning("warning");
			}
			using (var msgBox = new ZErrorMessageBox(dummy))
			{
				AssertEquals("Contains errors", true, msgBox.DetailsTextWithColumnNames.Contains("error1"));
				AssertEquals("Contains errors", true, msgBox.DetailsTextWithColumnNames.Contains("error2"));
				AssertEquals("Contains warnings", false, msgBox.DetailsTextWithColumnNames.Contains("warning"));
			}
		}

		public void TestDetailsTextWithColumnNames_WhenBusinessLayerThrowsException()
		{
			var mock = new Mock<DummyBusinessObject>(Factory, new RowFactory(Factory).New(DummyBusinessObject.Schema.TableName)) { CallBase = true };
			var bizObj = mock.Object;
			AssertNotNull("Poke so it is created and cached", bizObj.Z0_CodeInfo);

			mock.SetupGet(o => o.Z0_CodeInfo).Throws(new Exception()).Verifiable();

			using (var msgBox = new ZErrorMessageBox(mock.Object))
			{
				AssertEquals("An error occurred while generating the error details.", msgBox.DetailsTextWithColumnNames);
				AssertNotNull(ErrorReporter.LastExceptionReported);
			}

			mock.Verify();
			ErrorReporter.Clear();
		}

		public void TestSetupIgnoreOption()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (dummy.SuspendValidationTesting())
			{
				dummy.Z0_CodeInfo.AddError("error1");
			}

			using (var messageBox = new ZErrorMessageBox(dummy, includeIgnoreOption: true))
			{
				messageBox.Show();
				Application.DoEvents();

				Assert(messageBox.Button1.Visible);
				AssertEquals(DialogResult.Abort, messageBox.Button1.DialogResult);

				Assert(messageBox.Button2.Visible);
				AssertEquals(DialogResult.Ignore, messageBox.Button2.DialogResult);

				AssertEquals($"There are errors on this {dummy.HumanReadableName}. Do you want to ignore them and proceed with save?", messageBox.Message);
			}
		}
	}
}
