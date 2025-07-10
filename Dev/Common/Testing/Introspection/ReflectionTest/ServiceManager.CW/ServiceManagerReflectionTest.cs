using Enterprise.ReflectionTest;
using NUnit.Framework;

namespace ServiceManager.ReflectionTest
{
	[FrequentlyFailing()]
	public class ServiceManagerReflectionTest : ReflectionTestBase
	{
		public void TestNoExternalUsageOfServiceTaskScheduleBO() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestNoExternalUsageOfServiceTaskScheduleBO));
		public void TestAllowedListForServiceTaskSchedule() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestAllowedListForServiceTaskSchedule));
		public void TestExternalAssembliesOnlyReferenceExternalServiceManagerAssemblies() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestExternalAssembliesOnlyReferenceExternalServiceManagerAssemblies));
		public void TestExcludedAssembliesNotReferencingInternalServiceManagerAssemblies() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestExcludedAssembliesNotReferencingInternalServiceManagerAssemblies));
		public void TestBaseAssembliesReferenceAbstractionAssemblies_Runner() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestBaseAssembliesReferenceAbstractionAssemblies_Runner));
		public void TestBaseAssembliesReferenceAbstractionAssemblies_Host() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestBaseAssembliesReferenceAbstractionAssemblies_Host));
		public void TestBaseAssembliesReferenceAbstractionAssemblies_Shared() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestBaseAssembliesReferenceAbstractionAssemblies_Shared));
		public void TestInternalServiceManagerAssembliesDoNotReferenceExternalCWAssemblies() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestInternalServiceManagerAssembliesDoNotReferenceExternalCWAssemblies));
		public void TestExternalCW1AssembliesExclusionsList() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestExternalCW1AssembliesExclusionsList));
		public void TestDetectMisusedAssemblies() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestDetectMisusedAssemblies));
		public void TestDetectMisuseOfServiceTaskScheduleBO() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestDetectMisuseOfServiceTaskScheduleBO));
		public void TestDetectMisuseOfServiceTaskSchedule() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestDetectMisuseOfServiceTaskSchedule));
		public void TestDetectMisuseOfExternalCW1Assemblies() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestDetectMisuseOfExternalCW1Assemblies));
		public void TestNoLegacyUnitTests() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestNoLegacyUnitTests));
		public void TestLegacyTestAssembliesIsValid() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestLegacyTestAssembliesIsValid));
		public void TestAssertDoesNotContainLegacyUnitTests_WithLegacyTestAssemblies_Fails() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestAssertDoesNotContainLegacyUnitTests_WithLegacyTestAssemblies_Fails));
		public void TestAssertAllLegacyUnitTestsExist_WithNoLegacyTestAssemblies_Fails() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestAssertAllLegacyUnitTestsExist_WithNoLegacyTestAssemblies_Fails));
		public void TestAssertAllLegacyUnitTestsExist_WithEmptyLegacyTestAssemblies_Fails() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestAssertAllLegacyUnitTestsExist_WithEmptyLegacyTestAssemblies_Fails));
		public void TestLegacyTestAssembliesIsNotEmpty() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestLegacyTestAssembliesIsNotEmpty));
		public void TestServiceManagerClientContainsNoInternalOrServiceTaskAssemblies() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestServiceManagerClientContainsNoInternalOrServiceTaskAssemblies));
		public void TestAssertServiceManagerClientContainsNoInternalOrServiceTaskAssemblies_Fails() => InvokeTest(nameof(ServiceManagerReflectionTestHelper.TestAssertServiceManagerClientContainsNoInternalOrServiceTaskAssemblies_Fails));

		protected override ReflectionTestHelper GetHelper() => new ServiceManagerReflectionTestHelper();
	}
}
