using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(StlRawUsageReportRefCaption))]
	public class StlRawUsageReportRefCaptionTest : RegistryBusinessObjectTemplateTestCase<StlRawUsageReportRefCaption>
	{
		protected override StlRawUsageReportRefCaption GetBusinessObjectToClone()
		{
			return new StlRawUsageReportRefCaption();
		}

		protected override StlRawUsageReportRefCaption GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
