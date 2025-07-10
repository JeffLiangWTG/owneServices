using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class TemporaryLandingInfoValidation : CusSupportingInfoValidation
	{
		public TemporaryLandingInfoValidation(TemporaryLandingInfo parent) : base(parent)
		{
		}

		public new TemporaryLandingInfo Parent => (TemporaryLandingInfo)base.Parent;

		AsycudaBill Bill => Parent.Parent;

		bool IsNVC01 => Bill.IsNVC;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			if (IsNVC01)
			{
				var targetInfo = Parent.CSI_CodeInfo;
				ListValidation.MessageErrorIfInvalidCode(targetInfo);

				if (Bill.IsTemporaryLanding)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}

		protected override void CheckCSI_DateOfExpiry()
		{
			base.CheckCSI_DateOfExpiry();
			if (IsNVC01)
			{
				var parent = Parent;
				var startDate = parent.CSI_DateOfIssue;
				var endDate = parent.CSI_DateOfExpiry;
				if (startDate.IsValid && endDate.IsValid && startDate > endDate)
				{
					parent.CSI_DateOfExpiryInfo.AddMessageError(Res.GetString("61FB064E-7098-476E-8749-5F02364C1AA0", "Temporary Landing End Date must be later then Temporary Landing Start Date."));
				}
			}
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();
			if (IsNVC01 && Bill.IsTemporaryLanding)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ItemNumberInfo);
			}
		}

		protected override void CheckCSI_DateOfIssue()
		{
			base.CheckCSI_DateOfIssue();
			var parent = Parent;
			var bill = Bill;
			if (IsNVC01 && bill.IsTemporaryLandingTransportation)
			{
				var targetInfo = parent.CSI_DateOfIssueInfo;
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				var temporaryLandingStartDate = bill.TemporaryLandingStartDate;

				if (temporaryLandingStartDate.IsValid && (!(bill.TemporaryLandingStatus == TemporaryLandingStatusCodeList.Codes.CAN || bill.ABL_BillStatus == JPCustomsStatusList.Codes.REG) || bill.Header.IsNVC01SendingInProgress) && temporaryLandingStartDate < ZDate.Today)
				{
					targetInfo.AddMessageError(Res.GetString("9552D759-AD30-4ACE-8A36-59CB791E1DBA", "Start date for Temporary landing must be today or a future date."));
				}
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (IsNVC01)
			{
				var targetInfo = Parent.CSI_ReferenceNumberInfo;
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
				if (Bill.IsTemporaryLandingTransportation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}
	}
}
