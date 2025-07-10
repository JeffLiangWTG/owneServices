using System;
using System.Windows.Forms;
using Enterprise.Client.JAS.Business.Matching;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.GUI
{
	[TestedType(typeof(ExportPreMatchingDataForm))]
	class ExportPreMatchingDataFormTest : ZFormBasherTest
	{
		public void TestFormHeading()
		{
			AssertEquals("Export Data For Pre-Matching", Form.FormHeading);
		}

		public void TestOKButtonClick()
		{
			Form.OKBoundButton_Click(this, null);
			Assert("Should not allow export", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("Should not allow export", "There are errors that need to be fixed before Pre-Matching data can be exported", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Should not allow export", !Exporter.ExportCalled);
			fForm = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Exporter.NettingCycle = Exporter.NettingCycleList[0].Code;
			Exporter.DeliveryMethod = PreMatchedDataExporterForTest.DirectoryDeliveryMethodCode;
			Exporter.ExportDirectory = Env.TempPath;
			Form.OKBoundButton_Click(this, null);
			Assert("Should allow export", UnitTestUserNotification.Instance.LastMessage.WasNone);
			Assert("Should allow export", Exporter.ExportCalled);
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			return new ExportPreMatchingDataForm(new PreMatchedDataExporter());
		}

		ExportPreMatchingDataFormForTest Form
		{
			get
			{
				if (fForm == null)
				{
					fForm = new ExportPreMatchingDataFormForTest(Exporter);
				}

				return fForm;
			}
		}

		PreMatchedDataExporterForTest Exporter
		{
			get
			{
				if (fExporter == null)
				{
					fExporter = new PreMatchedDataExporterForTest();
				}

				return fExporter;
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (fForm != null)
			{
				Form.Dispose();
			}
		}

		ExportPreMatchingDataFormForTest fForm;
		PreMatchedDataExporterForTest fExporter;

		#region class ExportPreMatchingDataFormForTest
		class ExportPreMatchingDataFormForTest : ExportPreMatchingDataForm
		{
			public ExportPreMatchingDataFormForTest(PreMatchedDataExporter exporter) : base(exporter)
			{
			}

			protected override void fProgressForm_Cancelled(object sender, EventArgs e)
			{
				base.fProgressForm_Cancelled(sender, e);
				ProgressFormCancelButtonClicked = true;
			}

			public bool ProgressFormCancelButtonClicked;
		}

		#endregion
		#region class PreMatchedDataExporterForTest
		internal class PreMatchedDataExporterForTest : PreMatchedDataExporter
		{
			public override void Export()
			{
				ExportCalled = true;
			}

			public override bool HasMinimumRequirements { get { return true; } }

			public bool ExportCalled;
		}
		#endregion
		#endregion
	}
}
