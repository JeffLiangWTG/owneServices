using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class ChargeJRJExtensionsTest : TestCaseWithFactory
	{
		public void TestAreInternalFieldsEmpty()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Charge 1");
			AssertAreInternalFieldsEmpty("AreInternalFieldsEmpty should be false, since all internal fields are valued.",
				charge.JR_JH, charge.JR_GB, charge.JR_GE,
				expectedResult: false
			);
			AssertAreInternalFieldsEmpty("AreInternalFieldsEmpty should be false, even only internal job is blank.",
				ZGuid.Empty, charge.JR_GB, charge.JR_GE,
				expectedResult: false
			);
			AssertAreInternalFieldsEmpty("AreInternalFieldsEmpty should be false, even only internal branch is blank.",
				charge.JR_JH, ZGuid.Empty, charge.JR_GE,
				expectedResult: false
			);
			AssertAreInternalFieldsEmpty("AreInternalFieldsEmpty should be false, even only internal dept is blank.",
				charge.JR_JH, charge.JR_GB, ZGuid.Empty,
				expectedResult: false
			);
			AssertAreInternalFieldsEmpty("AreInternalFieldsEmpty should be false, even only internal job is valued.",
				charge.JR_JH, ZGuid.Empty, ZGuid.Empty,
				expectedResult: false
			);
			AssertAreInternalFieldsEmpty("AreInternalFieldsEmpty should be false, even only internal branch is valued.",
				ZGuid.Empty, charge.JR_GB, ZGuid.Empty,
				expectedResult: false
			);
			AssertAreInternalFieldsEmpty("AreInternalFieldsEmpty should be false, even only internal dept is valued.",
				ZGuid.Empty, ZGuid.Empty, charge.JR_GE,
				expectedResult: false
			);
			AssertAreInternalFieldsEmpty("AreInternalFieldsEmpty should be true, since all internal fields are blank.",
				ZGuid.Empty, ZGuid.Empty, ZGuid.Empty,
				expectedResult: true
			);

			void AssertAreInternalFieldsEmpty(string comment, ZGuid internalJob, ZGuid internalBranch, ZGuid internalDept, bool expectedResult)
			{
				using (charge.SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
				{
					charge.JR_JH_InternalJob = internalJob;
					charge.JR_GB_InternalBranch = internalBranch;
					charge.JR_GE_InternalDept = internalDept;
				}
				AssertEquals(comment, expectedResult, charge.AreInternalFieldsEmpty());
			}
		}

		public void TestIsExcludedFromAutoJRJ_EnableAutoJRJWithTaxRegNum() => AssertIsExcludedFromAutoJRJ(true);

		public void TestIsExcludedFromAutoJRJ_DisableAutoJRJWithTaxRegNum() => AssertIsExcludedFromAutoJRJ(false);

		void AssertIsExcludedFromAutoJRJ(bool shouldEnableAutoJRJWithTaxRegNum)
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			if (shouldEnableAutoJRJWithTaxRegNum)
			{
				AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly(companyPK);
			}
			else
			{
				AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(companyPK);
			}

			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var registeredNumberCodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			AssertEquals("PreCondition", CountryCodes.Australia, currentCountryCode);

			const string registeredNumberGroup1 = "21 003 980 130";
			const string registeredNumberGroup2 = "21 003 980 130 123";

			GlbBranch.CurrentBranch.OrgProxy.CompanyData.SetAPTaxApplicable(true);
			GlbBranch.CurrentBranch.OrgProxy.CompanyData.SetARTaxApplicable(true);
			TestObjectCreator.SetCustomsCodeForOrgHeader(GlbBranch.CurrentBranch.OrgProxy, registeredNumberCodeType, currentCountryCode, registeredNumberGroup1);

			var orgWithRegisteredNumberroup2 = TestObjectCreator.CreateOrgHeader("AA1", true, true);
			TestObjectCreator.SetCustomsCodeForOrgHeader(orgWithRegisteredNumberroup2, registeredNumberCodeType, currentCountryCode, registeredNumberGroup2);
			var branchWithRegisteredNumberroup2 = TestObjectCreator.CreateBranch("AA1", GlbCompany.CurrentCompany, orgWithRegisteredNumberroup2);

			var org2WithRegisteredNumberroup2 = TestObjectCreator.CreateOrgHeader("AA2", true, true);
			TestObjectCreator.SetCustomsCodeForOrgHeader(org2WithRegisteredNumberroup2, registeredNumberCodeType, currentCountryCode, registeredNumberGroup2);
			TestObjectCreator.CreateBranch("AA2", GlbCompany.CurrentCompany, org2WithRegisteredNumberroup2);

			var orgWithEmptyRegisteredNumber = TestObjectCreator.CreateOrgHeader("BB1", true, true);
			var branchWithEmptyRegisteredNumber = TestObjectCreator.CreateBranch("BB1", GlbCompany.CurrentCompany, orgWithEmptyRegisteredNumber);
			AssertNull("PreCondition", branchWithEmptyRegisteredNumber.OrgProxy.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == registeredNumberCodeType && x.OK_RN_NKCodeCountry == currentCountryCode));

			var branchWithEmptyOrgProxy = TestObjectCreator.CreateBranch("BB2", GlbCompany.CurrentCompany);
			branchWithEmptyOrgProxy.GB_OH_OrgProxy = ZGuid.Empty;

			GlbBranch.CurrentBranch.Factory.Save();
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = TestObjectCreator.CreateJob(shipment);
			job.PlugInData = shipment;
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.FRT, "Charge 1");

			AssertWhenChargeBranchHaveOrgRegisteredNumber();
			AssertWhenChargeBranchIsBlank();
			AssertWhenChargeBranchOrgProxyHaveNoRegisteredNumber();
			AssertWhenChargeBranchHaveNoOrgProxy();
			AssertWhenOrgAccountIsNull();

			void AssertWhenChargeBranchHaveOrgRegisteredNumber()
			{
				charge.JR_GB = branchWithRegisteredNumberroup2.PK;
				AssertEquals("Precondition", registeredNumberGroup2, charge.Branch.OrgProxy.CustomsCodes.Cast<OrgCusCode>().Single(x => x.OK_CodeType == registeredNumberCodeType && x.OK_RN_NKCodeCountry == currentCountryCode).OK_CustomsRegNo);

				AssertEquals(GetCommentWithSettingInfo("[Charge Branch have registered number]When OrgAccount is Charge Branch's OrgProxy."),
					false,
					charge.IsExcludedFromAutoJRJ(orgWithRegisteredNumberroup2));

				AssertEquals(GetCommentWithSettingInfo("[Charge Branch have registered number]When OrgAccount is not Charge Branch's OrgProxy but have same registered number."),
					false,
					charge.IsExcludedFromAutoJRJ(org2WithRegisteredNumberroup2));

				AssertEquals(GetCommentWithSettingInfo("[Charge Branch have registered number]When OrgAccount is not Charge Branch's OrgProxy and have different registered number."),
					shouldEnableAutoJRJWithTaxRegNum,
					charge.IsExcludedFromAutoJRJ(GlbBranch.CurrentBranch.OrgProxy));

				AssertEquals(GetCommentWithSettingInfo("[Charge Branch have registered number]When OrgAccount is not Charge Branch's OrgProxy and have no registered number."),
					shouldEnableAutoJRJWithTaxRegNum,
					charge.IsExcludedFromAutoJRJ(orgWithEmptyRegisteredNumber));
			}

			void AssertWhenChargeBranchIsBlank()
			{
				charge.JR_GB = ZGuid.Empty;
				AssertNull(charge.Branch);

				AssertEquals(GetCommentWithSettingInfo("[Charge Branch is null]When OrgAccount have registered number."),
					shouldEnableAutoJRJWithTaxRegNum,
					charge.IsExcludedFromAutoJRJ(orgWithRegisteredNumberroup2));

				AssertEquals(GetCommentWithSettingInfo("[Charge Branch is null]When OrgAccount have no registered number."),
					shouldEnableAutoJRJWithTaxRegNum,
					charge.IsExcludedFromAutoJRJ(orgWithEmptyRegisteredNumber));
			}

			void AssertWhenChargeBranchOrgProxyHaveNoRegisteredNumber()
			{
				charge.JR_GB = branchWithEmptyRegisteredNumber.PK;
				AssertEquals("Precondition", false, charge.Branch.OrgProxy.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_CodeType == registeredNumberCodeType && x.OK_RN_NKCodeCountry == currentCountryCode));

				AssertEquals(GetCommentWithSettingInfo("[Charge Branch have no registered number]When OrgAccount have registered number."),
					shouldEnableAutoJRJWithTaxRegNum,
					charge.IsExcludedFromAutoJRJ(orgWithRegisteredNumberroup2));

				AssertEquals(GetCommentWithSettingInfo("[Charge Branch have no registered number]When OrgAccount have no registered number."),
					false,
					charge.IsExcludedFromAutoJRJ(orgWithEmptyRegisteredNumber));
			}

			void AssertWhenChargeBranchHaveNoOrgProxy()
			{
				charge.JR_GB = branchWithEmptyOrgProxy.PK;
				AssertNull(charge.Branch.OrgProxy);

				AssertEquals(GetCommentWithSettingInfo("[Charge Branch have no OrgProxy]When OrgAccount have registered number."),
					shouldEnableAutoJRJWithTaxRegNum,
					charge.IsExcludedFromAutoJRJ(orgWithRegisteredNumberroup2));

				AssertEquals(GetCommentWithSettingInfo("[Charge Branch have no OrgProxy]When OrgAccount have no registered number."),
					false,
					charge.IsExcludedFromAutoJRJ(orgWithEmptyRegisteredNumber));
			}

			void AssertWhenOrgAccountIsNull()
			{
				AssertEquals(GetCommentWithSettingInfo("When OrgAccount is null."), shouldEnableAutoJRJWithTaxRegNum, charge.IsExcludedFromAutoJRJ(null));
			}

			string GetCommentWithSettingInfo(string comment)
			{
				var result = new ZStringBuilder();
				result.Append($"[EnableAJLForBranchWithSameTaxRegistration:{shouldEnableAutoJRJWithTaxRegNum}]");
				result.Append(comment);
				return result.ToString();
			}
		}

		public void TestIsNotEligableForAutoJRJ()
		{
			var shipment = TestObjectCreator.CreateShipment("S00000001");
			var job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			TestObjectCreator.PrepareAutoJRJTestEnvironment();
			TestObjectCreator.PrepareAutoJRJTaxRegistrationNumbers(false);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 190m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0m, null);
			AssertIsNotEligableForAutoJRJ("Cost Account is org proxy, tax registration number are different", charge, true, false, true, true, true);

			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 0m, null, TestObjectCreator.AUD, 190m, TestObjectCreator.AALSHI);
			AssertIsNotEligableForAutoJRJ("Sell Account is org proxy, tax registration number are different", charge, false, true, true, true, true);

			TestObjectCreator.PrepareAutoJRJTaxRegistrationNumbers(true, false);

			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 190m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0m, null);
			AssertIsNotEligableForAutoJRJ("Cost Account is org proxy, tax registration number are the same", charge, true, false, false, true, false);

			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 0m, null, TestObjectCreator.AUD, 190m, TestObjectCreator.AALSHI);
			AssertIsNotEligableForAutoJRJ("Sell Account is org proxy, tax registration number are the same", charge, false, true, true, false, false);

			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 190m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0m, null);
			AssertIsNotEligableForAutoJRJ("Cost Account is org proxy, tax registration number feature is off", charge, true, false, false, false, false);

			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 0m, null, TestObjectCreator.AUD, 190m, TestObjectCreator.AALSHI);
			AssertIsNotEligableForAutoJRJ("Sell Account is org proxy, tax registration number feature is off", charge, false, true, false, false, false);
		}

		void AssertIsNotEligableForAutoJRJ(string message, Charge charge, bool isCostAccountOrgProxy, bool isSellAccountOrgProxy, bool isCostExcludedFromAutoJRJ, bool isSellExcludedFromAutoJRJ, bool expectedValue)
		{
			AssertEquals(isCostAccountOrgProxy, charge.CostAccountIsOrgProxy);
			AssertEquals(isSellAccountOrgProxy, charge.SellAccountIsOrgProxy);
			AssertEquals(isCostExcludedFromAutoJRJ, charge.IsExcludedFromAutoJRJ(charge.CostAccount));
			AssertEquals(isSellExcludedFromAutoJRJ, charge.IsExcludedFromAutoJRJ(charge.SellAccount));
			AssertEquals(message, expectedValue, charge.IsNotEligableForAutoJRJ());
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
