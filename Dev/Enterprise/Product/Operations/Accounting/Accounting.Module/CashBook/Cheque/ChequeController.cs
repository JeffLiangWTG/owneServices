using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ChequeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ModuleIDs.Cheque;

		public override Type TypeOfTopLevelBusinessObject => typeof(AccReceivedCheque);

		public override ControllerID ID => ControllerIDs.Cheque;

		public override IZForm ShowNewForm() => null;

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity) => null;

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity) => null;

		protected override IZForm GetForm(IBusiness businessEntity) => null;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;
	}
}
