using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.COD
{
	public interface IGen
	{
		ZString EntryNumber { get; } //Gens/Gen/refdec
		ZString Direction { get; } //Gens/Gen/typflux
		ZString Numcod { get; } //Gens/Gen/numcod
		ZString Opecod { get; } //Gens/Gen/Operateur/opecod
	}
}
