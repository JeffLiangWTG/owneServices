using CargoWise.EntityFramework;
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
	public class DocumentListMessagesModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.KR.DocumentListMessages;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.DocumentListMessages;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowDelete => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.KR.DocumentListMessages);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new DocumentListMessagesFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new DocumentListMessagesFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new NonDependentDLTMessageCollection(Factory, GlbCompany.CurrentCompany);
	}
}
