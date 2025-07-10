using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class SendExitReportTransferMessageProcessor : IProcessor
	{
		public SendExitReportTransferMessageProcessor(BusinessObject parent)
		{
			exitReport = parent as CusExitReport;
		}
		readonly CusExitReport exitReport;

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			var exitHeader = exitReport.Header;
			var consignment = exitReport.Consignment;
			var consignmentItems = consignment.CusExitConsignmentItems;
			using (DisposableEnvironment.ForBranch(exitReport.RegistryBranchPK))
			{
				var jobNumber = exitHeader.CXH_JobReference;
				SetupReportDataAndSelectAllConsignmentItems(consignment, consignmentItems, exitReport);
				if (ValidateBeforeSending(exitHeader, notifications, jobNumber))
				{
					var factory = exitReport.Factory;
					factory.Save();
					var messageSendingObject = new ExitControlMessageSendingObject(exitReport);
					new EXTINFMessageSender(messageSendingObject).Send();
					try
					{
						factory.Save();
						notifications.Add(NotificationType.Information, ZString.Format((NoResString)"'EXTINF' message has been sent to customs for Job:{0}.", jobNumber));
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
						notifications.AddError(ZString.Format((NoResString)"There was a system error while attempting to send 'EXTINF' message for Job:{0}. See below for more information.\r\n{1}\r\n", jobNumber, ex.Message));
					}
				}
			}
		}

		void SetupReportDataAndSelectAllConsignmentItems(CusExitConsignment consignment, ICusExitConsignmentItemCollection<CusExitConsignmentItem> consignmentItems, CusExitReport exitReport)
		{
			using (consignment.CusExitConsignmentItems.SuspendAllowNew())
			using (ReportManager.SetupReportData(consignment, exitReport))
			{
				foreach (var consignmentItem in consignmentItems)
				{
					consignmentItem.CCI_Calc_ShouldReportItem = true;
					foreach (var pivot in consignmentItem.CusExitConsignmentPackagePivots)
					{
						var package = pivot.Package;
						if (package != null)
						{
							package.CXP_Calc_ShouldReportItem = true;
						}
					}
				}
				ReportManager.CreateOrUpdateReport(consignment, exitReport);
			}
		}

		bool ValidateBeforeSending(CusExitHeader exitHeader, INotifications notifications, ZString jobNumber)
		{
			var isValid = true;
			exitHeader.RunPreSaveValidation();
			if (exitHeader.HasErrors)
			{
				notifications.AddError(ZString.Format((NoResString)"System cannot send 'EXTINF' message because of following errors on Job:{0}.\r\n{1}", jobNumber, exitHeader.GetErrors().ToUniqueMessageListString()));
				isValid = false;
			}
			return isValid;
		}
	}
}
