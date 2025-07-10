using System;
using System.Collections.Specialized;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(StlUsageReportRequestHandler))]
	class StlUsageReportRequestHandlerTest : DataRequestHandlerTestCase<StlUsageReportRequestHelper>
	{
		protected override DataRequestHandler<StlUsageReportRequestHelper> GetNewRequestHandler()
		{
			var result = new StlUsageReportRequestHandlerForTest();
			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD", false);
			var db = lic.Database;
			var queryString = new SecureQueryString { [StlUsageReportRequestHelper.Constants.PeriodStart] = new DateTime(2010, 8, 1).ToString(ZDateTime.ISO8601ShortDateFormat), [StlUsageReportRequestHelper.Constants.DatabasePk] = db.PK.ToString(), [StlUsageReportRequestHelper.Constants.FileName] = "Dummy Report", [StlUsageReportRequestHelper.Constants.FileType] = BillingConstants.FileExtensions.Pdf, [StlUsageReportRequestHelper.Constants.SystemCodes] = "STL" };
			result.QueryString_Expose.Add(SecureQueryString.QueryStringKey, queryString.ToString());
			return result;
		}

		class StlUsageReportRequestHandlerForTest : StlUsageReportRequestHandler
		{
			public NameValueCollection QueryString_Expose
			{
				get
				{
					return QueryString;
				}
			}

			//Cannot test with StlReportingBusinessObject since client specific template is not loaded into database
			public override ZBlob GetBinaryData()
			{
				var reporting = BusinessObjects[0] as StlReportingBusinessObject;
				string fileType = BillingConstants.FileExtensions.Pdf;
				if (reporting != null && !string.IsNullOrEmpty(fileType))
				{
					lock (reporting)
					{
						StopStopwatchForTesting();
					}
				}

				return ZBlob.FromAscii("AAA");
			}
		}
	}
}
