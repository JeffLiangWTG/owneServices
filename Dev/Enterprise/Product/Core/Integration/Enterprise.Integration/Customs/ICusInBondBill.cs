using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusInBondBill
		{
			ZGuid PK { get; }
			ZString B0_IssuerCode { get; set; }
			ZString B0_MasterBillNumber { get; set; }
			ZString B0_HouseBillNumber { get; set; }
			ZString B0_HouseBillIssuerCode { get; set; }
			ZGuid B0_BH { get; set; }
			ZString B0_InBondPortOfDestDCode { get; set; }
			ZDateTime B0_A_ARV { get; set; }
			ZString B0_ShipmentType { get; set; }
		}
	}
}
