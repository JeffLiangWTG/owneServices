using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	internal class CcsukMenuTests : TestCaseWithFactory
	{
		public void TestCheckinAllPiecesMenu()
		{
			SetupAndAssertCheckinAllPiecesMenu(resultExpected: true, registryValue: true, status1Date: ZDate.Empty);
			SetupAndAssertCheckinAllPiecesMenu(resultExpected: false, registryValue: false, status1Date: ZDate.Empty);
			SetupAndAssertCheckinAllPiecesMenu(resultExpected: false, registryValue: true, status1Date: ZDate.Today);
			SetupAndAssertCheckinAllPiecesMenu(resultExpected: false, registryValue: true, status1Date: ZDate.Empty, isBasic: true);
			SetupAndAssertCheckinAllPiecesMenu(resultExpected: false, registryValue: true, status1Date: ZDate.Empty, presenceOnNetworkStatus: "NO");
			SetupAndAssertCheckinAllPiecesMenu(resultExpected: false, registryValue: true, status1Date: ZDate.Empty, presenceOnNetworkStatus: "DEL");
			SetupAndAssertCheckinAllPiecesMenu(resultExpected: false, registryValue: true, status1Date: ZDate.Empty, profile: "CUKFFW98999");
			SetupAndAssertCheckinAllPiecesMenu(resultExpected: false, registryValue: true, status1Date: ZDate.Empty, isInDatabase: false);
		}

		void SetupAndAssertCheckinAllPiecesMenu(
			bool resultExpected,
			bool registryValue,
			ZDate status1Date,
			bool isBasic = false,
			string presenceOnNetworkStatus = "YES",
			string profile = "CUKAIR98999",
			bool isInDatabase = true)
		{
			using (var menu = GetNewMenu(true))
			{
				var mawb = awb as CusMAWB;
				GBCustomsDataRegistry.Instance.CcsukAllowCheckinAtMawbLevel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
				mawb.Status1Date = status1Date;
				mawb.PresenceOnNetworkStatus = presenceOnNetworkStatus;
				mawb.Profile = profile;
				if (isInDatabase)
				{
					Factory.Save();
				}
				if (!isBasic)
				{
					mawb.ChildBills.AddNew();
				}

				AssertEquals(GBCustomsDataRegistry.Instance.CcsukAllowCheckinAtMawbLevel.Value, registryValue);
				AssertEquals(mawb.Status1Date, status1Date);
				AssertEquals(mawb.IsInDatabase, isInDatabase);
				AssertEquals(mawb.IsBasic, isBasic);
				AssertEquals(mawb.PresenceOnNetworkStatus, presenceOnNetworkStatus);
				AssertEquals(mawb.Profile, profile);
				menu.RefreshMenu();
				AssertEquals(resultExpected, menu.MenuItems.FindByText("Check in all pieces") != null);
			}
		}

		const int NumberOfCcsukMenuItemsExpected = 7;

		public void TestOnlyCcsukOptionsAreShownAndNoBaseOnes()
		{
			using (var menu = GetNewMenu(false))
			{
				menu.RefreshMenu();
				AssertEquals(1, menu.MenuItems.Count);
				AssertContains("Please first create", menu.MenuItems[0].Text);
			}
			using (var menu = GetNewMenu(true))
			{
				var basic = awb as CusMAWB;
				basic.Profile = "CUKFFW98000LXA";
				menu.RefreshMenu();
				AssertEquals(NumberOfCcsukMenuItemsExpected, menu.MenuItems.Count);
				basic.Profile = "CUKAIR98LHRBAC";
				menu.RefreshMenu();
				AssertEquals("No FRN and no CHIEF menu for sheds", NumberOfCcsukMenuItemsExpected - 2 + 1, menu.MenuItems.Count);
			}
		}

		public void TestPressingDeleteSendsFrxAndFsr()
		{
			GBCustomsDataRegistry.Instance.CcsukAlsoSendFsrAfterFrx.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			MawbTestHelper.MakeBadge("AAA", "CCSUK", true);
			using (var menu = GetNewMenu(true))
			{
				menu.SendsMessagesToCustoms = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
				awb.ShipmentDescriptionCode = "T";
				((CusMAWB)awb).Profile = "CUKFFW98000AAA";
				((CusMAWB)awb).CM_MAWB = "12312345678";
				awb.NumberOfPiecesExpected = 1;
				menu.RefreshMenu();
				var menuItem = menu.MenuItems.FindByText("Delete consignment (send FRX)", true);
				menuItem.PerformClick();
				AssertEquals(2, awb.Messages.Count);
				AssertContains("FRX", awb.Messages[0].EM_MessageText);
				AssertContains("FSR", awb.Messages[1].EM_MessageText);
			}
		}

		public void TestShowDeactivateOnlyToWtgAndWithRegistry()
		{
			using (var menu = GetNewMenu(true))
			{
				var basic = awb as CusMAWB;
				basic.Profile = "CUKFFW98000LXA";
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FRI", "FSR", "FRN", "FRX", "FRC", "CDS", "Contact" });

				GBCustomsDataRegistry.Instance.CcsukTemporarilyAllowDeactivationOfCcsukAwbs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FRI", "FSR", "FRN", "FRX", "FRC", "CDS", "Contact", "Deactivate" });

				GlbStaff.CurrentUser.GS_LoginName = "John Locke";
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FRI", "FSR", "FRN", "FRX", "FRC", "CDS", "Contact" });
			}
		}

		public void TestMenuItemsSimple()
		{
			using (var menu = GetNewMenu(true))
			{
				var basic = awb as CusMAWB;
				basic.Profile = "CUKFFW98000LXA";

				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FRI", "FSR", "FRN", "FRX", "FRC", "CDS", "Contact" });

				var fsrMenu = menu.MenuItems[1];
				RunItemsTest(fsrMenu, new string[] { "To community", "To shed", "update", "specifying", "FSN" });

				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FSR", "FRN", "FRX", "FRC", "split", "CDS", "Contact" }); // No FRI - it's known to be on  the network, but split menu now visible

				awb.ShipmentDescriptionCode = "C";
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FSR", "FRN", "FRX", "FRC", "Contact" }); // No CHIEF for C-status
				awb.ShipmentDescriptionCode = "T";

				awb.CreateNewStandaloneCDSDeclaration();
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FSR", "FRN", "FRX", "FRC", "split", "Contact" }); // No Chief.  
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NotOnCommDb;
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FRI", "FSR", "FRN", "FRC", "Contact" }); // FRI again, but no FRX or split
			}
		}

		public void TestMenuItemsShedWithOwnAgentNominatedAllowsChief()
		{
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CAR", "CUKFFW98000CAR", appendToExistingCredsAndBadges: true);
			CcsukInventoryBusinessObjectMessageSenderTests.MakeCcsukBadgeAndCredential("CAX", "CUKAIR98LHRCAX", appendToExistingCredsAndBadges: true);

			using (var menu = GetNewMenu(true))
			{
				var basic = awb as CusMAWB;
				basic.Profile = "CUKAIR98LHRCAX";
				basic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
				basic.AgentBadge = "CAR";
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FSR", "FRX", "FRC", "splits", "CDS", "Print Release/Removal Authority", "Contact" });
				basic.AgentBadge = "DJC";
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FSR", "FRX", "FRC", "splits", "Print Release/Removal Authority", "Contact" });
				basic.AgentBadge = "CAR";
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FSR", "FRX", "FRC", "splits", "CDS", "Print Release/Removal Authority", "Contact" });
			}
		}

		public void TestMenuItemsSplitWithEntry_CDS()
		{
			using (var menu = GetNewMenu(true))
			{
				var basic = (CusMAWB)awb;
				basic.Profile = "CUKFFW98000LXA";

				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new[] { "FRI", "FSR", "FRN", "FRX", "FRC", "CDS", "Contact" });

				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
				awb.ShipmentDescriptionCode = "T";
				var dec = awb.CreateNewStandaloneCDSDeclaration();
				dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new[] { "FSR", "FRN", "FRX", "FRC", "split", "Contact" }); // No Chief.

				dec.Factory.Save(); // generate DUCR
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new[] { "FSR", "FRN", "FRX", "FRC", "split", "Contact" }); // No Chief.

				var entry = dec.CustomsEntryHeaders.AddNew();
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new[] { "FSR", "FRN", "FRX", "FRC", "split", "Contact" }); // No Chief.

				GlbStaff.CurrentUser.GS_LoginName = "John Locke";
				GlbStaff.CurrentUser.GS_IsController = false;
				entry.EntryNumber = "120-123456";
				entry.CH_EntryStatus = "CAN";
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new[] { "FSR", "FRN", "FRX", "FRC", "Contact" }); // No split

				GlbStaff.CurrentUser.GS_LoginName = "CWSupport";
				GlbStaff.CurrentUser.GS_IsController = true;
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new[] { "FSR", "FRN", "FRX", "FRC", "split", "Contact" }); // Support User - Split override

				entry.CH_EntryStatus = ZString.Empty;
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new[] { "FSR", "FRN", "FRX", "FRC", "split", "Contact" });

				basic.MasterLevelHouseHelper.CS_CustomsStatus = CustomsStatusCodes.Codes.EntryOrRequestCancelled;
				entry.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new[] { "FSR", "FRN", "FRX", "FRC", "split", "Contact" }); // Entry Is Cancelled
			}
		}

		public void TestMenuItemsConsolidationOfHouses()
		{
			using (var menu = GetNewMenu(true))
			{
				var mawb = awb as CusMAWB;
				mawb.Profile = "CUKFFW98000LXA";
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FRI", "FSR", "FRN", "FRX", "FRC", "CDS", "Contact" });
				mawb.ChildBills.AddNew();
				AssertMenuItemsContain(menu, new string[] { "FRI", "FSR", "FRN", "FRC", "Contact" });
			}
		}

		public void TestMenuItemsWithSplit()
		{
			using (var menu = GetNewMenu(true))
			{
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck;
				awb.Splits.AddNew();
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FSR" }); // Cannot insert, delete or amend when split; but can create/send removals for chuildren 
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
				menu.RefreshMenu();
				AssertMenuItemsContain(menu, new string[] { "FSR", "split" });
			}
		}

		public void TestShedShouldNotManipulateSplitsOnPrearrivalMessageAndThatAnyValidationDeniesAccessToSplitPopup()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			using (var menu = GetNewMenu(true))
			{
				awb.Splits.AddNew();
				awb.NumberOfPiecesExpected = 15;
				var basic = awb as CusMAWB;
				basic.Profile = "CUKFFW98000LXA";
				basic.ShipmentDescriptionCode = "T";
				basic.NumberOfPiecesReceived = 10;
				basic.CM_MAWB = "12345678901";
				basic.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
				menu.RefreshMenu();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var splitMenu = menu.MenuItems.FindByText("Request and manage splits");
				splitMenu.PerformClick();
				AssertNotEquals("Sheds should not manipulate splits on pre-arrivals. Proceed anyway?", "Sheds should not manipulate splits on pre-arrivals. Proceed anyway?", UnitTestUserNotification.Instance.LastMessage.Text);

				basic.Profile = "CUKAIR98LHRBAC";
				basic.NumberOfPiecesReceived = 0;
				Assert("PreReq - should be prearrival to get popup challenge", basic.IsPrearrival);
				menu.RefreshMenu();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				splitMenu = menu.MenuItems.FindByText("Request and manage splits");
				splitMenu.PerformClick();
				AssertEquals("Sheds should not manipulate splits on pre-arrivals. Proceed anyway?", "Sheds should not manipulate splits on pre-arrivals. Proceed anyway?", UnitTestUserNotification.Instance.LastMessage.Text);

				basic.Profile = "CUKAIR98zzzzzz";
				menu.RefreshMenu();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				splitMenu = menu.MenuItems.FindByText("Request and manage splits");
				splitMenu.PerformClick();
				AssertEquals("Please fix the validation errors first", "Please fix the validation errors first", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestChiefMenuTerminating()
		{
			using (var menuForAgent = GetNewMenu(true))
			{
				var basic = awb as CusMAWB;
				basic.Profile = "CUKFFW98000XXX";
				Factory.Save();
				menuForAgent.RefreshMenu();
				var chiefMenu = menuForAgent.MenuItems.FindByText("Create CDS Declaration");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				chiefMenu.PerformClick();
				var createdDeclaration = basic.MasterLevelHouseHelper.Declaration;
				AssertNotNull(createdDeclaration);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Do you really want to create a standalone customs declaration"));
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("was created"));
				Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Through AWB"));
			}
		}

		public void TestChiefMenuTawb()
		{
			using (var menuForAgent = GetNewMenu(true))
			{
				var basic = awb as CusMAWB;
				basic.Profile = "CUKFFW98000XXX";
				basic.AirportOfDestination = "BHX";
				basic.AirportOfArrival = "LHR";
				Factory.Save();
				menuForAgent.RefreshMenu();
				var chiefMenu = menuForAgent.MenuItems.FindByText("Create CDS Declaration");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				chiefMenu.PerformClick();
				var createdDeclaration = basic.MasterLevelHouseHelper.Declaration;
				AssertNotNull(createdDeclaration);
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Do you really want to create a standalone customs declaration"));
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("was created"));
				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Through AWB"));
			}
		}

		public void TestMenuItemsForAgent()
		{
			using (var menuForAgent = GetNewMenu(true))
			{
				var basic = awb as CusMAWB;
				menuForAgent.RefreshMenu();
				AssertMenuItemsContain(menuForAgent, new string[] { "FRI", "FSR", "FRX", "FRC" });
				basic.Profile = "CUKFFW98000XXX";
				AssertMenuItemsContain(menuForAgent, new string[] { "FRI", "FSR", "FRN", "FRX", "FRC", "CDS", "Contact" }); // agent still sees FRN field 				

				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;

				awb.SetCustomsActionCode(CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval, ZDateTime.BrettsBirthday);
				menuForAgent.RefreshMenu();
				AssertMenuItemsContain(menuForAgent, new string[] { "FSR", "FRC", "Contact" });
				awb.NumberOfPiecesExpected = 5;
				awb.NumberOfPiecesReceived = 5;
				menuForAgent.RefreshMenu();
				AssertMenuItemsContain(menuForAgent, new string[] { "FSR", "FRC", "C1", "Contact" });

				awb.ReleaseThisNumberOfPieces(3, NumberOfPiecesReleasedHelper.AgentC1Event);
				menuForAgent.RefreshMenu();
				AssertMenuItemsContain(menuForAgent, new string[] { "FSR", "FRC", "C1", "Contact" });

				awb.ReleaseThisNumberOfPieces(1, NumberOfPiecesReleasedHelper.AgentC1Event);
				menuForAgent.RefreshMenu();
				AssertMenuItemsContain(menuForAgent, new string[] { "FSR", "FRC", "C1", "Contact" });

				awb.ReleaseThisNumberOfPieces(1, NumberOfPiecesReleasedHelper.AgentC1Event);
				menuForAgent.RefreshMenu();
				AssertMenuItemsContain(menuForAgent, new string[] { "FSR", "FRC", "Contact" });  // no more pieces to release, C1 option is hidden
			}
		}

		public void TestMenuItemsEcStatus()
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			using (var menuForECStatus = GetNewMenu(true))
			{
				var basic = awb as CusMAWB;
				basic.Profile = "CUKAIR98LHRBAC";
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
				awb.ShipmentDescriptionCode = "C";
				awb.NumberOfPiecesExpected = 5;
				var otOne = basic.OutTurns.AddNew();
				otOne.C5_PackagesOutturned = 3;
				var otTwo = basic.OutTurns.AddNew();
				otTwo.C5_PackagesOutturned = 2;
				basic.Factory.Save();
				AssertMenuItemsContain(menuForECStatus, new string[] { "FSR", "FRX", "FRC", "Set 'EC", "Print Release/Removal Authority", "Contact" });
				basic.SetCustomsActionCode(CustomsStatusCodes.Codes.EntryOrRequestCancelled, ZDateTime.Now);
				AssertMenuItemsContain(menuForECStatus, new string[] { "FSR", "FRX", "FRC", "Set 'EC", "Print Release/Removal Authority", "Contact" }); // CX status should allow EC status to be set, as the job is now open again
				GBCustomsDataRegistry.Instance.CcsukAllowSplittingOfEcStatusJobs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AssertMenuItemsContain(menuForECStatus, new string[] { "FSR", "FRX", "FRC", "split", "Set 'EC", "Print Release/Removal Authority", "Contact" });    // EC job, only shows split menu when rego allows
				GBCustomsDataRegistry.Instance.CcsukAllowSplittingOfEcStatusJobs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				awb.SetEcStatusRelease(true);
				AssertMenuItemsContain(menuForECStatus, new string[] { "FSR", "FRX", "FRC", "Unset 'EC", "Release", "Print Release/Removal Authority", "Contact" });
				awb.ReleaseThisNumberOfPieces(3, NumberOfPiecesReleasedHelper.ShedEvent);
				otOne.IsReleasedAlready = true;
				AssertMenuItemsContain(menuForECStatus, new string[] { "FSR", "FRX", "FRC", "Unset 'EC", "Release", "Print Release/Removal Authority", "Contact" });  // Can still unset EC status if some pieces are release, so long as non delivered
				awb.ReleaseThisNumberOfPieces(2, NumberOfPiecesReleasedHelper.ShedEvent);
				otTwo.IsReleasedAlready = true;
				AssertMenuItemsContain(menuForECStatus, new string[] { "FSR", "FRX", "FRC", "Unset 'EC", "Print Release/Removal Authority", "Contact" });  // none left to release, but can still unset
				otOne.IsDelivered = true;
				AssertMenuItemsContain(menuForECStatus, new string[] { "FSR", "FRX", "FRC", "Print Release/Removal Authority", "Contact" });  // at least one outturn is delivered, cannot unset
			}
		}

		public void TestDeleteDuplicateSplits()
		{
			GlbStaff.CurrentUser.GS_IsController = true;
			using var menu = GetNewMenu(true);
			var split1 = awb.Splits.AddNew();
			split1.SplitReference = "01";
			var split2Isr = awb.Splits.AddNew();
			split2Isr.SplitReference = "02";
			split2Isr.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.ExistsInAnotherShedIsrdHereStillNeedsFrc;
			split2Isr.CG_ArrivalDate = new ZDateTime(2024, 11, 19, 12, 13, 14);
			var split2Yes = awb.Splits.AddNew();
			split2Yes.SplitReference = "02";
			split2Yes.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
			var split3 = awb.Splits.AddNew();
			split3.SplitReference = "03";
			var split4Ass = awb.Splits.AddNew();
			split4Ass.SplitReference = "04";
			split4Ass.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection;
			split4Ass.CG_ArrivalDate = new ZDateTime(2024, 11, 19, 15, 16, 17);
			var split4Com = awb.Splits.AddNew();
			split4Com.SplitReference = "04";
			split4Com.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.CompletedOnCcsUk;
			split4Com.CG_CustomsStatus = CustomsStatusCodes.Codes.ClearedByCustoms;
			var split4Yes = awb.Splits.AddNew();
			split4Yes.SplitReference = "04";
			split4Yes.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;

			menu.RefreshMenu();
			var deleteMenu = menu.MenuItems.FindByText("Delete Duplicate Splits");
			AssertNotNull("Pre-requisite: find Delete Duplicate Splits menu item", deleteMenu);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			deleteMenu.PerformClick();
			var expectedText = "The following split record(s) are duplicated and will be deleted.\r\n\tSplit 02, presence ISR, created 19/11/2024 12:13\r\n\tSplit 04, presence ASS, created 19/11/2024 15:16\r\nPlease confirm this action.";
			AssertEquals(expectedText, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("No splits deleted", 7, awb.Splits.Count);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			deleteMenu.PerformClick();
			const string expectedText2 = "Duplicated splits have been deleted.";
			AssertEquals(expectedText2, UnitTestUserNotification.Instance.LastMessage.Text);

			AssertContainsExactElementsInAnyOrder(new[] { "01:", "02:YES", "03:", "04:COM", "04:YES" }, awb.Splits.Select(x => $"{x.SplitReference}:{x.PresenceOnNetworkStatus}"));
			AssertEquals("split2Isr.IsDeleted", expected: true, split2Isr.IsDeleted);
			AssertEquals("split4Ass.IsDeleted", expected: true, split4Ass.IsDeleted);
		}

		public void TestDeleteDuplicateSplits_Visibility()
		{
			GlbStaff.CurrentUser.GS_IsController = true;

			using var menu = GetNewMenu(true);
			MenuItem deleteMenu;
			void refreshAndFindMenuItem()
			{
				menu.RefreshMenu();
				deleteMenu = menu.MenuItems.FindByText("Delete Duplicate Splits");
			}

			refreshAndFindMenuItem();
			AssertNull("no splits", deleteMenu);

			var split1 = awb.Splits.AddNew();
			split1.SplitReference = "01";
			var split2 = awb.Splits.AddNew();
			split2.SplitReference = "02";

			refreshAndFindMenuItem();
			AssertNull("no duplicate splits", deleteMenu);

			var split2b = awb.Splits.AddNew();
			split2b.SplitReference = "02";

			refreshAndFindMenuItem();
			AssertNotNull("duplicate splits", deleteMenu);

			GlbStaff.CurrentUser.GS_IsController = false;

			refreshAndFindMenuItem();
			AssertNull("not controller", deleteMenu);
		}

		public void TestRRAMenuOptionDemandsSave()
		{
			using (var menu = GetNewMenu(true))
			{
				var shutup = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
				menu.SendsMessagesToCustoms = shutup;
				awb.ShipmentDescriptionCode = "T";
				((CusMAWB)awb).Profile = "CUKAIR98LHRBAC";
				((CusMAWB)awb).CM_MAWB = "12312345678";
				awb.SetCustomsActionCode("CC", ZDateTime.BrettsBirthday);
				var ot = ((CusMAWB)awb).OutTurns.AddNew();
				ot.C5_PackagesOutturned = 10;
				awb.NumberOfPiecesReceived = 10;
				awb.NumberOfPiecesExpected = 10;
				menu.RefreshMenu();
				var menuItem = menu.MenuItems.FindByText(CcsukMenu.RraMenuTitle, true);
				menuItem.PerformClick();
				AssertContains("save", shutup.InvalidOperationText);
				shutup = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
				menu.SendsMessagesToCustoms = shutup;
				Factory.Save();
				menuItem.PerformClick();
				AssertEquals(null, shutup.InvalidOperationText);
			}
		}

		public void TestMenuItemsShedRRA()
		{
			// If the agent's actions has products one of the CACs then the shed should be able to release the goods to them
			RunTestMenuItemsShedRRA(CustomsStatusCodes.Codes.ReleasedForInterShedRemoval);
			RunTestMenuItemsShedRRA(CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval);
			RunTestMenuItemsShedRRA(CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval);
			RunTestMenuItemsShedRRA(CustomsStatusCodes.Codes.ClearedByCustoms);
		}

		void RunTestMenuItemsShedRRA(ZString desiredCustomsActionCode)
		{
			using (var menuForShedRRA = GetNewMenu(true))
			{
				var basic = awb as CusMAWB;
				basic.Profile = "CUKAIR98LHRXXX";
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
				awb.ShipmentDescriptionCode = "T";
				awb.NumberOfPiecesExpected = 5;
				menuForShedRRA.RefreshMenu();
				AssertMenuItemsContain(menuForShedRRA, new string[] { "FSR", "FRX", "FRC", "split", "Print Release/Removal Authority", "Contact" });
				awb.SetCustomsActionCode(desiredCustomsActionCode, ZDateTime.BrettsBirthday);
				menuForShedRRA.RefreshMenu();
				AssertMenuItemsContain(menuForShedRRA, new string[] { "FSR", "FRC", "Print Release/Removal Authority", "Contact" });
				awb.NumberOfPiecesReceived = 5;
				AssertMenuItemsContain(menuForShedRRA, new string[] { "FSR", "FRC", "Release", "Contact" });
				awb.ReleaseThisNumberOfPieces(3, NumberOfPiecesReleasedHelper.ShedEvent);
				AssertMenuItemsContain(menuForShedRRA, new string[] { "FSR", "FRC", "Release", "Contact" });
				awb.ReleaseThisNumberOfPieces(2, NumberOfPiecesReleasedHelper.ShedEvent);
				AssertMenuItemsContain(menuForShedRRA, new string[] { "FSR", "FRC", "Print Release/Removal Authority", "Contact" });
			}
		}

		public void TestMenuItemsSplit()
		{
			using (var menuForSplit = GetNewMenuForSplit())
			{
				var split = awb as SplitBasic;
				split.Basic.Profile = "CUKFFW98000LXA";
				awb.PresenceOnNetworkStatus = PresenceOnNetworkList.Codes.OnCommDb;
				menuForSplit.RefreshMenu();
				AssertMenuItemsContain(menuForSplit, new string[] { "FSR", "FRN", "FRX", "FRC", "CDS", "Contact" });
			}
		}

		internal static void AssertMenuItemsContain(string[] expected, MenuItem itemWithChildrenWithoutNeedingRefresh)
		{
			RunItemsTest(itemWithChildrenWithoutNeedingRefresh, expected);
		}

		internal static void AssertMenuItemsContain(EDIMenu menu, string[] expected)
		{
			menu.RefreshMenu();
			RunItemsTest(menu, expected);
		}

		static void RunItemsTest(MenuItem menu, string[] expected)
		{
			AssertEquals("Right number of menu items", expected.Length, menu.MenuItems.Count);
			for (int i = 0; i < expected.Length; i++)
			{
				AssertEquals(string.Format("{0}th menu item [{1}] should contain [{2}]", i, menu.MenuItems[i].Text, expected[i]), true, menu.MenuItems[i].Text.Contains(expected[i]));
			}
		}

		public void TestViewOnlyFormDisablesMenu()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CargoTerminalOperator = "DAN";
			awb = mawb;
			manager = new CusAwbDelegateProvider(delegate
			{ return awb; });
			using (var form = new CcsukAirInventoryForm(mawb))
			{
				RunViewOnlyFormDisabledMenuTest(form);
			}
			var hawb = mawb.ChildBills.AddNew();
			using (var form = new CcsukAirInventoryFormHouse(hawb))
			{
				RunViewOnlyFormDisabledMenuTest(form);
			}
		}

		void RunViewOnlyFormDisabledMenuTest(ZForm form)
		{
			using (var menu = new CcsukMenu(manager, form))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.Edit;
				menu.RefreshMenu();
				Assert(menu.MenuItems.Count > 1);
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.ReadOnly;
				menu.RefreshMenu();
				AssertEquals(1, menu.MenuItems.Count);
				Assert(menu.MenuItems[0].Text.Contains("View mode"));
			}
		}

		public void TestMenuRefreshShowsShed()
		{
			using (var menu = GetNewMenu(true))
			{
				menu.RefreshMenu();
				AssertEquals("To shed DAN", menu.transmitFsrToShed.Text);
			}
		}

		public void TestContactOtherParty()
		{
			using (var menu = GetNewMenu(true))
			{
				var basic = awb as CusMAWB;
				basic.Profile = "CUKAIR98LHRXXX";
				basic.ShipmentDescriptionCode = "T";
				basic.CM_MAWB = "12312345678";
				menu.RefreshMenu();
				var contact = menu.MenuItems[menu.MenuItems.Count - 1];
				contact.PerformClick();
				AssertContains("save", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				basic.Factory.Save();
				contact.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertType(typeof(CcsukGenralMessageFormForNew), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		CcsukMenu GetNewMenu(bool shouldMakeAwbAvailable)
		{
			if (shouldMakeAwbAvailable)
			{
				var mawb = Factory.New<CusMAWB>();
				mawb.CargoTerminalOperator = "DAN";
				awb = mawb;
				manager = new CusAwbDelegateProvider(delegate
				{ return awb; });
			}
			return new CcsukMenu(manager, null);
		}

		CcsukMenu GetNewMenuForSplit()
		{
			var basic = Factory.New<CusMAWB>();
			basic.CargoTerminalOperator = "DAN";
			var split = basic.Splits.AddNew();
			awb = split;
			manager = new CusAwbDelegateProvider(delegate
			{ return awb; });
			return new CcsukMenu(manager, null);
		}

		ICcsukCusAwb awb;
		CusAwbDelegateProvider manager;
	}
}
