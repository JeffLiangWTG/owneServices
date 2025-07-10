using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCFXCollection))]
	sealed class DocCFXCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCFXCollection>
	{
		#region Implementation

		protected override DocCFXCollection GetCollectionToTest()
		{
			return new DocCFXCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocCFX("name", 5m);
		}

		#endregion
	}
}
