using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class HouseBilleManifestModule : ZFilterGridModule
	{
		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ConsolCAeManifestHouseBill; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.CA.CAHouseBilleManifest; }
		}

		protected override CargoWise.EntityFramework.IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusCAeMHMasterCollection(Factory);
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new HouseBilleManifestFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new HouseBilleManifestFilterStripBusinessObject();
		}

		protected override ZController GetNewController(CargoWise.EntityFramework.BusinessObject selectedBusinessObject)
		{
			var masterBill = selectedBusinessObject as CusCAeMHMaster;
			if (masterBill != null && masterBill.Consol != null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.CA.CAConsoleManifest);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.CA.CAHouseBilleManifest);
			}
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType => JobInvoicingConsumerTypes.CAeManifestCode;
	}
}
