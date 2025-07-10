using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Windows.UI.Design;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Builder.Generator
{
	/// <summary>
	/// This form calls the controller class to control the file generation on a New Database schema setup
	/// </summary>
	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class NewSchemaForm : Form, IProgressLogger
	{
		private protected NewSchemaForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		NewSchemaForm(GeneratorOutputDirectory outputDirectory) : this()
		{
			this.outputDirectory = outputDirectory;
		}

		readonly GeneratorOutputDirectory outputDirectory;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		public static bool OpenSetup(bool isAutoRegen, GeneratorOutputDirectory outputDirectory, RegenActions regenType)
		{
			using (NewSchemaForm setupForm = new NewSchemaForm(new GeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut, outputDirectory.DevSourceDirectory, outputDirectory.CWSharedSourceDirectory)))
			{
				setupForm.fIsAutoRegen = isAutoRegen;
				setupForm.fRegenType = regenType;
				setupForm.ShowDialog();
				return (setupForm.DialogResult == DialogResult.OK);
			}
		}

		bool fIsAutoRegen;
		bool fSuccessful;
		RegenActions fRegenType = RegenActions.FullRegen;

		void DeferredStartSetup()
		{
			Thread controllerThread = new Thread(new ThreadStart(OnControllerThreadStart));
			controllerThread.Start();
		}

		/// <summary>
		/// Note: 
		///   This method runs in a separated thread.
		///   Therefore do not set member properties or call member methods from it.
		/// </summary>
		protected void OnControllerThreadStart()
		{
			using (Db.DisposableActionForDbConnection())
			{
				Controller setupNewSchemaController = ControllerFactory(this);
				bool setupSucceeded = setupNewSchemaController.DoGeneration(fRegenType);
				Invoke(new Action<bool>(FinaliseSetup), new object[] { setupSucceeded });
			}
		}

		void FinaliseSetup(bool setupSucceeded)
		{
			fSuccessful = setupSucceeded;
			CloseButton.Visible = true;
			SaveToClipboardButton.Visible = true;

			if (fSuccessful)
			{
				// If it's Successful and AutoRegen, 
				// automatically closes the form to proceed with check-in
				if (fIsAutoRegen)
				{
					Close();
					return;
				}

				DisplayFinalResult("SETUP COMPLETE", Color.Green);
			}
			else
			{
				DisplayFinalResult("****** ERROR ******", Color.Red);
			}
		}

		/// <summary>
		/// Instantiates a setup Controller, hooks up to its events and returns it
		/// </summary>
#if DEBUG
		internal virtual
#endif
		protected Controller ControllerFactory(IProgressLogger logger) => new Controller(this, outputDirectory);

		void IProgressLogger.AddProgressText(string text) => DoOnUIThread(AddProgressText, text);
		void IProgressLogger.ReportSkippedFile(string text) => DoOnUIThread(ReportSkippedFile, text);
		void IProgressLogger.ShowStatusLine(string text) => DoOnUIThread(ShowStatusLine, text);

		void AddProgressText(string text)
		{
			string[] lines = text.Split('\n');

			for (int i = 0; i < lines.Length; i++)
			{
				ProgressTextBox.AppendText(lines[i].TrimEnd() + System.Environment.NewLine);
			}
		}

		void ReportSkippedFile(string text)
		{
			if (SkippedFilesTextBox.Text.IndexOf(text) < 0)
			{
				SkippedFilesTextBox.AppendText(text + System.Environment.NewLine);
				SkippedFilesTextBox.ScrollToCaret();
				SkippedFilesTextBox.Refresh();
			}
		}

		void ShowStatusLine(string text)
		{
			OutputPanel.Text = text.Trim().Replace(System.Environment.NewLine, " | ");
		}

		void DoOnUIThread<T>(Action<T> action, T argument)
		{
			if (InvokeRequired)
			{
				Invoke(action, argument);
			}
			else
			{
				action(argument);
			}
		}

		void DisplayFinalResult(string message, Color bGColor)
		{
			AddProgressText(System.Environment.NewLine + "\t" + message);
			BackColor = bGColor;
		}

		void NewSchemaForm_Load(object sender, EventArgs e)
		{
			BeginInvoke(new ThreadStart(DeferredStartSetup));
		}

		void NewSchemaForm_Closing(object sender, CancelEventArgs e)
		{
			if (fSuccessful)
			{
				DialogResult = DialogResult.OK;
			}
			else
			{
				DialogResult = DialogResult.Cancel;
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		void SaveToClipboardButton_Click(object sender, EventArgs e)
		{
			if (!SafeClipboard.SetDataObject(ProgressTextBox.Text, true))
			{
				MessageBox.Show("Windows Clipboard is not accessible at the moment. Try to repeat this operation later.");
			}
		}
	}
}
