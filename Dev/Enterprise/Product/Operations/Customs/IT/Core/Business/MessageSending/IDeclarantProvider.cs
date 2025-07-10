using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public interface IDeclarantProvider
{
	OrgAddress DeclarantAddress { get; }
	ZString RepresentativeType { get; }
}
