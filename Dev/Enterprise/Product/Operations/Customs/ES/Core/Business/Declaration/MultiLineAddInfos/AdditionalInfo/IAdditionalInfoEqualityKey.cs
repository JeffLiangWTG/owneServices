using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public interface IAdditionalInfoEqualityKey
	{
		ZString CSI_Code { get; }
		ZString CSI_Description { get; }
		ZString CSI_SubType { get; }
		ZString CSI_ReferenceNumber { get; }
		ZString CSI_ReferenceNumber2 { get; }
		ZString CSI_RX_NKCurrency { get; }
		ZDecimal CSI_Value { get; }
	}
}
