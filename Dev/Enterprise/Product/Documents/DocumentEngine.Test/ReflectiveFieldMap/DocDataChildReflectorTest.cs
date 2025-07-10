using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	[TestedType(typeof(DocDataReflector))]
	sealed class DocDataChildReflectorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValueForReflector()
		{
			var reflector = new DocDataReflector(new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("CollectionWithCustomProperties"), null));
			AssertEquals(true, reflector.ShowIndex);
		}

		public void TestMembersWithCustomPropertiesInCollection()
		{
			var reflector = new DocDataReflector(new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("CollectionWithCustomProperties"), null));
			var result = new ZStringBuilder();
			foreach (PropertyDescription propertyDescription in reflector.Members)
			{
				result.Append(string.Format("Type: [{0}] Name: [{1}]", propertyDescription.Property.PropertyType.Name, propertyDescription.Property.Name));
			}

			AssertMultilineASCIIEquals("reflector.Properties", @"
Type: [IEnumerable`1] Name: [CustomZStringEnumerableInBO]
Type: [IDummyIntegrationInterface] Name: [RelationInterfaceAsExpectedImplementation]
Type: [IDummyIntegrationInterface] Name: [RelationInterfaceAsInterface]
Type: [ZString] Name: [CustomZStringInBO1]
Type: [ZString] Name: [CustomZStringInBO2]
Type: [ZString] Name: [CustomZStringInCollection1]
Type: [ZString] Name: [CustomZStringInCollection2]
Type: [DocumentWrapperCollectionWithCustomPropertiesForTest] Name: [SelfCollection]
".Trim(), result.ToStringWithNewLineBetweenAppends());
		}

		public void TestReflectingOutMyWrapperType()
		{
			var reflector = new DocDataReflector(new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("Relation"), null));
			ZStringBuilder result = new ZStringBuilder();
			foreach (PropertyDescription propertyDescription in reflector.Members)
			{
				result.Append(string.Format("Type: [{0}] Name: [{1}]", propertyDescription.Property.PropertyType.Name, propertyDescription.Property.Name));
			}

			AssertMultilineASCIIEquals("reflector.Properties", @"
Type: [OrgHeader] Name: [DocDataProviderIsIn]
Type: [OrgContact] Name: [DocDataProviderIzIn]
Type: [DocumentWrapperForTest] Name: [Relation]
Type: [ZString] Name: [ZStringIsIn]
Type: [ZString] Name: [ZStringIsIn1]
Type: [ZString] Name: [ZStringIsIn2]
Type: [DocumentWrapperCollectionForTest] Name: [Collection]
Type: [ZString[]] Name: [CollectionIsIn]
Type: [DocumentWrapperCollectionWithCustomPropertiesForTest] Name: [CollectionWithCustomProperties]
Type: [OrgAddressCollection] Name: [DocDataProviderCollectionIsIn]
".Trim(), result.ToStringWithNewLineBetweenAppends());
		}

		public void TestReflectingOutMyWrapperType_WithMethods()
		{
			var reflector = new DocDataReflector(new PropertyDescription(typeof(DocumentWrapperWithMethodForTest).GetProperty("RelationWithMethod"), null));
			ZStringBuilder result = new ZStringBuilder();
			foreach (MemberDescription memberDescription in reflector.Members)
			{
				result.Append(memberDescription.GetFormattedTextLabel());
			}

			AssertMultilineASCIIEquals("reflector.Members", @"
DocDataProviderIsIn (OrgHeader)
DocDataProviderIzIn (OrgContact)
Relation (DocumentWrapperForTest)
RelationWithMethod (DocumentWrapperWithMethodForTest)
ZStringIsIn (ZString)
ZStringIsIn1 (ZString)
ZStringIsIn2 (ZString)
Collection (DocumentWrapperCollectionForTest)
CollectionIsIn (ZString[])
CollectionWithCustomProperties (DocumentWrapperCollectionWithCustomPropertiesForTest)
DocDataProviderCollectionIsIn (OrgAddressCollection)
GetRelationWithMethod({param1},{param2}) (DocumentWrapperWithMethodForTest)
GetZStringIsIn() (ZString)
GetCollection() (DocumentWrapperCollectionForTest)
GetCollectionIsIn() (ZString[])
".Trim(), result.ToStringWithNewLineBetweenAppends());
		}

		public void TestReflecting_InheritedInterface()
		{
			var reflector = new DocDataReflector(new PropertyDescription(typeof(MyChildClass).GetProperty("Child"), null));
			var result = new ZStringBuilder();
			foreach (var memberDescription in reflector.Members)
			{
				result.Append(memberDescription.GetFormattedTextLabel());
			}

			AssertMultilineASCIIEquals("GIVEN child-property is an interface with parent-child relationship WHEN reflecting THEN should return parent and child properties", @"
ChildDescription (ZString)
ChildName (ZString)
ParentDescription (ZString)
ParentName (ZString)
".Trim(), result.ToStringWithNewLineBetweenAppends());
		}

		public void TestNoIKeyDataPairCollection()
		{
			var propertyInfo = typeof(Logs).GetProperty("MostRecentLogByPostedDate");
			var reflector = new DocDataReflector(new PropertyDescription(propertyInfo, null));
			var countOfIKeyDataPairCollection = reflector.Members.OfType<PropertyDescription>().Count(x => x.Property.PropertyType.Name == "IKeyDataPairCollection");
			AssertEquals("IKeyDataPairCollection count should be zero", 0, countOfIKeyDataPairCollection);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocDataReflector(new PropertyDescription(typeof(DocumentWrapperForTest).GetProperty("DocDataProviderIzIn"), null));
		}

		public class MyChildClass
		{
			public IChildInterface Child { get; }
		}

		public interface IParentInterface
		{
			ZString ParentName { get; }
			ZString ParentDescription { get; }
		}

		public interface IChildInterface : IParentInterface
		{
			ZString ChildName { get; }
			ZString ChildDescription { get; }
		}

		#endregion
	}
}
