using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.BR.Module
{
	public class LPCOController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.BR.LPCO;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.BR.LPCO;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusLPCOHeader);

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.BRLPCOEdit;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.BRLPCOView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.BRLPCONew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.BRLPCODelete;

		protected override IZForm GetForm(IBusiness businessEntity) => new LPCOForm((CusLPCOHeader)businessEntity);
	}
}
