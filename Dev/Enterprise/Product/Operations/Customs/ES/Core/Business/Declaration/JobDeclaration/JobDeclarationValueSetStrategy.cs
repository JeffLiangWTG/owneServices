using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class JobDeclarationValueSetStrategy : EU.Business.Declaration.JobDeclarationValueSetStrategy
	{
		public JobDeclarationValueSetStrategy(JobDeclaration declaration) : base(declaration)
		{
			Declaration = declaration;
		}

		protected readonly new JobDeclaration Declaration;

		protected override ZString GetDeclarantType(OrgHeader orgHeader)
		{
			return ZString.Empty;
		}
	}
}
