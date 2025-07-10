using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NctsDepartureBaseDepartureLineWrapper : NctsDepartureLineMessageWrapper, INctsBaseDepartureLineMessageProvider
	{
		public NctsDepartureBaseDepartureLineWrapper(NctsDepartureCargoDesc goodsItem)
			: base(goodsItem)
		{
			previousDocument = goodsItem.PreviousDocuments.Count > 0 ? goodsItem.PreviousDocuments[0] : null;
		}

		readonly NctsPreviousDocument previousDocument;

		public ZDecimal OtherUnitsNumber => goodsItem.BY_CustomsThirdQuantity;

		public ZString OtherUnitsQualifier => goodsItem.BY_CustomsThirdUnitQty.ConvertCargoWiseToES(goodsItem.Factory);

		public ZString DangerousGoodsCode => goodsItem.UNDangerousGoodsCode.Left(4);

		public ZString DocumentTypeCode
		{
			get
			{
				if (documentTypeCode == null)
				{
					goodsItem.Factory.GetValue(ref documentTypeCode, () =>
					{
						var docType = previousDocument?.CSI_Code ?? ZString.Empty;

						if (docType.IsEmpty)
						{
							return ZString.Empty;
						}
						else if (docType == NctsPreviousDocumentTypeCodeList.Codes.SumDocument)
						{
							return previousDocument.CSI_ReferenceNumber.Length >= 17 ? NctsPreviousDocumentTypeCodeList.Codes.AfbDocument : NctsPreviousDocumentTypeCodeList.Codes.AeiDocument;
						}
						else
						{
							return PreviousDocumentHelper.NctDepartureAAEDocumentTypes.Contains(docType) ? NctsPreviousDocumentTypeCodeList.Codes.AaeDocument : NctsPreviousDocumentTypeCodeList.Codes.AcgDocument;
						}
					});
				}
				return documentTypeCode.Value;
			}
		}
		CachedProperty<ZString> documentTypeCode;

		public ZString DocumentReferenceNumber => previousDocument != null ? PreviousDocumentHelper.GetReferenceNumberToSendNctsDeparture(previousDocument) : ZString.Empty;

		public ZString DocumentLineNo
		{
			get
			{
				var returnLineNumber = ZString.Empty;
				if (previousDocument != null)
				{
					var lineNumber = previousDocument.CSI_LineNo;
					if (DocumentTypeCode == NctsPreviousDocumentTypeCodeList.Codes.AeiDocument && lineNumber > ZShort.Zero)
					{
						returnLineNumber = lineNumber.ToString();
					}
				}
				return returnLineNumber;
			}
		}

		public ZString DocumentClass => previousDocument != null ? previousDocument.CSI_SubType : ZString.Empty;
	}
}
