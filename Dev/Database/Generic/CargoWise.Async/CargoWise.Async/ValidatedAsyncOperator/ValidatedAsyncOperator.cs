using System;
using System.Threading;
using CargoWise.Common;

namespace CargoWise.Async
{
	public abstract class ValidatedAsyncOperator<TReal, TSnapshot> : IValidatedAsyncOperator
	{
		protected abstract TSnapshot GetSnapshot(TReal source);

		protected abstract TSnapshot TransformSnapshot(TSnapshot initialSnapshot, CancellationTokenSource cancellationTokenSource = null);

		protected abstract bool AssumptionsOfInitialSnapShotValid(TReal real, TSnapshot initialSnapshot);

		protected abstract void MapSnapshot(TReal newReal, TSnapshot newSnapshot);

		#region IValidatedAsyncOperator

		object IValidatedAsyncOperator.GetSnapshot(object source)
		{
			return GetSnapshot((TReal)source);
		}

		object IValidatedAsyncOperator.TransformSnapshot(object initialSnapshot, CancellationTokenSource cancellationTokenSource)
		{
			return TransformSnapshot((TSnapshot)initialSnapshot, cancellationTokenSource);
		}

		bool IValidatedAsyncOperator.AssumptionsOfInitialSnapShotValid(object real, object initialSnapshot)
		{
			return AssumptionsOfInitialSnapShotValid((TReal)real, (TSnapshot)initialSnapshot);
		}

		void IValidatedAsyncOperator.MapSnapshot(object newReal, object newSnapshot)
		{
			MapSnapshot((TReal)newReal, (TSnapshot)newSnapshot);
		}

		#endregion
	}

	public static class IValidatedAsyncOperatorExtensions
	{
		public static void RunTransform(this IValidatedAsyncOperator @operator, object source, IActionExecutionStrategy executionStrategy, CancellationTokenSource cancellationTokenSourceOverride = null)
		{
			Argument.NotNull(executionStrategy, nameof(executionStrategy));
			Argument.NotNull(source, nameof(source));
			Argument.NotNull(@operator, nameof(@operator));

			var cancellationTokenSource = cancellationTokenSourceOverride ?? new CancellationTokenSource();
			@operator.RunTransform(source, executionStrategy.StartOperation, executionStrategy.DoParallelisableTransform, executionStrategy.SynchroniseIntoMainContext, cancellationTokenSource, cancellationTokenSourceOverride == null);
		}

		static void RunTransform(this IValidatedAsyncOperator @operator, object source, Action<Action> startOperation, Action<Action, CancellationTokenSource> invokeTransform, Action<Action> invokeSynchronise, CancellationTokenSource cancellationTokenSource, bool shouldDisposeCancellationToken)
		{
			Argument.NotNull(startOperation, nameof(startOperation));
			Argument.NotNull(@operator, nameof(@operator));

			// startOperation is intended to run in the main thread to prepare some data (snapshot) for further calculations related to the source object
			startOperation(() =>
			{
				var snapshot = @operator.GetSnapshot(source);
				if (snapshot != null)
				{
					// invokeTransform is intended to run in a background thread to perform some time consuming calculations
					invokeTransform(new Action(() =>
					{
						var transformedSnapshot = @operator.TransformSnapshot(snapshot, cancellationTokenSource);

						// invokeSynchronise is intended to run in the main thread to merge the results of the calculations back to the source object
						invokeSynchronise(new Action(() =>
						{
							var shouldCancelMap = cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested; // Even if the map has been queued, cancelling prevents map.
							// we need to ensure that the source object did not change while we were performing the calculations
							if (!shouldCancelMap && @operator.AssumptionsOfInitialSnapShotValid(source, snapshot))
							{
								// merging the results of the calculations back to the source object
								@operator.MapSnapshot(source, transformedSnapshot);
							}
							if (shouldDisposeCancellationToken)
							{
								cancellationTokenSource.Dispose();
							}
						}));
					}), cancellationTokenSource);
				}
			});
		}
	}
}
