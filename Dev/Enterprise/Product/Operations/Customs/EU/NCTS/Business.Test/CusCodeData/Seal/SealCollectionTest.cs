using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(SealCollection<Seal>))]
	public class SealCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return new SealCollection<Seal>(header);
		}
	}

	[TestedType(typeof(SealCollection))]
	sealed class SealCollection_DoNotInheritTest : SealCollectionTest
	{
		public void TestAllSeals()
		{
			var sealCollection = (SealCollection)GetCollectionToTest();
			AssertEquals("When the collection is empty, AllSeals Count", 0, sealCollection.AllSeals.Count());

			sealCollection.AddNew().CY_Data = "1";
			sealCollection.AddNew().CY_Data = "";
			sealCollection.AddNew().CY_Data = "2";
			sealCollection.AddNew().CY_Data = "2";
			sealCollection.AddNew().CY_Data = "3";
			AssertContainsExactElementsInAnyOrder("When collection is filled, AllSeals", new ZString[] { "1", "2", "2", "3" }, sealCollection.AllSeals.ToArray());
		}
	}
}
