using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using ManifestValidationRuleCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class ConstantsAndUtilitiesTest : TestCaseWithFactory
	{
		public void TestGetNumberWithHyphen()
		{
			AssertEquals("ABC-12345678", AsycudaManifestHeaderHelper.GetNumberWithHyphen("!ABC--1234 56789"));
		}

		public void TestGetMessageSendingNotification()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SriLanka, "Sri Lanka", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var sg = helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SolomonIslands, "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sg.PK, RefCusCodeListTypes.Codes.NVC, "17.3.29.1");

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SolomonIslands, RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Honiara Point Cruz Seaport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, Core.Constants.TransportModes.Air, "SBHIR");

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var cusCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SolomonIslands, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.OfficeCode, "A Customs Office Code is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.Mandatory, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.SolomonIslands, RefCusCodeListTypes.Codes.ManifestValidationRule);
			cusCodeList.Attributes.AddNew(ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var cusCodeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SolomonIslands, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.CurrentUserEmailAddress, "An email address is required for the current user", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.Mandatory, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.SolomonIslands, RefCusCodeListTypes.Codes.ManifestValidationRule);
			cusCodeList2.Attributes.AddNew(ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var wcoDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SolomonIslands, parent: wcoDataGrouping);

			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = ZString.Empty;
			Factory.Save();
			using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SolomonIslands))
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SolomonIslands;
				var bill = header.Bills.AddNew();
				header.AMA_ManifestType = "ASY";
				AssertEquals(ValidationConstants.MissingCustomsOffice("Solomon Islands"), header.MessageSendingNotificationHelper.GetNotifications());
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals(ValidationConstants.MissingEmailAddress, header.MessageSendingNotificationHelper.GetNotifications());
				staff.GS_EmailAddress = "bob@where.com";
				Factory.Save();
				header.AMA_ManifestType = "ASY";
				AssertEquals(ZString.Empty, header.MessageSendingNotificationHelper.GetNotifications());
				header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
				AssertEquals("To create a message for South Africa you must link the current logged in company to the ZA Branch in the Registry, Customs>South Africa>Default Branch for Manifest Submission. This is so that the correct OrgProxy is selected for determining the Manifest EDI Profile.", header.MessageSendingNotificationHelper.GetNotifications());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var headerZA = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
				headerZA.AMA_TransportMode = Core.Constants.TransportModes.Air;
				headerZA.Bills.AddNew();
				staff.GS_EmailAddress = ZString.Empty;
				Factory.Save();
				AssertEquals(ZString.Empty, headerZA.MessageSendingNotificationHelper.GetNotifications());

				headerZA.AMA_ManifestType = "ABC";
				AssertEquals(ValidationConstants.MustHaveManifestType, headerZA.MessageSendingNotificationHelper.GetNotifications());

				var headerSB = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SolomonIslands, "ASY");
				headerSB.Bills.AddNew();
				AssertEquals(ValidationConstants.MissingCustomsOffice("Solomon Islands"), headerSB.MessageSendingNotificationHelper.GetNotifications());
				headerSB.AMA_CustomsOffice = "SBHIR";
				AssertEquals(ValidationConstants.MissingEmailAddress, headerSB.MessageSendingNotificationHelper.GetNotifications());
				staff.GS_EmailAddress = "bob@where.com";
				Factory.Save();
				AssertEquals(ZString.Empty, headerSB.MessageSendingNotificationHelper.GetNotifications());
			}
		}

		public void TestCreateNewAsycudaManifestHeader()
		{
			Assert(AsycudaManifestHeaderHelper.CreateNew(Factory, ZString.Empty, ZString.Empty) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader);
			Assert(AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Eritrea, ZString.Empty) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader);
			Assert(AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM") is Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader);
			Assert(AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI") is Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader);
			Assert(AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB") is Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader);
		}
	}
}
