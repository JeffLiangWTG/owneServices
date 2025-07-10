using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class WeakTargetDelegateTest : TestCase
	{
		public void TestInstanceHandler()
		{
			var handlerRef = CreateHandlerToUnreferencedInstance(out var instanceRef);
			GC.Collect();
			AssertEquals("Instance should be collected", false, instanceRef.IsAlive);
			AssertEquals("Handler should be collected", false, handlerRef.IsAlive);
			AssertNull("Handler should be collected", handlerRef.ToDelegate());
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakTargetDelegate<EventHandler> CreateHandlerToUnreferencedInstance(out WeakReference instanceRef)
		{
			var instance = new InstanceClass();
			instanceRef = new WeakReference(instance);
			var handlerRef = new WeakTargetDelegate<EventHandler>(new EventHandler(instance.OnInstanceEvent));
			AssertEquals("Instance should be collected", true, instanceRef.IsAlive);
			AssertEquals("Handler should be collected", true, handlerRef.IsAlive);
			handlerRef.ToDelegate()(null, null);
			AssertNotNull("Handler should exist", handlerRef.ToDelegate());
			return handlerRef;
		}

		public void TestStaticHandler()
		{
			var handlerRef = new WeakTargetDelegate<EventHandler>(new EventHandler(OnStaticEvent));
			AssertEquals(true, handlerRef.IsAlive);
			AssertEquals("Handler is alive", true, handlerRef.IsAlive);
			AssertNotNull("Handler should exist", handlerRef.ToDelegate());
			handlerRef.ToDelegate()(null, null);
			GC.Collect();
			AssertEquals("Handler is alive", true, handlerRef.IsAlive);
			AssertNotNull("Handler should exist", handlerRef.ToDelegate());
		}

		class InstanceClass
		{
			public void OnInstanceEvent(object sender, EventArgs e)
			{
			}
		}

		static void OnStaticEvent(object sender, EventArgs e)
		{
		}
	}
}