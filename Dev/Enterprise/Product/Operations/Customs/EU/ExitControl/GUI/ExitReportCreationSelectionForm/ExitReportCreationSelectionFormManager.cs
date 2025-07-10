using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public static class ExitReportCreationSelectionFormManager
	{
		public static void ShowExitReportCreationSelectionForm(CusExitConsignment consignment, CusExitReport report)
		{
			var consignmentItems = consignment.CusExitConsignmentItems;
			var consignmentItemsRequired = consignment.ConsignmentItemsRequiredToCreateCusExitReport;
			if (consignmentItems.Any() || !consignmentItemsRequired)
			{
				if (consignmentItemsRequired && consignmentItems.Any(item => !item.CusExitConsignmentPivots.Any()))
				{
					Globals.Message.ShowInformation(Res.GetString("52E3ECEF-90FF-4117-A1EF-29FEA8E950DC", "All items should have at least one package."));
				}
				else
				{
					using (consignment.CusExitConsignmentItems.SuspendAllowNew())
					using (var form = new ExitReportCreationSelectionForm(consignment, report))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
						{
							ReportManager.CreateOrUpdateReport(consignment, report);
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("1927D188-D488-4226-8C5B-EBB5F95BEEB6", "A consignment should have at least one item."));
			}
		}
	}
}
