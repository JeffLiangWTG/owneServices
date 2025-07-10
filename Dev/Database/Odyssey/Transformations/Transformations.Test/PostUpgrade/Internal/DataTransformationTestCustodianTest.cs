using System;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	static class Extensions
	{
		public static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
		{
			while (toCheck != null && toCheck != typeof(object))
			{
				var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur)
				{
					return true;
				}
				toCheck = toCheck.BaseType;
			}
			return false;
		}
	}

	class DataTransformationTestCustodianTest : TestCase
	{
		public void TestNoDataTransformationTestCaseSubclassOverridesTestRunAndAssertResultsTwiceBehaviour()
		{
			const string RunAndAssertTwiceTestMethodName = "TestRunAndAssertResultsTwice";
			var thisAssembly = GetType().Assembly;
			var transformationTestCaseType = typeof(DataTransformationTestCase);
			var transformationTestSubClasses = thisAssembly.GetTypes().Where(t => transformationTestCaseType.IsAssignableFrom(t));
			Assert("[PRE-CONDITION] DataTransformationTestCase should have subclasses", transformationTestSubClasses.Any());

			CombineAssertions($"The folowing {transformationTestCaseType.Name} subclasses hide method {RunAndAssertTwiceTestMethodName} overriding it. This test plays an important role ensuring our transformations are deterministic and should not be overriden.", () =>
			{
				foreach (var testType in transformationTestSubClasses)
				{
					var isHidingBaseRunAndAssertTwiceTestMethod = testType.GetMethods().Any(m => m.Name == RunAndAssertTwiceTestMethodName && m.DeclaringType != transformationTestCaseType);

					if (
						isHidingBaseRunAndAssertTwiceTestMethod
						&& !whiteListForOverridingRunAndAssertTwiceTestBehaviour.Any(t => t.IsAssignableFrom(testType) || testType.IsSubclassOfRawGeneric(t))
					)
					{
						Fail(testType.FullName);
					}
				}
			});
		}

		/// <summary>
		/// *********************************************************************************************************
		/// ** DO NOT ADD NEW TYPES TO THE LIST BELOW WITHOUT THE REVIEW OF A SENIOR ARCHITECT (Architecture Team) **
		/// *********************************************************************************************************
		/// In some exceptional cases, transformations were granted white-list status:
		/// * CreateTriggerTransformation - it blows up if run a second time (not the design transformation behaviour / can it be re-designed?)
		/// * Transformations which need a RequiresSoftware attribute which current does not work at class level (RequiresSoftware attribute must be fixed)
		/// </summary>
		readonly Type[] whiteListForOverridingRunAndAssertTwiceTestBehaviour = new Type[]
		{
			typeof(TriggerTransformationTestCase<>),
		};
	}
}
