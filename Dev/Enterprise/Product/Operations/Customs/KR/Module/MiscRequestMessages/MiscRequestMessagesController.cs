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
	public class MiscRequestMessagesController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.KR.MiscRequestMessages;
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.KR.MiscRequestMessages;
		public override Type TypeOfTopLevelBusinessObject => typeof(CusMiscRequestHeader);
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.MiscRequestMessagesView;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.MiscRequestMessagesNew;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.MiscRequestMessagesEdit;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.MiscRequestMessagesDelete;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusMiscRequestHeaderViewForm((CusMiscRequestHeader)businessEntity);
		}
	}
}
