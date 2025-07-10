using System;
using System.Reflection;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	sealed class MethodDescriptionTest : MemberDescriptionTestCase
	{
		public void TestCanHaveChildMembers()
		{
			AssertEquals("IBODocDataProvider return type", true, new MethodDescription(typeof(DocumentWrapperWithMethodForTest).GetMethod("GetRelationWithMethod"), null).CanHaveChildMembers());
			AssertEquals("ZString return type", false, new MethodDescription(typeof(DocumentWrapperWithMethodForTest).GetMethod("GetZStringIsIn"), null).CanHaveChildMembers());
			AssertEquals("IBODocDataProviderCollection return type", true, new MethodDescription(typeof(DocumentWrapperWithMethodForTest).GetMethod("GetCollection"), null).CanHaveChildMembers());
		}

		public void TestGetChildType()
		{
			AssertEquals("IBODocDataProvider return type", typeof(DocumentWrapperWithMethodForTest), new MethodDescription(typeof(DocumentWrapperWithMethodForTest).GetMethod("GetRelationWithMethod"), null).GetChildTypes().ChildType);
			AssertEquals("IBODocDataProviderCollection return type", typeof(DocumentWrapperForTest), new MethodDescription(typeof(DocumentWrapperWithMethodForTest).GetMethod("GetCollection"), null).GetChildTypes().ChildType);

			AssertExceptionThrown("ZString return type", typeof(InvalidOperationException), () => new MethodDescription(typeof(DocumentWrapperWithMethodForTest).GetMethod("GetZStringIsIn"), null).GetChildTypes());
		}

		public void TestGetCollectionChildType()
		{
			AssertEquals("ZString from ZString[]", typeof(ZString), new MethodDescription(typeof(DocumentWrapperWithMethodForTest).GetMethod("GetCollectionIsIn"), null).GetChildTypes().ChildType);
			AssertEquals("OrgAddress from OrgAddressCollection", typeof(DocumentWrapperForTest), new MethodDescription(typeof(DocumentWrapperWithMethodForTest).GetMethod("GetCollection"), null).GetChildTypes().ChildType);
		}

		public void TestMethodWithNoParent()
		{
			MethodInfo method = typeof(DocumentWrapperWithMethodForTest).GetMethod("GetZStringIsIn");
			MethodDescription methodDescription = new MethodDescription(method, null);
			AssertEquals(method, methodDescription.Method);
			AssertEquals(null, methodDescription.ParentMemberDescription);
			AssertEquals("GetZStringIsIn() (ZString)", methodDescription.GetFormattedTextLabel());
			AssertEquals("<GetZStringIsIn()>", methodDescription.GetMacro());
		}

		public void TestPropertyWithParent()
		{
			PropertyInfo parentProperty = typeof(DocumentWrapperWithMethodForTest).GetProperty("RelationWithMethod");
			PropertyDescription parentPropertyDescription = new PropertyDescription(parentProperty, null);
			MethodInfo method = typeof(DocumentWrapperWithMethodForTest).GetMethod("GetZStringIsIn");
			MethodDescription methodDescription = new MethodDescription(method, parentPropertyDescription);
			AssertEquals(method, methodDescription.Method);
			AssertEquals(parentPropertyDescription, methodDescription.ParentMemberDescription);
			AssertEquals("GetZStringIsIn() (ZString)", methodDescription.GetFormattedTextLabel());
			AssertEquals("<RelationWithMethod.GetZStringIsIn()>", methodDescription.GetMacro());
		}

		public void TestPropertyWithParentWithParent()
		{
			PropertyInfo parentParentProperty = typeof(DocumentWrapperWithMethodForTest).GetProperty("RelationWithMethod");
			PropertyDescription parentParentPropertyDescription = new PropertyDescription(parentParentProperty, null);
			PropertyInfo parentProperty = typeof(DocumentWrapperWithMethodForTest).GetProperty("RelationWithMethod");
			PropertyDescription parentPropertyDescription = new PropertyDescription(parentProperty, parentParentPropertyDescription);
			MethodInfo method = typeof(DocumentWrapperWithMethodForTest).GetMethod("GetZStringIsIn");
			MethodDescription methodDescription = new MethodDescription(method, parentPropertyDescription);
			AssertEquals(method, methodDescription.Method);
			AssertEquals(parentPropertyDescription, methodDescription.ParentMemberDescription);
			AssertEquals("GetZStringIsIn() (ZString)", methodDescription.GetFormattedTextLabel());
			AssertEquals("<RelationWithMethod.RelationWithMethod.GetZStringIsIn()>", methodDescription.GetMacro());
		}
	}
}
