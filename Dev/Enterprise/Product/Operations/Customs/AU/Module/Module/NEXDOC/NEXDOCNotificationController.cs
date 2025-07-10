using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class NEXDOCNotificationController : ZController
	{
		public NEXDOCNotificationController()
		{
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(QuarantineNexDocNotification); }
		}

		public override ControllerID ID => ControllerIDs.Customs.AU.NEXDOCNotificationController;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.AU.NexDocNotifications;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.NEXDOCNotificationFileView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.NEXDOCNotificationFileNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.NEXDOCNotificationFileEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.NEXDOCNotificationFileDelete;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new NexDocNotificationForm((QuarantineNexDocNotification)businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.AU.Module.Res.GetData("NEXDOCNotificationPluginTabPage|042B0119-10B2-4EAF-A2FB-050B879A68C8", "NEXDOC Notification", "NEXDOC Notification Tab."); } }
	}
}
