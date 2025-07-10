using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class JPAFRHeaderSailingSynchroniserTest : AFRSynchroniserTestCase
	{
		public void TestSynchroniseCarrier()
		{
			header.Carrier.E2_AddressOverride = ZBool.True;
			header.Synchroniser.Synchronise(true);
			AssertEquals(true, header.Carrier.ReadOnly);
			AssertEquals(ZBool.False, header.Carrier.E2_AddressOverride);
			AssertEquals(ZGuid.Empty, header.Carrier.E2_OA_Address);

			sourceVoyage.JV_OH_Line = TestShippingLine.PK;
			AssertEquals(TestShippingLine.PK, sourceVoyage.JV_OH_Line);
			AssertEquals(TestShippingLine.PK, sourceSailing.JX_JV_OH_Line);
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(true, header.Carrier.ReadOnly);
			AssertEquals(ZBool.False, header.Carrier.E2_AddressOverride);
			AssertEquals(TestShippingLine.MainAddress.PK, header.Carrier.E2_OA_Address);

			sourceVoyage.JV_OH_Line = TestShippingLine2.PK;
			AssertEquals(TestShippingLine2.PK, sourceSailing.JX_JV_OH_Line);
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(true, header.Carrier.ReadOnly);
			AssertEquals(ZBool.False, header.Carrier.E2_AddressOverride);
			AssertEquals(TestShippingLine2.MainAddress.PK, header.Carrier.E2_OA_Address);

			sourceVoyage.JV_OH_Line = TestShippingLine3.PK;
			AssertEquals(TestShippingLine3.PK, sourceSailing.JX_JV_OH_Line);
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(true, header.Carrier.ReadOnly);
			AssertEquals(ZBool.False, header.Carrier.E2_AddressOverride);
			AssertEquals(TestShippingLine3.MainAddress.PK, header.Carrier.E2_OA_Address);

			header.Synchroniser.SetEnabled(false, false);
			AssertEquals(false, header.Carrier.ReadOnly);
		}

		public void TestSynchroniseJPH_CarrierCode()
		{
			AssertEquals(string.Empty, header.JPH_CarrierCode);
			TestShippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);

			AssertEquals(string.Empty, header.JPH_CarrierCode);
			sourceVoyage.JV_OH_Line = TestShippingLine.PK;
			AssertEquals(TestShippingLine.PK, sourceVoyage.JV_OH_Line);
			AssertEquals(TestShippingLine.PK, sourceSailing.JX_JV_OH_Line);
			header.ChangeSailing(sourceSailing.PK);

			AssertEquals("SPQA", header.JPH_CarrierCode);
		}

		public void TestSynchroniseJPH_CarrierCode_WhenChangingFromOverridenToDefault()
		{
			var synchroniser = header.Synchroniser;
			TestShippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SPQA", Core.Constants.CountryCodes.Japan);
			AssertEquals(string.Empty, header.JPH_CarrierCode);

			sourceVoyage.JV_OH_Line = TestShippingLine.PK;
			header.ChangeSailing(sourceSailing.PK);

			synchroniser.SetEnabled(false, false);
			AssertEquals("SPQA", header.JPH_CarrierCode);
			header.JPH_CarrierCode = "SPQB";
			AssertEquals("SPQB", header.JPH_CarrierCode);

			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise(true);
			AssertEquals("SPQA", header.JPH_CarrierCode);
		}

		public void TestSynchroniseJPH_VesselName()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "VESSEL 1A";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "VESSEL 3";

			var synchroniser = header.Synchroniser;
			sourceVoyage.JV_RV_NKVessel = vessel1.RV_FK;
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals("VESSEL 1A", header.JPH_VesselName);

			sourceVoyage.JV_RV_NKVessel = vessel2.RV_FK;
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals("VESSEL 3", header.JPH_VesselName);
		}

		public void TestSynchroniseJPH_Voyage()
		{
			sourceVoyage.JV_VoyageFlight = "V1";
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals("V1", header.JPH_Voyage);

			sourceVoyage.JV_VoyageFlight = "V3";
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals("V3", header.JPH_Voyage);
		}

		public void TestSynchroniseJPH_RL_NKDischarge()
		{
			sourceDestination.JB_RL_NKPortOfDischarge = JPTKY.RL_Code;
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(JPTKY.RL_Code, header.JPH_RL_NKDischarge);

			sourceDestination.JB_RL_NKPortOfDischarge = JPHTR.RL_Code;
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(JPHTR.RL_Code, header.JPH_RL_NKDischarge);
		}

		public void TestSynchroniseJPH_RL_NKLoading()
		{
			sourceOrigin.JA_RL_NKPortOfLoading = AUSYD.RL_Code;
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(AUSYD.RL_Code, header.JPH_RL_NKLoading);

			sourceOrigin.JA_RL_NKPortOfLoading = SGSIN.RL_Code;
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(SGSIN.RL_Code, header.JPH_RL_NKLoading);
		}

		public void TestSynchroniseJPH_ETD()
		{
			sourceOrigin.JA_E_DEP = new ZDateTime(2013, 10, 8);
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(new ZDateTime(2013, 10, 8), header.JPH_ETD);

			sourceOrigin.JA_E_DEP = new ZDateTime(2013, 10, 9);
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(new ZDateTime(2013, 10, 9), header.JPH_ETD);

			sourceOrigin.JA_A_DEP = new ZDateTime(2013, 10, 8);
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(new ZDateTime(2013, 10, 8), header.JPH_ETD);

			sourceOrigin.JA_A_DEP = ZDateTime.Empty;
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(new ZDateTime(2013, 10, 9), header.JPH_ETD);
		}

		public void TestSynchroniseJPH_ETA()
		{
			sourceDestination.JB_E_ARV = new ZDateTime(2013, 10, 8);
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(new ZDateTime(2013, 10, 8), header.JPH_ETA);

			sourceDestination.JB_E_ARV = new ZDateTime(2013, 10, 9);
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(new ZDateTime(2013, 10, 9), header.JPH_ETA);

			sourceDestination.JB_A_ARV = new ZDateTime(2013, 10, 8);
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(new ZDateTime(2013, 10, 8), header.JPH_ETA);

			sourceDestination.JB_A_ARV = ZDateTime.Empty;
			header.ChangeSailing(sourceSailing.PK);
			AssertEquals(new ZDateTime(2013, 10, 9), header.JPH_ETA);
		}

		public OrgHeader TestShippingLine2
		{
			get
			{
				if (testShippingLine2 == null)
				{
					testShippingLine2 = Factory.New<OrgHeader>();
					testShippingLine2.OH_FullName = "Synchroniser Shipping Line 2";
					testShippingLine2.OH_Code = "TSTSHPLNE2";
					testShippingLine2.OH_IsShippingLine = true;
				}
				return testShippingLine2;
			}
		}
		OrgHeader testShippingLine2;

		public OrgHeader TestShippingLine3
		{
			get
			{
				if (testShippingLine3 == null)
				{
					testShippingLine3 = Factory.New<OrgHeader>();
					testShippingLine3.OH_FullName = "Synchroniser Shipping Line 3";
					testShippingLine3.OH_Code = "TSTSHPLNE3";
					testShippingLine3.OH_IsShippingLine = true;
				}
				return testShippingLine3;
			}
		}
		OrgHeader testShippingLine3;

		JobSailing sourceSailing;
		JobVoyage sourceVoyage;
		VoyageOrigin sourceOrigin;
		VoyageDestination sourceDestination;

		protected override void SetUp()
		{
			base.SetUp();
			SetUpSailingSource();
			header.JPH_ParentTableCode = sourceSailing.TablePrefix;
			header.JPH_ParentId = sourceSailing.PK;
			header.JPH_IsShippingLineEntry = true;
			header.ChangeSailing(sourceSailing.PK);
		}

		void SetUpSailingSource()
		{
			sourceVoyage = Factory.New<JobVoyage>();
			sourceVoyage.JV_RV_NKVessel = TestRV_NKVessel;
			sourceVoyage.JV_VoyageFlight = TestVoyage;

			sourceDestination = Factory.New<VoyageDestination>();
			sourceDestination.JB_RL_NKPortOfDischarge = JPTKY.Code;

			sourceOrigin = Factory.New<VoyageOrigin>();
			sourceOrigin.JA_RL_NKPortOfLoading = AUSYD.Code;
			sourceOrigin.JA_JV = sourceVoyage.PK;

			sourceSailing = Factory.New<JobSailing>();
			sourceSailing.JX_JA = sourceOrigin.PK;
			sourceSailing.JX_JB = sourceDestination.PK;
		}
	}
}
