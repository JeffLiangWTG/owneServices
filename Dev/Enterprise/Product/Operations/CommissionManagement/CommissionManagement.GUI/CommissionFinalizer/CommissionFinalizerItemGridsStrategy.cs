using System.Windows.Forms;
using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CommissionManagement.GUI
{
	public static class CommissionFinalizerItemGridsStrategy
	{
		public static void AddDetailsGridAdornments(ZGrid detailCommissionGrid)
		{
			AddDetailsGridDoubleClickHandler(detailCommissionGrid);
			AddDetailsGridMenuItems(detailCommissionGrid);
		}

		static void AddDetailsGridDoubleClickHandler(ZGrid detailsCommissionGrid)
		{
			detailsCommissionGrid.MouseDown += (sender, e) =>
			{
				if (e.Clicks == 2 && e.Button == MouseButtons.Left)
				{
					if (detailsCommissionGrid.HitTest(e.X, e.Y).Row > -1)
					{
						var current = detailsCommissionGrid.ListManager.GetCurrent() as CommissionFinalizerLineItemGrouping;
						if (current != null)
						{
							CommissionLineGroupingViewer.ShowViewForm(current);
						}
					}
				}
			};
		}

		static void AddDetailsGridMenuItems(ZGrid detailCommissionGrid)
		{
			var viewMenuItem = GetDetailsGridViewMenuItem(detailCommissionGrid);
			var viewApprovalRequestMenuItem = GetDetailsGridViewApprovalRequestMenuItem(detailCommissionGrid);
			detailCommissionGrid.ContextMenu.MenuItems.InsertRange(0, new[] { viewMenuItem, viewApprovalRequestMenuItem });
		}

		static ZMenuItem GetDetailsGridViewMenuItem(ZGrid detailCommissionGrid)
		{
			return new ZMenuItem(ResString.GetMultilingualString("19925589-d98f-400f-9441-ac9c8ea1be6b", "&View"),
				(sender, e) =>
				{
					var current = detailCommissionGrid.ListManager.GetCurrent() as CommissionFinalizerLineItemGrouping;
					if (current != null)
					{
						CommissionLineGroupingViewer.ShowViewForm(current);
						return;
					}

					Globals.Message.ShowInformation(Res.GetString("20adee24-c5e0-46f2-85c3-f1795aaf8a28", "Please select an entity commission to view."));
				});
		}

		static ZMenuItem GetDetailsGridViewApprovalRequestMenuItem(ZGrid detailCommissionGrid)
		{
			return new ZMenuItem(ResString.GetMultilingualString("cd4de1b0-6d50-4920-862b-d985fb093ee1", "&View Approval Request"),
				(sender, e) =>
				{
					var current = detailCommissionGrid.ListManager.GetCurrent() as CommissionFinalizerLineItemGrouping;
					if (current != null)
					{
						var approvalRequest = current.ApprovalRequest;
						if (approvalRequest != null)
						{
							var controller = ZControllerFactory.Create(ControllerIDs.CommissionApprovalRequest);
							controller.ShowEditForm(current.ApprovalRequest);
						}
						else
						{
							Globals.Message.ShowInformation(Res.GetString("0aeb1826-c0b8-4afc-a90a-dbf40f8fb71f", "Selected entity commission does not have an approval request."));
						}
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("91d50682-42eb-4e78-80a2-91b2d2ef1ea2", "Please select an entity commission to view approval request."));
					}
				});
		}
	}
}
