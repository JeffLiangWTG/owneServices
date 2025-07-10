using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class MutexIDsTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestEnsureUniqueness()
		{
			FieldInfo[] fields = typeof(MutexIDs).GetFields(BindingFlags.Static | BindingFlags.Public);
			List<string> mutexNames = new List<string>();

			foreach (FieldInfo field in fields)
			{
				if (field.DeclaringType == typeof(MutexID))
				{
					string mutexName = ((MutexID)field.GetValue(null)).Name;
					if (!mutexNames.Contains(mutexName))
					{
						mutexNames.Add(mutexName);
					}
					else
					{
						Fail("Mutex : " + mutexName + " is duplicated. Please ensure that they are unique");
					}
				}
			}
		}
	}
}
