using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(UnderbondSelectorLineCollection))]
	sealed class UnderbondSelectorLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UnderbondSelectorLineCollection>
	{
		protected override UnderbondSelectorLineCollection GetCollectionToTest()
		{
			return new UnderbondSelectorLineCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AirUnderbondSelectorLine(Factory.New<CusUnderbond>());
		}
	}
}
