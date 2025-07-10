namespace Enterprise.Accounting.Module.Testing
{
	public class APMatchingFilterControlTestCase : MatchingFilterControlTestCase
	{
		protected override MatchingBaseFilterBusinessObject GetTestFilterBizO()
		{
			return new APMatchingFilterBusinessObject();
		}
	}
}
