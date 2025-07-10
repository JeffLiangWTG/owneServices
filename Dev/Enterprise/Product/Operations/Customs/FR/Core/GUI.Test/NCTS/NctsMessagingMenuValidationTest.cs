using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration.Testing;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.Registry;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	class NctsMessagingMenuValidationTest : EU.NCTS.GUI.Testing.NctsMessagingMenuValidationTest
	{
		public void TestSendDepartureMessageWithDeltaTFallbackAnnouncedButNotActive()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(FR.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, "Fallback");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, FR.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Fallback, NctsHeader.Schema.DeltaT, ZDate.Today.AddDays(-1), ZDateTime.Today.AddDays(2));
			Factory.Save();

			using (var form = new ZForm())
			using (var menu = new NctsMessagingMenu(form))
			{
				var departure = Factory.NewWithValidTestData<NctsHeader>();
				departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				departure.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
				UnitTestUserNotification.Instance.AddAnswer(System.Windows.Forms.DialogResult.No);
				menu.NctsHeader = departure;
				Factory.Save();
				var departureMenuItem = menu.MenuItems.FindByText("Send Departure Message");
				departureMenuItem.PerformClick();
				AssertContains("Delta T fallback has been announced but you have not yet activated it in the CW1 registry.  You are advised to view this eLearning material and activate Delta T fallback as appropriate", UnitTestUserNotification.Instance.LastMessage.ToString());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestSendDepartureMessageWithDeltaTFallbackActive()
		{
			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaTMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			var departure = Factory.NewWithValidTestData<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();

			using (var form = new ZForm())
			using (var menu = new NctsMessagingMenu(form))
			{
				menu.NctsHeader = departure;
				var departureMenuItem = menu.MenuItems.FindByText("Send Departure Message");
				AssertNull(departureMenuItem);

				departureMenuItem = menu.MenuItems.FindByText("Create Fallback TAD Document");
				departureMenuItem.PerformClick();
				var headerReloaded = new BusinessObjectFactory().Load<NctsHeader>(departure.PK);
				var printJobs = Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, headerReloaded.PK));
				AssertEquals("There should be a TAD/TSAD in the transit declaration print job queue.", 1, printJobs.Length);
			}
		}

		public override void TestDepartureMandatoryErrorNotifications()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList();
			Factory.Save();
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();

			using (var nctsMovementForm = new NctsMovementForm(departure))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				Env.Security.EuNctsSendWithMessageErrors.IsAllowed = true;
				ClickMenuItem(departure, menu, "Send Departure Message");
				AssertNotContains("Please fix the following message errors before sending any messages.", UnitTestUserNotification.Instance.LastMessage.ToString());

				Env.Security.EuNctsSendWithMessageErrors.IsAllowed = false;
				ClickMenuItem(departure, menu, "Send Departure Message");
				AssertContains("Please fix the following message errors before sending any messages.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertExpectedMandatoryDepartureErrors();
			}
		}

		public override void TestArrivalValidationWithMandatoryErrors()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			using (var nctsMovementForm = new NctsMovementForm(header))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				Env.Security.EuNctsSendWithMessageErrors.IsAllowed = true;
				ClickMenuItem(header, menu, "Send Arrival Message");
				AssertMandatoryArrivalErrors(true);
				AssertNotContains("Please fix the following message errors before sending any messages.", UnitTestUserNotification.Instance.LastMessage.ToString());

				Env.Security.EuNctsSendWithMessageErrors.IsAllowed = false;
				ClickMenuItem(header, menu, "Send Arrival Message");
				AssertMandatoryArrivalErrors(true);
				AssertContains("Please fix the following message errors before sending any messages.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public override void TestMessageSendingDateIsNotOlderThanLimitDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList();
			Factory.Save();
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			departure.MovementHeader.IsSimplifiedNctsProcedure = false;

			using (var nctsMovementForm = new NctsMovementForm(departure))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				departure.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
				NctsTransmissionMessageGeneratorTests.MakeDefaultDepartureForMessagingTest(departure, Factory, addAllMandatoryData: true);

				ClickMenuItem(departure, menu, "Send Departure Message");
				AssertNotEquals("Expecting no Extra Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));

				departure.MovementHeader.IsSimplifiedNctsProcedure = true;
				departure.MovementHeader.BM_ExportDate = ZDateTime.Now.AddDays(-5);

				ClickMenuItem(departure, menu, "Send Departure Message");
				AssertEquals("Expecting Extra Errors", false, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));
				AssertEquals("Expecting Extra Errors", false, UnitTestUserNotification.Instance.LastMessage.Contains("Date-Limit must be in the future."));

				departure.MovementHeader.BM_ExportDate = ZDateTime.Now.AddDays(5);

				ClickMenuItem(departure, menu, "Send Departure Message");
				AssertNotEquals("Expecting no Extra Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Please enter the following mandatory data before sending any messages"));
				AssertNotEquals("Expecting no Extra Errors", true, UnitTestUserNotification.Instance.LastMessage.Contains("Date-Limit must be in the future."));
			}
		}

		protected override void SetupAndSendDepartureAndAssertResult(ZBool superuser)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList();
			Factory.Save();
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			using (var nctsMovementForm = new NctsMovementForm(departure))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				Env.Security.EuNctsSendWithMessageErrors.IsAllowed = superuser;
				departure = (NctsHeader)NctsTransmissionMessageGeneratorTests.MakeDefaultDepartureForMessagingTest(departure, Factory, true, "FR954131533000", true, "FR000060");
				departure.BH_FTZMove = true;
				var declarant = departure.GetDeclarantCore();

				var org = Factory.New<OrgHeader>();
				org.OH_Code = OrgCusCode.FranceCodeTypes.Siret;
				var address = Factory.New<OrgAddress>();
				address.OA_OH = org.PK;
				address.OA_Address1 = "Eugene Leroy Street ";
				declarant.E2_OA_Address = address.PK;
				var code = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "FR12312312312", Core.Constants.CountryCodes.France);
				departure.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.T1;
				Factory.Save();
				AssertNotContains("There are message errors that may cause your message to be rejected by NCTS", UnitTestUserNotification.Instance.PreviousMessages.ToString());
				ClickMenuItem(departure, menu, "Send Departure Message");
				AssertEquals("Send with message errors = true", superuser, Environment.Env.Security.EuNctsSendWithMessageErrors.IsAllowed);

				if (superuser)
				{
					AssertNotContains("Please fix the following message errors before sending any messages.", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
				else
				{
					AssertContains("Please fix the following message errors before sending any messages.", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
		}

		public override void TestDepartureNoNotifications()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNctsDeclarationTypeList();
			var departure = Factory.New<NctsHeader>();
			departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departure.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			Factory.Save();

			using (var nctsMovementForm = new NctsMovementForm(departure))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				Env.Security.EuNctsSendWithMessageErrors.IsAllowed = true;
				departure = (NctsHeader)NctsTransmissionMessageGeneratorTests.MakeDefaultDepartureForMessagingTest(departure, Factory, true, "FR954131533000", true, "FR000060");
				NCTSTestHelper.CreateJobDocAddressForTest(Factory, "CE1", departure.Consignee, "3");
				departure.BH_FTZMove = false;
				var declarant = departure.GetDeclarantCore();

				var org = Factory.New<OrgHeader>();
				org.OH_Code = OrgCusCode.FranceCodeTypes.Siret;
				var address = Factory.New<OrgAddress>();
				address.OA_OH = org.PK;
				address.OA_Address1 = "Eugene Leroy Street ";
				declarant.E2_OA_Address = address.PK;
				var code = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "FR12312312312", Core.Constants.CountryCodes.France);
				Factory.Save();

				ClickMenuItem(departure, menu, "Send Departure Message");
				AssertNotContains("Please fix the following message errors before sending any messages.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Sent", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Message Sent"));

				Env.Security.EuNctsSendWithMessageErrors.IsAllowed = false;
				ClickMenuItem(departure, menu, "Send Departure Message");
				AssertContains("Please fix the following message errors before sending any messages.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals("Sent", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("Message Sent"));
			}
		}

		public override void TestUnloadingValidationWithMandatoryErrors()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			using (var nctsMovementForm = new NctsMovementForm(header))
			using (var menu = new NctsMessagingMenu(nctsMovementForm))
			{
				Env.Security.EuNctsSendWithMessageErrors.IsAllowed = true;
				ClickMenuItem(header, menu, "Send Unloading Remarks");
				AssertMandatoryUnloadingErrors(true);
				AssertNotContains("Please fix the following message errors before sending any messages.", UnitTestUserNotification.Instance.LastMessage.ToString());

				Env.Security.EuNctsSendWithMessageErrors.IsAllowed = false;
				header = Factory.New<NctsHeader>();
				header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
				ClickMenuItem(header, menu, "Send Unloading Remarks");
				AssertMandatoryUnloadingErrors(true);
				AssertContains("Please fix the following message errors before sending any messages.", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}
		protected override void AssertExpectedMandatoryDepartureErrors()
		{
			var lastMessageText = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertContains("The declaration requires an office of type NCTS Office of departure with purpose DEP", lastMessageText);
			AssertContains("The declaration requires an office of type NCTS Office of destination with purpose DES", lastMessageText);
			AssertContains("You need to supply a valid guarantee", lastMessageText);
			AssertContains("You need to supply at least one goods item", lastMessageText);
			AssertContains("[C050] Please enter a Principal trader with an EORI or enter full address details.", lastMessageText);
		}
	}
}
