using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business
{
	public abstract class BaseInboundMessageInterpreter<TDataProvider> : BaseMessageInterpreter<TDataProvider>
	{
		protected BaseInboundMessageInterpreter(BaseEDIMessage message, TDataProvider provider) : base(message, provider)
		{
		}

		protected abstract string Summary { get; }

		protected virtual IEnumerable<string> GetAdvancedSummaries() => Enumerable.Empty<string>();

		protected virtual string MessageDetailsSummary { get; }

		protected abstract IEnumerable<(string Key, string Value)> GetMessageDetails();

		protected virtual IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails() => Enumerable.Empty<(string summary, IEnumerable<(string key, string value)>)>();

		protected virtual IEnumerable<(string summary, IEnumerable<string[]>)> GetAdditionalMessageWithFreeNumberOfColumns() => Enumerable.Empty<(string summary, IEnumerable<string[]>)>();

		protected virtual IEnumerable<string> GetDescriptions() => Enumerable.Empty<string>();

		protected CodeDescriptionPairList RevenueErrorsList =>
			RefCusCodeListTypes.GetCachedList(
				factory,
				Core.Constants.CountryCodes.Ireland,
				Constants.RefCusCodeListTypes.RevenueErrorType,
				MessageCreatedDate,
				includeParentDataGrouping: false
			);

		public string GetInterpretation() => GetInterpretationCore();

		protected virtual string GetInterpretationCore()
		{
			var htmlBuilder = new ZStringBuilder(Summary);
			htmlBuilder.AppendLine();
			var hasAdvancedSummary = false;
			foreach (var summary in GetAdvancedSummaries())
			{
				hasAdvancedSummary = true;
				htmlBuilder.Append(summary);
			}
			if (hasAdvancedSummary)
			{
				htmlBuilder.AppendLine();
			}

			var tableCreator = new HtmlTableCreator();
			if (!string.IsNullOrEmpty(MessageDetailsSummary))
			{
				tableCreator.WriteRowWithFormatting(new CellWithFormatting(MessageDetailsSummary, Colspan(2)));
			}

			foreach (var messageDetail in GetMessageDetails())
			{
				tableCreator.WriteRow(messageDetail.Key, messageDetail.Value);
			}

			htmlBuilder.Append(tableCreator.ToHtml());

			foreach (var (summary, additionalDetails) in GetAdditionalMessageDetails())
			{
				var additionalTableCreator = new HtmlTableCreator();
				if (!string.IsNullOrEmpty(summary))
				{
					htmlBuilder.AppendLine();
					htmlBuilder.Append(summary);
					htmlBuilder.AppendLine();
				}
				foreach (var additionalDetail in additionalDetails)
				{
					additionalTableCreator.WriteRow(additionalDetail.key, additionalDetail.value);
				}
				htmlBuilder.Append(additionalTableCreator.ToHtml());
			}

			foreach (var (summary, additionalDetails) in GetAdditionalMessageWithFreeNumberOfColumns())
			{
				var additionalTableCreator = new HtmlTableCreator();
				if (!string.IsNullOrEmpty(summary))
				{
					htmlBuilder.AppendLine();
					htmlBuilder.Append(summary);
					htmlBuilder.AppendLine();
				}
				foreach (var additionalDetail in additionalDetails)
				{
					additionalTableCreator.WriteRow(additionalDetail);
				}
				htmlBuilder.Append(additionalTableCreator.ToHtml());
			}

			var hasDescription = false;
			foreach (var description in GetDescriptions())
			{
				hasDescription = true;
				htmlBuilder.Append(description);
			}
			if (hasDescription)
			{
				htmlBuilder.AppendLine();
			}

			return htmlBuilder.ToStringWithDelimiterBetweenAppends(HtmlResponseEmailGenerator.HtmlConstants.Br);
		}

		NameValueCollection Colspan(int span)
		{
			return TableInterpretation.Attributes.GetColspanAttribute(span);
		}
	}
}
