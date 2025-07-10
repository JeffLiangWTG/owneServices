namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class AtLeastOneFilterNotEmptyValidator : FilterCollectionValidator
	{
		bool IsRecursive;
		bool fIsValid;

		public override bool IsValid(FilterField filterToValidate)
		{
			if (!IsRecursive)
			{
				fIsValid = false;
				foreach (FilterField field in Filters)
				{
					if (!field.IsEmpty && IsAdditionalValidationSuccessful(field))
					{
						fIsValid = true;
						break;
					}
				}
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

		protected virtual bool IsAdditionalValidationSuccessful(FilterField filterToValidate)
		{
			return true;
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			string result = (Filters.Count == 1 ? "'" + Filters[0].DisplayName + "'" : Res.GetString("7fdf8c0b-5d61-4379-93fc-aace9f7fb901", "At least one of the {0}", FieldNamesJoined));
			result += " " + Res.GetString("ee82e998-c8b0-4162-883a-3803bd929906", "should have data.");
			return result;
		}
	}
}
