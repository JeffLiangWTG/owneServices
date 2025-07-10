using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.ExcelTemplates;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	[TestedType(typeof(TemplateSectionCollectionView))]
	sealed class TemplateSectionCollectionViewTest : BusinessObjectCollectionViewTestCase<TemplateSectionCollectionView>
	{
		public override void TestAddNew()
		{
			AssertExceptionThrown<NotSupportedException>(() => Collection.AddNew());
			AssertExceptionThrown<NotSupportedException>(() => Collection.AddNew(typeof(TemplateSection)));
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, Collection.AllowNew);
		}

		public void TestElements()
		{
			var collectionToFilter = new TemplateSectionCollection(new ExcelTemplateReadFromByteArray(null, null, CustomisableSectionTest));
			collectionToFilter.Add(new TemplateSection(ConfigurableSectionTypeList.Codes.BodySection + ",x", 0, 0));
			var configItems = new StmMenuDocumentConfigItemCollection(Factory.New<StmMenuDocumentConfig>(), Factory);
			var view = new TemplateSectionCollectionView(collectionToFilter);

			AssertEquals("[0].TypeCode", ConfigurableSectionTypeList.Codes.DocumentHeader, view[0].TypeCode);
			AssertEquals("[1].TypeCode", ConfigurableSectionTypeList.Codes.PageHeader, view[1].TypeCode);
			AssertEquals("[2].TypeCode", ConfigurableSectionTypeList.Codes.BodySection, view[2].TypeCode);
			AssertNotEquals("[2].SectionName", "x", view[2].SectionName);
			AssertEquals("[3].TypeCode", ConfigurableSectionTypeList.Codes.PageFooter, view[3].TypeCode);
			AssertEquals("[4].TypeCode", ConfigurableSectionTypeList.Codes.BodySection, view[4].TypeCode);
			AssertEquals("[4].SectionName", "x", view[4].SectionName);

			configItems.AddFromTemplateSection(collectionToFilter[1]);
			configItems.AddFromTemplateSection(collectionToFilter[6]);
			view.Rebuild();
			AssertEquals("Count", 5, view.Count);
			AssertEquals("[0].TypeCode", ConfigurableSectionTypeList.Codes.DocumentHeader, view[0].TypeCode);
			AssertEquals("[1].TypeCode", ConfigurableSectionTypeList.Codes.PageHeader, view[1].TypeCode);
			AssertEquals("[2].TypeCode", ConfigurableSectionTypeList.Codes.BodySection, view[2].TypeCode);
			AssertEquals("[3].TypeCode", ConfigurableSectionTypeList.Codes.PageFooter, view[3].TypeCode);
			AssertEquals("[4].TypeCode", ConfigurableSectionTypeList.Codes.BodySection, view[4].TypeCode);

			configItems.AddFromTemplateSection(collectionToFilter[2]);
			configItems.AddFromTemplateSection(collectionToFilter[4]);
			view.Rebuild();
			AssertEquals("Count", 5, view.Count);
			AssertEquals("[0].TypeCode", ConfigurableSectionTypeList.Codes.DocumentHeader, view[0].TypeCode);
			AssertEquals("[1].TypeCode", ConfigurableSectionTypeList.Codes.PageHeader, view[1].TypeCode);
			AssertEquals("[2].TypeCode", ConfigurableSectionTypeList.Codes.BodySection, view[2].TypeCode);
			AssertEquals("[3].TypeCode", ConfigurableSectionTypeList.Codes.PageFooter, view[3].TypeCode);
			AssertEquals("[4].TypeCode", ConfigurableSectionTypeList.Codes.BodySection, view[4].TypeCode);

			configItems.AddFromTemplateSection(collectionToFilter[3]);
			view.Rebuild();
			AssertEquals("Count", 5, view.Count);
			AssertEquals("[0].TypeCode", ConfigurableSectionTypeList.Codes.DocumentHeader, view[0].TypeCode);
			AssertEquals("[1].TypeCode", ConfigurableSectionTypeList.Codes.PageHeader, view[1].TypeCode);
			AssertEquals("[2].TypeCode", ConfigurableSectionTypeList.Codes.BodySection, view[2].TypeCode);
			AssertEquals("[3].TypeCode", ConfigurableSectionTypeList.Codes.PageFooter, view[3].TypeCode);
			AssertEquals("[4].TypeCode", ConfigurableSectionTypeList.Codes.BodySection, view[4].TypeCode);

			configItems.SortByPrintOrder();
			configItems.Remove(configItems[4]);
			configItems.Remove(configItems[2]);
			view.Rebuild();
			AssertEquals("Count", 5, view.Count);
			AssertEquals("[0].TypeCode", ConfigurableSectionTypeList.Codes.DocumentHeader, view[0].TypeCode);
			AssertEquals("[1].TypeCode", ConfigurableSectionTypeList.Codes.PageHeader, view[1].TypeCode);
			AssertEquals("[2].TypeCode", ConfigurableSectionTypeList.Codes.BodySection, view[2].TypeCode);
			AssertEquals("[3].TypeCode", ConfigurableSectionTypeList.Codes.PageFooter, view[3].TypeCode);
			AssertEquals("[4].TypeCode", ConfigurableSectionTypeList.Codes.BodySection, view[4].TypeCode);
		}

		public void TestElements_FromTemplate()
		{
			var template = Factory.New<StmTemplateBase>();
			template.SO_Template = CustomisableSectionTest;
			var view = new TemplateSectionCollectionView(template);
			AssertEquals(4, view.Count);
			AssertEquals("DHD", view[0].TypeCode);
			AssertEquals("My Head Hurts", view[0].SectionName);
			AssertEquals("PHD", view[1].TypeCode);
			AssertEquals("Page Header", view[1].SectionName);
			AssertEquals("BOD", view[2].TypeCode);
			AssertEquals("Brett's Farts STILL Stink", view[2].SectionName);
			AssertEquals("PFT", view[3].TypeCode);
			AssertEquals("Page Footer", view[3].SectionName);
		}

		protected override TemplateSectionCollectionView GetCollectionToTest()
		{
			var resourceBytes = resourceRetriever.Value.GetBytes("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.DummyBusinessObjectAsDataSource.xls");
			var collectionToFilter = new TemplateSectionCollection(new ExcelTemplateReadFromByteArray(null, null, resourceBytes));
			return new TemplateSectionCollectionView(collectionToFilter);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new TemplateSection(ZString.Empty, 0, 0);

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] CustomisableSectionTest => resourceRetriever.Value.GetBytes("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.CustomisableSectionTest.xls");
	}
}
