using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(OverflowNoteCollection))]
	sealed class OverflowNoteCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OverflowNoteCollection>
	{
		protected override OverflowNoteCollection GetCollectionToTest()
		{
			return new OverflowNoteCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OverflowNote(null, null);
		}
	}
}
