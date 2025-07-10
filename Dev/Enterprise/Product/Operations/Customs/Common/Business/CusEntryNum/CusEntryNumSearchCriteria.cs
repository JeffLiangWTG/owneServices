using System;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	public class CusEntryNumSearchCriteria
	{
		public CusEntryNumSearchCriteria(ZString criteria)
		{
			Parse(criteria);
		}

		public string EntryType { get; private set; }
		public string Country { get; private set; }
		public string Category { get; private set; }
		public bool IsValid { get; private set; }

		void Parse(ZString index)
		{
			ZString[] results = index.Split(':');

			switch (results.Length)
			{
				case 1:
					EntryType = results[0];
					IsValid = true;
					break;

				case 2:
					Country = results[0];
					EntryType = results[1];
					IsValid = true;
					break;

				case 3:
					Category = results[0];
					Country = results[1];
					EntryType = results[2];
					IsValid = true;
					break;
			}
		}

		public ISearcheableCusEntryNumber Find(IEnumerable<ISearcheableCusEntryNumber> collection)
		{
			ISearcheableCusEntryNumber result = null;

			if (IsValid && collection != null)
			{
				foreach (ISearcheableCusEntryNumber cusEntryNumber in collection)
				{
					if (cusEntryNumber.CE_EntryType == EntryType &&
						(String.IsNullOrEmpty(Country) || cusEntryNumber.CE_RN_NKCountryCode == Country) &&
						(String.IsNullOrEmpty(Category) || cusEntryNumber.CE_Category == Category))
					{
						result = cusEntryNumber;
						break;
					}
				}
			}

			return result;
		}
	}
}
