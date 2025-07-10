using CargoWise.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.NCTS.Messaging;

namespace Enterprise.Customs.FR.NCTS
{
	public class TP5MessageSendingObjectCollection : NctsHeaderMessageSendingObjectCollection
	{
		public TP5MessageSendingObjectCollection(Business.NCTS.NctsHeader nctsHeader)
			: base(Argument.NotNull(nctsHeader, nameof(nctsHeader)).Factory)
		{
			AddMessageSendingActions(nctsHeader);
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public new TP5MessageSendingObject this[int index] => (TP5MessageSendingObject)base[index];

		public new TP5MessageSendingObject AddNew() => (TP5MessageSendingObject)base.AddNew();

		void AddMessageSendingActions(Business.NCTS.NctsHeader header)
		{
			Add(new TP5MessageSendingObject(header));
		}
	}
}
