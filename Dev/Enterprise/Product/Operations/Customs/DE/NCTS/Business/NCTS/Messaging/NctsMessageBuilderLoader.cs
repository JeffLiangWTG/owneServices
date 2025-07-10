using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Registry;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NctsMessageBuilderLoader
	{
		NctsMessageBuilderLoader() { }

		public static NctsMessageBuilderLoader Instance => instance ??= new NctsMessageBuilderLoader();
		[ThreadStatic]
		static NctsMessageBuilderLoader instance;

		public OutboundMessageDetails GetMessageDetails(ZString messageCode) => GetMessageDetailsForCurrentVersion(messageCode);

		public IProduceMessageXml GetStatusRequestMessageBuilder(IStatusRequestHeader nctsMessageHeader) => (IProduceMessageXml)Activator.CreateInstance(GetMessageDetailsForCurrentVersion(NctsMessageTypeList.Codes.TRQQUE).MessageBuilderType, nctsMessageHeader);

		OutboundMessageDetails GetMessageDetailsForCurrentVersion(ZString messageCode)
		{
			OutboundMessageDetails messageBuilderDetails = null;
			var currentAtlasVersion = MessageVersionRegistry.CurrentAtlasVersion;
			var success = currentAtlasVersion == ATLASVersionNumberList.Codes._102 && nctsATLASVersion10_2MessageBuilders.TryGetValue(messageCode, out messageBuilderDetails);
			if (!success)
			{
				success = currentAtlasVersion == ATLASVersionNumberList.Codes._101 && nctsATLASVersion10_1MessageBuilders.TryGetValue(messageCode, out messageBuilderDetails);
				if (!success)
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($"Invalid DE NCTS Message Builder for code: {messageCode} requested for ATLAS Version {currentAtlasVersion}"));
				}
			}
			return messageBuilderDetails;
		}

		readonly ImmutableDictionary<ZString, OutboundMessageDetails> nctsATLASVersion10_1MessageBuilders = new Dictionary<ZString, OutboundMessageDetails>
		{
			{ NctsMessageTypeList.Codes.DESNOT, new OutboundMessageDetails(typeof(CargoWise.Customs.DE.MessageContracts.NCTS.ATLASVersion10_1.DESNOTMessageBuilder), typeof(DESNOTMessageHeaderProvider)) },
			{ NctsMessageTypeList.Codes.DESREM, new OutboundMessageDetails(typeof(CargoWise.Customs.DE.MessageContracts.NCTS.ATLASVersion10_1.DESREMMessageBuilder), typeof(DESREMMessageHeaderProvider)) },
			{ NctsMessageTypeList.Codes.DEPDAT, new OutboundMessageDetails(typeof(CargoWise.Customs.DE.MessageContracts.NCTS.ATLASVersion10_1.DEPDATMessageBuilder), typeof(DEPDATMessageHeaderProvider)) },
			{ NctsMessageTypeList.Codes.GUACOD, new OutboundMessageDetails(typeof(CargoWise.Customs.DE.MessageContracts.NCTS.ATLASVersion10_1.GUACODMessageBuilder), typeof(GUACODMessageHeaderProvider)) },
			{ NctsMessageTypeList.Codes.TRQQUE, new OutboundMessageDetails(typeof(CargoWise.Customs.DE.MessageContracts.NCTS.ATLASVersion10_1.TRQQUEMessageBuilder), typeof(string)) },
		}.ToImmutableDictionary();

		readonly ImmutableDictionary<ZString, OutboundMessageDetails> nctsATLASVersion10_2MessageBuilders =
			ImmutableDictionary<ZString, OutboundMessageDetails>.Empty;
	}
}
