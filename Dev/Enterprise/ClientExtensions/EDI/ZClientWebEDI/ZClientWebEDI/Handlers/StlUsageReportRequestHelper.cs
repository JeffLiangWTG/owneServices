using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class StlUsageReportRequestHelper : DataRequestHelper
	{
		public override string BaseUrl
		{
			get { return "StlUsageReportRequestHandler.axd"; }
		}

		public override bool EnableCache
		{
			get { return false; }
		}

		public override bool UseSecureQueryString
		{
			get { return false; }
		}

		public static class Constants
		{
			public const string PeriodStart = "PeriodStart";
			public const string DatabasePk = "DatabasePk";
			public const string PriceItemPk = "PriceItemPk";
			public const string CompanyPk = "CompanyPk";
			public const string FileName = "FileName";
			public const string FileType = "FileType";
			public const string SystemCodes = "SystemCodes";
		}
	}
}
