using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DE.Registry;

namespace Enterprise.Customs.DE.Business
{
	public class ExportDeclarationMessageBuilderLoader
	{
		ExportDeclarationMessageBuilderLoader() { }
		public static ExportDeclarationMessageBuilderLoader Instance => instance ?? (instance = new ExportDeclarationMessageBuilderLoader());
		[ThreadStatic]
		static ExportDeclarationMessageBuilderLoader instance;

		public OutboundMessageDetails GetOutboundMessageDetailsForCurrentVersion(ZString messageCode)
		{
			OutboundMessageDetails outboundMessageDetails = null;
			var currentAESVersion = MessageVersionRegistry.CurrentAESVersion;
			var success = currentAESVersion == AESVersionNumberList.Codes._30 && aesVersion3_0MessageBuilders.TryGetValue(messageCode, out outboundMessageDetails);
			if (!success)
			{
				success = currentAESVersion == AESVersionNumberList.Codes._40 && aesVersion4_0MessageBuilders.TryGetValue(messageCode, out outboundMessageDetails);
				if (!success)
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($"Invalid DE AES Message Builder for code: {messageCode} requested for AES Version {currentAESVersion}"));
				}
			}
			return outboundMessageDetails;
		}

		readonly ImmutableDictionary<ZString, OutboundMessageDetails> aesVersion4_0MessageBuilders = new Dictionary<ZString, OutboundMessageDetails>().ToImmutableDictionary();

		readonly ImmutableDictionary<ZString, OutboundMessageDetails> aesVersion3_0MessageBuilders = new Dictionary<ZString, OutboundMessageDetails>
		{
			{ Amendment, new OutboundMessageDetails(typeof(Messaging.AESVersion3_0.EXPAMDMessageBuilder), typeof(AESVersion3_0.EXPAMDMessageHeaderProvider)) },
			{ CancellationRequest, new OutboundMessageDetails(typeof(CargoWise.Customs.DE.MessageContracts.AESVersion3_0.EXPINVMessageBuilder), typeof(AESVersion3_0.EXPINVMessageHeaderProvider)) },
			{ ExportData, new OutboundMessageDetails(typeof(Messaging.AESVersion3_0.EXPDATMessageBuilder), typeof(AESVersion3_0.EXPDATMessageHeaderProvider)) },
			{ EntireData, new OutboundMessageDetails(typeof(CargoWise.Customs.DE.MessageContracts.AESVersion3_0.EXPENTMessageBuilder), typeof(AESVersion3_0.EXPENTMessageHeaderProvider)) },
			{ ExportIndiction, new OutboundMessageDetails(typeof(string), typeof(string)) },
			{ ExportExit, new OutboundMessageDetails(typeof(CargoWise.Customs.DE.MessageContracts.AESVersion3_0.EXPEXTMessageBuilder), typeof(AESVersion3_0.EXPEXTMessageHeaderProvider)) },
			{ StatusRequest, new OutboundMessageDetails(typeof(CargoWise.Customs.DE.MessageContracts.AESVersion3_0.EXQQUEMessageBuilder), typeof(AESVersion3_0.EXQQUEProvider)) },
			{ DocumentData, new OutboundMessageDetails(typeof(string), typeof(string)) },
		}.ToImmutableDictionary();

		public const string Amendment = "EXPAMD";
		public const string CancellationRequest = "EXPCAN";
		public const string ExportData = "EXPDAT";
		public const string EntireData = "EXPENT";
		public const string ExportIndiction = "EXPIND";
		public const string ExportExit = "EXPEXT";
		public const string StatusRequest = "EXQQUE";
		public const string DocumentData = "DOCDAT";
	}
}

