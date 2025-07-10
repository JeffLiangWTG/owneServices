using Enterprise.Freight.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public partial class JobDeclaration : ITransportParent
	{
		TransportSupporter ITransportParent.TransportSupporter => new JobDeclarationTransportSupporter(this);
	}
}
