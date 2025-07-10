using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAPivotSeaCargoReportLineTest : TestCaseWithFactory
	{
		public void TestConsignorVendor()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "Test 2";
			consignor.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "123");

			Pivot.HouseBill.CA_OA_ConsignorAddress = consignor.MainAddress.PK;
			AssertEquals("123", Line.ConsignorVendor);
		}

		public void TestContainerMode()
		{
			Container.CN_ContainerMode = "XXX";
			AssertEquals("ContainerMode", "XXX", Line.ContainerMode);
		}

		public void TestContainerNumber()
		{
			Container.CN_ContainerNumber = "123";
			AssertEquals("ContainerNumber", "123", Line.ContainerNumber);

			Container.CN_ContainerMode = CMRImportCargoTypes.Codes.Bulk;
			AssertEquals("ContainerNumber when bulk", ZString.Empty, Line.ContainerNumber);

			Container.CN_ContainerMode = CMRImportCargoTypes.Codes.BreakBulk;
			AssertEquals("ContainerNumber when break bulk", ZString.Empty, Line.ContainerNumber);
		}

		public void TestSealNumber()
		{
			Container.CN_SealNumber = "321";
			AssertEquals("SealNumber", "321", Line.SealNumber);
		}

		public void TestMarksAndNumbers()
		{
			Pivot.CV_MarksAndNumbers = "M&N";
			AssertEquals("MarksAndNumbers", "M&N", Line.MarksAndNumbers);
		}

		public void TestShippingOwnedContainerIndicator()
		{
			Container.CN_ShipperOwnedContainer = true;
			AssertEquals("ShippingOwnedContainerIndicator", true, Line.ShippingOwnedContainerIndicator);
		}

		public void TestFumigationCertificateIndicator()
		{
			Pivot.CV_FumigationCert = true;
			AssertEquals("FumigationCertificateIndicator", true, Line.FumigationCertificateIndicator);
		}

		public void TestHazardousGoodsIndicator()
		{
			Pivot.CV_HazardousGoods = true;
			AssertEquals("HazardousGoodsIndicator", true, Line.HazardousGoodsIndicator);
		}

		public void TestDocumentsIndicator()
		{
			Pivot.CV_IsDocuments = true;
			AssertEquals("DocumentsIndicator", true, Line.DocumentsIndicator);
		}

		public void TestSACIndication()
		{
			Pivot.CV_IsSAC = true;
			AssertEquals("SACIndication", true, Line.SACIndication);
		}

		public void TestPerishableGoodsIndicator()
		{
			Pivot.CV_PerishableGoods = true;
			AssertEquals("PerishableGoodsIndicator", true, Line.PerishableGoodsIndicator);
		}

		public void TestTimberIndicator()
		{
			Pivot.CV_Timber = true;
			AssertEquals("TimberIndicator", true, Line.TimberIndicator);
		}

		public void TestPersonalEffectsIndicator()
		{
			Pivot.CV_PersonalEffects = true;
			AssertEquals("PersonalEffectsIndicator", true, Line.PersonalEffectsIndicator);
		}

		public void TestVolume()
		{
			Pivot.CV_Volume = 123m;
			AssertEquals("Volume", 123m, Line.Volume);
		}

		public void TestWeight()
		{
			Pivot.CV_Weight = 321m;
			Pivot.CV_NetWeight = 412m;
			AssertEquals("Weight", 321m, Line.Weight);

			Pivot.CV_WeightUQ = "KT";
			Pivot.CV_Weight = 322m;
			AssertEquals("Weight", 322000m, Line.Weight);
		}

		public void TestNetWeight()
		{
			Pivot.CV_Weight = 574m;
			Pivot.CV_NetWeight = 412m;
			AssertEquals("Net Weight", 412m, Line.NetWeight);

			Pivot.CV_WeightUQ = "KT";
			Pivot.CV_NetWeight = 322m;
			AssertEquals("Net Weight", 322000m, Line.NetWeight);
		}

		public void TestWeightUQ()
		{
			Pivot.CV_WeightUQ = "KG";
			AssertEquals("WeightUQ", "KG", Line.WeightUQ);

			Pivot.CV_WeightUQ = "KT";
			AssertEquals("WeightUQ", "T", Line.WeightUQ);
		}

		public void TestPackageCount()
		{
			Pivot.CV_PackageCount = 10;
			AssertEquals("PackageCount", 10, Line.PackageCount);
		}

		public void TestPackageType()
		{
			Pivot.CV_PackageType = "BOX";
			AssertEquals("PackageType", "BOX", Line.PackageType);
		}

		public void TestGoodsDescription()
		{
			Pivot.CV_GoodsDescription = "DESC";
			AssertEquals("GoodsDescription", "DESC", Line.GoodsDescription);
		}

		public void TestContainerType()
		{
			Container.CN_TypeOfContainer = "OTOP";
			AssertEquals("ContainerType", "OTOP", Line.ContainerType);
		}

		public void TestContainerSize()
		{
			Container.CN_ContainerSizeOrISOCode = "2008";
			AssertEquals("ContainerSize", "2008", Line.ContainerSize);
		}

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}

		CusSCAPivotSeaCargoReportLine Line => new CusSCAPivotSeaCargoReportLine(Pivot);

		CusSCAPivot pivot;
		CusSCAPivot Pivot
		{
			get
			{
				if (pivot == null)
				{
					var house = Container.OceanBill.HouseBills.AddNew();
					pivot = house.Pivot.AddNew();
					pivot.CV_CN = Container.PK;
				}
				return pivot;
			}
		}

		CusSCAContainer container;
		CusSCAContainer Container
		{
			get
			{
				if (container == null)
				{
					var oceanBill = Factory.New<CusSCAOceanBill>();
					container = oceanBill.Containers.AddNew();
				}
				return container;
			}
		}
	}
}
