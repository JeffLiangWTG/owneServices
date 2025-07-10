namespace Enterprise.Customs.JP.Business.Testing
{
	public class CusEntryInstructionValidationForTestSendingObject : CusEntryInstructionValidation
	{
		public CusEntryInstructionValidationForTestSendingObject(CusEntryInstructionForTestSendingObject parent) : base(parent)
		{
		}

		CusEntryInstructionForTestSendingObject EntryInstruction => (CusEntryInstructionForTestSendingObject)Parent;

		public override void ValidateAll()
		{
			CheckCEI_DateForDuty();
		}

		protected override void CheckCEI_DateForDuty()
		{
			var targetInfo = Parent.CEI_DateForDutyInfo;
			if (EntryInstruction.CreateMessageErrorForTest)
			{
				targetInfo.AddMessageError("Test message error on CEI_DateForDutyInfo.");
			}

			if (EntryInstruction.CreateWarningForTest)
			{
				targetInfo.AddWarning("Test warning on CEI_DateForDutyInfo.");
			}

			if (EntryInstruction.CreateErrorForTest)
			{
				targetInfo.AddError("Test error on CEI_DateForDutyInfo.");
			}
		}
	}
}
