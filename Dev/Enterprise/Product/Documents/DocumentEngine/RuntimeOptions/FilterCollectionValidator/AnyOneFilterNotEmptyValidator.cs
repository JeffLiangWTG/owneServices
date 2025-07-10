using System.Linq;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class AnyOneFilterNotEmptyValidator : FilterCollectionValidator
	{
		bool isRecursive;
		bool isValid;
		public override bool IsValid(FilterField filterToValidate)
		{
			if (!isRecursive)
			{
				var castedFields = Filters.Cast<FilterField>();
				isValid = castedFields.Count(field => !field.IsEmpty) <= 1;

				try
				{
					isRecursive = true;
					foreach (FilterField field in castedFields.Where(field => field != filterToValidate))
					{
						field.RunPreSaveValidation();
					}
				}
				finally
				{
					isRecursive = false;
				}
			}
			return isValid;
		}

		public override string GetErrorMessage(FilterField filterToValidate)
		{
			return Res.GetString("bb578770-1b91-4027-9d34-409d1f340c1a", "Only one of the {0} could have data.", FieldNamesJoined);
		}
	}
}
