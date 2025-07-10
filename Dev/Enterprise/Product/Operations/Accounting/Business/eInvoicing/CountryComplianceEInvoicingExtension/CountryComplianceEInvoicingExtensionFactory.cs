using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing
{
	/// <summary>
	///-------------------------------------------------------------------
	/// 🚩🚩🚩 IMPORTANT: THIS FACTORY MUST NOT BE USED FOR NEW FEATURES.
	///-------------------------------------------------------------------
	///
	/// It just keeps existing features that we did not move to a new Accounting.CountryCompliance solution.
	/// New features must be implemented in new Accounting.CountryCompliance solution.
	/// https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/11765/Accounting.CountryCompliance-solution
	/// </summary>
	public class CountryComplianceEInvoicingExtensionFactory : ICountryComplianceEInvoicingExtensionFactory
	{
		IEInvoicingTransactionValidation ICountryComplianceEInvoicingExtensionFactory.GetIEInvoicingTransactionValidation(ZString countryCode)
		{
			return GetCountryComplianceInfoExtension(countryCode) as IEInvoicingTransactionValidation;
		}

		IMostRecentPivotProvider ICountryComplianceEInvoicingExtensionFactory.GetIMostRecentPivotProvider(ZString countryCode)
		{
			return GetCountryComplianceInfoExtension(countryCode) as IMostRecentPivotProvider;
		}

		IEReportingStatusMessageProvider ICountryComplianceEInvoicingExtensionFactory.GetIEReportingStatusMessageProvider(ZString countryCode)
		{
			return GetCountryComplianceInfoExtension(countryCode) as IEReportingStatusMessageProvider;
		}

		public IExistPivotCheckProvider GetExistPivotCheckProvider(ZString countryCode)
		{
			return GetCountryComplianceInfoExtension(countryCode) as IExistPivotCheckProvider;
		}

		public static CountryComplianceInfo GetCountryComplianceInfoExtension(ZString countryCode)
		{
			if (CountryComplianceInfoEInvoicingExtensions == null)
			{
				//Keep the following list ordered alphabetically
				CountryComplianceInfoEInvoicingExtensions = new Dictionary<string, Func<CountryComplianceInfo>>
				{
					{ CountryCodes.China,                 () => new ChinaComplianceInfoEInvoicingExtension() },
					{ CountryCodes.KoreaSouth,            () => new KoreaSouthComplianceInfoEInvoicingExtension() },
					{ CountryCodes.Malaysia,              () => new MalaysiaComplianceInfoEInvoicingExtension() },
					{ CountryCodes.Mexico,                () => new MexicoComplianceInfoEInvoicingExtension() },
					{ CountryCodes.Turkey,                () => new TurkeyComplianceInfoEInvoicingExtension() },
					{ CountryCodes.VietNam,               () => new VietnamComplianceInfoEInvoicingExtension() },
				};
			}

			return CountryComplianceInfoEInvoicingExtensions.TryGetValue(countryCode, out var func) ? func() : null;
		}

		[ThreadStatic]
		static Dictionary<string, Func<CountryComplianceInfo>> CountryComplianceInfoEInvoicingExtensions;
	}
}
