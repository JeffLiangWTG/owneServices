using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsHeaderMessageSendingObjectValidation : EU.NCTS.Business.NctsHeaderMessageSendingObjectValidation
{
	public NctsHeaderMessageSendingObjectValidation(NctsHeaderMessageSendingObject parent) : base(parent)
	{
		this.parent = parent;
		zValidationInternals = this;
	}

	public override Type AutoValidationType => typeof(NctsHeaderMessageSendingObject);

	protected override void ValidateAllCore()
	{
		base.ValidateAllCore();
		ValidateReasonForCancellation();
	}

	public void ValidateReasonForCancellation()
	{
		zValidationInternals.Validate(parent.ReasonForCancellationInfo, CheckReasonForCancellation);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
	void CheckReasonForCancellation()
	{
		if (parent.MessageType == DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation && parent.ReasonForCancellation.IsEmpty)
		{
			parent.ReasonForCancellationInfo.AddError(Res.GetString("CBCC8F0A-263D-454F-8984-35D3AFC91178", "Please enter a cancellation reason."));
		}
	}

	readonly NctsHeaderMessageSendingObject parent;
	[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
	readonly IValidationInternals zValidationInternals;
}
