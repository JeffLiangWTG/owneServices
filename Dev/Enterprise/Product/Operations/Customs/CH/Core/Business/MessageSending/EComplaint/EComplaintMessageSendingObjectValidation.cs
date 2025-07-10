using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class EComplaintMessageSendingObjectValidation : AutoEComplaintMessageSendingObjectValidation
{
	internal EComplaintMessageSendingObjectValidation(AutoEComplaintMessageSendingObject parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();
		ValidateSendingObjectLines();
	}

	new EComplaintMessageSendingObject Parent => (EComplaintMessageSendingObject)base.Parent;

	protected override void CheckCorrectionReason()
	{
		base.CheckCorrectionReason();
		MandatoryValidation.CheckEntered(Parent.CorrectionReasonInfo);
		ListValidation.ErrorIfInvalidCode(Parent.CorrectionReasonInfo);
	}

	public void ValidateSendingObjectLines()
	{
		if (!Parent.SendingObjectLines.Any())
		{
			Parent.AddRowError(Res.GetString("E297CAE9-28FF-488E-A54D-4DC5E91DAA7B", "At least one line is required."));
		}
	}
}
