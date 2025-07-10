using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestsSubclassesOf(typeof(DocumentBrandingCollection))]
	public abstract class DocumentBrandingCollectionTestCase<T> : ClientAndAgentBrandingCollectionTestCase<T> where T : DocumentBrandingCollection
	{
		public void TestElementIsDocumentBrandingBusinessObject()
		{
			AssertEquals("Collection.AddNew() should return a DocumentBrandingBusinessObject.", true, Collection.AddNew() is DocumentBrandingBusinessObject);
		}
	}
}
