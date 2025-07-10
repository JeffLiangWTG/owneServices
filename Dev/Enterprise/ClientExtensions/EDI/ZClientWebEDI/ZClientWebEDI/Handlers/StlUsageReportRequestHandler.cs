using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class StlUsageReportRequestHandler : DataRequestHandler<StlUsageReportRequestHelper>
	{
		public override string FileName
		{
			get
			{
				string fileName = SecureQueryString[StlUsageReportRequestHelper.Constants.FileName];
				string fileType = SecureQueryString[StlUsageReportRequestHelper.Constants.FileType];
				return fileName + "." + fileType;
			}
		}

		/// <summary>
		/// Note, the web page only offers PDF and CSV format. Excel was dropped. CSV is handled elsewhere.
		/// </summary>
		public override ZBlob GetBinaryData()
		{
			ZBlob result = ZBlob.Empty;
			var reporting = BusinessObjects[0] as StlReportingBusinessObject;
			string fileType = SecureQueryString[StlUsageReportRequestHelper.Constants.FileType];

			if (reporting != null && fileType == BillingConstants.FileExtensions.Pdf)
			{
				lock (reporting)
				{
					result = reporting.GetPdfUsageReport();
				}
			}

			return result;
		}

		public override string ContentType
		{
			get
			{
				string fileType = SecureQueryString[OdplUsageReportRequestHelper.Constants.FileType];
				if (fileType == BillingConstants.FileExtensions.Pdf)
				{
					return DataContentTypes.Pdf;
				}
				else
				{
					return DataContentTypes.Excel;
				}
			}
		}

		protected override ZGuid[] GetNewPKs()
		{
			return System.Array.Empty<ZGuid>();
		}

		protected override BusinessObject[] GetNewBusinessObjects()
		{
			return new BusinessObject[1] { GetReportingBizO() };
		}

		StlReportingBusinessObject GetReportingBizO()
		{
			return ReportingBusinessObjectCreator.CreateStlReportingBusinessObject(SecureQueryString, Factory);
		}

		SecureQueryString SecureQueryString
		{
			get { return secureQueryString ?? (secureQueryString = new SecureQueryString(QueryString[SecureQueryString.QueryStringKey])); }
		}
		SecureQueryString secureQueryString;
	}
}
