using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobDeclarationTransportSupporter : BaseJobDeclarationTransportSupporter<JobDeclaration>
	{
		public JobDeclarationTransportSupporter(JobDeclaration parent)
			: base(parent)
		{
		}

		public override JobConsolTransportValidation GetNewTransportValidator(Transport transport) => new JobDeclarationTransportValidation(transport);
	}
}
