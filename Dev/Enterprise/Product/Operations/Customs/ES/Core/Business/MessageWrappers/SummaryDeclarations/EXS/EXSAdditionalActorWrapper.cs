using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class EXSAdditionalActorWrapper : IEXSAdditionalActor
	{
		public EXSAdditionalActorWrapper(CusReference reference)
		{
			refer = Argument.NotNull(reference, nameof(reference));
		}

		readonly CusReference refer;

		public ZString Role => refer.CFR_Code;
		public ZString Id => refer.CFR_Reference;
	}
}
