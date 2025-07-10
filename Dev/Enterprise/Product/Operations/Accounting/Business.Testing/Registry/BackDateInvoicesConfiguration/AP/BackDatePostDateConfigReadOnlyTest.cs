namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class BackDatePostDateConfigReadOnlyTest : PostDateConfigurationReadOnlyTest
	{
		protected override PostDateConfiguration GetNewBizObj
		{
			get
			{
				return new PostDateConfiguration();
			}
		}
	}
}