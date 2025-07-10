using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[TestedType(typeof(InvoiceRequestHelper))]
	public class InvoiceRequestHelperTest : DataRequestHelperTestCase
	{
		protected override DataRequestHelper GetRequestHelper()
		{
			return new InvoiceRequestHelper();
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
				return true;
			}
		}

		protected override string ExpectedBaseUrl
		{
			get
			{
				return "InvoiceRequestHandler.axd";
			}
		}
	}
}
