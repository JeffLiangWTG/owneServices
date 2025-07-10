using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.DE.Registry;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EmcsMessageBuilderLoader
	{
		EmcsMessageBuilderLoader() { }
		public static EmcsMessageBuilderLoader Instance => instance ?? (instance = new EmcsMessageBuilderLoader());
		[ThreadStatic]
		static EmcsMessageBuilderLoader instance;

		public IProduceMessageXml GetMessageBuilder(ZString messageCode, IEMCSMessageHeader dataProvider)
		{
			IProduceMessageXml result = null;

			var currentEmcsVersion = MessageVersionRegistry.CurrentEMCSVersion;

			if (currentEmcsVersion == DE.Business.EmcsVersionNumberList.Codes._24 && version2_4MessageBuilders.TryGetValue(messageCode, out var messageBuilderDetails2_4))
			{
				result = (IProduceMessageXml)Activator.CreateInstance(messageBuilderDetails2_4.MessageBuilderType, dataProvider);
			}
			else if (currentEmcsVersion == DE.Business.EmcsVersionNumberList.Codes._25 && version2_5MessageBuilders.TryGetValue(messageCode, out var messageBuilderDetails2_5))
			{
				result = (IProduceMessageXml)Activator.CreateInstance(messageBuilderDetails2_5.MessageBuilderType, dataProvider);
			}

			if (result == null)
			{
				ErrorReporter.ReportOnce(FormattableString.Invariant($"Invalid DE EMCS Message Builder for code: {messageCode} requested for EMCS Version {currentEmcsVersion}"));
			}
			return result;
		}

		readonly ImmutableDictionary<ZString, MessageBuilderDetails> version2_4MessageBuilders = new Dictionary<ZString, MessageBuilderDetails>
		{
			{ CancellationOfEAD, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED810MessageBuilder)) },
			{ ChangeOfDestination, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED813MessageBuilder)) },
			{ SubmittedDraftEAD, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED815MessageBuilder)) },
			{ ReportOfReceipt, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED818MessageBuilder)) },
			{ RejectionOfEAD, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED819MessageBuilder)) },
			{ ExplanationOnDelay, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED837MessageBuilder)) },
			{ ExplanationForShortage, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_4.ED871MessageBuilder)) },
		}.ToImmutableDictionary();

		readonly ImmutableDictionary<ZString, MessageBuilderDetails> version2_5MessageBuilders = new Dictionary<ZString, MessageBuilderDetails>
		{
			{ CancellationOfEAD, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED810MessageBuilder)) },
			{ ChangeOfDestination, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED813MessageBuilder)) },
			{ SubmittedDraftEAD, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED815MessageBuilder)) },
			{ ReportOfReceipt, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED818MessageBuilder)) },
			{ RejectionOfEAD, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED819MessageBuilder)) },
			{ ExplanationOnDelay, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED837MessageBuilder)) },
			{ ExplanationForShortage, new MessageBuilderDetails(typeof(CargoWise.Customs.DE.MessageContracts.EMCS.Version2_5.ED871MessageBuilder)) },
		}.ToImmutableDictionary();

		struct MessageBuilderDetails
		{
			public MessageBuilderDetails(Type messageBuilderType)
			{
				MessageBuilderType = messageBuilderType;
			}

			public Type MessageBuilderType;
		}

		public const string CancellationOfEAD = "ED810";
		public const string ChangeOfDestination = "ED813";
		public const string SubmittedDraftEAD = "ED815";
		public const string ReportOfReceipt = "ED818";
		public const string RejectionOfEAD = "ED819";
		public const string SubmittedDraftSplittingOperation = "ED825";
		public const string ExplanationOnDelay = "ED837";
		public const string ExplanationForShortage = "ED871";
	}
}

