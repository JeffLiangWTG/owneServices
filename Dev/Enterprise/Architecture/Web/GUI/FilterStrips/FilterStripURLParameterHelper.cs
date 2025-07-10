using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Web.Business.FilterStrips
{
	public class FilterStripURLParameterHelper
	{
		#region Setup Filter Strips

		public void SetupFilterStripsFromQueryString(FilterStripBusinessObject filterBizO, ZString queryString)
		{
			FilterNameAndValue[] namesAndValues = GetFilterNamesAndValues(queryString);

			foreach (FilterNameAndValue nameAndValue in namesAndValues)
			{
				FindFilterAndSetValues(filterBizO, nameAndValue);
			}
		}

		FilterNameAndValue[] GetFilterNamesAndValues(ZString unparsedQueryString)
		{
			QueryString queryString = new QueryString(unparsedQueryString);
			List<FilterNameAndValue> namesAndValues = new List<FilterNameAndValue>();

			foreach (string key in queryString)
			{
				ZString[] values = ((ZString)queryString[key]).Split(',');

				if (values != null)
				{
					namesAndValues.Add(new FilterNameAndValue(key, values));
				}
			}

			return namesAndValues.ToArray();
		}

		void FindFilterAndSetValues(FilterStripBusinessObject filterBizO, FilterNameAndValue nameAndValues)
		{
			ModuleFilter filter = filterBizO[nameAndValues.FilterName];

			if (filter != null)
			{
				SetFilterValue(filter, nameAndValues);
			}
		}

		#endregion

		#region Set Filter Values

		void SetFilterValue(ModuleFilter filter, FilterNameAndValue nameAndValues)
		{
			if (filter is ModuleDateFilter)
			{
				SetFilterValue_Date(filter as ModuleDateFilter, nameAndValues);
			}
			else if (filter is ModuleGuidFilter)
			{
				SetFilterValue_Guid(filter as ModuleGuidFilter, nameAndValues);
			}
			else if (filter is ModuleGuidsFilter)
			{
				SetFilterValue_Guids(filter as ModuleGuidsFilter, nameAndValues);
			}
			else if (filter is ModuleLocationFilter)
			{
				SetFilterValue_Location(filter as ModuleLocationFilter, nameAndValues);
			}
			else if (filter is ModuleNumberFilter || filter is ModuleFountainFilter || filter is ModuleNkFilter)
			{
				SetFilterValue_Number(filter as ModuleTextBaseFilter, nameAndValues);
			}
			else if (filter is ModuleNumberRangeFilter)
			{
				SetFilterValue_NumberRange(filter as ModuleNumberRangeFilter, nameAndValues);
			}
			else if (filter is ModuleSingleDateFilter)
			{
				SetFilterValue_SingleDate(filter as ModuleSingleDateFilter, nameAndValues);
			}
			else if (filter is ModuleTextFilter)
			{
				SetFilterValue_Text(filter as ModuleTextFilter, nameAndValues);
			}
			else if (filter is ModuleTextAndNkFilter)
			{
				SetFilterValue_TextAndNk(filter as ModuleTextAndNkFilter, nameAndValues);
			}
			else if (filter is ModuleTextRangeFilter)
			{
				SetFilterValue_TextRange(filter as ModuleTextRangeFilter, nameAndValues);
			}
		}

		void SetFilterValue_Date(ModuleDateFilter filter, FilterNameAndValue nameAndValues)
		{
			int numberOfValues = nameAndValues.Values.Count;

			if (numberOfValues == 1 && filter.PropertySearch_List.GetDescriptionFromCode(nameAndValues.Values[0]) != null)
			{
				ZString presetDateDescription = filter.PropertySearch_List.GetDescriptionFromCode(nameAndValues.Values[0]);
				ZString properlyCapitalisedCode = filter.PropertySearch_List.GetCodeFromDescription(presetDateDescription);

				filter.IsActive = true;
				filter.PropertySearch = properlyCapitalisedCode;
			}
			else
			{
				ZDateTime dateTime1 = (numberOfValues > 0) ? GetZDateTime(nameAndValues.Values[0]) : ZDateTime.Empty;
				ZDateTime dateTime2 = (numberOfValues > 1) ? GetZDateTime(nameAndValues.Values[1]) : ZDateTime.Empty;

				if (dateTime1.IsValid || dateTime2.IsValid)
				{
					filter.IsActive = true;
					filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

					if (dateTime1.IsValid)
					{
						filter.Property1 = dateTime1;
					}

					if (dateTime2.IsValid)
					{
						filter.Property2 = dateTime2;
					}
				}
			}
		}

		void SetFilterValue_Guid(ModuleGuidFilter filter, FilterNameAndValue nameAndValues)
		{
			int numberOfValues = nameAndValues.Values.Count;

			ZGuid guid = (numberOfValues > 0) ? GetZGuid(nameAndValues.Values[0], filter.List) : ZGuid.Empty;

			if (guid.IsValid)
			{
				filter.IsActive = true;
				filter.Property = guid;
			}
		}

		void SetFilterValue_Guids(ModuleGuidsFilter filter, FilterNameAndValue nameAndValues)
		{
			int numberOfValues = nameAndValues.Values.Count;

			ZGuid guid1 = (numberOfValues > 0) ? GetZGuid(nameAndValues.Values[0], filter.List1) : ZGuid.Empty;
			ZGuid guid2 = (numberOfValues > 1) ? GetZGuid(nameAndValues.Values[1], filter.List2) : ZGuid.Empty;

			if (!guid1.IsEmpty || !guid2.IsEmpty)
			{
				filter.IsActive = true;

				if (guid1.IsValid)
				{
					filter.Property1 = guid1;
				}
				if (guid2.IsValid)
				{
					filter.Property2 = guid2;
				}
			}
		}

		void SetFilterValue_Location(ModuleLocationFilter filter, FilterNameAndValue nameAndValues)
		{
			int numberOfValues = nameAndValues.Values.Count;

			ZString location1 = (numberOfValues > 0) ? GetZString(nameAndValues.Values[0]) : ZString.Empty;
			ZString location2 = (numberOfValues > 1) ? GetZString(nameAndValues.Values[1]) : ZString.Empty;

			if (!location1.IsEmpty || !location2.IsEmpty)
			{
				filter.IsActive = true;

				if (!location1.IsEmpty)
				{
					filter.Property1 = location1;
				}
				if (!location2.IsEmpty)
				{
					filter.Property2 = location2;
				}
			}
		}

		void SetFilterValue_Number(ModuleTextBaseFilter filter, FilterNameAndValue nameAndValues)
		{
			int numberOfValues = nameAndValues.Values.Count;

			ZString number1 = (numberOfValues > 0) ? GetZString(nameAndValues.Values[0]) : ZString.Empty;

			if (!number1.IsEmpty)
			{
				filter.IsActive = true;
				filter.Property = number1;
			}
		}

		void SetFilterValue_NumberRange(ModuleNumberRangeFilter filter, FilterNameAndValue nameAndValues)
		{
			int numberOfValues = nameAndValues.Values.Count;

			ZDecimal number1 = (numberOfValues > 0) ? GetZDecimal(nameAndValues.Values[0]) : ZDecimal.Zero;
			ZDecimal number2 = (numberOfValues > 1) ? GetZDecimal(nameAndValues.Values[1]) : ZDecimal.Zero;

			if (!number1.IsEmpty || !number2.IsEmpty)
			{
				filter.IsActive = true;

				if (!number1.IsEmpty)
				{
					filter.Property1 = number1;
				}
				if (!number2.IsEmpty)
				{
					if (number1.IsEmpty) // Number range filter requires property1 to be set, if there is only one value
					{
						filter.Property1 = number2;
					}
					else
					{
						filter.Property2 = number2;
					}
				}
			}
		}

		void SetFilterValue_SingleDate(ModuleSingleDateFilter filter, FilterNameAndValue nameAndValues)
		{
			int numberOfValues = nameAndValues.Values.Count;

			ZDateTime dateTime1 = (numberOfValues > 0) ? GetZDateTime(nameAndValues.Values[0]) : ZDateTime.Empty;

			if (!dateTime1.IsEmpty && dateTime1.IsValid)
			{
				filter.IsActive = true;
				filter.Property1 = dateTime1;
			}
		}

		void SetFilterValue_Text(ModuleTextFilter filter, FilterNameAndValue nameAndValues)
		{
			int numberOfValues = nameAndValues.Values.Count;

			ZString string1 = (numberOfValues > 0) ? GetZString(nameAndValues.Values[0]) : ZString.Empty;

			if (!string1.IsEmpty)
			{
				filter.IsActive = true;
				filter.Property = string1;
			}
		}

		void SetFilterValue_TextAndNk(ModuleTextAndNkFilter filter, FilterNameAndValue nameAndValues)
		{
			int numberOfValues = nameAndValues.Values.Count;

			ZString text = (numberOfValues > 0) ? GetZString(nameAndValues.Values[0]) : ZString.Empty;
			ZString nk = (numberOfValues > 1) ? GetZString(nameAndValues.Values[1]) : ZString.Empty;

			if (!text.IsEmpty || !nk.IsEmpty)
			{
				filter.IsActive = true;

				if (!text.IsEmpty)
				{
					filter.Property = text;
				}
				if (!nk.IsEmpty)
				{
					filter.NkProperty = nk;
				}
			}
		}

		void SetFilterValue_TextRange(ModuleTextRangeFilter filter, FilterNameAndValue nameAndValues)
		{
			int numberOfValues = nameAndValues.Values.Count;

			ZString string1 = (numberOfValues > 0) ? GetZString(nameAndValues.Values[0]) : ZString.Empty;
			ZString string2 = (numberOfValues > 1) ? GetZString(nameAndValues.Values[1]) : ZString.Empty;

			if (!string1.IsEmpty || !string2.IsEmpty)
			{
				filter.IsActive = true;

				if (!string1.IsEmpty)
				{
					filter.Property1 = string1;
				}
				if (!string2.IsEmpty)
				{
					filter.Property2 = string2;
				}
			}
		}

		#endregion

		#region GetZTypes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "This is used to parse URL parameters, and is essentially providing an interface for users to enter data.")]
		ZDateTime GetZDateTime(string value1)
		{
			ZDateTime dateTime = ZDateTime.Empty;

			if (!ZDateTime.TryParseExact(value1, out dateTime, "dd-MMM-yy")) // This is used to parse URL parameters, and is essentially providing an interface for users to enter data.
			{
				ZDateTime.TryParseExact(value1, out dateTime, "d-MMM-yy"); // This is used to parse URL parameters, and is essentially providing an interface for users to enter data.
			}

			return dateTime;
		}

		ZDecimal GetZDecimal(string stringValue)
		{
			decimal value = 0m;
			decimal.TryParse(stringValue, out value);
			return value;
		}

		ZGuid GetZGuid(string stringValue, IList list)
		{
			ZGuid value = ZGuid.Empty;

			if (!ZGuid.TryParse(stringValue, out value))
			{
				IFindBoxListProvider listProvider = list as IFindBoxListProvider;
				if (listProvider != null)
				{
					value = listProvider.PrimaryKeyFromCode(stringValue);
				}
			}

			return value;
		}

		ZString GetZString(string stringValue)
		{
			return new ZString(stringValue);
		}

		#endregion
	}

	#region FilterNameAndValue

	public class FilterNameAndValue
	{
		public FilterNameAndValue(ZString filterName, ZString[] values)
		{
			this.fFilterName = filterName;
			this.Values.AddRange(values);
		}

		public ZString FilterName
		{
			get { return fFilterName; }
		}
		readonly ZString fFilterName;

		public List<ZString> Values
		{
			get
			{
				if (fValues == null)
				{
					fValues = new List<ZString>();
				}

				return fValues;
			}
		}

		List<ZString> fValues;
	}

	#endregion
}
