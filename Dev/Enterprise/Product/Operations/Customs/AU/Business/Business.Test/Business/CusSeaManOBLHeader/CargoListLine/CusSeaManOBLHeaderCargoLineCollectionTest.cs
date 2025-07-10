using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLHeaderCargoLineCollection))]
	public class CusSeaManOBLHeaderCargoLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsOnNewChild()
		{
			AssertEquals(TranHead.PK, Line.BO_BT);
			AssertEquals(TranHead, Line.TransportHeader);
		}

		public void TestChildDefaultsDischargePort()
		{
			Port.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("AUSYD", Line.BO_RL_NKDischargePort);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CusSeaManTranHead transportHeader = Factory.New<CusSeaManTranHead>();
			return new CusSeaManOBLHeaderCargoLineCollection(transportHeader.Arrivals.AddNew());
		}

		CusSeaManTranHead TranHead
		{
			get
			{
				if (tranHead == null)
				{
					tranHead = Factory.New<CusSeaManTranHead>();
				}
				return tranHead;
			}
		}
		CusSeaManTranHead tranHead;

		CusSeaManArrivalPort Port
		{
			get
			{
				if (port == null)
				{
					port = TranHead.Arrivals.AddNew();
				}
				return port;
			}
		}
		CusSeaManArrivalPort port;

		CusSeaManOBLHeaderCargoLine Line
		{
			get
			{
				if (line == null)
				{
					line = Port.CargoLines.AddNew();
				}
				return line;
			}
		}
		CusSeaManOBLHeaderCargoLine line;

		#endregion
	}
}
