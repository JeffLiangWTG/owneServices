using System;
using System.Threading;

namespace CargoWise.Common
{
	public static class SynchronizationContextExtensionMethods
	{
		public static void BeginInvoke(this SynchronizationContext synchronizationContext, Action method)
		{
			synchronizationContext.Post(_ => method(), null);
		}

		public static void BeginInvoke(this SynchronizationContext synchronizationContext, Delegate method, params object[] args)
		{
			synchronizationContext.Post(a => method.DynamicInvoke((object[])a), args);
		}

		public static T Invoke<T>(this SynchronizationContext synchronizationContext, Func<T> method)
		{
			T result = default(T);
			synchronizationContext.Send(_ => result = method(), null);
			return result;
		}

		public static void Invoke(this SynchronizationContext synchronizationContext, Action method)
		{
			synchronizationContext.Send(_ => method(), null);
		}

		public static object Invoke(this SynchronizationContext synchronizationContext, Delegate method, params object[] args)
		{
			var result = default(object);
			synchronizationContext.Send(a => result = method.DynamicInvoke((object[])a), args);
			return result;
		}
	}
}
