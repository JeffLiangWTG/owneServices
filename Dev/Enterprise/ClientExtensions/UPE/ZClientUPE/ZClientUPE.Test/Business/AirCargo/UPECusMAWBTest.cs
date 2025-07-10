using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPECusMAWB))]
	public class UPECusMAWBTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPECusMAWB>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestCM_MAWBInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, fUPECusMAWB.CM_MAWBInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, fUPECusMAWB.CM_MAWBInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, fUPECusMAWB.CM_MAWBInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, fUPECusMAWB.CM_MAWBInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestCM_FlightNoInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, fUPECusMAWB.CM_FlightNoInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, fUPECusMAWB.CM_FlightNoInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, fUPECusMAWB.CM_FlightNoInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, fUPECusMAWB.CM_FlightNoInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestCM_ArrivalDateInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, fUPECusMAWB.CM_ArrivalDateInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, fUPECusMAWB.CM_ArrivalDateInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, fUPECusMAWB.CM_ArrivalDateInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, fUPECusMAWB.CM_ArrivalDateInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestCM_RL_NKLoadPortInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, fUPECusMAWB.CM_RL_NKLoadPortInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, fUPECusMAWB.CM_RL_NKLoadPortInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, fUPECusMAWB.CM_RL_NKLoadPortInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, fUPECusMAWB.CM_RL_NKLoadPortInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestCM_RL_NKDischargePortInfo()
		{
			GlbStaff.CurrentUser.GS_IsController = false;
			try
			{
				AssertEquals(false, fUPECusMAWB.CM_RL_NKDischargePortInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = true;
				AssertEquals(false, fUPECusMAWB.CM_RL_NKDischargePortInfo.ReadOnly);
				Factory.Save();
				AssertEquals(false, fUPECusMAWB.CM_RL_NKDischargePortInfo.ReadOnly);
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals(true, fUPECusMAWB.CM_RL_NKDischargePortInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = true;
			}
		}

		public void TestNilOutturn()
		{
			UPECusHAWB hAWB1 = (UPECusHAWB)fUPECusMAWB.ChildBills.AddNew();
			hAWB1.CS_PiecesManifested = 10;
			hAWB1.CS_PiecesLanded = 10;
			hAWB1.CS_IsSurplus = true;
			UPECusHAWB hAWB2 = (UPECusHAWB)fUPECusMAWB.ChildBills.AddNew();
			hAWB2.CS_PiecesManifested = 10;
			hAWB2.CS_PiecesLanded = 10;
			UPECusHAWB hAWB3 = (UPECusHAWB)fUPECusMAWB.ChildBills.AddNew();
			hAWB3.CS_PiecesManifested = 9;
			hAWB3.CS_PiecesLanded = 10;
			UPECusHAWB hAWB4 = (UPECusHAWB)fUPECusMAWB.ChildBills.AddNew();
			hAWB4.CS_PiecesManifested = 11;
			hAWB4.CS_PiecesLanded = 10;
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentID = fUPECusMAWB.PK;
			underbond.C4_ParentTableCode = "CM";
			CusOutturn outturn1 = underbond.Outturns.AddNew();
			CusOutturn outturn2 = underbond.Outturns.AddNew();
			CusOutturn outturn3 = underbond.Outturns.AddNew();
			CusOutturn outturn4 = underbond.Outturns.AddNew();
			((UPECusMAWBForTest)fUPECusMAWB).SetOutturnResultType(hAWB1, outturn1);
			((UPECusMAWBForTest)fUPECusMAWB).SetOutturnResultType(hAWB2, outturn2);
			((UPECusMAWBForTest)fUPECusMAWB).SetOutturnResultType(hAWB3, outturn3);
			((UPECusMAWBForTest)fUPECusMAWB).SetOutturnResultType(hAWB4, outturn4);
			AssertEquals(outturn1.C5_OutturnResultType, CMROutturnResultType.Codes.SurplusConsignment);
			AssertEquals(outturn2.C5_OutturnResultType, CMROutturnResultType.Codes.NilDiscrepancy);
			AssertEquals(outturn3.C5_OutturnResultType, CMROutturnResultType.Codes.SurplusPackages);
			AssertEquals(outturn4.C5_OutturnResultType, CMROutturnResultType.Codes.ShortLanded);
		}

		public class UPECusMAWBForTest : UPECusMAWB
		{
			public UPECusMAWBForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new void SetOutturnResultType(CusHAWB hAWB, CusOutturn outturn)
			{
				base.SetOutturnResultType(hAWB, outturn);
			}
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			fUPECusMAWB = Factory.New<UPECusMAWBForTest>();
		}

		UPECusMAWB fUPECusMAWB;
	}
}
