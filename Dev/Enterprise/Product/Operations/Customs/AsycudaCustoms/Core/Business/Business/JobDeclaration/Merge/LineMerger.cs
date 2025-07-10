using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies() => new Customs.Business.EntryCreationStrategy[] { new EntryCreationStrategy(Declaration) };

		protected override void PerformCountrySpecificOperationAfterMergeAfterCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeAfterCalculateDuty();

			foreach (CusEntryHeader entryHeader in Declaration.CustomsEntryHeaders)
			{
				var localReferenceNumber = entryHeader.EntryInstruction?.ASY_LocalReferenceNumber ?? ZString.Empty;
				if (!localReferenceNumber.IsEmpty && entryHeader.CH_BGMReference != localReferenceNumber)
				{
					entryHeader.CH_BGMReference = localReferenceNumber;
				}
			}
		}

		protected override IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);
	}
}
