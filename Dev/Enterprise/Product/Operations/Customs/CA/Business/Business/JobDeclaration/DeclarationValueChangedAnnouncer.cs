using System;

namespace Enterprise.Customs.CA.Business
{
	public class DeclarationValueChangedAnnouncer : Customs.Business.DeclarationValueChangedAnnouncer
	{
		public DeclarationValueChangedAnnouncer(JobDeclaration declaration)
			: base(declaration)
		{
			Declaration.CA_ServiceOptionInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		public override void Dispose()
		{
			base.Dispose();

			Declaration.CA_ServiceOptionInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
		}
	}
}
