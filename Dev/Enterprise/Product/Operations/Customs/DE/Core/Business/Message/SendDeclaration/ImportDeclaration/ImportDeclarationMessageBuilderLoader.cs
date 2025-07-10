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
	public sealed class ImportDeclarationMessageBuilderLoader
	{
		ImportDeclarationMessageBuilderLoader() { }

		public static ImportDeclarationMessageBuilderLoader Instance => instance ??= new ImportDeclarationMessageBuilderLoader();
		[ThreadStatic]
		static ImportDeclarationMessageBuilderLoader instance;

		public IProduceMessageXml GetMessageBuilder(ZString messageCode, IImportMessageHeader importHeader)
		{
			IProduceMessageXml result = null;
			var type = GetMessageBuilderTypeForCurrentVersion(messageCode);
			if (type != null)
			{
				result = (IProduceMessageXml)Activator.CreateInstance(type, importHeader);
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
					ErrorReporter.ReportOnce(FormattableString.Invariant($"Invalid DE Import Message Builder for code: {messageCode} requested for ATLAS Version {currentATLASVersion}"));
				}
			}
			return messageBuilderType;
		}

		readonly ImmutableDictionary<ZString, Type> atlasVersion10_1MessageBuilders = new Dictionary<ZString, Type>
		{
			{ SingleDeclarationFreeCirculation, typeof(Messaging.ATLASVersion10_1.CFCDECMessageBuilder) },
			{ ImportDeclarationConfirmation, typeof(Messaging.ATLASVersion10_1.CUSCONMessageBuilder) },
			{ SimplifiedDeclarationIntoBondedWarehouse, typeof(Messaging.ATLASVersion10_1.SCWRECMessageBuilder) },
			{ SingleDeclarationIntoBondedWarehouse, typeof(Messaging.ATLASVersion10_1.SCWDECMessageBuilder) },
			{ SingleDeclarationOutwardProcessing, typeof(Messaging.ATLASVersion10_1.SCIDECMessageBuilder) },
			{ SimplifiedDeclarationForInwardProcessing, typeof(Messaging.ATLASVersion10_1.SCIRECMessageBuilder) },
			{ SimplifiedDeclarationIntoFreeCirculation, typeof(Messaging.ATLASVersion10_1.CFCRECMessageBuilder) },
			{ WarehouseStockTransfer, typeof(Messaging.ATLASVersion10_1.CUSWATMessageBuilder) },
			{ CollectiveClearanceBondedWarehouse, typeof(Messaging.ATLASVersion10_1.ECWCCMMessageBuilder) }
		}.ToImmutableDictionary();

		readonly ImmutableDictionary<ZString, Type> atlasVersion10_2MessageBuilders =
			ImmutableDictionary<ZString, Type>.Empty;

		public const string SingleDeclarationFreeCirculation = "CFCDEC";
		public const string ImportDeclarationConfirmation = "CUSCON";
		public const string SimplifiedDeclarationIntoBondedWarehouse = "SCWREC";
		public const string SingleDeclarationIntoBondedWarehouse = "SCWDEC";
		public const string SingleDeclarationOutwardProcessing = "SCIDEC";
		public const string SimplifiedDeclarationForInwardProcessing = "SCIREC";
		public const string SimplifiedDeclarationIntoFreeCirculation = "CFCREC";
		public const string WarehouseStockTransfer = "CUSWAT";
		public const string CollectiveClearanceBondedWarehouse = "ECWCCM";
	}
}

