using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(BuyingGroupLicenceSetting))]
	internal class BuyingGroupLicenceSettingTest : EdiLicenceSettingTest
	{
	}

	internal class BuyingGroupLicenceSettingValidationTest : BusinessObjectValidationTestCase
	{
	}
}
