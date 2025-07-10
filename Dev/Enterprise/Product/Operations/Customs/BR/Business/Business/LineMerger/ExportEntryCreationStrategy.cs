namespace Enterprise.Customs.BR.Business
{
	public class ExportEntryCreationStrategy : EntryCreationStrategy
	{
		public ExportEntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, MessageTypeList.Codes.CDE)
		{
		}

		protected override bool IsActiveCore => Declaration.IsExport;
	}
}
