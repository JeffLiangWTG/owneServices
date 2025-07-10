using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusUnderbondCusOutturnCollection))]
	sealed class CusUnderbondCusOutturnCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTypeOfElements()
		{
			var underbond = Factory.New<CusUnderbond>();
			var collection = new CusUnderbondCusOutturnCollection(underbond, typeof(CusOutturn));
			underbond.Delete();
			AssertEquals(typeof(CusOutturn), collection.TypeOfElements);
		}

		public void TestCorrectAddNewType()
		{
			var underbond = Factory.New<CusUnderbond>();
			var collection = new CusUnderbondCusOutturnCollection(underbond, typeof(CusOutturn));
			AssertType<CusOutturn>(collection.AddNew());
			var shipment = Factory.New<CFSShipment>();
			underbond.LinkedObject = CFSShipmentWrapper.Load(shipment);
			collection = new CusUnderbondCusOutturnCollection(underbond, typeof(DepotCusOutturn));
			AssertType<DepotCusOutturn>(collection.AddNew());
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CusUnderbondCusOutturnCollection(Factory.New<CusUnderbond>(), typeof(CusOutturn));
	}
}
