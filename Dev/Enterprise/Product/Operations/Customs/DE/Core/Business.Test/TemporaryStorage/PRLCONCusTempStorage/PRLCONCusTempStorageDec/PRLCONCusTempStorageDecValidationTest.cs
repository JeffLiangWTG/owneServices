using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(PRLCONCusTempStorageDec))]
	class PRLCONCusTempStorageDecValidationTest : CusTempStorageDecValidationTest<PRLCONCusTempStorageDec>
	{
		public new void TestCheckSTH_IdentificationIndicator()
		{
			base.TestCheckSTH_IdentificationIndicator();

			var twoLinesRequireForREGMessageError = "For Identification Type 'REG' a minimum of two lines are required to consolidate";
			var dec = GetCusTempStorageDecToTest();
			var info = dec.STH_IdentificationIndicatorInfo;

			dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			AssertHasMessageError(info, twoLinesRequireForREGMessageError);

			dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			AssertNoMessageError(info, twoLinesRequireForREGMessageError);

			dec.CusTempStorageLines.AddNew();

			dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			AssertHasMessageError(info, twoLinesRequireForREGMessageError);

			dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			AssertNoMessageError(info, twoLinesRequireForREGMessageError);

			dec.CusTempStorageLines.AddNew();

			dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			AssertNoMessageError(info, twoLinesRequireForREGMessageError);

			dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			AssertNoMessageError(info, twoLinesRequireForREGMessageError);
		}

		public void TestShouldValidateRegistrationNumberLengthAndMrnFormat()
		{
			var validation = GetCusTempStorageDecToTest().Validation;
			AssertEquals(false, validation.ShouldValidateRegistrationNumberLengthAndMrnFormat);
		}

		protected override PRLCONCusTempStorageDec GetCusTempStorageDecToTest() => Factory.New<PRLCONCusTempStorageDec>();
	}
}
