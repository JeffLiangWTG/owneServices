using System;
using System.Threading;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation.Testing
{
	class TestTransformation : DataTransformation, IOnlineTransformation
	{
		public override string UserDescription => "Description";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			// must be overridden to trigger 'acceptance' that IOnlineTransformation is worth calling
		}

		public virtual void Run(Action<string> logInformation, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
		}
	}
}
