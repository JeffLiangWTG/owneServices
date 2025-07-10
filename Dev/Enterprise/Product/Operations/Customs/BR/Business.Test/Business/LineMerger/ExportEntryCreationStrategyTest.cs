using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ExportEntryCreationStrategyTest : EntryCreationStrategyTest<ExportEntryCreationForTesting>
	{
		protected override ExportEntryCreationForTesting CreateNewEntryCreationStrategy(JobDeclaration declaration)
		{
			return new ExportEntryCreationForTesting(declaration);
		}

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.Export;

		protected override ZString ExpectedMessageTypeToNewEntryHeader => MessageTypeList.Codes.CDE;
	}

	class ExportEntryCreationForTesting : ExportEntryCreationStrategy, IEntryCreationStrategyForTesting
	{
		public ExportEntryCreationForTesting(JobDeclaration declaration) : base(declaration)
		{
		}

		public bool UseAdditionalEntryLineLinksExposed => base.UseAdditionalEntryLineLinks;

		public bool IsActiveExposed => base.IsActiveCore;

		public string MessageTypeToNewEntryHeaderExposed => base.CH_MessageTypeToNewEntryHeader;

		public void ClearReferenceToEntryLineWhenLineIsNotValidForMergeExposed(BaseJobComInvoiceLine invoiceLine) => ClearReferenceToEntryLineWhenLineIsNotValidForMerge(invoiceLine);
	}
}
