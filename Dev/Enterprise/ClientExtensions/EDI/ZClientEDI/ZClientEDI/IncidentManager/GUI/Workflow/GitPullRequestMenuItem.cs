using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.TfsRest;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Workflow
{
	internal class GitPullRequestMenuItem : ZToolStripMenuItem, ITaskDetailsMenuItem
	{
		public GitPullRequestMenuItem()
			: base(Res.GetData("D3B2FEC2-DD46-4041-ADA6-F5266860076F", "Pull Requests"))
		{
		}

		public GitPullRequestMenuItem(ITaskDetailsControl taskDetailsControl)
			: this()
		{
			AttachToTaskDetailsControl(taskDetailsControl);
		}

		void DropDown_Closed(object sender, ToolStripDropDownClosedEventArgs e)
		{
			DropDown.Items.Clear();
			DropDown.Items.Add("-");
		}

		void DropDown_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var currentItem = TasksGrid.GetCurrent() as WorkItemProcessTask;
			var loginName = currentItem?.AssignedStaffMember?.GS_LoginName ?? ZString.Empty;
			if (string.IsNullOrWhiteSpace(loginName))
			{
				DropDown.Items.Add(GetNoAssignedStaffItem());
				DropDown.Items.RemoveAt(0);
				return;
			}

			var pullRequests = GetPullRequestsByOwnerForAllTeamProjectCollections(loginName).ToArray();
			if (pullRequests.Length == 0)
			{
				DropDown.Items.Add(GetNoPullRequestsItem(currentItem.P9_GS_NKAssignedStaffMember));
				DropDown.Items.RemoveAt(0);
				return;
			}

			var separatorIndex = 0;
			foreach (var pullRequest in pullRequests)
			{
				var item = new ZToolStripMenuItem { Text = pullRequest.title, ToolTipText = pullRequest.description };
				item.Click += PullRequestItem_Click;
				item.Tag = pullRequest;

				var workItemNumber = currentItem.Parent.Number.ToUpperInvariant();
				if (!string.IsNullOrEmpty(workItemNumber) && pullRequest.title.ToUpperInvariant().Contains(workItemNumber))
				{
					DropDown.Items.Insert(separatorIndex++, item);
				}
				else
				{
					DropDown.Items.Add(item);
				}
			}

			if (separatorIndex == 0 || separatorIndex == pullRequests.Length)
			{
				DropDown.Items.RemoveAt(separatorIndex);
			}
		}

		IEnumerable<PullRequestInfo> GetPullRequestsByOwnerForAllTeamProjectCollections(string loginName)
		{
			var result = new ConcurrentBag<PullRequestInfo>();
			Parallel.ForEach(GetAllTeamProjectCollections(), uri =>
			{
				try
				{
					var devOpsContext = GetDevOpsContextMethod(uri);
					var userId = devOpsContext.GetUserId(loginName);
					if (!string.IsNullOrEmpty(userId))
					{
						var pullRequests = devOpsContext.GetPullRequestsByOwner(userId);
						if (pullRequests != null)
						{
							foreach (var pullRequest in pullRequests)
							{
								result.Add(pullRequest);
							}
						}
					}
				}
				catch (WebException)
				{
				}
			});
			return result.OrderBy(p => p.title);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<Uri> GetAllTeamProjectCollections()
		{
			var result = new List<Uri>();
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var command = connection.Command("select TPC_Url from TeamProjectCollections"))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(new Uri((string)reader["TPC_Url"]));
				}
			}
			return result;
		}

		public Func<Uri, IDevOpsRestContext> GetDevOpsContextMethod { get; set; } = uri => new TfsRestContext(uri);

		ToolStripItem GetNoPullRequestsItem(string userCode) => new ZToolStripMenuItem { Text = Res.GetString("ADC7F287-2740-41D7-ABD9-4239A15A4586", "No Pull Requests Found for User {0}", userCode) };
		ToolStripItem GetNoAssignedStaffItem() => new ZToolStripMenuItem { Text = Res.GetString("ADB902C6-2836-48DB-8D0A-4903EE5C3F49", "No Staff is Assigned") };

		bool IsShelfTaskType(string type)
		{
			return type == EDITaskTypes.TaskActiveAspectOnlyBuild
				|| type == EDITaskTypes.TaskActiveCheckin
				|| type == EDITaskTypes.TaskActiveShelfTest
				|| type == EDITaskTypes.TaskActiveUATBuild
				|| type == EDITaskTypes.TaskAspectOnlyBuild
				|| type == EDITaskTypes.TaskCheckin
				|| type == EDITaskTypes.TaskShelfTest
				|| type == EDITaskTypes.TaskUATBuild;
		}

		void PullRequestItem_Click(object sender, EventArgs e)
		{
			var pullRequest = (PullRequestInfo)((ToolStripMenuItem)sender).Tag;
			if (pullRequest == null)
			{
				ErrorReporter.ReportOnce("The .Tag of object sender is null. \n It should not be, as if it was before, it would have blown up in PullRequestsItem_Opening).");
				return;
			}

			var replacement = pullRequest.pullRequestUrl;
			if (TextBox != null)
			{
				TextBox.Focus();
				#if !WINZOR
				TextBox.SelectedText = replacement.ToString();
				#endif
			}
		}

		public void HandleMenuStripInit(object sender, int evt)
		{
			if (sender is ContextMenuStrip contextMenuStrip)
			{
				var templatesItemIndex = evt;
				contextMenuStrip.Items.Insert(templatesItemIndex + 1, this);

				DropDown.Items.Add("-");
				DropDown.Opening += DropDown_Opening;
				DropDown.Closed += DropDown_Closed;
			}
		}

		public void HandleMenuStripOpen(object sender, EventArgs eventArgs)
		{
			if (sender is ContextMenuStrip contextMenuStrip)
			{
				var currentItem = TasksGrid.GetCurrent() as WorkItemProcessTask;
				if (currentItem != null && IsShelfTaskType(currentItem.P9_Type))
				{
					Visible = true;
					Enabled = !TextBox.ReadOnly;
				}
				else
				{
					Visible = false;
				}
			}
		}

		ZRichTextBox TextBox { get; set; }
		ZGrid TasksGrid { get; set; }
		public void AttachToTaskDetailsControl(ITaskDetailsControl taskDetailsControl)
		{
			if (taskDetailsControl != null)
			{
				TextBox = taskDetailsControl.NotesRichTextBox;
				TasksGrid = taskDetailsControl.TasksGrid;

				taskDetailsControl.NotesRichTextBox.OnAddExtraMenuItems += HandleMenuStripInit;
				taskDetailsControl.NotesRichTextBox.ContextMenuManagerContextMenuStripOpening += HandleMenuStripOpen;
			}
		}

		#region IDisposable

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DropDown.Items.Clear();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
