using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.Module
{
	public class TaxChangeAssessmentModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.DE.TaxChangeAssessment;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ImportMessaging;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.DE.TaxChangeAssessment);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TaxChangeAssessmentFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new TaxChangeAssessmentFilterStripControl(GridCollection, (TaxChangeAssessmentFilterStripBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new TaxChangeAssessmentCollection(Factory);

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		public override bool AllowDelete => false;

		public override bool AllowNew => false;
	}
}
