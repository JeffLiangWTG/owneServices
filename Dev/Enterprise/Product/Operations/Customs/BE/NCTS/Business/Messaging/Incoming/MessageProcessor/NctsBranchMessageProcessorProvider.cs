using System;
using System.Collections.Generic;

namespace Enterprise.Customs.BE.NCTS.Business
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This is used indirectly in Enterprise.Customs.BE.Business.BECIncomingMessageProcessor via type Integration.Customs.BE.INctsMessageProcessorProvider")]
	public sealed class NctsBranchMessageProcessorProvider : Integration.Customs.BE.INctsMessageProcessorProvider
	{
		public Dictionary<string, Type> NctsMessageProcessors => new Dictionary<string, Type>
		{
			{ BEIncomingMessageTypes.Codes.Acknowledgement, typeof(AcknowledgementMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC004C.Cc004CType).Substring(2, 3), typeof(CC004CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC009C.Cc009CType).Substring(2, 3), typeof(CC009CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC019C.Cc019CType).Substring(2, 3), typeof(CC019CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC022C.Cc022CType).Substring(2, 3), typeof(CC022CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC025C.Cc025CType).Substring(2, 3), typeof(CC025CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC028C.Cc028CType).Substring(2, 3), typeof(CC028CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC029C.Cc029CType).Substring(2, 3), typeof(CC029CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC035C.Cc035CType).Substring(2, 3), typeof(CC035CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC043C.Cc043CType).Substring(2, 3), typeof(CC043CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC045C.Cc045CType).Substring(2, 3), typeof(CC045CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC051C.Cc051CType).Substring(2, 3), typeof(CC051CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC055C.Cc055CType).Substring(2, 3), typeof(CC055CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC056C.Cc056CType).Substring(2, 3), typeof(CC056CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC057C.Cc057CType).Substring(2, 3), typeof(CC057CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC060C.Cc060CType).Substring(2, 3), typeof(CC060CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC140C.Cc140CType).Substring(2, 3), typeof(CC140CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC182C.Cc182CType).Substring(2, 3), typeof(CC182CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC917C.Cc917CType).Substring(2, 3), typeof(CC917CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.CC928C.Cc928CType).Substring(2, 3), typeof(CC928CMessageProcessor) },
			{ nameof(CargoWise.Customs.BE.MessageDefinitions.NCTSVersion51_8_2.IENCTS043.Iencts043Type).Substring(2, 3).ToUpper(), typeof(IENCTS043MessageProcessor) },
			{ BEIncomingMessageTypes.Codes.CustomsServiceErrorUniversalEvent, typeof(NCTSCustomsServiceErrorUniversalEventResponseMessageProcessor) },
		};
	}
}
