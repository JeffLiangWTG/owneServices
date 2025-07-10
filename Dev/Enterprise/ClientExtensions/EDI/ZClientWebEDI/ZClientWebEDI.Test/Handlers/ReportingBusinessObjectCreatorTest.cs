using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class ReportingBusinessObjectCreatorTest : TestCaseWithFactory
	{
		public void TestCreateOdplReportingBusinessObject()
		{
			var queryString = new SecureQueryString { [OdplUsageReportRequestHelper.Constants.PeriodStart] = new DateTime(2010, 8, 1).ToString(ZDateTime.ISO8601ShortDateFormat), [OdplUsageReportRequestHelper.Constants.SystemCode] = BillingConstants.BillingSystem.ODM, [OdplUsageReportRequestHelper.Constants.OrganisationPk] = Guid.NewGuid().ToString(), [OdplUsageReportRequestHelper.Constants.LicenceCompanyPk] = Guid.NewGuid().ToString(), [OdplUsageReportRequestHelper.Constants.FileName] = "Dummy Report", [OdplUsageReportRequestHelper.Constants.FileType] = BillingConstants.FileExtensions.Pdf };
			var reportBizo = ReportingBusinessObjectCreator.CreateOdplReportingBusinessObject(queryString, Factory);
			AssertNotNull("Report Bizo should be created", reportBizo);
			AssertEquals("Report Bizo should be created with correct type", typeof(OdplReportingBusinessObject), reportBizo.GetType());
		}

		public void TestCreateStlReportingBusinessObject()
		{
			var queryString = new SecureQueryString { [StlUsageReportRequestHelper.Constants.PeriodStart] = new DateTime(2010, 8, 1).ToString(ZDateTime.ISO8601ShortDateFormat), [StlUsageReportRequestHelper.Constants.DatabasePk] = Guid.NewGuid().ToString(), [StlUsageReportRequestHelper.Constants.FileName] = "Dummy Report", [StlUsageReportRequestHelper.Constants.FileType] = BillingConstants.FileExtensions.Pdf, [StlUsageReportRequestHelper.Constants.SystemCodes] = "STL" };
			var reportBizo = ReportingBusinessObjectCreator.CreateStlReportingBusinessObject(queryString, Factory);
			AssertNotNull("Report Bizo should be created", reportBizo);
			AssertEquals("Report Bizo should be created with correct type", typeof(StlReportingBusinessObject), reportBizo.GetType());
		}
	}
}