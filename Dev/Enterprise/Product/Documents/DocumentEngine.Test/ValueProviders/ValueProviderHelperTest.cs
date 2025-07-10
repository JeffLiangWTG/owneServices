using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.ValueProviders.Testing
{
	sealed class ValueProviderHelperTest : TestCaseWithFactory
	{
		public void TestGetBusinessObjectFromDataProvider()
		{
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
				var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				var menuItem = Factory.New<StmMenuItem>();
				var pack = new DocumentPack(menuItem);
				var report = new Report(pack, excelTemplate);
				var shipment = Factory.New<DummyShipmentBusinessObject>();
				shipment.Z0_Code = "SHP";
				shipment.Z0_Date = new DateTime(2012, 12, 12);
				shipment.Z0_Decimal = 100.00m;
				Factory.Save();
				IBODocDataProvider docDataProvider = new DummyShipmentDocumentWrapper(shipment, Factory);
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(docDataProvider);

				var header = Factory.NewWithValidTestData<OrgHeader>();
				var address = header.MainAddress;
				((DummyShipmentDocumentWrapper)docDataProvider).Consignee = header;
				Factory.Save();

				var value = ValueProviderHelper.GetBusinessObjectFromDataProvider("Consignee", report);
				AssertEquals(header.PK, ((BusinessObject)value).PK);

				value = ValueProviderHelper.GetBusinessObjectFromDataProvider("Consignee.MainAddress", report);
				AssertEquals(address.PK, ((BusinessObject)value).PK);

				value = ValueProviderHelper.GetBusinessObjectFromDataProvider("Z0_Code", report);
				AssertEquals("SHP", value);

				value = ValueProviderHelper.GetBusinessObjectFromDataProvider("Z0_Date", report);
				AssertEquals(new DateTime(2012, 12, 12), value);

				value = ValueProviderHelper.GetBusinessObjectFromDataProvider("Z0_Decimal", report);
				AssertEquals(100.00m, value);
			}
		}
	}
}
