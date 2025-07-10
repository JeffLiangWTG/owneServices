using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	class DWeakTargetDelegateListTest : TestCase
	{
		public void TestList()
		{
			AssertEquals("Initially", 0, new List<WeakTargetDelegate<EventHandler>>(List).Count);
			AssertEquals("IsEmpty initially", true, List.IsEmpty);
			AddUnreferencedInstance();
			GC.Collect();
			AssertEquals("Handlers collected", 0, new List<WeakTargetDelegate<EventHandler>>(List).Count);
			AssertEquals("IsEmpty after collection", true, List.IsEmpty);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		void AddUnreferencedInstance()
		{
			var instance = new InstanceClass();
			List.Add(new EventHandler(instance.OnInstanceEvent));
			List.Add(new EventHandler(instance.OnInstanceEvent));
			AssertEquals("With handlers", 2, new List<WeakTargetDelegate<EventHandler>>(List).Count);
			List.Remove(new EventHandler(instance.OnInstanceEvent));
			AssertEquals("With handlers", 1, new List<WeakTargetDelegate<EventHandler>>(List).Count);
			AssertEquals("IsEmpty with handlers", false, List.IsEmpty);
		}

		class InstanceClass
		{
			public void OnInstanceEvent(object sender, EventArgs e)
			{
			}
		}

		readonly WeakTargetDelegateList<EventHandler> List = new WeakTargetDelegateList<EventHandler>();
	}
}