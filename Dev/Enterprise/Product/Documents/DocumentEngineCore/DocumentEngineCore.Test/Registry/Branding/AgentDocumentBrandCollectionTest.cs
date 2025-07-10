using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(AgentDocumentBrandCollection))]
	public class AgentDocumentBrandCollectionTest : DocumentBrandingCollectionTestCase<AgentDocumentBrandCollection>
	{
		#region Implementation

		protected override AgentDocumentBrandCollection GetCollectionToTest()
		{
			return new AgentDocumentBrandCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AgentDocumentBrand();
		}

		#endregion
	}
}
