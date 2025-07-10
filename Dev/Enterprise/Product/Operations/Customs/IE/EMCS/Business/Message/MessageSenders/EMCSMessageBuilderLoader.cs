using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using MessageBuilderPhase4_1 = CargoWise.Customs.IE.MessageContracts.EMCSPhase4_1;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSMessageBuilderLoader
	{
		EMCSMessageBuilderLoader() { }
		public static EMCSMessageBuilderLoader Instance => instance ?? (instance = new EMCSMessageBuilderLoader());
		[ThreadStatic]
		static EMCSMessageBuilderLoader instance;

		public IXmlMessageBuilder GetMessageBuilder(ZString messageCode, IEMCSMessageHeader dataProvider)
		{
			IXmlMessageBuilder result = null;

			var messageBuildersDictionary = messageBuildersPhase4_1;
			if (messageBuildersDictionary.TryGetValue(messageCode, out var messageBuilderDetails))
			{
				result = (IXmlMessageBuilder)Activator.CreateInstance(messageBuilderDetails.MessageBuilderType, dataProvider);
			}

			if (result == null)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"Invalid IE EMCS Message Builder for code: {messageCode}"));
			}
			return result;
		}

		readonly ImmutableDictionary<ZString, MessageBuilderDetails> messageBuildersPhase4_1 = new Dictionary<ZString, MessageBuilderDetails>
		{
			{ CancellationOfEAD, new MessageBuilderDetails(typeof(MessageBuilderPhase4_1.IE810MessageBuilder)) },
			{ ChangeOfDestination, new MessageBuilderDetails(typeof(MessageBuilderPhase4_1.IE813MessageBuilder)) },
			{ SubmitDraftEAD, new MessageBuilderDetails(typeof(MessageBuilderPhase4_1.IE815MessageBuilder)) },
			{ ReportOfReceipt, new MessageBuilderDetails(typeof(MessageBuilderPhase4_1.IE818MessageBuilder)) },
			{ RejectionOfEAD, new MessageBuilderDetails(typeof(MessageBuilderPhase4_1.IE819MessageBuilder)) },
			{ ExplanationOnDelayForDelivery, new MessageBuilderDetails(typeof(MessageBuilderPhase4_1.IE837MessageBuilder)) },
			{ ExplanationOnReasonForShortage, new MessageBuilderDetails(typeof(MessageBuilderPhase4_1.IE871MessageBuilder)) },
		}.ToImmutableDictionary();

		struct MessageBuilderDetails
		{
			public MessageBuilderDetails(Type messageBuilderType)
			{
				MessageBuilderType = messageBuilderType;
			}

			public Type MessageBuilderType;
		}

		public const string CancellationOfEAD = "IE810";
		public const string ChangeOfDestination = "IE813";
		public const string SubmitDraftEAD = "IE815";
		public const string ReportOfReceipt = "IE818";
		public const string RejectionOfEAD = "IE819";
		public const string ExplanationOnDelayForDelivery = "IE837";
		public const string ExplanationOnReasonForShortage = "IE871";
	}
}
