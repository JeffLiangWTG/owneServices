namespace Enterprise.Accounting.Business.GlobalChargeCode.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;

	internal class GlobalChargeCodeMapPivotIntercompanyValidationTest : AccGlobalChargeCodeMapPivotValidationTest
	{
		protected override BusinessObjectCollection GetGlobalChargeCodePivotCollection
		{
			get
			{
				return new GlobalChargeCodeMapPivotIntercompanyCollection(Factory);
			}
		}

		public void TestCheckYP_TYPEMustBeAPIfLocalClientOverride()
		{
			BusinessObjectCollection globalChargeCodePivotCollection = GetGlobalChargeCodePivotCollection;
			GlobalChargeCodeMapPivot pivot1 = (GlobalChargeCodeMapPivot)globalChargeCodePivotCollection.AddNew();
			pivot1.YP_OH_LocalClientOverride = Factory.NewWithValidTestData<OrgHeader>().PK;
			pivot1.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			AssertNoErrors(pivot1.YP_TYPEInfo);
			pivot1.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			AssertHasError(pivot1.YP_TYPEInfo, "You can only select AP Ledger when Job Local Client is set.");
			pivot1.YP_OH_LocalClientOverride = ZGuid.Empty;
			pivot1.Validation.ValidateAll();
			AssertNoErrors("Type can be AR if local client override is not set", pivot1.YP_TYPEInfo);
		}

		public void TestYP_ACARChargeCodeUniqness_WithoutLocalClientOverride()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode anotherChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			globalChargeCode.YG_Code = "TEST";
			GlobalChargeCodeMapPivotIntercompany pivot = globalChargeCode.PivotWithoutOverrideLocalClientCollection.AddNew();
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot.YP_AC = chargeCode.PK;
			Factory.Save();
			globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			pivot = globalChargeCode.PivotWithoutOverrideLocalClientCollection.AddNew();
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot.YP_AC = chargeCode.PK;
			AssertHasError(pivot.YP_ACInfo, "Current Charge Code with AR ledger is already mapped for TEST Global Charge Code.");
			pivot.YP_AC = anotherChargeCode.PK;
			AssertNoErrors("expect no errors found", pivot.YP_ACInfo);
		}

		public void TestYP_ACARChargeCodeUniqness_WithLocalClientOverride()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode anotherChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			globalChargeCode.YG_Code = "TEST";
			GlobalChargeCodeMapPivotIntercompany pivot = globalChargeCode.PivotWithOverrideLocalClientCollection.AddNew();
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot.YP_AC = chargeCode.PK;
			pivot.YP_OH_LocalClientOverride = org1.PK;
			Factory.Save();
			globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			pivot = globalChargeCode.PivotWithOverrideLocalClientCollection.AddNew();
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot.YP_OH_LocalClientOverride = org1.PK;
			pivot.YP_AC = chargeCode.PK;
			AssertHasError(pivot.YP_ACInfo, "Current Charge Code with AR ledger and Override local client is already mapped for TEST Global Charge Code.");
			pivot.YP_AC = anotherChargeCode.PK;
			AssertNoErrors("expect no errors found", pivot.YP_ACInfo);
			pivot.YP_AC = chargeCode.PK;
			pivot.YP_OH_LocalClientOverride = org1.PK;
			AssertHasError(pivot.YP_OH_LocalClientOverrideInfo, "Current Charge Code with AR ledger and Override local client is already mapped for TEST Global Charge Code.");
			pivot.YP_OH_LocalClientOverride = org2.PK;
			AssertNoErrors("expect no errors found", pivot.YP_OH_LocalClientOverrideInfo);
		}

		public void TestAPLedgerNotUniqueErrorMessage()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode anotherChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			globalChargeCode.YG_Code = "TEST";
			GlobalChargeCodeMapPivotIntercompany pivot = globalChargeCode.PivotWithOverrideLocalClientCollection.AddNew();
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			pivot.YP_AC = chargeCode.PK;
			pivot.YP_OH_LocalClientOverride = org1.PK;
			Factory.Save();
			pivot = globalChargeCode.PivotWithOverrideLocalClientCollection.AddNew();
			pivot.YP_AC = anotherChargeCode.PK;
			pivot.YP_OH_LocalClientOverride = org1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			AssertHasError(pivot.YP_TYPEInfo, "You can only create a single AP Charge Code per mapping for each override local client");
			pivot.YP_OH_LocalClientOverride = org2.PK;
			pivot.Validation.ValidateAll();
			AssertNoErrors("expect no errors found", pivot.YP_TYPEInfo);
		}

		public void TestLocalClientShouldBeARWarningMessage()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.CompanyData.OB_IsDebtor = false;
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.CompanyData.OB_IsDebtor = true;
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			globalChargeCode.YG_Code = "TEST";
			GlobalChargeCodeMapPivotIntercompany pivot = globalChargeCode.PivotWithOverrideLocalClientCollection.AddNew();
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			pivot.YP_AC = chargeCode.PK;
			pivot.YP_OH_LocalClientOverride = org1.PK;
			AssertHasWarning(pivot.YP_OH_LocalClientOverrideInfo, "In most cases, the Job Local Client should be flagged as a Receivables organization.");
			pivot.YP_OH_LocalClientOverride = org2.PK;
			AssertNoWarning(pivot.YP_OH_LocalClientOverrideInfo, "In most cases, the Job Local Client should be flagged as a Receivables organization.");
		}

		public void TestChargeCodeAndTypeAlreadyMappedErrorMessage()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode anotherChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			GlobalChargeCodeMapIntercompany globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>();
			globalChargeCode.YG_Code = "TEST";
			GlobalChargeCodeMapPivotIntercompany pivot = globalChargeCode.PivotWithOverrideLocalClientCollection.AddNew();
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot.YP_AC = chargeCode.PK;
			pivot.YP_OH_LocalClientOverride = org1.PK;
			Factory.Save();
			pivot = globalChargeCode.PivotWithOverrideLocalClientCollection.AddNew();
			pivot.YP_OH_LocalClientOverride = org1.PK;
			pivot.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot.YP_AC = chargeCode.PK;
			AssertHasError(pivot.YP_ACInfo, "This Charge Code and Type is already mapped to this Global Code for the selected override local client");
			pivot.YP_OH_LocalClientOverride = org2.PK;
			pivot.Validation.ValidateAll();
			AssertNoErrors("expect no errors found", pivot.YP_ACInfo);
		}
	}
}