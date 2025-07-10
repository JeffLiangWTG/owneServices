using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AIROUTMessageLineTest : TestCaseWithFactory
	{
		public void TestSegmentGroupType()
		{
			AssertEquals(typeof(SegmentGroup7), MessageLine.SegmentGroupType);
		}

		public void TestGetNewSegmentGroup()
		{
			AssertEquals(typeof(SegmentGroup7), MessageLine.GetNewSegmentGroup(new CUSCARMessage()).GetType());
		}

		public void TestUniqueIdentifier()
		{
			LineInfo.MasterAirWaybillNumber = "321";
			LineInfo.HouseAirWaybillNumber = "123";
			AssertEquals("UniqueIdentifier", "MAWB=321HAWB=123", MessageLine.UniqueIdentifier);
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
			LineInfo.HouseAirWaybillNumber = "123";
			AssertMessageContains("RFF+HWB:123'");
		}

		public void TestMasterBill()
		{
			LineInfo.MasterAirWaybillNumber = "321";
			AssertMessageContains("RFF+MWB:321'");
		}

		public void TestDamageIndicator()
		{
			LineInfo.DamageIndicator = true;
			AssertMessageContains("GIS+DAM:109:95'");
		}

		public void TestPillageIndicator()
		{
			LineInfo.PillageIndicator = true;
			AssertMessageContains("GIS+PIL:109:95'");
		}

		public void TestPackages()
		{
			LineInfo.NumberOfPackages = 123;
			AssertMessageContains("PAC+123'");
		}

		public void TestPackagesForZeroPackages()
		{
			LineInfo.NumberOfPackages = 0;
			AssertMessageContains("PAC+0'");
		}

		public void TestGoodsDescription()
		{
			LineInfo.GoodsDescription = "DESCRIPTION";
			AssertMessageDoesNotContain("FTX+AAA");
			LineInfo.OutturnResultType = "SU";
			AssertMessageContains("FTX+AAA+++DESCRIPTION'");
			LineInfo.OutturnResultType = "SH";
			AssertMessageDoesNotContain("FTX+AAA");
			LineInfo.OutturnResultType = "SC";
			AssertMessageContains("FTX+AAA+++DESCRIPTION'");
			LineInfo.OutturnResultType = "NIL";
			AssertMessageDoesNotContain("FTX+AAA");
		}

		void AssertMessageDoesNotContain(ZString text)
		{
			AssertMessageContains(text, checkDoesNotContain: true);
		}

		void AssertMessageContains(ZString text, bool checkDoesNotContain = false)
		{
			var group7 = new SegmentGroup7();
			MessageLine.Populate(group7, "I");
			var messageText = group7.ToString(new Edifact.UNOCCMRCharacterSet());
			if (checkDoesNotContain)
			{
				Assert("Message Should not contain: '" + text + "'" + "\r\n\r\nMessage:\r\n" + messageText, !messageText.Contains(text));
			}
			else
			{
				Assert("Message Should contain: '" + text + "'" + "\r\n\r\nMessage:\r\n" + messageText, messageText.Contains(text));
			}
		}

		AIROUTMessageLine MessageLine => new AIROUTMessageLine(LineInfo);

		AirOutturnReportLineInformationForTest lineInfo;
		AirOutturnReportLineInformationForTest LineInfo
		{
			get
			{
				if (lineInfo == null)
				{
					lineInfo = new AirOutturnReportLineInformationForTest();
					lineInfo.MasterAirWaybillNumber = "123";
				}
				return lineInfo;
			}
		}

		sealed class AirOutturnReportLineInformationForTest : IAirOutturnReportLineInformation
		{
			public ZString HouseAirWaybillNumber { get; set; }

			public ZString MasterAirWaybillNumber { get; set; }

			public ZDateTime LastMessageDate { get; set; }
			public bool DamageIndicator { get; set; }

			public bool PillageIndicator { get; set; }

			public ZInt NumberOfPackages { get; set; }

			public ZString GoodsDescription { get; set; }

			public ZString OutturnResultType { get; set; }
		}
	}
}
