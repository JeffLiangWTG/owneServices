using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	public class AdditionalDataForBorderWise
	{
		public AdditionalDataForBorderWise(string parameterForBorderWise, ZDateTime dateForDutyRate)
		: this(parameterForBorderWise, dateForDutyRate, x => x)
		{
		}

		public AdditionalDataForBorderWise(string parameterForBorderWise, ZDateTime dateForDutyRate, string countryCodeOverride)
			: this(parameterForBorderWise, dateForDutyRate, x => x, countryCodeOverride)
		{
		}

		public AdditionalDataForBorderWise(string parameterForBorderWise, ZDateTime dateForDutyRate, Func<ZString, ZString> formatBorderWiseInputFunc, string countryCodeOverride = "")
		{
			this.ParameterForBorderWise = parameterForBorderWise;
			this.CountryCodeOverride = countryCodeOverride;
			this.DateForDutyRate = dateForDutyRate;
			this.formatBorderWiseInputFunc = formatBorderWiseInputFunc;
		}

		public readonly string ParameterForBorderWise;
		public string CountryCodeOverride;
		public readonly ZDateTime DateForDutyRate;
		readonly Func<ZString, ZString> formatBorderWiseInputFunc;

		public ZString FormatBorderWiseInput(ZString input)
		{
			return formatBorderWiseInputFunc == null ? input : formatBorderWiseInputFunc(input);
		}

		#region GetAdditionalDataFrom
		public static AdditionalDataForBorderWise GetAdditionalDataFrom(BusinessObject currentBusinessObject, string bindingProperty)
		{
			AdditionalDataForBorderWise additionalData = new AdditionalDataForBorderWise("E", ZDateTime.Today);
			if (DataSourceHasIHaveAdditionalDataForBorderWiseImplemented(currentBusinessObject))
			{
				additionalData = ((IHaveAdditionalDataForBorderWise)currentBusinessObject).GetAdditionalDataForBorderWise(bindingProperty);
			}
			return additionalData;
		}

		public static AdditionalDataForBorderWise GetAdditionalDataFrom(BusinessObject currentBusinessObject)
		{
			return GetAdditionalDataFrom(currentBusinessObject, "");
		}

		public static AdditionalDataForBorderWise GetAdditionalDataFrom(string tariffType, ZDateTime dateForDutyRate, string countryCodeOverride)
		{
			AdditionalDataForBorderWise additionalData = new AdditionalDataForBorderWise(string.IsNullOrWhiteSpace(tariffType) ? "E" : tariffType, dateForDutyRate, countryCodeOverride);
			return additionalData;
		}

		#endregion

		#region DataSourceHasIHaveAdditionalDataForBorderWiseImplemented
		public static bool DataSourceHasIHaveAdditionalDataForBorderWiseImplemented(BusinessObject dataSource)
		{
			bool result = false;
			if (dataSource == null)
			{
				ErrorReporter.ReportOnce(DeveloperErrorIfCurrentBusinessObjectIsNull, DeveloperErrorIfCurrentBusinessObjectIsNull);
			}
			else if (!(dataSource is IHaveAdditionalDataForBorderWise))
			{
				ErrorReporter.ReportOnce(dataSource.GetType().ToString() + " does not implement IHaveAdditionalDataForBorderWise.", dataSource.GetType().ToString() + DeveloperErrorIfCurrentBusinessObjectDoesNotImplementIHaveAdditionalDataForBorderWise);
			}
			else
			{
				result = true;
			}
			return result;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "cargowise")]
		public const string DeveloperErrorIfCurrentBusinessObjectDoesNotImplementIHaveAdditionalDataForBorderWise = " does not implement IHaveAdditionalDataForBorderWise. This Control cannot bind to a BusinessObject that does not implement this interface.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "cargowise")]
		public const string DeveloperErrorIfCurrentBusinessObjectIsNull = "CurrentBusinessObject passed to this method cannot be null.";
		#endregion
	}
}
