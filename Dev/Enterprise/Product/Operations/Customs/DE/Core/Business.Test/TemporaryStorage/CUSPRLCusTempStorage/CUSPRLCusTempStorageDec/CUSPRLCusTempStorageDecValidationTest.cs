using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPRLCusTempStorageDec))]
	class CUSPRLCusTempStorageDecValidationTest : CusTempStorageDecValidationTest<CUSPRLCusTempStorageDec>
	{
		public override void TestCheckSTH_IdentificationIndicator()
		{
			var storageDec = GetCusTempStorageDecToTest();
			CombineAssertions(() =>
			{
				foreach (var identificationIndicator in new ZString[] { "ERR", ZString.Empty })
				{
					storageDec.STH_IdentificationIndicator = identificationIndicator;
					AssertNoErrors($"No Errors for Identification Indicator {identificationIndicator}", storageDec.STH_IdentificationIndicatorInfo);
				}
			});
		}

		public void TestShouldValidateRegistrationNumberLengthAndMrnFormat()
		{
			var storageDec = GetCusTempStorageDecToTest();
			AssertEquals(false, storageDec.Validation.ShouldValidateRegistrationNumberLengthAndMrnFormat);
		}

		protected override CUSPRLCusTempStorageDec GetCusTempStorageDecToTest() => Factory.New<CUSPRLCusTempStorageDec>();
	}
}
