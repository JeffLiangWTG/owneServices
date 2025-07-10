using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.DeclarationActivation.Business;

public sealed class DeclarationActivationHeaderCollection(BusinessObjectFactory factory)
	: ActiveBusinessObjectCollection<DeclarationActivationHeader>(factory, GetFilter())
{
	static ZQuery GetFilter() => new ZQuery(CusExitHeaderSchema.CXH_GC_Company, GlbBranch.CurrentBranch.Company.PK)
		.AddToFilter(CusExitHeaderSchema.CXH_ApplicationCode, CusExitHeaderApplicationCodeList.Codes.CHDeclarationActivation);
}
