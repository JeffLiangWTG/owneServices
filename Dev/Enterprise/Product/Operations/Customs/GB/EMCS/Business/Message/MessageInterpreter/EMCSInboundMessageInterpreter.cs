using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public abstract class EMCSInboundMessageInterpreter<TDataProvider>
	{
		protected readonly Enterprise.Messaging.Business.EDIMessage message;

		protected readonly TDataProvider provider;

		IReadOnlyCollection<string> advancedSummaries;

		IReadOnlyCollection<(string Key, string Value)> messageDetails;

		IReadOnlyCollection<(string Key, string Value)> sequencedMessageDetails;

		IReadOnlyCollection<string> descriptions;

		protected abstract string Summary { get; }

		IReadOnlyCollection<string> AdvancedSummaries => advancedSummaries ?? (advancedSummaries = GetAdvancedSummaries());

		IReadOnlyCollection<(string Key, string Value)> MessageDetails => messageDetails ?? (messageDetails = GetMessageDetails());

		IReadOnlyCollection<(string Key, string Value)> SequencedMessageDetails => sequencedMessageDetails ?? (sequencedMessageDetails = GetSequencedMessageDetails());

		IReadOnlyCollection<string> Descriptions => descriptions ?? (descriptions = GetDescriptions());

		protected CodeDescriptionPairList RevenueErrorsList => RefCusCodeListTypes.GetCachedList(message.Factory, (ZString)"GB", (ZString)"GBROS", ZDateTime.Today, null, "", false, "");

		protected EMCSInboundMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TDataProvider provider)
		{
			this.message = Argument.NotNull(message, "message");
			this.provider = Argument.NotNull(provider, "provider");
		}

		protected virtual IReadOnlyCollection<string> GetAdvancedSummaries()
		{
			return (IReadOnlyCollection<string>)(object)Array.Empty<string>();
		}

		protected abstract IReadOnlyCollection<(string Key, string Value)> GetMessageDetails();

		protected virtual IReadOnlyCollection<(string Key, string Value)> GetSequencedMessageDetails()
		{
			return (IReadOnlyCollection<(string Key, string Value)>)(object)Array.Empty<(string, string)>();
		}

		protected virtual IReadOnlyCollection<string> GetDescriptions()
		{
			return (IReadOnlyCollection<string>)(object)Array.Empty<string>();
		}

		public string GetInterpretation()
		{
			return GetInterpretationCore();
		}

		protected virtual string GetInterpretationCore()
		{
			ZStringBuilder zStringBuilder = new ZStringBuilder(Summary);
			zStringBuilder.AppendLine();
			bool flag = false;
			foreach (string advancedSummary in AdvancedSummaries)
			{
				flag = true;
				zStringBuilder.Append(advancedSummary);
			}

			if (flag)
			{
				zStringBuilder.AppendLine();
			}

			HtmlTableCreator htmlTableCreator = new HtmlTableCreator();
			foreach (var messageDetail in MessageDetails)
			{
				htmlTableCreator.WriteRow(messageDetail.Key, messageDetail.Value);
			}

			if (SequencedMessageDetails.Count > 0)
			{
				foreach (var sequencedMessageDetail in SequencedMessageDetails)
				{
					htmlTableCreator.WriteRow(sequencedMessageDetail.Key, sequencedMessageDetail.Value);
				}
			}

			zStringBuilder.Append(htmlTableCreator.ToHtml());
			bool flag2 = false;
			foreach (string description in Descriptions)
			{
				flag2 = true;
				zStringBuilder.Append(description);
			}

			if (flag2)
			{
				zStringBuilder.AppendLine();
			}

			return zStringBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}
	}
}
