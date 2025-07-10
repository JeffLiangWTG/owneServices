using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSCAR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SEACRMessageLineTest : TestCaseWithFactory
	{
		[TestDate(2017, 4, 11)]
		public void TestVendor()
		{
			using (AUCustomsDataRegistry.Instance.ICSReleaseEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2017, 4, 1)))
			{
				var consignor = Factory.New<OrgHeader>();
				consignor.OH_Code = "Test 2";
				consignor.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "123");

				pivot.HouseBill.CA_OA_ConsignorAddress = consignor.MainAddress.PK;

				var group7 = (SegmentGroup7)messageLine.GetNewSegmentGroup(new CUSCARMessage());
				messageLine.Populate(group7, "I");
				Assert(messageLine.StringValue.Contains("NAD+VN+123::95'"));
			}
		}

		public void TestSegmentGroupType()
		{
			AssertEquals(typeof(SegmentGroup7), messageLine.SegmentGroupType);
		}

		public void TestGetNewSegmentGroup()
		{
			AssertEquals(typeof(SegmentGroup7), messageLine.GetNewSegmentGroup(new CUSCARMessage()).GetType());
		}

		public void TestCNI()
		{
			var group7 = (SegmentGroup7)messageLine.GetNewSegmentGroup(new CUSCARMessage());
			messageLine.Populate(group7, "I");
			Assert(group7.ToString(new UNOCCMRCharacterSet()).IndexOf("CNI++:::I'") != -1);
		}

		public void TestContainerNumber()
		{
			container.CN_ContainerNumber = "12345";
			Assert(messageLine.StringValue.IndexOf("RFF+AAQ:12345'") != -1);
		}

		public void TestContainerSize()
		{
			pivot.Container.CN_RC_NKContainerType = GetContainer();
			Assert(messageLine.StringValue.IndexOf("RFF+ACC:2008'") != -1);
		}

		public void TestSealNumber()
		{
			container.CN_SealNumber = "SEALNUM";
			Assert(messageLine.StringValue.IndexOf("RFF+SN:SEALNUM'") != -1);
		}

		public void TestTriggerRFFIfNoOtherRFFSegments()
		{
			container.CN_ContainerNumber = ZString.Empty;
			container.CN_SealNumber = ZString.Empty;
			container.CN_RC_NKContainerType = ZString.Empty;
			Assert(messageLine.StringValue.IndexOf("RFF+ZZZ:1'") != -1);
		}

		public void TestFumigationIndicator()
		{
			pivot.CV_FumigationCert = true;
			Assert(messageLine.StringValue.IndexOf("GIS+FUM:109:95'") != -1);
		}

		public void TestHazardousGoodsIndicator()
		{
			pivot.CV_HazardousGoods = true;
			Assert(messageLine.StringValue.IndexOf("GIS+HAZ:109:95'") != -1);
		}

		public void TestPerishableGoodsIndicator()
		{
			pivot.CV_PerishableGoods = true;
			Assert(messageLine.StringValue.IndexOf("GIS+PSH:109:95'") != -1);
		}

		public void TestReportableDocumentsIndicator()
		{
			pivot.CV_IsDocuments = true;
			Assert(messageLine.StringValue.IndexOf("GIS+DOC:109:95'") != -1);
			pivot.CV_IsDocuments = false;
			Assert(messageLine.StringValue.IndexOf("GIS+DOC:109:95'") == -1);
		}

		public void TestSelfAssessedClearanceIndicator()
		{
			pivot.CV_IsSAC = true;
			Assert(messageLine.StringValue.IndexOf("GIS+SAC:109:95'") != -1);
			pivot.CV_IsSAC = false;
			Assert(messageLine.StringValue.IndexOf("GIS+SAC:109:95'") == -1);
		}

		public void TestTimberIndicator()
		{
			pivot.CV_Timber = true;
			Assert(messageLine.StringValue.IndexOf("GIS+TMB:109:95'") != -1);
		}

		public void TestShipperOwnedContainerIndicator()
		{
			pivot.CN_ShipperOwnedContainer = true;
			Assert(messageLine.StringValue.IndexOf("GIS+SOC:109:95'") != -1);
		}

		public void TestMultipleIndicators()
		{
			container.CN_SealNumber = "spoons";

			pivot.CV_FumigationCert = true;
			pivot.CV_HazardousGoods = true;
			pivot.CV_IsSAC = true;
			pivot.CN_ShipperOwnedContainer = true;

			string message = messageLine.StringValue;
			string expectedMessage = @"CNI'RFF+AAQ:12345'GID+1'RFF+SN:SPOONS'GIS+FUM:109:95'GIS+HAZ:109:95'GIS+SAC:109:95'GIS+SOC:109:95'GID+1'";
			AssertEquals(expectedMessage, message);
		}

		public void TestSpecialReporterNumber()
		{
			Env.Registry.AUCustoms.HVLVSpecialReporterNumber = "123456";
			Assert(messageLine.StringValue.IndexOf("NAD+AQ+123456::95'") == -1);
		}

		public void TestGID()
		{
			Assert(messageLine.StringValue.IndexOf("GID+1'") != -1);
		}

		public void TestNumberOfPackages()
		{
			pivot.CV_PackageCount = 100;
			Assert(messageLine.StringValue.IndexOf("PAC+100'") != -1);
		}

		public void TestImportCargoType()
		{
			container.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			Assert(messageLine.StringValue.IndexOf("PAC+++FCL:67:95'") != -1);
		}

		public void TestContainerType()
		{
			pivot.Container.CN_RC_NKContainerType = GetContainer();
			Assert(messageLine.StringValue.IndexOf("PAC+++REFR:121:95'") != -1);
		}

		public void TestPackageType()
		{
			pivot.CV_PackageType = "BX";
			Assert(messageLine.StringValue.IndexOf("PAC+++BX:185:95'") != -1);
		}

		public void TestGoodsDescription()
		{
			pivot.CV_GoodsDescription = "DESCRIPTION";
			Assert(messageLine.StringValue.IndexOf("FTX+AAA+++DESCRIPTION'") != -1);
		}

		public void TestNetWeight()
		{
			pivot.CV_NetWeight = 241m;
			pivot.CV_WeightUQ = "KG";
			Assert(messageLine.StringValue.IndexOf("MEA+AAE+AAL+KG:241.00'") != -1);

			pivot.CV_WeightUQ = "KT";
			pivot.CV_NetWeight = 322m;
			Assert(messageLine.StringValue.IndexOf("MEA+AAE+AAL+T:322000.00'") != -1);
		}

		public void TestCargoVolume()
		{
			pivot.CV_Volume = 100m;
			Assert(messageLine.StringValue.IndexOf("MEA+AAE+ABJ+CU:100.00'") != -1);
		}

		public void TestGrossWeight()
		{
			pivot.CV_Weight = 121m;
			pivot.CV_WeightUQ = "KG";
			Assert(messageLine.StringValue.IndexOf("MEA+AAE+G+KG:121.00'") != -1);

			pivot.CV_Weight = 322m;
			pivot.CV_WeightUQ = "KT";
			Assert(messageLine.StringValue.IndexOf("MEA+AAE+G+T:322000.00'") != -1);
		}

		public void TestMarksAndNumbers()
		{
			pivot.CV_MarksAndNumbers = "MARKS AND NUMBERS";
			Assert(messageLine.StringValue.IndexOf("PCI+28+MARKS AND NUMBERS'") != -1);
		}

		public void TestBigMarksAndNumbers()
		{
			pivot.CV_MarksAndNumbers = "<PIONEER> AUSTRALIA ORDER 2624 CTN 1-56";
			Assert(messageLine.StringValue.IndexOf("PCI+28+<PIONEER> AUSTRALIA ORDER 2624 CTN :1-56'") != -1);
		}

		public void TestMarksAndNumbersWhenBulk()
		{
			pivot.Container.CN_ContainerMode = CMRImportCargoTypes.Codes.Bulk;
			Assert(messageLine.StringValue.IndexOf("PCI+28+N/A'") != -1);
		}

		public void TestNoPackingTypeWhenBulk()
		{
			pivot.Container.CN_ContainerMode = CMRImportCargoTypes.Codes.Bulk;
			pivot.CV_PackageType = CMRPackageTypes.Codes.UnpackedOrPacked;
			Assert("Bulk should not contain a reference to the packing type", messageLine.StringValue.IndexOf(":185:95") == -1);
		}

		public void TestMarksAndNumbersGetsFreeOfNewLineCharacterWhenPopulating()
		{
			pivot.CV_MarksAndNumbers = "Marks & Numbers" + "\r\n";
			Assert(messageLine.StringValue, messageLine.StringValue.IndexOf("PCI+28+MARKS & NUMBERS '") > -1);
		}

		public void TestUniqueIdentifier()
		{
			container.CN_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.CN_ContainerNumber = "123";
			AssertEquals("FCL123", messageLine.UniqueIdentifier);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var ocean = Factory.New<CusSCAOceanBill>();
			var house = ocean.HouseBills.AddNew();
			pivot = house.Pivot.AddNew();
			container = ocean.Containers.AddNew();
			container.CN_ContainerNumber = "12345";
			pivot.CV_CN = container.PK;
			messageLine = new SEACRMessageLine(new CusSCAPivotSeaCargoReportLine(pivot));
		}

		CusSCAPivot pivot;
		CusSCAContainer container;
		SEACRMessageLine messageLine;

		ZString GetContainer()
		{
			var container = Factory.New<RefContainer>();
			container.RC_Code = "REFC";
			container.RC_ContainerType = "RFG";
			container.RC_Length = 20;
			container.RC_Height = 8;
			container.RC_Width = 8;
			return container.RC_Code;
		}
	}
}
