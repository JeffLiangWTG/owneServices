using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class JobDeclaration : ITransportParent
{
	public new DeclarationTransportCollection Transports => base.Transports;

	protected override DeclarationTransportCollection GetNewDeclarationTransportCollection() => new DeclarationTransportCollection(this);

	TransportSupporter ITransportParent.TransportSupporter => new JobDeclarationTransportSupporter(this);
}
