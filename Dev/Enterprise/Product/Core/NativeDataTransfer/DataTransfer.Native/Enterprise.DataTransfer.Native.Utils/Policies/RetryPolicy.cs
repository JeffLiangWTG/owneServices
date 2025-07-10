using System;

namespace Enterprise.DataTransfer.Native.Utils.Policies
{
	public class RetryPolicy<TException> where TException : Exception
	{
		public RetryPolicy()
		{
			this.policy = this.Retry;
		}

		public IRetryState RetryState { get; set; }

		public int RetryCount { get; set; }

		readonly Action<Action> policy;

		public void Do(Action action)
		{
			policy(action);
		}

		public TResult Do<TResult>(Func<TResult> action)
		{
			var result = default(TResult);
			policy(() => result = action());
			return result;
		}

		public void Retry(Action action)
		{
			RetryState = new CountableRetryState(RetryCount);
			while (true)
			{
				try
				{
					action();
					return;
				}
				catch (TException ex)
				{
					if (!RetryState.CanRetry(ex))
					{
						throw;
					}
				}
			}
		}
	}

	public sealed class CountableRetryState : IRetryState
	{
		int errorCount;
		readonly Predicate<int> canRetry;

		public CountableRetryState(int retryCount)
		{
			this.canRetry = i => errorCount <= retryCount;
		}

		public bool CanRetry(Exception ex)
		{
			errorCount++;

			return canRetry(errorCount);
		}
	}

	public interface IRetryState
	{
		bool CanRetry(Exception ex);
	}
}
