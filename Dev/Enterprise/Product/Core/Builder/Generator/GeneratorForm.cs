using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.BuildTools;
using CargoWise.Data;
using Enterprise.DbUpgrader.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Builder.Generator
{
	public partial class GeneratorForm : Form
	{
		public GeneratorForm(GeneratorOutputDirectory outputDirectory)
		{
			InitializeComponent();

			this.outputDirectory = outputDirectory;
			this.Text = this.Text + " (Server: " + Db.ServerName + ", DB: " + Db.DatabaseName + ")";
		}

		GeneratorOutputDirectory outputDirectory;

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Utilities

		int ErrorCount;

		void AddProgressHeader(string header)
		{
			AddReportLine(header + "...");
		}

		void AddProgressError(string header)
		{
			ErrorCount++;
			AddReportLine("ERROR: " + header);
		}

		void AddReportLine(string line)
		{
			line = "\t" + line.Replace(System.Environment.NewLine, " | ");
			BeginInvoke(new Action(() =>
			{
				ProgressListBox.Items.Insert(0, line);
			}));
		}

		void SetProgressLine(string line)
		{
			BeginInvoke(new Action(() =>
			{
				StatusBarPanel.Text = line;
			}));
		}

		void InitProcess()
		{
			ProgressListBox.Items.Clear();
			BuildPanel.Enabled = false;
			ErrorCount = 0;
		}

		void InitGeneration()
		{
			InitProcess();

			SetStateOfSSButtons(false);
		}

		void FinishProcess()
		{
			BuildPanel.Enabled = true;
			string output = "Process completed.";

			if (ErrorCount > 0)
			{
				output += "   Error Count = " + ErrorCount;
			}

			AddProgressHeader(output);
			SetProgressLine(output);
			ErrorCount = 0;
		}

		#endregion

		#region Actions

		void RunUnitTests()
		{
			ArrayList assemblies = new ArrayList();
			assemblies.Add("Enterprise.Builder.DataUpgradeSetup");
			assemblies.Add("Generator");
			assemblies.Add("BuildTools");
			UnitTestForm generatorUnitTestForm = new UnitTestForm(assemblies, "Generator Unit Tests");
			generatorUnitTestForm.ShowDialog();
		}

		void PerformBuild()
		{
			SetProgressLine("Building");

			SetCancelButton(true);
			InitGeneration();

			Task.Run(() =>
			{
				if (GenerateBusinessObjectsCheckBox.Checked)
				{
					Generate(BusinessObjectGenerator, "Generating Business Objects");
				}

				if (GenerateStmEventConstantsCheckBox.Checked)
				{
					GenerateEventConstants();
				}

				outputDirectory.Save();
			}).ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					AddProgressError(task.Exception.ToString());
				}

				Invoke(new Action(() =>
				{
					FinishProcess();
					SetCancelButton(false);
				}));
			});
		}

		void SetStateOfSSButtons(bool isReadOnly)
		{
			UndoCheckoutButton.Enabled = !isReadOnly;
			CheckInChangesButton.Enabled = !isReadOnly;
		}

		void SetCancelButton(bool isEnabled)
		{
			Cancel_Button.Enabled = Cancel_Button.Visible = isEnabled;
		}

		void Generate(AbstractGenerator generator, string progressHeader)
		{
			AddProgressHeader(progressHeader);
			SetProgressLine(progressHeader);
			generator.Generate();
		}

		void DataUpgrateSaveXmlButton_Click(object sender, EventArgs e)
		{
			SystemDataUpgrade_SaveDbDataToXmlFiles();
		}

		#endregion

		#region Generate Business Objects

		BizObjGenerator BusinessObjectGenerator
		{
			get
			{
				if (fBusinessObjectGenerator == null)
				{
					fBusinessObjectGenerator = new BizObjGenerator(outputDirectory);
					fBusinessObjectGenerator.AddReportLineEvent(new GeneratorEvent(AddReportLine));
					fBusinessObjectGenerator.AddErrorLineEvent(new GeneratorEvent(AddReportLine));
				}

				return fBusinessObjectGenerator;
			}
		}

		BizObjGenerator fBusinessObjectGenerator;

		#endregion

		#region Generate System Data XML Files

		DataGeneratorManager SystemDataGeneratorManager
		{
			get
			{
				if (systemDataGeneratorManager == null)
				{
					systemDataGeneratorManager = new DataGeneratorManager();
					systemDataGeneratorManager.OnTaskStarted += new GeneratorEvent(AddProgressHeader);
					systemDataGeneratorManager.OnSubtaskStarted += new GeneratorEvent(AddReportLine);
					systemDataGeneratorManager.OnTaskFailed += new GeneratorEvent(AddReportLine);
				}
				return systemDataGeneratorManager;
			}
		}

		DataGeneratorManager systemDataGeneratorManager;

		void SystemDataUpgrade_PopulateUpgradeTasksCheckedListBox()
		{
			UpgradeTask[] upgradeTasks = SystemDataGeneratorManager.GetUpgradeTasks();

			foreach (UpgradeTask task in upgradeTasks)
			{
				bool isTaskCheckedOutByMe = SourceControl.EnterpriseDatabase.IsFileCheckedOutByMe(task.ResourceFile.FileFullPath);

				if (isTaskCheckedOutByMe)
				{
					UpgradeTasksCheckedListBox.Items.Insert(0, task);
					UpgradeTasksCheckedListBox.SetItemChecked(0, true);
				}
				else
				{
					UpgradeTasksCheckedListBox.Items.Add(task);
				}
			}

			ToggleSystemDataGenerationButtons(SystemDataGeneratorManager.IsDataVersionFileCheckedOutByMe);
		}

		void SystemDataUpgrade_CopyFilesAndApplyDataToDb()
		{
			UpgradeTask[] upgradeTasks = GetCheckedUpgradeTasks();
			if (upgradeTasks.Length > 0)
			{
				if (SystemDataGeneratorManager.CopyRequiredFilesToLocationAndApplyDataToDb(upgradeTasks))
				{
					ToggleSystemDataGenerationButtons(true);
				}
			}
		}

		void SystemDataUpgrade_SaveDbDataToXmlFiles()
		{
			UpgradeTask[] upgradeTasks = GetCheckedUpgradeTasks();
			if (upgradeTasks.Length > 0)
			{
				SystemDataGeneratorManager.SaveDbDataToXmlFiles(upgradeTasks);
			}
		}

		void ToggleSystemDataGenerationButtons(bool isCheckedOut)
		{
			DataUpgradeCheckOutButton.Enabled = !isCheckedOut;

			DataUpgrateSaveXmlButton.Enabled = isCheckedOut;
			DataUpgradeUndoCheckOutButton.Enabled = isCheckedOut;
			//DataUpgradeCheckInButton.Enabled = IsCheckedOut;
			//UpgradeTasksCheckedListBox.Enabled = !IsCheckedOut;
		}

		UpgradeTask[] GetCheckedUpgradeTasks()
		{
			UpgradeTask[] result = new UpgradeTask[UpgradeTasksCheckedListBox.CheckedItems.Count];
			UpgradeTasksCheckedListBox.CheckedItems.CopyTo(result, 0);
			return result;
		}

		#endregion

		#region Generate Events

		#region Generate DatabaseUtils

		void GenerateEventConstants()
		{
			EventGenerator eventGenerator = new EventGenerator(outputDirectory);
			eventGenerator.OnTaskStarted += new GeneratorEvent(AddProgressHeader);
			eventGenerator.OnSubtaskStarted += new GeneratorEvent(SetProgressLine);
			eventGenerator.OnTaskFailed += new GeneratorEvent(AddReportLine);

			eventGenerator.GenerateStmEventConstants();
		}

		#endregion

		#endregion

		#region EventHandlers

		void GeneratorForm_Load(object sender, EventArgs e)
		{
			SetStateOfSSButtons(true);
		}

		void BuildButton_Click(object sender, EventArgs e)
		{
			if (!GenerateBusinessObjectsCheckBox.Checked || RequireCWShared())
			{
				PerformBuild();
			}
		}

		void CheckInChangesButton_Click(object sender, EventArgs e)
		{
			SetStateOfSSButtons(true);
			AddProgressHeader("CheckIn complete");
		}

		void UndoCheckoutButton_Click(object sender, EventArgs e)
		{
			outputDirectory.UndoCheckout();

			SetStateOfSSButtons(true);
			AddProgressHeader("Changes are undone");
		}

		void FileExitMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		void SaveToClipboardMenuItem_Click(object sender, EventArgs e)
		{
			string clipboardData = "";

			foreach (object aLine in ProgressListBox.Items)
			{
				clipboardData += aLine.ToString() + System.Environment.NewLine;
			}

			if (!SafeClipboard.SetDataObject(clipboardData, true))
			{
				MessageBox.Show("Windows Clipboard is not accessible at the moment. Try to repeat this operation later.");
			}
		}

		void ClearMenuItem_Click(object sender, EventArgs e)
		{
			ProgressListBox.Items.Clear();
		}

		void UnitTestMenuItem_Click(object sender, EventArgs e)
		{
			RunUnitTests();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			throw new Exception("Build Cancelled");
		}

		#region System Data Update Events

		void GeneratorTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (GeneratorTabControl.SelectedTab == SystemDataUpdateTabPage && UpgradeTasksCheckedListBox.Items.Count == 0)
			{
				SystemDataUpgrade_PopulateUpgradeTasksCheckedListBox();
			}
		}

		void DataUpgradeCheckOutButton_Click(object sender, EventArgs e)
		{
			SystemDataUpgrade_CopyFilesAndApplyDataToDb();
		}

		void DataUpgradeUndoCheckOutButton_Click(object sender, EventArgs e)
		{
			outputDirectory.Dispose();
		}

		#endregion

		#endregion

		void button1_Click(object sender, EventArgs e)
		{
			var items = this.UpgradeTasksCheckedListBox.Items;
			for (int i = 0; i < items.Count; i++)
			{
				this.UpgradeTasksCheckedListBox.SetItemChecked(i, true);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		bool RequireCWShared()
		{
			if (string.IsNullOrEmpty(outputDirectory.CWSharedSourceDirectory))
			{
				var response = Globals.Message.QueryUserResponse(new UserResponseArgument()
				{
					Caption = "CargoWise/Shared path is required for this operation.",
					Message = "Please specify the path to your local clone of the CargoWise/Shared repository.",
					Buttons = ZMessageBoxButtons.OKCancel,
					MinimumResponseLength = 3,
					Icon = ZMessageBoxIcon.Question,
				});

				if (string.IsNullOrEmpty(response))
				{
					// cancelled
					return false;
				}

				if (!Directory.Exists(response))
				{
					MessageBox.Show("Specified path is not a valid directory.", "Invalid path.");
					return false;
				}

				var oldOutputDirectory = outputDirectory;
				outputDirectory = outputDirectory.WithCWShared(response);

				if (oldOutputDirectory != outputDirectory)
				{
					oldOutputDirectory.Dispose();
				}

				SourceControl.WithAdditionalRepository(response);
			}

			return true;
		}
	}
}
