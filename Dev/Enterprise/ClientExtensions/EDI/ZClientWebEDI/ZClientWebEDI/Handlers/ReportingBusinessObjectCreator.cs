using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class ReportingBusinessObjectCreator
	{
		public static OdplReportingBusinessObject CreateOdplReportingBusinessObject(SecureQueryString secureQueryString, BusinessObjectFactory factory)
		{
			OdplReportingBusinessObject result = null;
			if (DateTime.TryParseExact(secureQueryString[OdplUsageReportRequestHelper.Constants.PeriodStart], ZDateTime.ISO8601ShortDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime periodStart))
			{
				if (!ZGuid.TryParse(secureQueryString[OdplUsageReportRequestHelper.Constants.OrganisationPk], out ZGuid orgPk))
				{
					orgPk = ZGuid.Empty;
				}

				if (!ZGuid.TryParse(secureQueryString[OdplUsageReportRequestHelper.Constants.ClientCompanyPk], out ZGuid clientCompanyPk))
				{
					clientCompanyPk = ZGuid.Empty;
				}

				if (!ZGuid.TryParse(secureQueryString[OdplUsageReportRequestHelper.Constants.LicenceCompanyPk], out ZGuid licenceCompanyPk))
				{
					licenceCompanyPk = ZGuid.Empty;
				}

				if (!ZGuid.TryParse(secureQueryString[OdplUsageReportRequestHelper.Constants.DatabasePk], out ZGuid databasePk))
				{
					databasePk = ZGuid.Empty;
				}

				var systemCode = secureQueryString[OdplUsageReportRequestHelper.Constants.SystemCode];

				if (new ZDateTime(periodStart).IsValid)
				{
					result = new OdplReportingBusinessObject(factory, periodStart, systemCode, orgPk, clientCompanyPk, licenceCompanyPk, databasePk);
				}
			}

			return result;
		}

		public static StlReportingBusinessObject CreateStlReportingBusinessObject(SecureQueryString secureQueryString, BusinessObjectFactory factory)
		{
			StlReportingBusinessObject result = null;
			if (DateTime.TryParseExact(secureQueryString[StlUsageReportRequestHelper.Constants.PeriodStart], ZDateTime.ISO8601ShortDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime periodStart))
			{
				if (!ZGuid.TryParse(secureQueryString[StlUsageReportRequestHelper.Constants.DatabasePk], out ZGuid databasePk))
				{
					databasePk = ZGuid.Empty;
				}

				if (!ZGuid.TryParse(secureQueryString[StlUsageReportRequestHelper.Constants.PriceItemPk], out ZGuid priceItemPk))
				{
					priceItemPk = ZGuid.Empty;
				}

				if (!ZGuid.TryParse(secureQueryString[StlUsageReportRequestHelper.Constants.CompanyPk], out ZGuid companyPk))
				{
					companyPk = ZGuid.Empty;
				}

				var systemCodes = secureQueryString[StlUsageReportRequestHelper.Constants.SystemCodes];

				if (new ZDateTime(periodStart).IsValid)
				{
					result = new StlReportingBusinessObject(factory, periodStart, databasePk, priceItemPk, companyPk, systemCodes);
				}
			}

			return result;
		}
	}
}
