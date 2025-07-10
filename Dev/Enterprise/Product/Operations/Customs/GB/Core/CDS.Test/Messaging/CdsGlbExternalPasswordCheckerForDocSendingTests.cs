using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Chief.CusDec.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.GB.Business.GbConstants;

namespace Enterprise.Customs.GB.CDS.Messaging.Testing
{
	public class CdsGlbExternalPasswordCheckerForDocSendingTests : TestCaseWithFactory
	{
		[TestDate(2015, 8, 22)]
		public void TestCheckBusinessObjectValidationAndGlbExternalPasswordValidation()
		{
			var declaration = DeclarationChosererTester.CreateMcpDeclarationSoThatItHasRequirePropertiesToNotGiveRedWarningsDuringTransmission(Factory);
			declaration.ZG_Gateway = "CDS";
			declaration.JE_TransportMode = "XXX";
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var notificationCollection = new MessageSendingNotificationCollection();
			var checker = new CdsGlbExternalPasswordCheckerWithNotifications(declaration, notificationCollection);
			_ = checker.PasswordExistsAndOkToSendToCds;
			AssertContains("No company-level CDS credentials exist", notificationCollection.ErrorNotificationsAsString());

			var password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = declaration.CompanyPK;
			password.Badge = declaration.JE_CustomsProfile;
			password.EORI = declaration.DeclarantTraderId;
			password.StatusMessage = "My Status Message";
			password.Status = PasswordStatusList.Codes.Invalid;
			password.IsTokenForCDS = true;
			notificationCollection.Clear();
			_ = checker.PasswordExistsAndOkToSendToCds;
			AssertContains("Credentials are invalid", notificationCollection.ErrorNotificationsAsString());
			AssertContains("My Status Message", notificationCollection.ErrorNotificationsAsString());

			password.Status = PasswordStatusList.Codes.Deactivated;
			notificationCollection.Clear();
			_ = checker.PasswordExistsAndOkToSendToCds;
			AssertContains("Credentials are inactive", notificationCollection.ErrorNotificationsAsString());
			AssertContains("My Status Message", notificationCollection.ErrorNotificationsAsString());

			password.Status = PasswordStatusList.Codes.Valid;
			password.GP_ExpiryDate = new CargoWise.Types.ZDate(2015, 7, 1);
			notificationCollection.Clear();
			_ = checker.PasswordExistsAndOkToSendToCds;
			AssertContains("Credentials are invalid", notificationCollection.ErrorNotificationsAsString());
			AssertContains(StatusDescriptions.ExpiredAccessToken, notificationCollection.ErrorNotificationsAsString());

			password.GP_ExpiryDate = new CargoWise.Types.ZDate(2015, 9, 1);
			password.GP_IssueDate = new CargoWise.Types.ZDate(2014, 3, 20);

			notificationCollection.Clear();
			Assert(checker.PasswordExistsAndOkToSendToCds);
			AssertContains("Your credentials were issued on", notificationCollection.WarningNotificationsAsString());

			password.IsTokenForCDS = false;
			password.GP_ExpiryDate = default;
			notificationCollection.Clear();
			_ = checker.PasswordExistsAndOkToSendToCds;
			AssertContains("No valid token that is applicable to CDS was found", notificationCollection.ErrorNotificationsAsString());

			password.IsTokenForCDS = true;
			password.GP_ExpiryDate = new CargoWise.Types.ZDate(2015, 12, 1);
			password.GP_IssueDate = new CargoWise.Types.ZDate(2014, 3, 22);
			notificationCollection.Clear();
			Assert(checker.PasswordExistsAndOkToSendToCds);
			AssertNullOrEmpty(notificationCollection.WarningNotificationsAsString());
		}
	}
}
