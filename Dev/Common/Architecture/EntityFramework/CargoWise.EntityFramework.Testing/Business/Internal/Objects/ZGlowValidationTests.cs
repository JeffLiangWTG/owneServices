using CargoWise.Common;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZGlowValidationTests : TestCaseWithDummy
	{
		public void TestValidate_SingleRule()
		{
			var engine = new DummyRulesProvider()
				.AddRule(typeof(DummyBusinessObject), "Z0_Number", nameof(TestValidationMethods.GreaterThan), "<Z0_Number>", 0)
				.ToProvider();

			var validation = new ZGlowValidation(typeof(DummyBusinessObject), engine);
			Dummy.Z0_Number = 23;

			validation.Validate(Dummy.Z0_NumberInfo);
			AssertNoError(Dummy.Z0_NumberInfo, "Must be greater than 0");

			Dummy.Z0_Number = -2;

			validation.Validate(Dummy.Z0_NumberInfo);
			AssertHasError(Dummy.Z0_NumberInfo, "Must be greater than 0");
		}

		public void TestValidate_MethodReturnsValidResult()
		{
			var engine = new DummyRulesProvider()
				.AddRule(typeof(DummyBusinessObject), "Z0_NVarChar", nameof(TestValidationMethods.WarnIfNotEqual), "<Z0_NVarChar>", "X")
				.ToProvider();

			var validation = new ZGlowValidation(typeof(DummyBusinessObject), engine);

			Dummy.Z0_NVarChar = "X";
			validation.Validate(Dummy.Z0_NVarCharInfo);
			AssertNoWarnings(Dummy.Z0_NVarCharInfo);

			Dummy.Z0_NVarChar = "Z";
			validation.Validate(Dummy.Z0_NVarCharInfo);
			AssertHasWarning(Dummy.Z0_NVarCharInfo, "'Z' is not the same as 'X'");
		}

		public void TestValidate_MultipleRulesForOneObject()
		{
			var engine = new DummyRulesProvider()
				.AddRule(typeof(DummyBusinessObject), "Z0_Number", "GreaterThan", "<Z0_Number>", 0)
				.AddRule(typeof(DummyBusinessObject), "Z0_Number", "GreaterThan", "<Z0_Number>", 5)
				.ToProvider();

			var validation = new ZGlowValidation(typeof(DummyBusinessObject), engine);
			Dummy.Z0_Number = 6;

			validation.Validate(Dummy.Z0_NumberInfo);
			AssertNoError(Dummy.Z0_NumberInfo, "Must be greater than 0");
			AssertNoError(Dummy.Z0_NumberInfo, "Must be greater than 5");

			Dummy.Z0_Number = 3;

			validation.Validate(Dummy.Z0_NumberInfo);
			AssertNoError(Dummy.Z0_NumberInfo, "Must be greater than 0");
			AssertHasError(Dummy.Z0_NumberInfo, "Must be greater than 5");

			Dummy.Z0_Number = -1;

			validation.Validate(Dummy.Z0_NumberInfo);
			AssertHasError(Dummy.Z0_NumberInfo, "Must be greater than 0");
			AssertHasError(Dummy.Z0_NumberInfo, "Must be greater than 5");
		}

		public void TestValidate_SelectedCorrectProperty()
		{
			var engine = new DummyRulesProvider()
				.AddRule(typeof(DummyBusinessObject), "Z0_Number", "GreaterThan", "<Z0_Number>", 0)
				.AddRule(typeof(DummyBusinessObject), "Z0_AnotherNumber", "GreaterThan", "<Z0_AnotherNumber>", 0)
				.ToProvider();

			var validation = new ZGlowValidation(typeof(DummyBusinessObject), engine);

			Dummy.Z0_Number = -1;
			Dummy.Z0_AnotherNumber = -1;

			validation.Validate(Dummy.Z0_NumberInfo);
			RemoveChangedValidationOutsideOfCheckWarning();

			AssertHasError(Dummy.Z0_NumberInfo, "Must be greater than 0");
			AssertNoError(Dummy.Z0_AnotherNumberInfo, "Must be greater than 0");

			validation.Validate(Dummy.Z0_AnotherNumberInfo);
			RemoveChangedValidationOutsideOfCheckWarning();

			AssertHasError(Dummy.Z0_AnotherNumberInfo, "Must be greater than 0");
		}

		public void TestValidate_Warning()
		{
			var engine = new DummyRulesProvider()
				.AddRule(typeof(DummyBusinessObject), "Z0_Number", "AddWarning")
				.ToProvider();

			var validation = new ZGlowValidation(typeof(DummyBusinessObject), engine);
			validation.Validate(Dummy.Z0_NumberInfo);

			AssertHasWarning(Dummy.Z0_NumberInfo, "Be warned...");
		}

		void RemoveChangedValidationOutsideOfCheckWarning()
		{
			// We're testing the validation code by hand, hence we wont ever be in a check method.
			if (ErrorReporter.TotalErrorCount == 1 && ErrorReporter.LastMessageReported.StartsWith("Attempt to change validation on a property info outside of its Check method"))
			{
				ErrorReporter.Clear();
			}
		}

		protected override void TearDown()
		{
			base.TearDown();

			RemoveChangedValidationOutsideOfCheckWarning();
		}
	}
}
