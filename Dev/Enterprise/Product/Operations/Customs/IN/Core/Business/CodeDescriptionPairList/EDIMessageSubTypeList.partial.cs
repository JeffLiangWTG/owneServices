using CargoWise.Types;

namespace Enterprise.Customs.IN.Business;

partial class EDIMessageSubTypeList
{
	public static string GetNegativeSubType(string originalSubtype)
	{
		return originalSubtype switch
		{
			Codes.AirCgm => Codes.AirCgmNegativeAcknowledgement,
			Codes.SeaCgm => Codes.SeaCgmNegativeAcknowledgement,
			Codes.ShippingBillFresh => Codes.ShippingBillNegativeAcknowledgement,
			_ => (string)ZString.Empty
		};
	}
}
