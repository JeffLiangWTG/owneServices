using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business
{
	public class EntryHeaderConfiguration : EU.Business.EntryHeaderConfiguration
	{
		protected override IEntryHeaderValidationDecider GetImportValidationDecider() => new Declaration.UCC6ImportEntryHeaderValidationDecider();

		protected override IEntryHeaderValidationDecider GetExportValidationDecider() => null;
	}
}
