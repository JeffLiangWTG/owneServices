using System;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Windows.UI.Design;
using SysEnv = System.Environment;

namespace Enterprise.Builder.Generator
{
	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class DbRestoreForm : Form
	{
		public DbRestoreForm()
		{
			InitializeComponent();
		}

		public static bool OpenAndRun()
		{
			DbRestoreForm restoreForm = new DbRestoreForm();
			restoreForm.ShowDialog();
			return (restoreForm.DialogResult == DialogResult.OK);
		}

		protected void DeferredStartSetup()
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
			using (Db.DisableSchemaVersionCheck())
			{
				bool succeeded = false;

				AddProgressText(String.Format("Server: {0} - DB: {1}", Db.ServerName, Db.DatabaseName));

				try
				{
					try
					{
						var restorer = new DatabaseRestorer();
						AddProgressText($"Backup to use: {restorer.DevelopmentDbBackupPath}");
						restorer.Restore();
						succeeded = true;
					}
					catch (SqlException e) // TODO: Use Exception Filtering when Enterprise rolls around to VS2015/C#6
					{
						if (new DbErrorMatch(e).ExceptionType != DbErrorType.LoginFailedForUser)
						{
							throw;
						}

						AddProgressText(SysEnv.NewLine + "Error logging in to SQL Server: " + e.Message + SysEnv.NewLine + SysEnv.NewLine + "Is mixed-mode authentication (SQL Server and Windows authentication) enabled in SQL Server?");
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					AddProgressText(SysEnv.NewLine + e.ToString());
				}

				Invoke(new FinaliseDelegate(Finalise), new object[] { succeeded });
			}
		}

		protected bool fSuccessful;
		protected delegate void FinaliseDelegate(bool succeeded);
		protected void Finalise(bool succeeded)
		{
			fSuccessful = succeeded;
			CloseButton.Enabled = true;
			TopMost = true;
			BringToFront();
			System.Media.SystemSounds.Beep.Play();

			if (fSuccessful)
			{
				DisplayFinalResult("RESTORE COMPLETE", Color.DarkGreen);
			}
			else
			{
				DisplayFinalResult("****** ERROR ******", Color.Red);
			}
		}

		#region GUI Methods

		protected void AddProgressText(string text)
		{
			if (!ProgressTextBox.InvokeRequired)
			{
				ProgressTextBox.AppendText(text.TrimEnd() + System.Environment.NewLine);
			}
			else
			{
				ProgressTextBox.BeginInvoke(new AddProgressTextInvoker(AddProgressText), new object[] { text });
			}
		}

		delegate void AddProgressTextInvoker(string text);

		protected void DisplayFinalResult(string message, Color fontColor)
		{
			AddProgressText(System.Environment.NewLine + System.Environment.NewLine + "\t" + message);
			ProgressTextBox.ForeColor = fontColor;
		}

		#endregion

		#region Event Handlers

		void DbRestoreForm_Load(object sender, EventArgs e)
		{
			this.BeginInvoke(new ThreadStart(DeferredStartSetup));
		}

		void DbRestoreForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (fSuccessful)
			{
				this.DialogResult = DialogResult.OK;
			}
			else
			{
				this.DialogResult = DialogResult.Cancel;
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

	}
}
