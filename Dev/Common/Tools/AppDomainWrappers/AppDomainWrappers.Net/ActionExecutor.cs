using System;

namespace AppDomainWrappers.Net
{
	[Serializable]
	public class ActionExecutor : MarshalByRefObject
	{
		public void Execute(Action action)
		{
			action?.Invoke();
		}
	}
}
