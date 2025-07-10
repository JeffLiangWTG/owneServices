namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class PostDateConfigLookupsTest : PostDateConfigurationLookupsTest
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