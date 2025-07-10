using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ImportPreviousExpDecLineWrapperCollection))]
	sealed class ImportPreviousExpDecLineWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportPreviousExpDecLineWrapperCollection>
	{
		protected override ImportPreviousExpDecLineWrapperCollection GetCollectionToTest() => new ImportPreviousExpDecLineWrapperCollection(Enumerable.Empty<IImportEntryLine>(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ImportPreviousExpDecLineWrapper(0, new ImportPreviousExpDecLine());
	}
}
