namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class AsycudaBillValidationForMasterChild : EU.H7.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();
			if (!Parent.ABL_BillNumber.IsEmpty && !Parent.Header.EntryLineNumber.IsEmpty)
			{
				Parent.ABL_BillNumberInfo.AddMessageError(BothBillNumberAndEntryLineNumberEnteredMessageError);
			}
			else if (Parent.ABL_BillNumber.IsEmpty && Parent.Header.EntryLineNumber.IsEmpty)
			{
				Parent.ABL_BillNumberInfo.AddMessageError(BothBillNumberAndEntryLineNumberEmptyMessageError);
			}
		}

		public static string BothBillNumberAndEntryLineNumberEmptyMessageError => Res.GetString("b68f1520-58be-459e-b00b-4334e2b8d038", "You have not entered a Bill Number or a Entry Line Number.");
		public static string BothBillNumberAndEntryLineNumberEnteredMessageError => Res.GetString("2b61b9f4-8780-4143-9f63-cf72d7ec6ab7", "You have entered both a Master Bill Number and a Entry Line Number. Only one must be entered.");
	}
}
