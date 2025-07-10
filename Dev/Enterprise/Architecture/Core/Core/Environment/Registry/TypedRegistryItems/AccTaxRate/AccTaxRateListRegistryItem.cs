using System;
using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class AccTaxRateListRegistryItem : AccTaxRateRegistryItemWrapper<string>
	{
		public AccTaxRateListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, string defaultTaxRate)
			: this(name, category, caption, hint, defaultTaxRate, RegistryFindBoxFilter.None)
		{
		}

		public AccTaxRateListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, string defaultTaxRate, RegistryFindBoxFilter filter)
			: this(new AccTaxRateListRegistryItemImpl(name, category, caption, hint, defaultTaxRate), filter)
		{
		}

		protected AccTaxRateListRegistryItem(AccTaxRateRegistryItemImpl inner, RegistryFindBoxFilter filter)
			: base(inner)
		{
			EditorInfo = new AccTaxRateListRegistryEditorInfo(filter);
		}

		#region class AccTaxRateListRegistryItemImpl

		protected class AccTaxRateListRegistryItemImpl : AccTaxRateRegistryItemImpl
		{
			public AccTaxRateListRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, string defaultTaxRate)
				: base(name, category, caption, hint, new StringRegistryDataType(), defaultTaxRate)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return GetTaxRatePK(DefaultTaxRate, companyPK).ToString();
			}
		}

		#endregion

		public Guid[] GetAsGuidArray()
		{
			return StringToGuidArray(Value);
		}

		protected Guid[] StringToGuidArray(string value)
		{
			string[] listOfGuidsAsString = value.Split(',');
			List<Guid> result = new List<Guid>();
			foreach (string guidAsString in listOfGuidsAsString)
			{
				try
				{
					Guid guidFromString = new Guid(guidAsString);
					if (guidFromString != Guid.Empty)
					{
						result.Add(guidFromString);
					}
				}
				catch (ArgumentNullException)
				{
				}
				catch (FormatException)
				{
				}
			}
			return result.ToArray();
		}
	}
}
