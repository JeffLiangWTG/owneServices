using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(LocalExportEntryLineCollectionWrapper))]
	sealed class LocalExportEntryLineCollectionWrapperTest : NonPersistentBusinessObjectCollectionTestCase<LocalExportEntryLineCollectionWrapper>
	{
		protected override LocalExportEntryLineCollectionWrapper GetCollectionToTest() => new LocalExportEntryLineCollectionWrapper(Enumerable.Empty<ILocalExportEntryLine>(), Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entryLine = new LocalExportEntryLine();
			return new LocalExportEntryLineWrapper(entryLine, Factory);
		}
	}
}
