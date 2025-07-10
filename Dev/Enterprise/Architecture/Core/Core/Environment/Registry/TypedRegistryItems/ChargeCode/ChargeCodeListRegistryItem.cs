using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class ChargeCodeListRegistryItem : ChargeCodeRegistryItemWrapper<string>
	{
		public ChargeCodeListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, string defaultChargeCode)
			: this(name, category, caption, hint, defaultChargeCode, RegistryFindBoxFilter.None)
		{
		}

		public ChargeCodeListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, string defaultChargeCode, RegistryFindBoxFilter filter)
			: this(new ChargeCodeListRegistryItemImpl(name, category, caption, hint, defaultChargeCode), filter)
		{
		}

		public ChargeCodeListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryOptions options, string defaultChargeCode, RegistryFindBoxFilter filter)
			: this(new ChargeCodeListRegistryItemImpl(name, category, caption, hint, RegistryStorageFlags.Company, options, defaultChargeCode), filter)
		{
		}

		public ChargeCodeListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storageFlags, string defaultChargeCode, RegistryFindBoxFilter filter)
			: this(new ChargeCodeListRegistryItemImpl(name, category, caption, hint, storageFlags, defaultChargeCode), filter)
		{
			if (storageFlags != RegistryStorageFlags.Company &&
				storageFlags != (RegistryStorageFlags.Company | RegistryStorageFlags.System))
			{
				throw new ArgumentException("Can only except company or company and system");
			}
		}

		public ChargeCodeListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storageFlags, RegistryOptions options, string defaultChargeCode, RegistryFindBoxFilter filter)
			: this(new ChargeCodeListRegistryItemImpl(name, category, caption, hint, storageFlags, options, defaultChargeCode), filter)
		{
			if (storageFlags != RegistryStorageFlags.Company &&
				storageFlags != (RegistryStorageFlags.Company | RegistryStorageFlags.System))
			{
				throw new ArgumentException("Can only except company or company and system");
			}
		}

		protected ChargeCodeListRegistryItem(ChargeCodeRegistryItemImpl inner, RegistryFindBoxFilter filter)
			: base(inner)
		{
			EditorInfo = new AccChargeCodeListRegistryEditorInfo(filter);
		}

		#region class ChargeCodeListRegistryItemImpl

		protected class ChargeCodeListRegistryItemImpl : ChargeCodeRegistryItemImpl
		{
			public ChargeCodeListRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, string defaultChargeCode)
				: base(name, category, caption, hint, new StringRegistryDataType(), defaultChargeCode)
			{
			}

			public ChargeCodeListRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storageFlags, string defaultChargeCode)
				: base(name, category, caption, hint, new StringRegistryDataType(), storageFlags, defaultChargeCode)
			{
			}

			public ChargeCodeListRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storageFlags, RegistryOptions options, string defaultChargeCode)
				: base(name, category, caption, hint, new StringRegistryDataType(), storageFlags, options, defaultChargeCode)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return GetChargeCodePK(DefaultChargeCode, companyPK).ToString();
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
