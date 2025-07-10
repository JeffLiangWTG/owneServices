using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.DE.Registry;

namespace Enterprise.Customs.DE.Business
{
	public sealed class MonthlyClosingMessageBuilderLoader
	{
		MonthlyClosingMessageBuilderLoader() { }

		public static MonthlyClosingMessageBuilderLoader Instance => instance ??= new MonthlyClosingMessageBuilderLoader();

		[ThreadStatic]
		static MonthlyClosingMessageBuilderLoader instance;

		public IProduceMessageXml GetMessageBuilder(ZString messageCode, IImportMessageHeader importMessageHeader)
		{
			IProduceMessageXml result = null;
			var type = GetMessageBuilderTypeForCurrentVersion(messageCode);
			if (type != null)
			{
				result = (IProduceMessageXml)Activator.CreateInstance(type, importMessageHeader);
			}
			return result;
		}

		Type GetMessageBuilderTypeForCurrentVersion(ZString messageCode)
		{
			Type messageBuilderType = null;
			var currentATLASVersion = MessageVersionRegistry.CurrentAtlasVersion;
			var success = currentATLASVersion == ATLASVersionNumberList.Codes._102 && atlasVersion10_2MessageBuilders.TryGetValue(messageCode, out messageBuilderType);
			if (!success)
			{
				success = currentATLASVersion == ATLASVersionNumberList.Codes._101 && atlasVersion10_1MessageBuilders.TryGetValue(messageCode, out messageBuilderType);
				if (!success)
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($"Invalid DE MonthlyClosing Message Builder for code: {messageCode} requested for ATLAS Version {currentATLASVersion}"));
				}
			}
			return messageBuilderType;
		}

		readonly ImmutableDictionary<ZString, Type> atlasVersion10_1MessageBuilders = new Dictionary<ZString, Type>
		{
			{ MonthlyClosingFreeCirculation, typeof(Messaging.ATLASVersion10_1.CFCPEDMessageBuilder) },
			{ MonthlyClosingInwardProcessing, typeof(Messaging.ATLASVersion10_1.SCIPEDMessageBuilder) },
			{ MonthlyClosingBondedWarehouse, typeof(Messaging.ATLASVersion10_1.SCWPEDMessageBuilder) }
		}.ToImmutableDictionary();

		readonly ImmutableDictionary<ZString, Type> atlasVersion10_2MessageBuilders =
			ImmutableDictionary<ZString, Type>.Empty;

		public const string MonthlyClosingFreeCirculation = "CFCPED";
		public const string MonthlyClosingInwardProcessing = "SCIPED";
		public const string MonthlyClosingBondedWarehouse = "SCWPED";
	}
}

