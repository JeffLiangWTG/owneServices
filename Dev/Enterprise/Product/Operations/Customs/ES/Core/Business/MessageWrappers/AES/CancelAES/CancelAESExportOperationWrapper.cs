using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class CancelAESExportOperationWrapper : AESCommonExportOperationMRNWrapper, ICancelAESExportOperation
	{
		public CancelAESExportOperationWrapper(CusEntryHeader entryHeader, ReasonForCancellation reasonForCancellation) : base(entryHeader)
		{
			this.reasonForCancellation = Argument.NotNull(reasonForCancellation, nameof(reasonForCancellation));
		}
		readonly ReasonForCancellation reasonForCancellation;

		public ZString InvalidationReason => reasonForCancellation.Code + "-" + reasonForCancellation.Reason;
	}
}
