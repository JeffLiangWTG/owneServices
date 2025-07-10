using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.Module
{
	public class TaxChangeAssessmentController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity) => new TaxChangeAssessmentForm((TaxChangeAssessment)businessEntity);

		public override ControllerID ID => ControllerIDs.Customs.DE.TaxChangeAssessment;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.DE.TaxChangeAssessment;

		public override Type TypeOfTopLevelBusinessObject => typeof(TaxChangeAssessment);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ImportMessaging;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ImportMessaging;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ImportMessaging;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ImportMessaging;

		#endregion
	}
}
