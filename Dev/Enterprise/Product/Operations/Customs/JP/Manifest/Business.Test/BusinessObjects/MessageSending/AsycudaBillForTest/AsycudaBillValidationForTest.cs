using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	public class AsycudaBillValidationForTest : AsycudaBillValidation
	{
		public AsycudaBillValidationForTest(AsycudaBillForTest parent) : base(parent)
		{
		}

		AsycudaBillForTest Bill => (AsycudaBillForTest)Parent;

		public override void ValidateAll()
		{
			ValidateABL_BillNumber();
		}

		protected override void CheckABL_BillNumber()
		{
			var targetInfo = Bill.ABL_BillNumberInfo;
			if (Bill.CreateErrorForTest)
			{
				targetInfo.AddError("Test error on ABL_BillNumber.");
			}

			if (Bill.CreateMessageErrorForTest)
			{
				targetInfo.AddMessageError("Test message error on ABL_BillNumber.");

				if (Bill.Header?.IsNVC01SendingInProgress ?? false)
				{
					targetInfo.AddError("Test message error on ABL_BillNumber when sending NVC01 message.");
				}
			}

			if (Bill.CreateWarningForTest)
			{
				targetInfo.AddWarning("Test warning on ABL_BillNumber.");
			}
		}
	}
}
