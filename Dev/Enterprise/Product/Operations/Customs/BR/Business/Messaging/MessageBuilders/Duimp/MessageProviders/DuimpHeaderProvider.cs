using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class DuimpHeaderProvider : IDuimpHeader
	{
		public DuimpHeaderProvider(DuimpMessageSendingObject sendingObject)
		{
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
			EntryInstruction = this.sendingObject.Header.EntryInstruction;
		}

		readonly DuimpMessageSendingObject sendingObject;

		CusEntryInstruction EntryInstruction { get; set; }

		public IIdentification Identification => fIdentification ??= IdentificationProvider.New(EntryInstruction);
		IIdentification fIdentification;

		public ICargo Cargo => fCargo ??= CargoProvider.New(EntryInstruction);
		ICargo fCargo;

		public IDocument Document => fDocument ??= DocumentProvider.New(EntryInstruction);
		IDocument fDocument;
	}
}
