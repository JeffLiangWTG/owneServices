using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.FR.ServiceTasks.Testing
{
	[TestedType(typeof(FRCustomsFallbackProcessingServiceTask))]
	class FRCustomsFallbackProcessingServiceTaskTest : ServiceTaskTestCase<FRCustomsFallbackProcessingServiceTask>
	{
		public void TestHostedServiceAttribute()
		{
			var attr = GetHostedServiceAttributes().FirstOrDefault();
			AssertEquals("15Minutes", attr.MinimumPeriod);
			AssertEquals(true, attr.CanRunInAnyBranch);
			AssertEquals("15Minutes", attr.DefaultScheduleRunEvery);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				var result = new List<TaskNudgeInformationForTest>();
				foreach (var countryCode in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
				{
					result.Add(new TaskNudgeInformationForTest(
						CusEntryNumSchema.Constants.TableName,
						countryCode + " Customs Entries awaiting email",
						CusEntryNumSchema.Constants.CE_EntryType + "=" + CusEntryHeader.Schema.FallbackEntryType,
						CusEntryNumSchema.Constants.CE_EntryStatus + "=" + DeltaGFallbackStatusList.Codes.PPW,
						CusEntryNumSchema.Constants.CE_RN_NKCountryCode + "=" + countryCode,
						CusEntryNumSchema.Constants.CE_Category + "=" + CusEntryNumber.Categories.CustomsPermitClearanceNumber));
				}
				return result.ToArray();
			}
		}

		[TestDate(2020, 07, 20, 12, 0, 0)]
		public void TestRunTaskCore()
		{
			var companies = Factory.Load<GlbCompany>(new ZQuery());
			companies.ForEach(c => c.GC_RN_NKCountryCode = "AU"); // We only want to test with the companies below, make EDIHQ Australian.

			var frCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			frCompany1.GC_Code = "FR1";
			frCompany1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			frCompany1.GC_OH_OrgProxy = org1.PK;
			var branch1 = frCompany1.Branches.AddNew();
			branch1.GB_Code = "AAA";

			var frCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			frCompany2.GC_Code = "FR2";
			frCompany2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			frCompany2.GC_OH_OrgProxy = org2.PK;
			var branch2 = frCompany2.Branches.AddNew();
			branch2.GB_Code = "BBB";
			Factory.Save();

			using (FRCustomsDataRegistry.Instance.RegularisationTimerInMinutes.SetTemporaryValue(frCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (FRCustomsDataRegistry.Instance.RegularisationTimerInMinutes.SetTemporaryValue(frCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			using (DisposableEnvironment.ForCompany("FR1"))
			{
				SetDeltaGRegistrySetting(ZDateTime.Now.AddDays(-2), 60);
				var declaration1 = Factory.New<JobDeclaration>();

				var entry1 = declaration1.CustomsEntryHeaders.AddNew();
				entry1.CH_BGMReference = "123456";

				var message1 = entry1.Messages.AddNew();
				message1.EM_ApplicationReference = "112";
				message1.EM_ApplicationCode = FREDIMessage.ApplicationCodes.FRCustomsMessage;
				message1.EM_Status = EDIMessage.Status.Queued;
				message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				message1.MessageNumberStrategy = new FRMessageNumberStrategy(message1.Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
				message1.EM_MessageType = "XDC";
				message1.EM_HeldUntilDate = ZDateTime.Empty;

				CusEntryNumber cusEntryNumber1 = Factory.New<CusEntryNumber>();
				cusEntryNumber1.Parent = entry1;
				cusEntryNumber1.CE_EntryStatus = DeltaGFallbackStatusList.Codes.PDS;
				cusEntryNumber1.CE_IssueDate = ZDateTime.Now.AddDays(-1);
				cusEntryNumber1.CE_EntryType = CusEntryHeader.Schema.FallbackEntryType;
				cusEntryNumber1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				cusEntryNumber1.CE_EntryNum = "00003";
				Factory.Save();

				using (DisposableEnvironment.ForCompany("FR2"))
				{
					SetDeltaGRegistrySetting(ZDateTime.Now.AddDays(-2), 60);
					var declaration2 = Factory.New<JobDeclaration>();
					var entry2 = declaration2.CustomsEntryHeaders.AddNew();
					entry2.CH_BGMReference = "123456";

					var message2 = entry2.Messages.AddNew();
					message2.EM_ApplicationReference = "112";
					message2.EM_ApplicationCode = FREDIMessage.ApplicationCodes.FRCustomsMessage;
					message2.EM_Status = EDIMessage.Status.Queued;
					message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
					message2.MessageNumberStrategy = new FRMessageNumberStrategy(message2.Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
					message2.EM_MessageType = "XDC";
					message2.EM_HeldUntilDate = ZDateTime.Empty;

					CusEntryNumber cusEntryNumber2 = Factory.New<CusEntryNumber>();
					cusEntryNumber2.Parent = entry2;
					cusEntryNumber2.CE_EntryStatus = DeltaGFallbackStatusList.Codes.PDS;
					cusEntryNumber2.CE_IssueDate = ZDateTime.Now.AddDays(-1);
					cusEntryNumber2.CE_EntryType = CusEntryHeader.Schema.FallbackEntryType;
					cusEntryNumber2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
					cusEntryNumber2.CE_EntryNum = "00004";

					Factory.Save();

					var serviceTask = new FRCustomsFallbackProcessingServiceTask();
					InitialiseAndRunTaskSchedule(serviceTask);

					cusEntryNumber1.Reload();
					cusEntryNumber2.Reload();

					AssertEquals(DeltaGFallbackStatusList.Codes.RGA, cusEntryNumber1.CE_EntryStatus);
					AssertEquals(DeltaGFallbackStatusList.Codes.RGA, cusEntryNumber2.CE_EntryStatus);
				}
			}
		}

		FallbackSettings SetDeltaGRegistrySetting(ZDateTime strat, ZInt regularisationPeriod, ZDateTime? end = null, ZDateTime? regularisation = null)
		{
			var deltaGRegistrySetting = (FRCustomsDataRegistry.Instance.DeltaGMode.Value as FallbackSettings);
			deltaGRegistrySetting.Start = strat;
			deltaGRegistrySetting.End = end ?? ZDateTime.Now.AddDays(-1);
			deltaGRegistrySetting.Regularisation = regularisation ?? ZDateTime.Now.AddDays(-1);
			deltaGRegistrySetting.RegularisationPeriod = regularisationPeriod;
			deltaGRegistrySetting.InvocationReason = "InvocationReason";
			deltaGRegistrySetting.RevocationReason = "RevocationReason";
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, deltaGRegistrySetting);
			return deltaGRegistrySetting;
		}
	}
}
