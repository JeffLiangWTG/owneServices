using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationAeoCertificateSupporterWrapper : IAeoCertificateSupporter
{
	public JobDeclarationAeoCertificateSupporterWrapper(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}
	readonly JobDeclaration declaration;

	public OrgHeader Supplier => declaration.Supplier;

	public OrgHeader Importer => declaration.Importer;

	public OrgHeader Declarant => declaration.DeclarantAddress?.Header;

	public ZString RepresentationType => declaration.JE_DeclarantType;

	public ZBool ShouldAddY022Certificate => declaration.IsExport;

	public ZBool ShouldAddY023Certificate => declaration.IsImport;
}
