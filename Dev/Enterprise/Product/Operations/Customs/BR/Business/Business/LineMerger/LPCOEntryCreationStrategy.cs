namespace Enterprise.Customs.BR.Business
{
	public class LPCOEntryCreationStrategy : EntryCreationStrategy
	{
		public LPCOEntryCreationStrategy(JobDeclaration declaration)
			: base(declaration, MessageTypeList.Codes.LPC)
		{
		}

		protected override bool IsActiveCore => Declaration.IsLPCO;
	}
}
