using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Registry;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.FR.ServiceTasks.Testing
{
	class PdsRegulariserTest : TestCaseWithFactory
	{
		[TestDate(2020, 07, 20, 12, 0, 0)]
		public void TestRegularisationDate()
		{
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
			using (DisposableEnvironment.ForCompany("FR2"))
			{
				SetDeltaGRegistrySetting(ZDateTime.Now.AddDays(-5), 60, ZDateTime.Now.AddDays(-3), ZDateTime.Now.AddDays(1));
				var declaration1 = Factory.New<JobDeclaration>();
				var declaration2 = Factory.New<JobDeclaration>();
				Factory.Save();
				using (DisposableEnvironment.ForCompany("FR1"))
				{
					SetDeltaGRegistrySetting(ZDateTime.Now.AddDays(-5), 60, ZDateTime.Now.AddDays(-3), ZDateTime.Now.AddDays(1));
					using (DisposableEnvironment.ForCompany("FR2"))
					{
						SetUpEntry(declaration2, CusEntryHeader.Schema.FallbackEntryType, Core.Constants.CountryCodes.France, DeltaGFallbackStatusList.Codes.PDS, ZDateTime.Now.AddDays(-4));
						var message2 = declaration2.ActiveEntryHeaders[0].Messages[0];
						using (DisposableEnvironment.ForCompany("FR1"))
						{
							SetUpEntry(declaration1, CusEntryHeader.Schema.FallbackEntryType, Core.Constants.CountryCodes.France, DeltaGFallbackStatusList.Codes.PDS, ZDateTime.Now.AddDays(-4));

							var message1 = declaration1.ActiveEntryHeaders[0].Messages[0];

							var logger2 = new LoggingInformation();
							var pdsRegulariser2 = new PdsRegulariser(Factory, declaration2.Branch, logger2);
							pdsRegulariser2.DoEverything(GlbBranch.CurrentBranch.GB_RN_NKCountryCode);
							cusEntryNumber.Reload();
							message2.Reload();
							AssertEquals("message2 is held because it's not in the current company FR1, and regularization hasn't started yet.", ZDateTime.Empty, message2.EM_HeldUntilDate);

							var logger1 = new LoggingInformation();
							var pdsRegulariser1 = new PdsRegulariser(Factory, declaration1.Branch, logger1);
							pdsRegulariser1.DoEverything(GlbBranch.CurrentBranch.GB_RN_NKCountryCode);
							cusEntryNumber.Reload();
							message1.Reload();
							AssertEquals("message1 is held because the regularization hasn't started yet.", ZDateTime.Empty, message1.EM_HeldUntilDate);

							SetDeltaGRegistrySetting(ZDateTime.Now.AddDays(-5), 60, ZDateTime.Now.AddDays(-3), ZDateTime.Now.AddDays(-1));

							logger2 = new LoggingInformation();
							pdsRegulariser2 = new PdsRegulariser(Factory, declaration2.Branch, logger2);
							pdsRegulariser2.DoEverything(GlbBranch.CurrentBranch.GB_RN_NKCountryCode);
							cusEntryNumber.Reload();
							message2.Reload();
							AssertEquals("message2 is held because it's not in the current company FR1.", ZDateTime.Empty, message2.EM_HeldUntilDate);

							logger1 = new LoggingInformation();
							pdsRegulariser1 = new PdsRegulariser(Factory, declaration1.Branch, logger1);
							pdsRegulariser1.DoEverything(GlbBranch.CurrentBranch.GB_RN_NKCountryCode);
							cusEntryNumber.Reload();
							message1.Reload();
							AssertEquals("message1 should be released because the regularization has started.", new ZDateTime(2020, 7, 20, 12, 0, 0), message1.EM_HeldUntilDate);
						}
					}
				}
			}
		}

		[TestDate(2020, 07, 20, 12, 0, 0)]
		public void TestPdsRegulariserDoEverything()
		{
			using (FRCustomsDataRegistry.Instance.RegularisationTimerInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				SetDeltaGRegistrySetting(ZDateTime.Now.AddDays(-2), 60);
				var declaration = Factory.New<JobDeclaration>();
				SetUpEntry(declaration, CusEntryHeader.Schema.FallbackEntryType, Core.Constants.CountryCodes.France, DeltaGFallbackStatusList.Codes.PDS, ZDateTime.Now.AddDays(-1));
				Factory.Save();

				var message = declaration.ActiveEntryHeaders[0].Messages[0];
				cusEntryNumber.Reload();
				message.Reload();

				AssertEquals(DeltaGFallbackStatusList.Codes.PDS, cusEntryNumber.CE_EntryStatus);
				AssertEquals(ZDateTime.Empty, message.EM_HeldUntilDate);

				var logger = new LoggingInformation();
				var pdsRegulariser = new PdsRegulariser(Factory, declaration.Branch, logger);
				pdsRegulariser.DoEverything(GlbBranch.CurrentBranch.Country.Code);

				cusEntryNumber.Reload();
				message.Reload();

				AssertEquals(new ZDateTime(2020, 7, 20, 12, 0, 0), message.EM_HeldUntilDate);
				AssertEquals(DeltaGFallbackStatusList.Codes.RGA, cusEntryNumber.CE_EntryStatus);
			}
		}

		[TestDate(2020, 07, 20, 12, 0, 0)]
		public void TestSpecialCases()
		{
			using (FRCustomsDataRegistry.Instance.RegularisationTimerInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				var deltaGRegistrySetting = SetDeltaGRegistrySetting(ZDateTime.Empty, 1);

				AssertNoExceptionThrown(() =>
				{
					var logger = new LoggingInformation();
					var pdsRegulariser = new PdsRegulariser(Factory, GlbBranch.CurrentBranch, logger);
					pdsRegulariser.DoEverything(GlbBranch.CurrentBranch.Country.Code);
				});
			}
		}

		public void TestGetEdiMessageCondition()
		{
			using (FRCustomsDataRegistry.Instance.RegularisationTimerInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				SetDeltaGRegistrySetting(ZDateTime.Now.AddDays(-2), 60);
				var declaration = Factory.New<JobDeclaration>();
				SetUpEntry(declaration, CusEntryHeader.Schema.FallbackEntryType, Core.Constants.CountryCodes.France, DeltaGFallbackStatusList.Codes.PDS, ZDateTime.Now.AddDays(-1));
				Factory.Save();

				var logger = new LoggingInformation();
				var pdsRegulariser = new PdsRegulariser(Factory, declaration.Branch, logger);
				pdsRegulariser.DoEverything(GlbBranch.CurrentBranch.Country.Code);

				var deltaGRegistrySetting = (FRCustomsDataRegistry.Instance.DeltaGMode.Value as FallbackSettings);
				AssertEquals(1m, deltaGRegistrySetting.RegularisationCount);

				SetUpEntry(declaration, "ZZZ", Core.Constants.CountryCodes.France, DeltaGFallbackStatusList.Codes.PDS, ZDateTime.Now.AddDays(-1));
				Factory.Save();

				pdsRegulariser.DoEverything(GlbBranch.CurrentBranch.Country.Code);
				deltaGRegistrySetting = (FRCustomsDataRegistry.Instance.DeltaGMode.Value as FallbackSettings);
				AssertEquals(0m, deltaGRegistrySetting.RegularisationCount);

				SetUpEntry(declaration, CusEntryHeader.Schema.FallbackEntryType, Core.Constants.CountryCodes.Germany, DeltaGFallbackStatusList.Codes.PDS, ZDateTime.Now.AddDays(-1));
				Factory.Save();

				pdsRegulariser.DoEverything(GlbBranch.CurrentBranch.Country.Code);
				deltaGRegistrySetting = (FRCustomsDataRegistry.Instance.DeltaGMode.Value as FallbackSettings);
				AssertEquals(0m, deltaGRegistrySetting.RegularisationCount);

				SetUpEntry(declaration, CusEntryHeader.Schema.FallbackEntryType, Core.Constants.CountryCodes.France, DeltaGFallbackStatusList.Codes.PPS, ZDateTime.Now.AddDays(-1));
				Factory.Save();

				pdsRegulariser.DoEverything(GlbBranch.CurrentBranch.Country.Code);
				deltaGRegistrySetting = (FRCustomsDataRegistry.Instance.DeltaGMode.Value as FallbackSettings);
				AssertEquals(0m, deltaGRegistrySetting.RegularisationCount);

				SetUpEntry(declaration, CusEntryHeader.Schema.FallbackEntryType, Core.Constants.CountryCodes.France, DeltaGFallbackStatusList.Codes.PPS, ZDateTime.Now.AddDays(-4));
				Factory.Save();

				pdsRegulariser.DoEverything(GlbBranch.CurrentBranch.Country.Code);
				deltaGRegistrySetting = (FRCustomsDataRegistry.Instance.DeltaGMode.Value as FallbackSettings);
				AssertEquals(0m, deltaGRegistrySetting.RegularisationCount);
			}
		}

		[TestDate(2020, 07, 20, 12, 0, 0)]
		public void TestNextRuntime()
		{
			using (FRCustomsDataRegistry.Instance.RegularisationTimerInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				SetDeltaGRegistrySetting(ZDateTime.Now.AddDays(-2), 60);

				var declaration = Factory.New<JobDeclaration>();
				for (int i = 0; i < 10; i++)
				{
					SetUpEntry(declaration, CusEntryHeader.Schema.FallbackEntryType, Core.Constants.CountryCodes.France, DeltaGFallbackStatusList.Codes.PDS, ZDateTime.Now.AddDays(-1));
				}
				Factory.Save();

				var logger = new LoggingInformation();
				var pdsRegulariser = new PdsRegulariser(Factory, declaration.Branch, logger);
				pdsRegulariser.DoEverything(GlbBranch.CurrentBranch.Country.Code);
				AssertContains("Should be nudged", "FR Customs Fallback Processor nudged to run at: 20-Jul-20 12:30:00", logger.Logs.First().ToString());
			}
		}

		[TestDate(2020, 07, 20, 12, 0, 0)]
		public void TestDeltaGRegistrySettingAfterAutomaticRegularisation()
		{
			using (FRCustomsDataRegistry.Instance.RegularisationTimerInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				var deltaGRegistrySetting = SetDeltaGRegistrySetting(ZDateTime.Now.AddDays(-2), 60);

				var declaration = Factory.New<JobDeclaration>();
				for (int i = 0; i < 100; i++)
				{
					SetUpEntry(declaration, CusEntryHeader.Schema.FallbackEntryType, Core.Constants.CountryCodes.France, DeltaGFallbackStatusList.Codes.PDS, ZDateTime.Now.AddDays(-1));
				}
				Factory.Save();

				var logger = new LoggingInformation();
				var pdsRegulariser = new PdsRegulariser(Factory, declaration.Branch, logger);

				for (int i = 0; i < 2; i++)
				{
					pdsRegulariser.DoEverything(GlbBranch.CurrentBranch.Country.Code);
					deltaGRegistrySetting = (FRCustomsDataRegistry.Instance.DeltaGMode.Value as FallbackSettings);
					AssertEquals(new ZDateTime(2020, 7, 18, 12, 0, 0), deltaGRegistrySetting.Start);
					AssertEquals(new ZDateTime(2020, 7, 19, 12, 0, 0), deltaGRegistrySetting.End);
					AssertEquals(new ZDateTime(2020, 7, 19, 12, 0, 0), deltaGRegistrySetting.Regularisation);
					AssertEquals(60, deltaGRegistrySetting.RegularisationPeriod);
					AssertEquals(100m, deltaGRegistrySetting.RegularisationCount);
					AssertEquals(50m, deltaGRegistrySetting.RegularisationBatchSize);
					AssertEquals("InvocationReason", deltaGRegistrySetting.InvocationReason);
					AssertEquals("RevocationReason", deltaGRegistrySetting.RevocationReason);
				}

				pdsRegulariser.DoEverything(GlbBranch.CurrentBranch.Country.Code);
				deltaGRegistrySetting = (FRCustomsDataRegistry.Instance.DeltaGMode.Value as FallbackSettings);
				AssertEquals(ZDateTime.Empty, deltaGRegistrySetting.Start);
				AssertEquals(ZDateTime.Empty, deltaGRegistrySetting.End);
				AssertEquals(ZDateTime.Empty, deltaGRegistrySetting.Regularisation);
				AssertEquals(ZInt.Zero, deltaGRegistrySetting.RegularisationPeriod);
				AssertEquals(ZDecimal.Zero, deltaGRegistrySetting.RegularisationCount);
				AssertEquals(ZDecimal.Zero, deltaGRegistrySetting.RegularisationBatchSize);
				AssertEquals(ZString.Empty, deltaGRegistrySetting.InvocationReason);
				AssertEquals(ZString.Empty, deltaGRegistrySetting.RevocationReason);
			}
		}

		[TestDate(2020, 07, 20, 12, 0, 0)]
		public void TestScheduleReport()
		{
			using (FRCustomsDataRegistry.Instance.RegularisationTimerInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			{
				using (FRCustomsDataRegistry.Instance.FallbackRegularisationReportNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
				{
					SetDeltaGRegistrySetting(ZDateTime.Now.AddDays(-2), 60);
					var logger = new LoggingInformation();
					var pdsRegulariser = new PdsRegulariser(Factory, Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch, logger);
					pdsRegulariser.DoEverything(GlbBranch.CurrentBranch.Country.Code);
					var query = new ZQuery();
					query.AddToFilter(StmScheduleTaskSchema.S5_ParentID, Guid.Parse("a7c54ba4-08a0-427f-b409-f997c1d4b820"));
					query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, StmMenuItemSchema.Constants.Prefix);
					var scheduleTasks = Factory.Load<ReportScheduleTask>(query);
					AssertEquals(0, scheduleTasks.Length);
				}

				var group = Factory.New<GlbGroup>();
				group.GG_Code = "TST";
				group.GG_Desc = "GROUP 1";

				var staff = group.Staff.AddNew();
				staff.GS_Code = "Z1Z";
				staff.GS_FullName = "STAFF 1 DUMMY";
				staff.GS_EmailAddress = "staff1@test.com";
				staff.GS_LoginName = "Z1";
				Factory.Save();

				using (FRCustomsDataRegistry.Instance.FallbackRegularisationReportNotificationGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
				{
					SetDeltaGRegistrySetting(ZDateTime.Now.AddDays(-2), 60);
					var logger = new LoggingInformation();
					var pdsRegulariser = new PdsRegulariser(Factory, GlbBranch.CurrentBranch, logger);
					pdsRegulariser.DoEverything(GlbBranch.CurrentBranch.Country.Code);
					var query = new ZQuery();
					query.AddToFilter(StmScheduleTaskSchema.S5_ParentID, Guid.Parse("a7c54ba4-08a0-427f-b409-f997c1d4b820"));
					query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, StmMenuItemSchema.Constants.Prefix);
					var scheduleTasks = Factory.Load<ReportScheduleTask>(query);
					AssertEquals(1, scheduleTasks.Length);
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

		void SetUpEntry(JobDeclaration declaration, string entryType, string countryCode, string entryNumberStatus, ZDateTime issueDate)
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "123456";

			var message = entry.Messages.AddNew();
			message.EM_ApplicationReference = "112";
			message.EM_ApplicationCode = FREDIMessage.ApplicationCodes.FRCustomsMessage;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.MessageNumberStrategy = new FRMessageNumberStrategy(message.Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
			message.EM_MessageType = "XDC";
			message.EM_HeldUntilDate = ZDateTime.Empty;

			cusEntryNumber = Factory.New<CusEntryNumber>();
			cusEntryNumber.Parent = entry;
			cusEntryNumber.CE_EntryStatus = entryNumberStatus;
			cusEntryNumber.CE_IssueDate = issueDate;
			cusEntryNumber.CE_EntryType = entryType;
			cusEntryNumber.CE_RN_NKCountryCode = countryCode;
			cusEntryNumber.CE_EntryNum = "00003";

			Factory.Save();
		}

		CusEntryNumber cusEntryNumber;
	}
}
