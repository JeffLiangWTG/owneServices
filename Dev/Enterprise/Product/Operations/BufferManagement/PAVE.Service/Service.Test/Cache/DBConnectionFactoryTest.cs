using System.Collections.Generic;
using System.Data;
using System.Threading;
using Enterprise.BufferManagement.Service.Cache;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Test.Cache
{
	public class DBConnectionFactoryTest : TestCase
	{
		public void TestRetryGetConnection()
		{
			var connectionFactory = new DBConnectionFactory();
			using var disposable = DBConnectionFactory.SetMaxPoolSizeForTest(1);

			var threads = new List<Thread>();
			int numberOfThreads = 15;

			for (int i = 0; i < numberOfThreads; i++)
			{
				var thread = new Thread(() =>
				{
					IDbConnection cn = null;
					AssertNoExceptionThrown(() => cn = connectionFactory.CreateOpenedConnection());
					Thread.Sleep(2000);
					using var command = cn.CreateCommand();
					command.CommandText = "Select 1";
					command.ExecuteNonQuery();
					cn.Close();
				});

				threads.Add(thread);
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}
		}
	}
}
