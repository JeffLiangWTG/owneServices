using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(DispatchOfficeMustBeGermanIfDeclarationIsConsolidatedDocument))]
	class DispatchOfficeMustBeGermanIfDeclarationIsConsolidatedDocumentTest : ValidationRuleAbstractTest<DispatchOfficeMustBeGermanIfDeclarationIsConsolidatedDocument>
	{
		public void TestNotification()
		{
			AssertEquals(NotificationType.MessageError, Rule.NotificationSeverity);
		}

		public override void TestIsApplied()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", false, Rule.IsApplied);
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDispatch;
				AssertEquals("OfficeOfDispatch only", false, Rule.IsApplied);

				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDelivery;
				declaration.SetConsolidatedDocument();
				AssertEquals("Is ConsolidatedDocument only", false, Rule.IsApplied);

				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDispatch;
				Assert("OfficeOfDispatch AND Is ConsolidatedDocument", Rule.IsApplied);
			});
		}

		public override void TestValidate()
		{
			CombineAssertions(() =>
			{
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfDispatch;
				declaration.SetConsolidatedDocument();
				var validationResult = Rule.Validate(country);
				Assert("Valid", validationResult.IsValid);
				AssertEquals("Valid: no message", ZString.Empty, validationResult.Message);

				country.Code = Core.Constants.CountryCodes.Italy;
				validationResult = Rule.Validate(country);
				AssertEquals("Not valid", false, validationResult.IsValid);
				AssertEquals("Not valid: message", "The entered Office Of Dispatch (DIS) has to be a German Customs Office", validationResult.Message);
			});
		}

		protected override DispatchOfficeMustBeGermanIfDeclarationIsConsolidatedDocument Rule => new DispatchOfficeMustBeGermanIfDeclarationIsConsolidatedDocument(officeCode);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			officeCode = declaration.CustomsOffices.AddNew();
			country = Factory.New<RefCountry>();
			country.Code = Core.Constants.CountryCodes.Germany;
		}
		EMCSJobDeclaration declaration;
		RefCountry country;
		EMCSOfficeCode officeCode;
	}
}
