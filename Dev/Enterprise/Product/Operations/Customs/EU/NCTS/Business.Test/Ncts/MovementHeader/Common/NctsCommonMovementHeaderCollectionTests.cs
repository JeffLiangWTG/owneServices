using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCommonMovementHeaderCollection))]
	class NctsCommonMovementHeaderCollectionTests : ActiveBusinessObjectCollectionTestCase<NctsCommonMovementHeaderCollection>
	{
		public void TestLoadMovementHeaders()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.DepartureAndArrival);
			_ = nctsHeader.UnloadingMovementHeader;
			AssertEquals("Header should contain 3 MovementHeader", 3, nctsHeader.MovementHeaders.Count);
		}

		protected override NctsCommonMovementHeaderCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			Factory.Save();
			return header.MovementHeaders;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var unloadingMovement = Factory.New<NctsUnloadingMovementHeader>();
			unloadingMovement.BM_SubApplicationCode = Common.EU.NctsMoveHeaderType.Codes.Unloading;
			return unloadingMovement;
		}
	}
}
