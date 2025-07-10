using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class MessageSendingValidationTest : TestCaseWithFactory
	{
		public void TestWarnings()
		{
			ZString fakeCyData = "QQ000001";
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			var customsOffice = movementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			customsOffice.CY_Data = fakeCyData;

			nctsHeader.RunPreSaveValidation();
			var warnings = new CustomsNotificationCollector(nctsHeader, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetWarnings();
			var validation = MessageSendingValidation.New(nctsHeader, null, warnings, Env.Security.CustomsDeclarationSendWithMessageErrors);
			var notificationsAsString = validation.CheckBusinessObjectLevelWarning().NotificationsAsString();
			Assert("Should have this warning Text prefix", notificationsAsString.StartsWith("Your message(s) have the following warnings:"));
			Assert("Should have this Warning : Not listed as a country", notificationsAsString.Contains("is not listed as a country. Check the value of your office code or check that your list of economic groupings is up to date."));
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;
	}
}
