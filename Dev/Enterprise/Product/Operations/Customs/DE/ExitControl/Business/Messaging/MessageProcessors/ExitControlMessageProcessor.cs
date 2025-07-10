using System;
using System.Collections.Generic;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class ExitControlMessageProcessor : Integration.Customs.DE.IExitControlMessageProcessor
	{
		public Dictionary<string, Type> ExitControlMessageProcessors => new Dictionary<string, Type>
		{
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXTLF).Substring(0, 5), typeof(EXTCTLMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXTJE).Substring(0, 5), typeof(EXTREJMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXTSE).Substring(0, 5), typeof(EXTSTAMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXTDE).Substring(0, 5), typeof(EXTDATMessageProcessor) },
			{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEERRG).Substring(0, 5), typeof(ERRNCKMessageProcessor) }
		};
	}
}
