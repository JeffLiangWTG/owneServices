using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;
using static Enterprise.Customs.JP.Common.JPMessageActionList;

namespace Enterprise.Customs.JP.Business
{
	public partial class CusEntryInstructionValidation
	{
		protected override void CheckCEI_PreviousBillNumber()
		{
			base.CheckCEI_PreviousBillNumber();
			var parent = CusEntryInstruction;
			var info = parent.CEI_PreviousBillNumberInfo;

			if (RCRActionList.Codes.Nine.Equals(parent.CEI_RCRAction))
			{
				if (parent.CEI_PreviousBillNumber.IsEmpty)
				{
					ValidationHelper.AddMessageErrorIfNotEnteredForSpecificProcedureCodeAndAction(NotificationType.MessageError, JPProcedureCodeList.Codes.RCR, RCRActionList.Codes.Nine, RCRActionList.Descriptions.Nine, info);
				}
			}
		}

		protected override void CheckCEI_RCRAction()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_RCRActionInfo, CusEntryInstruction.Lookups.RCRActionList);
		}

		protected override void CheckCEI_ViaLocation()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CEI_ViaLocationInfo, CusEntryInstruction.Lookups.ViaList);
		}

		public void ValidateRCRExportControlNumber(ZPropertyInfo info)
		{
			if (RCRActionList.Codes.One.Equals(CusEntryInstruction.CEI_RCRAction) && info.Value.IsEmpty)
			{
				ValidationHelper.AddMessageErrorIfNotEnteredForSpecificProcedureCodeAndAction(NotificationType.MessageError, JPProcedureCodeList.Codes.RCR, RCRActionList.Codes.One, RCRActionList.Descriptions.One, info);
			}
		}

		public void CheckReceiptMode()
		{
			var parent = CusEntryInstruction;
			var info = parent.ReceiptModeInfo;
			info.AddAllNotificationsFrom(CusEntryInstruction.JobDeclaration.JE_ReceiptModeInfo);
			if (RCRActionList.Codes.Nine.Equals(parent.CEI_RCRAction) && info.Value.IsEmpty)
			{
				ValidationHelper.AddMessageErrorIfNotEnteredForSpecificProcedureCodeAndAction(NotificationType.MessageError, JPProcedureCodeList.Codes.RCR, RCRActionList.Codes.Nine, RCRActionList.Descriptions.Nine, info);
			}
		}

		public void CheckFinalDestination()
		{
			var parent = CusEntryInstruction;
			var info = parent.FinalDestinationInfo;
			info.AddAllNotificationsFrom(CusEntryInstruction.JobDeclaration.JE_RL_NKFinalDestinationInfo);
			if (RCRActionList.Codes.Nine.Equals(parent.CEI_RCRAction) && info.Value.IsEmpty)
			{
				ValidationHelper.AddMessageErrorIfNotEnteredForSpecificProcedureCodeAndAction(NotificationType.MessageError, JPProcedureCodeList.Codes.RCR, RCRActionList.Codes.Nine, RCRActionList.Descriptions.Nine, info);
			}
		}
	}
}
