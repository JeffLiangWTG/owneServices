using System.Collections.Generic;
using CargoWise.Customs.IL.MessageDefinitions.GPM.RES_135.GP_NG_1035_MSG2_GatepassFeedbackMessage;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.Message;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IL.Business
{
	sealed class ILGPM135ResponseMessagePrettier : ILEDIMessagePrettierBase<GpNg1035Msg2GatepassFeedbackMessage>
	{
		public ILGPM135ResponseMessagePrettier(MessageDataObject<GpNg1035Msg2GatepassFeedbackMessage> messageDataObject) : base(messageDataObject)
		{
		}

		public override ZString GetMessageInterpretation()
		{
			var factory = Factory;
			var cargoIdentifierTypeList = AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType);
			var gatepassFeedbackMessageIndicationTypeList = new GatepassFeedbackMessageIndicationTypeList();

			return BuildMessageInterpretation()
				.OfMultiple(
					() => MessageDataObject.MessageData.GatepassFeedbackMessage,
					b => b.WithResponseSection(
							(gatepassFeedbackMessage, sb) =>
							{
								var dictionary = new Dictionary<ZString, ZString>()
								{
									{ PrettiedCaptions.GatePassMovement.GatePassNumber, gatepassFeedbackMessage.GatepassNumber.ToString() },
									{ PrettiedCaptions.Common.Status, factory.GetCodeDescriptionFromRefCusCodeListCombinedCode(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILGatePassMovementStatus, gatepassFeedbackMessage.GatepassStatus.ToString()) }
								};

								if (gatepassFeedbackMessage.GatepassReturnCode.HasValue)
								{
									dictionary.Add(PrettiedCaptions.GatePassMovement.ReturnedCode, factory.GetCodeDescriptionFromRefCusCodeListCombinedCode(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILGatePassReturnedCode, gatepassFeedbackMessage.GatepassReturnCode.ToString()));
								}

								if (gatepassFeedbackMessage.CargoIdentifier != null)
								{
									dictionary.Add(PrettiedCaptions.GatePassMovement.CargoIdentifierType, $"{gatepassFeedbackMessage.CargoIdentifier.CargoIdentifierType} - {cargoIdentifierTypeList.GetDescriptionFromCode(gatepassFeedbackMessage.CargoIdentifier.CargoIdentifierType.ToString())}");
									dictionary.Add(PrettiedCaptions.GatePassMovement.IdentifierKey1, gatepassFeedbackMessage.CargoIdentifier.CargoIdentifierKey1);
									dictionary.Add(PrettiedCaptions.GatePassMovement.IdentifierKey2, gatepassFeedbackMessage.CargoIdentifier.CargoIdentifierKey2);
									dictionary.Add(PrettiedCaptions.GatePassMovement.IdentifierKey3, gatepassFeedbackMessage.CargoIdentifier.CargoIdentifierKey3);
								}

								sb.Append(ToBaseInformationPart(dictionary));
							}
						)
						.WithExceptionsSection(
							p => p.Exception != null && p.Exception.Count > 0,
							p => p.Exception,
							ex => ex.ExceptionLevel,
							ex => ex.ExeptionType,
							ex => ex.ExeptionDescription,
							ex => ex.ExceptionParms)
						.WithSectionOf<GpNg1035Msg2GatepassFeedbackMessageGatepassFeedbackMessageFeedbackIndications>(
							p => p.FeedbackIndications != null && p.FeedbackIndications.Count > 0,
							PrettiedCaptions.GatePassMovement.FeedbackIndicationsSection,
							p => () => p.FeedbackIndications,
							b1 => b1.WithSection(
								(feedbackIndication, sb) =>
								{
									if (feedbackIndication.IndicationType.HasValue)
									{
										var dictionary = new Dictionary<ZString, ZString>()
										{
											{ PrettiedCaptions.GatePassMovement.IndicationType, $"{feedbackIndication.IndicationType.Value} - {gatepassFeedbackMessageIndicationTypeList.GetDescriptionFromCode(feedbackIndication.IndicationType.Value.ToString())}" },
											{ PrettiedCaptions.GatePassMovement.IndicationUnit, feedbackIndication.IndicationUnit }
										};

										sb.Append(ToBaseInformationPart(dictionary));
									}
								}))
				)
				.OfSingle(
					() => MessageData.ResponseContentHeader,
					b => b
						.WithExceptionsSection(
							p => p.Exception?.Count > 0,
							p => p.Exception,
							ex => ex.ExceptionLevel,
							ex => ex.ExeptionType,
							ex => ex.ExeptionDescription,
							ex => ex.ExceptionParms
						)
				)
				.Build();
		}
	}
}
