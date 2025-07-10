using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using AccGenericCharge = Enterprise.Accounting.Business.GenericCharge.GenericCharge;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(IntercompanyCostsApportionmentInvoiceLine))]
	public class IntercompanyCostsApportionmentInvoiceLineTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_internal ?? (TestObjectCreator_internal = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_internal;

		protected override void SetUp()
		{
			base.SetUp();

			testInvoice = new IntercompanyCostsApportionmentInvoice(Factory);
			testInvoiceLine = new IntercompanyCostsApportionmentInvoiceLine(Factory, testInvoice);

			intercompanyBranch = TestObjectCreator.LoadIntercompanyBranch();
		}

		protected IntercompanyCostsApportionmentInvoice testInvoice;
		protected IntercompanyCostsApportionmentInvoiceLine testInvoiceLine;
		protected GlbBranch intercompanyBranch;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new IntercompanyCostsApportionmentInvoiceLine(Factory, testInvoice);
		}

		#endregion

		#region Tests

		public void TestShowGLAccountsForImportAction()
		{
			AssertNull(testInvoiceLine.ChargeList.ShowGLAccountsForImportAction);
			testInvoiceLine.ShowGLAccountsForImportAction = (glHeaderCollection, glHeaderList) => { glHeaderList.Add(Factory.New<AccGLHeader>()); };
			AssertEquals(testInvoiceLine.ShowGLAccountsForImportAction, testInvoiceLine.ChargeList.ShowGLAccountsForImportAction);
			AssertNotNull(testInvoiceLine.ChargeList.ShowGLAccountsForImportAction);
		}

		public void TestAL_TaxDateReadOnly()
		{
			testInvoiceLine.AL_AT = ZGuid.Empty;
			Assert(testInvoiceLine.AL_TaxDateInfo.ReadOnly);

			testInvoiceLine.AL_AT = AccTaxRate.CreateTaxRate_ForTestOnly(Factory).PK;
			Assert(!testInvoiceLine.AL_TaxDateInfo.ReadOnly);
		}

		public void TestAL_TaxDateValidation()
		{
			AssertEquals($"Precondition: {nameof(testInvoiceLine.AL_TaxDate)}", ZDate.Empty, testInvoiceLine.AL_TaxDate);
			AssertNoErrors(testInvoiceLine.AL_TaxDateInfo);

			using (testInvoiceLine.GetValidationSuspender())
			{
				testInvoiceLine.AL_TaxDate = ZDate.BrettsBirthday;
				AssertNoErrors(testInvoiceLine.AL_TaxDateInfo);
			}

			testInvoiceLine.RunPreSaveValidation();
			AssertHasError(testInvoiceLine.AL_TaxDateInfo, "The date '18-Sep-1971' is more than 10 years old and thus is not valid.");

			testInvoiceLine.AL_TaxDate = ZDate.Today;
			AssertNoErrors(testInvoiceLine.AL_TaxDateInfo);

			const string errorMessage = "No rate found for selected date.";

			testInvoiceLine.AL_AT = CreateTaxRate().PK;
			testInvoiceLine.AL_TaxDate = ZDate.Today;
			AssertNoError(testInvoiceLine.AL_TaxDateInfo, errorMessage);

			testInvoiceLine.AL_TaxDate = ZDate.Empty;
			AssertHasError(testInvoiceLine.AL_TaxDateInfo, "Please enter a Tax Date.");

			testInvoiceLine.AL_TaxDate = ZDate.Today.AddDays(5);
			AssertHasError(testInvoiceLine.AL_TaxDateInfo, errorMessage);

			testInvoiceLine.AL_AT = ZGuid.Empty;
			AssertNoError(testInvoiceLine.AL_TaxDateInfo, errorMessage);

			AccTaxRate CreateTaxRate()
			{
				var rate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
				rate.SetRate_ForTestOnly(10, 1, ZDate.Today.AddDays(-1), ZDate.Today);
				rate.SetRate_ForTestOnly(18, 6, ZDate.Today.AddDays(1), ZDate.Today.AddDays(2));
				return rate;
			}
		}

		public void TestAL_TaxDateAndAffectedProperties()
		{
			testInvoiceLine.Amount = 100;
			AssertEquals($"Precondition: {nameof(testInvoiceLine.AL_TaxDate)}", ZDate.Empty, testInvoiceLine.AL_TaxDate);
			AssertEquals($"Precondition: {nameof(testInvoiceLine.Amount)}", 100m, testInvoiceLine.Amount);
			AssertEquals($"Precondition: {nameof(testInvoiceLine.Tax)}", ZDecimal.Zero, testInvoiceLine.Tax);
			Assert($"Precondition: {nameof(testInvoiceLine.Tax)} {nameof(testInvoiceLine.TaxInfo.ReadOnly)}", testInvoiceLine.TaxInfo.ReadOnly);
			AssertEquals($"Precondition: LocalTax", ZDecimal.Zero, testInvoiceLine.LocalTax);

			var taxValueChangeCount = 0;
			testInvoiceLine.TaxInfo.ValueChanged += (sender, e) => taxValueChangeCount++;

			testInvoiceLine.AL_AT = TestObjectCreator.GST1WithDates.PK;
			AssertEquals($"Postcondition: {nameof(testInvoiceLine.AL_TaxDate)}", ZDate.Today, testInvoiceLine.AL_TaxDate);
			AssertEquals(nameof(testInvoiceLine.Amount), 100m, testInvoiceLine.Amount);
			AssertEquals(nameof(testInvoiceLine.Tax), 10m, testInvoiceLine.Tax);
			Assert($"{nameof(testInvoiceLine.Tax)} {nameof(testInvoiceLine.TaxInfo.ReadOnly)}", !testInvoiceLine.TaxInfo.ReadOnly);
			AssertEquals(nameof(testInvoiceLine.LocalTax), 10m, testInvoiceLine.LocalTax);
			AssertEquals(nameof(taxValueChangeCount), 1, taxValueChangeCount);

			testInvoiceLine.AL_TaxDate = TestObjectCreator.GST1WithDates_DateWithNoRate;
			AssertEquals(nameof(testInvoiceLine.Amount), 100m, testInvoiceLine.Amount);
			AssertEquals(nameof(testInvoiceLine.Tax), ZDecimal.Zero, testInvoiceLine.Tax);
			Assert($"{nameof(testInvoiceLine.Tax)} {nameof(testInvoiceLine.TaxInfo.ReadOnly)}", testInvoiceLine.TaxInfo.ReadOnly);
			AssertEquals(nameof(testInvoiceLine.LocalTax), ZDecimal.Zero, testInvoiceLine.LocalTax);
			AssertEquals(nameof(taxValueChangeCount), 2, taxValueChangeCount);

			testInvoiceLine.AL_TaxDate = ZDate.Today.AddMonths(-2);
			AssertEquals(nameof(testInvoiceLine.Amount), 100m, testInvoiceLine.Amount);
			AssertEquals(nameof(testInvoiceLine.Tax), 2m, testInvoiceLine.Tax);
			Assert($"{nameof(testInvoiceLine.Tax)} {nameof(testInvoiceLine.TaxInfo.ReadOnly)}", !testInvoiceLine.TaxInfo.ReadOnly);
			AssertEquals(nameof(testInvoiceLine.LocalTax), 2m, testInvoiceLine.LocalTax);
			AssertEquals(nameof(taxValueChangeCount), 3, taxValueChangeCount);
		}

		public void TestAL_ATSetsAL_TaxDate()
		{
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			var taxRate2 = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);

			var expectedDate = ZDate.BrettsBirthday;
			testInvoiceLine.AL_TaxDate = expectedDate;
			testInvoiceLine.AL_AT = taxRate.PK;
			AssertNotEquals("Precondition: expectedDate", ZDate.Today, expectedDate);
			AssertEquals(expectedDate, testInvoiceLine.AL_TaxDate);

			testInvoiceLine.AL_AT = taxRate2.PK;
			AssertEquals(expectedDate, testInvoiceLine.AL_TaxDate);

			testInvoiceLine.AL_AT = ZGuid.Empty;
			AssertEquals(ZDate.Empty, testInvoiceLine.AL_TaxDate);

			testInvoiceLine.AL_AT = taxRate.PK;
			AssertEquals(ZDate.Today, testInvoiceLine.AL_TaxDate);

			testInvoiceLine.AL_AT = taxRate2.PK;
			AssertEquals(ZDate.Today, testInvoiceLine.AL_TaxDate);

			testInvoiceLine.AL_AT = ZGuid.Empty;
			AssertEquals(ZDate.Empty, testInvoiceLine.AL_TaxDate);
		}

		public void TestAL_AG()
		{
			ZGuid guid = new ZGuid("EF9B26E3-2F5D-4AC7-BC7A-F1A4F2554A56");
			testInvoiceLine.AL_AG = guid;
			AssertEquals("AL_AG", guid, testInvoiceLine.AL_AG);
		}

		public void TestValidateAL_AG()
		{
			var template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_IsGlobal = false;
			foreach (AccApportionmentTemplateLines line in template.Lines)
			{
				var companyFilter = glHeader.CompanyFilters.AddNew();
				companyFilter.ACF_GC_Company = line.Company;
			}

			testInvoiceLine.AL_AG = glHeader.PK;
			testInvoiceLine.ApportionmentMethod = template.PK;
			AssertNoError(testInvoiceLine.AL_AGInfo, "This GL Account cannot be used for the companies set up in the apportionment template");

			foreach (AccGLHeaderCompanyFilter companyFilter in glHeader.CompanyFilters)
			{
				companyFilter.ACF_GC_Company = new ZGuid();
			}

			testInvoiceLine.ValidateAL_AG();
			AssertHasError(testInvoiceLine.AL_AGInfo, "This GL Account cannot be used for the companies set up in the apportionment template");
		}

		public void TestAL_AC()
		{
			ZGuid guid = new ZGuid("FF9B26E3-2F5D-4AC7-BC7A-F1A4F2554A56");
			testInvoiceLine.AL_AC = guid;
			AssertEquals("AL_AC", guid, testInvoiceLine.AL_AC);
		}

		public void TestDefaultGovtChargeCodeIsBeingSet()
		{
			foreach (var enableGovtChargeCode in new[] { false, true })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode))
				{
					var charge = Factory.NewWithValidTestData<AccChargeCode>();
					charge.AC_GovtChargeCode = "GVTCC1";

					AssertEquals("Before: AL_GovtChargeCode", string.Empty, testInvoiceLine.AL_GovtChargeCode);

					testInvoiceLine.AL_AC = charge.PK;
					AssertEquals("After: AL_GovtChargeCode", enableGovtChargeCode ? "GVTCC1" : string.Empty, testInvoiceLine.AL_GovtChargeCode);
				}
			}
		}

		public void TestAL_GovtChargeCode()
		{
			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			testInvoiceLine.AL_GovtChargeCode = ZString.Empty;
			AssertNoErrors(testInvoiceLine.AL_GovtChargeCodeInfo);

			AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			testInvoiceLine.AL_GovtChargeCode = ZString.Empty;
			AssertHasErrors(testInvoiceLine.AL_GovtChargeCodeInfo);

			testInvoiceLine.AL_GovtChargeCode = "Govt1.1";
			AssertNoErrors(testInvoiceLine.AL_GovtChargeCodeInfo);
		}

		public void TestAL_SupplyType()
		{
			var expectedLackOfCodeErrorMessage = "Please enter a value.";
			var expectedLackOfCodeWarningMessage = "The Supply Type is not specified. Please check if a supply type is needed before posting.";
			var expectedInvalidCodeErrorMessage = "Enter a valid selection.";
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				testInvoiceLine.AL_SupplyType = ZString.Empty;
				AssertHasWarning(testInvoiceLine.AL_SupplyTypeInfo, expectedLackOfCodeWarningMessage);
				testInvoiceLine.AL_SupplyType = "111";
				AssertHasError(testInvoiceLine.AL_SupplyTypeInfo, expectedInvalidCodeErrorMessage);
			}
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				testInvoiceLine.AL_SupplyType = ZString.Empty;
				AssertHasError(testInvoiceLine.AL_SupplyTypeInfo, expectedLackOfCodeErrorMessage);
				testInvoiceLine.AL_SupplyType = "111";
				AssertHasError(testInvoiceLine.AL_SupplyTypeInfo, expectedInvalidCodeErrorMessage);
			}
		}

		public void TestSupplyTypes()
		{
			var supplyTypes = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value;

			testInvoiceLine.Branch = GlbBranch.CurrentBranch.PK;
			AssertContainsExactElementsInAnyOrder(supplyTypes.GetActiveCodeDescriptionPairList(), testInvoiceLine.SupplyTypes);
		}

		public void TestChargeCode()
		{
			ZQuery query = new ZQuery(AccChargeCodeSchema.AC_IsActive, true);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_ChargeType, Core.Constants.ChargeType.Overhead);
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(query);
			testInvoiceLine.AL_AC = chargeCode.PK;
			AssertEquals("ChargeCode", chargeCode, testInvoiceLine.ChargeCode);
		}

		public void TestTaxMessagesIsUsingTheCorrectCountryCodeFilter()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("T01", "testTaxRate", 1);
			taxRate.AT_RN_NKCountry = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			taxRate.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg1.PK;
			TestObjectCreator.TaxMsg1.A9_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var testInvoiceLine1 = new IntercompanyCostsApportionmentInvoiceLine(Factory, testInvoice) { AL_AT = taxRate.PK };
			var testInvoiceLine2 = new IntercompanyCostsApportionmentInvoiceLine(Factory, testInvoice);
			AssertEquals(ZGuid.Empty, testInvoiceLine2.AL_AT);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				Assert("The TaxMessage should be found if the TaxRate country and message country are the same", testInvoiceLine1.TaxMessages.Contains(TestObjectCreator.TaxMsg1));
				Assert("The TaxMessage should not be found if the TaxRate is null and the Current Company is different from the message country", !testInvoiceLine2.TaxMessages.Contains(TestObjectCreator.TaxMsg1));
			}
			var testInvoiceLine3 = new IntercompanyCostsApportionmentInvoiceLine(Factory, testInvoice);
			AssertEquals(ZGuid.Empty, testInvoiceLine3.AL_AT);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Assert("The TaxMessage should be found if the TaxRate is null and the Current Company and message country are the same", testInvoiceLine3.TaxMessages.Contains(TestObjectCreator.TaxMsg1));
			}
		}

		public void TestSetAL_ATUpdatesAL_A9_VatClass()
		{
			var taxRate = TestObjectCreator.GST1;

			AccInvMsg msg1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxRate.AT_A9_DefaultVatClass = msg1.PK;
			Factory.Save();

			AssertEquals(ZGuid.Empty, testInvoiceLine.AL_AT);
			AssertEquals(ZGuid.Empty, testInvoiceLine.AL_A9_VATClass);
			testInvoiceLine.AL_AT = taxRate.PK;
			AssertEquals("line's tax message should be copied from tax rate", msg1.PK, testInvoiceLine.AL_A9_VATClass);

			testInvoiceLine.AL_AT = ZGuid.Empty;
			AssertEquals("line's tax message should be reset to empty if tax rate is missing", ZGuid.Empty, testInvoiceLine.AL_A9_VATClass);
		}

		public void TestAL_A9_VATClass_ReadOnly()
		{
			testInvoiceLine.AL_AT = ZGuid.Empty;
			Assert("tax message is readonly if tax rate is missing", testInvoiceLine.AL_A9_VATClass_ReadOnly);

			var taxRate = TestObjectCreator.GST1;

			testInvoiceLine.AL_AT = taxRate.PK;
			if (testInvoiceLine.AL_AT_ReadOnly)
			{
				Assert(testInvoiceLine.AL_A9_VATClass_ReadOnly);
			}
			else
			{
				Env.Security.NewPayablesOverrideTaxMessageAllows.IsAllowed = false;
				Assert(testInvoiceLine.AL_A9_VATClass_ReadOnly);
				Env.Security.NewPayablesOverrideTaxMessageAllows.IsAllowed = true;
				Assert(!testInvoiceLine.AL_A9_VATClass_ReadOnly);
			}
		}

		public void TestAL_AT()
		{
			testInvoice.ExchangeRate.Rate = 0.5;
			var taxRate = TestObjectCreator.GST1;
			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);
			testInvoiceLine.ApportionmentMethod = template.PK;
			ZDecimal amount = 1000.00m;
			testInvoiceLine.Amount = amount;
			AssertEquals("Tax", 0.00m, testInvoiceLine.Tax);
			testInvoiceLine.AL_AT = taxRate.PK;
			AssertEquals("AL_AT", taxRate.PK, testInvoiceLine.AL_AT);
			AssertEquals("Tax", amount * (taxRate.GetRate_ForTestOnly() / 100), testInvoiceLine.Tax);
			AssertApportionmentTax(template, 0, 33.34m, 66.68m);
			AssertApportionmentTax(template, 1, 33.33m, 66.66m);
			AssertApportionmentTax(template, 2, 23.33m, 46.66m);
			AssertApportionmentTax(template, 3, 10.00m, 20.00m);

			AssertEquals("TaxRate", taxRate, testInvoiceLine.TaxRate);
		}

		public void TestValidateAL_AT()
		{
			var taxRate = TestObjectCreator.GST1;
			testInvoiceLine.AL_AT = taxRate.PK;
			Assert("AL_ATInfo", !testInvoiceLine.AL_ATInfo.HasErrors());

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			ZDBOnlyQuery query1 = new ZDBOnlyQuery(typeof(OrgHeader));
			query1.AddToFilter(OrgHeaderSchema.OH_IsActive, ZBool.True);
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query1.AddSubQuery(subQuery, JoinCondition.And);
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(query1);
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			testInvoice.Creditor = orgHeader.PK;

			var query = new ZQuery(AccGLHeaderSchema.AG_IsActive, ZBool.True);
			query.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, ZBool.False);
			query.AddToFilter(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.BalanceSheetAccount);
			AccGLHeader header = Factory.LoadTop1<AccGLHeader>(query);
			ZQuery filter = new ZQuery(ViewGenericChargeSchema.PK, header.PK);
			AccGenericCharge genericTransactionCharge = Factory.LoadTop1<AccGenericCharge>(filter);
			testInvoiceLine.GenericTransactionCharge = genericTransactionCharge;

			testInvoiceLine.AL_AT = ZGuid.Empty;
			Assert("AL_ATInfo", testInvoiceLine.AL_ATInfo.HasError("Please enter a Tax ID."));
		}

		void AssertApportionmentTax(AccApportionmentTemplate template, int lineNumber, ZDecimal foreignGST, ZDecimal localGST)
		{
			AssertEquals(string.Format("ForeignGST Line {0}", lineNumber), foreignGST, testInvoiceLine.Apportionments[lineNumber].ForeignGST);
			AssertEquals(string.Format("LocalGST Line {0}", lineNumber), localGST, testInvoiceLine.Apportionments[lineNumber].LocalGST);
		}

		public void TestAL_AW()
		{
			ZGuid guid = new ZGuid("AA9B26E3-2F5D-4AC7-BC7A-F1A4F2554A56");
			testInvoiceLine.AL_AW = guid;
			AssertEquals("AL_AW", guid, testInvoiceLine.AL_AW);
		}

		public void TestGenericTransactionCharge()
		{
			ZQuery query = new ZQuery(AccChargeCodeSchema.AC_IsActive, true);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_ChargeType, Core.Constants.ChargeType.Overhead);
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(query);
			ZQuery filter = new ZQuery(ViewGenericChargeSchema.PK, chargeCode.PK);
			AccGenericCharge genericTransactionCharge = Factory.LoadTop1<AccGenericCharge>(filter);
			testInvoiceLine.GenericTransactionCharge = genericTransactionCharge;
			AssertEquals("GenericTransactionCharge", genericTransactionCharge, testInvoiceLine.GenericTransactionCharge);
		}

		public void TestGenericChargeBizO()
		{
			ZQuery query = new ZQuery(AccChargeCodeSchema.AC_IsActive, true);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_ChargeType, Core.Constants.ChargeType.Overhead);
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(query);
			testInvoiceLine.AL_AC = chargeCode.PK;
			ZQuery filter = new ZQuery(ViewGenericChargeSchema.PK, chargeCode.PK);
			AccGenericCharge genericCharge = Factory.LoadTop1<AccGenericCharge>(filter);
			AssertEquals("GenericChargeBizO", genericCharge, testInvoiceLine.GenericChargeBizO);
			query = new ZQuery(AccGLHeaderSchema.AG_IsActive, ZBool.True);
			query.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, ZBool.False);
			query.AddToFilter(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.BalanceSheetAccount);
			AccGLHeader header = Factory.LoadTop1<AccGLHeader>(query);
			testInvoiceLine.AL_AG = header.PK;
			filter = new ZQuery(ViewGenericChargeSchema.PK, header.PK);
			genericCharge = Factory.LoadTop1<AccGenericCharge>(filter);
			AssertEquals("GenericChargeBizO", genericCharge, testInvoiceLine.GenericChargeBizO);
		}

		public void TestGenericCharge()
		{
			ZQuery query = new ZQuery(AccChargeCodeSchema.AC_IsActive, true);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_ChargeType, Core.Constants.ChargeType.Overhead);
			AccChargeCode chargeCode = Factory.LoadTop1<AccChargeCode>(query);
			testInvoiceLine.GenericCharge = chargeCode.PK;
			AssertEquals("GenericCharge", chargeCode.PK, testInvoiceLine.GenericCharge);
			AssertEquals("AL_AC", chargeCode.PK, testInvoiceLine.AL_AC);
			AssertEquals("Description", chargeCode.AC_Desc, testInvoiceLine.Description);

			query = new ZQuery(AccGLHeaderSchema.AG_IsActive, ZBool.True);
			query.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, ZBool.False);
			query.AddToFilter(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.BalanceSheetAccount);
			AccGLHeader header = Factory.LoadTop1<AccGLHeader>(query);
			testInvoiceLine.GenericCharge = header.PK;
			AssertEquals("GenericCharge", header.PK, testInvoiceLine.GenericCharge);
			AssertEquals("AL_AG", header.PK, testInvoiceLine.AL_AG);
			AssertEquals("Description", header.AG_Description, testInvoiceLine.Description);
		}

		public void TestValidateGenericCharge()
		{
			testInvoiceLine.GenericCharge = Guid.NewGuid();
			Assert("GenericChargeInfo", testInvoiceLine.GenericChargeInfo.HasError("Please enter a valid Charge."));

			var query = new ZQuery(AccGLHeaderSchema.AG_IsActive, ZBool.True);
			query.AddToFilter(AccGLHeaderSchema.AG_ControlAccount, ZBool.False);
			query.AddToFilter(AccGLHeaderSchema.AG_DisallowDirectPosting, ZBool.False);
			query.AddToFilter(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.BalanceSheetAccount);
			var header = Factory.LoadTop1<AccGLHeader>(query);
			testInvoiceLine.GenericCharge = header.PK;
			Assert("GenericChargeInfo", !testInvoiceLine.GenericChargeInfo.HasErrors());

			testInvoiceLine.GenericCharge = ZGuid.Empty;
			Assert("GenericChargeInfo", testInvoiceLine.GenericChargeInfo.HasError("Please enter a valid Charge."));
		}

		public void TestSettingDepartmentFromChargeDepartmentFilterlist()
		{
			AssertEquals("Pre-condition: CurrentDepartment", GlbDepartment.CurrentDepartment.PK, testInvoiceLine.AL_GE);
			GlbDepartment otherDept = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.NotEqual, GlbDepartment.CurrentDepartment.GE_Code));
			AssertNotNull("Other Department", otherDept);

			ZQuery query = new ZQuery(ViewGenericChargeSchema.VC_IsActive, true);
			query.AddToFilter(ViewGenericChargeSchema.VC_GC, GlbCompany.CurrentCompany.PK);
			AccGenericCharge chargeCode = Factory.LoadTop1<AccGenericCharge>(query);
			AssertNotNull("ChargeCode", chargeCode);

			chargeCode.VC_DepartmentFilterList = otherDept.GE_Code;
			testInvoiceLine.GenericCharge = chargeCode.PK;
			AssertEquals("GenericCharge", chargeCode.PK, testInvoiceLine.GenericCharge);
			AssertEquals("AL_GE", otherDept.PK, testInvoiceLine.AL_GE);

			testInvoiceLine.GenericCharge = ZGuid.Empty;
			AssertNull("Pre-condition: Department BLA should not exist", Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "BLA"));
			chargeCode.VC_DepartmentFilterList = "BLA";
			testInvoiceLine.GenericCharge = chargeCode.PK;
			AssertEquals("GenericCharge", chargeCode.PK, testInvoiceLine.GenericCharge);
			AssertEquals("AL_GE was not changed", otherDept.PK, testInvoiceLine.AL_GE);

			testInvoiceLine.GenericCharge = ZGuid.Empty;
			chargeCode.VC_DepartmentFilterList = "";
			testInvoiceLine.GenericCharge = chargeCode.PK;
			AssertEquals("GenericCharge", chargeCode.PK, testInvoiceLine.GenericCharge);
			AssertEquals("AL_GE was not changed", otherDept.PK, testInvoiceLine.AL_GE);

			testInvoiceLine.GenericCharge = ZGuid.Empty;
			chargeCode.VC_DepartmentFilterList = "All";
			testInvoiceLine.GenericCharge = chargeCode.PK;
			AssertEquals("GenericCharge", chargeCode.PK, testInvoiceLine.GenericCharge);
			AssertEquals("AL_GE was not changed", otherDept.PK, testInvoiceLine.AL_GE);
		}

		public void TestDescription()
		{
			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);
			testInvoiceLine.ApportionmentMethod = template.PK;
			ZString description = "Some Text";
			testInvoiceLine.Description = description;
			AssertEquals("Description", description, testInvoiceLine.Description);
			AssertApportionmentDescription(template, 0, description);
			AssertApportionmentDescription(template, 1, description);
			AssertApportionmentDescription(template, 2, description);
			AssertApportionmentDescription(template, 3, description);
		}

		void AssertApportionmentDescription(AccApportionmentTemplate template, int lineNumber, ZString description)
		{
			AssertEquals(string.Format("Description Line {0}", lineNumber), description + " " + template.Lines[lineNumber].Y0_Description, testInvoiceLine.Apportionments[lineNumber].Description);
		}

		public void TestBranch()
		{
			ZGuid guid = new ZGuid("BB9B26E3-2F5D-4AC7-BC7A-F1A4F2554A56");
			testInvoiceLine.Branch = guid;
			AssertEquals("Branch", guid, testInvoiceLine.Branch);
		}

		public void TestValidateBranch()
		{
			testInvoiceLine.Branch = Guid.NewGuid();
			Assert("BranchInfo", testInvoiceLine.BranchInfo.HasError("Please enter a valid Branch."));

			testInvoiceLine.Branch = GlbBranch.CurrentBranch.PK;
			Assert("BranchInfo", !testInvoiceLine.BranchInfo.HasErrors());
		}

		public void TestBranchName()
		{
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, GlbBranch.CurrentBranch.PK));
			testInvoiceLine.Branch = branch.PK;
			AssertEquals("BranchName", branch.GB_BranchName, testInvoiceLine.BranchName);
		}

		public void TestTaxBranch()
		{
			var currentBranchPK = GlbBranch.CurrentBranch.PK;
			testInvoiceLine.TaxBranch = currentBranchPK;

			AssertEquals("TaxBranch", currentBranchPK, testInvoiceLine.TaxBranch);
			AssertNotNull(testInvoiceLine.TaxBranchInfo);
			AssertEquals(GlbCompany.CurrentCompany.Branches.Count(x => x.GB_IsActive = true), testInvoiceLine.Branches.Count);
			Assert(testInvoiceLine.TaxBranchInfo.ReadOnly);
		}

		public void TestApplyTaxBranchOfInvoiceLineToApportionment()
		{
			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);
			testInvoiceLine.ApportionmentMethod = template.PK;

			var apportionment = testInvoiceLine.Apportionments.AddNew();

			testInvoiceLine.TaxBranch = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbBranch.CurrentBranch.PK, apportionment.TaxBranch);

			testInvoiceLine.TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			AssertEquals(TestObjectCreator.NonCurrentBranch.PK, apportionment.TaxBranch);
		}

		public void TestAmount()
		{
			var taxRate = TestObjectCreator.GST1;
			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);
			testInvoiceLine.ApportionmentMethod = template.PK;
			AssertEquals("Tax", 0.00m, testInvoiceLine.Tax);
			testInvoiceLine.AL_AT = taxRate.PK;

			ZDecimal amount = 1000.00m;
			testInvoiceLine.Amount = amount;
			AssertEquals("Amount", amount, testInvoiceLine.Amount);
			AssertEquals("Tax", amount * (taxRate.GetRate_ForTestOnly() / 100), testInvoiceLine.Tax);
			AssertEquals("Total", amount + (amount * (taxRate.GetRate_ForTestOnly() / 100)), testInvoiceLine.Total);
			AssertApportionment(template, 0, 333.33m);
			AssertApportionment(template, 1, 333.33m);
			AssertApportionment(template, 2, 233.34m);
			AssertApportionment(template, 3, 100.00m);
		}

		public void TestValidateAmount()
		{
			IntercompanyCostsApportionment apportionment = testInvoiceLine.Apportionments.AddNew();
			testInvoiceLine.ApportionmentMethod = ZGuid.Empty;
			apportionment.ForeignApportionedAmount = 250.00m;
			testInvoiceLine.Amount = 500.00m;
			Assert("AmountInfo", testInvoiceLine.AmountInfo.HasError("Sum of Foreign Amounts on Apportionment Details must equal Charges Line Amount."));

			apportionment.ForeignApportionedAmount = 100.00m;
			testInvoiceLine.Amount = 100.00m;
			Assert("AmountInfo", !testInvoiceLine.AmountInfo.HasErrors());

			testInvoiceLine.Amount = 0.00m;
			Assert("AmountInfo", testInvoiceLine.AmountInfo.HasError("Please enter an Amount."));
		}

		void AssertApportionment(AccApportionmentTemplate template, int lineNumber, ZDecimal foreignApportionedAmount)
		{
			AssertEquals(string.Format("Company Line {0}", lineNumber), template.Lines[lineNumber].Company, testInvoiceLine.Apportionments[lineNumber].Company);
			AssertEquals(string.Format("Branch Line {0}", lineNumber), template.Lines[lineNumber].Y0_GB, testInvoiceLine.Apportionments[lineNumber].Branch);
			AssertEquals(string.Format("Department Line {0}", lineNumber), template.Lines[lineNumber].Y0_GE, testInvoiceLine.Apportionments[lineNumber].Department);
			AssertEquals(string.Format("ApportionmentFactor Line {0}", lineNumber), template.Lines[lineNumber].Y0_Percentage, testInvoiceLine.Apportionments[lineNumber].ApportionmentFactor);
			AssertEquals(string.Format("ForeignApportionedAmount Line {0}", lineNumber), foreignApportionedAmount, testInvoiceLine.Apportionments[lineNumber].ForeignApportionedAmount);
			AssertEquals(string.Format("ExchangeRate Line {0}", lineNumber), testInvoice.ExchangeRate.Rate, testInvoiceLine.Apportionments[lineNumber].ExchangeRate);
			AssertEquals(string.Format("TemplateLineDescription Line {0}", lineNumber), template.Lines[lineNumber].Y0_Description, testInvoiceLine.Apportionments[lineNumber].TemplateLineDescription);
			AssertEquals(string.Format("Description Line {0}", lineNumber), testInvoiceLine.Description + " " + template.Lines[lineNumber].Y0_Description, testInvoiceLine.Apportionments[lineNumber].Description);
		}

		public void TestTax()
		{
			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);
			ZDecimal exRate = 0.5m;
			testInvoice.ExchangeRate.Rate = exRate;
			testInvoiceLine.ApportionmentMethod = template.PK;
			var taxRate = TestObjectCreator.GST1;
			testInvoiceLine.AL_AT = taxRate.PK;
			ZDecimal tax = 100.00m;
			testInvoiceLine.Tax = tax;
			AssertEquals("Tax", tax, testInvoiceLine.Tax);
			AssertEquals("GSTAmount", tax, testInvoiceLine.GSTAmount);
			AssertApportionmentTax(template, 0, 33.34m, Env.CurrentCompany.ExchangeRate.ForeignToLocal(33.34m, exRate));
			AssertApportionmentTax(template, 1, 33.33m, Env.CurrentCompany.ExchangeRate.ForeignToLocal(33.33m, exRate));
			AssertApportionmentTax(template, 2, 23.33m, Env.CurrentCompany.ExchangeRate.ForeignToLocal(23.33m, exRate));
			AssertApportionmentTax(template, 3, 10.00m, Env.CurrentCompany.ExchangeRate.ForeignToLocal(10.00m, exRate));
		}

		public void TestGSTAmount()
		{
			ZDecimal gstAmount = 100.00m;
			ZDecimal amount = 500.00m;
			testInvoiceLine.Amount = amount;
			testInvoiceLine.GSTAmount = gstAmount;
			AssertEquals("Amount", amount, testInvoiceLine.Amount);
			AssertEquals("GSTAmount", gstAmount, testInvoiceLine.GSTAmount);
			AssertEquals("Total", amount + gstAmount, testInvoiceLine.Total);
			AssertEquals("GSTInclusiveAmount", amount + gstAmount, testInvoiceLine.GSTInclusiveAmount);
		}

		public void TestGSTInclusiveAmount()
		{
			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);
			testInvoiceLine.ApportionmentMethod = template.PK;
			var taxRate = TestObjectCreator.GST1;
			testInvoiceLine.AL_AT = taxRate.PK;
			ZDecimal gstInclusiveAmount = 1100.00m;
			testInvoiceLine.GSTInclusiveAmount = gstInclusiveAmount;
			AssertEquals("GSTInclusiveAmount", gstInclusiveAmount, testInvoiceLine.GSTInclusiveAmount);
			ZDecimal amount = gstInclusiveAmount / (1.0m + (taxRate.GetRate_ForTestOnly() / 100m));
			AssertEquals("Amount", amount, testInvoiceLine.Amount);
			ZDecimal tax = gstInclusiveAmount - amount;
			AssertEquals("Tax", tax, testInvoiceLine.Tax);
			AssertEquals("Total", amount + tax, testInvoiceLine.Total);
			AssertApportionment(template, 0, 333.33m);
			AssertApportionment(template, 1, 333.33m);
			AssertApportionment(template, 2, 233.34m);
			AssertApportionment(template, 3, 100.00m);
		}

		public void TestTotal()
		{
			ZDecimal total = 1100.00m;
			testInvoiceLine.Total = total;
			AssertEquals("Total", total, testInvoiceLine.Total);
		}

		public void TestApportionmentMethod()
		{
			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);
			testInvoiceLine.ApportionmentMethod = template.PK;
			AssertEquals("ApportionmentMethod", template.PK, testInvoiceLine.ApportionmentMethod);
			ZDecimal amount = 1000.00m;
			testInvoiceLine.Amount = amount;
			AssertEquals("Amount", amount, testInvoiceLine.Amount);
			AssertApportionment(template, 0, 333.33m);
			AssertApportionment(template, 1, 333.33m);
			AssertApportionment(template, 2, 233.34m);
			AssertApportionment(template, 3, 100.00m);
		}

		public void TestValidateApportionmentMethod()
		{
			testInvoiceLine.ApportionmentMethod = Guid.NewGuid();
			Assert("ApportionmentMethodInfo", testInvoiceLine.ApportionmentMethodInfo.HasError("Please enter a valid Apportionment Method."));

			testInvoiceLine.ApportionmentMethod = ZGuid.Empty;
			Assert("ApportionmentMethodInfo", testInvoiceLine.ApportionmentMethodInfo.HasError("Please enter a value."));

			AccApportionmentTemplate template = TestObjectCreator.CreateSameCompanyApportionmentTemplate();
			testInvoiceLine.ApportionmentMethod = template.PK;
			Assert("ApportionmentMethodInfo", !testInvoiceLine.ApportionmentMethodInfo.HasErrors());
		}

		public void TestIsGSTMandatory()
		{
			AssertEquals("IsGSTMandatory", false, testInvoiceLine.IsGSTMandatory);
			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.SetAPTaxApplicable(true);
			testInvoice.Creditor = header.PK;
			AssertEquals("IsGSTMandatory", GlbCompany.CurrentCompany.GC_IsGSTRegistered, testInvoiceLine.IsGSTMandatory);
		}

		public void TestDepartment()
		{
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_IsValid, true));
			testInvoiceLine.AL_GE = department.PK;
			AssertEquals("AL_GE", department.PK, testInvoiceLine.AL_GE);
			AssertEquals("Department", department, testInvoiceLine.Department);
		}

		public void TestValidateDepartment()
		{
			testInvoiceLine.AL_GE = Guid.NewGuid();
			Assert("AL_GEInfo", testInvoiceLine.AL_GEInfo.HasError("Please enter a valid Department."));

			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_IsValid, true));
			testInvoiceLine.AL_GE = department.PK;
			Assert("AL_GEInfo", !testInvoiceLine.AL_GEInfo.HasErrors());
		}

		public void TestDepartmentDescription()
		{
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_IsValid, true));
			testInvoiceLine.AL_GE = department.PK;
			AssertEquals("DepartmentDescription", department.GE_Desc, testInvoiceLine.DepartmentDescription);
		}

		public void TestLocalTax()
		{
			ZDecimal exRate = 0.7m;
			testInvoice.ExchangeRate.Rate = exRate;
			ZDecimal tax = 100.00m;
			testInvoiceLine.Tax = tax;
			AssertEquals("Tax", tax, testInvoiceLine.Tax);
			AssertEquals("LocalTax", Env.CurrentCompany.ExchangeRate.ForeignToLocal(tax, exRate), testInvoiceLine.LocalTax);
		}

		public void TestLocalAmount()
		{
			ZDecimal exRate = 0.7m;
			testInvoice.ExchangeRate.Rate = exRate;
			ZDecimal amount = 1000.00m;
			testInvoiceLine.Amount = amount;
			AssertEquals("Amount", amount, testInvoiceLine.Amount);
			AssertEquals("LocalAmount", Env.CurrentCompany.ExchangeRate.ForeignToLocal(amount, exRate), testInvoiceLine.LocalAmount);
		}

		public void TestLocalTotal()
		{
			ZDecimal exRate = 0.7m;
			testInvoice.ExchangeRate.Rate = exRate;
			ZDecimal amount = 1000.00m;
			testInvoiceLine.Amount = amount;
			AssertEquals("Amount", amount, testInvoiceLine.Amount);
			ZDecimal localAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(amount, exRate);
			AssertEquals("LocalAmount", localAmount, testInvoiceLine.LocalAmount);
			ZDecimal tax = 100.00m;
			testInvoiceLine.Tax = tax;
			AssertEquals("Tax", tax, testInvoiceLine.Tax);
			ZDecimal localTax = Env.CurrentCompany.ExchangeRate.ForeignToLocal(tax, exRate);
			AssertEquals("LocalTax", localTax, testInvoiceLine.LocalTax);
			AssertEquals("LocalTotal", localAmount + localTax, testInvoiceLine.LocalTotal);
		}

		public void TestIsApportionmentMethodEmpty()
		{
			AssertEquals("IsApportionmentMethodEmpty", true, testInvoiceLine.IsApportionmentMethodEmpty);
			AccApportionmentTemplate template = TestObjectCreator.CreateIntercompanyApportionmentTemplate(intercompanyBranch.PK);
			testInvoiceLine.ApportionmentMethod = template.PK;
			AssertEquals("IsApportionmentMethodEmpty", false, testInvoiceLine.IsApportionmentMethodEmpty);
		}

		public void TestBranchDepartmentCombinationValidation_IntercompanyCostsApportionmentInvoiceLine()
		{
			var bizObj = testInvoiceLine;

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch, department) => { bizObj.Branch = branch; bizObj.AL_GE = department; }, bizObj.AL_GEInfo);
		}

		public void TestTaxIdAndMessageDefaultedForNonJobRelatedChargeCode()
		{
			var chargeCode = TestObjectCreator.CreateChargeCode("NONJOB");
			var chargeCode2 = TestObjectCreator.CreateChargeCode("NONJOB2");
			var taxCode = TestObjectCreator.CreateTaxRate("EXEMPT", "Exempt", 0);
			var taxCode2 = TestObjectCreator.CreateTaxRate("EXEMPT2", "Exempt2", 0);
			var taxMsg = TestObjectCreator.TaxMsg1;
			var taxMsg2 = TestObjectCreator.TaxMsg2;
			TestObjectCreator.CreateTaxOverride(chargeCode2, taxCode2.PK, taxMsg2.PK);
			TestObjectCreator.CreateTaxOverride(chargeCode, taxCode.PK, taxMsg.PK, "OTH", "ALL", "ALL", "NJR");

			Factory.Save();

			OrgHeader header = Factory.NewWithValidTestData<OrgHeader>();
			header.CompanyData.SetAPTaxApplicable(true);
			testInvoice.Creditor = header.PK;
			testInvoiceLine.GenericCharge = chargeCode2.PK;

			AssertNotEquals("The Tax ID should not have been defaulted", taxCode2.PK, testInvoiceLine.AL_AT);
			AssertNotEquals("The Tax message should not have been defaulted", taxMsg2.PK, testInvoiceLine.AL_A9_VATClass);
			AssertNotEquals("The Tax ID should not have been defaulted", taxCode.PK, testInvoiceLine.AL_AT);
			AssertNotEquals("The Tax message should not have been defaulted", taxMsg.PK, testInvoiceLine.AL_A9_VATClass);

			testInvoiceLine.GenericCharge = chargeCode.PK;

			AssertEquals("The Tax ID should have been defaulted", taxCode.PK, testInvoiceLine.AL_AT);
			AssertEquals("The Tax message should have been defaulted", taxMsg.PK, testInvoiceLine.AL_A9_VATClass);
		}

		public void TestTaxRateCollection()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate.AT_Type = AccTaxRate.Types.NotReportable;
			rate.AT_TaxSystemCode = "Other";

			var rate2 = Factory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate2.AT_Code = "BBCXYZ";
			rate2.AT_Type = AccTaxRate.Types.NotReportable;
			rate2.AT_TaxSystemCode = "Other1";

			Factory.Save();

			testInvoiceLine.TaxRates.Load();
			Assert("Collection should have only VAT Tax IDs", testInvoiceLine.TaxRates.Cast<AccTaxRate>().All(item => item.AT_TaxSystemCode.IsEmpty));
		}

		[TestDate(2024, 9, 2)]
		public void TestNotifyTransactionLineTaxDateChanges()
		{
			var invoice = new IntercompanyCostsApportionmentInvoice(Factory);
			var taxDate = ZDate.Today;
			var invoiceDate = new ZDate(2024, 9, 1);
			invoice.AH_InvoiceDate = invoiceDate;
			var line = invoice.Lines.AddNew();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_TaxDate = taxDate;

			AssertEquals(taxDate, invoice.InvoiceTaxDate);
			line.AL_TaxDate = ZDate.Empty;
			AssertEquals("Using AH_InvoiceDate due to AL_TaxDate change to empty.", invoiceDate, invoice.InvoiceTaxDate);

			line.AL_AT = ZGuid.Empty;
			AssertEquals("Empty due to AL_AT change to empty.", ZDateTime.Empty, invoice.InvoiceTaxDate);
			line.AL_AT = TestObjectCreator.GST1.PK;
			AssertEquals(taxDate, invoice.InvoiceTaxDate);

			TestObjectCreator.CC2.AC_ChargeType = ChargeType.Comment;
			line.AL_AC = TestObjectCreator.CC2.PK;
			AssertEquals("Empty due to Charge Code change to CMT", ZDateTime.Empty, invoice.InvoiceTaxDate);
		}

		#endregion

	}
}
