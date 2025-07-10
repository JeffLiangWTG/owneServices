using CargoWise.Common;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class NonPersistentCheckInAllChildPiecesOrchestrator
	{
		public NonPersistentCheckInAllChildPiecesOrchestrator(ICcsukCusAwb awb)
		{
			Argument.NotNull(awb, "awb");
			Mawb = (CusMAWB)awb;
			CheckInAllChildPiecesData = new NonPersistentCheckInAllChildPieces(awb);
		}

		public NonPersistentCheckInAllChildPieces CheckInAllChildPiecesData { get; set; }

		public readonly CusMAWB Mawb;
	}
}
