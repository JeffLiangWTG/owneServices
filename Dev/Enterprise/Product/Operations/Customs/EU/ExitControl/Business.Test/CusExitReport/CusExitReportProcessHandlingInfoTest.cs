using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	sealed class CusExitReportProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestPopulateCascadingTargets()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			var exitConsignment = exitHeader.CusExitConsignments.AddNew();
			var exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = exitConsignment.PK;
			var stmALog = exitReport.Logs.AddNew(Events.CustomsEntryStatus);
			Factory.Save();
			AssertEquals(Enumerable.Empty<CascadingLink>(), (exitReport as IProcessHandlingInfoProvider).ProcessHandlingInfo.GetCascadingTargets(stmALog));
		}

		public void TestPopulateParentTriggers_NoLogParent()
		{
			var exitReport = Factory.NewWithValidTestData<CusExitReport>();
			exitReport.CER_CXH_Header = ZGuid.Empty;
			var stmALog = exitReport.Logs.AddNew(Events.CustomsEntryStatus);
			IEnumerable<IBaseTrigger> parentTriggers = null;
			AssertNoExceptionThrown(() =>
			{
				parentTriggers = ((IProcessHandlingInfoProvider)exitReport).ProcessHandlingInfo.GetParentTriggers(stmALog);
			});
			AssertEquals(false, parentTriggers.Any());
		}

		public void TestPopulateParentTriggers_CusExitHeader()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var correctLineTrigger = CreateTriggers(exitHeader);
			var exitReport = exitHeader.CusExitReports.AddNew();
			var stmALog = exitReport.Logs.AddNew(Events.CustomsEntryStatus);
			var parentTriggers = (exitReport as IProcessHandlingInfoProvider).ProcessHandlingInfo.GetParentTriggers(stmALog);
			AssertContainsExactElementsInAnyOrder(new[] { correctLineTrigger }, parentTriggers);
		}

		public void TestPopulateParentTriggers_JobDeclaration()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var declaration = Factory.New<JobDeclaration>();
			exitHeader.Parent = declaration;
			var jobDeclarationCorrectLineTrigger = CreateTriggers(declaration);
			var exitReport = exitHeader.CusExitReports.AddNew();
			var stmALog = exitReport.Logs.AddNew(Events.CustomsEntryStatus);
			var parentTriggers = (exitReport as IProcessHandlingInfoProvider).ProcessHandlingInfo.GetParentTriggers(stmALog);
			AssertContainsExactElementsInAnyOrder(new[] { jobDeclarationCorrectLineTrigger }, parentTriggers);
		}

		public void TestPopulateParentTriggers_ForwardingShipment()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			exitHeader.Parent = shipment;
			var shipmentCorrectLineTrigger = CreateTriggers(shipment);
			var exitReport = exitHeader.CusExitReports.AddNew();
			var stmALog = exitReport.Logs.AddNew(Events.CustomsEntryStatus);
			var parentTriggers = (exitReport as IProcessHandlingInfoProvider).ProcessHandlingInfo.GetParentTriggers(stmALog);
			AssertContainsExactElementsInAnyOrder(new[] { shipmentCorrectLineTrigger }, parentTriggers);
		}

		static ProcessTask CreateTriggers(IWorkflowProvider workflowProvider)
		{
			var correctLineTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			correctLineTrigger.P9_LineTriggerType = TriggerLineTypes.Codes.CusExitReport;
			correctLineTrigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatusCode;
			var wrongLineTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			wrongLineTrigger.P9_LineTriggerType = TriggerLineTypes.Codes.PkgPackage;
			wrongLineTrigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatusCode;
			var noLineTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			noLineTrigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatusCode;
			return correctLineTrigger;
		}
	}
}
