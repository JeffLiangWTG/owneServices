using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class OdplUsageReportRequestHelper : DataRequestHelper
	{
		public override string BaseUrl
		{
			get { return "OdplUsageReportRequestHandler.axd"; }
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
			public const string SystemCode = "SystemCode";
			public const string OrganisationPk = "OrganisationPk";
			public const string ClientCompanyPk = "ClientCompanyPk";
			public const string LicenceCompanyPk = "LicenceCompanyPk";
			public const string DatabasePk = "DatabasePk";
			public const string FileType = "FileType";
			public const string FileName = "FileName";
		}
	}
}
