using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.AESVersion3_0;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0;
using Enterprise.Customs.DE.Registry;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class ExitDeclarationMessageBuilderLoader
	{
		ExitDeclarationMessageBuilderLoader() { }
		public static ExitDeclarationMessageBuilderLoader Instance => instance ?? (instance = new ExitDeclarationMessageBuilderLoader());
		[ThreadStatic]
		static ExitDeclarationMessageBuilderLoader instance;

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

		public ZBool HasMessageBuildersForCurrentAESVersion
		{
			get
			{
				var currentAESVersion = MessageVersionRegistry.CurrentAESVersion;
				return (currentAESVersion == AESVersionNumberList.Codes._30 && aesVersion3_0MessageBuilders.Count > 0)
					|| (currentAESVersion == AESVersionNumberList.Codes._40 && aesVersion4_0MessageBuilders.Count > 0);
			}
		}

		readonly ImmutableDictionary<ZString, OutboundMessageDetails> aesVersion4_0MessageBuilders = new Dictionary<ZString, OutboundMessageDetails>().ToImmutableDictionary();

		readonly ImmutableDictionary<ZString, OutboundMessageDetails> aesVersion3_0MessageBuilders = new Dictionary<ZString, OutboundMessageDetails>
		{
			{ ExitAnticipation, new OutboundMessageDetails(typeof(EXTANTMessageBuilder), typeof(EXTANTMessageHeaderProvider)) },
			{ ExitPresentation, new OutboundMessageDetails(typeof(EXTPREMessageBuilder), typeof(EXTPREMessageHeaderProvider)) },
			{ ExitInformation, new OutboundMessageDetails(typeof(EXTINFMessageBuilder), typeof(EXTINFMessageHeaderProvider)) },
			{ ExitNotification, new OutboundMessageDetails(typeof(EXTNOTMessageBuilder), typeof(EXTNOTMessageHeaderProvider)) },
		}.ToImmutableDictionary();

		public const string ExitAnticipation = "EXTANT";
		public const string ExitPresentation = "EXTPRE";
		public const string ExitInformation = "EXTINF";
		public const string ExitNotification = "EXTNOT";
	}
}

