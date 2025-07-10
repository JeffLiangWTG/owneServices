namespace Enterprise.Customs.CN.Business
{
	public class DeclarationValueChangedAnnouncer : Customs.Business.DeclarationValueChangedAnnouncer
	{
		public DeclarationValueChangedAnnouncer(JobDeclaration declaration)
			: base(declaration)
		{
			Declaration.JE_CustomsOfficeInfo.ValueChanged += DeclarationValueChanged;
		}

		JobDeclaration Declaration => (JobDeclaration)base.declaration;

		public override void Dispose()
		{
			base.Dispose();
			Declaration.JE_CustomsOfficeInfo.ValueChanged -= DeclarationValueChanged;
		}
	}
}
