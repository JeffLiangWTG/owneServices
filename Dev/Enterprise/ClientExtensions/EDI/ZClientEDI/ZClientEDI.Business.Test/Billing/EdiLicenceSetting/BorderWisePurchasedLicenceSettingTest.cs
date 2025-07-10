using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(BorderWisePurchasedLicenceSetting))]
	internal class BorderWisePurchasedLicenceSettingTest : EdiLicenceSettingTest
	{
	}

	internal class BorderWisePurchasedLicenceSettingValidationTest : BusinessObjectValidationTestCase
	{
	}
}
