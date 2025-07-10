using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Windows.UI;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	[SuppressBindingMemberBashingTest] // to suppress consoleTextBox
	public partial class DocumentDbMergerForm : ZChildForm
	{
		public DocumentDbMergerForm(DocumentDbMerger businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
			InitializeDbMerger(businessEntity);

			progressBar.AllowOverlap(startButton);
			startButton.AllowOverlap(closeButton);
		}

		#region Initialization

		void InitializeDbMerger(DocumentDbMerger businessEntity)
		{
			merger = businessEntity;
			merger.OnProcessStopped += DbMerger_OnProcessStopped;
			merger.OnProcessFailed += DbMerger_OnProcessStopped;
			merger.OnNoMergeRequired += DbMerger_OnNoMergeRequired;
			merger.OnRefreshMergeProgress += DbMerger_OnRefreshMergeInfo;
			merger.OnShowMessage += ShowMessage;
			merger.Initialise(this);

			timer.Start();
		}

		void Timer_Tick(object sender, EventArgs e)
		{
			merger.GetDbMergeInfoCollectionAndRefreshUserFeedback();
		}

		DocumentDbMerger merger;

		const int StatusPanelDbSizeColumnWidthUnscaled = 100;
		const int MaxColumnsCount = 100;

		protected void RefreshStatusTable(DbMergeInfoCollection dbInfoCollection)
		{
			if (statusTableLayoutPanel != null)
			{
				statusTableLayoutPanel.SuspendLayout();
				barChartLabel.Text = Res.GetString("DocumentDbMergerForm|DocManagerDBMaxSize", "{0} DocManager database sizes - Max Size {1} MB (approx.)", dbInfoCollection.MainDbName, merger.MaxDbSizeMb);

				var dbsCount = dbInfoCollection.Count;
				var totalColumnsCount = (dbsCount > MaxColumnsCount ? MaxColumnsCount : dbsCount) + 1;
				var blocksCount = (dbsCount + MaxColumnsCount - 1) / MaxColumnsCount; //Same with (int)Math.Ceiling((double)dbsCount / MaxColumnsCount);
				var totalRowsCount = blocksCount * 2; //One block contains two rows: size label and legend label
				statusTableLayoutPanel.Size = ControlDpiScalingHelper.NewScaledSize(GetTableWidth(totalColumnsCount), GetTableHeight(blocksCount), false);

				if (statusTableLayoutPanel.ColumnCount <= totalColumnsCount)
				{
					statusTableLayoutPanel.ColumnCount = totalColumnsCount;

					while (statusTableLayoutPanel.ColumnCount > statusTableLayoutPanel.ColumnStyles.Count)
					{
						statusTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ControlDpiScalingHelper.ScaleToCurrentDpiX(StatusPanelDbSizeColumnWidthUnscaled)));
					}
				}

				if (statusTableLayoutPanel.RowCount <= totalRowsCount)
				{
					statusTableLayoutPanel.RowCount = totalRowsCount;
					while (statusTableLayoutPanel.RowCount > statusTableLayoutPanel.RowStyles.Count)
					{
						statusTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
						statusTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
					}
				}

				var maxHeight = (int)((zPanelChart.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(24)) * 0.8);
				var maxSize = dbsCount > 0 ? dbInfoCollection.GetMaximumUsedSize() : 0;

				AddUnitLabel(0, maxHeight);

				var columnIndex = 1;
				var rowIndex = 0;

				foreach (var dbInfo in dbInfoCollection)
				{
					if (columnIndex > MaxColumnsCount)
					{
						rowIndex += 2;
						columnIndex = 1;
						AddUnitLabel(rowIndex, maxHeight);
					}

					AddDbSizeAndLegendLabel(columnIndex, rowIndex, dbInfo, maxSize, maxHeight);
					++columnIndex;
				}

				RemoveRedundantLabels(dbsCount, blocksCount);

				statusTableLayoutPanel.ColumnCount = totalColumnsCount;
				statusTableLayoutPanel.RowCount = totalRowsCount;
				statusTableLayoutPanel.ResumeLayout();
			}
		}

		void AddUnitLabel(int rowIndex, int maxHeight)
		{
			var unitLabel = (KLabel)statusTableLayoutPanel.GetControlFromPosition(0, rowIndex);
			if (unitLabel == null)
			{
				unitLabel = new KLabel();
				unitLabel.Text = "(MB)";
				unitLabel.TextAlign = ContentAlignment.MiddleCenter;
				unitLabel.ForeColor = Color.Black;
				unitLabel.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(maxHeight);
				statusTableLayoutPanel.Controls.Add(unitLabel, 0, rowIndex);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "The 'bar chart' needs to be scaled to the current merge progress %.")]
		[SuppressMessage("CargoWiseOne", "CW1042", Justification = "The 'bar chart' needs to be scaled to the current merge progress %.")]
		void AddDbSizeAndLegendLabel(int columnIndex, int rowIndex, DbMergeInfo dbInfo, int maxSize, int maxHeight)
		{
			var dbSizeLabel = (KLabel)statusTableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex);
			if (dbSizeLabel == null)
			{
				dbSizeLabel = new KLabel
				{
					TextAlign = ContentAlignment.TopCenter,
					Padding = ControlDpiScalingHelper.NewScaledPadding(0, 2, 0, 0),
					ForeColor = Color.White,
					MinimumSize = ControlDpiScalingHelper.NewScaledSize(80, 25),
				};

				statusTableLayoutPanel.Controls.Add(dbSizeLabel, columnIndex, rowIndex);
			}

			dbSizeLabel.Text = dbInfo.Size.ToString();
			dbSizeLabel.Height = dbInfo.Size * maxHeight / maxSize - 10;
			dbSizeLabel.Margin = new Padding(5, maxHeight - dbSizeLabel.Height, 5, 0);
			dbSizeLabel.BackColor = dbInfo.Size == 0 ? Color.LightGray : Color.DarkBlue;

			var legendLabel = (KLabel)statusTableLayoutPanel.GetControlFromPosition(columnIndex, rowIndex + 1);
			if (legendLabel == null)
			{
				legendLabel = new KLabel
				{
					TextAlign = ContentAlignment.TopCenter
				};
				statusTableLayoutPanel.Controls.Add(legendLabel, columnIndex, rowIndex + 1);
			}

			legendLabel.Text = $"SD{dbInfo.Number:000}";

			if (dbInfo.IsReadOnly)
			{
				legendLabel.Text += "*";
			}
		}

		void RemoveRedundantLabels(int dbsCount, int totalRows)
		{
			var realRowCount = statusTableLayoutPanel.RowCount / 2;
			for (var rowCount = realRowCount; rowCount >= totalRows; --rowCount)
			{
				var rowIndex = (rowCount - 1) * 2;
				var columnIndex = dbsCount - (rowCount - 1) * MaxColumnsCount;
				RemoveSizeAndLegendLabels(rowIndex, columnIndex);
				RemoveRedundantRowStyles(totalRows);
			}
		}

		void RemoveSizeAndLegendLabels(int rowIndex, int columnIndex)
		{
			for (var index = statusTableLayoutPanel.ColumnCount - 1; index > columnIndex; --index)
			{
				var dbSizeLabel = statusTableLayoutPanel.GetControlFromPosition(index, rowIndex);
				if (dbSizeLabel != null)
				{
					statusTableLayoutPanel.Controls.Remove(dbSizeLabel);
				}

				var legendLabel = statusTableLayoutPanel.GetControlFromPosition(index, rowIndex + 1);
				if (legendLabel != null)
				{
					statusTableLayoutPanel.Controls.Remove(legendLabel);
				}

				if (rowIndex == 0)
				{
					statusTableLayoutPanel.ColumnStyles.RemoveAt(index);
				}

				if (index == 1)
				{
					var unitLabel = statusTableLayoutPanel.GetControlFromPosition(0, rowIndex);
					if (unitLabel != null)
					{
						statusTableLayoutPanel.Controls.Remove(unitLabel);
					}
				}
			}
		}

		void RemoveRedundantRowStyles(int totalRows)
		{
			for (var index = statusTableLayoutPanel.RowStyles.Count - 1; index >= totalRows * 2; --index)
			{
				statusTableLayoutPanel.RowStyles.RemoveAt(index);
			}
		}

		[return: DpiState(DpiState.ScaleX)]
		int GetTableWidth(int columnCount) => columnCount * ControlDpiScalingHelper.ScaleToCurrentDpiX(StatusPanelDbSizeColumnWidthUnscaled);

		[return: DpiState(DpiState.ScaleX)]
		int GetTableHeight(int rowCount) => rowCount * (zPanelChart.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(24));

		#endregion

		#region Form Events

		void StartButton_Click(object sender, EventArgs e)
		{
			if (startButton.Enabled)
			{
				startButton.Enabled = false;
				closeButton.Enabled = false;
				stopButton.Enabled = true;
				var dbMergerThread = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						merger.Run();
					}
				})
				{
					IsBackground = true
				};
				dbMergerThread.Start();
			}
		}

		void stopButton_Click(object sender, EventArgs e)
		{
			if (stopButton.Enabled)
			{
				Stop();
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			if (closeButton.Enabled)
			{
				Close();
			}
		}

		void DocumentDbMergerForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!closeButton.Enabled)
			{
				e.Cancel = true;
				if (stopButton.Enabled)
				{
					Stop();
				}
			}
		}

		void Stop()
		{
			ShowMessage(Res.GetString("bd3f1750-3820-4f08-80ee-d51fa0ac39ae", "Please wait until process loop will be completed."));
			stopButton.Enabled = false;
			merger.Stop();
		}

		#endregion

		#region DbMerger Events

		void DbMerger_OnProcessStopped(string message)
		{
			ShowMessage(message);
			startButton.Enabled = true;
			stopButton.Enabled = false;
			closeButton.Enabled = true;
		}

		void DbMerger_OnRefreshMergeInfo(DbMergeInfoCollection dbInfoCollection, int percent)
		{
			RefreshStatusTable(dbInfoCollection);
			progressBar.Value = percent;
			progressInPercentLabel.Text = percent + "%";
		}

		void DbMerger_OnNoMergeRequired(string message)
		{
			ShowMessage(message);
			startButton.Enabled = false;
			stopButton.Enabled = false;
			closeButton.Enabled = true;
		}

		#endregion

		#region Form Methods

		public override string FormVerb
		{
			get { return ""; }
		}

		protected void ShowMessage(string message)
		{
			consoleTextBox.AppendText(message + "\r\n");
		}

		#endregion

	}
}
