using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Module
{
	public class LPCOEntryHeaderModule : Customs.Module.EntryHeaderModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.BR.LPCOEntryHeader;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new LPCOEntryHeaderController();

		protected override IFilterControl GetNewFilterControl() => new LPCOEntryHeaderFilterControl(GridCollection, (LPCOEntryHeaderFilterBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new LPCOEntryHeaderFilterBusinessObject();

		public override bool AllowNew => false;

		public override bool AllowDelete => false;
	}
}
