using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class JobDeclarationSynchroniser : Customs.Business.JobDeclarationSynchroniser
{
	public JobDeclarationSynchroniser(BaseJobDeclaration declaration) : base(declaration)
	{
	}

	protected override Customs.Business.PackingSynchroniser GetPackingSynchroniser()
	{
		return new PackingSynchroniser(this, Destination);
	}
}
