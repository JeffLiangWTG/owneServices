using System;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class FilterCollectionValidator
	{
		public FilterCollectionValidator()
		{
			Filters = new FilterFieldList();
		}

		public void AddFilterField(FilterField filterToAdd)
		{
			Filters.Add(filterToAdd);
		}

		public virtual bool IsValid(FilterField filterToValidate)
		{
			return true;
		}

		public virtual string GetErrorMessage(FilterField filterToValidate)
		{
			return "";
		}

		protected internal FilterFieldList Filters;

		protected internal string FieldNamesJoined
		{
			get
			{
				string[] filterNames = new string[Filters.Count - 1];
				for (int i = 0; i < Filters.Count - 1; i++)
				{
					filterNames[i] = "'" + Filters[i].DisplayNameLocalized + "'";
				}
				return Filters.Count == 1 ? "'" + Filters[0].DisplayNameLocalized + "'" : String.Join(", ", filterNames) + (Filters.Count > 1 ? " " + Res.GetString("dcc0de8e-3d2e-4cf5-8097-3ee99fe60c32", "and") + " '" + Filters[Filters.Count - 1].DisplayNameLocalized + "'" : "");
			}
		}
	}
}
