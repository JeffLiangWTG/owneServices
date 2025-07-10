using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ImportPreviousExpDecLineWrapper))]
	sealed class ImportPreviousExpDecLineWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new ImportPreviousExpDecLineWrapper(0, null);
	}
}
