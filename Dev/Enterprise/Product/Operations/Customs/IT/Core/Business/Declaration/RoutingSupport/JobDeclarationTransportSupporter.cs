using Enterprise.Freight.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class JobDeclarationTransportSupporter : EU.Business.Declaration.JobDeclarationTransportSupporter
{
	public JobDeclarationTransportSupporter(EU.Business.Declaration.JobDeclaration parent) : base(parent)
	{
	}

	public override JobConsolTransportValidation GetNewTransportValidator(Transport transport) => new JobDeclarationTransportValidation(transport);
}
