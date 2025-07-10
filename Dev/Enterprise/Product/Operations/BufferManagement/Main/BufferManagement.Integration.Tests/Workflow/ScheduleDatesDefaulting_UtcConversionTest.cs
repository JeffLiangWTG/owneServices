using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Integration.Tests
{
	class ScheduleDatesDefaulting_UtcConversionTest : BMSTestCaseWithFactory
	{
		[TestDate(2019, 1, 1)]
		public void TestGetDateTimeOffsetFromParent_ForShipmentTaskDates_TakenFromConsolETA_AndETD_ShouldUseDepartureConsolLoadingPort_AndArrivalConsolDischargePortLocationForUtcConversion()
		{
			var branch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_RL_NKHomePort = "USCHI"; // Chicago (UTC-6)
			const int chicagoBranchUtcOffset = -6;

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "That's it.");
			var templateTask = BMSTestHelper.CreateTask(template, templateWorkflow);

			templateWorkflow.FH_EarliestStartDateDefaultsFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolLoadingETD;
			templateWorkflow.FH_AgreedDeliveryDateDefaultsFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolDischargeETA;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var currentBranchHomePort = ((GlbBranch)Env.CurrentBranch).HomePort;
				AssertEquals("USCHI", currentBranchHomePort.RL_Code);

				var expectedCurrentBranchLocalTime = ZDateTime.UtcNow.AddHours(chicagoBranchUtcOffset);
				AssertEquals("Pre-condition: it's currently 6 hours before 1st January 2019 in Chicago", new ZDateTime(2018, 12, 31, 18, 0, 0), expectedCurrentBranchLocalTime);
				AssertEquals("Pre-condition: it's currently 6 hours before 1st January 2019 in Chicago", expectedCurrentBranchLocalTime, currentBranchHomePort.LocationDateTime);
				AssertEquals("Pre-condition: it just turned 1st January 2019 in UTC time", new ZDateTime(2019, 1, 1), ZDateTime.UtcNow);

				var shipment = FreightTestHelper.CreateForwardingShipment(Factory, "AUPER", "AUBNE");
				var trigger = shipment.WorkflowItems.Triggers.AddNew();

				const int perthUtcOffset = 8;
				const int brisbaneUtcOffset = 10;
				var consolLoadingETDLocal = new ZDateTime(2019, 1, 1, 10, 0, 0); // 10am, departure port time (Perth, UTC+8)
				var consolLoadingETDUtc = consolLoadingETDLocal.AddHours(-perthUtcOffset);
				AssertEquals(new ZDateTime(2019, 1, 1, 2, 0, 0), consolLoadingETDUtc);

				var consolDischargeETALocal = new ZDateTime(2019, 1, 1, 17, 0, 0); // 5pm, arrival port time (Brisbane, UTC+10)
				var consolDischargeETAUtc = consolDischargeETALocal.AddHours(-brisbaneUtcOffset);
				AssertEquals(new ZDateTime(2019, 1, 1, 7, 0, 0), consolDischargeETAUtc);

				FreightTestHelper.SetConsolLoadingETD(shipment, consolLoadingETDLocal);
				FreightTestHelper.SetConsolDischargeETA(shipment, consolDischargeETALocal);

				Factory.Save();

				var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(((IWorkflowProvider)shipment).WorkflowType);
				var etdValue = workflowDescriptor.GetDateTimeFromParent(trigger, ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolLoadingETD, ZDateTime.Empty);
				var etaValue = workflowDescriptor.GetDateTimeFromParent(trigger, ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolDischargeETA, ZDateTime.Empty);

				AssertCorrectOffsetValue(etdValue, consolLoadingETDLocal, consolLoadingETDUtc); // based on port of departure (Perth)
				AssertCorrectOffsetValue(etaValue, consolDischargeETALocal, consolDischargeETAUtc); // based on port of arrival (Brisbane)

				var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);
				var workflow = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "That's it.");

				AssertEquals("The DNSB time is stored in UTC, which is the definitive source of when exactly the event happens, without having to look up the timezone through some complex way.", consolLoadingETDUtc, workflow.FH_DoNotStartBeforeDate);
				var expectedDoNotStartBeforeDateLocal = consolLoadingETDUtc.AddHours(chicagoBranchUtcOffset); // based on current branch's port (Chicago)
				AssertEquals(new ZDateTime(2018, 12, 31, 20, 0, 0), expectedDoNotStartBeforeDateLocal);
				AssertEquals("The DNSB time is shown in local time of the logged-in branch's timezone, which is UTC-6 hours", expectedDoNotStartBeforeDateLocal, workflow.DoNotStartBeforeDateLocal);

				AssertEquals("The ADD is stored in UTC, which is the definitive source of when exactly the event happens, without having to look up the timezone through some complex way.", consolDischargeETAUtc, workflow.FH_AgreedDeliveryDate);
				var expectedAgreedDeliveryDateLocal = consolDischargeETAUtc.AddHours(chicagoBranchUtcOffset); // based on current branch's port (Chicago)
				AssertEquals(new ZDateTime(2019, 1, 1, 1, 0, 0), expectedAgreedDeliveryDateLocal);
				AssertEquals("The ADD is shown in local time of the logged-in branch's timezone, which is UTC-6 hours", expectedAgreedDeliveryDateLocal, workflow.AgreedDeliveryDateLocal);
			}
		}

		[TestDate(2019, 1, 1)]
		public void TestGetDateTimeOffsetFromParent_ForShipmentTaskDates_TakenFromShipmentETA_AndETD_ShouldUseShipmentOriginPort_AndShipmentDestinationPortLocationForUtcConversion()
		{
			var branch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_RL_NKHomePort = "USCHI"; // Chicago (UTC-6)
			const int chicagoBranchUtcOffset = -6;

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "That's it.");
			var templateTask = BMSTestHelper.CreateTask(template, templateWorkflow);

			templateWorkflow.FH_EarliestStartDateDefaultsFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD;
			templateWorkflow.FH_AgreedDeliveryDateDefaultsFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var currentBranchHomePort = ((GlbBranch)Env.CurrentBranch).HomePort;
				AssertEquals("USCHI", currentBranchHomePort.RL_Code);

				var expectedCurrentBranchLocalTime = ZDateTime.UtcNow.AddHours(chicagoBranchUtcOffset);
				AssertEquals("Pre-condition: it's currently 6 hours before 1st January 2019 in Chicago", new ZDateTime(2018, 12, 31, 18, 0, 0), expectedCurrentBranchLocalTime);
				AssertEquals("Pre-condition: it's currently 6 hours before 1st January 2019 in Chicago", expectedCurrentBranchLocalTime, currentBranchHomePort.LocationDateTime);
				AssertEquals("Pre-condition: it just turned 1st January 2019 in UTC time", new ZDateTime(2019, 1, 1), ZDateTime.UtcNow);

				var shipment = FreightTestHelper.CreateForwardingShipment(Factory, departurePort: "AUPER", arrivalPort: "AUBNE", departureConsolLoadingPort: "FRPAR", arrivalConsolDischargePort: "ITROM"); // consols will have some random ports
				var trigger = shipment.WorkflowItems.Triggers.AddNew();

				const int perthUtcOffset = 8;
				const int brisbaneUtcOffset = 10;
				var consolLoadingETDLocal = new ZDateTime(2019, 1, 1, 10, 0, 0); // 10am, departure port time (Perth, UTC+8)
				var consolLoadingETDUtc = consolLoadingETDLocal.AddHours(-perthUtcOffset);
				AssertEquals(new ZDateTime(2019, 1, 1, 2, 0, 0), consolLoadingETDUtc);

				var consolDischargeETALocal = new ZDateTime(2019, 1, 1, 17, 0, 0); // 5pm, arrival port time (Brisbane, UTC+10)
				var consolDischargeETAUtc = consolDischargeETALocal.AddHours(-brisbaneUtcOffset);
				AssertEquals(new ZDateTime(2019, 1, 1, 7, 0, 0), consolDischargeETAUtc);

				FreightTestHelper.SetShipmentLoadingETD(shipment, consolLoadingETDLocal);
				FreightTestHelper.SetShipmentDischargeETA(shipment, consolDischargeETALocal);

				Factory.Save();

				var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(((IWorkflowProvider)shipment).WorkflowType);
				var etdValue = workflowDescriptor.GetDateTimeFromParent(trigger, ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD, ZDateTime.Empty);
				var etaValue = workflowDescriptor.GetDateTimeFromParent(trigger, ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA, ZDateTime.Empty);

				AssertCorrectOffsetValue(etdValue, consolLoadingETDLocal, consolLoadingETDUtc); // based on port of departure (Perth)
				AssertCorrectOffsetValue(etaValue, consolDischargeETALocal, consolDischargeETAUtc); // based on port of arrival (Brisbane)

				var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);
				var workflow = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "That's it.");

				AssertEquals("The DNSB time is stored in UTC, which is the definitive source of when exactly the event happens, without having to look up the timezone through some complex way.", consolLoadingETDUtc, workflow.FH_DoNotStartBeforeDate);
				var expectedDoNotStartBeforeDateLocal = consolLoadingETDUtc.AddHours(chicagoBranchUtcOffset); // based on current branch's port (Chicago)
				AssertEquals(new ZDateTime(2018, 12, 31, 20, 0, 0), expectedDoNotStartBeforeDateLocal);
				AssertEquals("The DNSB time is shown in local time of the logged-in branch's timezone, which is UTC-6 hours", expectedDoNotStartBeforeDateLocal, workflow.DoNotStartBeforeDateLocal);

				AssertEquals("The ADD is stored in UTC, which is the definitive source of when exactly the event happens, without having to look up the timezone through some complex way.", consolDischargeETAUtc, workflow.FH_AgreedDeliveryDate);
				var expectedAgreedDeliveryDateLocal = consolDischargeETAUtc.AddHours(chicagoBranchUtcOffset); // based on current branch's port (Chicago)
				AssertEquals(new ZDateTime(2019, 1, 1, 1, 0, 0), expectedAgreedDeliveryDateLocal);
				AssertEquals("The ADD is shown in local time of the logged-in branch's timezone, which is UTC-6 hours", expectedAgreedDeliveryDateLocal, workflow.AgreedDeliveryDateLocal);
			}
		}

		[TestDate(2019, 1, 1)]
		public void TestGetDateTimeOffsetFromParent_ForShipmentTaskDates_TakenFromShipmentETA_AndETD_ShouldUseShipmentOriginPort_AndShipmentDestinationPortLocationForUtcConversion_ForStandaloneShipment()
		{
			var branch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_RL_NKHomePort = "USCHI"; // Chicago (UTC-6)
			const int chicagoBranchUtcOffset = -6;

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "That's it.");
			var templateTask = BMSTestHelper.CreateTask(template, templateWorkflow);

			templateWorkflow.FH_EarliestStartDateDefaultsFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD;
			templateWorkflow.FH_AgreedDeliveryDateDefaultsFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var currentBranchHomePort = ((GlbBranch)Env.CurrentBranch).HomePort;
				AssertEquals("USCHI", currentBranchHomePort.RL_Code);

				var expectedCurrentBranchLocalTime = ZDateTime.UtcNow.AddHours(chicagoBranchUtcOffset);
				AssertEquals("Pre-condition: it's currently 6 hours before 1st January 2019 in Chicago", new ZDateTime(2018, 12, 31, 18, 0, 0), expectedCurrentBranchLocalTime);
				AssertEquals("Pre-condition: it's currently 6 hours before 1st January 2019 in Chicago", expectedCurrentBranchLocalTime, currentBranchHomePort.LocationDateTime);
				AssertEquals("Pre-condition: it just turned 1st January 2019 in UTC time", new ZDateTime(2019, 1, 1), ZDateTime.UtcNow);

				var shipment = FreightTestHelper.CreateStandaloneForwardingShipmentWithoutConsols(Factory, "AUPER", "AUBNE");
				Assert("Pre-condition", !shipment.Consols.Any());
				var trigger = shipment.WorkflowItems.Triggers.AddNew();

				const int perthUtcOffset = 8;
				const int brisbaneUtcOffset = 10;
				var consolLoadingETDLocal = new ZDateTime(2019, 1, 1, 10, 0, 0); // 10am, departure port time (Perth, UTC+8)
				var consolLoadingETDUtc = consolLoadingETDLocal.AddHours(-perthUtcOffset);
				AssertEquals(new ZDateTime(2019, 1, 1, 2, 0, 0), consolLoadingETDUtc);

				var consolDischargeETALocal = new ZDateTime(2019, 1, 1, 17, 0, 0); // 5pm, arrival port time (Brisbane, UTC+10)
				var consolDischargeETAUtc = consolDischargeETALocal.AddHours(-brisbaneUtcOffset);
				AssertEquals(new ZDateTime(2019, 1, 1, 7, 0, 0), consolDischargeETAUtc);

				FreightTestHelper.SetShipmentLoadingETD(shipment, consolLoadingETDLocal);
				FreightTestHelper.SetShipmentDischargeETA(shipment, consolDischargeETALocal);

				Factory.Save();

				var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(((IWorkflowProvider)shipment).WorkflowType);
				var etdValue = workflowDescriptor.GetDateTimeFromParent(trigger, ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD, ZDateTime.Empty);
				var etaValue = workflowDescriptor.GetDateTimeFromParent(trigger, ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA, ZDateTime.Empty);

				AssertCorrectOffsetValue(etdValue, consolLoadingETDLocal, consolLoadingETDUtc); // based on port of departure (Perth)
				AssertCorrectOffsetValue(etaValue, consolDischargeETALocal, consolDischargeETAUtc); // based on port of arrival (Brisbane)

				var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(shipment, Factory);
				var workflow = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "That's it.");

				AssertEquals("The DNSB time is stored in UTC, which is the definitive source of when exactly the event happens, without having to look up the timezone through some complex way.", consolLoadingETDUtc, workflow.FH_DoNotStartBeforeDate);
				var expectedDoNotStartBeforeDateLocal = consolLoadingETDUtc.AddHours(chicagoBranchUtcOffset); // based on current branch's port (Chicago)
				AssertEquals(new ZDateTime(2018, 12, 31, 20, 0, 0), expectedDoNotStartBeforeDateLocal);
				AssertEquals("The DNSB time is shown in local time of the logged-in branch's timezone, which is UTC-6 hours", expectedDoNotStartBeforeDateLocal, workflow.DoNotStartBeforeDateLocal);

				AssertEquals("The ADD is stored in UTC, which is the definitive source of when exactly the event happens, without having to look up the timezone through some complex way.", consolDischargeETAUtc, workflow.FH_AgreedDeliveryDate);
				var expectedAgreedDeliveryDateLocal = consolDischargeETAUtc.AddHours(chicagoBranchUtcOffset); // based on current branch's port (Chicago)
				AssertEquals(new ZDateTime(2019, 1, 1, 1, 0, 0), expectedAgreedDeliveryDateLocal);
				AssertEquals("The ADD is shown in local time of the logged-in branch's timezone, which is UTC-6 hours", expectedAgreedDeliveryDateLocal, workflow.AgreedDeliveryDateLocal);
			}
		}

		void AssertCorrectOffsetValue(ZDateTimeOffset actualOffset, ZDateTime expectedLocalTime, ZDateTime expectedUtcTime)
		{
			AssertEquals(expectedLocalTime, actualOffset.ToZDateTime());
			AssertEquals(expectedUtcTime, actualOffset.ToUtcZDateTime());
		}
	}
}
