using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class ExportClassificationModule : HTSClassificationModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.CA.CAExportClassification;

		protected override IBusinessObjectCollection GetNewGridCollection() => new ExportClassificationCollection(Factory);

		protected override bool ShowRecentItemsCore() => true;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ExportClassification;
	}
}
