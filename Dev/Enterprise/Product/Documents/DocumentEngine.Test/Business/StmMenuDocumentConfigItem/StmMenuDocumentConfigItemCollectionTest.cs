using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.DocBuilder;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(StmMenuDocumentConfigItemCollection))]
	sealed class StmMenuDocumentConfigItemCollectionTest : BusinessObjectCollectionTestCase
	{
		new StmMenuDocumentConfigItemCollection Collection
		{
			get { return (StmMenuDocumentConfigItemCollection)base.Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmMenuDocumentConfigItemCollection(Factory.New<StmMenuDocumentConfig>(), Factory);
		}

		public void TestAddFromTemplateSection()
		{
			var bodyConfigItem1 = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.BodySection + ",Oink", 0, 0));
			var documentHeaderConfigItem = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentHeader, 0, 0));
			var pageHeaderConfigItem = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.PageHeader, 0, 0));
			var documentFooterConfigItem = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentFooter, 0, 0));
			var bodyConfigItem2 = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.BodySection + ",Moo", 0, 0));
			var pageFooterConfigItem = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.PageFooter, 0, 0));
			var genericSectionBodyConfigItem = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.GenericSection + ", Generic Section Body 1", 0, 0));

			AssertEquals("documentHeaderConfigItem.S4_PrintOrder", 1, documentHeaderConfigItem.S4_PrintOrder);
			AssertEquals("pageHeaderConfigItem.S4_PrintOrder", 2, pageHeaderConfigItem.S4_PrintOrder);
			AssertEquals("bodyConfigItem1.S4_PrintOrder", 3, bodyConfigItem1.S4_PrintOrder);
			AssertEquals("bodyConfigItem2.S4_PrintOrder", 4, bodyConfigItem2.S4_PrintOrder);
			AssertEquals("genericSectionBodyConfigItem.S4_PrintOrder", 5, genericSectionBodyConfigItem.S4_PrintOrder);
			AssertEquals("pageFooterConfigItem.S4_PrintOrder", 6, pageFooterConfigItem.S4_PrintOrder);
			AssertEquals("documentFooterConfigItem.S4_PrintOrder", 7, documentFooterConfigItem.S4_PrintOrder);

			AssertEquals("bodyConfigItem1.S4_SectionType", ConfigurableSectionTypeList.Codes.BodySection, bodyConfigItem1.S4_SectionType);
			AssertEquals("bodyConfigItem1.S4_SectionItemName", "Oink", bodyConfigItem1.S4_SectionItemName);
			AssertEquals("bodyConfigItem2.S4_SectionItemName", "Moo", bodyConfigItem2.S4_SectionItemName);
			AssertEquals("genericSectionBodyConfigItem.S4_SectionItemName", GenericSectionUsageList.Codes.BodySection, genericSectionBodyConfigItem.S4_SectionType);
			AssertEquals("genericSectionBodyConfigItem.S4_SectionItemName", "Generic Section Body 1", genericSectionBodyConfigItem.S4_SectionItemName);
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, Collection.AllowNew);
		}

		public void TestContains()
		{
			StmMenuDocumentConfigItem bodyConfigItem1 = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.BodySection + ",Aye", 0, 0));
			StmMenuDocumentConfigItem bodyConfigItem2 = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.BodySection + ",Nay", 0, 0));
			StmMenuDocumentConfigItem pageHeaderConfigItem3 = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.PageHeader, 0, 0));
			AssertEquals("Contains(\"Aye\")", true, Collection.Contains("Aye"));
			AssertEquals("Contains(\"Nay\")", true, Collection.Contains("Nay"));
			AssertEquals("Contains(ConfigurableSectionTypeList.Descriptions.BodySection)", true, Collection.Contains(ConfigurableSectionTypeList.Descriptions.PageHeader));
			AssertEquals("Contains(\"Oink\")", false, Collection.Contains("Oink"));
			AssertEquals("Contains(\"1\")", false, Collection.Contains("1"));
		}

		public void TestDeleteAndReorder()
		{
			StmMenuDocumentConfigItem item1 = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentHeader, 0, 0));
			StmMenuDocumentConfigItem item2 = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.PageHeader, 0, 0));
			StmMenuDocumentConfigItem item3 = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.BodySection, 0, 0));
			StmMenuDocumentConfigItem item4 = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.PageFooter, 0, 0));
			StmMenuDocumentConfigItem item5 = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentFooter, 0, 0));

			Collection.DeleteAndReorder(item2);
			AssertEquals("item1.S4_PrintOrder", 1, item1.S4_PrintOrder);
			AssertEquals("item3.S4_PrintOrder", 2, item3.S4_PrintOrder);
			AssertEquals("item4.S4_PrintOrder", 3, item4.S4_PrintOrder);
			AssertEquals("item5.S4_PrintOrder", 4, item5.S4_PrintOrder);
			AssertEquals("item2.IsDeleted", true, item2.IsDeleted);

			Collection.DeleteAndReorder(new StmMenuDocumentConfigItem[] { item1, item4 });
			AssertEquals("item3.S4_PrintOrder", 1, item3.S4_PrintOrder);
			AssertEquals("item5.S4_PrintOrder", 2, item5.S4_PrintOrder);
			AssertEquals("item1.IsDeleted", true, item1.IsDeleted);
			AssertEquals("item4.IsDeleted", true, item4.IsDeleted);

			Collection.HasChanges = false;
			((IBusinessObjectCollectionInternals)Collection).HasChangesFromDelete = false;
			Collection.DeleteAndReorder(item5);
			AssertEquals("item3.S4_PrintOrder", 1, item3.S4_PrintOrder);
			AssertEquals("item5.IsDeleted", true, item5.IsDeleted);
			AssertEquals("HasChanges", true, Collection.HasChanges);
		}

		public void TestMove()
		{
			StmMenuDocumentConfigItem bodyConfigItem1 = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.BodySection, 0, 0));
			StmMenuDocumentConfigItem bodyConfigItem2 = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.BodySection, 0, 0));

			AssertEquals("bodyConfigItem1.S4_PrintOrder", 1, bodyConfigItem1.S4_PrintOrder);
			AssertEquals("bodyConfigItem2.S4_PrintOrder", 2, bodyConfigItem2.S4_PrintOrder);

			Collection.MoveDown(0);
			AssertEquals("bodyConfigItem1.S4_PrintOrder", 2, bodyConfigItem1.S4_PrintOrder);
			AssertEquals("bodyConfigItem2.S4_PrintOrder", 1, bodyConfigItem2.S4_PrintOrder);
			AssertEquals("[0]", bodyConfigItem2, Collection[0]);
			AssertEquals("[1]", bodyConfigItem1, Collection[1]);

			Collection.MoveDown(1);
			AssertEquals("bodyConfigItem1.S4_PrintOrder", 2, bodyConfigItem1.S4_PrintOrder);
			AssertEquals("bodyConfigItem2.S4_PrintOrder", 1, bodyConfigItem2.S4_PrintOrder);
			AssertEquals("[0]", bodyConfigItem2, Collection[0]);
			AssertEquals("[1]", bodyConfigItem1, Collection[1]);

			Collection.MoveUp(1);
			AssertEquals("bodyConfigItem1.S4_PrintOrder", 1, bodyConfigItem1.S4_PrintOrder);
			AssertEquals("bodyConfigItem2.S4_PrintOrder", 2, bodyConfigItem2.S4_PrintOrder);
			AssertEquals("[0]", bodyConfigItem1, Collection[0]);
			AssertEquals("[1]", bodyConfigItem2, Collection[1]);

			StmMenuDocumentConfigItem documentHeaderConfigItem = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentHeader, 0, 0));
			StmMenuDocumentConfigItem documentFooterConfigItem = Collection.AddFromTemplateSection(new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentFooter, 0, 0));

			Collection.MoveUp(2);
			Collection.MoveUp(1);
			AssertEquals("documentHeaderConfigItem.S4_PrintOrder", 1, documentHeaderConfigItem.S4_PrintOrder);
			AssertEquals("bodyConfigItem1.S4_PrintOrder", 3, bodyConfigItem1.S4_PrintOrder);
			AssertEquals("bodyConfigItem2.S4_PrintOrder", 2, bodyConfigItem2.S4_PrintOrder);
			AssertEquals("documentFooterConfigItem.S4_PrintOrder", 4, documentFooterConfigItem.S4_PrintOrder);
			AssertEquals("[0]", documentHeaderConfigItem, Collection[0]);
			AssertEquals("[1]", bodyConfigItem2, Collection[1]);
			AssertEquals("[2]", bodyConfigItem1, Collection[2]);
			AssertEquals("[3]", documentFooterConfigItem, Collection[3]);

			Collection.MoveDown(1);
			Collection.MoveDown(2);
			AssertEquals("documentHeaderConfigItem.S4_PrintOrder", 1, documentHeaderConfigItem.S4_PrintOrder);
			AssertEquals("bodyConfigItem1.S4_PrintOrder", 2, bodyConfigItem1.S4_PrintOrder);
			AssertEquals("bodyConfigItem2.S4_PrintOrder", 3, bodyConfigItem2.S4_PrintOrder);
			AssertEquals("documentFooterConfigItem.S4_PrintOrder", 4, documentFooterConfigItem.S4_PrintOrder);
			AssertEquals("[0]", documentHeaderConfigItem, Collection[0]);
			AssertEquals("[1]", bodyConfigItem1, Collection[1]);
			AssertEquals("[2]", bodyConfigItem2, Collection[2]);
			AssertEquals("[3]", documentFooterConfigItem, Collection[3]);
		}

		public void TestSortByPrintOrder()
		{
			StmMenuDocumentConfigItem item1 = Collection.AddNew();
			StmMenuDocumentConfigItem item2 = Collection.AddNew();
			StmMenuDocumentConfigItem item3 = Collection.AddNew();

			item1.S4_PrintOrder = 1;
			item2.S4_PrintOrder = 2;
			item3.S4_PrintOrder = 0;
			Collection.SortByPrintOrder();

			AssertEquals("[0]", item3, Collection[0]);
			AssertEquals("[1]", item1, Collection[1]);
			AssertEquals("[2]", item2, Collection[2]);
		}

		public void TestSupportsSorting()
		{
			AssertEquals("SupportsSorting", false, ((IBindingList)Collection).SupportsSorting);
		}

		public void TestParentlessConstructor()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			StmMenuDocumentConfigItemCollection collection = new StmMenuDocumentConfigItemCollection(newFactory);
			AssertEquals(newFactory, collection.Factory);
		}
	}
}
