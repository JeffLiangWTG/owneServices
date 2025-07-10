using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.FR.ServiceTasks.Testing
{
	[TestedType(typeof(FRAutomationDeltaServiceTask))]
	sealed class FRAutomationDeltaServiceTaskTest : ServiceTaskTestCase<FRAutomationDeltaServiceTask>
	{
		public void TestPeriod()
		{
			var attr = GetHostedServiceAttributes().FirstOrDefault();
			AssertEquals("15Minutes", attr.MinimumPeriod);
			AssertEquals(true, attr.CanRunInAnyBranch);
			AssertEquals("15Minutes", attr.DefaultScheduleRunEvery);
		}

		public void TestCheckRecipientIDRegistrySetting_HostServiceRequirementIsDefined()
		{
			var methodInfo = typeof(FRAutomationDeltaServiceTask).GetMethod(nameof(FRAutomationDeltaServiceTask.CheckRecipientIDRegistrySetting));
			Assert("[HostedServiceRequirement] is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		public void TestCheckRecipientIDRegistrySetting_ShouldReturnNoErrorMsg_WhenRegistryValueIsSet()
		{
			using (FRCustomsDataRegistry.Instance.RecipientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "EASYLOG2_EAD"))
			{
				var message = FRAutomationDeltaServiceTask.CheckRecipientIDRegistrySetting();
				AssertEquals(message, string.Empty);
			}
		}

		public void TestCheckRecipientIDRegistrySetting_ShouldReturnErrorMsg_WhenRegistryValueIsNotSet()
		{
			using (FRCustomsDataRegistry.Instance.RecipientID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var message = FRAutomationDeltaServiceTask.CheckRecipientIDRegistrySetting();
				AssertEquals($"The registry setting '{FRCustomsDataRegistry.Instance.RecipientID.GetLocationInEnglish()}' has not been configured.", message);
			}
		}

		public void TestRunTask()
		{
			var frCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			frCompany1.GC_Code = "FR1";
			frCompany1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			frCompany1.GC_OH_OrgProxy = org1.PK;
			var branch1 = frCompany1.Branches.AddNew();
			branch1.GB_Code = "AAA";

			FRCustomsDataRegistry.Instance.NbDaysWaitBeforeSendingDeltaDStep2.SetValue(frCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);
			var configuration = new TriggerPointsConfiguration();
			configuration.EnableAutomatedValidation = true;
			FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, configuration);

			var automaticAdvanceDate = new AutomatedModification();
			automaticAdvanceDate.EnableAutomatedModification = false;

			FRCustomsDataRegistry.Instance.FRAutomatedModification.SetValue(frCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, automaticAdvanceDate);

			var frCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			frCompany2.GC_Code = "FR2";
			frCompany2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			frCompany2.GC_OH_OrgProxy = org2.PK;
			var branch2 = frCompany2.Branches.AddNew();
			branch2.GB_Code = "BBB";
			FRCustomsDataRegistry.Instance.NbDaysWaitBeforeSendingDeltaDStep2.SetValue(frCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);

			var automaticAdvanceDate2 = new AutomatedModification();
			automaticAdvanceDate2.EnableAutomatedModification = true;
			automaticAdvanceDate2.TimeByDefault = ZDateTime.Today;

			FRCustomsDataRegistry.Instance.FRAutomatedModification.SetValue(frCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty, automaticAdvanceDate2);

			var frInactiveCompany = Factory.NewWithValidTestData<GlbCompany>();
			frInactiveCompany.GC_Code = "CA3";
			frInactiveCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "INA";
			frInactiveCompany.GC_OH_OrgProxy = org3.PK;
			var branch3 = frInactiveCompany.Branches.AddNew();
			branch3.GB_Code = "III";
			branch3.GB_IsActive = false;
			FRCustomsDataRegistry.Instance.NbDaysWaitBeforeSendingDeltaDStep2.SetValue(frInactiveCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);

			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_Code = "USA";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "CHI";
			usCompany.GC_OH_OrgProxy = org1.PK;
			var branch4 = usCompany.Branches.AddNew();
			branch4.GB_Code = "CHI";
			Factory.Save();

			var logger = new TestServiceLogger();

			var serviceTask = new FRAutomationDeltaServiceTask();
			serviceTask.ServiceLogger = logger;
			InitialiseAndRunTaskSchedule(serviceTask);

			var messages = logger.ToString();
			AssertContains(@"Information|Start to run Automated Delta D2M messages in Company FR1.
Information|No candidate entries in Company FR1.
Information|Finished running Automated Delta D2M messages in Company FR1.", messages);
			AssertContains(@"Information|Automated Delta D2M messages is not enabled in Company EDI.", messages);
			AssertContains(@"Information|Start to run Automated Delta D2M messages in Company FR2.
Information|No candidate entries in Company FR2.
Information|Finished running Automated Delta D2M messages in Company FR2.", messages);
			AssertContains(@"Information|Start to run Automated Delta VAA messages in Company FR1 Branch AAA.
Information|No candidate entries in Company FR1 Branch AAA.
Information|Finished running Automated Delta VAA messages in Company FR1 Branch AAA.", messages);
			AssertContains(@"Information|Automated Delta VAA messages is not enabled in Company EDI Branch SYD.", messages);
			AssertContains(@"Information|Automated Delta VAA messages is not enabled in Company EDI Branch BNE.", messages);
			AssertContains(@"Information|Automated Delta VAA messages is not enabled in Company EDI Branch TES.", messages);
			AssertContains(@"Information|Automated Delta VAA messages is not enabled in Company FR2 Branch BBB.", messages);
			AssertContains(@"Information|Start to run Automated Delta MAP messages in Company FR2 Branch BBB.
Information|No candidate entries in Company FR2 Branch BBB.
Information|Finished running Automated Delta MAP messages in Company FR2 Branch BBB", messages);
			AssertContains(@"Information|Automated Delta MAP messages is not enabled in Company FR1 Branch AAA.", messages);
			AssertContains(@"Information|Automated Delta MAP messages is not enabled in Company EDI Branch SYD.", messages);
			AssertContains(@"Information|Automated Delta MAP messages is not enabled in Company EDI Branch BNE.", messages);
			AssertContains(@"Information|Automated Delta MAP messages is not enabled in Company EDI Branch TES.", messages);
			AssertContains(@"Information|Start to run Automated Delta MDA messages in Company FR2 Branch BBB.
Information|No candidate entries in Company FR2 Branch BBB.
Information|Finished running Automated Delta MDA messages in Company FR2 Branch BBB", messages);
			AssertContains(@"Information|Automated Delta MDA messages is not enabled in Company FR1 Branch AAA.", messages);
			AssertContains(@"Information|Automated Delta MDA messages is not enabled in Company EDI Branch SYD.", messages);
			AssertContains(@"Information|Automated Delta MDA messages is not enabled in Company EDI Branch BNE.", messages);
			AssertContains(@"Information|Automated Delta MDA messages is not enabled in Company EDI Branch TES.", messages);
			AssertContains(@"Information|Start to run Automated Delta 413 messages in Company FR2 Branch BBB.
Information|No candidate entries in Company FR2 Branch BBB.
Information|Finished running Automated Delta 413 messages in Company FR2 Branch BBB.", messages);
			AssertContains(@"Information|Start to run Automated Delta 432 messages in Company FR1 Branch AAA.
Information|No candidate entries in Company FR1 Branch AAA.
Information|Finished running Automated Delta 432 messages in Company FR1 Branch AAA.", messages);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
