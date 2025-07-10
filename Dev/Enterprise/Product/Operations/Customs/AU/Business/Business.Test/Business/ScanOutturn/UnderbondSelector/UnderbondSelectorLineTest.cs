using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class UnderbondSelectorLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShipmentSelectorLineContructor()
		{
			var cusUnderbond = Factory.NewWithValidTestData<CusUnderbond>();
			cusUnderbond.C4_SendersMessageReference = "U121212";
			cusUnderbond.C4_MovementReason = "Bla";
			cusUnderbond.C4_ModeOfMovement = "RAK";
			cusUnderbond.C4_IsMoveFromDischarge = true;
			cusUnderbond.C4_OriginPremiseID = "2300";
			cusUnderbond.C4_DestinationPremiseID = "2400";
			cusUnderbond.C4_ResponsiblePartyID = "54545";
			cusUnderbond.C4_FlightNo = "QF120";
			cusUnderbond.C4_ArrivalDate = new DateTime(2012, 12, 22, 10, 33, 01);
			cusUnderbond.C4_PiecesManifested = 22;
			cusUnderbond.C4_PackageType = "RRR";
			cusUnderbond.C4_Status = "CLR";
			cusUnderbond.C4_MessageStatus = "MMM";

			var line = GetUnderbondSelectorLine(cusUnderbond);

			AssertEquals("U121212", line.Reference);
			AssertEquals("Bla", line.MovementReason);
			AssertEquals("RAK", line.ModeOfMove);
			AssertEquals("2400", line.DestinationID);
			AssertEquals("2300", line.OriginID);
			AssertEquals("54545", line.ResponsiblePartyID);
			AssertEquals("QF120", line.FlightNumber);
			AssertEquals(new DateTime(2012, 12, 22, 10, 33, 01), line.ArivalDate);
			AssertEquals(22, line.PiecesManifested);
			AssertEquals("RRR", line.PackageType);
			AssertEquals("CLR", line.Status);
			AssertEquals("MMM", line.MessageStatus);
			AssertEquals(cusUnderbond.PK, line.Underbond.PK);
		}

		public void TestMaxLengthInUnderbondSelectorLineIsTheSameAsInCusUnderbond()
		{
			var line = (UnderbondSelectorLine)GetNewBusinessObject();
			AssertEquals(CusUnderbond.Schema.C4_SendersMessageReferenceMaxLength, line.ReferenceInfo.MaxLength);
			AssertEquals(CusUnderbond.Schema.C4_MovementReasonMaxLength, line.MovementReasonInfo.MaxLength);
			AssertEquals(CusUnderbond.Schema.C4_ModeOfMovementMaxLength, line.ModeOfMoveInfo.MaxLength);
			AssertEquals(CusUnderbond.Schema.C4_DestinationPremiseIDMaxLength, line.DestinationIDInfo.MaxLength);
			AssertEquals(CusUnderbond.Schema.C4_OriginPremiseIDMaxLength, line.OriginIDInfo.MaxLength);
			AssertEquals(CusUnderbond.Schema.C4_ResponsiblePartyIDMaxLength, line.ResponsiblePartyIDInfo.MaxLength);
			AssertEquals(CusUnderbond.Schema.C4_FlightNoMaxLength, line.FlightNumberInfo.MaxLength);
			AssertEquals(CusUnderbond.Schema.C4_PackageTypeMaxLength, line.PackageTypeInfo.MaxLength);
			AssertEquals(CusUnderbond.Schema.C4_StatusMaxLength, line.StatusInfo.MaxLength);
			AssertEquals(CusUnderbond.Schema.C4_MessageStatusMaxLength, line.MessageStatusInfo.MaxLength);
		}

		protected abstract UnderbondSelectorLine GetUnderbondSelectorLine(CusUnderbond cusUnderbond);

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return GetUnderbondSelectorLine(Factory.New<CusUnderbond>());
		}
	}
}
