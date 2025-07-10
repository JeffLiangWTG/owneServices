using System;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class DeclarationValueChangedAnnouncer : Customs.Business.DeclarationValueChangedAnnouncer
	{
		public DeclarationValueChangedAnnouncer(JobDeclaration declaration)
			: base(declaration)
		{
			Declaration.ZG_BorderTransportMeansInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		public override void Dispose()
		{
			base.Dispose();
			Declaration.ZG_BorderTransportMeansInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
		}
	}
}
