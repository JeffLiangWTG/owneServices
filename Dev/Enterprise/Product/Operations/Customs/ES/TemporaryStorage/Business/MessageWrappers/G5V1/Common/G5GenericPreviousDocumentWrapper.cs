using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers
{
	public class G5GenericPreviousDocumentWrapper : DocumentCommonWrapper, ICommonDocumentGoodsItemId
	{
		public G5GenericPreviousDocumentWrapper(TemporaryStoragePreviousDocument doc) : base(doc.CSI_Code, doc.CSI_ReferenceNumber)
		{
			document = Argument.NotNull(doc, nameof(doc));
		}
		readonly TemporaryStoragePreviousDocument document;

		public ZString GoodsItemId => document.CSI_LineNo.IsEmpty ? ZString.Empty : document.CSI_LineNo.ToString();
	}
}
