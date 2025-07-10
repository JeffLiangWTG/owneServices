using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Diagnostics.Testing
{
	[TestedType(typeof(DocumentCollection))]
	public class DocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentCollection>
	{
		#region Implementation
		protected override DocumentCollection GetCollectionToTest()
		{
			return new DocumentCollection("");
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Document();
		}

		#endregion
	}
}
