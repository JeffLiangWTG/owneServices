using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSeaManOBLDetailsSeaCargoReportLineTest : TestCaseWithFactory
	{
		public void TestConsignorVendor()
		{
			AssertEquals(ZString.Empty, ReportLine.ConsignorVendor);
		}

		public void TestContainerMode()
		{
			Detail.BD_LineCargoType = "ABC";
			AssertEquals("ContainerMode", "ABC", ReportLine.ContainerMode);
		}

		public void TestContainerNumber()
		{
			Detail.BD_ContainerNumber = "123";
			AssertEquals("ContainerNumber", "123", ReportLine.ContainerNumber);

			Detail.BD_LineCargoType = CMRImportCargoTypes.Codes.Bulk;
			AssertEquals("ContainerNumber when bulk", ZString.Empty, ReportLine.ContainerNumber);

			Detail.BD_LineCargoType = CMRImportCargoTypes.Codes.BreakBulk;
			AssertEquals("ContainerNumber when break bulk", ZString.Empty, ReportLine.ContainerNumber);
		}

		public void TestSealNumber()
		{
			Detail.BD_SealNo = "321";
			AssertEquals("SealNumber", "321", ReportLine.SealNumber);
		}

		public void TestMarksAndNumbers()
		{
			Detail.BD_MarksAndNumbers = "MN";
			AssertEquals("MarksAndNumbers", "MN", ReportLine.MarksAndNumbers);
		}

		public void TestShippingOwnedContainerIndicator()
		{
			Detail.BD_ShipperOwnedContainerIndicator = true;
			AssertEquals("ShippingOwnedContainerIndicator", true, ReportLine.ShippingOwnedContainerIndicator);
		}

		public void TestFumigationCertificateIndicator()
		{
			Detail.BD_FumigationIndicator = true;
			AssertEquals("FumigationCertificateIndicator", true, ReportLine.FumigationCertificateIndicator);
		}

		public void TestHazardousGoodsIndicator()
		{
			Detail.BD_HazardousIndicator = true;
			AssertEquals("HazardousGoodsIndicator", true, ReportLine.HazardousGoodsIndicator);
		}

		public void TestDocumentsIndicator()
		{
			Detail.BD_ReportableDocsIndicator = true;
			AssertEquals("DocumentsIndicator", true, ReportLine.DocumentsIndicator);
		}

		public void TestSACIndication()
		{
			Detail.BD_SACIndicator = true;
			AssertEquals("SACIndication", true, ReportLine.SACIndication);
		}

		public void TestPerishableGoodsIndicator()
		{
			Detail.BD_PerishableIndicator = true;
			AssertEquals("PerishableGoodsIndicator", true, ReportLine.PerishableGoodsIndicator);
		}

		public void TestTimberIndicator()
		{
			Detail.BD_TimberIndicator = true;
			AssertEquals("TimberIndicator", true, ReportLine.TimberIndicator);
		}

		public void TestPersonalEffectsIndicator()
		{
			Detail.BD_PersonalEffectsIndicator = true;
			AssertEquals("PersonalEffectsIndicator", true, ReportLine.PersonalEffectsIndicator);
		}

		public void TestVolume()
		{
			Detail.BD_CargoVolume = 100m;
			AssertEquals("Volume", 100m, ReportLine.Volume);
		}

		public void TestWeight()
		{
			Detail.BD_GrossWeight = 100m;
			AssertEquals("Weight", 100m, ReportLine.Weight);
		}

		public void TestWeightUQ()
		{
			Detail.BD_GrossWeightUM = "LB";
			AssertEquals("WeightUQ", "LB", ReportLine.WeightUQ);
		}

		public void TestPackageCount()
		{
			Detail.BD_NoOfPacks = 100;
			AssertEquals("PackageCount", 100, ReportLine.PackageCount);
		}

		public void TestPackageType()
		{
			Detail.BD_PackType = "BOX";
			AssertEquals("PackageType", "BOX", ReportLine.PackageType);
		}

		public void TestGoodsDescription()
		{
			Detail.BD_GoodsDescription = "DESCRIPTION";
			AssertEquals("GoodsDescription", "DESCRIPTION", ReportLine.GoodsDescription);
		}

		public void TestContainerType()
		{
			Detail.BD_TypeOfContainer = "OTOP";
			AssertEquals("ContainerType", "OTOP", ReportLine.ContainerType);
		}

		public void TestContainerSize()
		{
			Detail.BD_ContainerSizeOrISOCode = "2008";
			AssertEquals("ContainerSize", "2008", ReportLine.ContainerSize);
		}

		CusSeaManOBLDetailsSeaCargoReportLine ReportLine => new CusSeaManOBLDetailsSeaCargoReportLine(Detail);

		CusSeaManOBLDetail detail;
		CusSeaManOBLDetail Detail => detail ?? (detail = Factory.New<CusSeaManOBLDetail>());
	}
}
