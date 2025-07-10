using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(OdplUsageReportRequestHelper))]
	public class OdplUsageReportRequestHelperTest : DataRequestHelperTestCase
	{
		protected override DataRequestHelper GetRequestHelper()
		{
			return new OdplUsageReportRequestHelper();
		}

		protected override bool ExpectedEnableCache
		{
			get
			{
				return false;
			}
		}

		protected override bool ExpectedUseSecureQueryString
		{
			get
			{
				return false;
			}
		}

		protected override string ExpectedBaseUrl
		{
			get
			{
				return "OdplUsageReportRequestHandler.axd";
			}
		}
	}
}
