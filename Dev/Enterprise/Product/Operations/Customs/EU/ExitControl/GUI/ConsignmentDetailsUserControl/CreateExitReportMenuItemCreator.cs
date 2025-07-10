using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public class CreateExitReportMenuItemCreator
	{
		public CreateExitReportMenuItemCreator(IConsignmentsGridUserControlProvider provider)
		{
			this.provider = provider;
		}
		readonly IConsignmentsGridUserControlProvider provider;

		public const string CreateExitReportMenuItemName = "CreateExitReportMenuItem";

		public ZMenuItem Create() => new ZMenuItem(ResString.GetMultilingualString("E8B088DD-7014-402E-8A04-6CA9903E195C", "&Create Exit Report"), CreateExitReport_Click) { Name = CreateExitReportMenuItemName };

		void CreateExitReport_Click(object sender, EventArgs e)
		{
			var consignmentsGrid = provider?.UserControl is IConsignmentsGridUserControl userControl ? userControl.ConsignmentsGrid : null;
			var selectedRowCount = consignmentsGrid?.SelectedRowCount ?? 0;
			if (selectedRowCount == 0)
			{
				Globals.Message.ShowError(Res.GetString("{ADBD1B00-0E00-4F66-82B3-7B42D846FC39}", "Please select a consignment first."));
			}
			else if (selectedRowCount == 1)
			{
				CreateExitReport((CusExitConsignment)consignmentsGrid.SelectedElements[0]);
			}
			else
			{
				CreateExitReports(consignmentsGrid.SelectedElements.Cast<CusExitConsignment>());
			}
		}

		protected virtual void CreateExitReport(CusExitConsignment consignment)
		{
			ExitReportCreationSelectionFormManager.ShowExitReportCreationSelectionForm(consignment, null);
		}

		protected virtual void CreateExitReports(IEnumerable<CusExitConsignment> consignments)
		{
			var suitableConsignments = new List<CusExitConsignment>();
			var unsuitableConsignments = new List<CusExitConsignment>();
			foreach (var consignment in consignments)
			{
				if (IsSuitableForBatchExitReportCreation(consignment))
				{
					suitableConsignments.Add(consignment);
				}
				else
				{
					unsuitableConsignments.Add(consignment);
				}
			}

			if (unsuitableConsignments.Count == 0
				|| ShowUnsuitableConsignmentsWarning(unsuitableConsignments) == DialogResult.OK)
			{
				foreach (var consignment in suitableConsignments)
				{
					ReportManager.CreateOrUpdateReport(consignment, null);
				}
			}
		}
		static bool IsSuitableForBatchExitReportCreation(CusExitConsignment consignment)
		{
			return consignment.CusExitConsignmentItems.Count == 0;
		}

		static DialogResult ShowUnsuitableConsignmentsWarning(List<CusExitConsignment> unsuitableConsignments)
		{
			var mrnList = string.Join(", ", unsuitableConsignments.Select(x => x.CXC_MovementReference));
			var message = Res.GetString("30A84CE6-48E0-4E8A-A8C7-1AB2DBC49FF7",
				"For the following consignments a report will not be created due to existing items and/or packing details: {0}", mrnList);
			var caption = Res.GetString("5EA4AC49-CA08-44FF-8E1E-92F1778E4946", "Warning: Creation of Reports");

			return Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
		}
	}
}
