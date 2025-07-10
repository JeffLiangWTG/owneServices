using System;

namespace Enterprise.Customs.IT.Business.Declaration;

public class DeclarationValueChangedAnnouncer : Customs.Business.DeclarationValueChangedAnnouncer
{
	public DeclarationValueChangedAnnouncer(JobDeclaration declaration) : base(declaration)
	{
		ITDeclaration.MessageVersionInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
	}

	JobDeclaration ITDeclaration => (JobDeclaration)declaration;

	public override void Dispose()
	{
		base.Dispose();
		ITDeclaration.MessageVersionInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
	}
}
