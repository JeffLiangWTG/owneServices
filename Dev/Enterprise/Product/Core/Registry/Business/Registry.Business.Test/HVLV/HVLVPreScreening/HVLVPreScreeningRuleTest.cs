using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPreScreeningRule))]
	sealed class HVLVPreScreeningRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Validation

		public void TestValidate_ModuleType()
		{
			BizObj.RunPreSaveValidation();
			AssertHasErrors("Empty ModuleType: Has errors", BizObj.ModuleTypeInfo);

			BizObj.ModuleType = "AA";
			AssertHasErrors("Invalid ModuleType Code: Has errors", BizObj.ModuleTypeInfo);

			BizObj.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVShipment;
			AssertNoErrors("Valid Origin Country Code: No errors", BizObj.OriginCountryCodeInfo);
		}

		public void TestValidate_TransportMode()
		{
			BizObj.ETailer = string.Empty;
			BizObj.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVShipment;

			BizObj.TransportMode = "AAA";
			AssertHasErrors("Invalid Transport Mode: Has errors", BizObj.TransportModeInfo);
			BizObj.TransportMode = TransportModes.Air;
			AssertNoErrors("Valid Transport Mode: No errors", BizObj.TransportModeInfo);

			BizObj.TransportMode = string.Empty;
			AssertHasErrors("Empty Transport Mode, with no ETailer: Has errors", BizObj.TransportModeInfo);
			BizObj.ETailer = ValidEtailer;
			AssertNoErrors("Empty Transport Mode, with ETailer set: No errors", BizObj.TransportModeInfo);
			BizObj.ETailer = string.Empty;
			AssertHasErrors("Empty Transport Mode, ETailer removed: Has errors again", BizObj.TransportModeInfo);

			BizObj.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVBookingHeader;
			BizObj.TransportMode = string.Empty;
			AssertNoErrors("Empty Transport Mode, with HVLV Booking Header set to true: No errors", BizObj.TransportModeInfo);
		}

		public void TestValidate_ETailer()
		{
			const string InvalidEtailerCode = "XXYYZZ";
			var invalidEtailer = Factory.LoadTop1<IOrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, InvalidEtailerCode));
			AssertNull("Precondition, Org code should not exist in DB", invalidEtailer);

			AssertNoErrors("Empty ETailer: No errors", BizObj.ETailerInfo);
			BizObj.ETailer = InvalidEtailerCode;
			AssertHasErrors("Invalid ETailer: Has errors", BizObj.ETailerInfo);
			BizObj.ETailer = ValidEtailer;
			AssertNoErrors("Valid ETailer: No errors", BizObj.ETailerInfo);
		}

		public void TestValidate_OriginCountry()
		{
			AssertNoErrors("Empty Origin Country Code: No errors", BizObj.OriginCountryCodeInfo);
			BizObj.OriginCountryCode = "AA";
			AssertHasErrors("Invalid Origin Country Code: Has errors", BizObj.OriginCountryCodeInfo);
			BizObj.OriginCountryCode = CountryCodes.Australia;
			AssertNoErrors("Valid Origin Country Code: No errors", BizObj.OriginCountryCodeInfo);
		}

		public void TestValidate_DestinationCountry()
		{
			AssertNoErrors("Empty Desination Country Code: No errors", BizObj.DestinationCountryCodeInfo);
			BizObj.DestinationCountryCode = "AA";
			AssertHasErrors("Invalid Desination Country Code: Has errors", BizObj.DestinationCountryCodeInfo);
			BizObj.DestinationCountryCode = CountryCodes.Australia;
			AssertNoErrors("Valid Desination Country Code: No errors", BizObj.DestinationCountryCodeInfo);
		}

		public void TestValidate_EmailNotificationType()
		{
			AssertNoErrors("Empty Email Notification Type: No errors", BizObj.EmailNotificationTypeInfo);
			BizObj.EmailNotificationType = "XYZ";
			AssertHasErrors("Invalid Email Notification Type: Has errors", BizObj.EmailNotificationTypeInfo);
			BizObj.EmailNotificationType = EmailTo.NoEmails;
			AssertNoErrors("Valid Desination Country Code: No errors", BizObj.EmailNotificationTypeInfo);
		}

		public void TestValidate_EmailNotificationGroup()
		{
			const string InvalidGroupCode = "XXYYZZ";
			var invalidNotificationGroup = Factory.LoadTop1<IGlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, InvalidGroupCode));
			AssertNull("Precondition, Group code should not exist in DB", invalidNotificationGroup);

			AssertNoErrors("Empty Notification Group: No errors", BizObj.EmailNotificationGroupInfo);
			BizObj.EmailNotificationType = EmailTo.NominatedGroup;
			BizObj.EmailNotificationGroup = InvalidGroupCode;
			AssertHasErrors("Invalid Notification Group: Has errors", BizObj.EmailNotificationGroupInfo);

			var validNotificationGroup = SetupEmailNotificationGroup();
			BizObj.EmailNotificationGroup = validNotificationGroup.GG_Code;
			AssertNoErrors("Valid Notification Group: No errors", BizObj.EmailNotificationGroupInfo);
		}

		public void TestEmailNotificationTypesList()
		{
			var validationRule = new HVLVPreScreeningRule();
			var expectedTypes = new[]
			{
				EmailTo.NoEmails,
				EmailTo.NominatedGroup,
				EmailTo.StaffMember,
				EmailTo.StaffMemberAndNominatedGroup
			};

			AssertContainsExactElementsInAnyOrder(expectedTypes, validationRule.EmailNotificationTypesList.GetAllCodes());
		}

		public void TestEmailNotificationGroup_WhenEmailNotificationTypeIsNotENGOrESG_IsEmptyAndReadOnly()
		{
			var rule = new HVLVDetailsPreScreeningConfiguration().Rules.AddNew();
			rule.EmailNotificationType = EmailTo.NominatedGroup;
			var testGroup = SetupEmailNotificationGroup();
			rule.EmailNotificationGroup = testGroup.GG_Code;
			AssertEquals("Precondition: Email Notification Group has value", testGroup.GG_Code, rule.EmailNotificationGroup);
			AssertEquals("Precondition: Email Notification Group is not read-only", false, rule.EmailNotificationGroupInfo.ReadOnly);

			rule.EmailNotificationType = EmailTo.NoEmails;
			AssertEquals("Email Notification Group should be cleared", string.Empty, rule.EmailNotificationGroup);
			AssertEquals("Email Notification Group should be read-only", true, rule.EmailNotificationGroupInfo.ReadOnly);
		}

		#endregion

		public void TestSetDefaultModuleType()
		{
			var rule = new HVLVPreScreeningRule();
			AssertEquals("precondition : module type is empty", string.Empty, rule.ModuleType);

			rule.SetDefaultModuleType();
			AssertEquals("default module type is SHP", HVLVPreScreeningRule.ModuleTypeCodes.HVLVShipment, rule.ModuleType);
		}

		public void TestSetDefaultEmailNotificationType()
		{
			var rule = new HVLVPreScreeningRule();
			AssertEquals("precondition : Email Notification Type  is empty", string.Empty, rule.EmailNotificationType);

			rule.SetDefaultEmailNotificationType();
			AssertEquals("default Email Notification Type is NOE", EmailTo.NoEmails, rule.EmailNotificationType);
		}

		public void TestPreScreeningRuleModuleType_BookingHeader()
		{
			var rule = new HVLVPreScreeningRule();
			rule.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVBookingHeader;

			CombineAssertions("apply to bookingHeader", () =>
			{
				Assert("apply to bookingHeader", rule.isApplyBookingHeader);
				Assert("not apply to shipment", !rule.isApplyShipment);
			});
		}

		public void TestPreScreeningRuleModuleType_Shipment()
		{
			var rule = new HVLVPreScreeningRule();
			rule.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVShipment;

			CombineAssertions("apply to shipment", () =>
			{
				Assert("apply to shipment", rule.isApplyShipment);
				Assert("not apply to bookingHeader", !rule.isApplyBookingHeader);
			});
		}

		public void TestCheckDuplicateValuesCombination()
		{
			var validationConfiguration = new HVLVDetailsPreScreeningConfiguration();
			var validationRule1 = validationConfiguration.Rules.AddNew();
			validationRule1.TransportMode = TransportModes.Air;
			validationRule1.ETailer = ValidEtailer;
			validationRule1.OriginCountryCode = CountryCodes.Australia;
			validationRule1.DestinationCountryCode = CountryCodes.UnitedStates;
			validationRule1.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVShipment;
			validationRule1.EmailNotificationType = EmailTo.NoEmails;
			validationRule1.EmailNotificationGroup = string.Empty;

			var validationRule2 = validationConfiguration.Rules.AddNew();
			validationRule2.TransportMode = TransportModes.Air;
			validationRule2.ETailer = ValidEtailer;
			validationRule2.OriginCountryCode = CountryCodes.Australia;
			validationRule2.DestinationCountryCode = CountryCodes.UnitedStates;
			validationRule2.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVShipment;
			validationRule2.EmailNotificationType = EmailTo.NoEmails;
			validationRule2.EmailNotificationGroup = string.Empty;

			validationRule1.RunPreSaveValidation();
			validationRule2.RunPreSaveValidation();

			AssertHasRowError(validationRule1, "Duplicate value combination (Transport Mode,eTailer,Origin Country/Region,Destination Country/Region,Module Type,Email Notification Type,Email Notification Group) are entered.");
			AssertHasRowError(validationRule2, "Duplicate value combination (Transport Mode,eTailer,Origin Country/Region,Destination Country/Region,Module Type,Email Notification Type,Email Notification Group) are entered.");

			validationRule1.TransportMode = TransportModes.Sea;
			validationRule1.RunPreSaveValidation();
			validationRule2.RunPreSaveValidation();

			AssertNoRowErrors(validationRule1);
			AssertNoRowErrors(validationRule2);

			validationRule2.TransportMode = TransportModes.Sea;
			validationRule2.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVBookingHeader;
			validationRule1.RunPreSaveValidation();
			validationRule2.RunPreSaveValidation();

			AssertNoRowErrors(validationRule1);
			AssertNoRowErrors(validationRule2);
		}

		public void TestTransportModeList()
		{
			var validationRule = new HVLVPreScreeningRule();
			var expectedModes = new[]
			{
				TransportModes.Air,
				TransportModes.Sea,
				TransportModes.SeaAir,
				TransportModes.AirSea,
				TransportModes.Road,
				TransportModes.Rail,
				TransportModes.Courier
			};

			AssertContainsExactElementsInAnyOrder(expectedModes, validationRule.TransportModeList.GetAllCodes());
		}

		public void TestTransportMode_WhenHVLVBookingHeaderSet_ShouldBeEmptyAndReadOnly()
		{
			var rule = new HVLVDetailsPreScreeningConfiguration().Rules.AddNew();
			rule.TransportMode = TransportModes.Sea;
			AssertEquals("pre condition", TransportModes.Sea, rule.TransportMode);
			AssertEquals("pre condition", false, rule.TransportModeInfo.ReadOnly);

			rule.ModuleType = HVLVPreScreeningRule.ModuleTypeCodes.HVLVBookingHeader;
			AssertEquals("Transport Mode should be cleared", string.Empty, rule.TransportMode);
			AssertEquals("Transport Mode should be read-only", true, rule.TransportModeInfo.ReadOnly);
		}

		#region Implementation

		string ValidEtailer => validEtailer ?? (validEtailer = Factory.LoadTop1<IOrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true)).Code);
		string validEtailer;

		IGlbGroup SetupEmailNotificationGroup()
		{
			string testGroupCode = "tst";
			IGlbGroup group = Factory.LoadTop1<IGlbGroup>(
				new ZQuery(GlbGroupSchema.GG_Code, SQLComparisonOperator.Equal, testGroupCode));
			if (group != null)
			{
				((EnterpriseBusinessObject)group).Delete();
			}
			IGlbGroup newNotificationGroup = Factory.New<IGlbGroup>();
			newNotificationGroup.GG_Code = testGroupCode;
			Factory.Save();

			return newNotificationGroup;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => GetBusinessObjectToSerialise();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.TransportMode = TransportModes.Sea;
			BizObj.ETailer = ValidEtailer;
			BizObj.OriginCountryCode = CountryCodes.Australia;
			BizObj.DestinationCountryCode = CountryCodes.UnitedStates;

			return BizObj;
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		new HVLVPreScreeningRule BizObj
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (HVLVPreScreeningRule)base.BizObj; }
		}
		#endregion
	}
}
