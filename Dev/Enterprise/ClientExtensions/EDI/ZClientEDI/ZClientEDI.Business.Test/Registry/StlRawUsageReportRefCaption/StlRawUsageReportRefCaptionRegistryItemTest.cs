using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(StlRawUsageReportRefCaptionRegistryItem))]
	class StlRawUsageReportRefCaptionRegistryItemTest : StronglyTypedRegistryItemTestCase<StlRawUsageReportRefCaptionCollection>
	{
		protected override StronglyTypedRegistryItem<StlRawUsageReportRefCaptionCollection, StlRawUsageReportRefCaptionCollection> GetNewRegistryItem()
		{
			return new StlRawUsageReportRefCaptionRegistryItem("StlRawUsageReportRefCaption", (NoResString)"Dummy Category", (NoResString)"Dummy Caption", null);
		}
	}
}
