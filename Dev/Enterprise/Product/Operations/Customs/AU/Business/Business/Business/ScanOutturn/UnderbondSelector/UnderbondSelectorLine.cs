using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class UnderbondSelectorLine : AutoUnderbondSelectorLine
	{
		public CusUnderbond Underbond { get; private set; }

		public UnderbondSelectorLine(CusUnderbond cusUnderbond)
		{
			this.Underbond = Argument.NotNull(cusUnderbond, "CusUnderbond");

			this.Reference = cusUnderbond.C4_SendersMessageReference;
			this.MovementReason = cusUnderbond.C4_MovementReason;
			this.ModeOfMove = cusUnderbond.C4_ModeOfMovement;
			this.DestinationID = cusUnderbond.C4_DestinationPremiseID;
			this.OriginID = cusUnderbond.C4_OriginPremiseID;
			this.ResponsiblePartyID = cusUnderbond.C4_ResponsiblePartyID;
			this.FlightNumber = cusUnderbond.C4_FlightNo;
			this.ArivalDate = cusUnderbond.C4_ArrivalDate;
			this.PiecesManifested = cusUnderbond.C4_PiecesManifested;
			this.PackageType = cusUnderbond.C4_PackageType;
			this.Status = cusUnderbond.C4_Status;
			this.MessageStatus = cusUnderbond.C4_MessageStatus;
			this.ContainerNo = cusUnderbond.ContainerLinked != null ? cusUnderbond.ContainerLinked.CN_ContainerNumber : ZString.Empty;
		}

		internal protected abstract void UpdateStatuses();

		protected override int Reference_MaxLength { get { return CusUnderbond.Schema.C4_SendersMessageReferenceMaxLength; } }
		protected override int MovementReason_MaxLength { get { return CusUnderbond.Schema.C4_MovementReasonMaxLength; } }
		protected override int ModeOfMove_MaxLength { get { return CusUnderbond.Schema.C4_ModeOfMovementMaxLength; } }
		protected override int DestinationID_MaxLength { get { return CusUnderbond.Schema.C4_DestinationPremiseIDMaxLength; } }
		protected override int OriginID_MaxLength { get { return CusUnderbond.Schema.C4_OriginPremiseIDMaxLength; } }
		protected override int ResponsiblePartyID_MaxLength { get { return CusUnderbond.Schema.C4_ResponsiblePartyIDMaxLength; } }
		protected override int FlightNumber_MaxLength { get { return CusUnderbond.Schema.C4_FlightNoMaxLength; } }
		protected override int PackageType_MaxLength { get { return CusUnderbond.Schema.C4_PackageTypeMaxLength; } }
		protected override int Status_MaxLength { get { return CusUnderbond.Schema.C4_StatusMaxLength; } }
		protected override int MessageStatus_MaxLength { get { return CusUnderbond.Schema.C4_MessageStatusMaxLength; } }
		protected override int ContainerNo_MaxLength { get { return CusSCAContainer.Schema.CN_ContainerNumberMaxLength; } }
	}
}
