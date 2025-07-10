using CargoWise.Integration;

namespace Enterprise.Customs.GB.Business.CodeDescriptionPairLists
{
	public partial class EntrySubStyleListExport : Integration.Customs.IEntrySubStyleListExport
	{
		ICodeDescriptionPairList Integration.Customs.IEntrySubStyleListExport.EntrySubStyleListExport()
		{
			return this;
		}
	}
}
