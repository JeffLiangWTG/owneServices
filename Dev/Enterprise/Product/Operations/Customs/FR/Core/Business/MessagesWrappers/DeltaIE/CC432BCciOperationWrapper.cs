using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CC432BCciOperationWrapper : ICC432BCciOperation
	{
		CC432BCciOperationWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		readonly CusEntryHeader entryHeader;

		public static CC432BCciOperationWrapper New(CusEntryHeader entryHeader) => entryHeader == null ? null : new CC432BCciOperationWrapper(entryHeader);

		public string CustomsRegistrationNumber => customsRegistrationNumber ?? (customsRegistrationNumber = entryHeader.CRN);
		string customsRegistrationNumber;

		public string LRN => lrn ?? (lrn = entryHeader.CorrelationID);
		string lrn;
	}
}
