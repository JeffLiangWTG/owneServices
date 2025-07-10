using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ARAP.Invoicing.Testing
{
	public class SurchargeConfigTest : TestCaseWithFactory
	{
		public void TestGetSurcharge()
		{
			var testObjectCreateor = new TestObjectCreator(Factory);

			testObjectCreateor.CC2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;

			var surchargeConfig = Factory.New<AccSurchargeConfiguration>();
			surchargeConfig.ASC_GC_Company = GlbCompany.CurrentCompany.PK;
			surchargeConfig.ASC_Code = "TS1";
			surchargeConfig.ASC_Description = "SurchargeConfig Test 1";
			surchargeConfig.ASC_AC_ChargeCode = testObjectCreateor.CC1.PK;
			surchargeConfig.ASC_Type = "PER";
			surchargeConfig.ASC_Rate = 1M;
			surchargeConfig.ASC_BasisType = "ALL";

			Factory.Save();

			ISurchargeConfig calculator = new SurchargeConfig();
			(var rate, var chargeCodePK) = calculator.GetSurcharge("TS2", testObjectCreateor.CC2.PK);
			AssertEquals("Rate is 0 when surcharge TS2 is not found.", 0M, rate);
			AssertEquals("chargeCodePK is empty when surcharge TS2 is not found.", ZGuid.Empty, chargeCodePK);

			(rate, chargeCodePK) = calculator.GetSurcharge("TS1", testObjectCreateor.CC2.PK);

			AssertEquals("Rate is 1 when surcharge TS1 is found.", 1M, rate);
			AssertEquals("chargeCodePK is CC1 PK when surcharge TS1 is found.", testObjectCreateor.CC1.PK, chargeCodePK);

			surchargeConfig.ASC_BasisType = "EXC";
			var surchargeBasis = Factory.New<AccSurchargeBasis>();
			surchargeBasis.ASB_ASC_SurchargeConfiguration = surchargeConfig.PK;
			surchargeBasis.ASB_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;

			Factory.Save();

			(rate, chargeCodePK) = calculator.GetSurcharge("TS1", testObjectCreateor.CC2.PK);

			AssertEquals("Rate is 0 because surcharge TS1 is not match when BasisType is EXC and has SurchargeBasis which ASB_ChargeGroup is Brokerage.", 0M, rate);
			AssertEquals("chargeCodePK is empty because surcharge TS1 is not match when BasisType is EXC and has SurchargeBasis which ASB_ChargeGroup is Brokerage.", ZGuid.Empty, chargeCodePK);

			surchargeBasis.ASB_ChargeGroup = ChargeCodeGroupList.Codes.CFSLoadList;
			Factory.Save();

			(rate, chargeCodePK) = calculator.GetSurcharge("TS1", testObjectCreateor.CC2.PK);

			AssertEquals("Rate is 1 because surcharge TS1 is match when BasisType is EXC and has SurchargeBasis which ASB_ChargeGroup is CFSLoadList.", 1M, rate);
			AssertEquals("chargeCodePK is CC1 PK because surcharge TS1 is match when BasisType is EXC and has SurchargeBasis which ASB_ChargeGroup is CFSLoadList.", testObjectCreateor.CC1.PK, chargeCodePK);

			surchargeBasis.ASB_ChargeGroup = null;
			surchargeBasis.ASB_AC_ChargeCode = testObjectCreateor.CC2.PK;
			Factory.Save();

			(rate, chargeCodePK) = calculator.GetSurcharge("TS1", testObjectCreateor.CC2.PK);

			AssertEquals("Rate is 0 because surcharge TS1 is not match when BasisType is EXC and has SurchargeBasis which ASB_AC_ChargeCode is CC2.", 0M, rate);
			AssertEquals("chargeCodePK is empty because surcharge TS1 is not match when BasisType is EXC and has SurchargeBasis which ASB_AC_ChargeCode is CC2.", ZGuid.Empty, chargeCodePK);

			surchargeBasis.ASB_AC_ChargeCode = testObjectCreateor.CC3.PK;
			Factory.Save();

			(rate, chargeCodePK) = calculator.GetSurcharge("TS1", testObjectCreateor.CC2.PK);

			AssertEquals("Rate is 1 because surcharge TS1 is match when BasisType is EXC and has SurchargeBasis which ASB_AC_ChargeCode is CC3.", 1M, rate);
			AssertEquals("chargeCodePK is CC1 PK because surcharge TS1 is match when BasisType is EXC and has SurchargeBasis which ASB_AC_ChargeCode is CC3.", testObjectCreateor.CC1.PK, chargeCodePK);

			surchargeConfig.ASC_BasisType = "INC";
			surchargeBasis.ASB_AC_ChargeCode = ZGuid.Empty;
			surchargeBasis.ASB_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			Factory.Save();

			(rate, chargeCodePK) = calculator.GetSurcharge("TS1", testObjectCreateor.CC2.PK);

			AssertEquals("Rate is 1 because surcharge TS1 is match when BasisType is INC and has SurchargeBasis which ASB_ChargeGroup is Brokerage.", 1M, rate);
			AssertEquals("chargeCodePK is CC1 PK because surcharge TS1 is match when BasisType is INC and has SurchargeBasis which ASB_ChargeGroup is Brokerage.", testObjectCreateor.CC1.PK, chargeCodePK);

			surchargeBasis.ASB_ChargeGroup = ChargeCodeGroupList.Codes.CFSLoadList;
			Factory.Save();

			(rate, chargeCodePK) = calculator.GetSurcharge("TS1", testObjectCreateor.CC2.PK);

			AssertEquals("Rate is 0 because surcharge TS1 is not match when BasisType is INC and has SurchargeBasis which ASB_ChargeGroup is CFSLoadList.", 0M, rate);
			AssertEquals("chargeCodePK is empty because surcharge TS1 is not match when BasisType is INC and has SurchargeBasis which ASB_ChargeGroup is CFSLoadList.", ZGuid.Empty, chargeCodePK);

			surchargeBasis.ASB_ChargeGroup = null;
			surchargeBasis.ASB_AC_ChargeCode = testObjectCreateor.CC3.PK;
			Factory.Save();

			(rate, chargeCodePK) = calculator.GetSurcharge("TS1", testObjectCreateor.CC2.PK);

			AssertEquals("Rate is 0 because surcharge TS1 is not match when BasisType is INC and has SurchargeBasis which ASB_AC_ChargeCode is CC3.", 0M, rate);
			AssertEquals("chargeCodePK is empty because surcharge TS1 is not match when BasisType is INC and has SurchargeBasis which ASB_AC_ChargeCode is CC3.", ZGuid.Empty, chargeCodePK);

			surchargeBasis.ASB_AC_ChargeCode = testObjectCreateor.CC2.PK;
			Factory.Save();

			(rate, chargeCodePK) = calculator.GetSurcharge("TS1", testObjectCreateor.CC2.PK);

			AssertEquals("Rate is 1 because surcharge TS1 is not match when BasisType is INC and has SurchargeBasis which ASB_AC_ChargeCode is CC2.", 1M, rate);
			AssertEquals("chargeCodePK is CC1 PK because surcharge TS1 is not match when BasisType is INC and has SurchargeBasis which ASB_AC_ChargeCode is CC2.", testObjectCreateor.CC1.PK, chargeCodePK);
		}
	}
}
