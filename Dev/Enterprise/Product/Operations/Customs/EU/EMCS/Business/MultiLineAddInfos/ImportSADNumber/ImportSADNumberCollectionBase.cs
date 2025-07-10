using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.EU.EMCS.Business;

public abstract class ImportSADNumberCollection : CusSupportingInfoCollection<ImportSADNumber>
{
	protected ImportSADNumberCollection(EMCSJobDeclaration declaration)
		: base(declaration, CusSupportingInfoTypeList.Codes.ImportSad)
	{
	}
}
