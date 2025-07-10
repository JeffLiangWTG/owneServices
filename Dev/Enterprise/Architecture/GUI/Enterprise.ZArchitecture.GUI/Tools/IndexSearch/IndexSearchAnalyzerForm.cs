using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.DevTools
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Developer Only Tool")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Developer Only Tool")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
	partial class IndexSearchAnalyzerForm : Form
	{
		readonly GlowIndexQueryParam glowIndexQueryParam;

		const string QueryCompletedText = "Query Completed.";
		const string QueryFailedText = "Query Failed.";
		const string QueryAbortedText = "Query Aborted.";

		internal IndexSearchAnalyzerForm(GlowIndexQueryParam glowIndexQueryParam)
		{
			InitializeComponent();
			this.glowIndexQueryParam = glowIndexQueryParam;
			IndexSearchUriTextBox.Text = GetShowIndexSearchUriText(FormatUriCheckBox.Checked);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Alt | Keys.Enter:
					WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
					return true;

				case Keys.F5:
					if (RunQueryButton.Enabled)
					{
						RunQuery();
					}
					return true;

				default:
					return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		string GetShowIndexSearchUriText(bool isFormatted = false)
		{
			var queryUri = glowIndexQueryParam.GetQueryUri(glowIndexQueryParam.MaxQueryResults);
			if (isFormatted)
			{
				queryUri = ZQueryFormatter.GetFormattedText(queryUri);
			}
			return $"{GlowRegistry.Instance.GlowServiceUri.TrimEnd('/')}/{queryUri}";
		}

		void ClearFormDisplayContent()
		{
			ResultsTextBox.Text = "";
			ResultsTextBox.ForeColor = Color.Black;
			QueryStatusTextBox.Text = "";
			ResetGridTabPage();
		}

		void RunQuery()
		{
			ClearFormDisplayContent();

			RunQueryButton.Enabled = false;
			Cursor = Cursors.WaitCursor;
			QueryStatusTextBox.Text = "Querying from Glow Index...";

			var glowIndexQueryEngine = ObjectFactory.Get<IGlowIndexQueryEngine>();
			var maximumNumberOfModuleFiltersSearchResults = ObjectFactory.Get<IGlowRegistry>().MaximumNumberOfModuleFiltersSearchResults;
			glowIndexQueryParam.MaxQueryResults = maximumNumberOfModuleFiltersSearchResults;
			glowIndexQueryParam.IncludeCount = true;
			glowIndexQueryParam.OnlyIncludePk = false;
			var glowIndexQueryResultCollection = glowIndexQueryEngine.Query(glowIndexQueryParam);
			if (glowIndexQueryResultCollection.Status == GlowIndexQueryStatus.Success)
			{
				QueryStatusTextBox.Text = "Processing Results...";
				ApplyQueryResult(new CommandResult($"{glowIndexQueryResultCollection.Results.Count} record(s) returned", CommandResultType.Success, ConvertIndexSearchResultsToDataSet(glowIndexQueryResultCollection.Results)));
			}
			else
			{
				ApplyQueryResult(new CommandResult(glowIndexQueryResultCollection.ErrorMessage, CommandResultType.Fail, null));
			}
		}

		DataSet ConvertIndexSearchResultsToDataSet(ICollection<GlowIndexQueryResult> results)
		{
			var dataSet = new DataSet();
			var dataTable = new DataTable();
			dataTable.Columns.Add("PK");

			foreach (var result in results)
			{
				var newRow = dataTable.NewRow();
				newRow["PK"] = result.PK;
				foreach (var keyField in result.KeyFields)
				{
					if (!dataTable.Columns.Contains(keyField.Key))
					{
						dataTable.Columns.Add(keyField.Key);
					}
					newRow[keyField.Key] = keyField.Value;
				}
				dataTable.Rows.Add(newRow);
			}
			dataSet.Tables.Add(dataTable);
			return dataSet;
		}

		void ApplyQueryResult(CommandResult commandResult)
		{
			QueryStatusTextBox.Text = commandResult.ResultText;
			ResultsTextBox.Text = commandResult.Message;
			RunQueryButton.Enabled = true;
			Cursor = Cursors.Default;

			if (commandResult.CommandResultType == CommandResultType.Success)
			{
				var glowMaximumNumberRegistry = GlowRegistry.Instance.GlowMaximumNumberOfModuleFiltersSearchResults;
				ResultsTextBox.Text = $"{commandResult.Message}, the max count of records is {glowMaximumNumberRegistry.Value} ({glowMaximumNumberRegistry.HumanReadableRegistryPath()})";
				DisplayTableResults(commandResult.Data);
			}
			else if (commandResult.CommandResultType == CommandResultType.Fail)
			{
				ResultsTextBox.ForeColor = Color.Red;
			}
		}

		internal enum CommandResultType { Success, Fail, Abort }
		internal class CommandResult
		{
			internal CommandResult(string message, CommandResultType resultType, DataSet data)
			{
				Message = message;
				CommandResultType = resultType;
				Data = data;
			}

			internal string Message { get; }
			internal DataSet Data { get; }
			internal CommandResultType CommandResultType { get; }

			internal string ResultText
			{
				get
				{
					switch (CommandResultType)
					{
						case CommandResultType.Success:
							return QueryCompletedText;
						case CommandResultType.Fail:
							return QueryFailedText;
						case CommandResultType.Abort:
							return QueryAbortedText;
						default:
							return string.Format("?? {0} ??", CommandResultType);
					}
				}
			}
		}

		void DisplayTableResults(DataSet dataSet)
		{
			try
			{
				ResultPanel.SuspendLayout();
				ResetGridTabPage();

				var tabIndex = 0;
				var gridDefaultHeight = ResultPanel.Height;
				DataTableGridView dataGridView = null;

				foreach (DataTable table in dataSet?.Tables)
				{
					if (ResultPanel.Controls.Count != 0)
					{
						if (dataGridView != null)
						{
							ResultPanel.Controls.Remove(dataGridView);
						}

						var splitter = CreateNewSplitter();
						ResultPanel.Controls.Add(splitter);
						splitter.Dock = DockStyle.Top;
						splitter.TabIndex = tabIndex++;

						if (dataGridView != null)
						{
							ResultPanel.Controls.Add(dataGridView);
						}
					}

					dataGridView = NewDataTableGridView(gridDefaultHeight);
					dataGridView.InstallDataTableSafe(table, false);
					ResultPanel.Controls.Add(dataGridView);
					dataGridView.SetDateColumnsToSecondsFormat();

					dataGridView.Dock = DockStyle.Top;
					dataGridView.TabIndex = tabIndex++;
					dataGridView.BringToFront();
				}

				if (dataGridView != null)
				{
					dataGridView.Dock = DockStyle.Fill;
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

		void HookGridView(DataTableGridView dataGridView)
		{
			dataGridView.MouseClick += GridViewOnMouseClick;
		}

		void UnHookGridView(DataTableGridView dataGridView)
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
					var gridView = childControl as DataTableGridView;
					UnHookGridView(gridView);
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
				Font = IndexSearchUriTextBox.Font,
				AllowUserToAddRows = false,
				AllowUserToDeleteRows = false,
				ReadOnly = true,
				ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
			};

			for (var i = 0; i < dataGridView.ExtensionMenu.MenuItems.Count; i++)
			{
				if (dataGridView.ExtensionMenu.MenuItems[i].Text == "Copy As Insert SQL")
				{
					dataGridView.ExtensionMenu.MenuItems.RemoveAt(i);
					break;
				}
			}
			HookGridView(dataGridView);

			return dataGridView;
		}

		void RunQueryButton_Click(object sender, EventArgs e)
		{
			RunQuery();
		}

		void OpenUrlButton_Click(object sender, EventArgs e)
		{
			WebUrlLauncher.Launch(GetShowIndexSearchUriText());
		}

		void FormatUriCheckBox_Click(object sender, EventArgs e)
		{
			IndexSearchUriTextBox.Text = GetShowIndexSearchUriText(FormatUriCheckBox.Checked);
		}
	}
}
