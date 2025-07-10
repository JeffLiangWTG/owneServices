using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWiseOne.ResourceStrings;
using Enterprise.AuditDataServices.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.ZAudit.PlugIn
{
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	public partial class ZAuditUserControl : ZUserControl
	{
		public ZAuditUserControl() : base()
		{
			InitializeComponent();
			SetGridProperties();
			this.AfterFirstBinding += new EventHandler(this.ZAuditUserControl_AfterFirstBinding);
			this.EventListDisplayGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(this.zDisplayGridUserTime_ColourDeciding);
			this.zButtonFind.Click += new EventHandler(this.zButtonFind_Click);
		}

		public ZAuditUserControl(ResourceStringData auditUnavailableMessage) : base()
		{
			InitializeDiabledComponent();
			disableMessage.CaptionResourceString = auditUnavailableMessage;
		}

		void SetGridProperties()
		{
			EventListDisplayGrid.ForceShowExportToExcelMenuItem = true;
			EventDetailsDisplayGrid.ForceShowExportToExcelMenuItem = true;
		}

		void DisableControl(ResourceStringData resourceStringData)
		{
			foreach (Control control in this.Controls)
			{
				control.Visible = false;
			}
			disableMessage.CaptionResourceString = resourceStringData;
			disableMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			disableMessage.Dock = System.Windows.Forms.DockStyle.Fill;
			disableMessage.BringToFront();
			this.Controls.Add(disableMessage);
		}

		#region Events

		void zDisplayGridUserTime_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			var auditEvent = (AuditEvent)e.ObjectAtRow;

			if (auditEvent.Operation == (int)Audit.ChangeOperation.Insert)
			{
				e.Colour = System.Drawing.Color.LightCyan;
			}
			else if (auditEvent.Operation == (int)Audit.ChangeOperation.Delete)
			{
				e.Colour = System.Drawing.Color.LightGoldenrodYellow;
			}
		}

		[ThreadStatic]
		static bool? isValidAuditServer;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error message")]
		public static bool IsValidAuditServer
		{
			get
			{
				if (isValidAuditServer == null)
				{
					string errorMessage = GetAuditServerConnectionErrorMessage();
					if (string.IsNullOrEmpty(errorMessage))
					{
						isValidAuditServer = true;
					}
					else
					{
						using (ZMessageBox notification = new ZMessageBox("An error occurred connecting to the audit database. Please contact your system administrator:\n" + errorMessage, "Audit server error", MessageBoxButtons.OK, MessageBoxIcon.Error))
						{
							ZFormModaliser.ShowDialogAndDispose(notification);
						}
						isValidAuditServer = false;
					}
				}
				return isValidAuditServer.Value;
			}
		}

		static string GetAuditServerConnectionErrorMessage()
		{
			string errorMessage;
			try
			{
				errorMessage = BiServiceTaskHelpers.IsAuditEnabled();
			}
			catch (SqlException ex)
			{
				var dbErrorMatch = new DbErrorMatch(ex);
				if (dbErrorMatch.ExceptionType == DbErrorType.ServerDoesNotExist ||
					dbErrorMatch.ExceptionType == DbErrorType.DatabaseDoesNotExist ||
					dbErrorMatch.ExceptionType == DbErrorType.DatabaseOffline)
				{
					errorMessage = ex.Message;
				}
				else
				{
					throw;
				}
			}
			return errorMessage;
		}

		void zButtonFind_Click(object sender, EventArgs e)
		{
			AuditBusinessObject.ValidateFilters();

			if (AuditBusinessObject.HasErrors)
			{
				Globals.Message.ShowInformation(UserControls.Res.GetString("BAC816DF-4F06-49D7-ABD8-55C53E4829CA", "Please correct filter errors."));
			}
			else
			{
				AuditBusinessObject.ReloadAuditEvents();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		void ZAuditUserControl_AfterFirstBinding(object sender, EventArgs e)
		{
			if (IsValidAuditServer)
			{
				if (AuditBusinessObject.IsMasterTableEnabledForCdc())
				{
					AuditBusinessObject.RefreshMinAndMaxAuditDataTime();
					AuditBusinessObject.AuditEventData.Sort(AuditEvent.Schema.TimeUtc, ListSortDirection.Descending);
				}
				else
				{
					DisableControl(new ResourceStringData("C2FD2935-1993-42A2-A06A-0C795AB364D7", "CDC is not enabled on data sources for this form."));
				}
			}
			else
			{
				DisableControl(new ResourceStringData("74A5258B-B4AA-45FD-A889-8E89BB934EAD", "Audit database is not accessible."));
			}
		}

		#endregion

		#region Properties

		Audit AuditBusinessObject
		{
			get { return (Audit)this.DataSource; }
		}

		#endregion
	}
}
