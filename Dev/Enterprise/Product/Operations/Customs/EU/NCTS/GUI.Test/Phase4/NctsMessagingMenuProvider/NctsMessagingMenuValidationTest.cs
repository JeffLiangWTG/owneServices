using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration.Testing;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	public class NctsMessagingMenuValidationTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestAddingDuplicateMesageErrorsToCollection()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			using (var nctsMovementForm = new NctsMovementForm(header))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				const string duplicateResult = "Please enter a Principal trader with an EORI or enter full address details.(C050)\r\nPlease enter a Principal trader with an EORI or enter full address details.(C050)";
				ClickMenuItem(header, menu, "Send Departure Message");
				AssertNotContains(duplicateResult, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[RequiresSTA]
		public virtual void TestDepartureMandatoryErrorNotifications()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNctsDeclarationTypeList(Core.Constants.CountryCodes.UnitedKingdom);
				Factory.Save();
				var departure = Factory.New<NctsHeader>();
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departure.SetMovementType(NctsMovementType.Codes.Departure);

				using (var nctsMovementForm = new NctsMovementForm(departure))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					ClickMenuItem(departure, menu, "Send Departure Message");
					AssertEquals("Expecting Mandatory Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));
					AssertExpectedMandatoryDepartureErrors();

					departure.MovementHeader.BM_RL_NKDestinationPort = "DE";
					departure.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.Australia;
					ClickMenuItem(departure, menu, "Send Departure Message");
					AssertEquals("Expecting Mandatory Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));

					departure = NctsTransmissionMessageGeneratorTests.MakeDefaultDepartureForMessagingTest(departure, Factory, addAllMandatoryData: true);
					ClickMenuItem(departure, menu, "Send Departure Message");
					AssertNotEquals("Expecting no Mandatory Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));
				}
			}
		}

		public virtual void TestMessageSendingDateIsNotOlderThanLimitDate()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNctsDeclarationTypeList(Core.Constants.CountryCodes.UnitedKingdom);
				Factory.Save();
				var departure = Factory.New<NctsHeader>();
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departure.SetMovementType(NctsMovementType.Codes.Departure);

				using (var nctsMovementForm = new NctsMovementForm(departure))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					departure.MovementHeader.IsSimplifiedNctsProcedure = false;

					departure.SetMovementType(NctsMovementType.Codes.Departure);
					NctsTransmissionMessageGeneratorTests.MakeDefaultDepartureForMessagingTest(departure, Factory, addAllMandatoryData: true);

					ClickMenuItem(departure, menu, "Send Departure Message");
					AssertNotEquals("Expecting no Extra Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));

					departure.MovementHeader.IsSimplifiedNctsProcedure = true;
					departure.MovementHeader.BM_ExportDate = ZDateTime.Now.AddDays(-5);

					ClickMenuItem(departure, menu, "Send Departure Message");
					AssertEquals("Expecting Extra Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));
					AssertEquals("Expecting Extra Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Date-Limit must be in the future."));

					departure.MovementHeader.BM_ExportDate = ZDateTime.Now.AddDays(5);

					ClickMenuItem(departure, menu, "Send Departure Message");
					AssertNotEquals("Expecting no Extra Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));
					AssertNotEquals("Expecting no Extra Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Date-Limit must be in the future."));
				}
			}
		}

		public void TestDepartureMessageErrorNotifications_Superuser()
		{
			SetupAndSendDepartureAndAssertResult(true);
		}

		[RequiresSTA]
		public void TestDepartureMessageErrorNotifications_NormalUser()
		{
			SetupAndSendDepartureAndAssertResult(false);
		}

		public virtual void TestDepartureNoNotifications()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNctsDeclarationTypeList(Core.Constants.CountryCodes.UnitedKingdom);
				Factory.Save();
				var departure = Factory.New<NctsHeader>();
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departure.SetMovementType(NctsMovementType.Codes.Departure);

				using (var nctsMovementForm = new NctsMovementForm(departure))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					departure = NctsTransmissionMessageGeneratorTests.MakeDefaultDepartureForMessagingTest(departure, Factory, addAllMandatoryData: true);
					departure.BH_FTZMove = false;
					ClickMenuItem(departure, menu, "Send Departure Message");
					AssertNotEquals("Expecting no Mandatory Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));
					AssertNotEquals("Expecting no Message Errors", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("There are message errors that may cause your message to be rejected by NCTS"));
					AssertEquals(1, departure.Messages.Count);
					AssertEquals("Sent", NctsMessageStatusList.Codes.MessageQueued, departure.EffectiveMessageStatus);
				}
			}
		}

		[RequiresSTA]
		public virtual void TestArrivalValidationWithMandatoryErrors()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			using (var nctsMovementForm = new NctsMovementForm(header))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				ClickMenuItem(header, menu, "Send Arrival Message");
				AssertMandatoryArrivalErrors(true);
				AssertEquals("Mandatory Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));
			}
		}

		[RequiresSTA]
		public void TestArrivalValidationNoErrors()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.Arrival);

				using (var nctsMovementForm = new NctsMovementForm(header))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					SetupHeaderWithMandatoryDataForArrivalTest(header, Factory);
					ClickMenuItem(header, menu, "Send Arrival Message");
					AssertMandatoryArrivalErrors(false);
					AssertNotEquals("No Mandatory Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));
				}
			}
		}

		public virtual void TestUnloadingValidationWithMandatoryErrors()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			using (var nctsMovementForm = new NctsMovementForm(header))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				ClickMenuItem(header, menu, "Send Unloading Remarks");
				AssertMandatoryUnloadingErrors(true);
				AssertEquals("Mandatory Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));
			}
		}

		public void TestUnloadingValidationWithNoErrors()
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Destination office code is GB00000x
			{
				var header = Factory.New<NctsHeader>();
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				header.SetMovementType(NctsMovementType.Codes.Arrival);

				using (var nctsMovementForm = new NctsMovementForm(header))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					SetupHeaderWithMandatoryDataForArrivalTest(header, Factory);
					SetupHeaderWithMandatoryDataForUnloadingTest(header);
					ClickMenuItem(header, menu, "Send Unloading Remarks");
					AssertMandatoryUnloadingErrors(false);
					AssertNotEquals("Mandatory Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));
				}
			}
		}

		protected virtual void SetupAndSendDepartureAndAssertResult(ZBool superuser)
		{
			NctsTransmissionMessageGeneratorTests.SetupGBPassword(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom)) // Departure office code is GB00000x
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNctsDeclarationTypeList(Core.Constants.CountryCodes.UnitedKingdom);
				Factory.Save();
				var departure = Factory.New<NctsHeader>();
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departure.SetMovementType(NctsMovementType.Codes.Departure);

				using (var nctsMovementForm = new NctsMovementForm(departure))
				using (var menu = new NctsMessagingMenu(nctsMovementForm))
				{
					Environment.Env.Security.EuNctsSendWithMessageErrors.IsAllowed = superuser;
					departure = NctsTransmissionMessageGeneratorTests.MakeDefaultDepartureForMessagingTest(departure, Factory, addAllMandatoryData: true);
					departure.BH_FTZMove = true;
					AssertNotEquals("Expecting no Message Errors", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("There are message errors that may cause your message to be rejected by NCTS"));
					ClickMenuItem(departure, menu, "Send Departure Message");
					AssertNotEquals("Expecting no Mandatory Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));
					AssertEquals("Send with message errors = true", superuser, Environment.Env.Security.EuNctsSendWithMessageErrors.IsAllowed);

					if (superuser)
					{
						AssertEquals("Expecting no \\r\\n characters in dialog", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("\\r\\n"));
						AssertEquals("Send with message errors set", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("It is likely that your message(s) will be rejected by NCTS"));
						AssertEquals(1, departure.Messages.Count);
						AssertContains("<CC015B>", departure.Messages[0].EM_MessageText);
					}
					else
					{
						AssertEquals("Send with message errors not set", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please fix the following message errors before sending any messages"));
						AssertEquals(0, departure.Messages.Count);
					}
				}
			}
		}

		protected void AssertMandatoryUnloadingErrors(ZBool assertion)
		{
			if (assertion)
			{
				AssertEquals("Mandatory unloading error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You need to supply a valid unloading date"));
				AssertEquals("Mandatory unloading error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You have not entered an Unloading Conforms"));
				AssertEquals("Mandatory unloading error", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You have not entered a State of Seals OK"));
				AssertEquals("Mandatory unloading error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You have not entered an Unloading Completed"));
			}
			else
			{
				AssertNotEquals("Mandatory unloading error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You need to supply a valid unloading date"));
				AssertNotEquals("Mandatory unloading error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You have not entered an Unloading Conforms"));
				AssertNotEquals("Mandatory unloading error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You have not entered a State of Seals OK"));
				AssertNotEquals("Mandatory unloading error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You have not entered an Unloading Completed"));
			}
		}

		protected virtual void AssertExpectedMandatoryDepartureErrors()
		{
			var lastMessageText = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertContains("You have not entered a [1] Declaration Type.", lastMessageText);
			AssertContains("The declaration requires an office of type NCTS Office of departure with purpose DEP", lastMessageText);
			AssertContains("The declaration requires an office of type NCTS Office of destination with purpose DES", lastMessageText);
			AssertContains("You need to supply a valid guarantee", lastMessageText);
			AssertContains("You need to supply at least one goods item", lastMessageText);
			AssertContains("[C050] Please enter a Principal trader with an EORI or enter full address details.", lastMessageText);
		}

		protected void ClickMenuItem(NctsHeader header, NctsMessagingMenu menu, ZString messageText)
		{
			menu.NctsHeader = header;
			var departureMenuItem = menu.MenuItems.FindByText(messageText);
			departureMenuItem?.PerformClick();
		}

		protected void AssertMandatoryArrivalErrors(ZBool assertion)
		{
			if (assertion)
			{
				AssertEquals("Mandatory arrival MRN error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You need to supply a movement reference number (MRN)"));
				AssertEquals("Mandatory arrival trader EORI error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please enter a Destination trader with an EORI"));
				AssertEquals("Mandatory arrival destination office error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You need to supply a destination customs office for Arrival that is supported"));
			}
			else
			{
				AssertNotEquals("Mandatory arrival MRN error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You need to supply a movement reference number (MRN)"));
				AssertNotEquals("Mandatory arrival trader EORI error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Please enter a Destination trader with an EORI"));
				AssertNotEquals("Mandatory arrival destination office error", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("You need to supply a destination customs office for Arrival that is supported"));
			}
		}

		void SetupHeaderWithMandatoryDataForUnloadingTest(NctsHeader header)
		{
			var unloadingRemark = header.UnloadingRemark;
			unloadingRemark.G9_UnloadingDate = ZDateTime.Today;
			unloadingRemark.G9_StateOfSealsOk = YesNoList.Codes.Yes;
			unloadingRemark.G9_Conform = YesNoList.Codes.Yes;
			unloadingRemark.G9_UnloadingCompletion = YesNoList.Codes.Yes;
		}

		void SetupHeaderWithMandatoryDataForArrivalTest(NctsHeader header, BusinessObjectFactory factory)
		{
			var mrn = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.Branch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "15GB000060100C82A8";
			NCTSTestHelper.CreateJobDocAddressForTest(factory, "TRD", header.DestinationTrader, "", "NCTS UK TEST LAB HMCE", "11TH FLOOR, ALEX HOUSE, VICTORIA AV", "SS99 1AA", "SOUTHEND-ON-SEA, ESSEX", "GBSOU", "GB", "GB954131533000");
			NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "GB000060", ZDateTime.Empty, false);
			if (header.IsArrivalMovement)
			{
				NCTSTestHelper.CreateCustomsOfficeForTest(header, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, "GB000060", ZDateTime.Empty, false);
			}
		}
	}
}
