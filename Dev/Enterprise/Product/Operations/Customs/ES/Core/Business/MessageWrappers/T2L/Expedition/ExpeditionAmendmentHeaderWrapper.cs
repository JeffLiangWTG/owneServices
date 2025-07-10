using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ExpeditionAmendmentHeaderWrapper : ExpeditionHeaderWrapper, IExpeditionAmendmentHeader
	{
		public ExpeditionAmendmentHeaderWrapper(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		public ZString ExpeditionT2LReference => entryHeader.MovementReferenceNumber;
	}
}
