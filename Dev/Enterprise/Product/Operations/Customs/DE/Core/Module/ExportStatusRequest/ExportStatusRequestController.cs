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
	public class ExportStatusRequestController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.DE.ExportStatusRequest;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.DE.ExportStatusRequest;

		public override Type TypeOfTopLevelBusinessObject => typeof(StatusRequest);

		protected override IZForm GetForm(IBusiness businessEntity) => new ExportStatusRequestForm((StatusRequest)businessEntity);

		#region Security

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ExportMessaging;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ExportMessaging;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ExportMessaging;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ExportMessaging;

		#endregion
	}
}
