using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public interface IAeoCertificateSupporter
{
	OrgHeader Supplier { get; }
	OrgHeader Importer { get; }
	OrgHeader Declarant { get; }
	ZString RepresentationType { get; }
	ZBool ShouldAddY022Certificate { get; }
	ZBool ShouldAddY023Certificate { get; }
}
