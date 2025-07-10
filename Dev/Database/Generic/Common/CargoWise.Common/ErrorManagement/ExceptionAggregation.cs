using System;
using System.Runtime.ExceptionServices;

namespace CargoWise.Common.ErrorManagement
{
	public static class ExceptionAggregation
	{
		public static void Using(IDisposable disposeable, Action action)
		{
			try
			{
				action();
			}
			catch (Exception bodyActionEx)
			{
				try
				{
					disposeable.Dispose();
				}
				catch (Exception finallyEx)
				{
					throw new AggregateException(bodyActionEx, finallyEx);
				}

				ExceptionDispatchInfo.Capture(bodyActionEx).Throw();
			}

			disposeable.Dispose();
		}

		public static void ExecuteWithFinally(Action bodyAction, Action finallyAction)
		{
			try
			{
				bodyAction();
			}
			catch (Exception bodyActionEx) when (!bodyActionEx.IsCriticalException())
			{
				try
				{
					finallyAction();
				}
				catch (Exception finallyEx) when (!finallyEx.IsCriticalException())
				{
					throw new AggregateException(bodyActionEx, finallyEx);
				}

				ExceptionDispatchInfo.Capture(bodyActionEx).Throw();
			}

			finallyAction();
		}
	}
}
