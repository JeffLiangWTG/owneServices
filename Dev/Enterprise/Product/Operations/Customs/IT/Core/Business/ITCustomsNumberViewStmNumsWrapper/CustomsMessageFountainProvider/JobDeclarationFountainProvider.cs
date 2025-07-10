using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class JobDeclarationFountainProvider : CustomsMessageFountainProvider
{
	public JobDeclarationFountainProvider(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}
	readonly JobDeclaration declaration;

	protected override ZString NodeCore => declaration.Node;
	protected override ZString FountainTypeCore => NumberRangeTypeList.Codes.CustomsDeclarations;
	protected override GlbCompany CompanyCore => declaration.Company;
}
