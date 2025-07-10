using System;

namespace Enterprise.Accounting.Business
{
	public static class AccountingSuspenders
	{
		public interface IRunMethodSuspending
		{
			bool RunMethodSuspended
			{
				get;
				set;
			}
		}

		public class RunMethodSuspender : IDisposable
		{
			public RunMethodSuspender(IRunMethodSuspending parent, MethodDelegate method)
			{
				Parent = parent;
				Method = method;
			}

			public void RunMethod()
			{
				if (Parent != null && !Parent.RunMethodSuspended && Method != null)
				{
					Parent.RunMethodSuspended = true;
					WasSuspendingActivatedHere = true;
					Method();
				}
			}

			public void Dispose()
			{
				if (WasSuspendingActivatedHere)
				{
					Parent.RunMethodSuspended = false;
				}
			}

			public delegate void MethodDelegate();

			readonly IRunMethodSuspending Parent;
			readonly MethodDelegate Method;
			bool WasSuspendingActivatedHere;
		}
	}
}