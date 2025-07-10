using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business
{
	public class EntryLineConfiguration : EU.Business.EntryLineConfiguration
	{
		protected override IEntryLineValidationDecider GetImportValidationDecider() => new Declaration.UCC6ImportEntryLineValidationDecider();

		protected override IEntryLineValidationDecider GetExportValidationDecider() => null;
	}
}
