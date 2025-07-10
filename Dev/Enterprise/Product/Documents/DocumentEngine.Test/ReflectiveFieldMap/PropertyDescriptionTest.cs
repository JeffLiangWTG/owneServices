using System;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.DocumentEngine.ReflectiveFieldMap.MemberDescription;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	sealed class PropertyDescriptionTest : MemberDescriptionTestCase
	{
		public void TestGetFullPath()
		{
			var propertyDescription1 = new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("CollectionWithCustomProperties"), null);
			AssertEquals("CollectionWithCustomProperties[1]", propertyDescription1.GetFullPath());

			var propertyDescription2 = new PropertyDescription(typeof(DummyBOForTest).GetProperty("CustomZStringInBO1"), propertyDescription1);
			AssertEquals("CollectionWithCustomProperties[1].CustomZStringInBO1", propertyDescription2.GetFullPath());

			var propertyDescription3 = new PropertyDescription(typeof(DocumentWrapperCollectionWithCustomPropertiesForTest).GetProperty("SelfCollection"), propertyDescription1) { MemberBelongsTo = MemberBelongsTo.Collection };
			AssertEquals("CollectionWithCustomProperties.SelfCollection[1]", propertyDescription3.GetFullPath());

			var propertyDescription4 = new PropertyDescription(typeof(DummyBOForTest).GetProperty("CustomZStringInBO2"), propertyDescription3) { MemberBelongsTo = MemberBelongsTo.Element };
			AssertEquals("CollectionWithCustomProperties.SelfCollection[1].CustomZStringInBO2", propertyDescription4.GetFullPath());

			var propertyDescription5 = new PropertyDescription(typeof(DummyBOForTest).GetProperty("CustomZStringEnumerableInBO"), propertyDescription3, "", MacroTagTypes.Document, new MCRDataReflectorFilter(), true, 0);
			AssertEquals("CollectionWithCustomProperties.SelfCollection[1].CustomZStringEnumerableInBO[0]", propertyDescription5.GetFullPath());
		}

		public void TestGetFullPathWithoutIndex()
		{
			var propertyDescription1 = new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("CollectionWithCustomProperties"), null);
			propertyDescription1.ShowIndex = false;
			var propertyDescription3 = new PropertyDescription(typeof(DocumentWrapperCollectionWithCustomPropertiesForTest).GetProperty("SelfCollection"), propertyDescription1) { MemberBelongsTo = MemberBelongsTo.Collection };
			propertyDescription3.ShowIndex = false;
			var propertyDescription4 = new PropertyDescription(typeof(DummyBOForTest).GetProperty("CustomZStringInBO2"), propertyDescription3) { MemberBelongsTo = MemberBelongsTo.Element };
			propertyDescription4.ShowIndex = false;
			AssertEquals("CollectionWithCustomProperties.SelfCollection.CustomZStringInBO2", propertyDescription4.GetFullPath());
		}

		public void TestCanHaveChildMembers()
		{
			AssertEquals("IBODocDataProvider property", true, new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("DocDataProviderIsIn"), null).CanHaveChildMembers());
			AssertEquals("ZString property", false, new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("ZStringIsIn"), null).CanHaveChildMembers());
			AssertEquals("IBODocDataProviderCollection property", true, new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("DocDataProviderCollectionIsIn"), null).CanHaveChildMembers());
		}

		public void TestGetChildType()
		{
			AssertEquals("IBODocDataProvider property", typeof(OrgHeader), new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("DocDataProviderIsIn"), null).GetChildTypes().ChildType);
			AssertNull("Possible Collection property should be null", new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("DocDataProviderIsIn"), null).GetChildTypes().PossibleCollectionType);
			AssertEquals("IBODocDataProviderCollection property", typeof(DocumentWrapperForTest), new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("Collection"), null).GetChildTypes().ChildType);
			AssertEquals("Possible Collection property", typeof(OrgAddressCollection), new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("DocDataProviderCollectionIsIn"), null).GetChildTypes().PossibleCollectionType);

			AssertEquals("Interface with ResolveTypeFromObjectFactoryForDocData but nothing registered", typeof(IDummyIntegrationInterface), new PropertyDescription(typeof(DummyBOForTest).GetProperty(nameof(DummyBOForTest.RelationInterfaceAsExpectedImplementation)), null).GetChildTypes().ChildType);

			using var toDispose = ObjectFactory.Substitute<IDummyIntegrationInterface>(new DummyIntegrationInterfaceImplementation());

			AssertEquals("Interface with ResolveTypeFromObjectFactoryForDocData and type registered", typeof(DummyIntegrationInterfaceImplementation), new PropertyDescription(typeof(DummyBOForTest).GetProperty(nameof(DummyBOForTest.RelationInterfaceAsExpectedImplementation)), null).GetChildTypes().ChildType);

			AssertEquals("Interface without ResolveTypeFromObjectFactoryForDocData", typeof(IDummyIntegrationInterface), new PropertyDescription(typeof(DummyBOForTest).GetProperty(nameof(DummyBOForTest.RelationInterfaceAsInterface)), null).GetChildTypes().ChildType);

			AssertExceptionThrown("ZString property", typeof(InvalidOperationException), () => new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("ZStringIsIn"), null).GetChildTypes());
		}

		public void TestGetCollectionChildType()
		{
			AssertEquals("ZString from ZString[]", typeof(ZString), new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("CollectionIsIn"), null).GetChildTypes().ChildType);
			AssertEquals("OrgAddress from OrgAddressCollection", typeof(OrgAddress), new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("DocDataProviderCollectionIsIn"), null).GetChildTypes().ChildType);
		}

		public void TestPropertyWithNoParent()
		{
			PropertyInfo property = typeof(DocumentWrapperForTest).GetProperty("DocDataProviderIsIn");
			PropertyDescription propertyDescription = new PropertyDescription(property, null);
			AssertEquals(property, propertyDescription.Property);
			AssertEquals(null, propertyDescription.ParentMemberDescription);
			AssertEquals("DocDataProviderIsIn (OrgHeader)", propertyDescription.GetFormattedTextLabel());
			AssertEquals("<DocDataProviderIsIn>", propertyDescription.GetMacro());
		}

		public void TestPropertyWithParent()
		{
			PropertyInfo parentProperty = typeof(DocumentWrapperForTest).GetProperty("Relation");
			PropertyDescription parentPropertyDescription = new PropertyDescription(parentProperty, null);
			PropertyInfo property = typeof(DocumentWrapperForTest).GetProperty("DocDataProviderIsIn");
			PropertyDescription propertyDescription = new PropertyDescription(property, parentPropertyDescription);
			AssertEquals(property, propertyDescription.Property);
			AssertEquals(parentPropertyDescription, propertyDescription.ParentMemberDescription);
			AssertEquals("DocDataProviderIsIn (OrgHeader)", propertyDescription.GetFormattedTextLabel());
			AssertEquals("<Relation.DocDataProviderIsIn>", propertyDescription.GetMacro());
		}

		public void TestPropertyWithParentWithParent()
		{
			PropertyInfo parentParentProperty = typeof(DocumentWrapperForTest).GetProperty("Relation");
			PropertyDescription parentParentPropertyDescription = new PropertyDescription(parentParentProperty, null);
			PropertyInfo parentProperty = typeof(DocumentWrapperForTest).GetProperty("Relation");
			PropertyDescription parentPropertyDescription = new PropertyDescription(parentProperty, parentParentPropertyDescription);
			PropertyInfo property = typeof(DocumentWrapperForTest).GetProperty("DocDataProviderIsIn");
			PropertyDescription propertyDescription = new PropertyDescription(property, parentPropertyDescription);
			AssertEquals(property, propertyDescription.Property);
			AssertEquals(parentPropertyDescription, propertyDescription.ParentMemberDescription);
			AssertEquals("DocDataProviderIsIn (OrgHeader)", propertyDescription.GetFormattedTextLabel());
			AssertEquals("<Relation.Relation.DocDataProviderIsIn>", propertyDescription.GetMacro());
		}

		public void TestPropertyWithControllerInfo()
		{
			PropertyInfo parentParentProperty = typeof(DocumentWrapperForTest).GetProperty("Relation");
			PropertyDescription parentParentPropertyDescription = new PropertyDescription(parentParentProperty, null);
			PropertyInfo parentProperty = typeof(DocumentWrapperForTest).GetProperty("Relation");
			PropertyDescription parentPropertyDescription = new PropertyDescription(parentProperty, parentParentPropertyDescription);
			PropertyInfo property = typeof(DocumentWrapperForTest).GetProperty("DocDataProviderIsIn");
			PropertyDescription propertyDescription = new PropertyDescription(property, parentPropertyDescription);

			AssertContains("Module ID: Organisation", propertyDescription.GetMemberInformation());
			AssertContains("Controller ID: Organisation", propertyDescription.GetMemberInformation());

			property = typeof(DocumentWrapperForTest).GetProperty("ZStringIsIn");
			propertyDescription = new PropertyDescription(property, null);

			AssertNotContains("The member information should not contains Module ID", "Module ID", propertyDescription.GetMemberInformation());
			AssertNotContains("The member information should not contains Controller ID", "Controller ID", propertyDescription.GetMemberInformation());
		}

		public void TestGetControllerInfoWithModuleIdNotSupported()
		{
			var property = typeof(DummyWrapper).GetProperty("Equipment");
			var perpertyDescription = new PropertyDescription(property, null);
			var controllerInfo = perpertyDescription.GetControllerInformation();
			AssertEquals("Controller ID: GPSSupporter", controllerInfo);
		}

		public void TestGetGenericSplitPropertyName()
		{
			var property = typeof(DummyWrapper).GetProperty("GenericField");
			var propertyDescription = new PropertyDescription(property, null);
			AssertEquals("GenericField (GenericAttribute)", propertyDescription.GetFormattedTextLabel());
		}

		class GenericAttribute<T>
		{
		}

		class DummyWrapper
		{
			public RefEquipment Equipment { get; set; }
			public GenericAttribute<string> GenericField { get; set; }
		}
	}
}
