using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.CIN
{
	public interface ICINLine
	{
		ZGuid PK { get; }
		ZString Type { get; }
		ZString ReferenceNumber { get; }
		ZInt NoPieces { get; }
		ZInt TotalNoPieces { get; }
		ZDecimal Mass { get; }
		ZDecimal TotalMass { get; }
		ZString DescriptionOfGoods { get; }
		bool IsAirwayBill { get; }
	}
}
