using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public class MessageChooserItemValidation : ASYCUDA.Business.MessageChooserItemValidation
{
	public MessageChooserItemValidation(MessageChooserItem parent) : base(parent)
	{
	}

	protected new MessageChooserItem Parent => (MessageChooserItem)base.Parent;

	protected override void ValidateAllCore()
	{
		base.ValidateAllCore();
		ValidateEntryType();
		ValidateSubjectCode();
		ValidateSubject();
	}

	public void ValidateEntryType()
	{
		ValidateCalculatedProperty(Parent.EntryTypeInfo);
	}

	protected void CheckEntryType()
	{
		var parent = Parent;

		MandatoryValidation.CheckEntered(parent.EntryTypeInfo);
		ListValidation.MessageErrorIfInvalidCode(parent.EntryTypeInfo);

		var bill = parent.Bill;
		var messages = bill.Messages.Cast<AEEDIMessage>();
		switch (parent.EntryType)
		{
			case EntryTypes.Codes.Original:
				if (SubmissionExist())
				{
					parent.EntryTypeInfo.AddMessageError(Res.GetString("92C95C00-3F8B-4717-9F01-47AEA58EBBC0", "This manifest has been submitted before."));
				}
				break;
			case EntryTypes.Codes.Change when bill.ABL_MessageStatus != AEConstants.Messaging.MessageTypes.CUSRES || bill.ABL_BillStatus != AEManifestConstants.CustomsStatus.RFI:
			case EntryTypes.Codes.Cancellation when bill.ABL_BillStatus != AEManifestConstants.CustomsStatus.ACT:
				if (!SubmissionAccepted())
				{
					parent.EntryTypeInfo.AddMessageError(Res.GetString("D50BE96C-FE90-474C-A996-3660AAE33782", "Amendment/Cancellation cannot be done on an unaccepted entry."));
				}
				break;
			default:
				break;
		}

		bool SubmissionExist() => messages.Any(m => m.EM_MessageType == AEConstants.Messaging.MessageTypes.CUSCAR && m.EM_Status == EDIMessage.Status.Sent);

		bool SubmissionAccepted() => messages.Any(m => m.EM_MessageType == AEConstants.Messaging.MessageTypes.CUSCAR && m.EM_Status == EDIMessage.Status.Acknowledged);
	}

	public void ValidateSubjectCode()
	{
		ValidateCalculatedProperty(Parent.SubjectCodeInfo);
	}

	protected void CheckSubjectCode()
	{
		var targetInfo = Parent.SubjectCodeInfo;
		ListValidation.MessageErrorIfInvalidCode(targetInfo);
		CheckIsEmptyForOriginalMessage(targetInfo);
		CheckSubjectCodeIsValidForEntryType(targetInfo);
	}

	void CheckIsEmptyForOriginalMessage(ZPropertyInfo targetInfo)
	{
		if (Parent.EntryType == EntryTypes.Codes.Original && !targetInfo.Value.IsEmpty)
		{
			targetInfo.AddMessageError(Res.GetString("3F6DB24D-4FC8-454F-B910-4E7C05757248"
				, "{0} must not be captured for Original Manifest Messages"
				, targetInfo.HumanReadableName));
		}
	}

	void CheckSubjectCodeIsValidForEntryType(ZPropertyInfo targetInfo)
	{
		var parent = Parent;
		var cancellationSubjectCode = SubjectCodes.Codes.ProvideJustificationForCanceling;
		if (parent.EntryType == EntryTypes.Codes.Cancellation
			&& parent.SubjectCode != cancellationSubjectCode)
		{
			targetInfo.AddMessageError(Res.GetString("9DACD8AA-B8FC-4B64-A29C-650CDA214A07"
				, "Subject Code must be {0} for Cancellation Manifest Messages"
				, cancellationSubjectCode));
		}

		if (parent.EntryType == EntryTypes.Codes.Change)
		{
			if (parent.SubjectCode.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("C868E727-5241-4B93-8C7C-2953E057CA3B"
					, "Subject Code must be selected for Change Manifest Messages"));
			}
			else if (parent.SubjectCode == cancellationSubjectCode)
			{
				targetInfo.AddMessageError(Res.GetString("F0BB5E5B-B784-4B53-9521-3B50E1326F84"
					, "Subject Code cannot be {0} for Change Manifest Messages"
					, cancellationSubjectCode));
			}
		}
	}

	public void ValidateSubject()
	{
		ValidateCalculatedProperty(Parent.SubjectInfo);
	}

	protected void CheckSubject()
	{
		CheckIsEmptyForOriginalMessage(Parent.SubjectInfo);
		CheckSubjectIsNotEmptyForEntryType();
	}

	void CheckSubjectIsNotEmptyForEntryType()
	{
		var entryTypesNeedingSubject = new List<ZString> { EntryTypes.Codes.Change, EntryTypes.Codes.Cancellation };
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(Parent.SubjectInfo,
			Parent.EntryTypeInfo, entryTypesNeedingSubject.Cast<IZType>(), GetSubjectNotEnteredMessage());

		string GetSubjectNotEnteredMessage()
		{
			var entryTypeDescription = Parent.Lookups.EntryTypeList.GetDescriptionFromCode(Parent.EntryType);
			return Res.GetString("09911D54-32ED-4E96-AFBB-9A3833D7B7EE"
				, $"A Subject/Reason must be supplied for {0} Manifest Messages"
				, entryTypeDescription);
		}
	}
}
