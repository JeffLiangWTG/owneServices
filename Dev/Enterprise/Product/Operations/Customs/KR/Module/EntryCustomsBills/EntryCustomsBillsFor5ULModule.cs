using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Module
{
	public class EntryCustomsBillsFor5ULModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.KR.EntryCustomsBillsFor5UL;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EntryCustomsBillsFor5UL;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;
		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.KR.EntryCustomsBillsFor5UL);
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryCustomsBillsFor5ULFilterStripBusinessObject();
		protected override IFilterControl GetNewFilterControl() => new EntryCustomsBillsFor5ULFilterControl(GridCollection, FilterBusinessObject);
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new KREntryCustomsBillsViewCollection(Factory, new KREntryCustomsBillsView.Loader(Factory).GetQueryFor5UL(), GlbCompany.CurrentCompany.PK);
		}
		public override bool AllowDelete => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider() => new EntryCustomsBillsFor5ULModuleProvider(this);
		protected override IModuleDecisionProvider GetModuleDecisionProviderForFindBoxPopupCore(IFindBox findbox)
		{
			return new EntryCustomsBillsModuleDecisionProvider(findbox);
		}

		protected class EntryCustomsBillsFor5ULModuleProvider : DefaultModuleDecisionProvider
		{
			public EntryCustomsBillsFor5ULModuleProvider(ZFilterModule module) : base(module)
			{
			}

			public override bool AllowExcelExport => false;
		}
	}
}
