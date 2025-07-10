namespace Enterprise.Customs.JP.Business.Testing
{
	public class DeclarationValidationForTestSendingObject : JobDeclarationValidation
	{
		public DeclarationValidationForTestSendingObject(DeclarationForTestSendingObject parent) : base(parent)
		{
		}

		DeclarationForTestSendingObject Declaration => (DeclarationForTestSendingObject)Parent;

		public override void ValidateAll()
		{
			ValidateJE_TransportMode();
			ValidateJE_ReceiptMode();
		}

		protected override void CheckJE_TransportMode()
		{
			if (Declaration.CreateMessageErrorForTest)
			{
				Parent.JE_TransportModeInfo.AddMessageError("Test message error on JE_TransportModeInfo.");
			}

			if (Declaration.CreateWarningForTest)
			{
				Parent.JE_TransportModeInfo.AddWarning("Test warning on JE_TransportModeInfo.");
			}

			if (Declaration.CreateErrorForTest)
			{
				Parent.JE_TransportModeInfo.AddError("Test error on JE_TransportModeInfo.");
			}
		}

		protected override void CheckJE_ReceiptMode()
		{
			if (Declaration.IsECRSendingInProgress)
			{
				if (Declaration.CreateMessageErrorForTest)
				{
					Parent.JE_ReceiptModeInfo.AddMessageError("Test message error on JE_ReceiptModeInfo.");
				}

				if (Declaration.CreateWarningForTest)
				{
					Parent.JE_ReceiptModeInfo.AddWarning("Test warning on JE_ReceiptModeInfo.");
				}
			}
		}
	}
}
