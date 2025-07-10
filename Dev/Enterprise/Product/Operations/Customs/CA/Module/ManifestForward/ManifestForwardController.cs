using System;
using Enterprise.Environment;
using Enterprise.Messaging.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class ManifestForwardController : EDIMessageController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CA.CAManifestForward;

		public override ControllerID ID => ControllerIDs.Customs.CA.CAManifestForward;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CAManifestForwardView;

		public override Type TypeOfTopLevelBusinessObject => typeof(Business.ACIForwarderMessage);
	}
}
