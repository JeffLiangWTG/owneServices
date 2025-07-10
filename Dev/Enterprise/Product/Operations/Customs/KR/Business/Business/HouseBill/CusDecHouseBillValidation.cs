using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class CusDecHouseBillValidation : AutoKRHouseBillValidation
	{
		public CusDecHouseBillValidation(Bill houseBill)
			: base(houseBill)
		{
		}

		public new Bill Bill => (Bill)base.Bill;

		protected new Bill Parent => (Bill)base.Parent;
		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateHBSplitDecReasonRemark();
		}

		protected override void CheckCU_HBSplitDecInd()
		{
			base.CheckCU_HBSplitDecInd();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CU_HBSplitDecIndInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.CU_HBSplitDecIndInfo);
		}

		protected override void CheckCU_HBSplitDecReasonCode()
		{
			base.CheckCU_HBSplitDecReasonCode();
			if (Parent.CU_HBSplitDecInd.ToUpper().Equals(HouseBillSplitDeclarationIndicatorCodeList.Codes.Y))
			{
				if (Parent.CU_HBSplitDecReasonCode.IsEmpty)
				{
					Parent.CU_HBSplitDecReasonCodeInfo.AddMessageError(Res.GetString("1D4F91D4-F0E1-4A98-8F09-0ED98F8BB4F5", "If House Bill Split Declaration Indicator is 'Y', you must enter House Bill Split Declaration Reason Code."));
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.CU_HBSplitDecReasonCodeInfo);
			}
			else if (!Parent.CU_HBSplitDecReasonCode.IsEmpty)
			{
				Parent.CU_HBSplitDecReasonCodeInfo.AddMessageError(Res.GetString("F718433B-D502-47F9-9415-59C069CC8D1B", "If House Bill Split Declaration Indicator is 'N', don't enter House Bill Split Declaration Reason Code."));
			}
		}

		public void ValidateHBSplitDecReasonRemark()
		{
			ValidateCalculatedProperty(Parent.HBSplitDecReasonRemarkInfo);
		}

		protected void CheckHBSplitDecReasonRemark()
		{
			if (Parent.CU_HBSplitDecReasonCode.ToUpper().Equals(HouseBillSplitDeclarationReasonCodeList.Codes.Z))
			{
				if (Parent.HBSplitDecReasonRemark.IsEmpty)
				{
					Parent.HBSplitDecReasonRemarkInfo.AddMessageError(Res.GetString("13EDB6B1-F60C-4B8A-AC39-01485BB77D49", "If House Bill Split Declaration Reason Code is 'Z', you must enter House Bill Split Declaration Reason Description."));
				}
			}
			else if (!Parent.HBSplitDecReasonRemark.IsEmpty)
			{
				Parent.HBSplitDecReasonRemarkInfo.AddMessageError(Res.GetString("91407FDA-A5D0-4E9D-8743-DA446B4A826F", "If House Bill Split Declaration Reason Code is  not 'Z', don't enter House Bill Split Declaration Reason Description."));
			}
		}
	}
}
