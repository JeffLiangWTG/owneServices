using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(OrgImpAddInfo))]
	sealed class OrgImpAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPropertiesReadOnly()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				var orgImpAddInfo = OrgImpAddInfo.Get(Factory.New<OrgHeader>());
				Assert("ZO_IsLVSConsolidatedInfo should be readonly", orgImpAddInfo.ZO_IsLVSConsolidatedInfo.ReadOnly);
				Assert("ZO_IsConsolidateByImporter returns true", orgImpAddInfo.ZO_IsLVSConsolidated);
				Assert("ZO_IsConsolidateByBranchInfo should be readonly", orgImpAddInfo.ZO_IsConsolidateByBranchInfo.ReadOnly);
				Assert("ZO_IsConsolidateByImporter returns false", !orgImpAddInfo.ZO_IsConsolidateByBranch);
				Assert("ZO_IsConsolidateByBrokerInfo should be readonly", orgImpAddInfo.ZO_IsConsolidateByBrokerInfo.ReadOnly);
				Assert("ZO_IsConsolidateByBroker returns false", !orgImpAddInfo.ZO_IsConsolidateByBroker);
				Assert("ZO_IsConsolidateByProvinceofClearanceInfo should be readonly", orgImpAddInfo.ZO_IsConsolidateByProvinceofClearanceInfo.ReadOnly);
				Assert("ZO_IsConsolidateByProvinceofClearance returns false", !orgImpAddInfo.ZO_IsConsolidateByProvinceofClearance);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				var orgImpAddInfo = OrgImpAddInfo.Get(Factory.New<OrgHeader>());
				Assert("ZO_IsLVSConsolidatedInfo should not be readonly", !orgImpAddInfo.ZO_IsLVSConsolidatedInfo.ReadOnly);
				Assert("ZO_IsConsolidateByBranchInfo should not be readonly", !orgImpAddInfo.ZO_IsConsolidateByBranchInfo.ReadOnly);
				Assert("ZO_IsConsolidateByBrokerInfo should not be readonly", !orgImpAddInfo.ZO_IsConsolidateByBrokerInfo.ReadOnly);
				Assert("ZO_IsConsolidateByProvinceofClearanceInfo should not be readonly", !orgImpAddInfo.ZO_IsConsolidateByProvinceofClearanceInfo.ReadOnly);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var orgImpAddInfo = OrgImpAddInfo.Get(Factory.New<OrgHeader>());

			return orgImpAddInfo;
		}

		public void TestZO_CFIAFeePaymentMethod()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			AssertEquals(CFIAPaymentMethods.Codes.RegistryDefault, orgImpAddInfo.ZO_CFIAFeePaymentMethod);
			AssertEquals(CFIAPaymentMethods.Codes.Other, orgImpAddInfo.ZO_EffectiveCFIAFeePaymentMethod);
			orgImpAddInfo.ZO_CFIAFeePaymentMethod = CFIAPaymentMethods.Codes.Importer;
			AssertEquals(CFIAPaymentMethods.Codes.Importer, orgImpAddInfo.ZO_EffectiveCFIAFeePaymentMethod);
		}

		public void TestZO_EffectiveLVSInvoiceDetailCode()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			AssertEquals(LVSInvoiceDetailCodes.Codes.RegistryDefault, orgImpAddInfo.ZO_LVSInvoiceDetailCode);
			AssertEquals(LVSInvoiceDetailCodes.Codes.Summarize, orgImpAddInfo.ZO_EffectiveLVSInvoiceDetailCode);
			orgImpAddInfo.ZO_LVSInvoiceDetailCode = LVSInvoiceDetailCodes.Codes.Detail;
			AssertEquals(LVSInvoiceDetailCodes.Codes.Detail, orgImpAddInfo.ZO_EffectiveLVSInvoiceDetailCode);
		}

		public void TestIsAscPasswordNotSpecified()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			Assert("IsAscPasswordNotSpecified", !orgImpAddInfo.IsAscPasswordNotSpecified);
			orgImpAddInfo.ZO_AccountSecurityNumber = "1";
			Assert("IsAscPasswordNotSpecified", orgImpAddInfo.IsAscPasswordNotSpecified);
			orgImpAddInfo.ZO_AccountSecirityPassword = "1";
			Assert("IsAscPasswordNotSpecified", !orgImpAddInfo.IsAscPasswordNotSpecified);
		}

		public void TestIsGSTDirectAutoRatedReadOnly()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			Assert("IsGSTDirectAutoRated readonly", orgImpAddInfo.ZO_IsGSTDirectAutoRatedInfo.ReadOnly);
			orgImpAddInfo.ZO_IsGSTDirectPayment = true;
			Assert("IsGSTDirectAutoRated not readonly", !orgImpAddInfo.ZO_IsGSTDirectAutoRatedInfo.ReadOnly);
			orgImpAddInfo.ZO_IsGSTDirectAutoRated = true;
			orgImpAddInfo.ZO_IsGSTDirectPayment = false;
			Assert("IsGSTDirectAutoRated readonly", orgImpAddInfo.ZO_IsGSTDirectAutoRatedInfo.ReadOnly);
			Assert("IsGSTDirectAutoRated reset", !orgImpAddInfo.ZO_IsGSTDirectAutoRated);
		}

		public void TestIsGSTDirectPayment()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			Assert("IsGSTDirectPayment readonly", !orgImpAddInfo.ZO_IsGSTDirectPaymentInfo.ReadOnly);

			orgImpAddInfo.ZO_IsGSTDirectPayment = true;
			orgImpAddInfo.ZO_IsImporterDirectPayment = true;
			Assert("IsGSTDirectPayment is readonly", orgImpAddInfo.ZO_IsGSTDirectPaymentInfo.ReadOnly);
			Assert("IsGSTDirectPayment is cleared", !orgImpAddInfo.ZO_IsGSTDirectPayment);

			orgImpAddInfo.ZO_IsImporterDirectPayment = false;
			Assert("IsGSTDirectPayment is readonly", !orgImpAddInfo.ZO_IsGSTDirectPaymentInfo.ReadOnly);
			Assert("IsGSTDirectPayment", !orgImpAddInfo.ZO_IsGSTDirectPayment);
		}

		public void TestAutoRateDutyGSTAmountActiveWhenZO_IsImporterDirectPaymentTicked()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			AssertEquals("ZO_IsHighImporterAutoDutyDirectAmts readonly", false, orgImpAddInfo.ZO_IsImporterDirectPayment);
			AssertEquals("ZO_IsLVSImporterDirectPayment readonly", false, orgImpAddInfo.ZO_IsLVSImporterDirectPayment);
			AssertEquals("ZO_IsHighImporterAutoDutyDirectAmts readonly", false, orgImpAddInfo.ZO_IsHighImporterAutoDutyDirectAmts);
			AssertEquals("ZO_IsLVSImporterDirectPayment readonly", false, orgImpAddInfo.ZO_IsLVSImporterAutoDutyDirectAmts);

			AssertEquals("ZO_IsHighImporterAutoDutyDirectAmts is readonly", true, orgImpAddInfo.ZO_IsHighImporterAutoDutyDirectAmtsInfo.ReadOnly);
			AssertEquals("ZO_IsLVSImporterAutoDutyDirectAmts is readonly", true, orgImpAddInfo.ZO_IsLVSImporterAutoDutyDirectAmtsInfo.ReadOnly);

			orgImpAddInfo.ZO_IsImporterDirectPayment = true;
			AssertEquals("ZO_IsHighImporterAutoDutyDirectAmts is editable", false, orgImpAddInfo.ZO_IsHighImporterAutoDutyDirectAmtsInfo.ReadOnly);
			orgImpAddInfo.ZO_IsLVSImporterDirectPayment = true;
			AssertEquals("ZO_IsLVSImporterDirectPayment is editable", false, orgImpAddInfo.ZO_IsLVSImporterAutoDutyDirectAmtsInfo.ReadOnly);

			orgImpAddInfo.ZO_IsImporterDirectPayment = false;
			AssertEquals("ZO_IsHighImporterAutoDutyDirectAmts readonly", true, orgImpAddInfo.ZO_IsHighImporterAutoDutyDirectAmtsInfo.ReadOnly);
			orgImpAddInfo.ZO_IsLVSImporterDirectPayment = false;
			AssertEquals("ZO_IsLVSImporterAutoDutyDirectAmts readonly", true, orgImpAddInfo.ZO_IsLVSImporterAutoDutyDirectAmtsInfo.ReadOnly);
		}

		public void TestDelayIntervalReadOnly()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			orgImpAddInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.None;
			Assert(orgImpAddInfo.ZO_HVSDelayIntervalAutoSendInfo.ReadOnly);
			orgImpAddInfo.ZO_CONDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.None;
			Assert(orgImpAddInfo.ZO_CONDelayIntervalAutoSendInfo.ReadOnly);
			orgImpAddInfo.ZO_HVSDelayIntervalTypeAutoSend = DelayIntervalTypeCodes.Codes.DAR;
			Assert(!orgImpAddInfo.ZO_HVSDelayIntervalAutoSendInfo.ReadOnly);
			orgImpAddInfo.ZO_HVSDelayIntervalTypeFailSafe = DelayIntervalTypeCodes.Codes.None;
			Assert(orgImpAddInfo.ZO_HVSDelayIntervalFailSafeInfo.ReadOnly);
			orgImpAddInfo.ZO_CONDelayIntervalTypeFailSafe = DelayIntervalTypeCodes.Codes.None;
			Assert(orgImpAddInfo.ZO_CONDelayIntervalFailSafeInfo.ReadOnly);
			orgImpAddInfo.ZO_HVSDelayIntervalTypeFailSafe = DelayIntervalTypeCodes.Codes.DAR;
			Assert(!orgImpAddInfo.ZO_HVSDelayIntervalFailSafeInfo.ReadOnly);
		}

		public void TestZO_IsCreateIndividualLVS()
		{
			var orgImpAddInfo = OrgImpAddInfo.Get(Factory.New<OrgHeader>());
			Assert("ZO_IsCreateIndividualLVS should not be read-only", !orgImpAddInfo.ZO_IsCreateIndividualLVSInfo.ReadOnly);
			orgImpAddInfo.ZO_IsCreateIndividualLVS = true;
			Assert("ZO_IsCreateIndividualLVS", orgImpAddInfo.ZO_IsCreateIndividualLVS);
		}

		public void TestZO_DeferrredNormalB3SendAction()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			orgImpAddInfo.ZO_DeferredNormalB3SendAction = DeferredB3SendActionListOverride.Codes.RegistryDefault;
			AssertEquals(DeferredB3SendActionListOverride.Codes.RegistryDefault, orgImpAddInfo.ZO_DeferredNormalB3SendAction);
			orgImpAddInfo.ZO_DeferredNormalB3SendAction = DeferredB3SendActionListOverride.Codes.Now;
			AssertEquals(DeferredB3SendActionListOverride.Codes.Now, orgImpAddInfo.ZO_DeferredNormalB3SendAction);
			orgImpAddInfo.ZO_DeferredNormalB3SendAction = DeferredB3SendActionListOverride.Codes.Defer;
			AssertEquals(DeferredB3SendActionListOverride.Codes.Defer, orgImpAddInfo.ZO_DeferredNormalB3SendAction);
		}

		public void TestZO_DeferrredLowValueB3SendAction()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			orgImpAddInfo.ZO_DeferredNormalB3SendAction = DeferredB3SendActionListOverride.Codes.RegistryDefault;
			AssertEquals(DeferredB3SendActionListOverride.Codes.RegistryDefault, orgImpAddInfo.ZO_DeferredNormalB3SendAction);
			orgImpAddInfo.ZO_DeferredLowValueB3SendAction = DeferredB3SendActionListOverride.Codes.Now;
			AssertEquals(DeferredB3SendActionListOverride.Codes.Now, orgImpAddInfo.ZO_DeferredLowValueB3SendAction);
			orgImpAddInfo.ZO_DeferredLowValueB3SendAction = DeferredB3SendActionListOverride.Codes.Defer;
			AssertEquals(DeferredB3SendActionListOverride.Codes.Defer, orgImpAddInfo.ZO_DeferredLowValueB3SendAction);
		}

		public void TestZO_ACROSSHighValueProductAuditAction()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			AssertEquals(ProductAuditActions.Codes.RegistryDefault, orgImpAddInfo.ZO_ACROSSHighValueProductAuditAction);
			orgImpAddInfo.ZO_ACROSSHighValueProductAuditAction = ProductAuditActions.Codes.NoAction;
			AssertEquals(ProductAuditActions.Codes.NoAction, orgImpAddInfo.ZO_ACROSSHighValueProductAuditAction);
			orgImpAddInfo.ZO_ACROSSHighValueProductAuditAction = ProductAuditActions.Codes.RegistryDefault;
			AssertEquals(ProductAuditActions.Codes.RegistryDefault, orgImpAddInfo.ZO_ACROSSHighValueProductAuditAction);
		}

		public void TestZO_ACROSSLowValueProductAuditAction()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			AssertEquals(ProductAuditActions.Codes.RegistryDefault, orgImpAddInfo.ZO_ACROSSLowValueProductAuditAction);
			orgImpAddInfo.ZO_ACROSSLowValueProductAuditAction = ProductAuditActions.Codes.NoAction;
			AssertEquals(ProductAuditActions.Codes.NoAction, orgImpAddInfo.ZO_ACROSSLowValueProductAuditAction);
			orgImpAddInfo.ZO_ACROSSLowValueProductAuditAction = ProductAuditActions.Codes.RegistryDefault;
			AssertEquals(ProductAuditActions.Codes.RegistryDefault, orgImpAddInfo.ZO_ACROSSLowValueProductAuditAction);
		}

		public void TestZO_B3HighValueProductAuditAction()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			AssertEquals(ProductAuditActions.Codes.RegistryDefault, orgImpAddInfo.ZO_B3HighValueProductAuditAction);
			orgImpAddInfo.ZO_B3HighValueProductAuditAction = ProductAuditActions.Codes.NoAction;
			AssertEquals(ProductAuditActions.Codes.NoAction, orgImpAddInfo.ZO_B3HighValueProductAuditAction);
			orgImpAddInfo.ZO_B3HighValueProductAuditAction = ProductAuditActions.Codes.RegistryDefault;
			AssertEquals(ProductAuditActions.Codes.RegistryDefault, orgImpAddInfo.ZO_B3HighValueProductAuditAction);
		}

		public void TestZO_B3LowValueProductAuditAction()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			AssertEquals(ProductAuditActions.Codes.RegistryDefault, orgImpAddInfo.ZO_B3LowValueProductAuditAction);
			orgImpAddInfo.ZO_B3LowValueProductAuditAction = ProductAuditActions.Codes.NoAction;
			AssertEquals(ProductAuditActions.Codes.NoAction, orgImpAddInfo.ZO_B3LowValueProductAuditAction);
			orgImpAddInfo.ZO_B3LowValueProductAuditAction = ProductAuditActions.Codes.RegistryDefault;
			AssertEquals(ProductAuditActions.Codes.RegistryDefault, orgImpAddInfo.ZO_B3LowValueProductAuditAction);
		}

		public void TestFreightPercentages()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgImpAddInfo = OrgImpAddInfo.Get(orgHeader);
			var freightPercentage = Factory.New<FreightPercentage>();
			freightPercentage.CY_ParentID = orgHeader.PK;
			var freightPercentages = orgImpAddInfo.FreightPercentages;
			AssertNotNull("Collection", freightPercentages);
			AssertEquals("Count", 1, freightPercentages.Count);
			Assert("Contains", freightPercentages.Contains(freightPercentage.PK));
		}

		public void TestPreventWarningOnSendingB3ReadOnly()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			orgImpAddInfo.ZO_PreventWarningOnSendingB3 = true;
			Assert(!orgImpAddInfo.ZO_PreventWarningOnSendingB3Info.ReadOnly);
			orgImpAddInfo.ZO_PreventWarningOnSendingB3 = false;
			orgImpAddInfo.ZO_IsLVSImporterDirectPayment = true;
			Assert(!orgImpAddInfo.ZO_PreventWarningOnSendingB3Info.ReadOnly);
			orgImpAddInfo.ZO_IsLVSImporterDirectPayment = false;
			orgImpAddInfo.ZO_IsGSTDirectPayment = true;
			Assert(!orgImpAddInfo.ZO_PreventWarningOnSendingB3Info.ReadOnly);
			orgImpAddInfo.ZO_IsGSTDirectPayment = false;
			orgImpAddInfo.ZO_IsImporterDirectPayment = true;
			Assert(!orgImpAddInfo.ZO_PreventWarningOnSendingB3Info.ReadOnly);
			orgImpAddInfo.ZO_IsImporterDirectPayment = false;
			Assert(orgImpAddInfo.ZO_PreventWarningOnSendingB3Info.ReadOnly);
		}

		public void TestSafeFoodLicenses()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgImpAddInfo = OrgImpAddInfo.Get(orgHeader);
			var safeFoodLicense = Factory.New<SafeFoodLicense>();
			safeFoodLicense.CY_ParentID = orgHeader.PK;
			var safeFoodLicenses = orgImpAddInfo.SafeFoodLicenses;
			AssertNotNull("Collection", safeFoodLicenses);
			AssertEquals("Count", 1, safeFoodLicenses.Count);
			Assert("Contains", safeFoodLicenses.Contains(safeFoodLicense.PK));
		}

		public void TestTradeChainPartnersToBeUpdated()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "FENIMP";
			org.OH_FullName = "FENIX IMPORTS INC";

			var orgTCP1 = Factory.New<OrgHeader>();
			orgTCP1.OH_Code = "FENVEN";
			orgTCP1.OH_FullName = "DOLE FRESH VEGETABLES";

			var tcp1Addr = orgTCP1.Addresses.AddNew();
			tcp1Addr.Address1 = "500 S ALTA ST";
			tcp1Addr.Address2 = "";
			tcp1Addr.OA_RN_NKCountryCode = "US";
			tcp1Addr.City = "GONZALES";
			tcp1Addr.Postcode = "93926";
			tcp1Addr.State = "CA";

			var orgImpAddInfo = OrgImpAddInfo.Get(org);

			var tcp1 = orgImpAddInfo.TradeChainPartners.AddNew();
			tcp1.CA_Org = org.PK;
			tcp1.CA_Address = tcp1Addr.PK;
			tcp1.CA_CSAIDType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			tcp1.CA_Action = CSAActionTypeList.Codes.ReqAdd;
			tcp1.CA_CSAID = "645321789";
			Factory.Save();

			var tradeChainPartners = orgImpAddInfo.TradeChainPartners;
			AssertNotNull("Collection", tradeChainPartners);
			AssertEquals("Count", 1, tradeChainPartners.Count);
		}

		public void TestZO_AccountingTimeOption_ReadOnly()
		{
			var orgImpAddInfo = (OrgImpAddInfo)GetNewBusinessObject();
			Assert("Should be readonly", orgImpAddInfo.ZO_AccountingTimeOptionInfo.ReadOnly);
			orgImpAddInfo.ZO_IsCSAApprovedImporter = true;
			Assert("Should not be readonly", !orgImpAddInfo.ZO_AccountingTimeOptionInfo.ReadOnly);
		}
	}
}
