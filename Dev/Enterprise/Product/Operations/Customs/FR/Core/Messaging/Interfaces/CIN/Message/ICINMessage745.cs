using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.CIN
{
	public interface ICINMessage745
	{
		ZString Level { get; }
		ZString Name { get; }
		ZInt MrnQuantity { get; }
		ZDecimal MrnWeight { get; }
		ZString MrnNumber { get; }
		ZString ExitOfOffice { get; }
		ZString OACI_Shipper { get; }
		ZString OACI_Carrier { get; }
		ZString CustomOffice { get; }
		ZString BGMReference { get; }
		ZString TransactionIDCIN745 { get; }
		ZString Magasin { get; }
	}
}
