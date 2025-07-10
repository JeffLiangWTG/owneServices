using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestsSubclassesOf(typeof(ClientAndAgentBrandingCollection))]
	public abstract class ClientAndAgentBrandingCollectionTestCase<T> : RegistryBusinessObjectCollectionTestCase<T> where T : ClientAndAgentBrandingCollection
	{
		public void TestElementIsClientAndAgentBrandingBusinessObject()
		{
			AssertEquals("Collection.AddNew() should return a ClientAndAgentBrandingBusinessObject.", true, Collection.AddNew() is ClientAndAgentBrandingBusinessObject);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
