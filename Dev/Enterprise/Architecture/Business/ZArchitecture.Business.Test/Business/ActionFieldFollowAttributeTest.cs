using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ActionFieldFollowAttributeTest : TestCase
	{
		public void TestGetReturnType()
		{
			List<ZString> errors = new List<ZString>();
			Type type = typeof(DynamicBusinessObject);
			PropertyInfo[] properties = type.GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				Type result = null;
				try
				{
					result = ActionFieldFollowAttribute.GetReturnType(propertyInfo);
				}
				catch (Exception ex)
				{
					errors.Add(String.Format("Error on property [{0}]: {1}", propertyInfo.Name, ex.Message));
				}
			}
			AssertMultilineASCIIEquals("Tests MUST NOT return any errors", "", string.Join("\n", errors.ToArray()));
		}
	}
}
