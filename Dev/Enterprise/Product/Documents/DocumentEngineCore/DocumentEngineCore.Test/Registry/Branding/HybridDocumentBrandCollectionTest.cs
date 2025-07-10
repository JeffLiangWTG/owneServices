using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(HybridDocumentBrandCollection))]
	public class HybridDocumentBrandCollectionTest : ClientAndAgentBrandingCollectionTestCase<HybridDocumentBrandCollection>
	{
		#region Implementation

		protected override HybridDocumentBrandCollection GetCollectionToTest()
		{
			return new HybridDocumentBrandCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new HybridDocumentBrand();
		}

		#endregion
	}
}
