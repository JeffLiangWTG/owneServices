using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusSCAHouseCollectionForOceanBill))]
	sealed class CusSCAHouseCollectionForOceanBillTest : ActiveBusinessObjectCollectionTestCase<CusSCAHouseCollectionForOceanBill>
	{
		public void TestSetDefaultsForNewChild()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			CusSCAOceanBill.CB_ParentId = consol.PK;
			CusSCAOceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var houseBill = CusSCAOceanBill.HouseBills.AddNew();
			Factory.Save();
			AssertEquals("CA_FROBTransitImportCode", ZString.Empty, houseBill.CA_FROBTransitImportCode);

			houseBill = CusSCAOceanBill.HouseBills.AddNew();
			houseBill.CA_FROBTransitImportCode = InTransitCodeList.Codes.FROB;
			Factory.Save();
			AssertEquals("CA_FROBTransitImportCode", InTransitCodeList.Codes.FROB, houseBill.CA_FROBTransitImportCode);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Canada))
			{
				FreightDataRegistry.Instance.CanadaConsolCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MBL");
				using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					consol = Factory.NewWithValidTestData<ForwardingConsol>();
					CusSCAOceanBill.CB_ParentId = consol.PK;
					var transportLeg1 = consol.Transports.AddNew();
					transportLeg1.JW_RL_NKLoadPort = "USPHL";
					transportLeg1.JW_RL_NKDiscPort = "CAYYZ";
					transportLeg1.JW_ETD = ZDateTime.Today;
					var transportLeg2 = consol.Transports.AddNew();
					transportLeg2.JW_RL_NKLoadPort = "CAYYZ";
					transportLeg2.JW_RL_NKDiscPort = "GBPME";
					consol.JK_RL_NKDischargePort = "GBPME";
					transportLeg2.JW_ETA = ZDateTime.Today;
					consol.JK_MasterBillNum = "234";
					Factory.Save();
					CusSCAOceanBill.HouseBills.DeleteAll();
					houseBill = CusSCAOceanBill.HouseBills.AddNew();
					AssertEquals(InTransitCodeList.Codes.FROB, houseBill.CA_FROBTransitImportCode);
				}
			}

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Canada))
			{
				FreightDataRegistry.Instance.CanadaConsolCargoControlNumberCustomization.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "MBL");
				using (FreightDataRegistry.Instance.AWBTracking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					consol = Factory.NewWithValidTestData<ForwardingConsol>();
					CusSCAOceanBill.CB_ParentId = consol.PK;
					CusSCAOceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
					var transportLeg1 = consol.Transports.AddNew();
					transportLeg1.JW_RL_NKLoadPort = "USPHL";
					transportLeg1.JW_RL_NKDiscPort = "CAYYZ";
					transportLeg1.JW_ETD = ZDateTime.Today;
					var transportLeg2 = consol.Transports.AddNew();
					transportLeg2.JW_RL_NKLoadPort = "CAYYZ";
					transportLeg2.JW_RL_NKDiscPort = "CAAJN";
					consol.JK_RL_NKDischargePort = "CAAJN";
					transportLeg2.JW_ETA = ZDateTime.Today;
					consol.JK_MasterBillNum = "234";
					Factory.Save();
					CusSCAOceanBill.HouseBills.DeleteAll();
					houseBill = CusSCAOceanBill.HouseBills.AddNew();
					AssertEquals(ZString.Empty, houseBill.CA_FROBTransitImportCode);
				}
			}
		}

		#region Implementation

		CusSCAOceanBill CusSCAOceanBill
		{
			get { return cusSCAOceanBill ?? (cusSCAOceanBill = Factory.New<CusSCAOceanBill>()); }
		}
		CusSCAOceanBill cusSCAOceanBill;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusSCAHouse>();
		}

		protected override CusSCAHouseCollectionForOceanBill GetCollectionToTest()
		{
			return new CusSCAHouseCollectionForOceanBill(CusSCAOceanBill);
		}

		#endregion
	}
}
