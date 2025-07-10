using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SEAOUTMessageLineTest : TestCaseWithFactory
	{
		public void TestSegmentGroupType()
		{
			AssertEquals(typeof(SegmentGroup7), MessageLine.SegmentGroupType);
		}

		public void TestGetNewSegmentGroup()
		{
			AssertEquals(typeof(SegmentGroup7), MessageLine.GetNewSegmentGroup(new CUSCARMessage()).GetType());
		}

		public void TestCNI()
		{
			AssertMessageContains("CNI++:::I'");
		}

		public void TestOutturnResultType()
		{
			LineInfo.OutturnResultType = "123";
			AssertMessageContains("RFF+ACU:123'");
		}

		public void TestHouseBill()
		{
			LineInfo.HouseBillOfLading = "123";
			AssertMessageContains("RFF+BH:123'");
		}

		public void TestOceanBill()
		{
			LineInfo.OceanBillOfLading = "321";
			AssertMessageContains("RFF+MB:321'");
		}

		public void TestContainerNumber()
		{
			LineInfo.ContainerNumber = "321";
			AssertMessageContains("RFF+AAQ:321'");
		}

		public void TestSealNumber()
		{
			LineInfo.SealNumber = "321";
			AssertMessageContains("RFF+SN:321'");
		}

		public void TestLongSealNumber()
		{
			LineInfo.SealNumber = "XYZ-456-789-0ABC";
			AssertMessageContains("RFF+SN:XYZ4567890'");
		}

		public void TestSealIntact()
		{
			LineInfo.SealIntactIndicator = true;
			AssertMessageContains("GIS+Y:62:95'");
		}

		public void TestVesselDischarge()
		{
			LineInfo.VesselDischargeUnderbondIndicator = true;
			AssertMessageContains("GIS+V:63:95'");
		}

		public void TestUnpackIndicator()
		{
			LineInfo.UnpackIndicator = true;
			AssertMessageContains("GIS+U:71:95'");
		}

		public void TestDamageIndicator()
		{
			LineInfo.DamageIndicator = true;
			AssertMessageContains("GIS+Y:186:95'");
		}

		public void TestPillageIndicator()
		{
			LineInfo.PillageIndicator = true;
			AssertMessageContains("GIS+Y:188:95'");
		}

		public void TestDateTimeOfCargoReceiptUnload()
		{
			LineInfo.DateTimeOfCargoReceiptUnload = new ZDateTime(2005, 6, 20, 19, 21, 0);
			AssertMessageContains("DTM+420:20050620:102'");
			AssertMessageContains("DTM+420:1921:401'");
		}

		public void TestDateTimeOfOutturn()
		{
			LineInfo.DateTimeOfOutturn = new ZDateTime(2005, 6, 20, 19, 21, 0);
			AssertMessageContains("DTM+570:20050620:102'");
			AssertMessageContains("DTM+570:1921:401'");
		}

		public void TestQuantity()
		{
			LineInfo.Quantity = 10;
			LineInfo.QuantityUnit = "XX";
			AssertMessageContains("QTY+126:10:XX'");
		}

		public void TestPackages()
		{
			LineInfo.NumberOfPackages = 123;
			AssertMessageContains("PAC+123'");
		}

		public void TestEmptyPackagesAreAllowed()
		{
			LineInfo.NumberOfPackages = 0;
			AssertMessageContains("PAC+0'");
		}

		public void TestPackageUnit()
		{
			LineInfo.NumberOfPackages = 5;
			LineInfo.PackageType = "BX";
			AssertMessageContains("PAC+++BX:185:95'");
		}

		public void TestImportCargoType()
		{
			LineInfo.ImportCargoType = Core.Constants.ContainerModes.FCL;
			AssertMessageContains("PAC+++FCL:67:95'");
		}

		public void TestGoodsDescription()
		{
			LineInfo.GoodsDescription = "DESCRIPTION";
			AssertMessageContains("FTX+AAA+++DESCRIPTION'");
		}

		public void TestMarksAndNumbers()
		{
			LineInfo.MarksAndNumbers = "MARKSANDNUMBERS";
			AssertMessageContains("PCI+28+MARKSANDNUMBERS'");
		}

		public void TestUniqueIdentifier()
		{
			LineInfo.ContainerNumber = "1";
			LineInfo.OceanBillOfLading = "2";
			LineInfo.HouseBillOfLading = "3";
			AssertEquals("CONTAINERNUMBER=1OCEANBILL=2HOUSEBILL=3", MessageLine.UniqueIdentifier);
		}

		void AssertMessageContains(ZString text)
		{
			var group7 = new SegmentGroup7();
			MessageLine.Populate(group7, "I");
			var messageText = group7.ToString(new Edifact.UNOCCMRCharacterSet());
			Assert("Message Should contain: '" + text + "'" + "\r\n\r\nMessage:\r\n" + messageText, messageText.Contains(text));
		}

		SEAOUTMessageLine MessageLine => new SEAOUTMessageLine(LineInfo);

		TestHelperSeaOutturnReportLineInformation lineInfo;
		TestHelperSeaOutturnReportLineInformation LineInfo
		{
			get
			{
				if (lineInfo == null)
				{
					lineInfo = new TestHelperSeaOutturnReportLineInformation();
					lineInfo.OceanBillOfLading = "123";
				}
				return lineInfo;
			}
		}

		sealed class TestHelperSeaOutturnReportLineInformation : ISeaOutturnReportLineInformation
		{
			public ZString ContainerNumber { get; set; }

			public ZString HouseBillOfLading { get; set; }

			public ZString OceanBillOfLading { get; set; }

			public ZString SealNumber { get; set; }

			public bool SealIntactIndicator { get; set; }

			public bool VesselDischargeUnderbondIndicator { get; set; }

			public bool UnpackIndicator { get; set; }

			public ZDateTime DateTimeOfCargoReceiptUnload { get; set; }

			public ZDateTime DateTimeOfOutturn { get; set; }

			public ZInt Quantity { get; set; }

			public ZString QuantityUnit { get; set; }

			public ZString ImportCargoType { get; set; }

			public ZString PackageType { get; set; }

			public ZString MarksAndNumbers { get; set; }

			public ZString OutturnStatus { get; set; }

			public bool DamageIndicator { get; set; }

			public bool PillageIndicator { get; set; }

			public ZInt NumberOfPackages { get; set; }

			public ZString GoodsDescription { get; set; }

			public ZString OutturnResultType { get; set; }

			public ZDateTime LastMessageDate { get; set; }
		}
	}
}
