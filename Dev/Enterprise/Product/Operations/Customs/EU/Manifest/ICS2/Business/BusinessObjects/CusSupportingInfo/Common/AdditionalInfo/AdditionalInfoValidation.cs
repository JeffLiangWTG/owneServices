namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AdditionalInfoValidation : Customs.Business.CusSupportingInfoValidation
	{
		public AdditionalInfoValidation(AdditionalInfo parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			if (!Parent.CSI_Code.IsEmpty && !Parent.CSI_Description.IsEmpty)
			{
				Parent.CSI_CodeInfo.AddMessageError(RedundantDataError);
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			if (!Parent.CSI_Code.IsEmpty && !Parent.CSI_Description.IsEmpty)
			{
				Parent.CSI_DescriptionInfo.AddMessageError(RedundantDataError);
			}
		}

		string RedundantDataError => Res.GetString("265aa231-8d02-4d18-88f7-8d95f6e12d07", "You have entered both Code and Text fields when not required. Please enter either Code or Text.");
	}
}
