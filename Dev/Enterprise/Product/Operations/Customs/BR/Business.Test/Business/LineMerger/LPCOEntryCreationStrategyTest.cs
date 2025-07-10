using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class LPCOEntryCreationStrategyTest : EntryCreationStrategyTest<LPCOEntryCreationForTesting>
	{
		protected override LPCOEntryCreationForTesting CreateNewEntryCreationStrategy(JobDeclaration declaration)
		{
			return new LPCOEntryCreationForTesting(declaration);
		}

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.LPCO;

		protected override ZString ExpectedMessageTypeToNewEntryHeader => MessageTypeList.Codes.LPC;
	}

	class LPCOEntryCreationForTesting : LPCOEntryCreationStrategy, IEntryCreationStrategyForTesting
	{
		public LPCOEntryCreationForTesting(JobDeclaration declaration) : base(declaration)
		{
		}

		public bool UseAdditionalEntryLineLinksExposed => base.UseAdditionalEntryLineLinks;

		public bool IsActiveExposed => base.IsActiveCore;

		public string MessageTypeToNewEntryHeaderExposed => base.CH_MessageTypeToNewEntryHeader;

		public void ClearReferenceToEntryLineWhenLineIsNotValidForMergeExposed(BaseJobComInvoiceLine invoiceLine) => ClearReferenceToEntryLineWhenLineIsNotValidForMerge(invoiceLine);
	}
}
