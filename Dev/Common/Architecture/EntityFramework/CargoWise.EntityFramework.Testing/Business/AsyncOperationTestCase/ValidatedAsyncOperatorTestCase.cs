using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	[TestsSubclassesOf(typeof(IValidatedAsyncOperator))]
	public abstract class ValidatedAsyncOperatorTestCase<TOperator, TReal, TSnapshot> : TestCaseWithFactory
		where TOperator : ValidatedAsyncOperator<TReal, TSnapshot>
	{
		protected abstract IValidatedAsyncOperator GetNewOperator();

		protected abstract TSnapshot GetExpectedInitialSnapshot();

		protected abstract TSnapshot GetExpectedResultSnapshot();

		protected abstract TReal GetNewInitialReal();

		protected abstract TReal GetRealWithIncompatibleChanges();

		protected abstract TReal GetExpectedReal();

		protected virtual bool SnapshotEquals(TSnapshot first, TSnapshot second)
		{
			return first.Equals(second);
		}

		protected virtual bool RealEquals(TReal first, TReal second)
		{
			return first.Equals(second);
		}

		public void TestOperatorIsExpected()
		{
			AssertEquals("Async operator is the expected type", true, GetNewOperator() is TOperator);
		}

		public void TestInitialSnapshot_MatchesExpected()
		{
			var real = GetNewInitialReal();
			var op = GetNewOperator();
			var initialSnapshot = op.GetSnapshot(real);

			AssertEquals("Initial snapshot is as expected", true, SnapshotEquals((TSnapshot)initialSnapshot, GetExpectedInitialSnapshot()));
		}

		public void TestInitialSnapshot_AssumptionsValid()
		{
			var real = GetNewInitialReal();
			var op = GetNewOperator();
			var initialSnapshot = op.GetSnapshot(real);

			AssertEquals("Initial snapshot can be mapped to data source", true, op.AssumptionsOfInitialSnapShotValid(real, (TSnapshot)initialSnapshot));
		}

		public void TestInitialSnapshot_AssumptionsValid_AfterTransform()
		{
			var real = GetNewInitialReal();
			var op = GetNewOperator();
			var initialSnapshot = op.GetSnapshot(real);
			var expectedSnapshot = op.TransformSnapshot(initialSnapshot);

			AssertEquals("Initial snapshot can be mapped to data source", true, op.AssumptionsOfInitialSnapShotValid(real, initialSnapshot));
		}

		public void TestInitialSnapshot_AssumptionsInvalid()
		{
			var real = GetNewInitialReal();
			var op = GetNewOperator();
			var initialSnapshot = op.GetSnapshot(real);

			AssertEquals("Initial snapshot cannot be mapped to data source after invalid changes", false, op.AssumptionsOfInitialSnapShotValid(GetRealWithIncompatibleChanges(), initialSnapshot));
		}

		public void TestExpectedSnapshot_MatchesExpected()
		{
			var real = GetNewInitialReal();
			var op = GetNewOperator();
			var initialSnapshot = op.GetSnapshot(real);
			var resultSnapshot = op.TransformSnapshot(initialSnapshot);

			AssertEquals("Snapshot after transform is as expected", true, SnapshotEquals((TSnapshot)resultSnapshot, GetExpectedResultSnapshot()));
		}

		public void TestExpectedResult()
		{
			var real = GetNewInitialReal();
			var op = GetNewOperator();
			var initialSnapshot = op.GetSnapshot(real);
			var resultSnapshot = op.TransformSnapshot(initialSnapshot);

			op.MapSnapshot(real, resultSnapshot);

			AssertEquals("The end result of a transformation should be different to the start", false, RealEquals(real, GetNewInitialReal()));
			AssertEquals("The end result should have the expected transformation", true, RealEquals(real, GetExpectedReal()));
		}

		public void TestRunTransfom_Synchronous()
		{
			var initial = GetNewInitialReal();
			var op = GetNewOperator();

			var justDoItAction = new Action<Action>(a => a());
			op.RunTransform(initial, new SynchronousActionExecutionStrategy());

			AssertEquals("One transform has taken place, and the end result should be as expected", true, RealEquals(initial, GetExpectedReal()));
		}

		public void TestRunTransfom_RunTwice_SecondMapIsSafe()
		{
			var initial = GetNewInitialReal();
			var op = GetNewOperator();

			var invokableStrategy = new InvokableExecutionStrategy();
			op.RunTransform(initial, invokableStrategy);
			op.RunTransform(initial, new SynchronousActionExecutionStrategy());

			AssertEquals("One transform has taken place, and the end result should be as expected", true, RealEquals(initial, GetExpectedReal()));

			AssertNoExceptionThrown(() => invokableStrategy.InvokableAction());

			AssertEquals("The second transform shouldn't have had results that are different to the first", true, RealEquals(initial, GetExpectedReal()));
		}

		public void TestRunTransfom_Cancellation()
		{
			var initial = GetNewInitialReal();
			var op = GetNewOperator();

			var invokableStrategy = new InvokableExecutionStrategy();
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.Cancel();

			AssertExceptionThrown<OperationCanceledException>(() => op.RunTransform(initial, new SynchronousActionExecutionStrategy(), cancellationTokenSource));
		}

		public void TestRunMap_CancellationPreventsMap()
		{
			var initial = GetNewInitialReal();
			var op = GetNewOperator();

			var invokableStrategy = new InvokableExecutionStrategy();
			var cancellationTokenSource = new CancellationTokenSource();
			op.RunTransform(initial, invokableStrategy, cancellationTokenSource);

			AssertEquals("The map hasn't run", true, RealEquals(initial, GetNewInitialReal()));

			cancellationTokenSource.Cancel();
			AssertNoExceptionThrown(() => invokableStrategy.InvokableAction());

			AssertEquals("Map didn't run, so the transform didn't run.", true, RealEquals(initial, GetNewInitialReal()));
		}

		public void TestFactoryHandover_RunThreadSentryBasherTwice_WhileAccessingUserOnOtherThread()
		{
			// simulate non-relinquished factory by getting UserContext in another thread
			var task = Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var env = ObjectFactory.Get<IEnv>();
					var currentUser = env.Instance.CurrentUserContext;
				}
			});

			Task.WaitAll(task);

			TestFactoryHandover_RunThreadSentryBasherTwice();
		}

		public void TestFactoryHandover_RunThreadSentryBasherTwice()
		{
			BashThreadSentry(1);
			BashThreadSentry(2);
		}

		void BashThreadSentry(int runInstance)
		{
			var initial = GetNewInitialReal();
			var op = GetNewOperator();

			AssertNoExceptionThrown(string.Format("No cross thread error reports should occur here on run {0}", runInstance), () => op.RunTransform(initial, new TestFactoryHandoverStrategy(Factory)));

			AssertEquals(string.Format("And also the transform succeeded on run {0}", runInstance), true, RealEquals(initial, GetExpectedReal()));
		}

		#region Test Strategies

		public class TestFactoryHandoverStrategy : IActionExecutionStrategy
		{
			public TestFactoryHandoverStrategy(BusinessObjectFactory factory)
			{
				Factory = factory;
				staticallyCachedFactories = GetAllFactories();
			}

			readonly IEnumerable<BusinessObjectFactory> staticallyCachedFactories;

			public BusinessObjectFactory Factory { get; private set; }

			void IActionExecutionStrategy.StartOperation(Action action)
			{
				// We simulate execution of the action in the main thread
				action();
			}

			void IActionExecutionStrategy.DoParallelisableTransform(Action action, CancellationTokenSource cancellationTokenSource)
			{
				// We simulate execution of the action in a background thread:
				// by relinquishing ownership we ensure that all the factories which were possibly created in StartOperation() are not hit while executing the action
				foreach (var factory in GetFactories())
				{
					if (factory.ThreadSentry.IsOwner)
					{
						factory.ThreadSentry.ForciblyRelinquishThreadOwnership_ForTest();
					}
				}

				action();
			}

			void IActionExecutionStrategy.SynchroniseIntoMainContext(Action action)
			{
				// We simulate execution of the action in the main thread again:
				// by relinquishing ownership we ensure that all the factories which were possibly created in DoParallelisableTransform() are not hit while executing the action
				// by taking ownership back we permit using of all the factories which were possibly created in StartOperation()
				foreach (var factory in GetFactories())
				{
					if (factory.ThreadSentry.IsOwner)
					{
						factory.ThreadSentry.ForciblyRelinquishThreadOwnership_ForTest();
					}
					else if (!factory.ThreadSentry.IsOwner)
					{
						factory.ThreadSentry.TakeThreadOwnership();
					}
				}

				action();
			}

			IEnumerable<BusinessObjectFactory> GetFactories()
			{
				return GetAllFactories().Except(staticallyCachedFactories);
			}

			IEnumerable<BusinessObjectFactory> GetAllFactories()
			{
				return PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Where(f => f.CrossThreadErrorReportingEnabled);
			}
		}

		public class InvokableExecutionStrategy : IActionExecutionStrategy
		{
			void IActionExecutionStrategy.StartOperation(Action action)
			{
				action();
			}

			void IActionExecutionStrategy.DoParallelisableTransform(Action action, CancellationTokenSource cancellationTokenSource)
			{
				action();
			}

			public Action InvokableAction { get; set; }

			void IActionExecutionStrategy.SynchroniseIntoMainContext(Action action)
			{
				InvokableAction = action;
			}
		}

		#endregion
	}
}
