using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	sealed class TaxInfo : DocDataObject
	{
		#region Code

		public ZString Code
		{
			get => code;
			set
			{
				if (SetNonPersistentPropertyValue(CodeInfo, ref code, value))
				{
				}
			}
		}

		ZString code;

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		#endregion

		#region CountryCode

		public ZString Country
		{
			get => country;
			set
			{
				if (SetNonPersistentPropertyValue(CountryInfo, ref country, value))
				{
				}
			}
		}

		ZString country;

		public ZPropertyInfo CountryInfo => GetZPropertyInfo(nameof(Country));

		#endregion

		#region RegulatingCountry

		public ZString RegulatingCountry
		{
			get => regulatingCountry;
			set
			{
				if (SetNonPersistentPropertyValue(RegulatingCountryInfo, ref regulatingCountry, value))
				{
				}
			}
		}

		ZString regulatingCountry;

		public ZPropertyInfo RegulatingCountryInfo => GetZPropertyInfo(nameof(RegulatingCountry));

		#endregion

		#region Description

		public ZString Description
		{
			get => description;
			set
			{
				if (SetNonPersistentPropertyValue(DescriptionInfo, ref description, value))
				{
				}
			}
		}

		ZString description;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		#endregion

		#region LongLabel

		public ZString LongLabel
		{
			get => longLabel;
			set
			{
				if (SetNonPersistentPropertyValue(LongLabelInfo, ref longLabel, value))
				{
					Validate(LongLabelInfo);
				}
			}
		}

		ZString longLabel;

		public ZPropertyInfo LongLabelInfo => GetZPropertyInfo(nameof(LongLabel));

		#endregion

		#region ShortLabel

		public ZString ShortLabel
		{
			get => shortLabel;
			set
			{
				if (SetNonPersistentPropertyValue(ShortLabelInfo, ref shortLabel, value))
				{
					if (ShortLabel == "8888" || ShortLabel == "9999")
					{
						LongLabel = ShortLabel;
					}
					Validate(ShortLabelInfo);
				}
			}
		}

		ZString shortLabel;

		public ZPropertyInfo ShortLabelInfo => GetZPropertyInfo(nameof(ShortLabel));

		#endregion

		#region Number

		public ZString Number
		{
			get => number;
			set
			{
				if (SetNonPersistentPropertyValue(NumberInfo, ref number, value))
				{
					Validate(NumberInfo);
				}
			}
		}

		ZString number;

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));

		#endregion

		#region IsChinaSpecific

		public ZBool IsChinaSpecific
		{
			get => isChinaSpecific;
			set
			{
				if (SetNonPersistentPropertyValue(IsChinaSpecificInfo, ref isChinaSpecific, value))
				{
				}
			}
		}

		ZBool isChinaSpecific = false;

		public ZPropertyInfo IsChinaSpecificInfo => GetZPropertyInfo(nameof(IsChinaSpecific));

		#endregion

		#region IsPlaceHolder

		public ZBool IsPlaceHolder
		{
			get => isPlaceHolder;
			set
			{
				if (SetNonPersistentPropertyValue(IsPlaceHolderInfo, ref isPlaceHolder, value))
				{
					Validate(IsPlaceHolderInfo);
				}
			}
		}

		ZBool isPlaceHolder = false;

		public ZPropertyInfo IsPlaceHolderInfo => GetZPropertyInfo(nameof(IsPlaceHolder));

		#endregion

		#region DisplayedLabel

		public ZString DisplayedLabel
		{
			get => displayedLabel;
			set
			{
				if (SetNonPersistentPropertyValue(DisplayedLabelInfo, ref displayedLabel, value))
				{
				}
			}
		}

		ZString displayedLabel;

		public ZPropertyInfo DisplayedLabelInfo => GetZPropertyInfo(nameof(DisplayedLabel));

		#endregion

		#region Clear

		public void Clear()
		{
			Code = string.Empty;
			Country = string.Empty;
			RegulatingCountry = string.Empty;
			Description = string.Empty;
			LongLabel = string.Empty;
			ShortLabel = string.Empty;
			Number = string.Empty;
			DisplayedLabel = string.Empty;
		}

		#endregion
	}
}
