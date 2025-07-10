using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.PaymentTimesReportingScheme;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ComplianceReport.PTRS
{
	public partial class ImportABNsForm : ZChildForm
	{
		ZButton CloseButton;
		ZButton ImportFromFileButton;
		ZButton SubmitButton;
		WhiteTextBox ProgressTextBox;

		protected ImportABNsForm()
		{
			MinimumSize = Size;
			InitializeComponent();
		}

		public ImportABNsForm(AccComplianceReport complianceReport) : this()
		{
			ComplianceReport = complianceReport;
		}

		AccComplianceReport ComplianceReport { get; }

		OrgABNImportHelper ImportHelper => importHelper ?? (importHelper = new OrgABNImportHelper(Factory));

		OrgABNImportHelper importHelper;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());

		BusinessObjectFactory factory;

		#region ZForm Overrides

		public override string FormVerb => "";

		#endregion

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			if (!CloseButton.Enabled)
			{
				e.Cancel = true;
			}
		}

		void ImportFromFile_Click(object sender, EventArgs e)
		{
			NotifyGuiHelper.ResetUpdateDuringImportFlags();

			var dialog = new ZOpenFileDialog();
			dialog.Filter = (NoResString)"CSV files (*.csv)|*.csv"; // File Extension Filter
			dialog.RestoreDirectory = true;
			dialog.CheckFileExists = true;
			DialogResult dR = dialog.ShowDialog(this);

			if (dR == DialogResult.OK)
			{
				ImportFromFile(dialog.ForceLocalFile());
			}
		}

		void ImportFromFile(ZString fileName)
		{
			bool fatalErrorOccurred = false;
			LastImportFileName = fileName;

			ProgressTextBox.Clear();
			ProgressTextBox.AppendText(Res.GetString("FD128127-F254-47AF-BED8-0DE21426A851", "Importing data from file [{0}]...", fileName));
			ProgressTextBox.AppendText("\r\n");
			Cursor originalCursor = Cursor;
			try
			{
				SuspendButton();
				if (!fileName.IsEmpty)
				{
					Cursor = Cursors.WaitCursor;
					ImportABNs(fileName);
					SubmitButton.Enabled = true;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Cursor = originalCursor;
				fatalErrorOccurred = true;
				HandleException(ex);
			}
			finally
			{
				Cursor = originalCursor;
				if (fatalErrorOccurred)
				{
					SuspendButton();
				}
				CloseButton.Enabled = true;
			}
		}

		void ImportABNs(string fileName)
		{
#if DEBUG
			ImportFileException_ForTestOnly?.Invoke();
#endif
			IEnumerable<string> listABNs = null;

			using (var progressForm = new ProgressForm())
			{
				progressForm.ShowCancelButton = false;
				progressForm.Status = Res.GetString("DC87D34B-33AF-4AC5-B54E-2AD4C3119FDF", "Importing ABNs...");
				progressForm.ShowModalTo(this);
				progressForm.ShowProgressBar = true;

				listABNs = ReadFileAndReturnABNs(fileName);

				if (listABNs.IsNullOrEmpty())
				{
					ProgressTextBox.AppendText(Res.GetString("37950D80-BDCE-44C7-BA7B-CF4C4BCBB157", "There is no valid ABN in this file."));
				}
				else
				{
					progressForm.SetStatusAndPercentComplete(Res.GetString("FC585968-A96D-4C9F-86A4-01FF97F734B4", "Creating document tracking records..."), 25);
					ProgressTextBox.AppendText("\r\n");
					ProgressTextBox.AppendText(ImportHelper.AnalysisABNList(listABNs, ComplianceReport.ACR_DateTo, ZDateTimeOffset.Today));
				}
			}
		}

		IEnumerable<string> ReadFileAndReturnABNs(string fileName)
		{
			var listABNs = new List<string>();
			using (TextReader dataReader = new StreamReader(fileName))
			{
				int lineNum = 0;
				string line;
				while ((line = dataReader.ReadLine()) != null)
				{
					lineNum++;
					if (line == "ABN")
					{
						continue;
					}

					if (ABNValidation.CheckValidABN(line))
					{
						listABNs.Add(line);
					}
					else
					{
						ProgressTextBox.AppendText(Res.GetString("46D1A14F-6A02-4A2B-AE2F-F709A73DE5C9", "Skip line {0} because it's not a valid ABN: {1}", lineNum, line));
						ProgressTextBox.AppendText("\r\n");
					}
				}
			}
			return listABNs;
		}

		ZString LastImportFileName;

		void SuspendButton()
		{
			this.ImportFromFileButton.Enabled = false;
			this.CloseButton.Enabled = false;
			this.SubmitButton.Enabled = false;
		}

		INotificationSubscriberQueryUserDataImport NotifyGuiHelper => notifyGuiHelper ?? (notifyGuiHelper = (INotificationSubscriberQueryUserDataImport)ObjectFactory.Get<INotificationSubscriberQueryUser>());
		INotificationSubscriberQueryUserDataImport notifyGuiHelper;

		void HandleException(Exception ex)
		{
			var isNonFatalException =
				ex is IOException ||
				ex is UnauthorizedAccessException ||
				ex is NotSupportedException ||
				ex is XmlException ||
				ex is OperationCanceledException;

			if (!isNonFatalException)
			{
				Globals.Message.ShowDeveloperException(ex);
			}

			ProgressTextBox.AppendText(Res.GetString("0064C80A-AA42-48ED-AD2D-E4CA0445EEE3", "Error: {0}", ex.Message));
			ProgressTextBox.AppendText("\r\n");
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SubmitButton_Click(object sender, EventArgs e)
		{
			IDocManagerSupport docManagerSupport = ComplianceReport;
			var eDoc = docManagerSupport.DocManagerInfo.AddFileOrDocument(LastImportFileName, Enterprise.Core.Constants.RefDocTypes.MiscellaneousDocument);
			eDoc.Description = Res.GetString("67094C92-768E-4A6E-B985-A53D862B118E", "SBI Tool ABN Small Bus List");
			((StorageDocsBase)eDoc).SC_IsSystemGenerated = true;
			((StorageDocsBase)eDoc).SC_FileName = $"SBI Tool ABN Small Bus List-{ZDateTime.Now:yyyyMMddHHmmss}";// File Name
			((StorageDocsBase)eDoc).SC_DataType = (NoResString)"csv"; // File Extension

			BusinessObjectFactory.SaveTogether(docManagerSupport.DocManagerInfo.MasterFactory, Factory);

			ZFormModaliser.ShowDialogAndDispose(
				new ZMessageBox(
					Res.GetString("0F784B3C-D1B3-4E8F-A2A8-A29C9F4F8DCC", "Please re-queue Compliance Report to see the effect of imported ABNs"),
					Res.GetString("4E87EFBF-B180-4BF3-9446-789C213A1F93", "ABNs are imported successfully"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Information)
			);
			Close();
		}

		[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
		class WhiteTextBox : TextBox
		{
			public override Color BackColor
			{
				get { return SystemColors.Window; }
				set { }
			}
		}

#if DEBUG
		#region Exposed For Test

		internal Action ImportFileException_ForTestOnly;

		internal void ImportFromFile_ForTestOnly(ZString fileName) => ImportFromFile(fileName);

		internal TextBox ProgressTextBox_ForTestOnly => ProgressTextBox;

		internal ZButton SubmitButton_ForTestOnly => SubmitButton;

		internal ZButton ImportFromFileButton_ForTestOnly => ImportFromFileButton;

		#endregion
#endif
	}
}

