using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public class EntryLineConfiguration : EU.Business.EntryLineConfiguration
{
	protected override ZBool ShouldFilterSupportingDocumentsByMergeKeysCore() => ZBool.False;
}
