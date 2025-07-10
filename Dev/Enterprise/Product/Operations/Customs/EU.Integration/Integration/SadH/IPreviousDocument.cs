using CargoWise.Types;

namespace Enterprise.Customs.EU.Integration.SadH
{
	public interface IPreviousDocument
	{
		ZString Class { get; }  // PREV-DOC-CLASS
		ZString Type { get; }   // PREV-DOC-TYPE
		ZString Reference { get; }  // PREV-DOC-REFERENCE
		ZString MoreInfo { get; }
	}
}
