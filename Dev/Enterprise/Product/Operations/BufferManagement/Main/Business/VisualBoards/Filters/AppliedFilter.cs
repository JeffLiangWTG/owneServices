namespace Enterprise.BufferManagement.Business
{
	public class AppliedFilter
	{
		public AppliedFilter(IFilterApplicator applicator, bool isApplicable)
		{
			Applicator = applicator;
			IsApplicable = isApplicable;
		}

		public IFilterApplicator Applicator { get; private set; }
		public bool IsApplicable { get; private set; }
	}
}
