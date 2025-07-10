using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ImportEntryLineWrapperCollection))]
	sealed class ImportEntryLineWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportEntryLineWrapperCollection>
	{
		protected override ImportEntryLineWrapperCollection GetCollectionToTest() => new ImportEntryLineWrapperCollection(Enumerable.Empty<IImportEntryLine>(), ZDecimal.Zero, ZString.Empty, Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entryLine = new ImportEntryLine();
			return new ImportEntryLineWrapper(entryLine, ZDecimal.Zero, ZString.Empty, ZBool.False);
		}
	}
}
