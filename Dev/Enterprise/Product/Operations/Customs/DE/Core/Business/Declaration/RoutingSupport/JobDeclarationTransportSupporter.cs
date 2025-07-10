using Enterprise.Freight.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class JobDeclarationTransportSupporter : EU.Business.Declaration.JobDeclarationTransportSupporter
	{
		public JobDeclarationTransportSupporter(JobDeclaration parent)
			: base(parent)
		{
		}

		public override JobConsolTransportValidation GetNewTransportValidator(Transport transport) => new JobDeclarationTransportValidation(transport);
	}
}
