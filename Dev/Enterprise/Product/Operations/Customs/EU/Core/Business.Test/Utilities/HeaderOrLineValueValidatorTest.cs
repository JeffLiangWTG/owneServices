using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class HeaderOrLineValueValidatorTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Header and line value provider missing",
					() => GetValidator(null, null));

				AssertExceptionThrown<ArgumentNullException>("Header missing and line value provider present",
					() => GetValidator(() => "XYZ", null));

				AssertExceptionThrown<ArgumentNullException>("Header present and line value provider missing",
					() => GetValidator(null, () => new[] { "XYZ" }));

				AssertNoExceptionThrown("Header and line value provider present",
					() => GetDefaultValidator());
			});
		}

		public void TestValidateHeaderGuard()
		{
			AssertExceptionThrown<ArgumentNullException>("Property info missing",
					() => GetDefaultValidator().ValidateHeader(null));
		}

		public void TestValidateLineGuard()
		{
			AssertExceptionThrown<ArgumentNullException>("Property info missing",
					() => GetDefaultValidator().ValidateLine(null, "XYZ"));

			AssertExceptionThrown<ArgumentNullException>("Property info and line value missing",
					() => GetDefaultValidator().ValidateLine(null, null));
		}

		public void TestIsEmptyFunc()
		{
			CombineAssertions(() =>
			{
				var validator = GetDefaultValidator();
				validator.IsEmptyFunc = x => string.IsNullOrEmpty(x);
				AssertEquals("IsEmptyFunc with empty", true, validator.IsEmptyFunc(""));
				AssertEquals("IsEmptyFunc with null", true, validator.IsEmptyFunc(null));
				AssertEquals("IsEmptyFunc with value", false, validator.IsEmptyFunc("xyz"));
			});
		}

		public void TestEmptyHeaderAndLinesMessageProvider()
		{
			var validator = GetDefaultValidator();
			validator.EmptyHeaderAndLinesMessageProvider = () => "Empty header and lines";
			AssertEquals("EmptyHeaderAndLinesMessageProvider", "Empty header and lines", validator.EmptyHeaderAndLinesMessageProvider());
		}

		public void TestIgnoredHeaderSameLinesMessageProvider()
		{
			var validator = GetDefaultValidator();
			validator.IgnoredHeaderSameLinesMessageProvider = () => "Ignore header because lines has same values";
			AssertEquals("IgnoredHeaderSameLinesMessageProvider", "Ignore header because lines has same values", validator.IgnoredHeaderSameLinesMessageProvider());
		}

		public void TestIgnoredHeaderWithLinesValuesMessageProvider()
		{
			var validator = GetDefaultValidator();
			validator.IgnoredHeaderWithLinesValuesMessageProvider = () => "Ignore header because lines has values";
			AssertEquals("IgnoredHeaderWithLinesValuesMessageProvider", "Ignore header because lines has values", validator.IgnoredHeaderWithLinesValuesMessageProvider());
		}

		public void TestLineValueEmptyMessageProvider()
		{
			var validator = GetDefaultValidator();
			validator.LineValueEmptyMessageProvider = () => "Line value empty";
			AssertEquals("LineValueEmptyMessageProvider", "Line value empty", validator.LineValueEmptyMessageProvider());
		}

		public void TestFieldName()
		{
			var validator = GetDefaultValidator();
			validator.FieldName = "Field1";
			AssertEquals("FieldName", "Field1", validator.FieldName);
		}

		public void TestValidateHeaderEmptyHeaderAndLinesMessage()
		{
			CombineAssertions(() =>
			{
				const string expectedMessage = "A Field1 must be declared at header or line level.";
				var dummy = Factory.New<DummyBusinessObjectForTest>();
				var validator = GetValidator(() => "", () => new[] { "" });
				dummy.AdditionalValidationAction = info => validator.ValidateHeader(info);
				dummy.Validation.ValidateZ0_NVarChar();
				AssertHasMessageError("When header and line values are empty", dummy.Z0_NVarCharInfo, expectedMessage);

				validator = GetValidator(() => "xyz", () => new[] { "" });
				dummy.AdditionalValidationAction = info => validator.ValidateHeader(info);
				dummy.Validation.ValidateZ0_NVarChar();
				AssertNoMessageError("When header value is present and line values are empty", dummy.Z0_NVarCharInfo, expectedMessage);

				validator = GetValidator(() => "", () => new[] { "XYZ" });
				dummy.AdditionalValidationAction = info => validator.ValidateHeader(info);
				dummy.Validation.ValidateZ0_NVarChar();
				AssertNoMessageError("When header value is empty and line values present", dummy.Z0_NVarCharInfo, expectedMessage);
			});
		}

		public void TestValidateHeaderIgnoredHeaderSameLinesMessage()
		{
			CombineAssertions(() =>
			{
				const string expectedMessage = "The Field1 declared in the header will be ignored because all the lines have the same value, which differs from the value declared in the header.";
				var dummy = Factory.New<DummyBusinessObjectForTest>();
				var validator = GetValidator(() => "ABC", () => new[] { "XYZ", "XYZ" });
				dummy.AdditionalValidationAction = info => validator.ValidateHeader(info);
				dummy.Validation.ValidateZ0_NVarChar();
				AssertHasWarning("When header filled and all lines filled with same value different from header", dummy.Z0_NVarCharInfo, expectedMessage);

				validator = GetValidator(() => "", () => new[] { "XYZ", "XYZ" });
				dummy.AdditionalValidationAction = info => validator.ValidateHeader(info);
				dummy.Validation.ValidateZ0_NVarChar();
				AssertNoWarning("When header is empty and all lines filled with same value", dummy.Z0_NVarCharInfo, expectedMessage);

				validator = GetValidator(() => "ABC", () => new[] { "XYZ", "PQR" });
				dummy.AdditionalValidationAction = info => validator.ValidateHeader(info);
				dummy.Validation.ValidateZ0_NVarChar();
				AssertNoWarning("When header filled and all lines filled with different values", dummy.Z0_NVarCharInfo, expectedMessage);
			});
		}

		public void TestValidateHeaderIgnoredHeaderWithLinesValuesMessage()
		{
			CombineAssertions(() =>
			{
				const string expectedMessage = "The Field1 declared in the header will be ignored because all lines have values, which different from the header one.";
				var dummy = Factory.New<DummyBusinessObjectForTest>();
				var validator = GetValidator(() => "ABC", () => new[] { "XYZ", "PQR" });
				dummy.AdditionalValidationAction = info => validator.ValidateHeader(info);
				dummy.Validation.ValidateZ0_NVarChar();
				AssertHasWarning("When header filled and all lines filled with values different from header", dummy.Z0_NVarCharInfo, expectedMessage);

				validator = GetValidator(() => "XYZ", () => new[] { "XYZ", "PQR" });
				dummy.AdditionalValidationAction = info => validator.ValidateHeader(info);
				dummy.Validation.ValidateZ0_NVarChar();
				AssertNoWarning("When header filled and all lines filled with values one is same as header", dummy.Z0_NVarCharInfo, expectedMessage);

				validator = GetValidator(() => "ABC", () => new[] { "XYZ", "XYZ" });
				dummy.AdditionalValidationAction = info => validator.ValidateHeader(info);
				dummy.Validation.ValidateZ0_NVarChar();
				AssertNoWarning("When header filled and all lines filled with same values different from header", dummy.Z0_NVarCharInfo, expectedMessage);

				validator = GetValidator(() => "", () => new[] { "XYZ", "PQR" });
				dummy.AdditionalValidationAction = info => validator.ValidateHeader(info);
				dummy.Validation.ValidateZ0_NVarChar();
				AssertNoWarning("When header empty and all lines filled with different values", dummy.Z0_NVarCharInfo, expectedMessage);
			});
		}

		public void TestValidateLineEmptyHeaderAndLinesMessage()
		{
			CombineAssertions(() =>
			{
				const string expectedMessage = "A Field1 must be declared at header or line level.";
				var dummy = Factory.New<DummyBusinessObjectForTest>();
				var validator = GetValidator(() => "", () => new[] { "" });
				dummy.AdditionalValidationAction = info => validator.ValidateLine(info, "");
				dummy.Validation.ValidateZ0_NVarChar();
				AssertHasMessageError("When header and line values are empty", dummy.Z0_NVarCharInfo, expectedMessage);

				validator = GetValidator(() => "xyz", () => new[] { "" });
				dummy.AdditionalValidationAction = info => validator.ValidateLine(info, "");
				dummy.Validation.ValidateZ0_NVarChar();
				AssertNoMessageError("When header value is present and line values are empty", dummy.Z0_NVarCharInfo, expectedMessage);

				validator = GetValidator(() => "", () => new[] { "XYZ" });
				dummy.AdditionalValidationAction = info => validator.ValidateLine(info, "XYZ");
				dummy.Validation.ValidateZ0_NVarChar();
				AssertNoMessageError("When header value is empty and line values present", dummy.Z0_NVarCharInfo, expectedMessage);
			});
		}

		public void TestValidateLineValueEmptyMessage()
		{
			CombineAssertions(() =>
			{
				const string expectedMessage = "You have not entered a Field1.";
				var dummy = Factory.New<DummyBusinessObjectForTest>();
				var validator = GetValidator(() => "", () => new[] { "XYZ", "" });
				dummy.AdditionalValidationAction = info => validator.ValidateLine(info, "");
				dummy.Validation.ValidateZ0_NVarChar();
				AssertHasMessageError("When header and this line value is empty, other lines filled", dummy.Z0_NVarCharInfo, expectedMessage);

				validator = GetValidator(() => "", () => new[] { "", "PQR" });
				dummy.AdditionalValidationAction = info => validator.ValidateLine(info, "PQR");
				dummy.Validation.ValidateZ0_NVarChar();
				AssertNoMessageError("When header and other line value is empty, this line is filled", dummy.Z0_NVarCharInfo, expectedMessage);
			});
		}

		HeaderOrLineValueValidator<string> GetValidator(Func<string> headerValueProvider,
			Func<IEnumerable<string>> lineValuesProvider)
		{
			return new HeaderOrLineValueValidator<string>(headerValueProvider, lineValuesProvider)
			{
				FieldName = "Field1",
				IsEmptyFunc = x => string.IsNullOrEmpty(x),
			};
		}

		HeaderOrLineValueValidator<string> GetDefaultValidator()
		{
			return new HeaderOrLineValueValidator<string>(() => "XYZ", () => new[] { "XYZ" });
		}
	}

	#region DummyBusinessObject

	sealed class DummyBusinessObjectForTest : DummyBusinessObject
	{
		public DummyBusinessObjectForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public Action<ZPropertyInfo> AdditionalValidationAction { get; set; }

		protected override DummyBizoValidation GetNewValidation()
		{
			return new DummyBizoValidationForTest(this, AdditionalValidationAction);
		}
	}

	sealed class DummyBizoValidationForTest : DummyBizoValidation
	{
		public DummyBizoValidationForTest(AutoDummyBizo parent, Action<ZPropertyInfo> additionalValidationAction) : base(parent)
		{
			this.additionalValidationAction = additionalValidationAction;
		}

		readonly Action<ZPropertyInfo> additionalValidationAction;

		protected override void CheckZ0_NVarChar()
		{
			base.CheckZ0_NVarChar();
			additionalValidationAction?.Invoke(Parent.Z0_NVarCharInfo);
		}
	}

	#endregion
}
