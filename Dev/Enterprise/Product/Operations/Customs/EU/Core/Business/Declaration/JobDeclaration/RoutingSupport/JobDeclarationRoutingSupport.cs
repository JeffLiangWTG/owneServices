using Enterprise.Freight.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public partial class JobDeclaration : ITransportParent
	{
		TransportSupporter ITransportParent.TransportSupporter => new JobDeclarationTransportSupporter(this);
	}
}
