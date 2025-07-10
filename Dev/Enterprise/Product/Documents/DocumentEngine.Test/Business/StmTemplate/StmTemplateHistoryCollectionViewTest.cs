using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing;

[TestedType(typeof(StmTemplateHistoryCollectionView))]
public class StmTemplateHistoryCollectionViewTest : BusinessObjectCollectionViewTestCase<StmTemplateHistoryCollectionView>
{
	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var provider = ObjectFactory.Get<IDocumentFactoryProvider>();
		var documentFactory = provider.GetFactory(Factory) as BusinessObjectFactory;
		return documentFactory.New<IStorageFile>() as BusinessObject;
	}

	protected override StmTemplateHistoryCollectionView GetCollectionToTest()
	{
		var templateData1 =  DocumentEngineTestHelper.CreateTemplateFromString(
			@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#EndOfReport]");
		var templateData2 =  DocumentEngineTestHelper.CreateTemplateFromString(
			@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]");

		var template = Factory.NewWithValidTestData<StmTemplateBase>();
		template.SO_Template = templateData1;
		template.SO_Name = "ATest";
		template.SO_ExcelTemplatePath = "Test.xls";
		Factory.Save();

		template.SO_Template = templateData2;
		Factory.Save();
		return new StmTemplateHistoryCollectionView(template.TemplateHistories);
	}
}