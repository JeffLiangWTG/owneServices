using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.GUI
{
	public static class AccountingCaptionHelper
	{
		public static ResourceStringData OSExtraTaxAmountCaption
		{
			get
			{
				ResourceStringData resString = ResourceStringData.Empty;
				if (GlbCompany.CurrentCompany != null && GlbCompany.CurrentCompany.IsExtraTaxApplicable())
				{
					var countryComplainceInfo = CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as ICountryComplianceInfo;
					resString = countryComplainceInfo.GetExtraTaxOSAmountCaption();
				}
				return resString;
			}
		}

		public static ResourceStringData LocalExtraTaxAmountCaption
		{
			get
			{
				ResourceStringData resString = ResourceStringData.Empty;
				if (GlbCompany.CurrentCompany != null && GlbCompany.CurrentCompany.IsExtraTaxApplicable())
				{
					var countryComplainceInfo = CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as ICountryComplianceInfo;
					resString = countryComplainceInfo.GetExtraTaxLocalAmountCaption();
				}
				return resString;
			}
		}

		public static ResourceStringData OSTaxAmountCaption
		{
			get
			{
				ResourceStringData resString = ResourceStringData.Empty;
				if (GlbCompany.CurrentCompany != null && GlbCompany.CurrentCompany.IsExtraTaxApplicable())
				{
					var countryComplainceInfo = CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as ICountryComplianceInfo;
					resString = countryComplainceInfo.GetTaxOSAmountCaption();
				}
				return resString;
			}
		}

		public static ResourceStringData LocalTaxAmountCaption
		{
			get
			{
				ResourceStringData resString = ResourceStringData.Empty;
				if (GlbCompany.CurrentCompany != null && GlbCompany.CurrentCompany.IsExtraTaxApplicable())
				{
					var countryComplainceInfo = CountryComplianceFactory.GetCountryComplianceInfo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as ICountryComplianceInfo;
					resString = countryComplainceInfo.GetTaxLocalAmountCaption();
				}
				return resString;
			}
		}
	}
}
