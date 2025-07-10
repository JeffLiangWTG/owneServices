using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GlowDataWizardImportFormTest : TestCase
	{
		public void TestCallFuncOnLoad()
		{
			log.AppendLog(LogType.Error, "error message", 20);
			log.AppendLog(LogType.Info, "info message", 30);
			log.AppendLog(LogType.Warning, "warning message", 60);
			log.AppendLog(LogType.ImportFinished, "import finished", 100);

			ZFormModaliser.ShowDialogWithoutDispose(form);

			Assert(form.progressTextBox.Text.Contains("error message"));
			Assert(form.progressTextBox.Text.Contains("info message"));
			Assert(form.progressTextBox.Text.Contains("warning message"));
			Assert(form.progressTextBox.Text.Contains("import finished"));
		}

		public void TestCallFuncOnLoad_ServiceException()
		{
			log.AppendLog(LogType.Error, "some service exception", 20);
			ZFormModaliser.ShowDialogWithoutDispose(form);
			Assert(form.progressTextBox.Text.Contains("some service exception"));
		}

		#if WINZOR
		public void TestWinzorHtml()
		{
			log.AppendLog(LogType.Error, "error message", 20);
			log.AppendLog(LogType.Info, "info message", 30);
			log.AppendLog(LogType.Warning, "warning message", 60);
			log.AppendLog(LogType.ImportFinished, "import finished", 100);

			ZFormModaliser.ShowDialogWithoutDispose(form);

			Assert(form.progressTextBox.Html.Contains("<p><strong><span style=\"font-family: Tahoma, sans-serif; font-size: 8pt; color: #ff0000;\">error message</span></strong></p>"));
			Assert(form.progressTextBox.Html.Contains("<p><span style=\"font-family: Tahoma, sans-serif; font-size: 8pt; color: #0000ff;\">info message</span></p>"));
			Assert(form.progressTextBox.Html.Contains("<p><span style=\"font-family: Tahoma, sans-serif; font-size: 8pt; color: #ffa500;\">warning message</span></p>"));
			Assert(form.progressTextBox.Html.Contains("<p><span style=\"font-family: Tahoma, sans-serif; font-size: 8pt; color: #008000;\">import finished</span></p>"));
		}
		#endif

		GlowDataWizardImportForm form;
		GlowLog log;

		protected override void SetUp()
		{
			base.SetUp();
			ZFormModaliser.ShowDialogsInTest = true;
			log = new GlowLog();
			form = new GlowDataWizardImportForm(log);
		}

		protected override void TearDown()
		{
			base.TearDown();
			form.Dispose();
		}
	}
}
