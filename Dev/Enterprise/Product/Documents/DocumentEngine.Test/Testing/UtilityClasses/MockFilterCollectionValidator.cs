using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	class MockFilterCollectionValidator : FilterCollectionValidator
	{
		public override bool IsValid(FilterField filterToValidate)
		{
			return isValid;
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			return ErrorMessage;
		}

		public void SetIsValid(bool value)
		{
			isValid = value;
		}

		public const string ErrorMessage = "This mock is in error.";
		bool isValid;
	}
}
