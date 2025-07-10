using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class EdiPriceHeaderLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPHL_VolumeCode()
		{
			var link = Factory.NewWithValidTestData<EdiPriceHeaderLink>();
			link.PHL_VolumeCode = "";
			AssertHasError(link.PHL_VolumeCodeInfo, "Please enter a value.");

			link.PHL_VolumeCode = "aaa";
			AssertHasError(link.PHL_VolumeCodeInfo, "Enter a valid selection.");

			link.PHL_VolumeCode = "HV";
			AssertNoErrors(link.PHL_VolumeCodeInfo);
		}

		public void TestCheckPHL_CorePackCode()
		{
			var link = Factory.NewWithValidTestData<EdiPriceHeaderLink>();
			link.PHL_CorePackCode = "";
			AssertHasError(link.PHL_CorePackCodeInfo, "Please enter a value.");

			link.PHL_CorePackCode = "aaa";
			AssertHasError(link.PHL_CorePackCodeInfo, "Enter a valid selection.");

			link.PHL_CorePackCode = "EX";
			AssertNoErrors(link.PHL_CorePackCodeInfo);
		}

		public void TestCheckPHL_CoreUpliftPercent()
		{
			var link = Factory.NewWithValidTestData<EdiPriceHeaderLink>();
			link.PHL_CoreUpliftPercent = 0m;
			AssertNoErrors(link.PHL_CoreUpliftPercentInfo);

			link.PHL_CoreUpliftPercent = -999m;
			AssertHasError(link.PHL_CoreUpliftPercentInfo, "Please enter a 'Core Uplift Percent' within the range -99 to 500.");

			link.PHL_CoreUpliftPercent = 999m;
			AssertHasError(link.PHL_CoreUpliftPercentInfo, "Please enter a 'Core Uplift Percent' within the range -99 to 500.");

			link.PHL_CoreUpliftPercent = 50m;
			AssertNoErrors(link.PHL_CoreUpliftPercentInfo);

			link.PHL_CoreUpliftPercent = -99m;
			AssertNoErrors(link.PHL_CoreUpliftPercentInfo);

			link.PHL_CoreUpliftPercent = 500m;
			AssertNoErrors(link.PHL_CoreUpliftPercentInfo);
		}

		public void TestCheckPHL_ValidTo()
		{
			var link = Factory.NewWithValidTestData<EdiPriceHeaderLink>();
			link.PHL_ValidFrom = new ZDateTime(2018, 5, 1);
			link.PHL_ValidTo = new ZDateTime(2018, 4, 1);
			AssertHasErrors(link.PHL_ValidToInfo);

			link.PHL_ValidTo = ZDateTime.Empty;
			AssertNoErrors(link.PHL_ValidToInfo);
		}

		public void TestCheckPHL_L6()
		{
			var l6 = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			l6.L6_SystemCode = "STL";
			AssertEquals(false, PriceHeaderType.IsGlobal(l6.L6_SystemCode));

			var link = Factory.NewWithValidTestData<EdiPriceHeaderLink>();
			link.PHL_L6 = l6.PK;
			AssertNoErrors(link.PHL_L6Info);

			l6.L6_SystemCode = "FMS";
			AssertEquals(true, PriceHeaderType.IsGlobal(l6.L6_SystemCode));
			link.PHL_L6 = ZGuid.Empty;
			link.PHL_L6 = l6.PK;
			AssertHasError(link.PHL_L6Info, "Universal Pricelist cannot be setup as a client pricelist.");
		}
	}
}
