using System;
using Enterprise.Environment;
using Enterprise.Messaging.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class CAQueryMessagesController : EDIMessageController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.CA.CAQueryMessages; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.CA.CAQueryMessages; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CAQueryMessagesView; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Business.EDIMessage); }
		}
	}
}
