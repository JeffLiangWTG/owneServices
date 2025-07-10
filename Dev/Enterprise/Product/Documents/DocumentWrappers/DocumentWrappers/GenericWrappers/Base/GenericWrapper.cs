using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base
{
	[AllowNoStaticNew, AllowPublicConstructor]
	public abstract class GenericWrapper : DocBaseWrapper
	{
		protected GenericWrapper(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
			: base(businessObjectToWrap, factory)
		{
			WrappedBO = businessObjectToWrap;
		}
		protected readonly BusinessObject WrappedBO;

		public string GetFieldMap(bool includeChildrenAndRelatedObjects)
		{
			return Map.GenericWrapperMapper.GetMapAsText(this.GetType(), includeChildrenAndRelatedObjects, false);
		}

		protected ZString GetCombinedValue(ZString string1, ZString string2)
		{
			return GetCombinedValue(string1, string2, " - ");
		}

		protected ZString GetCombinedValue(ZString string1, ZString string2, string separator)
		{
			if (string1 == string2)
			{
				return string1;
			}
			return string1 + (string2.IsEmpty || string1.IsEmpty ? "" : separator) + string2;
		}

		protected ZString GetBestValueWithFallback(ZString bestValue, ZString fallbackValue)
		{
			return bestValue.IsEmpty ? fallbackValue : bestValue;
		}

		protected ZDateTime GetBestValueWithFallback(ZDateTime bestValue, ZDateTime fallbackValue)
		{
			return bestValue.IsEmpty ? fallbackValue : bestValue;
		}

		public RegistryWrapper Registry
		{
			get
			{
				if (fRegistry == null)
				{
					fRegistry = new RegistryWrapper(Factory);
				}
				return fRegistry;
			}
		}
		RegistryWrapper fRegistry;

		internal ZGuid WrappedObjectPK
		{
			get
			{
				if (WrappedBO != null)
				{
					return WrappedBO.PK;
				}
				return PK;
			}
		}

		protected string FormatNumeric(ZDecimal input, int decimalPlaces, bool trimTrailingZeros = false)
		{
			string result;
			var formatInfo = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Germany)
			{
				formatInfo = (NumberFormatInfo)formatInfo.Clone();
				formatInfo.CurrencySymbol = "";
				formatInfo.CurrencyDecimalDigits = decimalPlaces;
				result = input.ToString("C", formatInfo).TrimEnd();
			}
			else
			{
				result = input.ToString(decimalPlaces);
			}

			if (trimTrailingZeros && result.Contains(formatInfo.NumberDecimalSeparator))
			{
				result = result.TrimEnd('0');
				result = result.TrimEnd(DecimalSeparator.ToCharArray());
			}

			return result;
		}
	}
}
