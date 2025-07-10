using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	[SuppressFormsLocalizedTest]
	public class CcsukSplitBasicModule : ZFilterGridModule
	{
		protected override IFilterControl GetNewFilterControl()
		{
			return null;
		}

		protected override ZController GetNewController(CargoWise.EntityFramework.BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.GB.CcsukSplitBasicController);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return null;
		}

#if DEBUG
		public override bool IsExcludedFromUtcTestsEgNoGuiNeededOrNoFilterBusinessStripNeeded => true;
#endif

		protected override CargoWise.EntityFramework.IBusinessObjectCollection GetNewGridCollection()
		{
			return new SplitCollection(this.Factory);
		}

		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.GB.CcsukSplitBasic;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AirCcsukBase;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.AirCcsukMaster;

		public override bool AllowUniversalCopy => false;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
	}
}
