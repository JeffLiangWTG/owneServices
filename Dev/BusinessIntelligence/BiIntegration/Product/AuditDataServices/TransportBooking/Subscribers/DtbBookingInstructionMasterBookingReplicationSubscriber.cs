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
	public class DtbBookingInstructionMasterBookingReplicationSubscriber : BaseDtbMasterBookingInsertingReplicationSubscriber
	{
		public DtbBookingInstructionMasterBookingReplicationSubscriber() : base()
		{
		}

		public DtbBookingInstructionMasterBookingReplicationSubscriber(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override string Code => "MBI";

		public override ITableSchema Table => DtbBookingInstructionSchema.Instance;

		protected override Type BizOType
		{
			get
			{
				if (bizOType == null)
				{
					bizOType = CargoWise.Application.ObjectFactory.GetType<IDtbBookingInstruction>();
				}

				return bizOType;
			}
		}
		Type bizOType;

		protected override Type ParentDtbEntityBizOType
		{
			get
			{
				if (parentBizOType == null)
				{
					parentBizOType = CargoWise.Application.ObjectFactory.GetType<IDtbBooking>();
				}

				return parentBizOType;
			}
		}
		Type parentBizOType;

		protected override SchemaColumn ParentDtbEntityLinkColumn => DtbBookingInstructionSchema.KN_KM_BookingMovement;

		protected override SchemaColumn ParentSubToMasterLinkColumn => DtbBookingSchema.KM_KM_MasterBooking;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Property name")]
		protected override string ParentEntityCollectionProperty => "Instructions";

		protected override IEnumerable<SchemaColumn> OtherIncludedColumns => new SchemaColumn[]
		{
			DtbBookingInstructionSchema.KN_KM_BookingMovement,
		};

		protected override string GetChangeRowEntityDescription(DataRow changeRow, BusinessObject parentEntity) =>
			FormattableString.Invariant($"Booking Instruction of type {changeRow[DtbBookingInstructionSchema.Constants.KN_InstructionType].GetDataRowValue<string>()} sequence {changeRow[DtbBookingInstructionSchema.Constants.KN_Sequence].GetDataRowValue<int>()} on Booking Job ID {((IDtbBooking)parentEntity).KM_JobID}");

		protected override string GetChangeRowEntityMultilingualDescription(DataRow changeRow, BusinessObject parentEntity) =>
			Res.GetString("e1e73cc4-dbaa-4ed5-9324-3cc5a5cb9813", "Booking Instruction of type {0} sequence {1} on Booking Job ID {2}", changeRow[DtbBookingInstructionSchema.Constants.KN_InstructionType].GetDataRowValue<string>(), changeRow[DtbBookingInstructionSchema.Constants.KN_Sequence].GetDataRowValue<int>(), ((IDtbBooking)parentEntity).KM_JobID);

		protected override string GetEntityDescription(BusinessObject entity)
		{
			var instruction = (IDtbBookingInstruction)entity;
			return FormattableString.Invariant($"Booking Instruction of type {instruction.KN_InstructionType} sequence {instruction.KN_Sequence} on Booking Job ID {instruction.Booking.KM_JobID}");
		}

		protected override string GetEntityMultilingualDescription(BusinessObject entity)
		{
			var instruction = (IDtbBookingInstruction)entity;
			return Res.GetString("e1e73cc4-dbaa-4ed5-9324-3cc5a5cb9813", "Booking Instruction of type {0} sequence {1} on Booking Job ID {2}", instruction.KN_InstructionType, instruction.KN_Sequence, instruction.Booking.KM_JobID);
		}

		protected override string GetErrorNoteSubAndMasterMessagePart(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subParentEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow, BusinessObject masterParentEntity)
		{
			var masterInstructionMultilingualDescription = GetInstructionMultilingualDescription(masterEntityChangeRow);
			var rowChangeTypeMultilingualDescription = GetRowChangeTypeMultilingualString(rowChangeType);
			var subBookingJobID = (subDtbBooking == null) ? ZString.Empty : ((IDtbBooking)subDtbBooking).KM_JobID;
			var masterBookingJobID = ((IDtbBooking)masterParentEntity).KM_JobID;
			if (rowChangeType == RowChangeType.Insert)
			{
				return Res.GetString("b83d7afe-1771-4826-abbb-13c201b3958c", "{0} of {1} on sub Booking Job ID {2} for master Booking Job ID {3}", rowChangeTypeMultilingualDescription, masterInstructionMultilingualDescription, subBookingJobID, masterBookingJobID);
			}
			else
			{
				var subInstruction = (IDtbBookingInstruction)subEntity;
				var subInstructionMultilingualDescription = (subInstruction == null) ? string.Empty : GetInstructionMultilingualDescription(subInstruction);
				return Res.GetString("72cad712-23c8-4cf5-9b91-be36e707a916", "{0} of sub {1} on Booking Job ID {2} for master {3} on Booking Job ID {4}", rowChangeTypeMultilingualDescription, subInstructionMultilingualDescription, subBookingJobID, masterInstructionMultilingualDescription, masterBookingJobID);
			}
		}

		string GetInstructionMultilingualDescription(IDtbBookingInstruction instruction) =>
			GetInstructionMultilingualDescription(instruction.KN_InstructionType, instruction.KN_Sequence);

		string GetInstructionMultilingualDescription(DataRow instructionChangeRow) =>
			GetInstructionMultilingualDescription(
				instructionChangeRow[DtbBookingInstructionSchema.Constants.KN_InstructionType].GetDataRowValue<string>(),
				instructionChangeRow[DtbBookingInstructionSchema.Constants.KN_Sequence].GetDataRowValue<int>());

		string GetInstructionMultilingualDescription(string instructionType, int instructionSequence) =>
			Res.GetString("2b341e36-3488-43d5-8885-38bb2337f8d2", "Booking Instruction of type {0} sequence {1}", instructionType, instructionSequence);

		protected override string GetErrorNoteSubAndMasterLogPart(RowChangeType rowChangeType, BusinessObject subEntity, BusinessObject subParentEntity, BusinessObject subDtbBooking, DataRow masterEntityChangeRow, BusinessObject masterParentEntity)
		{
			var masterInstructionDescription = GetInstructionDescription(masterEntityChangeRow);
			var subBookingJobID = ((IDtbBooking)subDtbBooking).KM_JobID;
			var masterBookingJobID = ((IDtbBooking)masterParentEntity).KM_JobID;
			if (rowChangeType == RowChangeType.Insert)
			{
				return FormattableString.Invariant($"{rowChangeType} of {masterInstructionDescription} on sub Booking Job ID {subBookingJobID} for master Booking Job ID {masterBookingJobID}");
			}
			else
			{
				var subInstruction = (IDtbBookingInstruction)subEntity;
				var subInstructionDescription = GetInstructionDescription(subInstruction);
				return FormattableString.Invariant($"{rowChangeType} of sub {subInstructionDescription} on Booking Job ID {subBookingJobID} for master {masterInstructionDescription} on Booking Job ID {masterBookingJobID}");
			}
		}

		string GetInstructionDescription(IDtbBookingInstruction instruction) =>
			GetInstructionDescription(instruction.KN_InstructionType, instruction.KN_Sequence);

		string GetInstructionDescription(DataRow instructionChangeRow) =>
			GetInstructionDescription(
				instructionChangeRow[DtbBookingInstructionSchema.Constants.KN_InstructionType].GetDataRowValue<string>(),
				instructionChangeRow[DtbBookingInstructionSchema.Constants.KN_Sequence].GetDataRowValue<int>());

		string GetInstructionDescription(string instructionType, int instructionSequence) =>
			FormattableString.Invariant($"Booking Instruction of type {instructionType} sequence {instructionSequence}");

		protected override BusinessObject GetDtbBookingFromParentEntity(BusinessObject parentEntity) => parentEntity;
	}
}
