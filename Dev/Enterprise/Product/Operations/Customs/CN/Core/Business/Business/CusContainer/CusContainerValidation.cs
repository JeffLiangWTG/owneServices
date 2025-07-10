namespace Enterprise.Customs.CN.Business
{
	public class CusContainerValidation : Customs.Business.CusContainerValidation
	{
		public CusContainerValidation(CusContainer parent)
			: base(parent)
		{
		}

		public new CusContainer Parent => (CusContainer)base.Parent;

		internal IValidationModeProvider ValidationModeProvider => Parent.Declaration;

		protected override void CheckCO_RC()
		{
			base.CheckCO_RC();

			if (Parent.ContainerCode.IsEmpty)
			{
				Parent.CO_RCInfo.AddNotification(Res.GetString("6dc3d708-5691-48da-a538-5cf94644cc94", "The selected Container Types doesn't have a code of China Customs. Please specify a China Customs Container Code in the 'Customs Container Codes' section of this Container Type."), ValidationModeProvider);
			}
		}
	}
}
