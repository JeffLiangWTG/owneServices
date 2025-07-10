using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.ZMGermany;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ComplianceReport.ZMD
{
	public partial class ZMGermanyForm : ZChildForm
	{
		public ZMGermanyForm()
		{
		}

		public ZMGermanyForm(AccComplianceReport report) : base(new ZMGermanyReport(report))
		{
			ComplianceReport = report;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File description")]
		const string FileDescription = "Zusammenfassende Meldung";
		readonly AccComplianceReport ComplianceReport;
		public ZMGermanyReport ZMReport => (ZMGermanyReport)BusinessEntity;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (!this.IsDesignMode())
			{
				HandleButtonsAvailability(ComplianceReport, "");
			}
		}

		public void HandleButtonsAvailability(object sender, string versionOverrideText)
		{
			if (!IsDisposing)
			{
				var overrideIsNumber = int.TryParse(versionOverrideText, out var versionOverrideNumber);
				var selectedVersion = overrideIsNumber ? versionOverrideNumber : ZMReport.SelectedVersionNumber;
				generateButton.Enabled = (ZMReport.CurrentTaxReturn?.ATR_Status ?? "") == AccTaxReturn.Status.Saved
					&& (ZMReport.CurrentTaxReturn?.ATR_Version ?? 0) == selectedVersion;
				var generatedVersion = ZMReport.TaxReturnHeaderList.Find(v => v.ATR_Status == AccTaxReturn.Status.Generated);
				markAsSubmittedButton.Enabled = (generatedVersion?.ATR_Status ?? "") == AccTaxReturn.Status.Generated
					&& (generatedVersion?.ATR_Version ?? 0) == selectedVersion;
			}
		}

		void AttachFileToEdoc(string filename, string fileContent)
		{
			var docManager = ComplianceReport.DocManagerInfo();
			docManager.SetupEDocsFactoryToBeSavedWithMainFactory(false);
			var newFile = docManager.AddFileOrDocument(System.Text.Encoding.UTF8.GetBytes(fileContent), filename, Core.Constants.RefDocTypes.MiscellaneousDocument, description: FileDescription);
			newFile.IsPublished = true;
			docManager.UseBusinessEntityFactoryAsInternal = true;
		}

#if DEBUG
		public ZButton GenerateButton_ForTestOnly => generateButton;
		public ZButton MarkAsSubmittedButton_ForTestOnly => markAsSubmittedButton;
		public ZGrid ReportLinesGrid_ForTestOnly => reportLinesGrid;
		public ZTextBox RegistrationIDTextBox_ForTestOnly => registrationIDTextBox;
		public ZTextBox SenderIDTextBox_ForTestOnly => senderIDTextBox;
		public ZDropEdit VersionDropEdit_ForTestOnly => versionDropEdit;
#endif

		#region Event Handlers

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		void generateButton_Click(object sender, EventArgs e)
		{
			if (!ZMReport.CanGenerate())
			{
				Globals.Message.Show(Res.GetString("03480EB6-AA2F-4450-B93C-23B6E0C2A66C", "Please submit your last generated file before generating a new version."),
					Res.GetString("460CD73C-2A5B-4156-B772-0296AC2B0302", "Validation error"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			var errormessage = ZMReport.ValidateCurrentTaxReturn();
			if (!errormessage.IsEmpty)
			{
				Globals.Message.Show(errormessage,
					Res.GetString("460CD73C-2A5B-4156-B772-0296AC2B0302", "Validation error"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			var exporter = new ZMFileExporter(ZMReport, ComplianceReport);

			ZMFileExporter.ZMFileExportResult exportResult;

			try
			{
				exportResult = exporter.Export();
			}
			catch (InvalidOperationException ex)
			{
				Globals.Message.ShowError(ex.Message, Res.GetString("877804B9-D704-4D80-82B4-7860F30B9ADC", "Report generation failed"));
				return;
			}

			if (exportResult.NewRecords + exportResult.UpdatedRecords == 0)
			{
				Globals.Message.Show(Res.GetString("FE2B2626-AD10-4276-8354-4D59C5DDF7AF", "Selected version does not contain any records that differ from previous versions."),
					Res.GetString("6167EA38-8C4C-4ECA-87AB-E28AF777E94F", "No records to transmit found."),
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning);
			}
			else
			{
				AttachFileToEdoc(exportResult.Filename, exportResult.FileContent);
				Globals.Message.Show(Res.GetString("1B1ED4EE-A461-446F-B460-1C44C652ED69", "Number of new records: {0}\r\nNumber of updated records: {1}\r\n\r\nFile {2} has been generated and attached to eDocs. Please submit this file to Fiscal Authorities now.", exportResult.NewRecords, exportResult.UpdatedRecords, exportResult.Filename),
					Res.GetString("BE32FE72-DED2-4BDB-805C-D206EE234FC5", "Records successfully exported."),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			ZMReport.MarkSelectedVersionAsGenerated();
			ZMReport.Factory.Save();
			UpdateVersionDropEditDescriptionBox();
		}

		void markAsSubmittedButton_Click(object sender, EventArgs e)
		{
			ZMReport.MarkGeneratedVersionRowAsSubmitted();
			ZMReport.Factory.Save();
			HandleButtonsAvailability(ComplianceReport, "");
			UpdateVersionDropEditDescriptionBox();
		}

		void UpdateVersionDropEditDescriptionBox()
		{
			var list = ZMReport.Version_List;
			versionDropEdit.List = list;
			var selectedVersion = ZMReport.SelectedVersion;
			var statusDescription = list.GetDescriptionFromCode(selectedVersion);
			versionDropEdit.DescriptionBox.Text = statusDescription;
		}

		void closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void versionDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			HandleButtonsAvailability(sender, versionDropEdit.CodeBox.Text);  // use the entered text because when using keyboard, ZMReport.SelectedVersionNumber is not yet set to the new version number
		}

		#endregion
	}
}
