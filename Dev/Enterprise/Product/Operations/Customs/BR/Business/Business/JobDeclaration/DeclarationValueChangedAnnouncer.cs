using System;

namespace Enterprise.Customs.BR.Business
{
	public class DeclarationValueChangedAnnouncer : Customs.Business.DeclarationValueChangedAnnouncer
	{
		public DeclarationValueChangedAnnouncer(JobDeclaration declaration)
			: base(declaration)
		{
			Declaration.JE_DispatchModalityInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
		}

		JobDeclaration Declaration => (JobDeclaration)declaration;

		public override void Dispose()
		{
			base.Dispose();
			Declaration.JE_DispatchModalityInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
		}
	}
}
