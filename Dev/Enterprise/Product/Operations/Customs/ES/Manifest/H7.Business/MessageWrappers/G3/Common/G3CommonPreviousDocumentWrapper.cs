using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3CommonPreviousDocumentWrapper : DocumentCommonWrapper, ICommonDocumentGoodsItemId
	{
		public G3CommonPreviousDocumentWrapper(AsycudaManifestHeader header, bool isMasterConsignment)
			: base(isMasterConsignment ? MasterConsignmentPreviousDocumentCode : CusEntryNumberTypes.Standard.MovementReferenceNumber,
				  isMasterConsignment ? (header?.AMA_MasterInformation ?? ZString.Empty) : (header?.G3MRNToRevoke ?? ZString.Empty))
		{
			Argument.NotNull(header, nameof(header));
			goodsItemId = isMasterConsignment ? header.EntryLineNumber : string.Empty;
		}

		public G3CommonPreviousDocumentWrapper(PreviousDocument document) : base(HouseConsignmentPreviousDocumentCode, document?.CSI_ReferenceNumber ?? ZString.Empty)
		{
			Argument.NotNull(document, nameof(document));
			goodsItemId = document.CSI_ReferenceNumber2;
		}

		public ZString GoodsItemId => goodsItemId;
		readonly ZString goodsItemId;

		const string HouseConsignmentPreviousDocumentCode = "335";
		const string MasterConsignmentPreviousDocumentCode = "337";
	}
}
