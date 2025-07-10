using NUnit.Framework;

namespace Enterprise.ReflectionTest
{
	[FrequentlyFailing()]
	public class ReflectionTest : ReflectionTestBase
	{
		#region Metadata Tests

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestModuleIDSpecifiedExists()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestModuleIDSpecifiedExists));
		}

		#endregion

		#region Coding Rules

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAssembliesStrongNamed()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestAssembliesStrongNamed));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIntegrationAssembliesContainNoCode()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestIntegrationAssembliesContainNoCode));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoPublicWeaklyTypedCollections()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestNoPublicWeaklyTypedCollections));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSetterDoesNotExistOnTypedIndexersOnBusinessObjectCollections()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestSetterDoesNotExistOnTypedIndexersOnBusinessObjectCollections));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOverriddenAddNewExistsOnBusinessObjectCollections()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestOverriddenAddNewExistsOnBusinessObjectCollections));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSetterDoesNotExistOnCollectionsHostedByBusinessObject()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestSetterDoesNotExistOnCollectionsHostedByBusinessObject));
		}

		[DeveloperOnlyTest]
		public void TestUnitPropertiesReferencedInMeasureUnitAttributes()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestUnitPropertiesReferencedInMeasureUnitAttributes));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDetectStaticBusinessObjectsCollectionsAndFactories()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestDetectStaticBusinessObjectsCollectionsAndFactories));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCustomsSolutionsDoNotExposeBaseLevelProperties()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestCustomsSolutionsDoNotExposeBaseLevelProperties));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public virtual void TestZWinFormHasTypedConstructor()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestZWinFormHasTypedConstructor));
		}

		//public void TestTestClassesAreNotNested()
		//{
		//  InvokeTest(nameof(ReflectionTestHelper.TestTestClassesAreNotNested));
		//}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestControllersUsingSameIDAllDescendFromCommonBase()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestControllersUsingSameIDAllDescendFromCommonBase));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestModulesUsingSameIDAllDescendFromCommonBase()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestModulesUsingSameIDAllDescendFromCommonBase));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDebugOnlyDllsHaveDeployToClientsFalse()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestDebugOnlyDllsHaveDeployToClientsFalse));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocWrapperNewMethodsDoNotThrowException()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestDocWrapperNewMethodsDoNotThrowException));
		}

		// TODO: WI00801582 - Remove Serialization Reflection Tests
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestClassesInheritedFromSerializableClassesMustAlsoBeSerializable()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestClassesInheritedFromSerializableClassesMustAlsoBeSerializable));
		}

		// TODO: WI00801582 - Remove Serialization Reflection Tests
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSerializableClassesMeetSerializableRequirements()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestSerializableClassesMeetSerializableRequirements));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCustomViewSchemasValidation()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestCustomViewSchemasValidation));
		}

		#endregion

		#region Naming Rules

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestClassNamesAreUniqueAcrossAssembliesForEachNamespace()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestClassNamesAreUniqueAcrossAssembliesForEachNamespace));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestSchemaConstantsHaveSameNameAsValue()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestSchemaConstantsHaveSameNameAsValue));
		}

		//TODO: Implement when consensus agreed
		//		[ExpectNoExceptions]
		//		public void TestProtectedFieldsRepresentingBusinessObjectsStartWithLowercaseF()
		//		{
		//			InvokeTest(nameof(ReflectionTestHelper.TestProtectedFieldsRepresentingBusinessObjectsStartWithLowercaseF));
		//		}
		//
		//		[ExpectNoExceptions]
		//		public void TestPublicReadOnlyBusinessObjectFieldsDoNotExist()
		//		{
		//			InvokeTest(nameof(ReflectionTestHelper.TestPublicReadOnlyBusinessObjectFieldsDoNotExist));
		//		}

		#endregion

		#region TestAllClassesRequiringASpecificTestCaseHaveIt

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllClassesRequiringASpecificTestCaseHaveIt()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestAllClassesRequiringASpecificTestCaseHaveIt));
		}

		#endregion

		#region Tests For Tests

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetPublicWeaklyTypedCollections()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestGetPublicWeaklyTypedCollections));
		}

		//public void TestAllSupportedCountriesHaveReportTests()
		//{
		//    InvokeTest("TestAllSupportedCountriesHaveReportTests");
		//}

		#endregion

		#region TestIComplianceRiskStatusProviderCanNotBeImplementedUnlessConfirmedWithMDM

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIComplianceItemRiskStatusProviderCanNotBeImplementedUnlessConfirmedWithMDM()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestIComplianceItemRiskStatusProviderCanNotBeImplementedUnlessConfirmedWithMDM));
		}

		#endregion TestIComplianceRiskStatusProviderCanNotBeImplementedUnlessConfirmedWithMDM

		#region TestIAutoRatingImplementersHaveAutoratingAuditLogNote

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIAutoRatingImplementersHaveAutoratingAuditLogNote()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestIAutoRatingImplementersHaveAutoratingAuditLogNote));
		}

		#endregion

		#region TestIImportExportImplementedImplicitly

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIImportExportImplementedImplicitly()
		{
			//JobDirection must be implemented as public member because clients use it in filters and cutomized documents
			InvokeTest(nameof(ReflectionTestHelper.TestIImportExportImplementedImplicitly));
		}

		#endregion

		#region TestAllSchemaBoundResourceStringsAreValid

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllSchemaBoundResourceStringsAreValid()
		{
			InvokeTest(nameof(ReflectionTestHelper.TestAllSchemaBoundResourceStringsAreValid));
		}

		#endregion
	}
}
