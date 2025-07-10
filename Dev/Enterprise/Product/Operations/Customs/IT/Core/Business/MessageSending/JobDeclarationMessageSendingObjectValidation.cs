using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business;

public class JobDeclarationMessageSendingObjectValidation : Customs.Business.JobDeclarationMessageSendingObjectValidation
{
	public JobDeclarationMessageSendingObjectValidation(AutoJobDeclarationMessageSendingObject parent) : base(parent)
	{
	}

	public new JobDeclarationMessageSendingObject Parent => (JobDeclarationMessageSendingObject)base.Parent;

	protected override void ValidateAllCore()
	{
		base.ValidateAllCore();
		ValidateCancellationAndAmendmentLegislativeReference();
	}

	protected override void CheckShouldSend()
	{
		base.CheckShouldSend();

		var sendingObject = Parent;
		var shouldSendInfo = sendingObject.ShouldSendInfo;
		CheckStatusAllowsSending(sendingObject, shouldSendInfo);
		CheckEntryHasAnAssociatedEntryInstruction(sendingObject, shouldSendInfo);
	}

	public void ValidateCancellationAndAmendmentLegislativeReference()
	{
		((IValidationInternals)this).Validate(Parent.CancellationAndAmendmentLegislativeReferenceInfo, () => CheckCancellationAndAmendmentLegislativeReference());
	}

	protected virtual void CheckCancellationAndAmendmentLegislativeReference()
	{
	}

	#region Implementation

	static void CheckEntryHasAnAssociatedEntryInstruction(JobDeclarationMessageSendingObject sendingObject, ZPropertyInfo shouldSendInfo)
	{
		if (sendingObject.ShouldSend && sendingObject.Header.EntryInstruction == null)
		{
			shouldSendInfo.AddError(ValidationCaptions.MessageSendingObject.SelectedEntryDoesNotHaveAnAssociatedEntryInstruction);
		}
	}

	protected virtual void CheckStatusAllowsSending(JobDeclarationMessageSendingObject sendingObject, ZPropertyInfo shouldSendInfo)
	{
		if (!sendingObject.StatusAllowsSending)
		{
			shouldSendInfo.AddWarning(ValidationCaptions.MessageSendingObject.NonSendableStatusWarning(sendingObject.Header.CH_BGMReference));
		}
	}

	#endregion
}
