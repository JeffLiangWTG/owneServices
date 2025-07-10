using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureCargoDescRemarksCollection))]
sealed class NctsDepartureCargoDescRemarksCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var cargoDesc = Factory.New<NctsDepartureCargoDesc>();
		return new NctsDepartureCargoDescRemarksCollection(cargoDesc);
	}
}
