using System;
using System.Collections;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	static class StaffRolesNotificationTestHelper
	{
		public static void AssertDefaultRoles(ICodeDescriptionBoolList list)
		{
			CodeDescriptionPairList testList = new StaffAssignmentRoles();
			Assertion.Assert("Number of elements", list.Count >= testList.Count);

			for (int i = 0; i < testList.Count; i++)
			{
				Assertion.Assert(String.Format("Contains Code for {0}", testList[i].Description), list.ContainsCode(testList[i].Code));
				Assertion.AssertEquals(String.Format("Correct Description for {0}", testList[i].Description), testList[i].Description, list.GetDescriptionFromCode(testList[i].Code));
			}
		}

		public static void AssertRoles(ICodeDescriptionBoolList list, CodeDescriptionPair roleToCheck)
		{
			Assertion.Assert("Role does not exist", list.ContainsCode(roleToCheck.Code));
		}

		public static void AssertBoolValue(ICodeDescriptionBoolList list, bool value, bool including, params string[] codes)
		{
			foreach (CodeDescriptionBool element in list)
			{
				bool containsCode = ((IList)codes).Contains(element.Code.ToString());

				if (including && containsCode)
				{
					Assertion.AssertEquals(String.Format("Element with Code {0} should have Bool {1}", element.Code, value), value, element.Bool);
				}
				else if (!including && !containsCode)
				{
					Assertion.AssertEquals(String.Format("Element with Code {0} should have Bool {1}", element.Code, value), value, element.Bool);
				}
			}
		}
	}
}
