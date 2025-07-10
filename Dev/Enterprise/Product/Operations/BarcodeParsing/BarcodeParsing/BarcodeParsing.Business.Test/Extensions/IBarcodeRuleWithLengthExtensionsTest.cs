using CargoWise.Types;
using Moq;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class IBarcodeRuleWithLengthExtensionsTest : BarcodeParsingTestCase
	{
		public void TestCalculateLengthType()
		{
			var mockObj = new Mock<IBarcodeRuleWithLength>();
			var calculateLengthType = (bool isInDatabase, ZShort min, ZShort max) =>
			{
				mockObj.SetupProperty(s => s.MinLength, min);
				mockObj.SetupProperty(s => s.MaxLength, max);
				return mockObj.Object.CalculateLengthType(isInDatabase);
			};

			AssertEquals(LengthTypes.Codes.Any, calculateLengthType(true, 0, 0));
			AssertEquals(LengthTypes.Codes.Range, calculateLengthType(true, 0, 1));
			AssertEquals(LengthTypes.Codes.Range, calculateLengthType(true, 1, 0));
			AssertEquals(LengthTypes.Codes.Fixed, calculateLengthType(true, 1, 1));

			AssertEquals(LengthTypes.Codes.Range, calculateLengthType(false, 0, 0));
			AssertEquals(LengthTypes.Codes.Range, calculateLengthType(false, 0, 1));
			AssertEquals(LengthTypes.Codes.Range, calculateLengthType(false, 1, 0));
			AssertEquals(LengthTypes.Codes.Range, calculateLengthType(false, 1, 1));
		}

		public void TestIsLengthTypeAny()
		{
			var mockObj = new Mock<IBarcodeRuleWithLength>();
			mockObj.Setup(s => s.LengthType).Returns(LengthTypes.Codes.Any);
			AssertEquals(true, mockObj.Object.IsLengthTypeAny());

			mockObj.Setup(s => s.LengthType).Returns(LengthTypes.Codes.Range);
			AssertEquals(false, mockObj.Object.IsLengthTypeAny());

			mockObj.Setup(s => s.LengthType).Returns(LengthTypes.Codes.Fixed);
			AssertEquals(false, mockObj.Object.IsLengthTypeAny());
		}

		public void TestIsLengthTypeRange()
		{
			var mockObj = new Mock<IBarcodeRuleWithLength>();
			mockObj.Setup(s => s.LengthType).Returns(LengthTypes.Codes.Any);
			AssertEquals(false, mockObj.Object.IsLengthTypeRange());

			mockObj.Setup(s => s.LengthType).Returns(LengthTypes.Codes.Range);
			AssertEquals(true, mockObj.Object.IsLengthTypeRange());

			mockObj.Setup(s => s.LengthType).Returns(LengthTypes.Codes.Fixed);
			AssertEquals(false, mockObj.Object.IsLengthTypeRange());
		}

		public void TestSetMinAndMaxLength()
		{
			var mockObj = new Mock<IBarcodeRuleWithLength>();
			mockObj.SetupProperty(s => s.MinLength, (ZShort)1);
			mockObj.SetupProperty(s => s.MaxLength, (ZShort)2);
			mockObj.Setup(s => s.LengthType).Returns(LengthTypes.Codes.Any);
			mockObj.Object.SetMinAndMaxLength();
			AssertEquals(0, (int)mockObj.Object.MinLength);
			AssertEquals(0, (int)mockObj.Object.MaxLength);

			mockObj.SetupProperty(s => s.MinLength, (ZShort)1);
			mockObj.SetupProperty(s => s.MaxLength, (ZShort)3);
			mockObj.Setup(s => s.LengthType).Returns(LengthTypes.Codes.Fixed);
			mockObj.Object.SetMinAndMaxLength();
			AssertEquals(3, (int)mockObj.Object.MinLength);
			AssertEquals(3, (int)mockObj.Object.MaxLength);

			mockObj.SetupProperty(s => s.MinLength, (ZShort)4);
			mockObj.SetupProperty(s => s.MaxLength, (ZShort)5);
			mockObj.Setup(s => s.LengthType).Returns(LengthTypes.Codes.Range);
			mockObj.Object.SetMinAndMaxLength();
			AssertEquals(4, (int)mockObj.Object.MinLength);
			AssertEquals(5, (int)mockObj.Object.MaxLength);
		}

		public void TestSetLengthForLengthTypeFixed()
		{
			var mockObj = new Mock<IBarcodeRuleWithLength>();
			mockObj.SetupProperty(s => s.MinLength, (ZShort)1);
			mockObj.SetupProperty(s => s.MaxLength, (ZShort)2);
			mockObj.Setup(s => s.LengthType).Returns(LengthTypes.Codes.Any);
			mockObj.Object.SetLengthForLengthTypeFixed(3);
			AssertEquals(1, (int)mockObj.Object.MinLength);
			AssertEquals(2, (int)mockObj.Object.MaxLength);

			mockObj.SetupProperty(s => s.MinLength, (ZShort)1);
			mockObj.SetupProperty(s => s.MaxLength, (ZShort)2);
			mockObj.Setup(s => s.LengthType).Returns(LengthTypes.Codes.Fixed);
			mockObj.Object.SetLengthForLengthTypeFixed(3);
			AssertEquals(3, (int)mockObj.Object.MinLength);
			AssertEquals(3, (int)mockObj.Object.MaxLength);

			mockObj.SetupProperty(s => s.MinLength, (ZShort)1);
			mockObj.SetupProperty(s => s.MaxLength, (ZShort)2);
			mockObj.Setup(s => s.LengthType).Returns(LengthTypes.Codes.Range);
			mockObj.Object.SetLengthForLengthTypeFixed(3);
			AssertEquals(1, (int)mockObj.Object.MinLength);
			AssertEquals(2, (int)mockObj.Object.MaxLength);
		}
	}
}
