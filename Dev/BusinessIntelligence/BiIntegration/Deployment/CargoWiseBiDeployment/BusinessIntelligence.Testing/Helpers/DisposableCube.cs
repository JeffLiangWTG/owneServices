using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Bi.Deployment.AnalysisServices;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Bi.BusinessIntelligence.Testing
{
	public class DisposableCube : Disposable
	{
		public DisposableCube(SsasServer connection, string cubeName)
		{
			this.connection = connection;
			this.cubeName = cubeName;

			RestoreCube();
		}

		readonly SsasServer connection;
		readonly string cubeName;
		readonly Stack<IDisposable> actionsBeforeDispose = new Stack<IDisposable>();
		void RestoreCube()
		{
			SsasHelper.RestoreModelBackup(connection, cubeName);
		}

		public void AddDisposable(IDisposable resource)
		{
			actionsBeforeDispose.Push(resource);
		}

		void PreDispose()
		{
			while (actionsBeforeDispose.Count > 0)
			{
				var resource = actionsBeforeDispose.Pop();
				resource?.Dispose();
			}
		}

		#region Implementation Base
		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				PreDispose();
				if (connection.DatabaseExists(cubeName))
				{
					connection.DropModel(cubeName);
				}
			}
		}
		#endregion
	}

	class DisposableCubeTest : TestCase
	{
		static StringBuilder resultString;
		public void TestAddDisposable()
		{
			resultString = new StringBuilder();
			int order = 3;
			using (var connection = SsasServer.New(SsasHelper.AnalysisServerName))
			using (var disposableCubeForTest = new DisposableCubeForTest(connection, string.Empty))
			{
				while (order-- > 0)
				{
					int i = order;
					disposableCubeForTest.AddDisposable(new DisposableAction(() => { resultString.Append(i); }));
				}
			}

			AssertEquals("First added dispose action should be the last invoked.", "012", resultString.ToString());
		}

		class DisposableCubeForTest : DisposableCube
		{
			public DisposableCubeForTest(SsasServer connection, string cubeName) : base(connection, cubeName)
			{
			}
		}
	}
}
