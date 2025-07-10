using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Integration.Tests
{
	#region Order

	class ScheduleDatesDefaultingTest_ForOrderDate : ScheduleDatesDefaultingTestCase<Order>
	{
		protected override ZString DateTimeSourceType => OrderMilestoneEstimateDefaultedFromList.Codes.OrderDate;

		protected override Order CreateJob()
		{
			return Factory.NewWithValidTestData<Order>();
		}

		protected override void SetDateTimeValue(Order job, ZDateTime dateTime)
		{
			job.JD_OrderDate = dateTime;
		}

		protected override TimeSpan ExpectedUtcOffset => ZDateTimeOffset.Now.Offset; //The utcOffset is taken using P9_GC on the ProcessTask

		protected override string WorkflowType => WorkflowDescriptors.OrderWorkflowDescriptorCode;
	}

	#endregion

	#region Shipment

	class ScheduleDatesDefaultingShipmentTest_ForConsolETA : ScheduleDatesDefaultingShipmentTestCase
	{
		protected override ZString DateTimeSourceType => ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolDischargeETA;

		protected override void SetDateTimeValue(ForwardingShipment job, ZDateTime dateTime)
		{
			FreightTestHelper.SetConsolDischargeETA(job, dateTime);
		}

		protected override TimeSpan ExpectedUtcOffset => TimeSpan.FromHours(-8); // Arriving at LA (UTC-8 hours)
	}

	class ScheduleDatesDefaultingShipmentTest_ForConsolETD : ScheduleDatesDefaultingShipmentTestCase
	{
		protected override ZString DateTimeSourceType => ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolLoadingETD;

		protected override void SetDateTimeValue(ForwardingShipment job, ZDateTime dateTime)
		{
			FreightTestHelper.SetConsolLoadingETD(job, dateTime);
		}

		protected override TimeSpan ExpectedUtcOffset => TimeSpan.FromHours(8); // Departing from Singapore (UTC+8 hours)
	}

	class ScheduleDatesDefaultingShipmentTest_ForShipmentETA : ScheduleDatesDefaultingShipmentTestCase
	{
		protected override ZString DateTimeSourceType => ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA;

		protected override void SetDateTimeValue(ForwardingShipment job, ZDateTime dateTime)
		{
			FreightTestHelper.SetShipmentDischargeETA(job, dateTime);
		}

		protected override TimeSpan ExpectedUtcOffset => TimeSpan.FromHours(-8); // Arriving at LA (UTC-8 hours)
	}

	class ScheduleDatesDefaultingShipmentTest_ForShipmentETD : ScheduleDatesDefaultingShipmentTestCase
	{
		protected override ZString DateTimeSourceType => ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD;

		protected override void SetDateTimeValue(ForwardingShipment job, ZDateTime dateTime)
		{
			FreightTestHelper.SetShipmentLoadingETD(job, dateTime);
		}

		protected override TimeSpan ExpectedUtcOffset => TimeSpan.FromHours(8); // Departing from Singapore (UTC+8 hours)
	}

	abstract class ScheduleDatesDefaultingShipmentTestCase : ScheduleDatesDefaultingTestCase<ForwardingShipment>
	{
		protected override ForwardingShipment CreateJob()
		{
			return FreightTestHelper.CreateForwardingShipment(Factory, "SGSIN", "USLAX"); // from Singapore to LA
		}

		protected override string WorkflowType => WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
	}

	#endregion

	#region Shared Base Class

	[TestDate(2019, 1, 1)]
	abstract class ScheduleDatesDefaultingTestCase<TJobType> : BMSTestCaseWithFactory
		where TJobType : BusinessObject, IWorkflowProvider
	{
		[TestTimeZoneUNLOCO("AUBNE")]
		public void TestDefaultingEarliestStartDate_FromJobField()
		{
			TestDateAttribute.UseUNLOCO = true;
			RunDefaultingDateTest(workflow => workflow.FH_DoNotStartBeforeDateInfo, workflow => workflow.FH_EarliestStartDateDefaultsFromInfo, workflow => workflow.FH_EarliestStartDefaultHoursOffsetInfo);
		}

		[TestTimeZoneUNLOCO("AUBNE")]
		public void TestDefaultingAgreedDeliveryDate_FromJobField()
		{
			TestDateAttribute.UseUNLOCO = true;
			RunDefaultingDateTest(workflow => workflow.FH_AgreedDeliveryDateInfo, workflow => workflow.FH_AgreedDeliveryDateDefaultsFromInfo, workflow => workflow.FH_AgreedDeliveryDateDefaultHoursOffsetInfo);
		}

		void RunDefaultingDateTest(Func<ProcessHeader, ZPropertyInfo> dateTimeInfoGetter, Func<ProcessHeader, ZPropertyInfo> dateTimeSourceTypeInfoGetter, Func<ProcessHeader, ZPropertyInfo> dateTimeSourceOffsetInfoGetter)
		{
			dateTimeSourceTypeInfoGetter(templateJobHeader).Value = DateTimeSourceType;
			dateTimeSourceTypeInfoGetter(templateWorkflow1).Value = DateTimeSourceType;
			dateTimeSourceTypeInfoGetter(templateWorkflow2).Value = DateTimeSourceType;

			dateTimeSourceOffsetInfoGetter(templateWorkflow1).Value = new ZInt(60).GetDateTimeFromMinutes();
			dateTimeSourceOffsetInfoGetter(templateWorkflow2).Value = new ZInt(-60).GetDateTimeFromMinutes();

			templateTask1.P9_EstimatedDefaultedFrom = DateTimeSourceType;
			templateTask2.P9_EstimatedDefaultedFrom = DateTimeSourceType;

			templateTask1.P9_EstimatedDefaultTimeDelta = new ZInt(60).GetDateTimeFromMinutes();
			templateTask2.P9_EstimatedDefaultTimeDelta = new ZInt(-60).GetDateTimeFromMinutes();

			Factory.Save();

			var job = CreateJob();
			SetDateTimeValue(job, ZDateTime.Empty); // In case the job starts with a default value for the relevant field.

			Factory.Save();

			var jobHeader = (ProcessJobHeader)ProcessJobHeaderProvider.GetForParent(job, Factory);

			AssertNotNull(jobHeader);
			AssertEquals(2, jobHeader.ProcessHeaders.Count);

			var workflow1 = jobHeader.ProcessHeaders.Cast<ProcessHeader>().Single(w => w.FH_CompletionStatement == "Workflow1");
			var workflow2 = jobHeader.ProcessHeaders.Cast<ProcessHeader>().Single(w => w.FH_CompletionStatement == "Workflow2");
			var task1 = workflow1.Tasks.Single();
			var task2 = workflow2.Tasks.Single();

			AssertEquals(DateTimeSourceType, dateTimeSourceTypeInfoGetter(jobHeader).Value);
			AssertEquals(DateTimeSourceType, dateTimeSourceTypeInfoGetter(workflow1).Value);
			AssertEquals(DateTimeSourceType, dateTimeSourceTypeInfoGetter(workflow2).Value);

			var yearGap = ZDateTime.DefaultNegatableDurationEpoch.Year - ZDateTime.DefaultDurationEpoch.Year;
			AssertEquals(ZDateTime.Empty, dateTimeSourceOffsetInfoGetter(jobHeader).Value);
			AssertEquals(new ZInt(60).GetDateTimeFromMinutes().AddYears(yearGap), dateTimeSourceOffsetInfoGetter(workflow1).Value);
			AssertEquals(new ZInt(-60).GetDateTimeFromMinutes().AddYears(yearGap), dateTimeSourceOffsetInfoGetter(workflow2).Value);

			AssertEquals(DateTimeSourceType, task1.P9_EstimatedDefaultedFrom);
			AssertEquals(DateTimeSourceType, task2.P9_EstimatedDefaultedFrom);
			AssertEquals(new ZInt(60).GetDateTimeFromMinutes().AddYears(yearGap), task1.P9_EstimatedDefaultTimeDelta);
			AssertEquals(new ZInt(-60).GetDateTimeFromMinutes().AddYears(yearGap), task2.P9_EstimatedDefaultTimeDelta);

			AssertEquals(ZDateTime.Empty, dateTimeInfoGetter(jobHeader).Value);
			AssertEquals(ZDateTime.Empty, dateTimeInfoGetter(workflow1).Value);
			AssertEquals(ZDateTime.Empty, dateTimeInfoGetter(workflow2).Value);
			AssertEquals(ZDateTime.Empty, task1.P9_ScheduledDate);
			AssertEquals(ZDateTime.Empty, task2.P9_ScheduledDate);

			var utcTime = ZDateTime.UtcNow;
			var localTimeValueToSet = utcTime.Add(ExpectedUtcOffset);
			SetDateTimeValue(job, localTimeValueToSet);

			AssertEquals(ZDateTime.Empty, dateTimeInfoGetter(jobHeader).Value);
			AssertEquals(ZDateTime.Empty, dateTimeInfoGetter(workflow1).Value);
			AssertEquals(ZDateTime.Empty, dateTimeInfoGetter(workflow2).Value);
			AssertEquals(ZDateTime.Empty, task1.P9_ScheduledDate);
			AssertEquals(ZDateTime.Empty, task2.P9_ScheduledDate);

			Factory.Save();

			AssertEquals(utcTime, dateTimeInfoGetter(jobHeader).Value);
			AssertEquals(utcTime.AddHours(1), dateTimeInfoGetter(workflow1).Value);
			AssertEquals(utcTime.AddHours(-1), dateTimeInfoGetter(workflow2).Value);
			AssertEquals(utcTime.ToLocalBranchTime().AddHours(1), task1.P9_ScheduledDateLocalForBinding);
			AssertEquals(utcTime.ToLocalBranchTime().AddHours(-1), task2.P9_ScheduledDateLocalForBinding);
			AssertEquals(utcTime.AddHours(1), task1.P9_ScheduledDateUtc);
			AssertEquals(utcTime.AddHours(-1), task2.P9_ScheduledDateUtc);

			utcTime = utcTime.AddDays(1);
			localTimeValueToSet = utcTime.Add(ExpectedUtcOffset);

			SetDateTimeValue(job, localTimeValueToSet);
			Factory.Save();

			AssertEquals(utcTime, dateTimeInfoGetter(jobHeader).Value);
			AssertEquals(utcTime.AddHours(1), dateTimeInfoGetter(workflow1).Value);
			AssertEquals(utcTime.AddHours(-1), dateTimeInfoGetter(workflow2).Value);
			AssertEquals(utcTime.ToLocalBranchTime().AddHours(1), task1.P9_ScheduledDateLocalForBinding);
			AssertEquals(utcTime.ToLocalBranchTime().AddHours(-1), task2.P9_ScheduledDateLocalForBinding);
			AssertEquals(utcTime.AddHours(1), task1.P9_ScheduledDateUtc);
			AssertEquals(utcTime.AddHours(-1), task2.P9_ScheduledDateUtc);
		}

		protected abstract ZString DateTimeSourceType { get; }
		protected abstract TJobType CreateJob();
		protected abstract void SetDateTimeValue(TJobType job, ZDateTime dateTime);
		protected abstract string WorkflowType { get; }

		protected virtual TimeSpan ExpectedUtcOffset => TimeSpan.Zero;

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowType);

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowType);

			templateJobHeader = template.GetJobHeader();
			templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "Workflow1");
			templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "Workflow2");

			templateTask1 = BMSTestHelper.CreateTask(template, templateWorkflow1, description: "Task1");
			templateTask2 = BMSTestHelper.CreateTask(template, templateWorkflow2, description: "Task1");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "WI00637617 - This is needed to have a valid BMSystem, see http://crikey.wtg.zone/TestResults/ee1d3757-f61e-40ab-9941-fa3c4b90abcc")]
		SchematicTestConfig config;
		ProcessJobHeader templateJobHeader;
		ProcessHeader templateWorkflow1, templateWorkflow2;
		ProcessTask templateTask1, templateTask2;
	}

	#endregion
}
