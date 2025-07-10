using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class EntryHeaderConfiguration
	{
		public IEntryHeaderValidationDecider GetValidationDecider(CusEntryHeader cusEntryHeader) => GetValidationDeciderCore(cusEntryHeader);

		protected virtual IEntryHeaderValidationDecider GetValidationDeciderCore(CusEntryHeader cusEntryHeader)
		{
			var declaration = cusEntryHeader.Declaration;
			if ((object)declaration != null && declaration.IsUCC6)
			{
				if ((bool)declaration.IsImport)
				{
					return GetImportValidationDecider();
				}

				if ((bool)declaration.IsExport)
				{
					return GetExportValidationDecider();
				}

				return null;
			}

			return null;
		}

		protected virtual IEntryHeaderValidationDecider GetImportValidationDecider() => null;

		protected virtual IEntryHeaderValidationDecider GetExportValidationDecider() => null;
	}
}
