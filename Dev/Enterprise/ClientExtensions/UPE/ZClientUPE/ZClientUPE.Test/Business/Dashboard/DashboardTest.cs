using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(Dashboard))]
	public class DashboardTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2006, 8, 1)] // Not Monday
		public void TestRefresh_Admin_Past()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			Factory.Save();
			using (branch.SetAsTemporaryContext())
			using (ClientHookLoader.Instance.OverrideClientHookForTest(null, true))
			{
				CreateCargoReportForNonUPEBranches(YesterdaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Hold, 1);
				CreateCargoReportForNonUPEBranches(YesterdaysDate, CargoReportQueueCodeDescriptionPairList.Codes.EIR, 2);
				CreateCargoReportForNonUPEBranches(YesterdaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Unknown, 3);
				CreateCargoReportForNonUPEBranches(YesterdaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Intervention, 4);
				CreateCargoReportForNonUPEBranches(YesterdaysDate, CargoReportQueueCodeDescriptionPairList.Codes.AwaitingEvaluation, 5);
				CreateCargoReportForNonUPEBranches(YesterdaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, 6);
			}

			CreateCargoReport(YesterdaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Hold, 1);
			CreateCargoReport(YesterdaysDate, CargoReportQueueCodeDescriptionPairList.Codes.EIR, 2);
			CreateCargoReport(YesterdaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Unknown, 3);
			CreateCargoReport(YesterdaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Intervention, 4);
			CreateCargoReport(YesterdaysDate, CargoReportQueueCodeDescriptionPairList.Codes.AwaitingEvaluation, 5);
			CreateCargoReport(YesterdaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, 6);
			AssertAdminPastArrivals("1", "3", "2", "3", "7", "11");
			using (branch.SetAsTemporaryContext())
			using (ClientHookLoader.Instance.OverrideClientHookForTest(null, true))
			{
				CreateDeclarationForNonUPEBranches(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.EIR, "", 3);
				CreateDeclarationForNonUPEBranches(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, "", 4);
				CreateDeclarationForNonUPEBranches(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes._34_Missort, 5);
				CreateDeclarationForNonUPEBranches(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, 6);
				CreateDeclarationForNonUPEBranches(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments, 7);
				CreateDeclarationForNonUPEBranches(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, 8);
				CreateDeclarationForNonUPEBranches(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, 9);
				CreateDeclarationForNonUPEBranches(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold, 10);
				CreateDeclarationForNonUPEBranches(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SS_CustomsHold, 11);
			}

			CreateDeclaration(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.EIR, "", 3);
			CreateDeclaration(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, "", 4);
			CreateDeclaration(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes._34_Missort, 5);
			CreateDeclaration(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, 6);
			CreateDeclaration(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments, 7);
			CreateDeclaration(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, 8);
			CreateDeclaration(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, 9);
			CreateDeclaration(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold, 10);
			CreateDeclaration(YesterdaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SS_CustomsHold, 11);
			AssertAdminPastArrivals("1", "7", "7", "3", "28", "53");
		}

		[TestDate(2006, 10, 5)]
		public void TestPiecesFromCancelledJobDeclarationAreNotCounted()
		{
			UPEJobDeclaration uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.JE_DateOfArrival = new ZDateTime(2006, 10, 3);
			uPEJobDeclaration.JE_TotalNoOfPieces = 5;
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = DeclarationQueueCodeDescriptionPairList.Codes.Lodgement;
			uPEJobDeclaration.CurrentQueue.P4_CustomsStatus = "";
			Factory.Save();
			Dashboard.Refresh();
			AssertEquals("1", Dashboard.OpsBrokersPastArrivalShipments);
			AssertEquals("5", Dashboard.OpsBrokersPastArrivalPieces);
			uPEJobDeclaration.JE_IsCancelled = ZBool.True;
			Factory.Save();
			Dashboard.Refresh();
			AssertEquals("0", Dashboard.OpsBrokersPastArrivalShipments);
			AssertEquals("0", Dashboard.OpsBrokersPastArrivalPieces);
		}

		[TestDate(2006, 8, 1)] // Not Monday
		public void TestRefresh_Admin_Today()
		{
			CreateCargoReport(TodaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Hold, 0);
			CreateCargoReport(TodaysDate, CargoReportQueueCodeDescriptionPairList.Codes.EIR, 1);
			CreateCargoReport(TodaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Unknown, 2);
			CreateCargoReport(TodaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Intervention, 3);
			CreateCargoReport(TodaysDate, CargoReportQueueCodeDescriptionPairList.Codes.AwaitingEvaluation, 4);
			CreateCargoReport(TodaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, 5);
			AssertAdminTodaysArrivals("1", "3", "2", "2", "4", "9");
			CreateDeclaration(TodaysDate, DeclarationQueueCodeDescriptionPairList.Codes.EIR, "", 0);
			CreateDeclaration(TodaysDate, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, "", 1);
			CreateDeclaration(TodaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes._34_Missort, 2);
			CreateDeclaration(TodaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, 3);
			CreateDeclaration(TodaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments, 4);
			CreateDeclaration(TodaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, 5);
			CreateDeclaration(TodaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, 6);
			CreateDeclaration(TodaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold, 7);
			CreateDeclaration(TodaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SS_CustomsHold, 8);
			AssertAdminTodaysArrivals("1", "7", "7", "2", "13", "36");
		}

		[TestDate(2006, 8, 1)] // Not Monday
		public void TestRefreshAdmin_Future()
		{
			CreateCargoReport(TommorrowsDate, CargoReportQueueCodeDescriptionPairList.Codes.Hold, 6);
			CreateCargoReport(TommorrowsDate, CargoReportQueueCodeDescriptionPairList.Codes.EIR, 5);
			CreateCargoReport(TommorrowsDate, CargoReportQueueCodeDescriptionPairList.Codes.Unknown, 4);
			CreateCargoReport(TommorrowsDate, CargoReportQueueCodeDescriptionPairList.Codes.Intervention, 3);
			CreateCargoReport(TommorrowsDate, CargoReportQueueCodeDescriptionPairList.Codes.AwaitingEvaluation, 2);
			CreateCargoReport(TommorrowsDate, CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, 1);
			AssertAdminFutureArrivals("1", "3", "2", "4", "14", "3");
			CreateDeclaration(TommorrowsDate, DeclarationQueueCodeDescriptionPairList.Codes.EIR, "", 9);
			CreateDeclaration(TommorrowsDate, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, "", 8);
			CreateDeclaration(TommorrowsDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes._34_Missort, 7);
			CreateDeclaration(TommorrowsDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, 6);
			CreateDeclaration(TommorrowsDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments, 5);
			CreateDeclaration(TommorrowsDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, 4);
			CreateDeclaration(TommorrowsDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, 3);
			CreateDeclaration(TommorrowsDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold, 2);
			CreateDeclaration(TommorrowsDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SS_CustomsHold, 1);
			AssertAdminFutureArrivals("1", "7", "7", "4", "41", "21");
		}

		[TestDate(2006, 8, 1)] // Not Monday
		public void TestRefresh_OPS_Past()
		{
			SetupOps(YesterdaysDate, 1, 0, 4, 6, 5, 9, 6, 7, 1, 9, 15, 1, 15, 18, 11, 16, 15, 12, 15, 24, 3, 4, 25, 9, 7, 4, 3, 4, 0, 5, 1);
			AssertOpsPastArrivals("29", "241", "2", "14");
		}

		[TestDate(2006, 8, 1)] // Not Monday
		public void TestRefresh_OPS_Today()
		{
			SetupOps(TodaysDate, 11, 3, 0, 2, 2, 10, 5, 1, 2, 10, 10, 14, 2, 3, 1, 5, 5, 15, 19, 4, 1, 8, 23, 4, 9, 3, 8, 0, 8, 2, 6);
			AssertOpsTodayArrivals("29", "184", "2", "12");
		}

		[TestDate(2006, 8, 1)] // Not Monday
		public void TestRefresh_OPS_Future()
		{
			SetupOps(TommorrowsDate, 18, 6, 4, 0, 9, 9, 10, 5, 1, 2, 10, 10, 14, 2, 3, 4, 6, 15, 1, 17, 4, 5, 9, 15, 1, 7, 4, 4, 8, 9, 5);
			AssertOpsFutureArrivals("29", "199", "2", "18");
		}

		[TestDate(2006, 7, 31)]
		public void TestTodaysAndPastArrivalsWhenTodayIsMonday()
		{
			ZDateTime sunday = YesterdaysDate;
			ZDateTime saturday = sunday.AddDays(-1);
			ZDateTime friday = saturday.AddDays(-1);
			ZDateTime thursday = friday.AddDays(-1);
			CreateCargoReport(TodaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Hold, 0);
			CreateCargoReport(sunday, CargoReportQueueCodeDescriptionPairList.Codes.EIR, 1);
			CreateCargoReport(friday, CargoReportQueueCodeDescriptionPairList.Codes.Unknown, 2);
			CreateCargoReport(saturday, CargoReportQueueCodeDescriptionPairList.Codes.Intervention, 3);
			CreateCargoReport(TodaysDate, CargoReportQueueCodeDescriptionPairList.Codes.AwaitingEvaluation, 4);
			CreateCargoReport(friday, CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, 5);
			CreateCargoReport(thursday, CargoReportQueueCodeDescriptionPairList.Codes.Unknown, 10);
			CreateDeclaration(thursday, DeclarationQueueCodeDescriptionPairList.Codes.EIR, "", 1);
			CreateDeclaration(saturday, DeclarationQueueCodeDescriptionPairList.Codes.EIR, "", 0);
			CreateDeclaration(sunday, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, "", 1);
			CreateDeclaration(thursday, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, "", 5);
			CreateDeclaration(friday, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes._34_Missort, 2);
			CreateDeclaration(saturday, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, 3);
			CreateDeclaration(sunday, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments, 4);
			CreateDeclaration(TodaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, 5);
			CreateDeclaration(friday, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, 6);
			CreateDeclaration(saturday, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold, 7);
			CreateDeclaration(sunday, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SS_CustomsHold, 8);
			AssertAdminTodaysArrivals("0", "6", "5", "0", "11", "25");
			AssertAdminPastArrivals("2", "2", "3", "12", "3", "16");
		}

		[TestDate(2006, 7, 31)]
		public void TestTodaysAndPastArrivalsWhenTodayIsMonday_HolidayOnFriday()
		{
			ZDateTime sunday = YesterdaysDate;
			ZDateTime saturday = sunday.AddDays(-1);
			ZDateTime friday = saturday.AddDays(-1);
			ZDateTime thursday = friday.AddDays(-1);
			GlbHoliday holiday = Factory.New<GlbHoliday>();
			holiday.GH_Date = friday;
			holiday.GH_ParentTableCode = GlbBranchSchema.Constants.Prefix;
			holiday.GH_ParentID = GlbBranch.CurrentBranch.PK;
			holiday.GH_HolidayName = "UPS Express Holiday";
			holiday.GH_Recurring = ZBool.False;
			CreateCargoReport(TodaysDate, CargoReportQueueCodeDescriptionPairList.Codes.Hold, 0);
			CreateCargoReport(sunday, CargoReportQueueCodeDescriptionPairList.Codes.EIR, 1);
			CreateCargoReport(friday, CargoReportQueueCodeDescriptionPairList.Codes.Unknown, 2);
			CreateCargoReport(saturday, CargoReportQueueCodeDescriptionPairList.Codes.Intervention, 3);
			CreateCargoReport(TodaysDate, CargoReportQueueCodeDescriptionPairList.Codes.AwaitingEvaluation, 4);
			CreateCargoReport(friday, CargoReportQueueCodeDescriptionPairList.Codes.Quarantine, 5);
			CreateCargoReport(thursday, CargoReportQueueCodeDescriptionPairList.Codes.Unknown, 10);
			CreateDeclaration(thursday, DeclarationQueueCodeDescriptionPairList.Codes.EIR, "", 1);
			CreateDeclaration(saturday, DeclarationQueueCodeDescriptionPairList.Codes.EIR, "", 0);
			CreateDeclaration(sunday, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, "", 1);
			CreateDeclaration(thursday, DeclarationQueueCodeDescriptionPairList.Codes.CustomsBonding, "", 5);
			CreateDeclaration(friday, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes._34_Missort, 2);
			CreateDeclaration(saturday, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.B5_ClientRegistration, 3);
			CreateDeclaration(sunday, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments, 4);
			CreateDeclaration(TodaysDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.E8_QuarantineHold, 5);
			CreateDeclaration(friday, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.ER_CustomsInspection, 6);
			CreateDeclaration(saturday, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SN_PersonalEffectsHold, 7);
			CreateDeclaration(sunday, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SS_CustomsHold, 8);
			AssertAdminTodaysArrivals("1", "7", "7", "2", "13", "36");
			AssertAdminPastArrivals("1", "1", "1", "10", "1", "5");
		}

		void SetupOps(ZDateTime arrivalDate, params int[] pieces)
		{
			if (pieces.Length == 31)
			{
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Pending, "", pieces[0]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Unknown, "", pieces[1]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Classification, "", pieces[2]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Compiling, "", pieces[3]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Lodgement, "", pieces[4]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Submitted, "", pieces[5]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment, pieces[6]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes._R0_Rebill, pieces[7]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes._R1_Abandon, pieces[8]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes._R2_Transhipment, pieces[9]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes._R3_RTS, pieces[10]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes._R4_FreeDomicile, pieces[11]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.AM_RefusedCancelledOrder, pieces[12]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.AN_RefusedDuplicateOrder, pieces[13]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.AS_RefusedShippedTooLate, pieces[14]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.BA_InadequateDescription, pieces[15]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.BK_CertificateOfOriginRequired, pieces[16]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.BP_PhoneNumberMissing, pieces[17]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment, pieces[18]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.DN_SplitShipment, pieces[19]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.FE_DutyTaxRefused, pieces[20]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.FF_RTSAuthorisationRequired, pieces[21]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.NR_FreeDomicile, pieces[22]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.NY_MissingInvoice, pieces[23]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.RJ_ShipSparesUnderbondsTranshipments, pieces[24]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.S1_ShipperConsigneeDetailsInsufficient, pieces[25]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.SR_AwaitingRelease, pieces[26]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.UD_AlternateDeliveryAddress, pieces[27]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration, pieces[28]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.XH_DocumentInsufficient, pieces[29]);
				CreateDeclaration(arrivalDate, DeclarationQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.XN_PhoneNumberInvalid, pieces[30]);
			}
			else
			{
				throw new DeveloperNotificationException("Please enter 31 int parameters for the bashboard Ops tests");
			}
		}

		#region AssertAdminArrivals
		void AssertAdminPastArrivals(string noStatusShipment, string heldShipment, string houseShipment, string noStatusPieces, string heldPieces, string housePieces)
		{
			Factory.Save();
			Dashboard.Refresh();
			AssertAdminArrivals(noStatusShipment, heldShipment, houseShipment, noStatusPieces, heldPieces, housePieces, Dashboard.AdminNoStatusPastArrivalShipments, Dashboard.AdminHeldPastArrivalShipments, Dashboard.AdminHousePastArrivalShipments, Dashboard.AdminNoStatusPastArrivalPieces, Dashboard.AdminHeldPastArrivalPieces, Dashboard.AdminHousePastArrivalPieces);
		}

		void AssertAdminTodaysArrivals(string noStatusShipment, string heldShipment, string houseShipment, string noStatusPieces, string heldPieces, string housePieces)
		{
			Factory.Save();
			Dashboard.Refresh();
			AssertAdminArrivals(noStatusShipment, heldShipment, houseShipment, noStatusPieces, heldPieces, housePieces, Dashboard.AdminNoStatusTodayArrivalShipments, Dashboard.AdminHeldTodayArrivalShipments, Dashboard.AdminHouseTodayArrivalShipments, Dashboard.AdminNoStatusTodayArrivalPieces, Dashboard.AdminHeldTodayArrivalPieces, Dashboard.AdminHouseTodayArrivalPieces);
		}

		void AssertAdminFutureArrivals(string noStatusShipment, string heldShipment, string houseShipment, string noStatusPieces, string heldPieces, string housePieces)
		{
			Factory.Save();
			Dashboard.Refresh();
			AssertAdminArrivals(noStatusShipment, heldShipment, houseShipment, noStatusPieces, heldPieces, housePieces, Dashboard.AdminNoStatusFutureArrivalShipments, Dashboard.AdminHeldFutureArrivalShipments, Dashboard.AdminHouseFutureArrivalShipments, Dashboard.AdminNoStatusFutureArrivalPieces, Dashboard.AdminHeldFutureArrivalPieces, Dashboard.AdminHouseFutureArrivalPieces);
		}

		void AssertAdminArrivals(string noStatusShipment, string heldShipment, string houseShipment, string noStatusPieces, string heldPieces, string housePieces, string dashBoardNoStatusShipment, string dashBoardHeldShipment, string dashBoardHouseShipment, string dashBoardNoStatusPieces, string dashBoardHeldPieces, string dashBoardHousePieces)
		{
			AssertEquals(noStatusShipment, dashBoardNoStatusShipment);
			AssertEquals(noStatusPieces, dashBoardNoStatusPieces);
			AssertEquals(heldShipment, dashBoardHeldShipment);
			AssertEquals(heldPieces, dashBoardHeldPieces);
			AssertEquals(houseShipment, dashBoardHouseShipment);
			AssertEquals(housePieces, dashBoardHousePieces);
		}

		#endregion
		#region AssertOpsArrivals
		void AssertOpsArrivals(string classifiersDecArrivals, string classifiersPiecesArrivals, string brkDecArrivals, string brkPiecesArrivals, string dashBoardClassifiersDecArrivals, string dashBoardClassifiersPiecessArrivals, string dashBoardBrkDecArrivals, string dashBoardBrkPiecesArrivals)
		{
			AssertEquals(classifiersDecArrivals, dashBoardClassifiersDecArrivals);
			AssertEquals(classifiersPiecesArrivals, dashBoardClassifiersPiecessArrivals);
			AssertEquals(brkDecArrivals, dashBoardBrkDecArrivals);
			AssertEquals(brkPiecesArrivals, dashBoardBrkPiecesArrivals);
		}

		void AssertOpsPastArrivals(string classifiersDecArrivals, string classifiersPiecesArrivals, string brkDecArrivals, string brkPiecesArrivals)
		{
			Factory.Save();
			Dashboard.Refresh();
			AssertOpsArrivals(classifiersDecArrivals, classifiersPiecesArrivals, brkDecArrivals, brkPiecesArrivals, Dashboard.OpsClassifiersPastArrivalShipments, Dashboard.OpsClassifiersPastArrivalPieces, Dashboard.OpsBrokersPastArrivalShipments, Dashboard.OpsBrokersPastArrivalPieces);
		}

		void AssertOpsTodayArrivals(string classifiersDecArrivals, string classifiersPiecesArrivals, string brkDecArrivals, string brkPiecesArrivals)
		{
			Factory.Save();
			Dashboard.Refresh();
			AssertOpsArrivals(classifiersDecArrivals, classifiersPiecesArrivals, brkDecArrivals, brkPiecesArrivals, Dashboard.OpsClassifiersTodayArrivalShipments, Dashboard.OpsClassifiersTodayArrivalPieces, Dashboard.OpsBrokersTodayArrivalShipments, Dashboard.OpsBrokersTodayArrivalPieces);
		}

		void AssertOpsFutureArrivals(string classifiersDecArrivals, string classifiersPiecesArrivals, string brkDecArrivals, string brkPiecesArrivals)
		{
			Factory.Save();
			Dashboard.Refresh();
			AssertOpsArrivals(classifiersDecArrivals, classifiersPiecesArrivals, brkDecArrivals, brkPiecesArrivals, Dashboard.OpsClassifiersFutureArrivalShipments, Dashboard.OpsClassifiersFutureArrivalPieces, Dashboard.OpsBrokersFutureArrivalShipments, Dashboard.OpsBrokersFutureArrivalPieces);
		}

		#endregion
		void CreateCargoReport(ZDateTime arrivalDate, string queue, ZInt pieceCount)
		{
			UPECusMAWB uPECusMAWB = Factory.New<UPECusMAWB>();
			uPECusMAWB.CM_ArrivalDate = arrivalDate;
			UPECusHAWB uPECusHAWB = Factory.New<UPECusHAWB>();
			uPECusHAWB.CS_CM = uPECusMAWB.PK;
			uPECusHAWB.CS_PiecesLanded = (ZShort)pieceCount;
			uPECusHAWB.CurrentQueue.P4_CustomsQueue = queue;
		}

		void CreateCargoReportForNonUPEBranches(ZDateTime arrivalDate, string queue, ZInt pieceCount)
		{
			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_ArrivalDate = arrivalDate;
			CusHAWB cusHAWB = Factory.New<CusHAWB>();
			cusHAWB.CS_CM = cusMAWB.PK;
			cusHAWB.CS_PiecesLanded = (ZShort)pieceCount;
			cusHAWB.CurrentQueue.P4_CustomsQueue = queue;
		}

		void CreateDeclaration(ZDateTime arrivalDate, string queue, string reason, ZInt pieceCount)
		{
			UPEJobDeclaration uPEJobDeclaration = Factory.New<UPEJobDeclaration>();
			uPEJobDeclaration.JE_DateOfArrival = arrivalDate;
			uPEJobDeclaration.JE_TotalNoOfPieces = pieceCount;
			uPEJobDeclaration.CurrentQueue.P4_CustomsQueue = queue;
			uPEJobDeclaration.CurrentQueue.P4_CustomsStatus = reason;
		}

		void CreateDeclarationForNonUPEBranches(ZDateTime arrivalDate, string queue, string reason, ZInt pieceCount)
		{
			JobDeclaration jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_DateOfArrival = arrivalDate;
			jobDeclaration.JE_TotalNoOfPieces = pieceCount;
			jobDeclaration.CurrentQueue.P4_CustomsQueue = queue;
			jobDeclaration.CurrentQueue.P4_CustomsStatus = reason;
		}

		#region Setup
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			Dashboard = new Dashboard(Factory);
			YesterdaysDate = ZDateTime.Now.AddDays(-1);
			TodaysDate = ZDateTime.Now;
			TommorrowsDate = ZDateTime.Now.AddDays(1);
		}

		Dashboard Dashboard;
		ZDateTime YesterdaysDate;
		ZDateTime TodaysDate;
		ZDateTime TommorrowsDate;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Dashboard(Factory);
		}
		#endregion
	}
}
