namespace Enterprise.Accounting.Module.Testing
{
	public class ARMatchingFilterControlTestCase : MatchingFilterControlTestCase
	{
		protected override MatchingBaseFilterBusinessObject GetTestFilterBizO()
		{
			return new ARMatchingFilterBusinessObject();
		}
	}
}
