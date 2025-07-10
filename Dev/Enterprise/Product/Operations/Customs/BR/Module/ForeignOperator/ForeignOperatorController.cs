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
	public class ForeignOperatorController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.BR.ForeignOperator;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.BR.ForeignOperator;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusBRForeignOperator);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.BRForeignOperatorView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.BRForeignOperatorNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.BRForeignOperatorEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.BRForeignOperatorDelete;

		protected override IZForm GetForm(IBusiness businessEntity) => new ForeignOperatorForm((CusBRForeignOperator)businessEntity);

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			return base.GetLoadedBusinessEntityInLocalFactory(sourceEntity) as CusBRForeignOperator;
		}
	}
}
