using CargoWise.ComponentModel;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	sealed class AsycudaBillGlowImporterTest : TestCaseWithFactory
	{
		public void TestImportChildlessChildren()
		{
			var logger = new Mock<INotifications>().Object;
			var bill = Factory.New<AsycudaBill>();
			var importer = new AsycudaBillGlowImporter() as IGlowCustomImporter;

			Assert(!importer.ImportChildlessChildren(bill, logger, 1, ["chilProperty"], [new ImportPreviewLineDetails("test", 1)]));
			Assert(!importer.ShouldCustomizeChildrenImport);
		}

		public void TestConvertCustomLine_CusGoodsLocation()
		{
			var logger = new Mock<INotifications>().Object;
			var bill = Factory.New<AsycudaBill>();
			var importer = new AsycudaBillGlowImporter();

			importer.ConvertCustomLine(bill, "A", logger, 1, "CusGoodsLocation_CGL_Type");
			AssertEquals("A", bill.CusGoodsLocation.CGL_Type);

			importer.ConvertCustomLine(bill, "A", logger, 1, "CusGoodsLocation_CGL_Qualifier");
			AssertEquals("A", bill.CusGoodsLocation.CGL_Qualifier);

			importer.ConvertCustomLine(bill, "A", logger, 1, "CusGoodsLocation_CGL_AdditionalIdentifier");
			AssertEquals("A", bill.CusGoodsLocation.CGL_AdditionalIdentifier);

			importer.ConvertCustomLine(bill, "A", logger, 1, "CusGoodsLocation_CGL_CustomsOffice");
			AssertEquals("A", bill.CusGoodsLocation.CGL_CustomsOffice);
		}

		public void TestConvertCustomLine_InvalidPropertyName()
		{
			var mockedLogger = new Mock<INotifications>();
			var bill = Factory.New<AsycudaBill>();
			var importer = new AsycudaBillGlowImporter();

			importer.ConvertCustomLine(bill, "A", mockedLogger.Object, 1, "CusGoodsLocation_CGL_ABC");
			mockedLogger.Verify(m => m.Add(It.IsAny<INotification>()), Times.Once);
			var cusGoodsLocation = Factory.LoadTop1<CusGoodsLocation>(new CargoWise.EntityFramework.ZQuery());

			importer.ConvertCustomLine(bill, "A", mockedLogger.Object, 1, "ABC");
			mockedLogger.Verify(m => m.Add(It.IsAny<INotification>()), Times.Exactly(2));
			cusGoodsLocation = Factory.LoadTop1<CusGoodsLocation>(new CargoWise.EntityFramework.ZQuery());
			AssertEquals(string.Empty, bill.CusGoodsLocation.CGL_Type);
			AssertEquals(string.Empty, bill.CusGoodsLocation.CGL_Qualifier);
			AssertEquals(string.Empty, bill.CusGoodsLocation.CGL_AdditionalIdentifier);
			AssertEquals(string.Empty, bill.CusGoodsLocation.CGL_CustomsOffice);
		}
	}
}
