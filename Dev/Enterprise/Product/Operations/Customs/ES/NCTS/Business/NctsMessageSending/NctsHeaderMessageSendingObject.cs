using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsHeaderMessageSendingObject : EU.NCTS.Business.NctsHeaderMessageSendingObject
{
	public NctsHeaderMessageSendingObject(NctsHeader nctsHeader, SendingType sendingType = SendingType.None, NctsMessageFunctionSet messageFunction = null) : base(nctsHeader)
	{
		this.sendingType = sendingType;
		this.messageFunction = messageFunction;
		SetMessageType();
	}

	readonly SendingType sendingType;
	readonly NctsMessageFunctionSet messageFunction;

	public new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

	public new sealed class Schema : EU.NCTS.Business.NctsHeaderMessageSendingObject.Schema
	{
		public const string MessageSubType = "MessageSubType";
		public const string CustomsStatus = "CustomsStatus";
		public const string RequestDispatch = "RequestDispatch";
		public const string ReasonForCancellation = "ReasonForCancellation";
	}

	#region New Properties

	public ZString ReasonForCancellation
	{
		get => reasonForCancellation;
		set
		{
			SetNonPersistentPropertyValue(ReasonForCancellationInfo, ref reasonForCancellation, value);
			if (!IsValidationSuspended)
			{
				(Validation as NctsHeaderMessageSendingObjectValidation).ValidateReasonForCancellation();
			}
		}
	}
	ZString reasonForCancellation;

	public ZPropertyInfo ReasonForCancellationInfo => GetZPropertyInfo(Schema.ReasonForCancellation);

	[List(nameof(RequestDispatchList))]
	[ReadOnlyMember(nameof(RequestDispatch_ReadOnly))]
	public ZString RequestDispatch
	{
		get => requestDispatch;
		set => SetNonPersistentPropertyValue(RequestDispatchInfo, ref requestDispatch, value);
	}
	ZString requestDispatch;

	public ZPropertyInfo RequestDispatchInfo => GetZPropertyInfo(Schema.RequestDispatch);

	ZBool RequestDispatch_ReadOnly => NctsHeader.GetAllSendableEDocPivots().Count == 1;

	[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ES.Business.MessageSending.NctsHeaderMessageSendingObject|MessageSubType", ShortCaption = "Msg. Sub Type", Caption = "Message Sub Type")]
	[ReadOnlyMember(nameof(MessageSubType_ReadOnly))]
	public ZString MessageSubType
	{
		get => messageSubType;
		set
		{
			SetNonPersistentPropertyValue(MessageSubTypeInfo, ref messageSubType, value);
		}
	}
	ZString messageSubType;

	public ZPropertyInfo MessageSubTypeInfo => GetZPropertyInfo(Schema.MessageSubType);

	ZBool MessageSubType_ReadOnly => true;

	[CargoWiseOne.ResourceStrings.ResourceStringData("NPBO:Enterprise.Customs.ES.Business.MessageSending.NctsHeaderMessageSendingObject|CustomsStatus", ShortCaption = "Customs Status", Caption = "Customs Status")]
	[ReadOnlyMember(nameof(CustomsStatus_ReadOnly))]
	public ZString CustomsStatus
	{
		get => customsStatus;
		set
		{
			SetNonPersistentPropertyValue(CustomsStatusInfo, ref customsStatus, value);
		}
	}
	ZString customsStatus;

	public ZPropertyInfo CustomsStatusInfo => GetZPropertyInfo(Schema.CustomsStatus);

	ZBool CustomsStatus_ReadOnly => true;

	#endregion

	[List(nameof(MessageTypeList))]
	[ReadOnlyMember(nameof(MessageType_ReadOnly))]
	public override ZString MessageType { get => base.MessageType; set => base.MessageType = value; }

	ZBool MessageType_ReadOnly => !(NctsHeader.IsPhase5Departure || NctsHeader.IsPhase5Arrival) || IsPhase5DepartureAndAdditionalDeclarationTypeAorDorEmptyAndCustomsStatusEmpty || MessageTypeList.Count < 2;

	ZBool IsPhase5DepartureAndAdditionalDeclarationTypeAorDorEmptyAndCustomsStatusEmpty =>  NctsHeader.IsPhase5Departure && CustomsStatus.IsEmpty && (IsAdditionalDeclarationType_A || IsAdditionalDeclarationType_D || IsAdditionalDeclarationType_Empty);
	ZBool IsAdditionalDeclarationType_A => NctsHeader.MovementHeader.BM_AdditionalDeclarationType == EU.NCTS.Business.NctsTypeOfAdditionalDeclarationList.Codes.A;
	ZBool IsAdditionalDeclarationType_D => NctsHeader.MovementHeader.BM_AdditionalDeclarationType == EU.NCTS.Business.NctsTypeOfAdditionalDeclarationList.Codes.D;
	ZBool IsAdditionalDeclarationType_Empty => NctsHeader.MovementHeader.BM_AdditionalDeclarationType.IsEmpty;

	public override ZString LRN =>
		(MessageType == DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration
		&& NctsHeader.IsArrivalMovement)
			? (NctsHeader.ArrivalMovementHeader.HeaderTNN?.MovementHeader?.BM_PaperlessInbondNum ?? NctsHeader.ArrivalMovementHeader?.BM_PaperlessInbondNum ?? ZString.Empty)
			: (NctsHeader.MovementHeader?.BM_PaperlessInbondNum ?? NctsHeader.ArrivalMovementHeader?.BM_PaperlessInbondNum ?? ZString.Empty);

	protected override void SetDefaultDataCore()
	{
		base.SetDefaultDataCore();
		MessageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
		CustomsStatus = NctsHeader.MovementHeader?.BM_CustomsStatus ?? NctsHeader.ArrivalMovementHeader?.BM_CustomsStatus ?? ZString.Empty;
		RequestDispatch = YesNoList.Codes.Yes;
	}

	protected override EU.NCTS.Business.NctsHeaderMessageSendingObjectValidation GetNewValidation() => new NctsHeaderMessageSendingObjectValidation(this);

	#region SetMessageType

	ZString SetDefaultMessageType()
	{
		if (CustomsStatus.IsEmpty)
		{
			return IsAdditionalDeclarationType_A
					? DeclarationMessageTypeList.Codes.Ncts5Departure
					: IsAdditionalDeclarationType_D
					? DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration
					: ZString.Empty;
		}

		return MessageTypeList[0].Code;
	}

	void SetMessageType()
	{
		MessageType = sendingType == SendingType.TransitQuery ? (ZString)DeclarationMessageTypeList.Codes.TransitNcts5Query : GetArrivalOrDepartureMessageType();
	}

	ZString GetArrivalOrDepartureMessageType() =>
		NctsHeader switch
		{
			var x when x.IsArrivalMovement => GetArrivalMessageType(x),
			var x when x.IsDepartureMovement => GetDepartureMessageType(x),
			_ => ZString.Empty
		};

	ZString GetArrivalMessageType(NctsHeader header)
	{
		string GetDefaultMessageTypeArrival() =>
			header switch
			{
				var x when x.IsPhase5 && MessageTypeList.Count > 0 => MessageTypeList[0].Code,
				var x when x.IsPhase4 => DeclarationMessageTypeList.Codes.NctsArrivalNotification,
				_ => ZString.Empty
			};

		string GetMessageTypeByMessageFunction() =>
			messageFunction switch
			{
				NctsMessageFunctionSet.ArrivalNotificationMessage when header.CombinedMessage => DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs,
				NctsMessageFunctionSet.UnloadingRemarksMessage => DeclarationMessageTypeList.Codes.NctsUnloadingRemarks,
				NctsMessageFunctionSet.CombinedArrivalAndDepartureMessage when header.CombinedMessage => DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb,
				NctsMessageFunctionSet.CombinedArrivalAndDepartureMessage => DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi,
				_ => GetDefaultMessageTypeArrival()
			};

		return messageFunction != null ? GetMessageTypeByMessageFunction() : GetDefaultMessageTypeArrival();
	}

	ZString GetDepartureMessageType(NctsHeader header) =>
		header switch
		{
			var x when x.IsPhase5 && MessageTypeList.Count > 0 => SetDefaultMessageType(),
			var x when x.IsPhase4 && x.MovementHeader.IsTIRDeclaration => DeclarationMessageTypeList.Codes.NctsTir,
			var x when x.IsPhase4 => DeclarationMessageTypeList.Codes.NctsDeparture,
			_ => ZString.Empty
		};

	#endregion

	#region Lookup Lists

	public CodeDescriptionPairList RequestDispatchList => Factory.GetCachedValue<YesNoList>();

	public CodeDescriptionPairList MessageTypeList =>
		(NctsHeader.IsPhase5Departure, NctsHeader.IsPhase5Arrival) switch
		{
			(true, false) => MessageTypeListForPhase5Departure,
			(false, true) => MessageTypeListForPhase5Arrival,
			_ => Factory.GetCachedValue<DeclarationMessageTypeList>()
		};

	CodeDescriptionPairList MessageTypeListForPhase5Departure =>
		(string)NctsHeader.MovementHeader?.BM_CustomsStatus switch
		{
			"" => MessageTypeListForPhase5DepartureStatusEmpty,
			ESNCTS5DepartureCustomsStatusList.Codes.PreLodged => MessageTypeListForPhase5DepartureStatusPRE,
			ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl => MessageTypeListForPhase5DepartureStatusCO1,
			ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance => MessageTypeListForPhase5DepartureStatusDGP,
			_ => []
		};

	CodeDescriptionPairList MessageTypeListForPhase5DepartureStatusEmpty => messageTypeListForPhase5Departure ??= GetMessageTypeListForPhase5Departure();
	CodeDescriptionPairList messageTypeListForPhase5Departure;
	CodeDescriptionPairList GetMessageTypeListForPhase5Departure()
	{
		var list = new CodeDescriptionPairList();
		list.AddPair(DeclarationMessageTypeList.Codes.Ncts5Departure, DeclarationMessageTypeList.Descriptions.Ncts5Departure);
		list.AddPair(DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, DeclarationMessageTypeList.Descriptions.Ncts5DeparturePreDeclaration);
		return list;
	}

	CodeDescriptionPairList MessageTypeListForPhase5DepartureStatusPRE => messageTypeListForPhase5DeparturePRE ??= GetMessageTypeListForPhase5DeparturePRE();
	CodeDescriptionPairList messageTypeListForPhase5DeparturePRE;
	CodeDescriptionPairList GetMessageTypeListForPhase5DeparturePRE()
	{
		var list = new CodeDescriptionPairList();
		list.AddPair(DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, DeclarationMessageTypeList.Descriptions.Ncts5DepartureNotification);
		list.AddPair(DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, DeclarationMessageTypeList.Descriptions.Ncts5DepartureAmendment);
		list.AddPair(DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation, DeclarationMessageTypeList.Descriptions.Ncts5DepartureCancellation);
		return list;
	}

	CodeDescriptionPairList MessageTypeListForPhase5DepartureStatusCO1 => messageTypeListForPhase5DepartureCO1 ??= GetMessageTypeListForPhase5DepartureCO1();
	CodeDescriptionPairList messageTypeListForPhase5DepartureCO1;
	CodeDescriptionPairList GetMessageTypeListForPhase5DepartureCO1()
	{
		var list = new CodeDescriptionPairList();
		list.AddPair(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, DeclarationMessageTypeList.Descriptions.Ncts5DepartureAnnexes);
		return list;
	}

	CodeDescriptionPairList MessageTypeListForPhase5DepartureStatusDGP => messageTypeListForPhase5DepartureDGP ??= GetMessageTypeListForPhase5DepartureDGP();
	CodeDescriptionPairList messageTypeListForPhase5DepartureDGP;
	CodeDescriptionPairList GetMessageTypeListForPhase5DepartureDGP()
	{
		var list = new CodeDescriptionPairList();
		list.AddPair(DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation, DeclarationMessageTypeList.Descriptions.Ncts5DepartureCancellation);
		return list;
	}

	CodeDescriptionPairList MessageTypeListForPhase5Arrival =>
		(string)NctsHeader.ArrivalMovementHeader?.BM_CustomsStatus switch
		{
			"" when NctsHeader.ESNctsHeader.CEN_TNNArrival && RelevantTNNDepartureMRNIsNotAllocated => MessageTypeListForPhase5ArrivalStatusEmptyWithTNN,
			"" => MessageTypeListForPhase5ArrivalStatusEmpty,
			ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted => MessageTypeListForPhase5ArrivalStatusUAP,
			_ => []
		};

	bool RelevantTNNDepartureMRNIsNotAllocated => (NctsHeader.ArrivalMovementHeader.HeaderTNN?.MovementHeader?.BM_CustomsStatus ?? ZString.Empty) != ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated;

	CodeDescriptionPairList MessageTypeListForPhase5ArrivalStatusEmptyWithTNN => messageTypeListForPhase5ArrivalEmptyWithTNN ??= GetMessageTypeListForPhase5DArrivalEmptyWithTNN();
	CodeDescriptionPairList messageTypeListForPhase5ArrivalEmptyWithTNN;
	CodeDescriptionPairList GetMessageTypeListForPhase5DArrivalEmptyWithTNN()
	{
		var list = new CodeDescriptionPairList();
		list.AddPair(DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration, DeclarationMessageTypeList.Descriptions.Ncts5IndirectDepartureRegistration);
		list.AddPair(DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification, DeclarationMessageTypeList.Descriptions.Ncts5ArrivalNotification);
		return list;
	}

	CodeDescriptionPairList MessageTypeListForPhase5ArrivalStatusEmpty => messageTypeListForPhase5ArrivalEmpty ??= GetMessageTypeListForPhase5DArrivalEmpty();
	CodeDescriptionPairList messageTypeListForPhase5ArrivalEmpty;
	CodeDescriptionPairList GetMessageTypeListForPhase5DArrivalEmpty()
	{
		var list = new CodeDescriptionPairList();
		list.AddPair(DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification, DeclarationMessageTypeList.Descriptions.Ncts5ArrivalNotification);
		return list;
	}

	CodeDescriptionPairList MessageTypeListForPhase5ArrivalStatusUAP => messageTypeListForPhase5ArrivalUAP ??= GetMessageTypeListForPhase5DArrivalUAP();
	CodeDescriptionPairList messageTypeListForPhase5ArrivalUAP;
	CodeDescriptionPairList GetMessageTypeListForPhase5DArrivalUAP()
	{
		var list = new CodeDescriptionPairList();
		list.AddPair(DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods, DeclarationMessageTypeList.Descriptions.Ncts5ArrivalDownloadGoods);
		return list;
	}

	#endregion
}
