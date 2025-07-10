
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader>))]
	public class NctsDepartureMovementHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader>>
	{
		public void TestFilterOnBM_SubApplicationCode()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			_ = header.MovementHeader;
			_ = header.ArrivalMovementHeader;
			var collection = new NctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader>(header);
			AssertEquals(Common.EU.NctsMoveHeaderType.Codes.Departure, collection.Single().BM_SubApplicationCode);
		}

		public void TestNoResultOnArrival()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var collection = new NctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader>(header);
			Assert(collection.CompleteFilter.IsNoResultQuery);
		}

		public void TestNoResultOnPhase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;

			var collection = new NctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader>(header);
			Assert(collection.CompleteFilter.IsNoResultQuery);
		}

		protected override NctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader> GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			Factory.Save();

			return (NctsDepartureMovementHeaderCollection<NctsDepartureMovementHeader>)header.DepartureMovementHeaders;
		}
	}
}
