using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	[ModuleID(ModuleId.CusReconDeclaration)]
	public class CusReconDeclarationCollection : ActiveBusinessObjectCollection<CusReconDeclaration>
	{
		public CusReconDeclarationCollection(BusinessObjectFactory factory, GlbCompany company)
			: base(factory, new ZQuery(CusReconDeclarationSchema.CRD_GB_Branch, company.Branches.GetPKs()))
		{
		}
		protected override bool AllowNew => false;
	}
}
