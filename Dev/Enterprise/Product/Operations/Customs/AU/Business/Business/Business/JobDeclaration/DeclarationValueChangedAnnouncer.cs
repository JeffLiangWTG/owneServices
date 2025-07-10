using System;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DeclarationValueChangedAnnouncer : Customs.Business.DeclarationValueChangedAnnouncer
	{
		public DeclarationValueChangedAnnouncer(JobDeclaration declaration)
			: base(declaration)
		{
			Declaration.ZA_CustShipNoOverride_HiddenInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
			Declaration.JE_ToOrderInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		public override void Dispose()
		{
			base.Dispose();

			Declaration.JE_ToOrderInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
			Declaration.ZA_CustShipNoOverride_HiddenInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
		}
	}
}
