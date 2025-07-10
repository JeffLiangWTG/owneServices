using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(StlRawUsageReportRefCaptionCollection))]
	internal sealed class StlRawUsageReportRefCaptionCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<StlRawUsageReportRefCaptionCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override StlRawUsageReportRefCaptionCollection GetCollectionToTest()
		{
			return new StlRawUsageReportRefCaptionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StlRawUsageReportRefCaption();
		}

		#endregion
	}
}
