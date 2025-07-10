using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Module
{
	public class ConsolidatedDeclarationModule : Customs.Module.ConsolidatedDeclarationModule
	{
		protected override IBusinessObjectCollection GetNewGridCollection() => new Business.ConsolidatedDeclarationCollection<ConsolidatedDeclaration>(Factory, Customs.Business.ConsolidatedDeclaration.ApplicationCodes.CMR);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ConsolidatedDeclarationFilterBusinessObject();
	}
}
