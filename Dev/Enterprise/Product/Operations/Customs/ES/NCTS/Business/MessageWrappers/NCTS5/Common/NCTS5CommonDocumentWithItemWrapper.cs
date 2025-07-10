using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonDocumentWithItemWrapper : NCTS5CommonDocumentWithInfoWrapper, INCTSCommonDocumentWithItem
	{
		public NCTS5CommonDocumentWithItemWrapper(CusSupportingInfo doc, ZInt seqNum) : base(doc, seqNum)
		{
			document = doc;
		}

		protected readonly CusSupportingInfo document;

		public ZString GoodsItemNumber => !document.CSI_ItemNumber.IsEmpty ? (ZString)document.CSI_ItemNumber.ToString() : ZString.Empty;
	}
}
