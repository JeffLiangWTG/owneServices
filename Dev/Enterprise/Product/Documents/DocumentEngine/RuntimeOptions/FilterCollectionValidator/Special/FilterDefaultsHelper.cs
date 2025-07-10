using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public static class FilterDefaultsHelper
	{
		public static void AddDefaultValue(FilterField filterField, BusinessObject defaultBizo)
		{
			if (defaultBizo != null)
			{
				LookupField lookupField = filterField as LookupField;
				if (lookupField != null)
				{
					lookupField.Value = defaultBizo.PK.ToGuid();
				}
				else
				{
					MultipleSelectionLookup multipleSelectionLookup = filterField as MultipleSelectionLookup;
					if (multipleSelectionLookup != null && !multipleSelectionLookup.IsHidden)
					{
						multipleSelectionLookup.BindToList.Add(defaultBizo);
					}
				}
			}
		}
	}
}
