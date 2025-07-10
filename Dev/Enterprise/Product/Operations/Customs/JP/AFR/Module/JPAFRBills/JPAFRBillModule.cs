using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.JP.AFR.Module
{
	public class JPAFRBillModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.JP.AFRBill; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.JP.AFRBill);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new JPAFRBillFilterStripControl(GridCollection, (JPAFRBillFilterStrip)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new JPAFRBillsModuleCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JPAFRBillFilterStrip();
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.JPAFRBillReporting; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}
	}
}
