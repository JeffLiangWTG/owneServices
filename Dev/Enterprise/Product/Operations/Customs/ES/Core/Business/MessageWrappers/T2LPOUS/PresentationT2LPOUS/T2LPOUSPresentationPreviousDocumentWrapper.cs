using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class T2LPOUSPresentationPreviousDocumentWrapper : DocumentCommonWrapper, IT2LPOUSPresentationPreviousDocument
	{
		public T2LPOUSPresentationPreviousDocumentWrapper(PreviousDocument doc, ZDecimal totalGrossWeightInKGFromInvoiceLine, ZString firstType, ZInt firstQty) : base(doc.CSI_Code, doc.CSI_ReferenceNumber)
		{
			document = Argument.NotNull(doc, nameof(doc));
			this.totalGrossWeightInKGFromInvoiceLine = totalGrossWeightInKGFromInvoiceLine;
			this.firstType = firstType;
			this.firstQty = firstQty;
		}
		readonly PreviousDocument document;
		readonly ZDecimal totalGrossWeightInKGFromInvoiceLine;
		readonly ZString firstType;
		readonly ZInt firstQty;

		public ZString MeasurementUnitAndQualifier => TypeOfMesurementUnitFixed;

		ZBool isPreviousDocumentEmpty => document.CSI_PackType.IsEmpty && document.CSI_PackQty.IsEmpty && document.CSI_Quantity.IsEmpty && !PackageHelper.PackTypeIsBulk(document.CSI_PackType, document.Factory);

		public ZDecimal Quantity => isPreviousDocumentEmpty ? totalGrossWeightInKGFromInvoiceLine : document.CSI_Quantity;

		ZInt packQty => isPreviousDocumentEmpty ? firstQty : document.CSI_PackQty;

		ZString packType => isPreviousDocumentEmpty ? firstType : document.CSI_PackType;

		public ZBool QuantityValueSpecified => !Quantity.IsEmpty;

		public ZInt GoodsItemIdentifier => document.CSI_LineNo;

		public IT2LPOUSCommonPackaging Packaging => packaging ?? (packaging = new T2LPOUSCommonPackagingWrapper(packType, packQty, document.Factory));
		T2LPOUSCommonPackagingWrapper packaging;

		const string TypeOfMesurementUnitFixed = "KGMG";
	}
}
