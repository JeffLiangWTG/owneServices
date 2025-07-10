using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class LPCOJobDeclarationValidationTest : JobDeclarationValidationTest
	{
		public override void TestCheckJE_MessageTypeIsEnteredOrValid()
		{
			base.TestCheckJE_MessageTypeIsEnteredOrValid();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.Validation.ValidateJE_MessageType();
			AssertHasError(declaration.JE_MessageTypeInfo, "This Shipment Type can only be used on the LPCO module (Operate > Customs > LPCO).");

			declaration.FixedJobMessageType = BRJobMessageTypeList.Codes.LPCO;
			declaration.Validation.ValidateJE_MessageType();
			AssertNoError(declaration.JE_MessageTypeInfo, "This Shipment Type can only be used on the LPCO module (Operate > Customs > LPCO).");
		}

		protected override string JobMessageType => BRJobMessageTypeList.Codes.LPCO;
	}
}
