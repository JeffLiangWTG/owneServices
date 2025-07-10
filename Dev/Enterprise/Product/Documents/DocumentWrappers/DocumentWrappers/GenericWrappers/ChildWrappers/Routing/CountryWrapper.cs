using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("CodeAndName")]
	public class CountryWrapper : GenericWrapper
	{
		public CountryWrapper(ZString countryCode, BusinessObjectFactory factory)
			: base(null, factory)
		{
			fCountryCode = countryCode;
		}

		public CountryWrapper(RefCountry countryBO, BusinessObjectFactory factory)
			: base(countryBO, factory)
		{
			fCountryBO = countryBO;
			fCountryCode = countryBO != null ? countryBO.Code : ZString.Empty;
		}

		public ZString Code
		{
			get { return fCountryCode; }
		}

		public ZString Name
		{
			get { return CountryBO == null ? fCountryCode : CountryBO.RN_DescMultilingual; }
		}

		public ZString CodeAndName
		{
			get { return GetCombinedValue(Code, Name); }
		}

		public ZString EconomicGrouping
		{
			get { return CountryBO == null ? ZString.Empty : CountryBO.RN_EconomicGrouping; }
		}

		public ZString ISONumericCode
		{
			get { return CountryBO == null ? ZString.Empty : CountryBO.RN_IsoNumericUNM49Code; }
		}

		#region Working Days
		ZBool IsNonWorkingDay(string weekDay)
		{
			switch (weekDay)
			{
				case (AutoDayOfWeekCodeList.Codes.Monday):
					return CountryBO.IsMondayNonWorkingDay;
				case (AutoDayOfWeekCodeList.Codes.Tuesday):
					return CountryBO.IsTuesdayNonWorkingDay;
				case (AutoDayOfWeekCodeList.Codes.Wednesday):
					return CountryBO.IsWednesdayNonWorkingDay;
				case (AutoDayOfWeekCodeList.Codes.Thursday):
					return CountryBO.IsThursdayNonWorkingDay;
				case (AutoDayOfWeekCodeList.Codes.Friday):
					return CountryBO.IsFridayNonWorkingDay;
				case (AutoDayOfWeekCodeList.Codes.Saturday):
					return CountryBO.IsSaturdayNonWorkingDay;
				case (AutoDayOfWeekCodeList.Codes.Sunday):
					return CountryBO.IsSundayNonWorkingDay;
				default:
					return false;
			}
		}

		public ZString[] WorkingDays
		{
			get
			{
				if (CountryBO == null || WeekDays.Length == 0)
				{
					return Array.Empty<ZString>();
				}

				if (fGetWorkingDays == null)
				{
					fGetWorkingDays = WeekDays.Where(x => !IsNonWorkingDay(x)).ToArray();
				}
				return fGetWorkingDays;
			}
		}
		ZString[] fGetWorkingDays;

		public ZString[] NonWorkingDays
		{
			get
			{
				if (CountryBO == null || WeekDays.Length == 0)
				{
					return Array.Empty<ZString>();
				}

				if (fGetNonWorkingDays == null)
				{
					fGetNonWorkingDays = WeekDays.Where(x => IsNonWorkingDay(x)).ToArray();
				}
				return fGetNonWorkingDays;
			}
		}
		ZString[] fGetNonWorkingDays;

		public ZString[] WeekDays
		{
			get
			{
				return CountryBO == null ? Array.Empty<ZString>() : CountryBO.WeekDays.GetAllCodesZString();
			}
		}
		#endregion

		#region Implementation
		readonly ZString fCountryCode;

		RefCountry CountryBO
		{
			get
			{
				if (fCountryBO == null && !fCountryCode.IsEmpty)
				{
					fCountryBO = RefCountry.LoadFromCountryCode(Factory, fCountryCode);
				}
				return fCountryBO;
			}
		}
		RefCountry fCountryBO;
		#endregion
	}
}
