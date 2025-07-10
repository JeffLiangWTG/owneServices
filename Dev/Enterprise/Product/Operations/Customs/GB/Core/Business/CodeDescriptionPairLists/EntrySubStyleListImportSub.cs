using CargoWise.Integration;

namespace Enterprise.Customs.GB.Business.CodeDescriptionPairLists
{
	public partial class EntrySubStyleListImport : Integration.Customs.IEntrySubStyleListImport
	{
		ICodeDescriptionPairList Integration.Customs.IEntrySubStyleListImport.EntrySubStyleListImport()
		{
			return this;
		}
	}
}
