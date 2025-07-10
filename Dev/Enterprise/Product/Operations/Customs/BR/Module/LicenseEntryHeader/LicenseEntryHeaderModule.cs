using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Module
{
	public class LicenseEntryHeaderModule : Customs.Module.EntryHeaderModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.BR.LicenseEntryHeader;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new LicenseEntryHeaderController();

		protected override IFilterControl GetNewFilterControl() => new LicenseEntryHeaderFilterControl(GridCollection, (LicenseEntryHeaderFilterBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new LicenseEntryHeaderFilterBusinessObject();

		public override bool AllowNew => false;

		public override bool AllowDelete => false;
	}
}
