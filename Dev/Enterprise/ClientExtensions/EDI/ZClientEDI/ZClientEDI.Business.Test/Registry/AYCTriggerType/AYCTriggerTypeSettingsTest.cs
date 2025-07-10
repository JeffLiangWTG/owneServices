using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(AYCTriggerTypeSettings))]
	class AYCTriggerTypeSettingsTest : RegistryBusinessObjectTemplateTestCase<AYCTriggerTypeSettings>
	{
		public void TestValidatePrimaryChargeCode()
		{
			SetupChargeCodes();
			var settings = new AYCTriggerTypeSettings();

			settings.PrimaryChargeCode = "XXX";
			AssertHasErrors(settings.PrimaryChargeCodeInfo);

			settings.PrimaryChargeCode = "";
			AssertNoErrors(settings.PrimaryChargeCodeInfo);

			settings.PrimaryChargeCode = "BAD1";
			AssertHasErrors(settings.PrimaryChargeCodeInfo);

			settings.PrimaryChargeCode = "BAD2";
			AssertHasErrors(settings.PrimaryChargeCodeInfo);

			settings.PrimaryChargeCode = "GOOD1";
			AssertNoErrors(settings.PrimaryChargeCodeInfo);

			settings.PrimaryChargeCode = "GOOD2";
			AssertNoErrors(settings.PrimaryChargeCodeInfo);
		}

		public void TestValidateSecondaryChargeCode()
		{
			SetupChargeCodes();
			var settings = new AYCTriggerTypeSettings();

			settings.SecondaryChargeCode = "XXX";
			AssertHasErrors(settings.SecondaryChargeCodeInfo);

			settings.SecondaryChargeCode = "";
			AssertNoErrors(settings.SecondaryChargeCodeInfo);

			settings.SecondaryChargeCode = "BAD1";
			AssertHasErrors(settings.SecondaryChargeCodeInfo);

			settings.SecondaryChargeCode = "BAD2";
			AssertHasErrors(settings.SecondaryChargeCodeInfo);

			settings.SecondaryChargeCode = "GOOD1";
			AssertNoErrors(settings.SecondaryChargeCodeInfo);

			settings.SecondaryChargeCode = "GOOD2";
			AssertNoErrors(settings.SecondaryChargeCodeInfo);
		}

		void SetupChargeCodes()
		{
			AccChargeCode good1 = BillingTestHelper.CreateChargeCode(Factory, null, "GOOD1");
			good1.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			good1.AC_ChargeGroup = ChargeCodeGroupList.Codes.NonJobRelated;

			AccChargeCode bad1 = BillingTestHelper.CreateChargeCode(Factory, null, "BAD1");
			bad1.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			bad1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;

			AccChargeCode good2 = BillingTestHelper.CreateChargeCode(Factory, null, "GOOD2");
			good2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			good2.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;

			AccChargeCode bad2 = BillingTestHelper.CreateChargeCode(Factory, null, "BAD2");
			bad2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			bad2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;

			Factory.Save();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override AYCTriggerTypeSettings GetBusinessObjectToClone()
		{
			return new AYCTriggerTypeSettings();
		}

		protected override AYCTriggerTypeSettings GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
