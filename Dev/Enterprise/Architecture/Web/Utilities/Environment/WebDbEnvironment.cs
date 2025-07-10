using CargoWise.Data;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment
{
	public class WebDbEnvironment : BaseDbEnvironment
	{
		public override IConnectionPooling ConnectionPooling { get; } = new WebConnectionPooling();

		public override bool IsServingWebBasedApp => true;

		public class WebConnectionPooling : DefaultConnectionPooling
		{
			public override int MaxPoolSize => 1000;

			public override int MinPoolSize => 0;
		}
	}
}
