namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PackingSynchroniser : Customs.Business.PackingSynchroniser
	{
		public PackingSynchroniser(JobDeclarationSynchroniser parentSynchroniser, JobDeclaration declaration)
			: base(parentSynchroniser, declaration)
		{
		}
	}
}
