using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class RootTypesHelperTest : TestCaseWithFactory
	{
		public void TestRootTypes_1stIsIAllowAdditionalRootTypeAndAdditionalRootTypeExists()
		{
			var rootTypes = new[]
			{
				typeof(DummyChildBusinessObjectForRootType),
				typeof(DummyBusinessObjectForRootType),
				typeof(DummyChildBusinessObjectForRootTypeCollection)
			};
			AssertSequencesEqual("1st is IAllowAdditionalRootType and AdditionalRootType exists", new[]
			{
				typeof(DummyChildBusinessObjectForRootType),
				typeof(DummyBusinessObjectForRootType)
			}, RootTypesHelper.GetDataFieldsOnlyRootTypes(rootTypes));
		}

		public void TestRootTypes_1stIsNotIAllowAdditionalRootType()
		{
			var rootTypes = new[]
			{
				typeof(DummyChildBusinessObjectForRootTypeCollection),
				typeof(DummyChildBusinessObjectForRootType),
				typeof(DummyBusinessObjectForRootType)
			};
			AssertSequencesEqual("1st is not IAllowAdditionalRootType", new[]
			{
				typeof(DummyChildBusinessObjectForRootTypeCollection)
			}, RootTypesHelper.GetDataFieldsOnlyRootTypes(rootTypes));
		}

		public void TestRootTypes_1stIsIAllowAdditionalRootTypeAndAdditionalRootTypeDoesNotExist()
		{
			var rootTypes = new[]
			{
				typeof(DummyChildBusinessObjectForRootType),
				typeof(DummyChildBusinessObjectForRootTypeCollection)
			};
			AssertSequencesEqual("1st is IAllowAdditionalRootType and AdditionalRootType doesn't exist", new[]
			{
				typeof(DummyChildBusinessObjectForRootType)
			}, RootTypesHelper.GetDataFieldsOnlyRootTypes(rootTypes));
		}

		public void TestRootTypes_1stIsIAllowAdditionalRootTypeAndAdditionalRootType()
		{
			var rootTypes = new[]
			{
				typeof(DummyChildBusinessObjectForAdditionalRootType),
				typeof(DummyBusinessObjectForRootType),
				typeof(DummyChildBusinessObjectForRootTypeCollection)
			};
			AssertSequencesEqual("1st is IAllowAdditionalRootType & AdditionalRootType", new[]
			{
				typeof(DummyChildBusinessObjectForAdditionalRootType),
				typeof(DummyBusinessObjectForRootType)
			}, RootTypesHelper.GetDataFieldsOnlyRootTypes(rootTypes));
		}

		public void TestRootTypes_Empty()
		{
			var rootTypes = Array.Empty<Type>();
			AssertEquals("Empty root types", 0, RootTypesHelper.GetDataFieldsOnlyRootTypes(rootTypes).Length);
		}
	}
}
