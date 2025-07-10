using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DeferredCusOutturnHeaderSavingOptions))]
	sealed class DeferredCusOutturnHeaderSavingOptionsTest : NonPersistentBusinessObjectTestCase
	{
		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject() => new DeferredCusOutturnHeaderSavingOptions(Factory.New<Customs.Business.CusOutturnHeader>());
	}
}
