using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Integration;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace Enterprise.ZArchitecture.DevTools
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	partial class ZQueryAnalyzerForm : Form  // ZQueryAnalyzerForm form is not part of Enterprise.
	{
		#region Form Overrides

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CancelQuery();

				if (components != null)
				{
					components.Dispose();
				}
				GetDbConnectionWithTakeThreadOwnership().Dispose();
			}
			base.Dispose(disposing);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Alt | Keys.Enter:
					WindowState = (WindowState == FormWindowState.Maximized ?
						FormWindowState.Normal : FormWindowState.Maximized);
					return true;

				case Keys.F5:
					RunCommand(SelectedCommand);
					return true;

				case Keys.F5 | Keys.Control:
					CancelQuery();
					return true;

				default:
					return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		#endregion

		public ZQueryAnalyzerForm(string sqlText)
		{
			InitializeComponent();
			EnableControlButtons(true);
			SetSqlText(sqlText);
		}

		#region DPI scaling overrides

		protected override void OnLayout(LayoutEventArgs levent)
		{
			if (VisualStudioDetector.IsVisualStudio)
			{
				this.AutoScaleMode = AutoScaleMode.None;
			}
			else
			{
				this.AutoScaleMode = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleMode;
				this.AutoScaleDimensions = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleDimensions;
			}

			base.OnLayout(levent);
		}

		#endregion
		#region Properties

		void SetSqlText(string value)
		{
			SQLTextBox.Text = value;
			SQLTextBox.SelectionStart = 0;
			SQLTextBox.SelectionLength = 0;
		}

		string SelectedCommand
		{
			get { return (SQLTextBox.SelectedText.Length > 0 ? SQLTextBox.SelectedText : SQLTextBox.Text); }
		}

		#endregion

		void ClearFormDisplayContent()
		{
			ResultsTextBox.Text = "";
			ResultsTextBox.ForeColor = Color.Black;
			SetCommandStatus("");
			ResetGridTabPage();
		}

		#region Commands

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		internal void RunCommand(string commandText)
		{
			ResultsTextBox.Text = "";

			switch (commandText.ToLower().Trim())
			{
				case "begin tran":
				case "begin transaction":
					BeginTransaction();
					break;

				case "commit":
					CommitTransaction();
					break;

				case "rollback":
					RollbackTransaction();
					break;

				default:
					RunCommandInANewTask(commandText);
					break;
			}
		}

		void CancelQuery()
		{
			// The sql query blocks in native code so Thread.Abort will not take
			// effect until the query is finished, which defeats the purpose.
			// We need to call IDbCommand.Cancel() (the cancelDelegate) to stop
			// the query.
			cancelDelegate?.Invoke();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		void BeginTransaction()
		{
			if (pendingTransaction == null)
			{
				pendingTransaction = GetDbConnectionWithTakeThreadOwnership().BeginTransactionWithManager();
				TransactionButton.Text = "Commit";
				SetCommandStatus("Transaction started.");
				RollbackButton.Enabled = true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		void CommitTransaction()
		{
			if (pendingTransaction != null)
			{
				pendingTransaction.CommitTransaction();
				SetCommandStatus("Transaction committed.");
				EndTransactionCleanup();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		void RollbackTransaction()
		{
			SetCommandStatus("Transaction rolled back.");
			EndTransactionCleanup();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		void EndTransactionCleanup()
		{
			TransactionButton.Text = "Begin Tran";
			RollbackButton.Enabled = false;
			if (pendingTransaction != null)
			{
				pendingTransaction.Dispose();
				pendingTransaction = null;
			}
		}

		#endregion

		#region Threading
		// WARNING: the query thread should not interact with any of the controls
		// or non-thread safe instance members on this form, such interactions
		// should be left to the GUI thread. The query thread can get the GUI
		// thread call a method by using the Invoke members.
		//
		// Exception: Both the query thread and the GUI thread need to use the
		// same db connection, otherwise the transaction operations (begin,
		// commit, rollback) will be applied to the wrong connection. This
		// 'shouldnt' cause a problem as the transaction operation buttons are
		// disabled while the query thread is doing it's thing.

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		protected virtual void RunCommandInANewTask(string commandText)
		{
			// this method is run on the GUI thread.

			ClearFormDisplayContent();

			if (commandText == null || commandText.Trim().Length == 0)
			{
				SetCommandStatus("Please input SQL statement...");
				ResultsTextBox.ForeColor = Color.Red;
				ResultsTextBox.Text = "no SQL statement";
				return;
			}

			EnableControlButtons(false);
			this.Cursor = Cursors.WaitCursor;

			SetCommandStatus("Executing Query...");

			RelinquishDbConnectionThreadOwnership();

			_ = Task.Run(() => ExecuteQuery(commandText))
				.ContinueWith((queryResult) => ApplyQueryResult(queryResult.Result));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		CommandResult ExecuteQuery(string commandText)
		{
			// this method is run on the query thread.

			CommandResult result;

			try
			{
				result = ExecuteQueryCore(commandText);
				if (IsHandleCreated)
				{
					BeginInvoke(() => ResultsTextBox.ForeColor = Color.Black);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (IsHandleCreated)
				{
					BeginInvoke(() => ResultsTextBox.ForeColor = Color.Red);
				}

				if (IsSqlAbortedException(ex))
				{
					result = new CommandResult("Query Aborted", CommandResultType.Abort);
				}
				else
				{
					ShowQueryFailedMessage(ex.ToString());
					result = new CommandResult(ex.ToString(), CommandResultType.Fail);
				}
			}

			return result;
		}

		void ShowQueryFailedMessage(string message)
		{
			if (InvokeRequired)
			{
				Invoke(() => ShowQueryFailedMessage(message));
				return;
			}

			Globals.Message.ShowError(message, "Query Failed");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		CommandResult ExecuteQueryCore(string commandText)
		{
			// this method is run on the query thread.

			string message;
			DataSet dataSet;

			using (IDbCommand command = GetDbConnectionWithTakeThreadOwnership().Command(commandText))
			{
				try
				{
					SetCancelDelegate(command.Cancel);

					if (IsCommandNonQuery(commandText))
					{
						dataSet = null;
						message = command.ExecuteNonQuery() + " record(s) affected";
					}
					else
					{
						var dataAdapter = ((DbCommand)command).NewDataAdapter();
						dataSet = new DataSet();
						message = dataAdapter.Fill(dataSet) + " record(s) returned";
					}
				}
				finally
				{
					RelinquishDbConnectionThreadOwnership();
					SetCancelDelegate(null);
				}
			}

			return new CommandResult(message, dataSet);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		protected virtual void ApplyQueryResult(CommandResult result)
		{
			if (InvokeRequired)
			{
				// this is run on the query thread (call ourselvs on the GUI thread).
				Invoke(() => ApplyQueryResult(result));
			}
			else
			{
				// this is run on the GUI thread.
				// if run a UT without calling form.Show(), this is still run on the query thread
				SetCommandStatus("Processing Results...");

				ResultsTextBox.Text = result.Message;
				ResultsTextBox.SelectionStart = 0;

				if (result.Data != null)
				{
					DisplayTableResults(result.Data);
				}

				SetCommandStatus(result.ResultText);
				SQLTextBox.Focus();

				this.Cursor = Cursors.Default;
				EnableControlButtons(true);
			}
		}

		void SetCancelDelegate(MethodInvoker cancelDelegate)
		{
			if (InvokeRequired)
			{
				// this is run on the query thread (call ourselvs on the GUI thread).
				Invoke(new SetCancelDelegateDelegate(SetCancelDelegate), cancelDelegate);
			}
			else
			{
				// this is run on the GUI thread.
				this.cancelDelegate = cancelDelegate;
				this.StopButton.Enabled = (cancelDelegate != null);
			}
		}
		delegate void SetCancelDelegateDelegate(MethodInvoker cancelDelegate);

		static bool IsCommandNonQuery(string query)
		{
			// this method is run on the query thread.
			using (TextReader reader = new StringReader(query))
			{
				var parser = new TSql160Parser(false);
				var tokens = parser.GetTokenStream(reader, out var errors);
				return tokens.Any(x => x.TokenType == TSqlTokenType.Insert || x.TokenType == TSqlTokenType.Delete || x.TokenType == TSqlTokenType.Update);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		static bool IsSqlAbortedException(Exception ex)
		{
			// this method is run on the query thread.

			if (ex is System.Data.Common.DbException sqlEx)
			{
				foreach (var error in new SqlExceptionWrapper(sqlEx).Errors)
				{
					if ((error.Message) == "Operation cancelled by user.")
					{
						return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region Display Query Results

		void DisplayTableResults(DataSet dataSet)
		{
			try
			{
				ResultPanel.SuspendLayout();
				ResetGridTabPage();

				var tabIndex = 0;
				var gridDefaultHeight = ResultPanel.Height / dataSet.Tables.Count;
				DataTableGridView dataGridView = null;

				foreach (DataTable table in dataSet.Tables)
				{
					if (ResultPanel.Controls.Count != 0)
					{
						if (dataGridView != null)
						{
							ResultPanel.Controls.Remove(dataGridView);
						}

						var splitter = CreateNewSplitter();
						ResultPanel.Controls.Add(splitter);
						splitter.Dock = System.Windows.Forms.DockStyle.Top;
						splitter.TabIndex = tabIndex++;

						if (dataGridView != null)
						{
							ResultPanel.Controls.Add(dataGridView);
						}
					}

					dataGridView = NewDataTableGridView(gridDefaultHeight);

					dataGridView.InstallDataTableSafe(table, false);
					dataGridView.SetDateColumnsToSecondsFormat();
					ResultPanel.Controls.Add(dataGridView);

					dataGridView.Dock = System.Windows.Forms.DockStyle.Top;
					dataGridView.TabIndex = tabIndex++;
					dataGridView.BringToFront();
				}

				if (dataGridView != null)
				{
					dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ResultsTextBox.Text += System.Environment.NewLine + ex.Message;
			}
			finally
			{
				ResultPanel.ResumeLayout();
			}
		}

		void HookGridView(Control dataGridView)
		{
			dataGridView.MouseClick += GridViewOnMouseClick;
		}

		void UnHookGridView(Control dataGridView)
		{
			dataGridView.MouseClick -= GridViewOnMouseClick;
		}

		void GridViewOnMouseClick(object sender, MouseEventArgs e)
		{
			if (sender is DataTableGridView gridView && e.Button == MouseButtons.Right)
			{
				gridView.ExtensionMenu.Show(gridView, e.Location);
			}
		}

		void ResetGridTabPage()
		{
			for (var i = ResultPanel.Controls.Count - 1; i >= 0; i--)
			{
				var childControl = ResultPanel.Controls[i];
				ResultPanel.Controls.Remove(childControl);

				if (childControl is DataTableGridView)
				{
					UnHookGridView(childControl);
				}

				childControl.Dispose();
			}
		}

		Splitter CreateNewSplitter()
		{
			var result = new Splitter
			{
				Location = ControlDpiScalingHelper.NewScaledPoint(0, 0),
				Name = "splitter1",
				TabStop = false,
				BackColor = SystemColors.ControlDarkDark,
				Size = ControlDpiScalingHelper.NewScaledSize(6, 6)
			};
			return result;
		}

		DataTableGridView NewDataTableGridView(int height)
		{
			var dataGridView = new DataTableGridView
			{
				Location = ControlDpiScalingHelper.NewScaledPoint(0, 0),
				Size = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(ResultPanel.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(height)),
				Name = "DataTableDataGridView",
				Font = SQLTextBox.Font,
				ReadOnly = true,
				AllowUserToAddRows = false,
				AllowUserToDeleteRows = false,
				ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
			};
			HookGridView(dataGridView);

			return dataGridView;
		}

		#endregion

		#region Control Events

		void RunQueryButton_Click(object sender, EventArgs e)
		{
			RunCommand(SelectedCommand);
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			SQLTextBox.Text = "";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		void TransactionButton_Click(object sender, EventArgs e)
		{
			if (pendingTransaction != null)
			{
				RunCommand("commit");
			}
			else
			{
				RunCommand("begin tran");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		void RollbackButton_Click(object sender, EventArgs e)
		{
			if (pendingTransaction != null)
			{
				RunCommand("rollback");
			}
		}

		void StopButton_Click(object sender, EventArgs e)
		{
			CancelQuery();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "ZQueryAnalyzerForm form is not part of Enterprise")]
		protected override void OnClosing(CancelEventArgs e)
		{
			if (pendingTransaction != null)
			{
				if (MessageBox.Show("A transaction is pending. Click Ok to commit the transaction and close. Click Cancel to keep the transaction and form open", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) == DialogResult.OK)
				{
					CommitTransaction();
				}
				else
				{
					e.Cancel = true;
				}
			}

			base.OnClosing(e);
		}

		void SQLTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case (Keys.Control | Keys.A):
					{
						SQLTextBox.SelectionStart = 0;
						SQLTextBox.SelectionLength = SQLTextBox.Text.Length;
						break;
					}
			}
		}

		#endregion

		#region CommandResult

		protected enum CommandResultType { Success, Fail, Abort }
		protected class CommandResult
		{
			public CommandResult(string message, CommandResultType resultType)
				: this(message, resultType, null)
			{
			}

			public CommandResult(string message, DataSet data)
				: this(message, CommandResultType.Success, data)
			{
			}

			CommandResult(string message, CommandResultType resultType, DataSet data)
			{
				this.message = message;
				this.resultType = resultType;
				this.data = data;
			}

			public string Message
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return message; }
			}

			public CommandResultType ResultType
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return resultType; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
			public string ResultText
			{
				get
				{
					switch (ResultType)
					{
						case CommandResultType.Success:
							return "Query Completed.";
						case CommandResultType.Fail:
							return "Query Failed.";
						case CommandResultType.Abort:
							return "Query Aborted.";
						default:
							return string.Format("?? {0} ??", ResultType);
					}
				}
			}

			public DataSet Data
			{
				[System.Diagnostics.DebuggerStepThrough]
				get { return data; }
			}

			readonly string message;
			readonly CommandResultType resultType;
			readonly DataSet data;
		}

		#endregion

		#region Implementation

		void SetCommandStatus(string status)
		{
			CommandTextBox.Text = status;
		}

		void RelinquishDbConnectionThreadOwnership()
		{
			if (connection.ThreadSentry.IsOwner)
			{
				connection.ThreadSentry.RelinquishThreadOwnership();
			}
		}

		DbConnection GetDbConnectionWithTakeThreadOwnership()
		{
			if (!connection.ThreadSentry.IsOwner)
			{
				connection.ThreadSentry.TakeThreadOwnership();
			}
			return connection;
		}

		void EnableControlButtons(bool enable)
		{
			RunQueryButton.Enabled = enable;
			TransactionButton.Enabled = enable;
			RollbackButton.Enabled = enable && (pendingTransaction != null);
			ClearButton.Enabled = enable;
		}

		ITransactionManager pendingTransaction;
		MethodInvoker cancelDelegate;

		// Db.Connection is thread static, so we need to use the same connection
		// in both the query thread and the GUI thread, otherwise the transaction
		// operations will not work properly.
		readonly DbConnection connection = Db.NewExtraConnectionToMainDb();

		#endregion
	}
}
