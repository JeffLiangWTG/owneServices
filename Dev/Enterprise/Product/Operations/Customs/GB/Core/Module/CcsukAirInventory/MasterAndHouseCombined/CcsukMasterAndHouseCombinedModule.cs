using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukMasterAndHouseCombinedModule : ZFilterGridModule
	{
		public CcsukMasterAndHouseCombinedModule()
			: base()
		{
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusHAWBCollectionNonDependentShowMastersToo(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukMasterAndHouseCombined; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.GB.CcsukMasterAndHouseCombined);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CcsukMasterAndHouseCombinedFilterStripBusinessObject();
		}

		protected override ZArchitecture.GUI.IFilterControl GetNewFilterControl()
		{
			return new CcsukMasterAndHouseCombinedFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AirCcsukBase; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AirCcsukAllAWBs; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.CustomsHouseAirCargoCode; }
		}
	}
}
