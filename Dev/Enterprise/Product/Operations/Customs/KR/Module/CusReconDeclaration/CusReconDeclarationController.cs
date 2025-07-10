using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Module
{
	public class CusReconDeclarationController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.KR.CusReconDeclaration;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.KR.CusReconDeclaration;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusReconDeclaration);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CusReconDeclarationView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CusReconDeclarationEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CusReconDeclarationNew;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CusReconDeclarationDelete;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefundDeclarationForm((CusReconDeclaration)businessEntity);
		}
	}
}
