using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.Ccsuk.ServiceTasks.Testing
{
	[TestedType(typeof(CcsukMaintenanceServiceTask))]
	class CcsukMaintenanceServiceTaskTest : ServiceTaskTestCase<CcsukMaintenanceServiceTask>
	{
		delegate ICcsukCusAwb MakeAndPrepareAirWaybill(ZString customsActionCode, ZDateTime customsActionDate, ZShort npr, ZDateTime status1Date, string shipmentDescriptionCode = "T");

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestArchiveStatus2()
		{
			var emailAddress = CuscarInboundParserTests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationCcsukArchive, Guid.Empty, Factory);
			SetupPostmaster();
			var awbCompleted = MakeBasic("", ZDateTime.Empty, 10, ZDateTime.Now.AddDays(-8), "C");
			var awbCompletedButNoStatus2Agent = (CusMAWB)MakeBasic("", ZDateTime.Empty, 10, ZDateTime.Now.AddDays(-8), "C");
			awbCompletedButNoStatus2Agent.Status2Granted = false;
			var awbCompletedButNoStatus2Etsf = (CusMAWB)MakeBasic("", ZDateTime.Empty, 10, ZDateTime.Now.AddDays(-8), "C");
			awbCompletedButNoStatus2Etsf.Status2Granted = false;
			awbCompletedButNoStatus2Etsf.Profile = "CUKAIR98LHRXXX";
			Factory.Save();

			var task = new CcsukMaintenanceServiceTask();
			InitialiseAndRunTaskSchedule(task);
			ReloadAndAssertArchived("Completed - marked as archived", awbCompleted, true);
			ReloadAndAssertArchived("Completed - marked as archived - no S2 but not a shed job", awbCompletedButNoStatus2Agent, true);
			ReloadAndAssertArchived("Open - no status 2 and shed job", awbCompletedButNoStatus2Etsf, false);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains(">-111-55555501</a></p></td><td>Status1 date older than 7 days and SDC=C/E", email.Body);
			AssertContains(">-111-55555502</a></p></td><td>Status1 date older than 7 days and SDC=C/E", email.Body);
			AssertContains(">LHRXXX-111-55555503</a></p></td><td>Not archived because Status 2 is not set on ETSF record.", email.Body);
			AssertContains(emailAddress, Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients[0].Email);
		}

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestArchiveOmitsASectionsOfReport()
		{
			TestArchiveAwbRunner(MakeBasic);

			// Run again - Warnings, no actions
			var task = new CcsukMaintenanceServiceTask();
			InitialiseAndRunTaskSchedule(task);
			var email2 = Env.OutgoingCustomsMailManager.EmailsCreated[1];
			AssertContains("We still have one AWB that does not get archived, it is included in warning report", "The following AWBs have not been archived", email2.Body);
			AssertContains("111-55555514", email2.Body);
			AssertNotContains("No AWBs were archived just now (they were archived in the previous run) so the whole section is omitted", "The following AWBs have been archived", email2.Body);

			// Neither actions nor warnings
			Factory.LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "11155555514")).PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ArchivedOnCcsuk; // was on warning list before, now we exclude him
			Factory.Save();

			task = new CcsukMaintenanceServiceTask();
			InitialiseAndRunTaskSchedule(task);
			AssertEquals("This run produces no (additional) email as there is nothing to archive or exlcude", 2, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			// Actions, no warnings
			Factory.LoadTop1<CusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, "11155555506")).PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.CompletedOnCcsUk;
			Factory.Save();

			task = new CcsukMaintenanceServiceTask();
			InitialiseAndRunTaskSchedule(task);
			var email3 = Env.OutgoingCustomsMailManager.EmailsCreated[2];
			AssertContains("Finally just one AWB is archived", "The following AWBs have been archived", email3.Body);
			AssertNotContains("No warning", "The following AWBs have not been archived", email3.Body);
			AssertContains("111-55555506", email3.Body);

			AssertEquals("Pre-req: Registry default should be false", false, GBCustomsDataRegistry.Instance.CcsukAlwaysSendAwbReport.Value);
			GBCustomsDataRegistry.Instance.CcsukAlwaysSendAwbReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, GBCustomsDataRegistry.Instance.CcsukAlwaysSendAwbReport.Value);
			task = new CcsukMaintenanceServiceTask();
			InitialiseAndRunTaskSchedule(task);
			AssertEquals("This run produces an email as it was overrriden by the registry setting", 4, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var emailReport = Env.OutgoingCustomsMailManager.EmailsCreated[3];
			AssertContains("Nothing was archived nor warned on this occasion", emailReport.Body);
		}

		[TestDate(1986, 3, 12)]
		public void TestArchiveWithMultipleCompanies()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Name = "Company 001";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var branch1 = company1.Branches.AddNew();
			branch1.FillWithValidTestData();

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_Name = "Company 002";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;

			var branch2 = company2.Branches.AddNew();
			branch2.FillWithValidTestData();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = "staff1@test.com";

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "staff2@test.com";

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.Staff.Add(staff1);

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.Staff.Add(staff2);

			Factory.Save();

			GBCustomsDataRegistry.Instance.NotificationCcsukArchive.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, group1.PK.ToGuid());
			GBCustomsDataRegistry.Instance.NotificationCcsukArchive.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, group2.PK.ToGuid());

			awbSerialNumberSuffix++;

			var mawb1 = Factory.New<CusMAWB>();
			mawb1.Profile = "CUKFFW98000AAA";
			mawb1.CM_MAWB = "111555555" + awbSerialNumberSuffix.ToString("0#");
			mawb1.CM_GB = branch1.PK;

			var cusAwb1 = SetupAwb(mawb1, mawb1.MasterLevelHouseHelper.Logs, string.Empty, ZDateTime.Empty, 10, ZDateTime.Now.AddDays(-8), "C");

			awbSerialNumberSuffix++;

			var mawb2 = Factory.New<CusMAWB>();
			mawb2.Profile = "CUKFFW98000AAA";
			mawb2.CM_MAWB = "111555555" + awbSerialNumberSuffix.ToString("0#");
			mawb2.CM_GB = branch2.PK;

			var cusAwb2 = SetupAwb(mawb2, mawb1.MasterLevelHouseHelper.Logs, string.Empty, ZDateTime.Empty, 10, ZDateTime.Now.AddDays(-8), "C");

			Factory.Save();

			var task = new CcsukMaintenanceServiceTask();
			InitialiseAndRunTaskSchedule(task);

			ReloadAndAssertArchived("S1 set 8 days ago, ARCHIVED", cusAwb1, true);
			ReloadAndAssertArchived("S1 set 8 days ago, ARCHIVED", cusAwb2, true);

			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(2, emails.Count);

			var firstEmail = emails.First(c => c.Recipients.Contains(staff1.GS_EmailAddress));

			AssertContains(cusAwb1.ReferenceNumberWithShed, firstEmail.Body);
			AssertContains(company1.CompanyName, firstEmail.Body);

			AssertNotContains(cusAwb2.ReferenceNumberWithShed, firstEmail.Body);
			AssertNotContains(company2.CompanyName, firstEmail.Body);

			var secondEmail = emails.First(c => c.Recipients.Contains(staff2.GS_EmailAddress));

			AssertNotContains(cusAwb1.ReferenceNumberWithShed, secondEmail.Body);
			AssertNotContains(company1.CompanyName, secondEmail.Body);

			AssertContains(cusAwb2.ReferenceNumberWithShed, secondEmail.Body);
			AssertContains(company2.CompanyName, secondEmail.Body);
		}

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestArchiveBasics()
		{
			TestArchiveAwbRunner(MakeBasic);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("&ControllerID=CcsukAirInventory&", email.Body);
			AssertContains(">-111-55555514</a></p></td><td>Not archived because NPR exceeds NPX.", email.Body);
			AssertContains(">-111-55555506</a></p></td><td>Status1 and final CAC date both older than 7 days", email.Body);
			AssertContains(">-111-55555510</a></p></td><td>Status1 date older than 7 days and SDC=C/E.", email.Body);
			AssertContains(">-111-55555512</a></p></td><td>NPX exceeds NPR and final CAC older than 180 days", email.Body);
			AssertContains(">-111-55555517</a></p></td><td>Pre-Arrival record on network older than 4 days.", email.Body);
			// Following AWBs not mentioned in the emailed report
			AssertEquals(false, email.Body.Contains("111-55555501"));
			AssertEquals(false, email.Body.Contains("111-55555502"));
			AssertEquals(false, email.Body.Contains("111-55555503"));
			AssertEquals(false, email.Body.Contains("111-55555504"));
			AssertEquals(false, email.Body.Contains("111-55555505"));
			AssertEquals(false, email.Body.Contains("111-55555507"));
			AssertEquals(false, email.Body.Contains("111-55555508"));
			AssertEquals(false, email.Body.Contains("111-55555509"));
			AssertEquals(false, email.Body.Contains("111-55555511"));
			AssertEquals(false, email.Body.Contains("111-55555513"));
			AssertEquals(false, email.Body.Contains("111-55555515"));
			AssertEquals(false, email.Body.Contains("111-55555516"));
			AssertEquals(false, email.Body.Contains("111-55555518"));
			var newFactoryToCheckEmailsReallyDidGetSaved = new BusinessObjectFactory();
			var emailLoadedFromDb = newFactoryToCheckEmailsReallyDidGetSaved.LoadTop1<MailItem>(new ZQuery());
			AssertEquals("CCSUK finalisation report", emailLoadedFromDb.MI_Subject);
		}

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestArchiveHouses()
		{
			TestArchiveAwbRunner(MakeHouse);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("&ControllerID=CcsukAirInventoryHouse&", email.Body);
			AssertContains(">-111-55555514-HAWB0014</a></p></td><td>Not archived because NPR exceeds NPX.", email.Body);
			AssertContains(">-111-55555506-HAWB0006</a></p></td><td>Status1 and final CAC date both older than 7 days", email.Body);
			AssertContains(">-111-55555510-HAWB0010</a></p></td><td>Status1 date older than 7 days and SDC=C/E.", email.Body);
			AssertContains(">-111-55555512-HAWB0012</a></p></td><td>NPX exceeds NPR and final CAC older than 180 days", email.Body);
			AssertContains(">-111-55555517-HAWB0017</a></p></td><td>Pre-Arrival record on network older than 4 days.", email.Body);
			// Following AWBs not mentioned in the emailed report
			AssertEquals(false, email.Body.Contains("HAWB0001"));
			AssertEquals(false, email.Body.Contains("HAWB0002"));
			AssertEquals(false, email.Body.Contains("HAWB0003"));
			AssertEquals(false, email.Body.Contains("HAWB0004"));
			AssertEquals(false, email.Body.Contains("HAWB0005"));
			AssertEquals(false, email.Body.Contains("HAWB0007"));
			AssertEquals(false, email.Body.Contains("HAWB0008"));
			AssertEquals(false, email.Body.Contains("HAWB0009"));
			AssertEquals(false, email.Body.Contains("HAWB0011"));
			AssertEquals(false, email.Body.Contains("HAWB0013"));
			AssertEquals(false, email.Body.Contains("HAWB0015"));
			AssertEquals(false, email.Body.Contains("HAWB0016"));
			AssertEquals(false, email.Body.Contains("HAWB0018"));
		}

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestArchiveSplitHouses()
		{
			TestArchiveAwbRunner(MakeSplitHouse);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("&ControllerID=CcsukSplitHouseController&", email.Body);
			AssertContains(">-111-55555514-HAWBXXXX/14</a></p></td><td>Not archived because NPR exceeds NPX.", email.Body);
			AssertContains(">-111-55555506-HAWBXXXX/06</a></p></td><td>Status1 and final CAC date both older than 7 days", email.Body);
			AssertContains(">-111-55555510-HAWBXXXX/10</a></p></td><td>Status1 date older than 7 days and SDC=C/E.", email.Body);
			AssertContains(">-111-55555512-HAWBXXXX/12</a></p></td><td>NPX exceeds NPR and final CAC older than 180 days", email.Body);
			AssertContains(">-111-55555517-HAWBXXXX/17</a></p></td><td>Pre-Arrival record on network older than 4 days.", email.Body);
			// Following AWBs not mentioned in the emailed report
			AssertEquals(false, email.Body.Contains("HAWBXXXX/01"));
			AssertEquals(false, email.Body.Contains("HAWBXXXX/02"));
			AssertEquals(false, email.Body.Contains("HAWBXXXX/03"));
			AssertEquals(false, email.Body.Contains("HAWBXXXX/04"));
			AssertEquals(false, email.Body.Contains("HAWBXXXX/05"));
			AssertEquals(false, email.Body.Contains("HAWBXXXX/07"));
			AssertEquals(false, email.Body.Contains("HAWBXXXX/08"));
			AssertEquals(false, email.Body.Contains("HAWBXXXX/09"));
			AssertEquals(false, email.Body.Contains("HAWBXXXX/11"));
			AssertEquals(false, email.Body.Contains("HAWBXXXX/13"));
			AssertEquals(false, email.Body.Contains("HAWBXXXX/15"));
			AssertEquals(false, email.Body.Contains("HAWBXXXX/16"));
			AssertEquals(false, email.Body.Contains("HAWBXXXX/18"));
		}

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestArchiveSplitBasics()
		{
			TestArchiveAwbRunner(MakeSplitBasic);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("&ControllerID=CcsukSplitBasicController&", email.Body);
			AssertContains(">-111-55555514/14</a></p></td><td>Not archived because NPR exceeds NPX.", email.Body);
			AssertContains(">-111-55555506/06</a></p></td><td>Status1 and final CAC date both older than 7 days", email.Body);
			AssertContains(">-111-55555510/10</a></p></td><td>Status1 date older than 7 days and SDC=C/E.", email.Body);
			AssertContains(">-111-55555512/12</a></p></td><td>NPX exceeds NPR and final CAC older than 180 days", email.Body);
			AssertContains(">-111-55555517/17</a></p></td><td>Pre-Arrival record on network older than 4 days.", email.Body);
			// Following AWBs not mentioned in the emailed report
			AssertEquals(false, email.Body.Contains("111-55555501/01"));
			AssertEquals(false, email.Body.Contains("111-55555502/02"));
			AssertEquals(false, email.Body.Contains("111-55555503/03"));
			AssertEquals(false, email.Body.Contains("111-55555504/04"));
			AssertEquals(false, email.Body.Contains("111-55555505/05"));
			AssertEquals(false, email.Body.Contains("111-55555507/07"));
			AssertEquals(false, email.Body.Contains("111-55555508/08"));
			AssertEquals(false, email.Body.Contains("111-55555506/09"));
			AssertEquals(false, email.Body.Contains("111-55555510/11"));
			AssertEquals(false, email.Body.Contains("111-55555510/13"));
			AssertEquals(false, email.Body.Contains("111-55555510/15"));
			AssertEquals(false, email.Body.Contains("111-55555510/16"));
			AssertEquals(false, email.Body.Contains("111-55555510/18"));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void SetupPostmaster()
		{
			var postmastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var currentUserInCurrentFactory = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUserInCurrentFactory.GS_EmailAddress = "yawn@soPointless.com";
			postmastersGroup.Staff.Add(currentUserInCurrentFactory);
		}

		void TestArchiveAwbRunner(MakeAndPrepareAirWaybill makeAndPrepareAirWaybill)
		{
			SetupPostmaster();

			var awbStillAwaitingPieces = makeAndPrepareAirWaybill("", ZDateTime.Empty, 0, ZDateTime.Empty); //111-55555501
			var awbAllArrivedToday = makeAndPrepareAirWaybill("", ZDateTime.Empty, 10, ZDateTime.Now); //111-55555502
			var awbAllArrived8daysAgoButNoCAC = makeAndPrepareAirWaybill("", ZDateTime.Empty, 10, ZDateTime.Now.AddDays(-8)); //111-55555503

			var awbAllArrivedYesterdayButRemovalApproved8DaysAgo = makeAndPrepareAirWaybill("CW", ZDateTime.Now.AddDays(-8), 10, ZDateTime.Now.AddDays(-1)); //111-55555504
			var awbAllArrived8DaysAgoButRemovalApprovedYesterday = makeAndPrepareAirWaybill("CW", ZDateTime.Now.AddDays(-1), 10, ZDateTime.Now.AddDays(-8)); //111-55555505
			var awbAllArrived8DaysAgoAndReleasedThenToo = makeAndPrepareAirWaybill("CW", ZDateTime.Now.AddDays(-8), 10, ZDateTime.Now.AddDays(-8)); //111-55555506

			var awbEuropeanEStillAwaitingPieces = makeAndPrepareAirWaybill("", ZDateTime.Empty, 0, ZDateTime.Empty, "E"); //111-55555507
			var awbEuropeanCStillAwaitingPieces = makeAndPrepareAirWaybill("", ZDateTime.Empty, 0, ZDateTime.Empty, "C"); //111-55555508
			var awbEuropeanCAllArrivedYesterday = makeAndPrepareAirWaybill("", ZDateTime.Empty, 10, ZDateTime.Now.AddDays(-1), "C"); //111-55555509
			var awbEuropeanCAllArrived8DaysAgo = makeAndPrepareAirWaybill("", ZDateTime.Empty, 10, ZDateTime.Now.AddDays(-8), "C"); //111-55555510

			var awbCustomsReleasedJustWithin6MonthsButStillAwaitingPieces = makeAndPrepareAirWaybill("CW", ZDateTime.Now.AddDays(-179), 1, ZDateTime.Empty); //111-55555511
			var awbCustomsReleasedOver6MonthsAgoButStillAwaitingPieces_FinalCAC = makeAndPrepareAirWaybill("CW", ZDateTime.Now.AddDays(-180), 1, ZDateTime.Empty); //111-55555512
			var awbCustomsReleasedOver6MonthsAgoButStillAwaitingPieces_TransientCAC = makeAndPrepareAirWaybill("CA", ZDateTime.Now.AddDays(-180), 1, ZDateTime.Empty); //111-55555513
			var awbCustomsReleasedOver6MonthsAgoTooManyPiecesReceived = makeAndPrepareAirWaybill("CW", ZDateTime.Now.AddDays(-180), 11, ZDateTime.Empty); //111-55555514

			var awbAlreadyArchivedShouldNotBeTouchedAgainEvenThoughItWouldBeACandidateForArchiving = makeAndPrepareAirWaybill("CW", ZDateTime.Now.AddDays(-8), 10, ZDateTime.Now.AddDays(-8)); //111-55555515
			awbAlreadyArchivedShouldNotBeTouchedAgainEvenThoughItWouldBeACandidateForArchiving.ArchiveOnCcsuk(ReasonForArchiving.TestingOnly);
			AssertEquals("PreReq - is already archived", true, awbAlreadyArchivedShouldNotBeTouchedAgainEvenThoughItWouldBeACandidateForArchiving.IsArchivedOnCcsuk);

			var preArrivalAwbThreeDaysOld = makeAndPrepareAirWaybill("", ZDateTime.Empty, 0, ZDateTime.Empty); //111-55555516
			preArrivalAwbThreeDaysOld.LocalCreationDate = ZDateTime.Now.AddDays(-3);
			var preArrivalAwbFiveDaysOld_OnNetwork = makeAndPrepareAirWaybill("", ZDateTime.Empty, 0, ZDateTime.Empty); //111-55555517
			preArrivalAwbFiveDaysOld_OnNetwork.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			preArrivalAwbFiveDaysOld_OnNetwork.LocalCreationDate = ZDateTime.Now.AddDays(-5);
			var preArrivalAwbFiveDaysOld_NotOnNetwork = makeAndPrepareAirWaybill("", ZDateTime.Empty, 0, ZDateTime.Empty); //111-55555518
			preArrivalAwbFiveDaysOld_NotOnNetwork.LocalCreationDate = ZDateTime.Now.AddDays(-5);

			var awbExcludeEventInAwb = makeAndPrepareAirWaybill("CW", ZDateTime.Now.AddDays(-180), 11, ZDateTime.Empty); //111-55555519
			((EnterpriseBusinessObject)awbExcludeEventInAwb).Logs.AddNew(AutoEvents.CustomisableEvent00);

			var awbExcludeEventInParent = makeAndPrepareAirWaybill("CW", ZDateTime.Now.AddDays(-180), 11, ZDateTime.Empty); //111-55555520
			LinkToParentAndAddLog(awbExcludeEventInParent);

			SetRegistry();

			Factory.Save();
			var task = new CcsukMaintenanceServiceTask();
			InitialiseAndRunTaskSchedule(task);
			ReloadAndAssertArchived("S1 not set, CAC blank - still open", awbStillAwaitingPieces, false);
			ReloadAndAssertArchived("S1 but no CAC - still open", awbAllArrivedToday, false);
			ReloadAndAssertArchived("S1 set long enough ago but no CAC - still open", awbAllArrived8daysAgoButNoCAC, false);
			ReloadAndAssertArchived("S1 is set and CAC set 8 days ago - still open", awbAllArrivedYesterdayButRemovalApproved8DaysAgo, false);
			ReloadAndAssertArchived("S1 set and CAC set, but CAC only set yesterday - still open", awbAllArrived8DaysAgoButRemovalApprovedYesterday, false);
			ReloadAndAssertArchived("S1 set and CAC set, both set over a week ago - ARCHIVED", awbAllArrived8DaysAgoAndReleasedThenToo, true);
			ReloadAndAssertArchived("S1 not set - still open", awbEuropeanEStillAwaitingPieces, false);
			ReloadAndAssertArchived("S1 not set - still open", awbEuropeanCStillAwaitingPieces, false);
			ReloadAndAssertArchived("S1 set yesterday, still open", awbEuropeanCAllArrivedYesterday, false);
			ReloadAndAssertArchived("S1 set 8 days ago, ARCHIVED", awbEuropeanCAllArrived8DaysAgo, true);
			ReloadAndAssertArchived("released 179 days ago but still awaiting S1 - still open", awbCustomsReleasedJustWithin6MonthsButStillAwaitingPieces, false);
			ReloadAndAssertArchived("Transient CAC, released 180 days ago but still awaiting S1, NPR<NPX - still open", awbCustomsReleasedOver6MonthsAgoButStillAwaitingPieces_TransientCAC, false);
			ReloadAndAssertArchived("Final CAC, released 180 days ago but still awaiting S1, NPR<NPX - ARCHIVED", awbCustomsReleasedOver6MonthsAgoButStillAwaitingPieces_FinalCAC, true);
			ReloadAndAssertArchived("released 180 days ago but still awaiting S1, NPR>NPX - still open", awbCustomsReleasedOver6MonthsAgoTooManyPiecesReceived, false);
			ReloadAndAssertArchived("Was already archived, assert still archived", awbAlreadyArchivedShouldNotBeTouchedAgainEvenThoughItWouldBeACandidateForArchiving, true);
			ReloadAndAssertArchived("Young prearrival is not touched", preArrivalAwbThreeDaysOld, false);
			ReloadAndAssertArchived("Old prearrival on network is archived", preArrivalAwbFiveDaysOld_OnNetwork, true);
			ReloadAndAssertArchived("Old prearrival not on network is untouched", preArrivalAwbFiveDaysOld_NotOnNetwork, false);
			ReloadAndAssertArchived("ARCHIVED due to specified excluded event in Awb", awbExcludeEventInAwb, true);
			ReloadAndAssertArchived("ARCHIVED due to specified excluded event in Parent", awbExcludeEventInParent, true);

			var logs = ((BusinessObject)awbAlreadyArchivedShouldNotBeTouchedAgainEvenThoughItWouldBeACandidateForArchiving).GetLogs();
			var logsOfBeingArchived = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentDeletedCode));
			AssertEquals("Should only have one event for archiving, not two, because after being archived it should not be touched again. AWB=" + awbAlreadyArchivedShouldNotBeTouchedAgainEvenThoughItWouldBeACandidateForArchiving.ReferenceNumber,
							1, logsOfBeingArchived.Length);
			AssertContains("testing", logsOfBeingArchived[0].SL_Reference);
		}

		void SetRegistry()
		{
			var settings = new ExcessNPRExclusionEventCodeSettingCollection();
			var setting = settings.AddNew();
			setting.EventCode = AutoEvents.CustomisableEvent00Code;
			GBCustomsDataRegistry.Instance.NPRExclusionEventCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		}

		void LinkToParentAndAddLog(ICcsukCusAwb cusAwb)
		{
			if (cusAwb is CusMAWB || cusAwb is SplitBasic)
			{
				var consol = Factory.New<ForwardingConsol>();
				if (cusAwb is CusMAWB)
				{
					((CusMAWB)cusAwb).CM_JK = consol.PK;
				}
				else
				{
					((CusMAWB)((SplitBasic)cusAwb).AWB).CM_JK = consol.PK;
				}
			}
			else
			{
				var shipment = Factory.New<ForwardingShipment>();
				if (cusAwb is CusHAWB)
				{
					((CusHAWB)cusAwb).CS_JS = shipment.PK;
				}
				else
				{
					((SplitHouse)cusAwb).HAWB.CS_JS = shipment.PK;
				}
			}
			cusAwb.ForwardingParent.Logs.AddNew(AutoEvents.CustomisableEvent00);
		}

		void ReloadAndAssertArchived(string message, ICcsukCusAwb awb, bool isArchivedExpected)
		{
			((BusinessObject)awb).Reload();
			AssertEquals(message, isArchivedExpected, awb.IsArchivedOnCcsuk);
		}

		ICcsukCusAwb MakeBasic(ZString customsActionCode, ZDateTime customsActionDate, ZShort npr, ZDateTime status1Date, string shipmentDescriptionCode = "T")
		{
			awbSerialNumberSuffix++;
			var basic = Factory.New<CusMAWB>();
			basic.Profile = "CUKFFW98000AAA";
			basic.CM_MAWB = "111555555" + awbSerialNumberSuffix.ToString("0#");
			return SetupAwb(basic, basic.MasterLevelHouseHelper.Logs, customsActionCode, customsActionDate, npr, status1Date, shipmentDescriptionCode);
		}

		ICcsukCusAwb MakeHouse(ZString customsActionCode, ZDateTime customsActionDate, ZShort npr, ZDateTime status1Date, string shipmentDescriptionCode = "T")
		{
			awbSerialNumberSuffix++;
			awbChildSerialNumberSuffix++;
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "111555555" + awbSerialNumberSuffix.ToString("0#");
			mawb.Profile = "CUKFFW98000AAA";
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HAWB00" + awbChildSerialNumberSuffix.ToString("0#");
			return SetupAwb(hawb, hawb.Logs, customsActionCode, customsActionDate, npr, status1Date, shipmentDescriptionCode);
		}

		ICcsukCusAwb MakeSplitHouse(ZString customsActionCode, ZDateTime customsActionDate, ZShort npr, ZDateTime status1Date, string shipmentDescriptionCode = "T")
		{
			awbSerialNumberSuffix++;
			awbChildSerialNumberSuffix++;
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "111555555" + awbSerialNumberSuffix.ToString("0#");
			mawb.Profile = "CUKFFW98000AAA";
			var hawb = mawb.ChildBills.AddNew();
			var splitHouse = (SplitHouse)hawb.Splits.AddNew();
			hawb.CS_HAWB = "HAWBXXXX";
			splitHouse.SplitReference = awbChildSerialNumberSuffix.ToString("0#");
			return SetupAwb(splitHouse, splitHouse.Logs, customsActionCode, customsActionDate, npr, status1Date, shipmentDescriptionCode);
		}

		ICcsukCusAwb MakeSplitBasic(ZString customsActionCode, ZDateTime customsActionDate, ZShort npr, ZDateTime status1Date, string shipmentDescriptionCode = "T")
		{
			awbSerialNumberSuffix++;
			awbChildSerialNumberSuffix++;
			var basic = Factory.New<CusMAWB>();
			basic.CM_MAWB = "111555555" + awbSerialNumberSuffix.ToString("0#");
			basic.Profile = "CUKFFW98000AAA";
			var splitBasic = (SplitBasic)basic.Splits.AddNew();
			splitBasic.SplitReference = awbChildSerialNumberSuffix.ToString("0#");
			return SetupAwb(splitBasic, splitBasic.Logs, customsActionCode, customsActionDate, npr, status1Date, shipmentDescriptionCode);
		}

		ICcsukCusAwb SetupAwb(ICcsukCusAwb awb, Logs logs, ZString customsActionCode, ZDateTime customsActionDate, ZShort npr, ZDateTime status1Date, string shipmentDescriptionCode)
		{
			if (!customsActionCode.IsEmpty && !customsActionDate.IsEmpty)
			{
				awb.SetCustomsActionCode(customsActionCode, customsActionDate);
			}
			awb.ShipmentDescriptionCode = shipmentDescriptionCode;
			awb.NumberOfPiecesExpected = 10;
			awb.NumberOfPiecesReceived = npr;
			if (awb.NumberOfPiecesExpected == awb.NumberOfPiecesReceived && !status1Date.IsEmpty)
			{
				awb.Status1Date = status1Date;
			}
			var ot = awb.Factory.New<CusOutTurn>();
			ot.C5_PackagesOutturned = npr;
			awb.OutTurns.Add(ot);
			if (!status1Date.IsEmpty)
			{
				var existing = (from StmALog l in logs.GetAllLogs() where l.SL_SE_NKEvent == Events.StatusChangeCode && l.SL_Reference == "ST1" select l).FirstOrDefault();
				if (existing == null)
				{
					logs.AddNew(Events.StatusChange, "ST1", status1Date.ToOffset());
				}
				else
				{
					using (existing.LockForUpdatingKeyFieldsForTesting())
					{
						existing.SL_EventTime = status1Date;
					}
				}
			}
			return awb;
		}

		int awbSerialNumberSuffix;
		int awbChildSerialNumberSuffix;
	}
}
