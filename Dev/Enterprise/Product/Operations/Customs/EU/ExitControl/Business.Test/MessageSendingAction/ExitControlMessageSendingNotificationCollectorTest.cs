using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	sealed class ExitControlMessageSendingNotificationCollectorTest : TestCaseWithFactory
	{
		public void TestIncludeNotificationsFromObject()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var exitConsignment1 = exitHeader.CusExitConsignments.AddNew();
			var exitConsignment2 = exitHeader.CusExitConsignments.AddNew();
			var exitConsignmentLine1 = exitConsignment1.CusExitConsignmentItems.AddNew();
			exitConsignmentLine1.CCI_LineNumber = -1;
			var exitConsignmentLine2 = exitConsignment2.CusExitConsignmentItems.AddNew();
			var package = exitConsignmentLine2.CusExitConsignmentPackagePivots.AddNew();
			package.Package.CXP_PackageType = "XY";
			var container1 = exitConsignmentLine1.CusExitConsignmentContainerPivots.AddNew();
			container1.Container.CXN_ContainerNumber = string.Empty;
			var container2 = exitConsignmentLine2.CusExitConsignmentContainerPivots.AddNew();
			container2.Container.CXN_ContainerNumber = "CONT";
			var exitReport1 = exitHeader.CusExitReports.AddNew();
			exitReport1.CER_CXC_Consignment = exitConsignment1.PK;
			exitReport1.CER_DateTime = ZDateTimeOffset.Now;
			exitReport1.CER_OfficeOfExit = "DE001";
			var exitReport2 = exitHeader.CusExitReports.AddNew();
			exitReport2.CER_CXC_Consignment = exitConsignment2.PK;
			exitReport2.CER_DateTime = ZDateTimeOffset.Now;
			var exitReportLine1 = exitReport1.CusExitReportItems.AddNew();
			exitReportLine1.ERI_CCI_ConsignmentItem = exitConsignmentLine1.PK;
			var exitReportLine2 = exitReport1.CusExitReportItems.AddNew();
			exitReportLine2.ERI_CCI_ConsignmentItem = exitConsignmentLine1.PK;
			exitReportLine2.ERI_CXP_Package = package.PK;

			var parent = new ExitControlMessageSendingObjectParent(exitHeader);
			parent.SendingObjectsCollection.Cast<ExitControlMessageSendingObject>().ForEach(x => x.ShouldSend = true);
			exitHeader.LoadChildEditableObjects();
			exitHeader.RunPreSaveValidation();

			var collector = new ExitControlMessageSendingNotificationCollector(exitHeader, new CusExitReport[] { exitReport1, exitReport2 });
			var messageErrors = collector.GetMessageErrors().GetUniqueMessageList().Concat(collector.GetErrors().GetUniqueMessageList());

			CombineAssertions("Both reports", () =>
			{
				AssertCollectionContains("Header Carrier", "Carrier: You have not entered a Carrier.", messageErrors);
				AssertCollectionContains("Report 1 office of exit", "Office of Exit: The code you have selected is not in the list.", messageErrors);
				AssertCollectionContains("Report 2 office of exit", "Office of Exit: You have not entered an Office of Exit.", messageErrors);
				AssertCollectionContains("Consignment Item 1 office of exit", "Item Number: Item Number cannot be negative.", messageErrors);
				AssertCollectionContains("Consignment Item 2 Package type", "Pack Type: The code you have selected is not in the list.", messageErrors);
				AssertCollectionContains("Consignment Item 1 Container", "Number: You have not entered a Number.", messageErrors);
			});

			collector = new ExitControlMessageSendingNotificationCollector(exitHeader, new CusExitReport[] { exitReport1 });
			messageErrors = collector.GetMessageErrors().GetUniqueMessageList();

			CombineAssertions("Report 1", () =>
			{
				AssertCollectionContains("Header Carrier", "Carrier: You have not entered a Carrier.", messageErrors);
				AssertCollectionContains("Report 1 office of exit", "Office of Exit: The code you have selected is not in the list.", messageErrors);
				AssertCollectionNotContains("Report 2 office of exit", "Office of Exit: You have not entered an Office of Exit.", messageErrors);
				AssertCollectionNotContains("Consignment Item 1 Package type", "Pack Type: The code you have selected is not in the list.", messageErrors);
				AssertCollectionContains("Consignment Item 1 Container", "Number: You have not entered a Number.", messageErrors);
			});

			collector = new ExitControlMessageSendingNotificationCollector(exitHeader, new CusExitReport[] { exitReport2 });
			messageErrors = collector.GetMessageErrors().GetUniqueMessageList();

			CombineAssertions("Report 2", () =>
			{
				AssertCollectionContains("Header Carrier", "Carrier: You have not entered a Carrier.", messageErrors);
				AssertCollectionNotContains("Report 1 office of exit", "Office of Exit: The code you have selected is not in the list.", messageErrors);
				AssertCollectionContains("Report 2 office of exit", "Office of Exit: You have not entered an Office of Exit.", messageErrors);
				AssertCollectionNotContains("Consignment Item 1 office of exit", "Item Number: Item Number cannot be negative.", messageErrors);
				AssertCollectionContains("Consignment Item 1 Package type", "Pack Type: The code you have selected is not in the list.", messageErrors);
				AssertCollectionNotContains("Consignment Item 1 Container", "Number: You have not entered a Number.", messageErrors);
			});
		}
	}
}
