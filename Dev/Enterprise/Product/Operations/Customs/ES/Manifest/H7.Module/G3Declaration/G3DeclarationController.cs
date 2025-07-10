using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ES.Manifest.H7.Module
{
	public class G3DeclarationController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.ES.G3Declaration;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.ES.G3Declaration;

		public override Type TypeOfTopLevelBusinessObject => typeof(G3EDIMessage);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.G3Declaration;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.G3Declaration;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.G3Declaration;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.G3Declaration;

		protected override IZForm GetForm(IBusiness businessEntity) => new Messaging.GUI.EDIMessageForm((G3EDIMessage)businessEntity);
	}
}
