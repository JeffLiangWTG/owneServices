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
	public class EntryDetailsFor5ACModule : EntryDetailsModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryDetailsFor5ACFilterStripBusinessObject();
		protected override IFilterControl GetNewFilterControl() => new EntryDetailsFor5ACFilterControl(GridCollection, FilterBusinessObject);
		public override ModuleIdentifier ID => ModuleIDs.Customs.KR.ExportEntryDetails;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ExportEntryDetails;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;
		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.KR.ExportEntryDetails);
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5ACAnd5GW();
			return new KREntryHeaderDetailsViewCollection(Factory, KRJobMessageTypeList.Codes.Export, query, GlbCompany.CurrentCompany.PK);
		}
	}
}
