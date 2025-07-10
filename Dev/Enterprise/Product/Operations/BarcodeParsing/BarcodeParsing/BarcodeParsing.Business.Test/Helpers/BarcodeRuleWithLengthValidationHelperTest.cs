using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeRuleWithLengthValidationHelperTest : TestCaseWithDummy
	{
		#region SetupCheckMinLength

		void SetupCheckMinLength(string lengthType, ZShort minLength, ZShort maxLength)
		{
			var mock = new Mock<IBarcodeRuleWithLength>();
			Dummy.Z0_Short = minLength;
			Dummy.Z0_ShortInfo.ClearAllNotifications();
			mock.Setup(s => s.LengthType).Returns(lengthType);
			mock.Setup(s => s.MinLength).Returns(minLength);
			mock.Setup(s => s.MaxLength).Returns(maxLength);
			mock.Setup(s => s.MinLengthInfo).Returns(Dummy.Z0_ShortInfo);
			BarcodeRuleWithLengthValidationHelper.CheckMinLength(mock.Object);
		}

		#endregion

		#region TestCheckMinLength_CheckNotNegative

		public void TestCheckMinLength_CheckNotNegative()
		{
			var lengthError = "value cannot be negative.";
			using (Dummy.SuspendValidationTesting())
			{
				SetupCheckMinLength(LengthTypes.Codes.Fixed, -1, -1);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Range, -1, -2);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Range, -1, -1);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Range, -1, 0);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Range, -1, 1);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Fixed, 1, 1);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Range, 1, 2);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);
			}
		}

		#endregion

		#region TestCheckMinLength_CheckMinLengthIsLessThanMaxLength

		public void TestCheckMinLength_CheckMinLengthIsLessThanMaxLength()
		{
			var lengthError = "Min Length cannot be greater than Max Length.";
			using (Dummy.SuspendValidationTesting())
			{
				SetupCheckMinLength(LengthTypes.Codes.Range, 10, 2);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Fixed, 2, 2);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Range, 2, 2);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Range, 2, 3);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);
			}
		}

		#endregion

		#region TestCheckMinLength_CheckIfMaxLengthIsGreaterThanZeroThenMinLengthShouldBeGreaterThanZero

		public void TestCheckMinLength_CheckIfMaxLengthIsGreaterThanZeroThenMinLengthShouldBeGreaterThanZero()
		{
			var lengthError = "Min Length should be greater than zero if Max Length is greater than zero.";
			using (Dummy.SuspendValidationTesting())
			{
				SetupCheckMinLength(LengthTypes.Codes.Range, 0, 1);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Any, 0, 0);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Fixed, 1, 1);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Range, 1, 2);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);
			}
		}

		#endregion

		#region TestCheckMinLength_CheckMaxLengthAndMinLengthGreaterThanZeroWhenLengthTypeIsNotAny

		public void TestCheckMinLength_CheckMaxLengthAndMinLengthGreaterThanZeroWhenLengthTypeIsNotAny()
		{
			var lengthError = "Min and Max Length should be greater than zero if Length Type is not Any.";
			using (Dummy.SuspendValidationTesting())
			{
				SetupCheckMinLength(LengthTypes.Codes.Fixed, 0, 0);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Range, 0, 0);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Any, 0, 0);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Fixed, 1, 1);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Range, 1, 2);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);
			}
		}

		#endregion

		#region TestCheckMinLength_CheckMaxLengthGreaterThanMinLengthWhenLengthTypeIsRange

		public void TestCheckMinLength_CheckMaxLengthGreaterThanMinLengthWhenLengthTypeIsRange()
		{
			var lengthError = "Max Length should be greater than Min Length if Length Type is Range.";
			using (Dummy.SuspendValidationTesting())
			{
				SetupCheckMinLength(LengthTypes.Codes.Range, 1, 1);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Any, 0, 0);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Fixed, 1, 1);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMinLength(LengthTypes.Codes.Range, 1, 2);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);
			}
		}

		#endregion

		#region SetupCheckMaxLength

		void SetupCheckMaxLength(string lengthType, ZShort minLength, ZShort maxLength)
		{
			var mock = new Mock<IBarcodeRuleWithLength>();
			Dummy.Z0_Short = maxLength;
			Dummy.Z0_ShortInfo.ClearAllNotifications();
			mock.Setup(s => s.LengthType).Returns(lengthType);
			mock.Setup(s => s.MinLength).Returns(minLength);
			mock.Setup(s => s.MaxLength).Returns(maxLength);
			mock.Setup(s => s.MaxLengthInfo).Returns(Dummy.Z0_ShortInfo);
			BarcodeRuleWithLengthValidationHelper.CheckMaxLength(mock.Object);
		}

		#endregion

		#region TestCheckMaxLength_CheckNotNegative

		public void TestCheckMaxLength_CheckNotNegative()
		{
			var lengthError = "value cannot be negative.";
			using (Dummy.SuspendValidationTesting())
			{
				SetupCheckMaxLength(LengthTypes.Codes.Fixed, -1, -1);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Range, -2, -1);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Range, -1, -1);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Range, 0, -1);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Range, 1, -1);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Fixed, 1, 1);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Range, 1, 2);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);
			}
		}

		#endregion

		#region TestCheckMaxLength_CheckMaxLengthIsGreaterThanMinLength

		public void TestCheckMaxLength_CheckMaxLengthIsGreaterThanMinLength()
		{
			var lengthError = "Max Length cannot be less than Min Length.";
			using (Dummy.SuspendValidationTesting())
			{
				SetupCheckMaxLength(LengthTypes.Codes.Range, 10, 2);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Fixed, 2, 2);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Range, 2, 2);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Range, 2, 3);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);
			}
		}

		#endregion

		#region TestCheckMaxLength_CheckIfMaxLengthIsGreaterThanZeroThenMinLengthShouldBeGreaterThanZero

		public void TestCheckMaxLength_CheckIfMaxLengthIsGreaterThanZeroThenMinLengthShouldBeGreaterThanZero()
		{
			var lengthError = "Min Length should be greater than zero if Max Length is greater than zero.";
			using (Dummy.SuspendValidationTesting())
			{
				SetupCheckMaxLength(LengthTypes.Codes.Range, 0, 1);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Fixed, 0, 0);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Fixed, 1, 1);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Range, 1, 2);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);
			}
		}

		#endregion

		#region TestCheckMaxLength_CheckMaxLengthAndMinLengthGreaterThanZeroWhenLengthTypeIsNotAny

		public void TestCheckMaxLength_CheckMaxLengthAndMinLengthGreaterThanZeroWhenLengthTypeIsNotAny()
		{
			var lengthError = "Min and Max Length should be greater than zero if Length Type is not Any.";
			using (Dummy.SuspendValidationTesting())
			{
				SetupCheckMaxLength(LengthTypes.Codes.Fixed, 0, 0);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Range, 0, 0);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Any, 0, 0);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Fixed, 1, 1);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Range, 1, 2);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);
			}
		}

		#endregion

		#region TestCheckMaxLength_CheckMaxLengthGreaterThanMinLengthWhenLengthTypeIsRange

		public void TestCheckMaxLength_CheckMaxLengthGreaterThanMinLengthWhenLengthTypeIsRange()
		{
			var lengthError = "Max Length should be greater than Min Length if Length Type is Range.";
			using (Dummy.SuspendValidationTesting())
			{
				SetupCheckMaxLength(LengthTypes.Codes.Range, 1, 1);
				AssertHasError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Any, 0, 0);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Fixed, 1, 1);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);

				SetupCheckMaxLength(LengthTypes.Codes.Range, 1, 2);
				AssertNoError(Dummy.Z0_ShortInfo, lengthError);
			}
		}

		#endregion
	}
}
