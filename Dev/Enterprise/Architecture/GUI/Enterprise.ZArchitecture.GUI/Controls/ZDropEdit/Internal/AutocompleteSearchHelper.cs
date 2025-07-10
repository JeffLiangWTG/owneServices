using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public enum AutoCompleteStringComparisonType
	{
		Exact,
		StartsWith,
		Contains
	}

	public static class AutocompleteSearchHelper
	{
		public static int FindIndex(IDropFormParent parent, IList list, string text, AutoCompleteStringComparisonType type, ICodeDescription cachedItem = null)
		{
			if (list != null)
			{
				if (cachedItem != null)
				{
					var index = list.IndexOf(cachedItem);
					if (index >= 0 && Compare(parent, cachedItem, text, type))
					{
						return index;
					}
				}

				for (var i = 0; i < list.Count; i++)
				{
					var businessObject = list[i] as BusinessObject;
					if (businessObject != null && businessObject.IsDeleted)
					{
						continue;
					}
					var item = (ICodeDescription)list[i];
					if (Compare(parent, item, text, type))
					{
						return i;
					}
				}
			}

			return -1;
		}

		public static ICodeDescription FindItem(IDropFormParent parent, IList list, string text, AutoCompleteStringComparisonType type, ICodeDescription cachedItem = null)
		{
			var index = FindIndex(parent, list, text, type, cachedItem);

			return index >= 0 ? (ICodeDescription)list[index] : null;
		}

		static bool Compare(IDropFormParent parent, ICodeDescription item, string text, AutoCompleteStringComparisonType type)
		{
			if (parent.IsItemValidForAutoComplete(item))
			{
				var itemText = parent.GetMultilingualValue(item).ToString();

				if (Compare(itemText, text, type))
				{
					return true;
				}
			}

			return false;
		}

		static bool Compare(string lhs, string rhs, AutoCompleteStringComparisonType type)
		{
			switch (type)
			{
				case AutoCompleteStringComparisonType.Exact:
					return lhs.Equals(rhs, StringComparison.OrdinalIgnoreCase);
				case AutoCompleteStringComparisonType.StartsWith:
					return lhs.StartsWith(rhs, StringComparison.OrdinalIgnoreCase);
				case AutoCompleteStringComparisonType.Contains:
					return lhs.Contains(rhs, StringComparison.OrdinalIgnoreCase);
				default:
					ErrorReporter.ReportOnce("UnknownComparisonType - " + type);
					return false;
			}
		}
	}
}
