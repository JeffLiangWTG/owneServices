using System;
using System.Collections.Generic;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework.Testing
{
	[CodeAlive("This is used by reflection code in UnitTestRunner.cs.")]
	public sealed class ClearExtraSubscriptionListener : BaseTestListener
	{
		public override void StartTest(TestCase test, DateTime startTime)
		{
			base.StartTest(test, startTime);
			SaveSubscriptionsSnapshot();
		}

		public override void EndTest(TestCase test, DateTime endTime)
		{
			base.EndTest(test, endTime);
			RestoreSubscriptionsSnapshot();
		}

		#region Implementation

		void SaveSubscriptionsSnapshot()
		{
			factoriesSnapShot.Clear();
			foreach (var factory in DataRefreshManager.Bus.Factories)
			{
				factoriesSnapShot.Add(new WeakReference(factory));
			}
		}

		void RestoreSubscriptionsSnapshot()
		{
			var bus = DataRefreshManager.Bus;
			foreach (var factoryInBus in bus.Factories)
			{
				var found = false;
				foreach (var factoryInSnapShot in factoriesSnapShot)
				{
					if (factoryInBus == factoryInSnapShot.Target)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					bus.Subscriptions.RemoveFactory(factoryInBus);
				}
			}

			factoriesSnapShot.Clear();
		}

		readonly List<WeakReference> factoriesSnapShot = new List<WeakReference>();

		#endregion
	}
}
