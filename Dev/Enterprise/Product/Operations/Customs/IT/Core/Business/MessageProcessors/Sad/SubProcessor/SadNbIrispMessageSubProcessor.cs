using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging;
using Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public class SadNbIrispMessageSubProcessor : SadMessageSubProcessorBase
{
	public SadNbIrispMessageSubProcessor(ISadCustomsLinkedObjectAdapter entryAdapter, EDIInterchange sentInterchange) : base(entryAdapter)
	{
		this.sentInterchange = Argument.NotNull(sentInterchange, nameof(sentInterchange));
	}
	readonly EDIInterchange sentInterchange;

	public void PerformActionsForMessageResponses(IEnumerable<UnifiedDeclarationResponseMessage> nbResponseMessages)
	{
		if (nbResponseMessages != null && nbResponseMessages.Any())
		{
			var idocBodyText = sentInterchange.EI_BodyText;
			var progressiveNumberWithEntryLineNumberDictionary = GetProgressiveMessageNumberAndEntryLineNumberDictionaryFromIdocText(idocBodyText);

			foreach (var responseMessage in nbResponseMessages)
			{
				var entryLine = GetEntryLine(progressiveNumberWithEntryLineNumberDictionary, EntryAdapter.CustomsLines, responseMessage);

				SetStatusToEntryLine(responseMessage, entryLine);
			}
		}
	}

	void SetStatusToEntryLine(UnifiedDeclarationResponseMessage responseMessage, ISadCustomsLineLinkedObjectAdapter customsLine)
	{
		var newEntryLineCustomsStatus = responseMessage is SadNbPositiveResponseMessage ? EntryLineCustomsStatusList.Codes.Approved : EntryLineCustomsStatusList.Codes.Rejected;
		if (!IsChangeStatusAllowed(customsLine.NBStatus, newEntryLineCustomsStatus))
		{
			throw new IncomingMessageDoesNotMatchWithStatusException(GetInvalidAttemptToChangeStatusExceptionMessage());
		}

		customsLine.SetNBStatus(newEntryLineCustomsStatus);
	}

	ISadCustomsLineLinkedObjectAdapter GetEntryLine(Dictionary<(ZString, ZInt), ZInt> progressiveNumberWithEntryLineNumberDictionary, IEnumerable<ISadCustomsLineLinkedObjectAdapter> mergedLines, UnifiedDeclarationResponseMessage responseMessage)
	{
		var panMessageProgressiveNumberKey = (responseMessage.DeclarationNumber, responseMessage.MessageProgressiveNumber);
		if (!progressiveNumberWithEntryLineNumberDictionary.ContainsKey(panMessageProgressiveNumberKey))
		{
			throw new IncomingMessageDoesNotMatchWithIdocException(FormattableString.Invariant($"The sent idoc does not contain a NB Message with Pan: {panMessageProgressiveNumberKey.DeclarationNumber}, Progressive number: {panMessageProgressiveNumberKey.MessageProgressiveNumber}"));
		}

		var entryLineNo = progressiveNumberWithEntryLineNumberDictionary[panMessageProgressiveNumberKey];
		return mergedLines.FirstOrDefault(x => x.LineNo == entryLineNo)
			?? throw new CouldNotFindRelatedBusinessObjectException(FormattableString.Invariant($"Entry: {EntryAdapter.EntryReferenceNumber} has not an EntryLine with LineNumber: {entryLineNo}"));
	}

	#region Implementation

	Dictionary<(ZString, ZInt), ZInt> GetProgressiveMessageNumberAndEntryLineNumberDictionaryFromIdocText(ZString idocBodyText)
	{
		try
		{
			if (idocBodyText.IsEmpty)
			{
				throw new UnableToInterpretCustomsMessageException("Sent Idoc is Empty!");
			}

			var panProgressiveNumberWithEntryLineDictionary = new Dictionary<(ZString, ZInt), ZInt>();
			var nbHeaderFixedPartWithFieldCollection = LoadFixedPartWithFieldsFromIdocText(idocBodyText).Where(x => x.FixedPart.RecordType == IdocRecordType.SadNbHeader);
			foreach (var (fixedPart, fields) in nbHeaderFixedPartWithFieldCollection)
			{
				if (fields.Length < 5)
				{
					throw new UnableToInterpretCustomsMessageException("Sent Idoc must have at least 5 fields with Tab Separator");
				}

				var entryLineNo = ZInt.Parse(fields.ElementAtOrDefault(4));
				var panProgressiveNumberKey = (fixedPart.AnnualProgressiveNumber, fixedPart.ProgressiveNumber);
				if (panProgressiveNumberWithEntryLineDictionary.ContainsKey(panProgressiveNumberKey))
				{
					throw new UnableToInterpretCustomsMessageException(FormattableString.Invariant($"Sent Idoc has two or more messages with the same Pan: {panProgressiveNumberKey.AnnualProgressiveNumber}, Progression number: {panProgressiveNumberKey.ProgressiveNumber}"));
				}
				panProgressiveNumberWithEntryLineDictionary.Add(panProgressiveNumberKey, entryLineNo);
			}
			return panProgressiveNumberWithEntryLineDictionary;
		}
		catch (UnableToInterpretCustomsMessageException unableToInterpretMessageExc)
		{
			throw new UnableToInterpretInterchangeException("Unable to interpret the sent Idoc interchange", unableToInterpretMessageExc);
		}
	}

	protected override IEnumerable<CustomsStatusOrder> LoadCustomsStatusWithInformationOrderCollection()
	{
		yield return new CustomsStatusOrder(Empty, 0);
		yield return new CustomsStatusOrder(EntryLineCustomsStatusList.Codes.Sent, 1);
		yield return new CustomsStatusOrder(EntryLineCustomsStatusList.Codes.Rejected, 2);
		yield return new CustomsStatusOrder(EntryLineCustomsStatusList.Codes.Approved, 3);
	}

	IEnumerable<(SadFixedPartReader FixedPart, ZString[] Fields)> LoadFixedPartWithFieldsFromIdocText(ZString idocBodyText)
	{
		var rows = idocBodyText.SplitByNewLine(StringSplitOptions.RemoveEmptyEntries);
		foreach (var row in rows)
		{
			var fixedPart = SadFixedPartReader.LoadFromRow(row);
			var fields = row.RemoveSafe(0, 23).Split(new string[] { "\t" });

			yield return (FixedPart: fixedPart, Fields: fields);
		}
	}

	#endregion
}
