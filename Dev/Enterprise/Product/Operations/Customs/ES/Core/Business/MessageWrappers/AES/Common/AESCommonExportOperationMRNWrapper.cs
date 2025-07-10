using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AESCommonExportOperationMRNWrapper : IAESCommonExportOperationMRN
	{
		public AESCommonExportOperationMRNWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}
		protected readonly CusEntryHeader entryHeader;

		public ZString MRN => entryHeader.MovementReferenceNumber;
	}
}
