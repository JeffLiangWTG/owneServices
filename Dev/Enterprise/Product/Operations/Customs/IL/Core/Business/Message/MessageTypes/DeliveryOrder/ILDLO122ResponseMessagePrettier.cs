using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.DLO.RES_121.MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business.Message
{
	public class ILDLO122ResponseMessagePrettier : ILEDIMessagePrettierBase<MnNg1220Msg22DeliveryOrderFeedBackMessage>
	{
		public ILDLO122ResponseMessagePrettier(MessageDataObject<MnNg1220Msg22DeliveryOrderFeedBackMessage> messageDataObject) : base(messageDataObject)
		{
		}

		public override ZString GetMessageInterpretation()
			=> BuildMessageInterpretation()
			.OfSingle(
				() => MessageData.DeliveryOrderResponse.FirstOrDefault(),
				b => b
					.WithResponseSection(
						(p, sb) =>
						{
							var dictionary = new Dictionary<ZString, ZString>()
							{
								{ PrettiedCaptions.DeliveryOrder.DeliveryOrderNumber, p?.DeliveryOrderNumber.ToString() },
								{ PrettiedCaptions.Common.Status, new DeliveryOrderResponseStatusList().GetDescriptionFromCode(p?.ResponseStatus.ToString()) }
							};

							sb.Append(ToBaseInformationPart(dictionary));
						})
					.WithExceptionsSection(
						p => p?.ResponseStatus.ToString() is not DeliveryOrderResponseStatusList.Codes.Accepted && p.Exception?.Count > 0,
						p => p.Exception,
						ex => ex.ExceptionLevel,
						ex => ex.ExeptionType,
						ex => ex.ExeptionDescription,
						ex => ex.ExceptionParms
						)
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
