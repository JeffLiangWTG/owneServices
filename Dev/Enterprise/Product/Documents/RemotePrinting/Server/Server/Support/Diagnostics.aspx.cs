using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.RemotePrinting.Server.Business;
using Enterprise.RemotePrinting.Server.Controls;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.RemotePrinting.Server
{
	public partial class SupportDiagnostics : ZAjaxPage
	{
		protected override void OnPreBind()
		{
			base.OnPreBind();

			SetupPrintQueueGrid();
			SetupSignalRClientGrid();
		}

		protected override void RenderPageSpecificScripts()
		{
			base.RenderPageSpecificScripts();
			RegisterReloadSupportDiagnosticsPageScriptBlock();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			VersioNumberVal.Text = SupportBizo.VersionNumber;
			VersionDate.Text = SupportBizo.VersionDate;
			ReleaseText.Text = SupportBizo.Release;
			LicenseCode.Text = SupportBizo.LicenseCode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected void SetupPrintQueueGrid()
		{
			PrintQueuesDataGrid.Columns.Add(new ZTextEditColumn("Server Name", nameof(StmPrintQueue.SQ_ServerName)));
			PrintQueuesDataGrid.Columns.Add(new ZTextEditColumn("Queue Name", StmPrintQueueSchema.Constants.SQ_QueueName));
			PrintQueuesDataGrid.Columns.Add(new ZTextEditColumn("Allow Printing", StmPrintQueueSchema.Constants.SQ_AllowPrinting));
			PrintQueuesDataGrid.Columns.Add(new ZTextEditColumn("Queued", nameof(StmPrintQueueExt.QueuedPrintJobs)));
			PrintQueuesDataGrid.Columns.Add(new ZTextEditColumn("Failed", nameof(StmPrintQueueExt.FailedPrintJobs)));
			PrintQueuesDataGrid.Columns.Add(new ZTextEditColumn("In Progress", nameof(StmPrintQueueExt.WorkingPrintJobs)));
			PrintQueuesDataGrid.Columns.Add(new ZTextEditColumn("Web Print Server Address", StmPrintQueueSchema.Constants.SQ_WebPrintServiceAddress));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected void SetupSignalRClientGrid()
		{
			SignalRClientGrid.Columns.Add(new ZTextEditColumn("Server Name", SignalRClientBusinessObject.Schema.ServerName));
			SignalRClientGrid.Columns.Add(new ZTextEditColumn("Printers Count", SignalRClientBusinessObject.Schema.PrintersCount));
			SignalRClientGrid.Columns.Add(new ZTextEditColumn("WebPrint Server", SignalRClientBusinessObject.Schema.WebPrintServerAddress));
			SignalRClientGrid.Columns.Add(new ZTextEditColumn("Client Version", SignalRClientBusinessObject.Schema.WebPrintClientVersionNumber));

			SignalRClientGrid.Columns.Add(new RequestLogsColumn("Logs", SignalRClientBusinessObject.Schema.ClientId));
		}

		#region BusinessObject

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return new SupportDiagnosticsInfo();
		}

		protected SupportDiagnosticsInfo SupportBizo => (SupportDiagnosticsInfo)DataSource;

		#endregion

		#region ReloadPageScript

		void RegisterReloadSupportDiagnosticsPageScriptBlock()
		{
			if (!Page.ZClientScript.IsStartupScriptRegistered(GetType(), ReloadSupportDiagnosticsPageScriptBlockKey))
			{
				Page.ZClientScript.RegisterStartupScript(GetType(), ReloadSupportDiagnosticsPageScriptBlockKey, ReloadSupportDiagnosticsPageScriptBlock);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public string ReloadSupportDiagnosticsPageScriptBlock
		{
			get
			{
				// Add TimeStamp to force reload
				return string.Format(@"<SCRIPT type=""text/javascript"">
						function {0}()
						{{
							window.location.href='{1}?TimeStamp=' + Date.now();
						}}
						</SCRIPT>", ReloadSupportDiagnosticsPageScriptFunctionName, ((Global)AppInstance).SupportPage);
			}
		}

		public const string ReloadSupportDiagnosticsPageScriptFunctionName = "Reload_SupportDiagnostics_Page";

		public const string ReloadSupportDiagnosticsPageScriptBlockKey = "ReloadPageScriptBlock";

		#endregion
	}
}
