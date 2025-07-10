using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.DE.Registry;
using MessageDefinitions = CargoWise.Customs.DE.MessageDefinitions;
using TS = CargoWise.Customs.DE.MessageContracts.TemporaryStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public sealed class TemporaryStorageMessageBuilderLoader
	{
		TemporaryStorageMessageBuilderLoader() { }

		public static TemporaryStorageMessageBuilderLoader Instance => instance ??= new TemporaryStorageMessageBuilderLoader();
		[ThreadStatic]
		static TemporaryStorageMessageBuilderLoader instance;

		public IProduceMessageXml GetMessageBuilder(ZString messageCode, ITempStorageDec tempStorageDec)
		{
			IProduceMessageXml result = null;
			var currentAtlasVersion = MessageVersionRegistry.CurrentAtlasVersion;
			if (currentAtlasVersion == ATLASVersionNumberList.Codes._101 && temporaryStorageATLASVersion10_1MessageBuilders.TryGetValue(messageCode, out var messageBuilderDetails10_1))
			{
				result = messageBuilderDetails10_1.SumAHeaderType.HasValue
					? (IProduceMessageXml)Activator.CreateInstance(messageBuilderDetails10_1.MessageBuilderType, tempStorageDec, (MessageDefinitions.ATLASVersion10_1.SCPRLKHeaderDeclarationType)messageBuilderDetails10_1.SumAHeaderType.Value)
					: (IProduceMessageXml)Activator.CreateInstance(messageBuilderDetails10_1.MessageBuilderType, tempStorageDec);
			}
			else if (currentAtlasVersion == ATLASVersionNumberList.Codes._102 && temporaryStorageATLASVersion10_2MessageBuilders.TryGetValue(messageCode, out _))
			{
				throw new NotImplementedException("ATLAS version 10.2 is not implemented yet");
			}
			if (result == null)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"Invalid DE Temporary Storage Message Builder for code: {messageCode} requested for ATLAS Version {currentAtlasVersion}"));
			}
			return result;
		}

		readonly ImmutableDictionary<ZString, MessageBuilderDetails> temporaryStorageATLASVersion10_1MessageBuilders = new Dictionary<ZString, MessageBuilderDetails>
		{
			{ ChangeCustodyInformation, new MessageBuilderDetails(typeof(TS.ATLASVersion10_1.CHGTSTMessageBuilder)) },
			{ ChangeOwnerReference, new MessageBuilderDetails(typeof(TS.ATLASVersion10_1.CHGSPOMessageBuilder)) },
			{ Split, new MessageBuilderDetails(typeof(TS.ATLASVersion10_1.CUSPCSMessageBuilder)) },
			{ ChangeDisposalEntitledTrader, new MessageBuilderDetails(typeof(TS.ATLASVersion10_1.CHGOFFMessageBuilder)) },
			{ Consolidation, new MessageBuilderDetails(typeof(TS.ATLASVersion10_1.PRLCONMessageBuilder)) },
			{ AmendmentSumA, new MessageBuilderDetails(typeof(TS.ATLASVersion10_1.CUSPRLMessageBuilder), (int)MessageDefinitions.ATLASVersion10_1.SCPRLKHeaderDeclarationType.VSM) },
			{ FinalSumAWithAPreliminary, new MessageBuilderDetails(typeof(TS.ATLASVersion10_1.CUSPRLMessageBuilder), (int)MessageDefinitions.ATLASVersion10_1.SCPRLKHeaderDeclarationType.ESA) },
			{ FinalSumAWithoutPreliminary, new MessageBuilderDetails(typeof(TS.ATLASVersion10_1.CUSPRLMessageBuilder), (int)MessageDefinitions.ATLASVersion10_1.SCPRLKHeaderDeclarationType.ESV) },
			{ PreliminarySumA, new MessageBuilderDetails(typeof(TS.ATLASVersion10_1.CUSPRLMessageBuilder), (int)MessageDefinitions.ATLASVersion10_1.SCPRLKHeaderDeclarationType.VSA) },
			{ ReExport, new MessageBuilderDetails(typeof(TS.ATLASVersion10_1.REXDISMessageBuilder)) }
		}.ToImmutableDictionary();

		readonly ImmutableDictionary<ZString, MessageBuilderDetails> temporaryStorageATLASVersion10_2MessageBuilders =
			ImmutableDictionary<ZString, MessageBuilderDetails>.Empty;

		struct MessageBuilderDetails
		{
			public MessageBuilderDetails(Type messageBuilderType, int sumAHeaderType)
			{
				MessageBuilderType = messageBuilderType;
				SumAHeaderType = sumAHeaderType;
			}

			public MessageBuilderDetails(Type messageBuilderType)
			{
				MessageBuilderType = messageBuilderType;
				SumAHeaderType = null;
			}

			public Type MessageBuilderType;

			public int? SumAHeaderType;
		}

		public const string ChangeDisposalEntitledTrader = "CHGOFF";
		public const string ChangeOwnerReference = "CHGSPO";
		public const string ChangeCustodyInformation = "CHGTST";
		public const string Consolidation = "PRLCON";
		public const string Split = "CUSPCS";
		public const string AmendmentSumA = "SUMVSM";
		public const string FinalSumAWithAPreliminary = "SUMESA";
		public const string FinalSumAWithoutPreliminary = "SUMESV";
		public const string PreliminarySumA = "SUMVSA";
		public const string ReExport = "REXDIS";
	}
}
