using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using MapElement = Enterprise.DocumentEngineCore.DocumentSupport.DataContextMapList.MapElement;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	[TestedType(typeof(DocDataProviderReflector))]
	sealed class DocDataProviderReflectorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocTypeNotReflectedFromIEDoc()
		{
			var reflector = new DocDataProviderReflector(typeof(IeDoc));
			var doctype = reflector.Members.Find(property => ((PropertyDescription)property).Property.Name == "DocType") as PropertyDescription;
			AssertNull("DocType", doctype);
		}

		public void TestTopLevelDataSourceInformation()
		{
			var reflector = new DocDataProviderReflector(new MapElement("MyDataContext", typeof(DocumentWrapperForTest)));
			AssertMultilineASCIIEquals("reflector.TopLevelDataSourceInformation", @"
DataContext=MyDataContext

Data Source Type: DocumentWrapperForTest
Namespace: Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
Assembly: Enterprise.DocumentEngine.Test
".Trim(), reflector.TopLevelDataSourceInformation);

			reflector = new DocDataProviderReflector(typeof(DocumentWrapperForTest));
			AssertMultilineASCIIEquals("reflector.TopLevelDataSourceInformation", @"
Data Source Type: DocumentWrapperForTest
Namespace: Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
".Trim(), reflector.TopLevelDataSourceInformation);
		}

		public void TestReflectingOutMyWrapperType()
		{
			var reflector = new DocDataProviderReflector(typeof(DocumentWrapperForTest));
			var result = new ZStringBuilder();
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

		public void TestIPasswordEmailSourceFieldsAreReflectedFromContactPasswordEmail()
		{
			var reflector = new DocDataProviderReflector(typeof(ContactPasswordEmail));
			var match = reflector.Members.Find(property => ((PropertyDescription)property).Property.Name == "EmailInfo") as PropertyDescription;
			AssertNotNull("EmailInfo", match);
			AssertEquals(typeof(IPasswordEmailSource), match.Property.PropertyType);
			reflector = new DocDataProviderReflector(match.Property.PropertyType);
			AssertNotNull("Salutation", reflector.Members.Find(property => ((PropertyDescription)property).Property.Name == "Salutation"));
			AssertNotNull("Password", reflector.Members.Find(property => ((PropertyDescription)property).Property.Name == "Password"));
		}

		public void TestHtmlEmailWithAttachmentTemplatesPropertyOmitted()
		{
			var reflector = new DocDataProviderReflector(typeof(HtmlEmailWithAttachment));
			var match = reflector.Members.Find(property => ((PropertyDescription)property).Property.Name == "Templates") as PropertyDescription;
			AssertNull(match);
		}

		public void TestUnrestrictedAdditionalAddressInformation_ShouldBeHiddenInDocumentDataFieldMap()
		{
			CombineAssertions(() =>
			{
				AssertNull_UnrestrictedAdditionalAddressInformation(typeof(JobDocAddress));
				AssertNull_UnrestrictedAdditionalAddressInformation(typeof(OrgAddress));
				AssertNull_UnrestrictedAdditionalAddressInformation(typeof(GlbBranch));
				AssertNull_UnrestrictedAdditionalAddressInformation(typeof(GlbCompany));
				AssertNull_UnrestrictedAdditionalAddressInformation(typeof(GlbPerson));
				AssertNull_UnrestrictedAdditionalAddressInformation(typeof(GlbStaff));
				AssertNull_UnrestrictedAdditionalAddressInformation(typeof(OrgTranslatedAddress));
				AssertNull_UnrestrictedAdditionalAddressInformation(typeof(SalesEnquiry));
				AssertNull_UnrestrictedAdditionalAddressInformation(typeof(RefCityTownPostcodeHelper));
			});
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocDataProviderReflector(typeof(DocumentWrapperForTest));
		}

		void AssertNull_UnrestrictedAdditionalAddressInformation(Type type)
		{
			var reflector = new DocDataProviderReflector(type);
			var match = reflector.Members.Find(property => ((PropertyDescription)property).Property.Name == "UnrestrictedAdditionalAddressInformation") as PropertyDescription;
			AssertNull(reflector.DocDataProviderType.Name + ".UnrestrictedAdditionalAddressInformation field map", match);
		}

		#endregion
	}
}
