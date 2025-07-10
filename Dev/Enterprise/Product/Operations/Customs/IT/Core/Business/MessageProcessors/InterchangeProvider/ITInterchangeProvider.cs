using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public abstract class ITInterchangeProvider : InterchangeProviderBase
{
	protected ITInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
	{
	}

	protected sealed override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

	protected sealed override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
	{
		PopulateInterchangeLinkAndSetMessageStatus(messages, interchange);
	}

	/// <summary>
	/// Wraps SetInterchangeValuesForTransmit to give it a more meaningful name.
	/// SetInterchangeValuesForTransmit does more than its name conveys:
	/// - Populates interchange attributes
	/// - Adds message to interchange
	/// - Sets message status
	/// </summary>
	void PopulateInterchangeLinkAndSetMessageStatus(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
	{
		var message = GetOneAndOnlyMessageFromCollection(messages);
		var customsInterchangeAndAccountInfo = GetCustomsInterchangeAndAccountInfo(message);

		var messageType = message.EM_MessageType;
		var interchangeTo = ITCustomsDataRegistry.Instance.ITRecipientID.Value;
		var interchangeFrom = message.Company.GC_Code;

		if (customsInterchangeAndAccountInfo.ErrorCollector.IsEmpty)
		{
			SetInterchangeValuesForTransmit(interchange, messages, messageType, interchangeTo, interchangeFrom);
			SetAdditionalInterchangeValuesForTransmit(interchange);
			SetInterchangeHeaderText(interchange, customsInterchangeAndAccountInfo);
			SetInterchangeFileNameStrategy((ITEDIInterchange)interchange, message, customsInterchangeAndAccountInfo);
		}
		else
		{
			MarkMessageAndInterchangeAsFailed(interchange, message, messageType, interchangeTo, interchangeFrom, customsInterchangeAndAccountInfo.ErrorCollector);
		}
	}

	void MarkMessageAndInterchangeAsFailed(EDIInterchange interchange, EDIMessage message, ZString interchangeType, ZString interchangeTo, ZString interchangeFrom, ZStringBuilder errorCollector)
	{
		message.EM_Status = EDIMessageStatusList.Codes.Failed;
		interchange.ContainedMessages.Add(message);
		interchange.IsTransmitInterchange = true;
		interchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
		interchange.EI_InterchangeType = interchangeType;
		interchange.EI_From = interchangeFrom;
		interchange.EI_To = interchangeTo;
		var interchangeNote = interchange.Notes.AddNew();
		interchangeNote.ST_Description = (NoResString)"CargoWiseOne error";
		interchangeNote.ST_NoteDataAsText = errorCollector.ToStringWithNewLineBetweenAppends();
	}

	protected virtual void SetAdditionalInterchangeValuesForTransmit(EDIInterchange interchange) { }

	void SetInterchangeFileNameStrategy(ITEDIInterchange interchange, EDIMessage message, CustomsInterchangeAndAccountInfo customsInterchangeAndAccountInfo)
	{
		interchange.FileNameStrategy = GetInterchangeFileNameStrategy(message, customsInterchangeAndAccountInfo);
	}

	protected virtual IInterchangeFileNameStrategy GetInterchangeFileNameStrategy(EDIMessage message, CustomsInterchangeAndAccountInfo customsInterchangeAndAccountInfo) => null;

	void SetInterchangeHeaderText(EDIInterchange interchange, CustomsInterchangeAndAccountInfo customsInterchangeAndAccountInfo)
	{
		var fieldsProvider = new OutgoingInterchangeHeaderTextFieldsWrapper(interchange, customsInterchangeAndAccountInfo);
		var headerTextStrategy = GetInterchangeHeaderTextStrategy(fieldsProvider);
		interchange.EI_HeaderText = headerTextStrategy.GetText();
	}

	protected abstract IInterchangeHeaderTextStrategy GetInterchangeHeaderTextStrategy(IOutgoingInterchangeHeaderTextFieldsProvider fieldsProvider);

	CustomsInterchangeAndAccountInfo GetCustomsInterchangeAndAccountInfo(EDIMessage message)
	{
		var result = new CustomsInterchangeAndAccountInfo();
		var linkedObject = message.EM_LinkedObject;
		if (linkedObject == null)
		{
			result.ErrorCollector.Append(Res.GetString("C4C92A90-E6D4-4809-B75B-95563F1E9D47", "Message has no linked object."));
			return result;
		}

		if (!(linkedObject is ICustomsEntryApplicationReference applicationReference))
		{
			var unexpectedBusinessObjectErrorMessage = Res.GetString("DF3B3287-068A-4D2D-A329-6EA9120D606C", "Unexpected business object linked to the message: '{0}' does not implement '{1}'.", linkedObject.GetType().FullName, nameof(ICustomsEntryApplicationReference));
			ErrorReporter.ReportOnce(unexpectedBusinessObjectErrorMessage);
			result.ErrorCollector.Append(unexpectedBusinessObjectErrorMessage);
			return result;
		}

		var accountDetail = CustomsCredentialHelper.GetAccountDetailFromInternalCode(applicationReference.CustomsProfile);
		if (accountDetail == null)
		{
			result.ErrorCollector.Append(Res.GetString("0B2C249B-5EBC-4C7F-B9B9-8CBDA989878E", "Failed to find the registry account."));
			return result;
		}

		result.Staff = applicationReference.Subscriber;
		result.Header = GetCustomsInterchangeHeader(message, applicationReference, accountDetail);
		result.Account = accountDetail.Account;
		return result;
	}

	CustomsInterchangeHeader GetCustomsInterchangeHeader(EDIMessage message, ICustomsEntryApplicationReference applicationReference, AccountDetail accountDetail)
	{
		return CustomsInterchangeHeader.NewForEhubSending(
			applicationReference.Node,
			ITEDIInterchange.Constants.FileNamePlaceHolder,
			SADWrapperHelper.RemoveIsoCodeIfIsItalianCustomsOffice(applicationReference.CustomsOffice),
			accountDetail.DeclarantTaxNumber,
			accountDetail.WorkstationSequentialNumber,
			numberOfRecordsInTheFile: message.EM_MessageText.Trim().SplitByNewLine().Count() + 1);
	}

	/// <summary>
	/// Ensures message collection has one and only one element (collation key = do not collate).
	/// </summary>
	/// <returns>Single message from collection</returns>
	EDIMessage GetOneAndOnlyMessageFromCollection(NonDependentEDIMessageCollection messages) => messages.Cast<EDIMessage>().Single();

	/// <summary>
	/// One interchange is created for each message (DoNotCollateType)
	/// </summary>
	protected sealed override string GetCollationKey(EDIMessage message) => DoNotCollateType;

	protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange) => EDIInterchange.Status.eHubQueued;

	protected sealed override Type InterchangeType => typeof(ITEDIInterchange);

	protected sealed override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages) => ZString.Empty;
}
