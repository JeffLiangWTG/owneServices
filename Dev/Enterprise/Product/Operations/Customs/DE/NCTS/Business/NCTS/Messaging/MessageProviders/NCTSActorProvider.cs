using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSActorProvider : INCTSActor
	{
		public static NCTSActorProvider NewOrNull(CusReference actor) => actor != null ? new NCTSActorProvider(actor) : null;

		NCTSActorProvider(CusReference actor)
		{
			this.actor = Argument.NotNull(actor, nameof(actor));
		}

		public string Role => actor.CFR_Code;

		public string EoriNumber => actor.CFR_Reference;

		readonly CusReference actor;
	}
}
