using CargoWise.Data;
using Enterprise.ZArchitecture.Web.Common;

namespace Enterprise.ZArchitecture.Web.GlobalBase
{
	public class BaseWebDbEnvironment : BaseDbEnvironment
	{
		public override IConnectionPooling ConnectionPooling { get; } = new WebConnectionPooling();

		public override IDbConnectionGuiPlugin ConnectionGuiPlugin { get; } = new WebDbConnectionGuiPlugin();

		public override bool IsServingWebBasedApp => true;

		public class WebConnectionPooling : DefaultConnectionPooling
		{
			public override int MaxPoolSize => 1000;

			public override int MinPoolSize => 0;
		}
	}
}
