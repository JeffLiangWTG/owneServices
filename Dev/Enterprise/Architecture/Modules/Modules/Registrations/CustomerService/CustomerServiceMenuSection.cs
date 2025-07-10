using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Modules
{
	public class CustomerServiceMenuSection
	{
		CustomerServiceMenuSection(string code, MultilingualString description)
		{
			this.code = code;
			this.description = description;
		}

		#region Properties

		public string Code
		{
			get { return code; }
		}
		readonly string code;

		public MultilingualString Description
		{
			get { return description; }
		}
		readonly MultilingualString description;

		#endregion

		#region Lookup class

		public class Lookup : Dictionary<string, CustomerServiceMenuSection>
		{
			internal Lookup()
				: base(StringComparer.OrdinalIgnoreCase)
			{
			}

			public class Codes
			{
			}

			public class Descriptions
			{
			}

			protected void AddPair(string code, MultilingualString description)
			{
				Add(code, new CustomerServiceMenuSection(code, description));
			}

			public string GetDescriptionFromCode(string code)
			{
				CustomerServiceMenuSection item;
				if (TryGetValue(code, out item))
				{
					return item.Description;
				}

				return string.Empty;
			}
		}

		#endregion
	}
}
