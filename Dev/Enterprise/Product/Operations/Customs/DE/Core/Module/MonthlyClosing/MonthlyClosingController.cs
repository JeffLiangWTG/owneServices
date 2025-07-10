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
	public class MonthlyClosingController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity) => new MonthlyClosingForm((CusReconDeclaration)businessEntity);

		public override ControllerID ID => ControllerIDs.Customs.DE.MonthlyClosing;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.DE.MonthlyClosing;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusReconDeclaration);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.MonthlyClosing;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.MonthlyClosing;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.MonthlyClosing;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.MonthlyClosing;
	}
}
