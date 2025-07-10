using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(PaymentAuthorisationSettingsControl))]
	sealed class PaymentAuthorisationSettingsControlWithThreeLevelsAuthorisationTest : PaymentAuthorisationSettingsControlWithPaymentAuthorisationTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new PaymentThreeLevelAuthorisationSettingsCollection();
		}
	}
}
