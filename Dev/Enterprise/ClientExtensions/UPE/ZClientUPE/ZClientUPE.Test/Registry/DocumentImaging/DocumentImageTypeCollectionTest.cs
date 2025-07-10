using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Registry.Testing
{
	[TestedType(typeof(DocumentImageTypeCollection))]
	internal class DocumentImageTypeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DocumentImageTypeCollection>
	{
		public void TestIndexerAndAddNew()
		{
			DocumentImageType imageType1 = Collection.AddNew();
			DocumentImageType imageType2 = Collection.AddNew();
			AssertEquals("Collection[0]", imageType1, Collection[0]);
			AssertEquals("Collection[1]", imageType2, Collection[1]);
		}

		public void TestAddNew_WithPropertyValues()
		{
			DocumentImageType type = Collection.AddNew("UPS", "EDI", "Description", true, true);
			AssertEquals("UPS", type.UPSCode);
			AssertEquals("EDI", type.DocTypeCode);
			AssertEquals("Description", type.Description);
			AssertEquals(true, type.MoveJobToClassOnImport);
			AssertEquals(true, type.NotifyOnImport);
		}

		public void TestFindByUPSCode()
		{
			DocumentImageType type1 = Collection.AddNew("U1", "EDI", "Description", true, true);
			DocumentImageType type2 = Collection.AddNew("U2", "EDI", "Description", true, true);
			DocumentImageType catchAllType = Collection.AddNew("", "EDI", "Description", true, true);
			AssertEquals(type1, Collection.FindByUPSCode("U1"));
			AssertEquals(type2, Collection.FindByUPSCode("U2"));
			AssertEquals(catchAllType, Collection.FindByUPSCode("XX"));
		}

		#region Implementation
		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override DocumentImageTypeCollection GetCollectionToTest()
		{
			return new DocumentImageTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocumentImageType();
		}

		new DocumentImageTypeCollection Collection
		{
			get
			{
				return base.Collection;
			}
		}
		#endregion
	}
}
