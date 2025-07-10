using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.Business.EInvoicing.KoreaSouth
{
	public class KoreaSouthEInvoicingDataElementProvider : NonPersistentBusinessObject
	{
		public KoreaSouthEInvoicingDataElementProvider(TransactionInfo transactionInfo, AdditionalInfo additionalInfo)
		{
			TransactionInfo = Argument.NotNull(transactionInfo, nameof(transactionInfo));
			AdditionalInfo = Argument.NotNull(additionalInfo, nameof(additionalInfo));
		}

		public ZString InvoiceHeaderDescription => TransactionInfo.Description ?? ZString.Empty;

		public ZString ForeignerRegistrationNumber => AdditionalInfo.InvoiceeAlienRegistrationNo;

		public ZString PassportNumber => AdditionalInfo.InvoiceePassportNo;

		public ZString AmendStatusCodeDescriptionInKorean => EInvoicingKoreaSouthConstants.AmendStatusList.GetDescriptionFromCode(AmendStatusCode);

		public ZString OriginalApprovalNumber => AdditionalInfo.OriginalIssueID;

		public ZString OriginalApprovalDate => (ZString)DateTime.ParseExact(AdditionalInfo.OriginalIssueID.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

		public ZString OriginalApprovalDateForCode020304 => new List<string> { "02", "03", "04" }.Contains(AmendStatusCode) ? OriginalApprovalDate : ZString.Empty;

		public ZString Evaluate(string text)
		{
			var matchedSections = Regex.Matches(text, @"\[([^\[\]]*)\]");
			var sections = new List<Section>();

			foreach (Match match in matchedSections)
			{
				var section = new Section();

				section.FullText = match.Groups[1].Value;
				section.FullTextAfterReplaceMarco = TextMacroProcessor.Replace(section.FullText, new[] { this });

				section.HasMarco = Regex.IsMatch(section.FullText, "<([^<>]*)>");
				section.IsMarcoAllEmpty = section.FullTextAfterReplaceMarco == Regex.Replace(section.FullText, "<[^<>]*>", "");

				sections.Add(section);
			}

			RemoveSection(sections);
			return FormatSection(sections);
		}

		void RemoveSection(List<Section> sections)
		{
			var linkedList = new LinkedList<Section>(sections);

			sections.RemoveAll(x =>
			{
				var currentNode = linkedList.Find(x);
				var previousSectionWithMarco = SearchPreviousNode(currentNode, (node) => node.Value.HasMarco);
				return !currentNode.Value.HasMarco && previousSectionWithMarco != null && previousSectionWithMarco.Value.IsMarcoAllEmpty;
			});

			sections.RemoveAll(x => x.HasMarco && x.IsMarcoAllEmpty);

			var sectionsToRemove = new List<Section>();
			for (var i = sections.Count - 1; i >= 0; i--)
			{
				var currentNode = linkedList.Find(sections[i]);
				if (currentNode.Value.HasMarco)
				{
					break;
				}
				else if (!currentNode.Value.HasMarco)
				{
					var previousSectionWithMarco = SearchPreviousNode(currentNode, (node) => node.Value.HasMarco);
					if (previousSectionWithMarco != null)
					{
						sectionsToRemove.Add(currentNode.Value);
					}
				}
			}
			sections.RemoveAll(x => sectionsToRemove.Contains(x));
		}

		ZString FormatSection(List<Section> sections)
		{
			var result = string.Join("", sections.Select(x => x.HasMarco ? x.FullTextAfterReplaceMarco : x.FullText));
			return result.Replace(typeof(KoreaSouthEInvoicingDataElementProvider).FullName, "");
		}

		LinkedListNode<Section> SearchPreviousNode(LinkedListNode<Section> node, Func<LinkedListNode<Section>, bool> condition)
		{
			if (node.Previous == null || condition(node.Previous))
			{
				return node.Previous;
			}

			return SearchPreviousNode(node.Previous, condition);
		}

		ZString AmendStatusCode => AdditionalInfo.AmendStatusCode;

		readonly TransactionInfo TransactionInfo;
		readonly AdditionalInfo AdditionalInfo;

		readonly ITextMacroProcessor TextMacroProcessor = ObjectFactory.Get<ITextMacroProcessor>();
	}

	class Section
	{
		internal bool HasMarco;
		internal bool IsMarcoAllEmpty;

		internal ZString FullText;
		internal ZString FullTextAfterReplaceMarco;
	}
}
