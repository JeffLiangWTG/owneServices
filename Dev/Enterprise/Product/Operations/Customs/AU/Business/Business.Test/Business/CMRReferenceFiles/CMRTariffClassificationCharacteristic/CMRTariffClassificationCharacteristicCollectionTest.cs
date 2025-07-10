using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTariffClassificationCharacteristicCollection))]
	sealed class CMRTariffClassificationCharacteristicCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CMRTariffClassificationCharacteristicCollection(Factory);
	}
}
