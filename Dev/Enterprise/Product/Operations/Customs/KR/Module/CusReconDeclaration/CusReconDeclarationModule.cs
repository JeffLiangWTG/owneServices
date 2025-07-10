using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Module
{
	public class CusReconDeclarationModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.KR.CusReconDeclaration;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CusReconDeclaration;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.KR.CusReconDeclaration);

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CusReconDeclarationFilterStripBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusReconDeclarationFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusReconDeclarationCollection(Factory, GlbCompany.CurrentCompany);
		}
	}
}
