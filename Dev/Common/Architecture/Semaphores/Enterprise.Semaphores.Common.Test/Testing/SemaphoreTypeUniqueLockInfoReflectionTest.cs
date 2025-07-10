using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.BuildTools;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Semaphores.Common.Testing
{
	class SemaphoreTypeUniqueLockInfoReflectionTest : TestCase
	{
		public void TestSemaphoreTypeLockInfoIsUnique()
		{
			StringBuilder errorMessage = new StringBuilder();
			errorMessage.AppendLine("All classes inherited of ISemaphoreType should have unique LockInfo.");
			errorMessage.AppendLine("The following types have the same LockInfo:");

			var semaphores = GetISemaphoreTypes();

			var isUnique = true;
			for (var i = 0; i < semaphores.Length - 1; i++)
			{
				for (var j = i + 1; j < semaphores.Length; j++)
				{
					if (semaphores[i].LockInfo == semaphores[j].LockInfo)
					{
						errorMessage.AppendLine(string.Format("{0} and {1} have the same {2} LockInfo",
							semaphores[i].GetType(), semaphores[j].GetType(), semaphores[i].LockInfo));
						isUnique = false;
					}
				}
			}

			Assert(errorMessage.ToString(), isUnique);
		}

		ISemaphoreType[] GetISemaphoreTypes()
		{
			var semaphores = new List<ISemaphoreType>();
			var iSemaphoreType = typeof(ISemaphoreType);
			var assemblyNames = GetEnterpriseAssemblies();
			var unableToCreateTypeMessages = new ZStringBuilder();
			foreach (string assembleName in assemblyNames)
			{
				var assembly = Assembly.Load(assembleName);
				Type[] types;
				try
				{
					types = assembly.GetTypes();
				}
				catch
				{
					continue;
				}

				foreach (var type in types)
				{
					if (!type.IsInterface && iSemaphoreType.IsAssignableFrom(type))
					{
						if (type.GetConstructor(Type.EmptyTypes) != null)
						{
							try
							{
								semaphores.Add((ISemaphoreType)Activator.CreateInstance(type));
							}
							catch (MissingMethodException ex)
							{
								unableToCreateTypeMessages.AppendLine($"Cannot create {type.FullName}, " + ex.Message);
							}
						}
						else
						{
							var helpers = ObjectFactory.Get<Hashtable>("ISemaphoreTypeWithoutParameterLessContructorTestHelpers");
							var objectHandle = (ObjectHandle)helpers[type.Name];
							if (objectHandle == null)
							{
								unableToCreateTypeMessages.AppendLine($"Unable to create {type.FullName}, please implement ISemaphoreTypeWithoutParameterLessContructorTestHelper and add to ISemaphoreTypeWithoutParameterLessContructorTestHelpers");
							}
							else
							{
								var helper = (ISemaphoreTypeWithoutParameterLessContructorTestHelper)objectHandle.GetObject();
								semaphores.AddRange(helper.GetUniqueSemaphores());
							}
						}
					}
				}
			}
			if (!unableToCreateTypeMessages.IsEmpty)
			{
				Fail(unableToCreateTypeMessages.ToString());
			}

			return semaphores.ToArray();
		}

		string[] GetEnterpriseAssemblies()
		{
			List<string> assemblyNames = new ();

			foreach (var name in BuildXml.Instance.GetAllTestedAssemblies())
			{
				if (IsNotTargetPrefix(name))
				{
					continue;
				}

				if (name.ToLower().EndsWith(".dll") || name.ToLower().EndsWith(".exe"))
				{
					assemblyNames.Add(name.Substring(0, name.Length - 4));
				}
			}

			return assemblyNames.ToArray();
		}

		public bool IsNotTargetPrefix(string assemblyName)
		{
#pragma warning disable CS0436 // Type conflicts with imported type - due to InternalsVisibleTo
			bool isNetCoreTargetFrameworkPrefix = assemblyName.StartsWith(CommonAssemblyInfo.CWNetCoreSubfolder, StringComparison.OrdinalIgnoreCase);
#pragma warning restore CS0436 // Type conflicts with imported type
#if NETFRAMEWORK
			return isNetCoreTargetFrameworkPrefix;
#elif NET
			return !isNetCoreTargetFrameworkPrefix;
#else
#error Unexpected target platform
#endif
		}
	}
}
