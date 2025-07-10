using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration.Testing;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class NctsMessagingMenuTest : TestCaseWithFactory
	{
		public void TestImportCustomsEntryLinesMenuItem()
		{
			using (var form = new ZForm())
			using (var menu = new NctsMessagingMenu(form))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				menu.NctsHeader = header;

				header.SetMovementType(NctsMovementType.Codes.Arrival);
				AssertNull(menu.MenuItems.FindByText("Import Customs Entry Lines"));

				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				AssertNotNull(menu.MenuItems.FindByText("Import Customs Entry Lines"));

				header.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
				AssertNull(menu.MenuItems.FindByText("Import Customs Entry Lines"));
			}
		}

		public void TestChangingBH_HeaderType_ShouldRefreshMenus()
		{
			using (var form = new ZForm())
			using (var menu = new NctsMessagingMenu(form))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				menu.NctsHeader = header;

				header.SetMovementType(NctsMovementType.Codes.Arrival);
				AssertNull(menu.MenuItems.FindByText("Send Departure Message"));
				AssertNotNull(menu.MenuItems.FindByText("Send Arrival Message"));

				header.BH_HeaderType = NctsMovementType.Codes.Departure;
				AssertNotNull(menu.MenuItems.FindByText("Send Departure Message"));
				AssertNull(menu.MenuItems.FindByText("Send Arrival Message"));

				header.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
				AssertNull(menu.MenuItems.FindByText("Send Departure Message"));
				AssertNotNull(menu.MenuItems.FindByText("Send Arrival Message"));
			}
		}

		[RequiresSTA]
		public void TestMenu_SendArrivalMessage_DE()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
				orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Germany);
				orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "4567", Core.Constants.CountryCodes.Germany);
				orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber, "4567", Core.Constants.CountryCodes.Germany);

				var authorisation = orgProxy.Factory.New<CusAuthorisationHeader>();
				authorisation.CPH_OH_PermitHolder = orgProxy.PK;
				authorisation.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				authorisation.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				authorisation.CPH_StartDate = ZDate.Today.AddDays(-1);
				authorisation.CPH_EndDate = ZDate.Today.AddDays(1);
				authorisation.CPH_Number = "DE003302";

				var authorisationRule = authorisation.CusAuthorisationRules.AddNew();
				authorisationRule.CPR_RuleCode = "LOC";
				authorisationRule.CPR_ValueFrom = "LOC01";

				var orgContact = orgProxy.Contacts.AddNew();
				orgContact.OC_ContactName = "VIC";
				orgContact.OC_Title = "DEV";
				orgContact.OC_Phone = "12345";
				orgContact.OC_Fax = "VICFFF";
				orgContact.OC_Email = "VIC@Wisetechglobal.com";
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgProxy.PK;

				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = "DE003302";
				header.ArrivalMrnFromUser = "19DE12345678900000";
				header.EnRouteIncidents.AddNew();
				header.ArrivalMovementHeader.BM_PlaceOfUnloading = "LOC01";
				header.DestinationTrader.OrganisationPK = orgProxy.PK;
				header.DestinationTrader.ContactPK = orgContact.PK;
				header.BH_GB = GlbBranch.CurrentBranch.PK;

				using (var nctsMovementForm = new NctsMovementForm(header))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					menu.NctsHeader = header;
					var menuItem = menu.MenuItems.FindByText("Send Arrival Message");
					menuItem.PerformClick();
					AssertEquals(1, header.Messages.Count);
				}
			}
		}

		[RequiresSTA]
		public void TestMenu_SendDepartureMessage_DE()
		{
			var countryCode = Core.Constants.CountryCodes.Germany;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList(countryCode);
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.Departure);
				header = NctsTransmissionMessageGeneratorTests.MakeDefaultDepartureForMessagingTest(header, Factory, true, addAllMandatoryData: true);
				header.MovementHeader.BM_AdditionalText = "";
				var principlaAddress = header.Principal.Address;
				var customsCode = principlaAddress.Header.CustomsCodes.AddNew();
				customsCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber;
				customsCode.OK_CustomsRegNo = "1234";
				customsCode.OK_RN_NKCodeCountry = "DE";
				customsCode.OK_OA_PremisesAddress = principlaAddress.PK;
				var contact = principlaAddress.Header.Contacts.AddNew();
				contact.OC_IsActive = true;
				contact.OC_ContactName = "contact1";
				contact.OC_Title = "AAA";
				contact.OC_Phone = "123456";

				using (var nctsMovementForm = new NctsMovementForm(header))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					menu.NctsHeader = header;
					var menuItem = menu.MenuItems.FindByText("Send Departure Message");

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					menuItem.PerformClick();
					AssertNotContains("Commercial Reference Number: ", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotContains("Place of Unloading: ", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotContains("MRN: ", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotContains("Importer Documentary Address: ", UnitTestUserNotification.Instance.LastMessage.Text);

					header.BH_FTZMove = true;
					header.MovementHeader.GoodsItems[0].SupportingDocuments.AddNew();
					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					menuItem.PerformClick();
					AssertNotContains("Commercial Reference Number: ", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("Place of Unloading Code: ", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotContains("MRN: ", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotContains("Importer Documentary Address: ", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestMenu_DepartureToNonSupportedCountry()
		{
			using (var form = new ZForm())
			using (var menu = new NctsMessagingMenu(form))
			{
				AssertDepartureMenu(menu, NctsMovementType.Codes.Departure, Core.Constants.CountryCodes.Japan);
			}
		}

		public void TestMenu_DepartureToSupportedCountry()
		{
			using (var form = new ZForm())
			using (var menu = new NctsMessagingMenu(form))
			{
				foreach (var country in NctsHeaderValidationHelper.NctsContractingParties)
				{
					AssertDepartureMenu(menu, NctsMovementType.Codes.Departure, country);
					AssertRefreshDepartureMenu(menu, NctsMovementType.Codes.Departure, country);
				}
			}
		}

		void AssertDepartureMenu(NctsMessagingMenu menu, ZString movementType, ZString destinationOffice)
		{
			menu.NctsHeader = null;
			menu.OnPopup(EventArgs.Empty);
			AssertEquals("Menu has no menuItems", 0, menu.MenuItems.Count);

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(movementType);

			var office = header.CustomsOffices.AddNew();
			office.CY_Data = destinationOffice;
			office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;

			menu.NctsHeader = header;
			AssertNotNull(menu.MenuItems.FindByText("Send Departure Message"));
			if (header.IsDepartureDestinationOfficeInNctsContractingCountry)
			{
				AssertNotNull(menu.MenuItems.FindByText("Make &Arrival Notification for this Departure"));
			}
			else
			{
				AssertNull(menu.MenuItems.FindByText("Make &Arrival Notification for this Departure"));
			}
			AssertNotNull(menu.MenuItems.FindByText("Request &Cancellation"));
			AssertNotNull(menu.MenuItems.FindByText("Re-open Declaration for &Amendment"));
			AssertNull(menu.MenuItems.FindByText("Send Arrival Message"));
			AssertNull(menu.MenuItems.FindByText("Send &Unloading Remarks"));
			AssertNull(menu.MenuItems.FindByText("Send Combined Arrival and Departure"));

			menu.OnPopup(EventArgs.Empty);
			Assert("Menu has at least 3 menuItems", 3 <= menu.MenuItems.Count);
		}

		void AssertRefreshDepartureMenu(NctsMessagingMenu menu, ZString movementType, ZString destinationOffice)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(movementType);

			var office = header.CustomsOffices.AddNew();
			office.CY_Data = destinationOffice;
			office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination;

			menu.NctsHeader = header;
			var sendDep = menu.MenuItems.FindByText("Send Departure Message");

			CombineAssertions(() =>
			{
				AssertEquals("Send Departure Message MenuItem is visible", true, sendDep.Visible);

				menu.NctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationCancelled;
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("Send Departure Message MenuItem is not visible", false, sendDep.Visible);
			});
		}

		public void TestMenu_CheckMenuItemVisibility()
		{
			using (var form = new ZForm())
			using (var menu = new NctsMessagingMenu(form))
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);

				menu.NctsHeader = header;
				var requestCancellation = menu.MenuItems.FindByText("Request &Cancellation");

				CombineAssertions(() =>
				{
					AssertEquals("Precondition", false, header.IsDepartureCancellationAllowed);
					menu.OnPopup(EventArgs.Empty);
					AssertEquals("Request Cancellation should not be visible when IsDepartureCancellationAllowed is false", false, requestCancellation.Visible);
					menu.NctsHeader.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationMrnAllocated;
					AssertEquals("Precondition", true, header.IsDepartureCancellationAllowed);
					menu.OnPopup(EventArgs.Empty);
					AssertEquals("Request Cancellation should be visible when IsDepartureCancellationAllowed is true", true, requestCancellation.Visible);
				});
			}
		}

		public void TestMenu_Arrival()
		{
			using (var form = new ZForm())
			using (var menu = new NctsMessagingMenu(form))
			{
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("Menu has no menuItems", 0, menu.MenuItems.Count);

				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				menu.NctsHeader = header;
				AssertNull(menu.MenuItems.FindByText("Send Departure Message"));
				AssertNull(menu.MenuItems.FindByText("Make &Arrival Notification for this Departure"));
				AssertNull(menu.MenuItems.FindByText("Request &Cancellation"));
				AssertNull(menu.MenuItems.FindByText("Re-open Declaration for &Amendment"));
				AssertNull(menu.MenuItems.FindByText("Send Combined Arrival and Departure"));
				AssertNotNull(menu.MenuItems.FindByText("Send Arrival Message"));
				AssertNotNull(menu.MenuItems.FindByText("Send &Unloading Remarks"));

				menu.OnPopup(EventArgs.Empty);
				AssertEquals("Menu has at 2 menuItems", 2, menu.MenuItems.Count);
			}
		}

		public void TestMenu_ArrivalVisibility()
		{
			using (var form = new ZForm())
			using (var menu = new NctsMessagingMenu(form))
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				menu.NctsHeader = header;

				var sendArrival = menu.MenuItems.FindByText("Send Arrival Message");

				CombineAssertions(() =>
				{
					AssertEquals("Send Arrival Message MenuItem is visible", true, sendArrival.Visible);

					menu.NctsHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
					menu.OnPopup(EventArgs.Empty);
					AssertEquals("Send Arrival Message MenuItem is not visible", false, sendArrival.Visible);
				});
			}
		}

		public void TestDeparturesFromCountriesSupportedByNCTS()
		{
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Austria, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Belgium, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Bulgaria, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Croatia, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Cyprus, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.CzechRepublic, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Denmark, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Estonia, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Finland, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.France, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Germany, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Greece, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Hungary, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Ireland, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Italy, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Latvia, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Lithuania, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Luxembourg, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Malta, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Netherlands, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Poland, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Portugal, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Romania, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Slovakia, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Slovenia, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Spain, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Sweden, "RU", true);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.UnitedKingdom, "RU", true);
			}
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Andorra, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Iceland, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Liechtenstein, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Macedonia, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Norway, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.SanMarino, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Serbia, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.SvalbardAndJanMayen, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Switzerland, "RU", true);
			SetupAndAssertSupportedCountry(true, Core.Constants.CountryCodes.Turkey, "RU", true);
		}

		public void TestArrivalsToCountriesSupportedByNCTS()
		{
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Austria, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Belgium, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Bulgaria, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Croatia, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Cyprus, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.CzechRepublic, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Denmark, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Estonia, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Finland, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.France, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Germany, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Greece, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Hungary, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Ireland, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Italy, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Latvia, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Lithuania, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Luxembourg, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Malta, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Netherlands, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Poland, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Portugal, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Romania, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Slovakia, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Slovenia, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Spain, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Sweden, true);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.UnitedKingdom, true);
			}
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Andorra, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Iceland, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Liechtenstein, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Macedonia, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Norway, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.SanMarino, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Serbia, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.SvalbardAndJanMayen, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Switzerland, true);
			SetupAndAssertSupportedCountry(false, "RU", Core.Constants.CountryCodes.Turkey, true);
		}

		public void TestDepartureFromAustraliaIsNotSupported()
		{
			SetupAndAssertSupportedCountry(true, "AU", "RU", false);
		}

		public void TestArrivalToAustraliaIsNotSupported()
		{
			SetupAndAssertSupportedCountry(false, "RU", "AU", false);
		}

		public void SetupAndAssertSupportedCountry(bool isDeparture, ZString departureCountryCode, ZString arrivalCountryCode, bool assertSupported)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList();
			Factory.Save();

			var nctsMovement = Factory.New<NctsHeader>();
			nctsMovement.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			if (isDeparture)
			{
				nctsMovement.SetMovementType(NctsMovementType.Codes.Departure);
				nctsMovement = NctsTransmissionMessageGeneratorTests.MakeDefaultDepartureForMessagingTest(nctsMovement, Factory, true, addAllMandatoryData: true);
			}
			else
			{
				nctsMovement.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsMovement = NctsTransmissionMessageGeneratorTests.MakeDefaultArrivalForMessagingTest(Factory);
			}
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsMovement, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, departureCountryCode + "000060", ZDateTime.Empty, true);
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsMovement, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, arrivalCountryCode + "025100", ZDateTime.Empty, false);
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsMovement, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, arrivalCountryCode + "025100", ZDateTime.Empty, false);

			using (var nctsMovementForm = new NctsMovementForm(nctsMovement))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				menu.NctsHeader = nctsMovement;

				if (isDeparture)
				{
					menu.MenuItems.FindByText("Send &Departure Message").PerformClick();
				}
				else
				{
					menu.MenuItems.FindByText("Send &Arrival Message").PerformClick();
				}
				if (assertSupported)
				{
					AssertContains("", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertContains("NCTS is not supported by the target country", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[RequiresSTA]
		public void TestPressCancel()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var departure = Factory.New<NctsHeader>();
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departure.SetMovementType(NctsMovementType.Codes.Departure);

				using (var nctsMovementForm = new NctsMovementForm(departure))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					menu.NctsHeader = departure;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					var menuItem = menu.MenuItems.FindByText("Request &Cancellation", true);
					menuItem.PerformClick();
					AssertEquals(0, departure.Messages.Count);
					AssertContains("MRN", UnitTestUserNotification.Instance.LastMessage.Text);

					departure = Factory.New<NctsHeader>();
					departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					departure.SetMovementType(NctsMovementType.Codes.Departure);
					NCTSTestHelper.SetMrnForTest(departure, "ABC123");
					menu.NctsHeader = departure;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					menuItem = menu.MenuItems.FindByText("Request &Cancellation", true);
					menuItem.PerformClick();
					AssertEquals(0, departure.Messages.Count);

					departure = Factory.New<NctsHeader>();
					departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					departure.SetMovementType(NctsMovementType.Codes.Departure);
					NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", departure.Principal, suffix: "", traderTin: "123456789012");
					var office = departure.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
					office.CY_Data = "GB000060";
					NCTSTestHelper.SetMrnForTest(departure, "ABC123");
					menu.NctsHeader = departure;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddUserResponse("REASON HERE");
					menuItem = menu.MenuItems.FindByText("Request &Cancellation", true);
					menuItem.PerformClick();
					AssertEquals(1, departure.Messages.Count);
					AssertContains("014", departure.Messages[0].EM_MessageText);
					AssertContains("REASON HERE", departure.Messages[0].EM_MessageText);
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("reason here and press Yes"));
				}
			}
		}

		[RequiresSTA]
		public void TestPressAmendDeparture()
		{
			using (var form = new ZForm())
			using (var menu = new NctsMessagingMenu(form))
			{
				var departure = Factory.New<NctsHeader>();
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departure.SetMovementType(NctsMovementType.Codes.Departure);
				departure.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationCancelled;
				Factory.Save();
				var lrnNumber = departure.LocalReferenceNumber;
				AssertNotContains("/", lrnNumber);

				menu.NctsHeader = departure;
				var menuItem = menu.MenuItems.FindByText("Re-open Declaration for &Amendment", true);
				menuItem.PerformClick();
				AssertContains(lrnNumber + "/1", departure.LocalReferenceNumber);
			}
		}

		[RequiresSTA]
		public void TestPressUnloadingRemarksGeneratesIE44Message()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Destination office code is GB00000x
			{
				CreateZZRefTestValuesIfNeeded("GB000060", Core.Constants.CountryCodes.UnitedKingdom, new ZString[] { "DES" });

				var arrival = NctsTransmissionMessageGeneratorTests.MakeDefaultUnloadingRemarksForMessagingTest(Factory);
				arrival.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
				arrival.UnloadingMovementHeader.GoodsItems.AddNew();

				using (var nctsMovementForm = new NctsMovementForm(arrival))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					menu.NctsHeader = arrival;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					var menuItem = menu.MenuItems.FindByText("Send &Unloading Remarks", true);
					AssertEquals(0, arrival.Messages.Count);
					menuItem.PerformClick();
					AssertEquals(1, arrival.Messages.Count);
					AssertContains("<CC044A>", arrival.Messages[0].EM_MessageText);
				}
			}
		}

		[RequiresSTA]
		public void TestPressArrivalGeneratesIE07Message()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Destination office code is GB00000x
			{
				CreateZZRefTestValuesIfNeeded("GB000060", Core.Constants.CountryCodes.UnitedKingdom, new ZString[] { "DES" });
				var arrival = NctsTransmissionMessageGeneratorTests.MakeDefaultArrivalForMessagingTest(Factory);
				using (var nctsMovementForm = new NctsMovementForm(arrival))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					menu.NctsHeader = arrival;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					var menuItem = menu.MenuItems.FindByText("Send &Arrival Message", true);
					AssertEquals(0, arrival.Messages.Count);
					menuItem.PerformClick();
					AssertEquals(1, arrival.Messages.Count);
					AssertContains("<CC007A>", arrival.Messages[0].EM_MessageText);
				}
			}
		}

		[RequiresSTA]
		public void TestPressDepartureGeneratesIE15Message()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNctsDeclarationTypeList(Core.Constants.CountryCodes.UnitedKingdom);
				Factory.Save();

				var departure = Factory.New<NctsHeader>();
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departure = NctsTransmissionMessageGeneratorTests.MakeDefaultDepartureForMessagingTest(departure, Factory, true, addAllMandatoryData: true);

				using (var nctsMovementForm = new NctsMovementForm(departure))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					menu.NctsHeader = departure;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					var menuItem = menu.MenuItems.FindByText("Send &Departure Message", true);
					AssertEquals(0, departure.Messages.Count);
					menuItem.PerformClick();
					AssertEquals(1, departure.Messages.Count);
					AssertContains("<CC015B>", departure.Messages[0].EM_MessageText);
					AssertEquals(false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("resend this message?"));
				}
			}
		}

		[RequiresSTA]
		public void TestSendingArrivalMessageSetsMessageStatusToQueued()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Destination office code is GB00000x
			{
				var arrival = NctsTransmissionMessageGeneratorTests.MakeDefaultArrivalForMessagingTest(Factory);
				using (var nctsMovementForm = new NctsMovementForm(arrival))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					menu.NctsHeader = arrival;
					var menuItem = menu.MenuItems.FindByText("Send Arrival Message", true);

					CombineAssertions(() =>
					{
						AssertEquals("Pre-req Arrival Status", NctsTransitStatusList.Codes.Unknown, arrival.ArrivalMovementHeader.BM_CustomsStatus);
						AssertEquals("Pre-req Message Status", NctsMessageStatusList.Codes.ArrivalNotificationNotSent, arrival.EffectiveMessageStatus);

						menuItem.PerformClick();

						AssertEquals("Arrival message not processed yet", NctsTransitStatusList.Codes.Unknown, arrival.ArrivalMovementHeader.BM_CustomsStatus);
						AssertEquals("Message queued", NctsMessageStatusList.Codes.MessageQueued, arrival.EffectiveMessageStatus);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestResendDepartureMessageWarnsUser()
		{
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure = NctsTransmissionMessageGeneratorTests.MakeDefaultDepartureForMessagingTest(departure, Factory, true);
			departure.MovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.DeclarationAccepted;

			using (var nctsMovementForm = new NctsMovementForm(departure))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				menu.NctsHeader = departure;
				AssertEquals("Pre-req", true, departure.IsDepartureTabReadOnly);
				var menuItem = menu.MenuItems.FindByText("Send &Departure Message", true);
				AssertEquals(false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("resend this message?"));
				menuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("resend this message?"));
			}
		}

		[RequiresSTA]
		public void TestResendArrivalMessageWarnsUser()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Destination office code is GB00000x
			{
				var arrival = NctsTransmissionMessageGeneratorTests.MakeDefaultArrivalForMessagingTest(Factory);
				arrival.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;

				using (var nctsMovementForm = new NctsMovementForm(arrival))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					menu.NctsHeader = arrival;
					AssertEquals("Pre-req", true, arrival.IsArrivalTabReadOnly);
					var menuItem = menu.MenuItems.FindByText("Send Arrival Message", true);
					AssertEquals(false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("resend this message?"));
					menuItem.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("resend this message?"));
				}
			}
		}

		[RequiresSTA]
		public void TestResendUnloadingMessageWarnsUser()
		{
			var arrival = NctsTransmissionMessageGeneratorTests.MakeDefaultArrivalForMessagingTest(Factory);
			arrival.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.GoodsReleasedFromTransitUponArrival;
			arrival.EffectiveMessageStatus = NctsMessageStatusList.Codes.UnloadingRemarksSent;

			using (var nctsMovementForm = new NctsMovementForm(arrival))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				menu.NctsHeader = arrival;
				AssertEquals("Pre-req", true, arrival.IsUnloadingRemarksTabReadOnly);
				var menuItem = menu.MenuItems.FindByText("Send &Unloading Remarks", true);
				AssertEquals(false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("resend this message?"));
				menuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("resend this message?"));
			}
		}

		public void TestInitializeParentFormOnSetupMessageSenderProvider()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = "IT";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = orgHeader.PK;

			var mockSettings = new Mock<Integration.Customs.Shared.INctsSettings>();
			mockSettings.Setup(x => x.IsNctsEnabled).Returns(true);
			mockSettings.Setup(x => x.IsUsingPhase5(It.IsAny<string>())).Returns(false);

			using (ObjectFactory.Substitute(mockSettings.Object))
			using (var form = new ZForm())
			using (var plugIn = new NctsPlugin(shipment))
			{
				form.Menu.MenuItems.Add(plugIn.TopLevelMenu);
				plugIn.CreateInBond();
				var menuItem = plugIn.TopLevelMenu.MenuItems.FindByText("Send Departure Message");
				AssertNoExceptionThrown("", () => menuItem.PerformClick());
				AssertEquals(true, shipment.IsInDatabase);
			}
		}

		public static void CreateZZRefTestValuesIfNeeded(ZString officeCode, ZString dataGroupingCode, ZString[] officePurpose)
		{
			var factory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);

			var eunzzz = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, parent: eunzzz);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingCusCodeList(officeCode, dataGroupingCode, "Office Description " + officeCode, officePurpose);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			factory.Save();
		}
	}
}
