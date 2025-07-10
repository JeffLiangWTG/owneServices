using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Module
{
	public class EntryLineDetailsFor5ULModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.KR.EntryLineDetailsFor5UL;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EntryLineDetailsFor5UL;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;
		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.KR.EntryLineDetailsFor5UL);
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryLineDetailsFor5ULFilterStripBusinessObject();
		protected override IFilterControl GetNewFilterControl() => new EntryLineDetailsFor5ULFilterControl(GridCollection, FilterBusinessObject);
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new KREntryLineDetailsViewCollection(Factory);
		}
		public override bool AllowDelete => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider() => new EntryLineDetailsFor5ULModuleProvider(this);

		protected class EntryLineDetailsFor5ULModuleProvider : DefaultModuleDecisionProvider
		{
			public EntryLineDetailsFor5ULModuleProvider(ZFilterModule module) : base(module)
			{
			}

			public override bool AllowExcelExport => false;
		}
	}
}
