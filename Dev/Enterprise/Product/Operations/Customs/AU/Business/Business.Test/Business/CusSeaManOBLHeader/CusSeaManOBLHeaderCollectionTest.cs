using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLHeaderCollection))]
	public class CusSeaManOBLHeaderCollectionTest : Customs.Business.Testing.CusSeaManOBLHeaderCollectionTest
	{
		public void TestRelationshipFilter()
		{
			CusSeaManTranHead tranHead = Factory.New<CusSeaManTranHead>();
			CusSeaManArrivalPort port = tranHead.Arrivals.AddNew();
			CusSeaManOBLHeader header1 = tranHead.OceanBills.AddNew();
			CusSeaManOBLHeader header2 = tranHead.OceanBills.AddNew();

			AssertEquals("Includes both oceanbills", 2, tranHead.OceanBills.Count);
			AssertEquals("Arrival port has no cargo lines", 0, port.CargoLines.Count);

			header2.BO_BA = port.PK;

			tranHead.OceanBills.Load();
			port.CargoLines.Load();

			AssertEquals("Excludes oceanbills linked to an arrivalport", 1, tranHead.OceanBills.Count);
			AssertEquals("Arrival port has linked oceanbill as cargo line", 1, port.CargoLines.Count);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusSeaManOBLHeaderCollection(Factory.New<CusSeaManTranHead>());
		}

		#endregion
	}
}
