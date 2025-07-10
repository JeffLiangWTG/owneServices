using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public interface IESDocSADHLineTaxBoxSupporter : IDocSADHLineTaxBoxSupporter
	{
		ZBool DestinationStateIsCanaryIsland { get; }
		ZString EntryLineMethodOfPayment { get; }
		ZString EntryLineMethodOfPayment2 { get; }
	}
}
