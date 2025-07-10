using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(LocalExportStevedoreCollectionWrapper))]
	sealed class LocalExportStevedoreCollectionWrapperTest : NonPersistentBusinessObjectCollectionTestCase<LocalExportStevedoreCollectionWrapper>
	{
		protected override LocalExportStevedoreCollectionWrapper GetCollectionToTest() => new LocalExportStevedoreCollectionWrapper(Enumerable.Empty<ILocalExportStevedore>(), Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var stevedore = new LocalExportStevedore();
			return new LocalExportStevedoreWrapper(stevedore, Factory);
		}
	}
}
