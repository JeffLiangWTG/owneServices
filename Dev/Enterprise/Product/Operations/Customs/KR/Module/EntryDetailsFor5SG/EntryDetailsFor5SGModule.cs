using CargoWise.EntityFramework;
using Enterprise.Customs.Common.KR;
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
	public class EntryDetailsFor5SGModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.KR.EntryDetailsFor5SG;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EntryDetailsFor5SG;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;
		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.KR.EntryDetailsFor5SG);
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryDetailsFor5SGFilterStripBusinessObject();
		protected override IFilterControl GetNewFilterControl() => new EntryDetailsFor5SGFilterControl(GridCollection, FilterBusinessObject);
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new KREntryHeaderDetailsViewCollection(Factory, KRJobMessageTypeList.Codes.Import, new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG(), GlbCompany.CurrentCompany.PK);
		}
		public override bool AllowDelete => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider() => new EntryDetailsFor5SGModuleDecisionProvider(this);

		protected class EntryDetailsFor5SGModuleDecisionProvider : DefaultModuleDecisionProvider
		{
			public EntryDetailsFor5SGModuleDecisionProvider(ZFilterModule module) : base(module)
			{
			}

			public override bool AllowExcelExport => false;
		}
	}
}
