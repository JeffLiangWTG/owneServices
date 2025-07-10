using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Environment;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class MessageSendingValidationTest : TestCaseWithFactory
	{
		public void TestWarnings()
		{
			var notificationsAsString = validation.CheckBusinessObjectLevelWarning().NotificationsAsString();
			Assert("Should have this warningText prefix", notificationsAsString.StartsWith("Your message(s) have the following warnings:"));
			Assert("Should have this Warning : Declarant's Ref is Empty", notificationsAsString.Contains("If Declarant's Reference is empty, Declaration Reference"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OwnerRef = "";
			var warnings = new CustomsNotificationCollector(declaration, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetWarnings();
			validation = MessageSendingValidation.New(declaration, null, warnings, Env.Security.CustomsDeclarationSendWithMessageErrors);
		}

		JobDeclaration declaration;
		MessageSendingValidation validation;
	}
}
