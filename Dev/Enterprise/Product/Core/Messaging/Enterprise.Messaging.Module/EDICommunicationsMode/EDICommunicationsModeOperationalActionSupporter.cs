using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Messaging.Module
{
	public class EDICommunicationsModeOperationalActionSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext => BusinessContext.EDIMessage;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.Organisation;

		public override Type RootType => typeof(EDICommunicationsMode);

		public override SecurityCheckpoint CustomizationSecurityCheckpoint => Env.Security.Organisation;

		public override SecurityCheckpoint RunSecurityCheckpoint => Env.Security.Organisation;
	}
}
