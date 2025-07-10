using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public sealed class NctsHeaderMessageSendingObjectValidation : EU.NCTS.Business.NctsHeaderMessageSendingObjectValidation
{
	public NctsHeaderMessageSendingObjectValidation(NctsHeaderMessageSendingObject parent) : base(parent)
	{
	}

	new NctsHeaderMessageSendingObject Parent => (NctsHeaderMessageSendingObject)base.Parent;

	protected override void ValidateAllCore()
	{
		base.ValidateAllCore();
		ValidateMessageSubType();
		ValidateReason();
		ValidateLegislativeReference();
	}

	#region MessageSubType

	public void ValidateMessageSubType()
	{
		ValidateCalculatedProperty(Parent.MessageSubTypeInfo);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
	void CheckMessageSubType()
	{
		var subTypeInfo = Parent.MessageSubTypeInfo;
		MandatoryValidation.CheckEntered(subTypeInfo);
		ListValidation.ErrorIfInvalidCode(subTypeInfo);
	}

	#endregion

	#region Reason

	public void ValidateReason()
	{
		ValidateCalculatedProperty(Parent.ReasonInfo);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
	void CheckReason()
	{
		var parent = Parent;
		if (parent.IsCancel || parent.IsAmend)
		{
			var reasonInfo = parent.ReasonInfo;
			MandatoryValidation.CheckEntered(reasonInfo);
			ListValidation.ErrorIfInvalidCode(reasonInfo);
		}
	}

	#endregion

	#region LegislativeReference

	public void ValidateLegislativeReference()
	{
		ValidateCalculatedProperty(Parent.LegislativeReferenceInfo);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
	void CheckLegislativeReference()
	{
		var parent = Parent;
		if (parent.IsCancel || parent.IsAmend)
		{
			var legislativeReferenceInfo = parent.LegislativeReferenceInfo;
			MandatoryValidation.CheckEntered(legislativeReferenceInfo);
			ListValidation.ErrorIfInvalidCode(legislativeReferenceInfo);
		}
	}

	#endregion
}
