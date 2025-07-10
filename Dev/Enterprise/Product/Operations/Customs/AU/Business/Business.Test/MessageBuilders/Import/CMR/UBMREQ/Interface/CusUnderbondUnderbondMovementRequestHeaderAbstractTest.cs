using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CusUnderbondUnderbondMovementRequestHeaderAbstractTest : TestCaseWithFactory
	{
		public void TestRequestReasonCode()
		{
			Underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.DeliveryToFinalDestination;
			AssertEquals("RequestReasonCode", CMRUnderbondRequestCodes.Codes.DeliveryToFinalDestination, RequestHeader.RequestReasonCode);
		}

		public void TestUnderbondBySeaVoyageNumber()
		{
			Underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.SeaDomesticVessel;
			Underbond.C4_UnderbondBySeaVoyage = "123";
			AssertEquals("UnderbondBySeaVoyageNumber", "123", RequestHeader.UnderbondBySeaVoyageNumber);
		}

		public void TestUnderbondBySeaVesselID()
		{
			const string LloydsNumber = "9130913";
			Underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.SeaDomesticVessel;
			Underbond.C4_UnderbondBySeaLloydsIMONum = LloydsNumber;
			AssertEquals("UnderbondBySeaVesselID", LloydsNumber, RequestHeader.UnderbondBySeaVesselID);
		}

		public void TestUnderbondArrivalDate()
		{
			Underbond.C4_ArrivalDate = new ZDateTime(2005, 12, 12);
			AssertEquals("Arrival Date", new ZDateTime(2005, 12, 12), RequestHeader.EstimatedDateOfArrival);
		}

		public void TestUnderbondPieces()
		{
			Underbond.C4_PiecesManifested = 123;
			AssertEquals("Pieces Manifested", 123, RequestHeader.NumberOfPackages);
		}

		public void TestModeOfTransport()
		{
			AssertEquals("ModeOfTransport", "ROA", RequestHeader.ModeOfTransport);
		}

		public void TestDestaintionEstablishmentID()
		{
			AssertEquals("AirlineCode", "54321", RequestHeader.DestaintionEstablishmentID);
		}

		public void TestOriginatingEstablishmentID()
		{
			AssertEquals("OriginatingEstablishmentID", "12345", RequestHeader.OriginatingEstablishmentID);
		}

		public void TestUnderbondBySeaOverseasRoutingPort()
		{
			AssertEquals("UnderbondBySeaOverseasRoutingPort", ZString.Empty, RequestHeader.UnderbondBySeaOverseasRoutingPort);
		}

		protected abstract IUnderbondMovementRequestHeader RequestHeader { get; }

		CusUnderbond underbond;
		protected CusUnderbond Underbond
		{
			get
			{
				if (underbond == null)
				{
					underbond = Factory.New<CusUnderbond>();
					underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Road;
					underbond.C4_DischargePremiseID = "24680";
					underbond.C4_OriginPremiseID = "12345";
					underbond.C4_DestinationPremiseID = "54321";
				}
				return underbond;
			}
		}
	}
}
