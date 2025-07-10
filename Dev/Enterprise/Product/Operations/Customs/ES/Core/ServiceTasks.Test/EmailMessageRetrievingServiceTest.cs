using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.ES.ServiceTasks.Testing;

[TestedType(typeof(EmailMessageRetrievingService))]
class EmailMessageRetrievingServiceTest : ServiceTaskTestCase<EmailMessageRetrievingService>
{
	public void TestNoActiveBranch()
	{
		var company = Factory.New<GlbCompany>();
		company.GC_Code = "VKO";
		company.GC_Name = "COMPANY TEST";
		company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Spain;
		var branch = company.Branches.AddNew();
		branch.FillWithValidTestData();
		branch.GB_Code = "TST";
		branch.GB_IsActive = false;

		Factory.Save();

		DeactivateSpanishBranches();

		using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
		{
			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] Does Current Company have active branches?", false, Env.CurrentCompany.ActiveBranches.Any());
				var serviceTask = new EmailMessageRetrievingService();
				AssertNoExceptionThrown("Running the service task should not throw an exception even if there are no active branches", () => InitialiseAndRunTaskSchedule(serviceTask));
			});
		}
	}

	public void TestEsActiveBranch()
	{
		var company = Factory.New<GlbCompany>();
		company.GC_Code = "COM";
		company.GC_Name = "COMPANY TEST";
		company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Italy;
		var noActiveBranch = company.Branches.AddNew();
		noActiveBranch.FillWithValidTestData();
		noActiveBranch.GB_Code = "STS";
		noActiveBranch.GB_IsActive = false;

		var spanishCompany = Factory.New<GlbCompany>();
		spanishCompany.GC_Code = "VKO";
		spanishCompany.GC_Name = "ES COMPANY TEST";
		spanishCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Spain;
		var spanishActiveBranch = spanishCompany.Branches.AddNew();
		spanishActiveBranch.FillWithValidTestData();
		spanishActiveBranch.GB_Code = "TST";
		spanishActiveBranch.GB_RL_NKHomePort = Enterprise.Core.Constants.CountryCodes.Spain;

		Factory.Save();

		DeactivateSpanishBranches();
		spanishActiveBranch.GB_IsActive = true;

		using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, noActiveBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
		{
			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("AgenciaTributaria@correo.aeat.es"))
			{
				CombineAssertions(() =>
				{
					AssertEquals("[PRE-CONDITION] Does Current Company have active branches?", false, Env.CurrentCompany.ActiveBranches.Any());
					AssertEquals("[PRE-CONDITION] An ES company with active branches exist", true, GlbBranch.FindAnyBranchInSameCountry(Factory, RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Spain)).GB_IsActive);

					var correctMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho DUA IMP: 20 ES 009999 3 001164 1");
					Factory.Save();

					var serviceTask = new EmailMessageRetrievingService();
					InitialiseAndRunTaskSchedule(serviceTask);

					ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
					messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
					EDIMessage[] messages = Factory.Load<EDIMessage>(messagesQuery);

					AssertEquals("messages created", 1, messages.Length);
				});
			}
		}
	}

	public void TestHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
		var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "ESM", hostedServiceAttribute.Code);
				AssertEquals("Description", "ES Customs Mail Retrieving", hostedServiceAttribute.Description);
				AssertEquals("Category", "ESC", hostedServiceAttribute.Category);
				AssertEquals("RequiresCompanyInCountry", Enterprise.Core.Constants.CountryCodes.Spain, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("DefaultScheduleRunEvery", "15minutes", hostedServiceAttribute.DefaultScheduleRunEvery);
			});
		}

		public void TestInitialiseSchedule()
		{
			var serviceTask = new EmailMessageRetrievingService();
			InitialiseTaskSchedule(serviceTask, out StmServiceTask taskSchedule);

			CombineAssertions(() =>
			{
				AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
				Assert("TaskPeriod", taskSchedule.Recurrence.MinutesRange);
				AssertEquals("TaskPeriodCount", 15, taskSchedule.Recurrence.Period);
				AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
				AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
			});
		}

	public void TestRunTask_Import()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("AgenciaTributaria@correo.aeat.es"))
		{
			var correctMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho DUA IMP: 20 ES 009999 3 001164 1");
			var correctMailVexcan = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho DUA IMP: 20 ES 009998 3 000053 7 - VEXCAN");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Despacho DUA IMP: 20 ES 009998 3 000053 8");
			var incorrectStatusMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Unprocessed, "Despacho DUA IMP: 20 ES 009998 3 000053 9");
			var incorrectSubjectMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho AAA: 20 ES 009998 3 000053 6");

			Factory.Save();

			ErrorReporter.Clear();

			var serviceTask = new EmailMessageRetrievingService();
			InitialiseAndRunTaskSchedule(serviceTask);

			CombineAssertions(() =>
			{
				ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				EDIMessage[] messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("messages created", 2, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("20ES00999930011641", DeclarationMessageTypeList.Codes.ImportClearanceEmail),
															("20ES00999830000537", DeclarationMessageTypeList.Codes.ImportClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				correctMailVexcan.Reload();
				AssertEquals("correctMailVexcan MI_Status", "PRS", correctMailVexcan.MI_Status);
				var message2 = messages[1];
				AssertMessage("CorrectMail 2", message2);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);

				AssertEquals("ServiceTaskThatRequiresSetUserContext is not reported", ZString.Empty, ErrorReporter.LastMessageReported);
				AssertNotContains("EnableImplicitUserContextAccessReporting message", "accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}
	}

	public void TestRunTask_Ncts()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("AgenciaTributaria@correo.aeat.es"))
		{
			var correctMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho del tránsito con MRN: 21ES00084152088719");
			var correctMail2 = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho del tránsito con MRN: 21ES00084152088720");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Despacho del tránsito con MRN: 21ES00359150119711");
			var incorrectStatusMail = AddMailItem("AgenciaTributaria@correo.aeat.es", MailStatus.Unprocessed, "Despacho del tránsito con MRN: 21ES00359150119712");
			var incorrectSubjectMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho AAA: 20 ES 009998 3 000053 6");

			Factory.Save();

			ErrorReporter.Clear();

			var serviceTask = new EmailMessageRetrievingService();
			InitialiseAndRunTaskSchedule(serviceTask);

			CombineAssertions(() =>
			{
				ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				EDIMessage[] messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("messages created", 2, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("21ES00084152088719", DeclarationMessageTypeList.Codes.NctsClearanceEmail),
															("21ES00084152088720", DeclarationMessageTypeList.Codes.NctsClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				correctMail2.Reload();
				AssertEquals("correctMail2 MI_Status", "PRS", correctMail2.MI_Status);
				var message2 = messages[1];
				AssertMessage("CorrectMail 2", message2);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);

				AssertEquals("ServiceTaskThatRequiresSetUserContext is not reported", ZString.Empty, ErrorReporter.LastMessageReported);
				AssertNotContains("EnableImplicitUserContextAccessReporting message", "accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}
	}

	public void TestRunTask_T2L()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("AgenciaTributaria@correo.aeat.es"))
		{
			var correctMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho T2L: 21ES009999L0002748");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Despacho T2L: 21ES009999L0002748");
			var incorrectStatusMail = AddMailItem("AgenciaTributaria@correo.aeat.es", MailStatus.Unprocessed, "Despacho T2L: 21ES009999L0002748");
			var incorrectSubjectMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho AAA: 20 ES 009999 L 000037 4");

			Factory.Save();

			ErrorReporter.Clear();

			var serviceTask = new EmailMessageRetrievingService();
			InitialiseAndRunTaskSchedule(serviceTask);

			CombineAssertions(() =>
			{
				var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				var messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("1 message1 created", 1, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("21ES009999L0002748", DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);

				AssertEquals("ServiceTaskThatRequiresSetUserContext is not reported", ZString.Empty, ErrorReporter.LastMessageReported);
				AssertNotContains("EnableImplicitUserContextAccessReporting message", "accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}
	}

	public void TestRunTask_T2C()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("AgenciaTributaria@correo.aeat.es"))
		{
			var correctMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho T2L: 21ES009999M0000707");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Despacho T2L: 21ES009999M0002748");
			var incorrectStatusMail = AddMailItem("AgenciaTributaria@correo.aeat.es", MailStatus.Unprocessed, "Despacho T2L: 21ES009999M0002748");
			var incorrectSubjectMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho AAA: 20 ES 009999 M 000037 4");

			Factory.Save();

			ErrorReporter.Clear();

			var serviceTask = new EmailMessageRetrievingService();
			InitialiseAndRunTaskSchedule(serviceTask);

			CombineAssertions(() =>
			{
				var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				var messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("1 message1 created", 1, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("21ES009999M0000707", DeclarationMessageTypeList.Codes.T2cClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);

				AssertEquals("ServiceTaskThatRequiresSetUserContext is not reported", ZString.Empty, ErrorReporter.LastMessageReported);
				AssertNotContains("EnableImplicitUserContextAccessReporting message", "accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}
	}

	public void TestRunTask_T2CPOUS()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("AgenciaTributaria@correo.aeat.es"))
		{
			var correctMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho JEC: 21ES009999M0000707");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Despacho JEC: 21ES009999M0002748");
			var incorrectStatusMail = AddMailItem("AgenciaTributaria@correo.aeat.es", MailStatus.Unprocessed, "Despacho JEC: 21ES009999M0002748");
			var incorrectSubjectMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho AAA: 20 ES 009999 M 000037 4");

			Factory.Save();

			ErrorReporter.Clear();

			var serviceTask = new EmailMessageRetrievingService();
			InitialiseAndRunTaskSchedule(serviceTask);

			CombineAssertions(() =>
			{
				var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				var messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("1 message1 created", 1, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("21ES009999M0000707", DeclarationMessageTypeList.Codes.T2cClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);

				AssertEquals("ServiceTaskThatRequiresSetUserContext is not reported", ZString.Empty, ErrorReporter.LastMessageReported);
				AssertNotContains("EnableImplicitUserContextAccessReporting message", "accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}
	}

	public void TestRunTask_Export()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("AgenciaTributaria@correo.aeat.es"))
		{
			var correctMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Levante AES EXP: 21ES00280120889150");
			var correctMail2 = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Levante AES EXP: 21ES00389110181283 VEXCAN");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Levante AES EXP: 21ES00280120889150");
			var incorrectStatusMail = AddMailItem("AgenciaTributaria@correo.aeat.es", MailStatus.Unprocessed, "Levante AES EXP: 21ES00280120889150");
			var incorrectSubjectMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Levante AAA: 21 ES 00280 12 0889150");

			Factory.Save();

			ErrorReporter.Clear();

			var serviceTask = new EmailMessageRetrievingService();
			InitialiseAndRunTaskSchedule(serviceTask);

			CombineAssertions(() =>
			{
				var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				var messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("messages created", 2, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("21ES00280120889150", DeclarationMessageTypeList.Codes.ExportClearanceEmail),
															("21ES00389110181283", DeclarationMessageTypeList.Codes.ExportClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				correctMail2.Reload();
				AssertEquals("correctMail2 MI_Status", "PRS", correctMail2.MI_Status);
				var message2 = messages[1];
				AssertMessage("CorrectMail 2", message2);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);

				AssertEquals("ServiceTaskThatRequiresSetUserContext is not reported", ZString.Empty, ErrorReporter.LastMessageReported);
				AssertNotContains("EnableImplicitUserContextAccessReporting message", "accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}
	}

	public void TestRunTask_G5()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("AgenciaTributaria@correo.aeat.es"))
		{
			var correctMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho de G5G de Recepción con MRN 25ESG5G000000749Y0");
			var correctMail2 = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho de G5G de Recepción con MRN 21ES00084152088720");

			var incorrectFromMail = AddMailItem("<mail@mail.com>", MailStatus.Queued, "Despacho de G5G de Recepción con MRN 21ES00359150119711");
			var incorrectStatusMail = AddMailItem("AgenciaTributaria@correo.aeat.es", MailStatus.Unprocessed, "Despacho de G5G de Recepción con MRN 21ES00359150119712");
			var incorrectSubjectMail = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho AAA 20 ES 009998 3 000053 6");

			Factory.Save();

			ErrorReporter.Clear();

			var serviceTask = new EmailMessageRetrievingService();
			InitialiseAndRunTaskSchedule(serviceTask);

			CombineAssertions(() =>
			{
				ZQuery messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				EDIMessage[] messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("messages created", 2, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("25ESG5G000000749Y0", DeclarationMessageTypeList.Codes.G5ClearanceEmail),
															("21ES00084152088720", DeclarationMessageTypeList.Codes.G5ClearanceEmail)
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMail.Reload();
				AssertEquals("CorrectMail MI_Status", "PRS", correctMail.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				correctMail2.Reload();
				AssertEquals("correctMail2 MI_Status", "PRS", correctMail2.MI_Status);
				var message2 = messages[1];
				AssertMessage("CorrectMail 2", message2);

				incorrectFromMail.Reload();
				AssertEquals("IncorrectFromMail MI_Status", "QUE", incorrectFromMail.MI_Status);

				incorrectStatusMail.Reload();
				AssertEquals("IncorrectStatusMail MI_Status", "UPR", incorrectStatusMail.MI_Status);

				incorrectSubjectMail.Reload();
				AssertEquals("IncorrectSubjectMail MI_Status", "QUE", incorrectSubjectMail.MI_Status);

				AssertEquals("ServiceTaskThatRequiresSetUserContext is not reported", ZString.Empty, ErrorReporter.LastMessageReported);
				AssertNotContains("EnableImplicitUserContextAccessReporting message", "accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}
	}

	public void TestRunTask_All()
	{
		using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailFrom("AgenciaTributaria@correo.aeat.es"))
		{
			var correctMailImport = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho DUA IMP: 20 ES 009999 3 001164 1");
			var correctMailNcts = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho del tránsito con MRN: 21ES00084152088719");
			var correctMailT2L = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho T2L: 21ES009999L0002748");
			var correctMailT2C = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho T2L: 21ES009999M0000707");
			var correctMailT2CPOUS = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho JEC: 21ES009999M0000808");
			var correctMailExport = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Levante AES EXP: 21ES00280120889150");
			var correctMailG5 = AddMailItem("<AgenciaTributaria@correo.aeat.es>", MailStatus.Queued, "Despacho de G5G de Recepción con MRN 25ESG5G000000749Y0");

			Factory.Save();

			ErrorReporter.Clear();

			var serviceTask = new EmailMessageRetrievingService();
			InitialiseAndRunTaskSchedule(serviceTask);

			CombineAssertions(() =>
			{
				var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
				messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				var messages = Factory.Load<EDIMessage>(messagesQuery);
				AssertEquals("messages created", 7, messages.Length);
				AssertContainsExactElementsInAnyOrder("messages created with correct EM_ApplicationReferences (mrn codes) and EM_MessageType",
														new (ZString, ZString)[] {
															("20ES00999930011641", DeclarationMessageTypeList.Codes.ImportClearanceEmail),
															("21ES00084152088719", DeclarationMessageTypeList.Codes.NctsClearanceEmail),
															("21ES009999L0002748", DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail),
															("21ES009999M0000707", DeclarationMessageTypeList.Codes.T2cClearanceEmail),
															("21ES009999M0000808", DeclarationMessageTypeList.Codes.T2cClearanceEmail),
															("21ES00280120889150", DeclarationMessageTypeList.Codes.ExportClearanceEmail),
															("25ESG5G000000749Y0", DeclarationMessageTypeList.Codes.G5ClearanceEmail),
														}, messages.Select(x => (x.EM_ApplicationReference, x.EM_MessageType)).ToArray());

				correctMailImport.Reload();
				AssertEquals("CorrectMailImport MI_Status", "PRS", correctMailImport.MI_Status);
				var message1 = messages[0];
				AssertMessage("CorrectMail 1", message1);

				correctMailNcts.Reload();
				AssertEquals("correctMailNcts MI_Status", "PRS", correctMailNcts.MI_Status);
				var message2 = messages[1];
				AssertMessage("CorrectMail 2", message2);

				correctMailT2L.Reload();
				AssertEquals("correctMailT2L MI_Status", "PRS", correctMailT2L.MI_Status);
				var message3 = messages[2];
				AssertMessage("CorrectMail 3", message3);

				correctMailT2C.Reload();
				AssertEquals("correctMailT2C MI_Status", "PRS", correctMailT2C.MI_Status);
				var message4 = messages[3];
				AssertMessage("CorrectMail 4", message4);

				correctMailT2CPOUS.Reload();
				AssertEquals("correctMailT2CPOUS MI_Status", "PRS", correctMailT2CPOUS.MI_Status);
				var message5 = messages[4];
				AssertMessage("CorrectMail 5", message5);

				correctMailExport.Reload();
				AssertEquals("correctMailExport MI_Status", "PRS", correctMailExport.MI_Status);
				var message6 = messages[5];
				AssertMessage("CorrectMail 6", message6);

				correctMailG5.Reload();
				AssertEquals("correctMailExport MI_Status", "PRS", correctMailG5.MI_Status);
				var message7 = messages[5];
				AssertMessage("CorrectMail 7", message7);

				AssertEquals("ServiceTaskThatRequiresSetUserContext is not reported", ZString.Empty, ErrorReporter.LastMessageReported);
				AssertNotContains("EnableImplicitUserContextAccessReporting message", "accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
	{
		get
		{
			return new TaskNudgeInformationForTest[]
			{
				new TaskNudgeInformationForTest(
					MailDBItemsSchema.Constants.TableName,
					EmailMessageRetrievingService.FriendlyName,
					MailDBItemsSchema.Constants.MI_Status + "=" + MailStatus.Queued,
					MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.ESImportMailTask,
					MailDBItemsSchema.Constants.MI_Direction + "=" + EDIMessage.Direction.Receive),
			};
		}
	}

	void AssertMessage(ZString mailType, EDIMessage message)
	{
		AssertEquals(mailType + " message.EM_ApplicationCode", ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
		AssertEquals(mailType + " message.EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		AssertEquals(mailType + " message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
		AssertEquals(mailType + " message.EM_IsActive", true, message.EM_IsActive);
		AssertEquals(mailType + " message.EM_MessageText", "Body", message.EM_MessageText);
	}

	MailItem AddMailItem(ZString from, ZString status, ZString subject)
	{
		var mailItem = Factory.New<MailItem>();
		mailItem.MI_From = from;
		mailItem.MI_Subject = subject;
		mailItem.MI_Status = status;
		mailItem.MI_Direction = MailDirection.Receive;
		mailItem.MI_SendDateTime = ZDateTime.Now.AddMinutes(-5);
		mailItem.MI_ReceivedDateTime = ZDateTime.Now;
		mailItem.MI_Header = "Header";
		mailItem.MI_Body = "Body";
		mailItem.MI_Application = MailFilterCodes.ESImportMailTask;
		return mailItem;
	}

	void DeactivateSpanishBranches()
	{
		var anotherFactory = new BusinessObjectFactory();
		var esBranchQuery = new ZQuery(GlbBranchSchema.GB_RL_NKHomePort, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.Spain);
		var esBranches = anotherFactory.Load<GlbBranch>(esBranchQuery);
		esBranches.ForEach(x => x.GB_IsActive = false);
		anotherFactory.Save();
	}
}
