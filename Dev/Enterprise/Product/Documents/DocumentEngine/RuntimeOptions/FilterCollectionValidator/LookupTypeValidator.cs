namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class LookupTypeValidator : FilterCollectionValidator
	{
		readonly string filterDescription;

		public LookupTypeValidator(string filterDescription)
		{
			this.filterDescription = filterDescription;
		}

		public override bool IsValid(FilterField filterToValidate)
		{
			LookupField filter = filterToValidate as LookupField;
			return filter == null || !filter.IsFiltered;
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			return string.IsNullOrEmpty(filterDescription) ? Res.GetString("24b1d6cb-dfd0-43fd-9f37-e1574ffbdb3d", "Value doesn't match lookup criteria.") : filterDescription;
		}
	}
}
