using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(LocalExportAmendItemWrapper))]
	sealed class LocalExportAmendItemWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new LocalExportAmendItemWrapper(new LocalExportAmendItem(), Factory);
	}
}
