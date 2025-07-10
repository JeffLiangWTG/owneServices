using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(StlUsageReportRequestHelper))]
	public class StlUsageReportRequestHelperTest : DataRequestHelperTestCase
	{
		protected override DataRequestHelper GetRequestHelper()
		{
			return new StlUsageReportRequestHelper();
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
				return "StlUsageReportRequestHandler.axd";
			}
		}
	}
}
