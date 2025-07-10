using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class CTOCusHAWBCargoReportHeaderTest : CusHAWBAirCargoReportHeaderTest
	{
		public override void TestMasterHouseBill()
		{
			HAWB.CS_MasterHouseBill = "ABC";
			AssertEquals("MasterHouseBill ", ZString.Empty, Header.MasterHouseBill);
		}

		public override void TestMasterHouseBillCanComeFromMAWB()
		{
			HAWB.CS_MasterHouseBill = "ABC";
			HAWB.MAWB.CM_MasterHouseBill = "CBA";
			AssertEquals("MasterHouseBill ", ZString.Empty, Header.MasterHouseBill);
		}

		public override void TestHAWBNum()
		{
			HAWB.CS_HAWB = "ABC";
			AssertEquals("HAWBNum ", ZString.Empty, Header.HAWBNum);
		}

		public override void TestMAWB()
		{
			HAWB.MAWB.CM_MAWB = "VBA";
			HAWB.CS_HAWB = "ABC";
			AssertEquals("MAWB ", "ABC", Header.MAWB);
		}

		public override void TestLoading()
		{
			HAWB.MAWB.CM_RL_NKLoadPort = "ABC";
			HAWB.CS_RL_NKLoadPort = "CBA";
			AssertEquals("Loading", "CBA", Header.Loading);
		}

		public override void TestIsHVLVSpecialReporter()
		{
			AssertEquals("HVLV Special Reporter option should not be available in CTO", false, Header.IsHVLVSpecialReporter);
		}

		public override void TestIsRemailSpecialReporter()
		{
			AssertEquals("Remail Special Reporter option should not be available in CTO", false, Header.IsRemailSpecialReporter);
		}

		IAirCargoReportHeader fHeader;
		protected override IAirCargoReportHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = new CTOCusHAWBCargoReportHeader((CTOCusHAWB)HAWB);
				}
				return fHeader;
			}
		}

		CusHAWBBase fHAWB;
		protected override CusHAWBBase HAWB
		{
			get
			{
				if (fHAWB == null)
				{
					fHAWB = ((CTOCusMAWB)MAWB).ChildBills.AddNew();
				}
				return fHAWB;
			}
		}

		CusMAWBBase fMAWB;
		protected override CusMAWBBase MAWB
		{
			get
			{
				if (fMAWB == null)
				{
					fMAWB = Factory.New<CTOCusMAWB>();
				}
				return fMAWB;
			}
		}
	}
}
