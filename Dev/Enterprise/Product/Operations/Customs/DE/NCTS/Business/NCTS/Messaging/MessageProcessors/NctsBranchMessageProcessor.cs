using System;
using System.Collections.Generic;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsBranchMessageProcessor : Integration.Customs.DE.INctsMessageProcessor
	{
		public Dictionary<string, Type> NctsMessageProcessors => new Dictionary<string, Type>
		{
			{ "DETBS", typeof(NctsTBESTAMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETQSC).Substring(0, 5), typeof(NctsTRQSTAMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETPIA).Substring(0, 5), typeof(NctsDEPINCMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETPRH).Substring(0, 5), typeof(NctsDEPRELMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETSPC).Substring(0, 5), typeof(NctsDESPERMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETSSB).Substring(0, 5), typeof(NctsDESSTAMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETPJF).Substring(0, 5), typeof(NctsDEPREJMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETPSF).Substring(0, 5), typeof(NctsDEPSTAMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETSJB).Substring(0, 5), typeof(NctsDESREJMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.DETGAE).Substring(0, 5), typeof(NctsGUAACKMessageProcessor) },
		};
	}
}
