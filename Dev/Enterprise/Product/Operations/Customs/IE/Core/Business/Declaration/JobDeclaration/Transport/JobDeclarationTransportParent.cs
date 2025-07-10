using Enterprise.Freight.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class JobDeclaration : ITransportParent
	{
		TransportSupporter ITransportParent.TransportSupporter => new JobDeclarationTransportSupporter(this);
	}
}
