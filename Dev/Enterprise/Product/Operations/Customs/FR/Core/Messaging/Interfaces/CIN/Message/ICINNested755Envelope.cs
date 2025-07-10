using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.CIN
{
	public interface ICINNested755Envelope
	{
		ZString OACI { get; }
		ZString REFERENCE { get; }
		ZString MRN_ECS { get; }
		ZString MAGASIN { get; }
		ZString BUR_DOUANE { get; }
		ZString DEST_OACI { get; }
		ZString NUM_LTA { get; }
		ZString CIN { get; }
	}
}
