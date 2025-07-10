using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ConversionCreditLicenceSetting))]
	internal class ConversionCreditLicenceSettingTest : EdiLicenceSettingTest
	{
	}

	internal class ConversionCreditLicenceSettingValidationTest : BusinessObjectValidationTestCase
	{
	}
}
