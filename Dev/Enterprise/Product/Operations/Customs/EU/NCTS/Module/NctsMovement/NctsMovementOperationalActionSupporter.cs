using System;
using CargoWise.Definitions;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.EU.NCTS.Module
{
	public sealed class NctsMovementOperationalActionSupporter : Services.OperationalActions.Support.OperationalActionSupporter
	{
		public override Type RootType => typeof(NctsHeader);

		public override BusinessContext BusinessContext => BusinessContext.CusInBondHeader;

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.EuNctsMovement;
	}
}
