using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business
{
	public class RefundApplicationDocumentSendingObject : AutoRefundApplicationDocumentSendingObject
	{
		public RefundApplicationDocumentSendingObject(Declaration.CusEntryHeader entryHeader, RefundApplicationMessageSendingAction parent)
			: base(entryHeader.Factory)
		{
			EntryHeader = entryHeader;
			Parent = Argument.NotNull(parent, nameof(parent));
		}

		public Declaration.CusEntryHeader EntryHeader { get; }

		public RefundApplicationMessageSendingAction Parent { get; }

		[List(nameof(Lookups) + "." + nameof(RefundApplicationDocumentSendingObjectLookups.DocumentTypeList))]
		public override ZString DocumentType
		{
			get => base.DocumentType;
			set => base.DocumentType = value;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("35B396A4-E418-4348-BC6A-9E57D82DB70C", "Refund Application Document");

		public RefundApplicationDocumentSendingObjectLookups Lookups => lookups ?? (lookups = new RefundApplicationDocumentSendingObjectLookups(this));
		RefundApplicationDocumentSendingObjectLookups lookups;
	}
}
