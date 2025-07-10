using System;
using CargoWise.Data;
using Moq;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	class DbEnvironmentWithMockGuiPlugin : BaseDbEnvironment, IDisposable
	{
		public DbEnvironmentWithMockGuiPlugin()
		{
			existingEnv = DbEnv.Instance;
			DbEnv.SetDbEnvironment(this);
		}

		public override IDbConnectionGuiPlugin ConnectionGuiPlugin => connectionGuiPlugin;
		readonly IDbConnectionGuiPlugin connectionGuiPlugin = Mock.Of<IDbConnectionGuiPlugin>();

		public void Dispose() => DbEnv.SetDbEnvironment(existingEnv);
		readonly IDbEnvironment existingEnv;
	}
}
