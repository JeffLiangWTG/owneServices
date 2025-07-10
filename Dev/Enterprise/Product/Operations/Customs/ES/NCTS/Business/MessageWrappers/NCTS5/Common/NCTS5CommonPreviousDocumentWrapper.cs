using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonPreviousDocumentWrapper : NCTS5CommonDocumentWithItemWrapper, INCTSCommonPreviousDocument
	{
		public NCTS5CommonPreviousDocumentWrapper(CusSupportingInfo doc, ZInt seqNum, string calculatedUOM = "", decimal calculatedQuantity = 0) : base(doc, seqNum)
		{
			this.calculatedUOM = calculatedUOM;
			this.calculatedQuantity = calculatedQuantity;
		}

		readonly ZString calculatedUOM;
		readonly ZDecimal calculatedQuantity;

		public ZString MeasurementUnitAndQualifier => calculatedUOM.IsEmpty ? document?.CSI_UnitOfQuantity ?? ZString.Empty : calculatedUOM;

		public ZDecimal Quantity => calculatedQuantity.IsEmpty ? document?.CSI_Quantity ?? ZDecimal.Zero : calculatedQuantity;

		public ZBool QuantitySpecified => (!document?.CSI_Quantity.IsEmpty ?? false) || !calculatedQuantity.IsEmpty;
	}
}
