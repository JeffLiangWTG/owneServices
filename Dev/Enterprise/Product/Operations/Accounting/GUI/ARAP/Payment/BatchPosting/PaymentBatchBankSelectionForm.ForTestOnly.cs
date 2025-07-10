#if DEBUG

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentBatchBankSelectionForm
	{
		public Core.Forms.ZPostOrCancelButton SaveButton_ForTestOnly
		{
			get { return SaveButton; }
			set { SaveButton = value; }
		}

		public Core.Forms.ZPostOrCancelButton CloseButton_ForTestOnly
		{
			get { return CloseButton; }
			set { CloseButton = value; }
		}
	}
}

#endif
