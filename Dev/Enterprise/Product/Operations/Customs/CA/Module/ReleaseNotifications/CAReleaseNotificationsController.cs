using System;
using Enterprise.Environment;
using Enterprise.Messaging.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class CAReleaseNotificationsController : EDIMessageController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.CA.CAReleaseNotifications; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.CAReleaseNotifications; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CAReleaseNotificationsView; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Business.EDIMessage); }
		}
	}
}
