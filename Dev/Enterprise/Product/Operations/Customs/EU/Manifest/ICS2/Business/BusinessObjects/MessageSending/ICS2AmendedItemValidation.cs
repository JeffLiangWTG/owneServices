using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2AmendedItemValidation : ZValidation
	{
		public ICS2AmendedItemValidation(ICS2AmendedItem parent)
			: base(parent)
		{
			Parent = parent;
		}

		ICS2AmendedItem Parent { get; }

		public void ValidateIsSelected()
		{
			ValidateCalculatedProperty(Parent.IsSelectedInfo);
		}

		protected void CheckIsSelected()
		{
			if (Parent.IsSelected)
			{
				var info = Parent.IsSelectedInfo;

				if (Parent.RequestHeader.IsAwaiting)
				{
					info.AddMessageError(Res.GetString("F7396513-BED6-40B0-B384-53162D30E2A5", "This Referral request IS waiting for a response."));
				}
				else if (Parent.RequestHeader.IsSubmited)
				{
					info.AddMessageError(Res.GetString("72D13F19-3219-4B6D-B7B9-0DEE1A116BF6", "This Referral request has already been submitted."));
				}

				if (Parent.Header.NeedReplyInformation && Parent.RequestHeader.RequestResponses.Count == 0)
				{
					info.AddWarning(Res.GetString("3877CF19-2141-4E89-BF0C-CF5E48D0530F", "This Referral request does not have any reply information entered."));
				}
			}
		}

		public override Type AutoValidationType => typeof(ICS2AmendedItemValidation);

		public override void ValidateAll()
		{
			ValidateIsSelected();
		}
	}
}
