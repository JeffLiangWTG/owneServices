using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(TaxStructCollection))]
	public class TaxCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TaxStructCollection>
	{
		protected override TaxStructCollection GetCollectionToTest()
		{
			return new TaxStructCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TaxStruct();
		}
	}
}
