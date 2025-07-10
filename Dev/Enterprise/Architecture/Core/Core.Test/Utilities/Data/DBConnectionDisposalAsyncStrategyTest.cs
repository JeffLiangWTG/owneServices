using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Async;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class DBConnectionDisposalAsyncStrategyTest : TestCase
	{
		public void TestThreadsFinishing_ShouldDisposeExtraConnection()
		{
			var methods = typeof(IAsyncStrategy).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name != nameof(IAsyncStrategy.GetAutoRefresher));
			AssertEquals(4, methods.Count());

			foreach (var method in methods)
			{
				StartThreadAndAssertConnectionDisposedAfterThreadCompletes(method);
			}
		}

		void StartThreadAndAssertConnectionDisposedAfterThreadCompletes(MethodInfo strategyMethod)
		{
			var mainConnection = Db.Connection;
			mainConnection.EnsureIsOpen(); //may have been closed by the actions of the previous unit test to run
			var backgroundThreadConnection = default(DbConnection);
			strategy.ewh = new AutoResetEvent(false);

			var action = new Action(() =>
			{
				backgroundThreadConnection = Db.Connection;
				backgroundThreadConnection.EnsureIsOpen();
			});

			InvokeMethod(strategyMethod, action);

			strategy.ewh.WaitOne();

			CombineAssertions(strategyMethod.Name, () =>
			{
				AssertNotEquals("Background thread should have its own db connection", mainConnection, backgroundThreadConnection);
				AssertEquals("Background thread connection should be closed after thread finished executing",
					ConnectionState.Closed, strategy.ConnectionState);
				AssertEquals("Main connection should still be open", ConnectionState.Open, mainConnection.State);
			});
		}

		void InvokeMethod(MethodInfo strategyMethod, Action action)
		{
			object[] parameters;

			if (strategyMethod.Name == nameof(IAsyncStrategy.ParallelForEach))
			{
				strategyMethod = strategyMethod.MakeGenericMethod(typeof(int));
				parameters = new object[] { new[] { 1, 2, 3 }, new Action<int>(i => action()) };
			}
			else if (strategyMethod.IsGenericMethodDefinition)
			{
				strategyMethod = strategyMethod.MakeGenericMethod(typeof(object));
				parameters = new object[] { new Func<object>(() =>
				{
					action();
					return null;
				}), null, "InvokeMethod", null };
			}
			else if (strategyMethod.Name == nameof(IAsyncStrategy.DoAsyncAsThread))
			{
				parameters = new object[] { action, ApartmentState.STA, null, "InvokeMethod" };
			}
			else
			{
				parameters = new object[] { action, null, "InvokeMethod" };
			}

			strategyMethod.Invoke(strategy, parameters);
		}

		DbConnectionDisposalAsyncStrategyForTest strategy;

		protected override void SetUp()
		{
			base.SetUp();
			strategy = DbConnectionDisposalAsyncStrategyForTest.Get();
		}
	}
}
