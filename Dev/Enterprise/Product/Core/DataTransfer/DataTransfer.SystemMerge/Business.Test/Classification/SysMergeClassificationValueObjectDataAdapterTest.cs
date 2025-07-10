using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business.Testing;
using Enterprise.DataTransfer.SystemMerge.Xml.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeClassificationValueObjectDataAdapterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestExportWithNoClassification()
		{
			Notifications.Clear();
			BaseCusClassPartPivot pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_AddInfo = "for test";
			var result = new Xsd.CusClassification();
			DataAdapter.ExportToValueObject(pivot, result, new ValueObjectExportContext(Notifications));

			Assert("Classification element should be empty", result.PK.IsEmpty);
			AssertEquals("Pivot information should be exported anyway", "for test", result.CusClassPartPivot.AddInfo);
		}

		public void TestExport()
		{
			Notifications.Clear();
			BaseCusClassification classification = TestHelper.ActiveClassificationWithCPDecAnswers;
			Xsd.CusClassification xsdClassification = DataAdapter.ExportToValueObject(classification, new ValueObjectExportContext(Notifications));

			TestHelper.AssertCusClassificationXSD(xsdClassification, classification, false);
			AssertEquals("no of elements in cpdecanswer", 2, xsdClassification.DefaultCPDecAnswers.Count);
			foreach (Xsd.CPDecAnswer cpDecAnsXSd in xsdClassification.DefaultCPDecAnswers)
			{
				BaseCusEntryCPDec cpDecAnswer = Factory.Load<BaseCusEntryCPDec>(new ZGuid(cpDecAnsXSd.PK));
				TestHelper.AssertCPDecAnswerXSD(cpDecAnsXSd, cpDecAnswer);
			}
		}

		public void TestXsdPivotIsEmpty()
		{
			var xsdPivot = new Xsd.CusClassificationCusClassPartPivot();
			Assert(DataAdapter.XsdPivotIsEmpty(xsdPivot));

			xsdPivot = new Xsd.CusClassificationCusClassPartPivot();
			xsdPivot.TariffNum = "123";
			Assert(!DataAdapter.XsdPivotIsEmpty(xsdPivot));

			xsdPivot = new Xsd.CusClassificationCusClassPartPivot();
			xsdPivot.RN_NKCountry = "AU";
			Assert(!DataAdapter.XsdPivotIsEmpty(xsdPivot));

			xsdPivot = new Xsd.CusClassificationCusClassPartPivot();
			xsdPivot.AddInfo = "abc";
			Assert(!DataAdapter.XsdPivotIsEmpty(xsdPivot));

			xsdPivot = new Xsd.CusClassificationCusClassPartPivot();
			xsdPivot.LastAuditedDate = ZDateTime.UtcNow;
			Assert(!DataAdapter.XsdPivotIsEmpty(xsdPivot));

			xsdPivot = new Xsd.CusClassificationCusClassPartPivot();
			xsdPivot.LastAuditedUser = "M.K";
			Assert(!DataAdapter.XsdPivotIsEmpty(xsdPivot));

			xsdPivot = new Xsd.CusClassificationCusClassPartPivot();
			xsdPivot.TariffChangePendingSpecified = true;
			Assert(!DataAdapter.XsdPivotIsEmpty(xsdPivot));

			xsdPivot = new Xsd.CusClassificationCusClassPartPivot();
			xsdPivot.TariffNumSpecified = true;
			xsdPivot.AddInfo = "abc";
			Assert(!DataAdapter.XsdPivotIsEmpty(xsdPivot));
		}

		public void TestImport()
		{
			Notifications.Clear();
			BaseCusClassification classification = TestHelper.ActiveClassificationWithCPDecAnswers;
			Xsd.CusClassification xsdClassification = DataAdapter.ExportToValueObject(classification, new ValueObjectExportContext(Notifications));
			BusinessObjectFactory importFactory = NewFactory();

			xsdClassification.IsActive = false;

			AssertNull("Classification with the pk specified on the xsd doens't exist", importFactory.Load<BaseCusClassification>(new ZGuid(xsdClassification.PK)));
			ValueObjectImportContext context = new ValueObjectImportContext(importFactory, Notifications);
			BaseCusClassification importedClassification = DataAdapter.CreateOrUpdateFromValueObject(xsdClassification, context);

			TestHelper.AssertCusClassificationXSD(xsdClassification, classification, true);

			foreach (Xsd.CPDecAnswer cpDecAnsXSd in xsdClassification.DefaultCPDecAnswers)
			{
				BaseCusEntryCPDec cpDecAnswer = Factory.Load<BaseCusEntryCPDec>(new ZGuid(cpDecAnsXSd.PK));
				TestHelper.AssertCPDecAnswerXSD(cpDecAnsXSd, cpDecAnswer);
				AssertEquals("parent ID", classification.PK, cpDecAnswer.ON_ParentID);
				AssertEquals("parent prefix", CusClassificationSchema.Constants.Prefix, cpDecAnswer.ON_ParentTableCode);
			}
		}

		public void TestImportWithEmptyClassification()
		{
			Notifications.Clear();
			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_TariffNum = "123";
			pivot.CI_AddInfo = "Add info";
			var xsdClassification = new Xsd.CusClassification();
			DataAdapter.ExportToValueObject(pivot, xsdClassification, new ValueObjectExportContext(Notifications));

			Assert("Precondition: Classification element should be empty", xsdClassification.PK.IsEmpty);
			AssertEquals("Precondition: Pivot information should be exported anyway", "Add info", xsdClassification.CusClassPartPivot.AddInfo);

			var product = TestHelper.GetNewProduct();
			//product.Factory.Save();

			var importFactory = NewFactory();
			var importedClassification = new SysMergeClassificationValueObjectDataAdapter(product)
				.CreateOrUpdateFromValueObject(xsdClassification, new ValueObjectImportContext(importFactory, Notifications));

			Assert("Classification bizo should be deleted - thus not saved to db", importedClassification.IsDeleted);

			var query = new ZQuery(CusClassPartPivotSchema.CI_OP, product.PK);
			var classPivots = importFactory.Load<BaseCusClassPartPivot>(query);
			AssertEquals("Shoud have 1 cus classification pivots", 1, classPivots.Length);
			AssertEquals("123", classPivots[0].CI_TariffNum);
			AssertEquals("Add info", classPivots[0].CI_AddInfo);
			AssertEquals(ZGuid.Empty, classPivots[0].CI_CC);
		}

		public void TestImportAndExport()
		{
			SysMergeClassifcationXmlValueObjectSerializerForTesting serializer = new SysMergeClassifcationXmlValueObjectSerializerForTesting(DataAdapter);
			Notifications.Clear();
			BaseCusClassification classification = TestHelper.ActiveClassificationWithCPDecAnswers;
			Xsd.CusClassification xsdClassification = DataAdapter.ExportToValueObject(classification, new ValueObjectExportContext(Notifications));

			string exportedclassificationXsd1 = TestHelper.WriteValueObjectToXml(xsdClassification, serializer, DataAdapter);

			Notifications.Clear();
			BusinessObjectFactory importingFactory = NewFactory();
			IValueObjectImportContext importingContext = new ValueObjectImportContext(importingFactory, Notifications);
			BaseCusClassification importedclassification = DataAdapter.CreateOrUpdateFromValueObject(xsdClassification, importingContext);

			Xsd.CusClassification xsdClassification2 = DataAdapter.ExportToValueObject(importedclassification, new ValueObjectExportContext(Notifications));
			string exportedclassificationXsd2 = TestHelper.WriteValueObjectToXml(xsdClassification2, serializer, DataAdapter);

			AssertXMLEquals("Comparing 2 ValueObject in XML format", exportedclassificationXsd1, exportedclassificationXsd2);
		}

		public void TestLinkingExistingClassificationToProduct()
		{
			Notifications.Clear();
			BusinessObjectFactory importingFactory = NewFactory();
			var classification = TestHelper.ActiveClassificationWithCPDecAnswers;
			classification.Factory.Save();

			var product = TestHelper.GetNewProduct();

			var pivotQuery = new ZQuery(CusClassPartPivotSchema.CI_CC, classification.PK);
			pivotQuery.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, classification.CC_RN_NKCountryCode);
			pivotQuery.AddToFilter(CusClassPartPivotSchema.CI_OP, product.PK);

			AssertEquals("cusclasspartpivot should not exist", true, classification.Factory.Load<BaseCusClassPartPivot>(pivotQuery).Length == 0);
			SysMergeClassifcationXmlValueObjectSerializerForTesting serializer = new SysMergeClassifcationXmlValueObjectSerializerForTesting(DataAdapter);

			var productXSD = new SysMergeProductValueObjectDataAdapter().ExportToValueObject(product, new ValueObjectExportContext(Notifications));
			var classificationXSD = productXSD.CusClassifications.AddNew();
			classificationXSD.LookupCode = classification.CC_LookupCode;
			classificationXSD.PK = classification.PK.ToString();
			classificationXSD.RN_Code = classification.CC_RN_NKCountryCode;
			classificationXSD.CusClassPartPivot.RN_NKCountry = classification.CC_RN_NKCountryCode;

			IValueObjectImportContext importingContext = new ValueObjectImportContext(importingFactory, Notifications);

			new SysMergeProductValueObjectDataAdapter().CreateOrUpdateFromValueObject(productXSD, importingContext);

			importingFactory.Save();
			AssertEquals("cusclasspartpivot should exist", true, importingFactory.Load<BaseCusClassPartPivot>(pivotQuery).Length > 0);
		}

		NotificationBuffer Notifications
		{
			get { return notifications ?? (notifications = new NotificationBuffer()); }
		}
		NotificationBuffer notifications;

		SysMergeClassificationValueObjectDataAdapter DataAdapter
		{
			get { return dataAdapter ?? (dataAdapter = new SysMergeClassificationValueObjectDataAdapter()); }
		}
		SysMergeClassificationValueObjectDataAdapter dataAdapter;

		SysMergeTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new SysMergeTestHelper(Factory)); }
		}
		SysMergeTestHelper testHelper;
	}
}
