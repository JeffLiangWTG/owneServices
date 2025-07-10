using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.CIN
{
	public interface ICINNested745Message
	{
		ZString Level { get; }
		ZString Name { get; }
		ZInt MrnQuantity { get; }
		ZDecimal MrnWeight { get; }
		ZString MrnNumber { get; }
		ZString ExitOfOffice { get; }
		ZString OACI { get; }
		ZString BUR_DOUANE { get; }
		ZString MRN_ECS { get; }
		ZString REFERENCE { get; }
		ZString DEST_OACI { get; }
		ZString OACI_Carrier { get; }
		ZString OACI_Shipper { get; }
		ZString SchemaID { get; }
		ZString SchemaVersion { get; }
		ZString TransactionID { get; }
		ZDateTime Time { get; }
		ZDate Date { get; }
		ZString MAGASIN { get; }
	}
}
