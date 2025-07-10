using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.Registry;

namespace Enterprise.DocumentEngineCore.Testing
{
	sealed class DeliveryMethodValidationTest : TestCaseWithFactory
	{
		public void TestDeliveryMethodValidation()
		{
			var dummy = new DummyWithValidation();
			AssertNoErrors(dummy.UseEPrintInfo);

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				dummy.UseEPrint = true;
				AssertHasError(dummy.UseEPrintInfo, Res.GetString("45d17575-3ed5-454c-8ada-58fa0ec29632", "ePrint email address is not defined. This can be set in the registry setting Documents > ePrint Email Address."));
			}

			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "email@domain.com"))
			{
				dummy.UseEPrint = true;
				AssertNoErrors(dummy.UseEPrintInfo);
			}
		}
	}
}
