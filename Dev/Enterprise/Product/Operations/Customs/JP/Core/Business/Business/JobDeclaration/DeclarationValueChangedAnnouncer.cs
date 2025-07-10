using System;

namespace Enterprise.Customs.JP.Business
{
	public class DeclarationValueChangedAnnouncer : Customs.Business.DeclarationValueChangedAnnouncer
	{
		public DeclarationValueChangedAnnouncer(JobDeclaration declaration) : base(declaration)
		{
			Declaration.IsMailedCargoInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			Declaration.JE_RL_NKPortOfLoadingInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		public override void Dispose()
		{
			base.Dispose();

			Declaration.IsMailedCargoInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			Declaration.JE_RL_NKPortOfLoadingInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
		}
	}
}
