using System;

namespace Enterprise.Customs.NL.Business.Declaration;

public class DeclarationValueChangedAnnouncer : Customs.Business.DeclarationValueChangedAnnouncer
{
	public DeclarationValueChangedAnnouncer(JobDeclaration declaration)
		: base(declaration)
	{
		Declaration.JE_TransportModeInlandInfo.ValueChanged += new EventHandler(DeclarationValueChanged);
	}

	JobDeclaration Declaration
	{
		get { return (JobDeclaration)base.declaration; }
	}

	public override void Dispose()
	{
		base.Dispose();
		Declaration.JE_TransportModeInlandInfo.ValueChanged -= new EventHandler(DeclarationValueChanged);
	}
}
