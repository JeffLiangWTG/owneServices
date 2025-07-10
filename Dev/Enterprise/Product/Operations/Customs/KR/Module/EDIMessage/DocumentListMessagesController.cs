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
	public class DocumentListMessagesController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.KR.DocumentListMessages;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.KR.DocumentListMessages;

		public override Type TypeOfTopLevelBusinessObject => typeof(EDIMessage);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.DocumentListMessages;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity) => new DLTEDIMessageForm((EDIMessage)businessEntity);
	}
}
