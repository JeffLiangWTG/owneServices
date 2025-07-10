namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class RequiredFilterValidator : FilterCollectionValidator
	{
		public override bool IsValid(FilterField filterToValidate)
		{
			return !filterToValidate.IsEmpty;
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			return Res.GetString("fa19e858-a31a-4fe2-a358-be0ba881356f", "'{0}' should have data.", filterToValidate.DisplayNameLocalized);
		}
	}
}
