using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public static class FilterValidationHelper
	{
		public static bool FilterContainsOnlyAllowed(FilterField filterToValidate, params BusinessObject[] allowedBizos)
		{
			var allowedBizosNotNull = allowedBizos.Where(allowed => allowed != null).ToArray();
			bool result = !filterToValidate.IsEmpty;
			if (result && allowedBizosNotNull.Length > 0)
			{
				LookupField lookupField = filterToValidate as LookupField;
				if (lookupField != null)
				{
					result = false;
					foreach (BusinessObject allowed in allowedBizosNotNull)
					{
						if (lookupField.Value == allowed.PK)
						{
							result = true;
							break;
						}
					}
				}
				else
				{
					MultipleSelectionLookup multipleSelectionLookup = filterToValidate as MultipleSelectionLookup;
					if (multipleSelectionLookup != null)
					{
						foreach (BusinessObject bizo in multipleSelectionLookup.BindToList.ToArray())
						{
							bool isAllowed = false;
							foreach (BusinessObject allowed in allowedBizosNotNull)
							{
								if (bizo.PK == allowed.PK)
								{
									isAllowed = true;
									break;
								}
							}
							if (!isAllowed)
							{
								result = false;
								break;
							}
						}
					}
				}
			}

			return result;
		}
	}
}
