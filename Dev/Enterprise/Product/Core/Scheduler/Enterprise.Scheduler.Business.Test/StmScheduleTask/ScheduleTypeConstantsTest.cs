using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Scheduler.Business.Testing
{
	sealed class ScheduleTypeConstantsTest : TestCase
	{
		public void TestLinkTypesNotDuplicated()
		{
			FieldInfo[] fields = typeof(ScheduleTypeConstants).GetFields(BindingFlags.Public | BindingFlags.Static);
			List<string> serviceTypes = new List<string>();
			foreach (FieldInfo field in fields)
			{
				string serviceType = field.GetValue(null).ToString();
				AssertCollectionNotContains("ServiceType code " + serviceType + " is duplicated", serviceType, serviceTypes);
				serviceTypes.Add(serviceType);
			}
		}
	}
}
