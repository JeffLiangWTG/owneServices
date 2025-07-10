using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsHeaderCustomsMessageFountainProvider : CustomsMessageFountainProvider
{
	public NctsHeaderCustomsMessageFountainProvider(NctsHeader nctsHeader)
	{
		this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
	}
	readonly NctsHeader nctsHeader;

	protected override ZString NodeCore => nctsHeader.Node;
	protected override ZString FountainTypeCore => NumberRangeTypeList.Codes.CustomsDeclarations;
	protected override GlbCompany CompanyCore => nctsHeader.Company;
}
