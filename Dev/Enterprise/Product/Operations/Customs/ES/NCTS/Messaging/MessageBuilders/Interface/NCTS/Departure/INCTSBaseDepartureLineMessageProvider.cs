using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface INctsBaseDepartureLineMessageProvider : INctsLineMessageProvider
	{
		#region Fields For MEA

		ZDecimal OtherUnitsNumber { get; }
		ZString OtherUnitsQualifier { get; }

		#endregion

		#region Fields For TDT

		ZString DangerousGoodsCode { get; }

		#endregion

		#region Fields For RFF

		ZString DocumentTypeCode { get; }
		ZString DocumentReferenceNumber { get; }
		ZString DocumentLineNo { get; }
		ZString DocumentClass { get; }

		#endregion
	}
}
