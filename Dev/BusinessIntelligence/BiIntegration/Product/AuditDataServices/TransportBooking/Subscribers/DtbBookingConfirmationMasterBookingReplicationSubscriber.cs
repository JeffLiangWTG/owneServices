using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration.TransportBooking;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.TransportBooking.Subscribers
{
	public class DtbBookingConfirmationMasterBookingReplicationSubscriber : BaseDtbMasterBookingInsertingReplicationSubscriber
	{
		public DtbBookingConfirmationMasterBookingReplicationSubscriber() : base()
		{
		}

		public DtbBookingConfirmationMasterBookingReplicationSubscriber(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override string Code => "MBK";

		public override ITableSchema Table => DtbBookingConfirmationSchema.Instance;

		protected override Type BizOType
		{
			get
			{
				if (bizOType == null)
				{
					bizOType = CargoWise.Application.ObjectFactory.GetType<IDtbBookingConfirmation>();
				}

				return bizOType;
			}
		}
		Type bizOType;

		protected override IEnumerable<SchemaColumn> OtherIncludedColumns => new SchemaColumn[]
		{
			DtbBookingConfirmationSchema.KK_KN_BookingInstruction,
		};

		protected override Type ParentDtbEntityBizOType
		{
			get
			{
				if (parentBizOType == null)
				{
					parentBizOType = CargoWise.Application.ObjectFactory.GetType<IDtbBookingInstruction>();
				}

				return parentBizOType;
			}
		}
		Type parentBizOType;

		protected override SchemaColumn ParentDtbEntityLinkColumn => DtbBookingConfirmationSchema.KK_KN_BookingInstruction;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Property name")]
		protected override string ParentEntityCollectionProperty => "Confirmations";

		protected override SchemaColumn ParentSubToMasterLinkColumn => DtbBookingInstructionSchema.KN_KN_MasterBookingInstruction;

		protected override string GetEntityDescription(BusinessObject entity)
		{
			var confirmation = (IDtbBookingConfirmation)entity;
			return FormattableString.Invariant($"Booking Confirmation of type {confirmation.KK_ConfirmationType} on Instruction of type {confirmation.Instruction.KN_InstructionType} sequence {confirmation.Instruction.KN_Sequence} on Booking Job ID {confirmation.Booking.KM_JobID}");
		}

		protected override string GetEntityMultilingualDescription(BusinessObject entity)
		{
			var confirmation = (IDtbBookingConfirmation)entity;
			return GetEntityMultilingualDescriptionCore(confirmation.KK_ConfirmationType, confirmation.Instruction.KN_InstructionType, confirmation.Instruction.KN_Sequence, confirmation.Booking.KM_JobID);
		}

		protected override string GetChangeRowEntityDescription(DataRow changeRow, BusinessObject parentEntity)
		{
			var instruction = (IDtbBookingInstruction)parentEntity;
			return FormattableString.Invariant($"Booking Confirmation of type {changeRow[DtbBookingConfirmationSchema.Constants.KK_ConfirmationType].GetDataRowValue<string>()} on Instruction of type {instruction.KN_InstructionType} sequence {instruction.KN_Sequence} on Booking Job ID {instruction.Booking.KM_JobID}");
		}

		protected override string GetChangeRowEntityMultilingualDescription(DataRow changeRow, BusinessObject parentEntity)
		{
			var instruction = (IDtbBookingInstruction)parentEntity;
			return GetEntityMultilingualDescriptionCore(changeRow[DtbBookingConfirmationSchema.Constants.KK_ConfirmationType].GetDataRowValue<string>(), instruction.KN_InstructionType, instruction.KN_Sequence, instruction.Booking.KM_JobID);
		}

		string GetEntityMultilingualDescriptionCore(string confirmationType, string instructionType, int instructionSequence, string bookingJobID) =>
			Res.GetString("ee42d3ad-e151-4db8-8677-cc3eb9fa6a65", "Booking Confirmation of type {0} on Booking Instruction of type {1} sequence {2} on Booking Job ID {3}", confirmationType, instructionType, instructionSequence, bookingJobID);

		protected override string GetErrorNoteSubAndMasterMessagePart(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subParentEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow, BusinessObject masterParentEntity)
		{
			var masterInstruction = (IDtbBookingInstruction)masterParentEntity;
			var masterConfirmationDescription = GetConfirmationMultilingualDescription(masterEntityChangeRow, masterInstruction);
			var subInstruction = (IDtbBookingInstruction)subParentEntity;
			var rowChangeTypeMultilingualDescription = GetRowChangeTypeMultilingualString(rowChangeType);
			var subBookingJobID = (subDtbBooking == null) ? ZString.Empty : ((IDtbBooking)subDtbBooking).KM_JobID;
			var masterBookingJobID = ((BusinessObject)masterParentEntity["Booking"])[DtbBookingSchema.KM_JobID];
			var subConfirmation = (IDtbBookingConfirmation)subEntity;
			var subConfirmationDescription = (rowChangeType == RowChangeType.Insert) ?
				GetConfirmationMultilingualDescription(masterEntityChangeRow, subInstruction) :
				GetConfirmationMultilingualDescription(subConfirmation, subInstruction);
			return Res.GetString("78c3ebd6-e415-43b6-9bde-a3bce4f9c391", "{0} of sub {1} on Booking Job ID {2} for master {3} on Booking Job ID {4}", rowChangeTypeMultilingualDescription, subConfirmationDescription, subBookingJobID, masterConfirmationDescription, masterBookingJobID);
		}

		string GetConfirmationMultilingualDescription(IDtbBookingConfirmation confirmation, IDtbBookingInstruction instruction)
		{
			var confirmationType = (confirmation == null) ? ZString.Empty : confirmation.KK_ConfirmationType;
			var instructionType = (instruction == null) ? ZString.Empty : instruction.KN_InstructionType;
			var instructionSequence = (instruction == null) ? ZInt.Zero : instruction.KN_Sequence;
			return GetConfirmationMultilingualDescription(confirmationType, instructionType, instructionSequence);
		}

		string GetConfirmationMultilingualDescription(DataRow confirmationChangeRow, IDtbBookingInstruction instruction)
		{
			var instructionType = (instruction == null) ? ZString.Empty : instruction.KN_InstructionType;
			var instructionSequence = (instruction == null) ? ZInt.Zero : instruction.KN_Sequence;
			return GetConfirmationMultilingualDescription(confirmationChangeRow[DtbBookingConfirmationSchema.Constants.KK_ConfirmationType].GetDataRowValue<string>(), instructionType, instructionSequence);
		}

		string GetConfirmationMultilingualDescription(string confirmationType, string instructionType, int instructionSequence) =>
			Res.GetString("de5e7d5f-32de-4c60-b836-1d05f682b161", "Booking Confirmation of type {0} on Instruction of type {1} sequence {2}", confirmationType, instructionType, instructionSequence);

		protected override string GetErrorNoteSubAndMasterLogPart(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subParentEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow, BusinessObject masterParentEntity)
		{
			var masterInstruction = (IDtbBookingInstruction)masterParentEntity;
			var masterConfirmationDescription = GetConfirmationDescription(masterEntityChangeRow, masterInstruction);
			var subInstruction = (IDtbBookingInstruction)subParentEntity;
			var subBookingJobID = ((IDtbBooking)subDtbBooking).KM_JobID;
			var masterBookingJobID = ((BusinessObject)masterParentEntity["Booking"])[DtbBookingSchema.KM_JobID];
			var subConfirmation = (IDtbBookingConfirmation)subEntity;
			var subConfirmationDescription = (rowChangeType == RowChangeType.Insert) ?
				GetConfirmationDescription(masterEntityChangeRow, subInstruction) :
				GetConfirmationDescription(subConfirmation, subInstruction);

			return FormattableString.Invariant($"{rowChangeType} of sub {subConfirmationDescription} on Booking Job ID {subBookingJobID} for master {masterConfirmationDescription} on Booking Job ID {masterBookingJobID}");
		}

		string GetConfirmationDescription(IDtbBookingConfirmation confirmation, IDtbBookingInstruction instruction)
		{
			var confirmationType = (confirmation == null) ? ZString.Empty : confirmation.KK_ConfirmationType;
			var instructionType = (instruction == null) ? ZString.Empty : instruction.KN_InstructionType;
			var instructionSequence = (instruction == null) ? ZInt.Zero : instruction.KN_Sequence;
			return GetConfirmationDescription(confirmationType, instructionType, instructionSequence);
		}

		string GetConfirmationDescription(DataRow confirmationChangeRow, IDtbBookingInstruction instruction)
		{
			var instructionType = (instruction == null) ? ZString.Empty : instruction.KN_InstructionType;
			var instructionSequence = (instruction == null) ? ZInt.Zero : instruction.KN_Sequence;
			return GetConfirmationDescription(confirmationChangeRow[DtbBookingConfirmationSchema.Constants.KK_ConfirmationType].GetDataRowValue<string>(), instructionType, instructionSequence);
		}

		string GetConfirmationDescription(string confirmationType, string instructionType, int instructionSequence) =>
			Res.GetString("afe74d56-71b7-4726-ab18-ed6fba0855b9", "Booking Confirmation of type {0} on Instruction of type {1} sequence {2}", confirmationType, instructionType, instructionSequence);

		protected override BusinessObject GetDtbBookingFromParentEntity(BusinessObject parentEntity) => (BusinessObject)parentEntity[nameof(IDtbBookingInstruction.Booking)];
	}
}
