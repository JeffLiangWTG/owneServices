using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(MinSpendLicenceSetting))]
	internal class MinSpendLicenceSettingTest : EdiLicenceSettingTest
	{
	}

	internal class MinSpendLicenceSettingValidationTest : BusinessObjectValidationTestCase
	{
	}
}
