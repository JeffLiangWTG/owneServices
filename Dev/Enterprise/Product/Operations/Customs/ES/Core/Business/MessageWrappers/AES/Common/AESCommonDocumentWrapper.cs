using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AESCommonDocumentWrapper : AESCommonLineNumberDocumentWrapper, IAESCommonDocument
	{
		public AESCommonDocumentWrapper(CusSupportingInfo doc, ZShort seqNum, string calculatedUOM = "", decimal calculatedQuantity = 0) : base(doc, seqNum)
		{
			this.calculatedUOM = calculatedUOM;
			this.calculatedQuantity = calculatedQuantity;
		}

		public AESCommonDocumentWrapper(ZString code, ZString referenceNumber, ZShort seqNum, string calculatedUOM = "", decimal calculatedQuantity = 0) : base(code, referenceNumber, seqNum)
		{
			this.calculatedUOM = calculatedUOM;
			this.calculatedQuantity = calculatedQuantity;
		}

		protected readonly ZString calculatedUOM;
		protected readonly ZDecimal calculatedQuantity;

		public ZString Measurement => calculatedUOM.IsEmpty ? document?.CSI_UnitOfQuantity ?? ZString.Empty : calculatedUOM;

		public ZDecimal Quantity => calculatedQuantity.IsEmpty ? document?.CSI_Quantity ?? ZDecimal.Zero : calculatedQuantity;

		public ZBool QuantitySpecified => (!document?.CSI_Quantity.IsEmpty ?? false) || !calculatedQuantity.IsEmpty;

		protected override ZString LineNumberCore => GetFormattedLineNumber(base.LineNumberCore);

		ZString GetFormattedLineNumber(ZString lineNo)
		{
			var formattedLineNo = lineNo;

			if (!lineNo.IsEmpty && document != null && document.CSI_Type == Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument)
			{
				var lengthForGoodsItemNumber = document.CSI_Code == UniversalReferenceConstants.PreviousDocumentType.NMRN ? 5 : 3;

				formattedLineNo = lineNo.PadLeft(lengthForGoodsItemNumber, '0');
			}

			return formattedLineNo;
		}
	}
}
