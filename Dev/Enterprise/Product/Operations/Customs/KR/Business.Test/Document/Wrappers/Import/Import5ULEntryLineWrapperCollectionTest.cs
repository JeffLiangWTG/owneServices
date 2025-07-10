using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(Import5ULEntryLineWrapperCollection))]
	sealed class Import5ULEntryLineWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<Import5ULEntryLineWrapperCollection>
	{
		protected override Import5ULEntryLineWrapperCollection GetCollectionToTest() => new Import5ULEntryLineWrapperCollection(new Import5ULHeader(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = new Import5ULEntryLine();

			return new Import5ULEntryLineWrapper(line, ZBool.False);
		}
	}
}
