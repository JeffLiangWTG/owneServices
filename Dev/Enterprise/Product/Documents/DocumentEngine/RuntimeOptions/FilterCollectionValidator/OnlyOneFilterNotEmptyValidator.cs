namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class OnlyOneFilterNotEmptyValidator : FilterCollectionValidator
	{
		bool IsRecursive;
		bool fIsValid;
		public override bool IsValid(FilterField filterToValidate)
		{
			if (!IsRecursive)
			{
				int nonEmptyCount = 0;
				foreach (FilterField field in Filters)
				{
					if (!field.IsEmpty)
					{
						nonEmptyCount++;
					}
				}
				fIsValid = nonEmptyCount == 1;
				try
				{
					IsRecursive = true;
					foreach (FilterField field in Filters)
					{
						if (field != filterToValidate)
						{
							field.RunPreSaveValidation();
						}
					}
				}
				finally
				{
					IsRecursive = false;
				}
			}
			return fIsValid;
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			string result = (Filters.Count == 1 ? "'" + Filters[0].DisplayName + "'" : Res.GetString("2892a120-1de0-4c57-beda-03d6634914fe", "Only one of the {0}", FieldNamesJoined));
			result += " " + Res.GetString("547a0c00-890d-49d5-8fc8-5fc8c20c8e39", "should have data.");
			return result;
		}
	}
}
