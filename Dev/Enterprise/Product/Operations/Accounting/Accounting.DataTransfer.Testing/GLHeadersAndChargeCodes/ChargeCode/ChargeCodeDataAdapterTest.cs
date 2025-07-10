using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing.GLHeadersAndChargeCodes.ChargeCode
{
	[TestedType(typeof(ChargeCodeDataAdapter))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:EAdaptorNamingRule", Justification = "Property is Charge Code Adapter")]
	sealed class ChargeCodeDataAdapterTest : BaseAccountingDataAdapterTest<AccChargeCode, Xsd.ChargeCodesChargeCode>
	{
		public void TestImportChargeCodeFromValueObject()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);

			AssertEquals("TESTCODE", NewChargeCode.AC_Code);
			AssertEquals("Test Description!", NewChargeCode.AC_Desc);
			AssertEquals("FEA, FIA, FES, FIS, CEA, CIA, CES", NewChargeCode.AC_DepartmentFilterList);
			AssertEquals(Core.Constants.ChargeType.Margin, NewChargeCode.AC_ChargeType.ToString());
			AssertEquals(50m, NewChargeCode.AC_MarginPercentage);
			AssertEquals(TaxRate.PK, NewChargeCode.AC_AT_GSTRate);
			AssertEquals(WHTRate.PK, NewChargeCode.AC_AW_WithholdingTaxRate);
			AssertEquals(SalesAccGroup.PK, NewChargeCode.AC_AR_SalesGroup);
			AssertEquals(ExpenseGroup.PK, NewChargeCode.AC_AR_ExpenseGroup);
			AssertEquals(RevenueAccount.PK, NewChargeCode.AC_AG_RevenueAccount);
			AssertEquals(WIPAccount.PK, NewChargeCode.AC_AG_WIPAccount);
			AssertEquals(CostAccount.PK, NewChargeCode.AC_AG_CostAccount);
			AssertEquals(AccrualAccount.PK, NewChargeCode.AC_AG_AccrualAccount);
			AssertEquals("CSH", NewChargeCode.AC_ChargeGroup);
			AssertEquals(true, NewChargeCode.AC_IsGroupageCharge);
			AssertEquals("STG", NewChargeCode.AC_ChargeSubGroup);
			AssertEquals("AGY", NewChargeCode.AC_RateCalculator);
			AssertEquals(true, NewChargeCode.AC_ShowOnQuotation);
			AssertEquals(false, NewChargeCode.AC_SuppressOnQuoteIfZero);
			AssertEquals("AT", NewChargeCode.AC_IATA_ChargeCodeMap);
			AssertEquals(false, NewChargeCode.IsGlobal);
			AssertEquals("S201.256.32", NewChargeCode.AC_GovtChargeCode);
			AssertEquals(0, NewChargeCode.Notifications.GetErrors().Count());
		}

		public void TestIncorrectChargeType()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_ChargeTypeInfo.HasErrors());

			Value.ChargeType = "ZZZ";
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_ChargeTypeInfo.HasErrors());
		}

		public void TestLinkingToGlobalChargeCodeOnlyShowsWarning()
		{
			Env.Security.ChargeCodesLTGNew.IsAllowed = true;

			var companies = AccountingUtils.GetAllActiveCompanies(Factory);
			foreach (var company in companies)
			{
				company.GC_IsGSTRegistered = false; //This is to make the creation of global charge code easier
			}
			Factory.Save();

			var globalChargeCode = CreateGlobalChargeCodeFromXsdValues(Value);

			var newFactory = new BusinessObjectFactory();
			var chargeCode = "ZZ" + Value.Code;
			var query = new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			var localChargeCode = newFactory.LoadTop1<AccChargeCode>(query);

			AssertNotNull(localChargeCode);

			localChargeCode.Delete();

			newFactory.Save();

			var anotherFactory = new BusinessObjectFactory();
			localChargeCode = anotherFactory.LoadTop1<AccChargeCode>(query);
			AssertNull(localChargeCode);

			Value.Code = chargeCode;
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);

			Assert("Should not have any error.", !Buffer.HasErrors);

			Assert("Should have warning", Buffer.HasWarnings);
			AssertContains("Warning: Charge Code ZZTESTCODE - Warning - AC_Code: This change will link this code to the Global Charge Code 'ZZTESTCODE'", Buffer.AsString);
		}

		AccChargeCode CreateGlobalChargeCodeFromXsdValues(Xsd.ChargeCodesChargeCode xsdChargeCode)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var code = "ZZ" + Value.Code;
			var chargeCode = new BusinessObjectFactory().New<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = Value.Description;
			chargeCode.AC_DepartmentFilterList = Value.DepartmentFilterList;
			chargeCode.AC_ChargeType = Value.ChargeType;
			chargeCode.AC_MarginPercentage = Convert.ToDecimal(Value.MarginPercentage);

			Value.GSTRate = "";
			chargeCode.AC_AT_GSTRate = ZGuid.Empty;

			Value.WithholdingTaxRate = "";
			chargeCode.AC_AW_WithholdingTaxRate = ZGuid.Empty;

			Value.SalesGroup = "";
			chargeCode.AC_AR_SalesGroup = ZGuid.Empty;

			Value.ExpenseGroup = "";
			chargeCode.AC_AR_ExpenseGroup = ZGuid.Empty;

			Value.RevenueAccount = testObjectCreator.GLHeader1.AccountNum;
			chargeCode.AC_AG_RevenueAccount = testObjectCreator.GLHeader1.PK;

			Value.WIPAccount = testObjectCreator.GLHeader1.AccountNum;
			chargeCode.AC_AG_WIPAccount = testObjectCreator.GLHeader1.PK;

			Value.CostAccount = testObjectCreator.GLHeader1.AccountNum;
			chargeCode.AC_AG_CostAccount = testObjectCreator.GLHeader1.PK;

			Value.AccrualAccount = testObjectCreator.GLHeader1.AccountNum;
			chargeCode.AC_AG_AccrualAccount = testObjectCreator.GLHeader1.PK;

			chargeCode.AC_ChargeGroup = Value.ChargeGroup;
			chargeCode.AC_IsGroupageCharge = true;
			chargeCode.AC_ChargeSubGroup = Value.SubGroup;
			chargeCode.AC_RateCalculator = Value.RateCalculator;
			chargeCode.AC_ShowOnQuotation = true;
			chargeCode.AC_SuppressOnQuoteIfZero = false;
			Value.IATA_ChargeCodeMap = "";
			chargeCode.AC_IATA_ChargeCodeMap = "";
			chargeCode.AC_GC = ZGuid.Empty;
			chargeCode.AC_GovtChargeCode = Value.GovtChargeCode;

			chargeCode.Factory.Save();

			return Factory.Load<AccChargeCode>(chargeCode.PK);
		}

		public void TestIncorrectCode()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_CodeInfo.HasErrors());

			for (int i = 0; i < AccChargeCodeSchema.AC_Code.MaxLength; i++)
			{
				Value.Code += "a";
			}
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(Buffer.HasWarnings);
		}

		public void TestIncorrectDescription()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_DescInfo.HasErrors());

			for (int i = 0; i < AccChargeCodeSchema.AC_Desc.MaxLength; i++)
			{
				Value.Description += "a";
			}
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(Buffer.HasWarnings);
		}

		public void TestIncorrectDepartmentFilterList()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_DepartmentFilterListInfo.HasErrors());

			Value.DepartmentFilterList = "!12345%.ABC";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_DepartmentFilterListInfo.HasErrors());
		}

		public void TestIncorrectMarginPercentage()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_MarginPercentageInfo.HasErrors());

			Value.MarginPercentage = "-100";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_MarginPercentageInfo.HasErrors());
		}

		public void TestIncorrectGSTRate()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_AT_GSTRateInfo.HasErrors());

			Value.GSTRate = "-100TEST";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_AT_GSTRateInfo.HasErrors());
		}

		public void TestIncorrectWithholdingTaxRate()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_AW_WithholdingTaxRateInfo.HasErrors());

			Value.WithholdingTaxRate = "-!TEST?";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(ZGuid.Empty, NewChargeCode.AC_AW_WithholdingTaxRate);
		}

		public void TestIncorrectSalesGroup()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_AR_SalesGroupInfo.HasErrors());

			Value.SalesGroup = "XXXX.XX.XX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(ZGuid.Empty, NewChargeCode.AC_AR_SalesGroup);
		}

		public void TestIncorrectExpenseGroup()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_AR_ExpenseGroupInfo.HasErrors());

			Value.ExpenseGroup = "XXXX.XX.XX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(ZGuid.Empty, NewChargeCode.AC_AR_ExpenseGroup);
		}

		public void TestIncorrectRevenueAccount()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_AG_RevenueAccountInfo.HasErrors());

			Value.RevenueAccount = "XXXX.XX.XX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_AG_RevenueAccountInfo.HasErrors());
		}

		public void TestIncorrectWIPAccount()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_AG_WIPAccountInfo.HasErrors());

			Value.WIPAccount = "XXXX.XX.XX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_AG_WIPAccountInfo.HasErrors());
		}

		public void TestIncorrectCostAccount()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_AG_CostAccountInfo.HasErrors());

			Value.CostAccount = "XXXX.XX.XX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_AG_CostAccountInfo.HasErrors());
		}

		public void TestIncorrectAccrualAccount()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_AG_AccrualAccountInfo.HasErrors());

			Value.AccrualAccount = "XXXX.XX.XX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_AG_AccrualAccountInfo.HasErrors());
		}

		public void TestIncorrectChargeGroup()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_ChargeGroupInfo.HasErrors());

			Value.ChargeGroup = "XXX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_ChargeGroupInfo.HasErrors());
		}

		public void TestIncorrectIsGroupageCharge()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_IsGroupageChargeInfo.HasErrors());

			Value.IsGroupageCharge = "5";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, NewChargeCode.AC_IsGroupageCharge);
		}

		public void TestIncorrectSubGroup()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_ChargeSubGroupInfo.HasErrors());

			Value.SubGroup = "XXX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_ChargeSubGroupInfo.HasErrors());
		}

		public void TestIncorrectRateCalculator()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_RateCalculatorInfo.HasErrors());

			Value.RateCalculator = "XXX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_RateCalculatorInfo.HasErrors());
		}

		public void TestIncorrectShowOnQuotation()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_ShowOnQuotationInfo.HasErrors());

			Value.ShowOnQuotation = "XXX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, NewChargeCode.AC_ShowOnQuotation);
		}

		public void TestIncorrectSuppressOnQuoteIfZero()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_SuppressOnQuoteIfZeroInfo.HasErrors());

			Value.SuppressOnQuoteIfZero = "XXX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, NewChargeCode.AC_SuppressOnQuoteIfZero);
		}

		public void TestIncorrectIATA_ChargeCodeMap()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_IATA_ChargeCodeMapInfo.HasErrors());

			Value.IATA_ChargeCodeMap = "XX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_IATA_ChargeCodeMapInfo.HasErrors());
		}

		public void TestIncorrectGovtChargeCode()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_GovtChargeCodeInfo.HasErrors());

			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				Value.GovtChargeCode = "Test";
				ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
				AssertNoError(NewChargeCode.AC_GovtChargeCodeInfo, "Please enter a value.");

				Value.GovtChargeCode = "";
				ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
				AssertHasError(NewChargeCode.AC_GovtChargeCodeInfo, "Please enter a value.");

				Value.IsGlobal = Core.Constants.BooleanTrueString;
				Value.GovtChargeCode = "";
				ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
				AssertNoError(NewChargeCode.AC_GovtChargeCodeInfo, "Please enter a value.");
			}
		}

		public void TestAddErrorsToNotifications()
		{
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(!NewChargeCode.AC_IATA_ChargeCodeMapInfo.HasErrors());

			Value.IATA_ChargeCodeMap = "XX";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			Assert(NewChargeCode.AC_IATA_ChargeCodeMapInfo.HasErrors());

			Assert(Buffer.AsString.Contains(NewChargeCode.AC_IATA_ChargeCodeMapInfo.GetErrors().GetFirstMessage()));
		}

		public void TestSetMarginPercentage()
		{
			Value.ChargeType = Core.Constants.ChargeType.Margin;
			Value.MarginPercentage = "";
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetMarginPercentageErrorMessage));

			TestObjectCreator creator = new TestObjectCreator(Factory);
			AccChargeCode mjaChargeCode = creator.ManualJobAccrualChargeCode;

			Value = new Xsd.ChargeCodesChargeCode();
			Buffer = new NotificationBuffer();
			Context = new ValueObjectImportContext(Factory, Buffer);

			Value.ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			Value.MarginPercentage = "";
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(mjaChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetMarginPercentageErrorMessage));
		}

		public void TestSetRevenueAccount()
		{
			Value.ChargeType = Core.Constants.ChargeType.Margin;

			AccGLHeader revenueAccount = GetSavedAGHeader("9999.99.99");
			Value.RevenueAccount = "9999.99.99";

			AccGLHeader wIPAccount = GetSavedAGHeader("1111.11.11");
			Value.WIPAccount = "1111.11.11";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetRevenueAccountErrorMessage));

			Value.RevenueAccount = "";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetRevenueAccountErrorMessage));

			Value.RevenueAccount = "1234.12.12";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetRevenueAccountErrorMessage));

			AccGLHeader altAccount = GetSavedAGHeader("7777.66.55", Core.Constants.AccountType.Alternate);
			Value.RevenueAccount = "7777.66.55";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetGLAccountTypeErrorMsg(Value.RevenueAccount)));

			Value.ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			revenueAccount = GetSavedAGHeader("4444.44.44");
			Value.RevenueAccount = "4444.44.44";

			Value.RevenueAccount = "4444.44.44";
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetRevenueAccountErrorMessage));

			Value.RevenueAccount = "";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetRevenueAccountErrorMessage));

			Value.RevenueAccount = "1234.12.12";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetRevenueAccountErrorMessage));
		}

		public void TestSetWIPAccount()
		{
			Value.ChargeType = Core.Constants.ChargeType.Margin;

			AccGLHeader wIPAccount = GetSavedAGHeader("1111.11.11");
			Value.WIPAccount = "1111.11.11";

			AccGLHeader revenueAccount = GetSavedAGHeader("9999.99.99");
			Value.RevenueAccount = "9999.99.99";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetWIPAccountErrorMessage));

			Value.WIPAccount = "";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetWIPAccountErrorMessage));

			Value.WIPAccount = "1234.12.12";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetWIPAccountErrorMessage));

			Value.ChargeType = Core.Constants.ChargeType.NonAccrual;

			Value.WIPAccount = "";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetWIPAccountErrorMessage));

			Value.WIPAccount = "1234.12.12";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetWIPAccountErrorMessage));

			AccGLHeader altAccount = GetSavedAGHeader("7777.66.55", Core.Constants.AccountType.Alternate);
			Value.WIPAccount = "7777.66.55";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetGLAccountTypeErrorMsg(Value.WIPAccount)));

			Value.ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			wIPAccount = GetSavedAGHeader("4444.44.44");
			Value.WIPAccount = "4444.44.44";

			Value.WIPAccount = "4444.44.44";
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetWIPAccountErrorMessage));

			Value.WIPAccount = "";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetWIPAccountErrorMessage));

			Value.WIPAccount = "1234.12.12";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetWIPAccountErrorMessage));
		}

		public void TestSetCostAccount()
		{
			Value.ChargeType = Core.Constants.ChargeType.Margin;

			AccGLHeader accrualAccount = GetSavedAGHeader("3333.33.33");
			Value.AccrualAccount = "3333.33.33";

			AccGLHeader costAccount = GetSavedAGHeader("2222.22.22");
			Value.CostAccount = "2222.22.22";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetCostAccountErrorMessage));

			Value.CostAccount = "";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetCostAccountErrorMessage));

			Value.CostAccount = "1234.12.12";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetCostAccountErrorMessage));

			AccGLHeader altAccount = GetSavedAGHeader("7777.66.55", Core.Constants.AccountType.Alternate);
			Value.CostAccount = "7777.66.55";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetGLAccountTypeErrorMsg(Value.CostAccount)));

			Value.ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			costAccount = GetSavedAGHeader("4444.44.44");
			Value.CostAccount = "4444.44.44";

			Value.CostAccount = "4444.44.44";
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetCostAccountErrorMessage));

			Value.CostAccount = "";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetCostAccountErrorMessage));

			Value.CostAccount = "1234.12.12";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetCostAccountErrorMessage));
		}

		public void TestSetAccrualAccount()
		{
			Value.ChargeType = Core.Constants.ChargeType.Margin;

			AccGLHeader accrualAccount = GetSavedAGHeader("3333.33.33");
			Value.AccrualAccount = "3333.33.33";

			AccGLHeader costAccount = GetSavedAGHeader("2222.22.22");
			Value.CostAccount = "2222.22.22";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetAccrualAccountErrorMessage));

			Value.AccrualAccount = "";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetAccrualAccountErrorMessage));

			Value.AccrualAccount = "1234.12.12";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetAccrualAccountErrorMessage));

			Value.ChargeType = Core.Constants.ChargeType.NonAccrual;

			Value.AccrualAccount = "";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetAccrualAccountErrorMessage));

			Value.AccrualAccount = "1234.12.12";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetAccrualAccountErrorMessage));

			Value.ChargeType = Core.Constants.ChargeType.Overhead;

			Value.AccrualAccount = "";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetAccrualAccountErrorMessage));

			Value.AccrualAccount = "1234.12.12";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetAccrualAccountErrorMessage));

			AccGLHeader altAccount = GetSavedAGHeader("7777.66.55", Core.Constants.AccountType.Alternate);
			Value.AccrualAccount = "7777.66.55";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetGLAccountTypeErrorMsg(Value.AccrualAccount)));

			Value.ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			accrualAccount = GetSavedAGHeader("4444.44.44");
			Value.AccrualAccount = "4444.44.44";

			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(false, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetAccrualAccountErrorMessage));

			Value.AccrualAccount = "";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetAccrualAccountErrorMessage));

			Value.AccrualAccount = "1234.12.12";
			Buffer.Clear();
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals(true, Buffer.AsString.Contains(ChargeCodeDataAdapter.SetAccrualAccountErrorMessage));
		}

		public void TestNotifyBizObjCreatedOrUpdated()
		{
			ChargeCodeAdapter.NotifyBizObjCreatedOrUpdated_ForTestOnly(Buffer, new BusinessObjectThatDoesntSave(Factory));
			AssertEquals("The adapter must not add message about creation BusinessObjectThatDoesntSave.", 0, Buffer.Events.Length);
		}

		public void TestGlobalChargeCodeImport()
		{
			Value.IsGlobal = "Y";
			Value.GSTRate = "";
			Value.WithholdingTaxRate = "";
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertEquals("Charge Is Global", true, NewChargeCode.IsGlobal);
		}

		public void TestGlobalChargeCodeImport_CantCreate()
		{
			var creator = new TestObjectCreator(Factory);
			var clashingCode = creator.CreateChargeCode("XYZ");
			clashingCode.AC_Code = "TESTCODE";
			Factory.Save();

			NewChargeCode = Factory.New<AccChargeCode>();
			Value.IsGlobal = "Y";
			ChargeCodeAdapter.ImportChargeCodeFromValueObject(NewChargeCode, Value, Context);
			AssertContains(string.Format(@"There is a charge code in company '{0}' with the Code 'TESTCODE'. Please choose another Code.", clashingCode.Company.GC_Name), Buffer.AsString);
		}

		#region Base Tests

		protected override bool IsExportToValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return true; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return ChargeCodeAdapter.RootCollectionElementName; }
		}

		protected override string ExpectedRootElementName
		{
			get { return ChargeCodeAdapter.RootElementName; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return null;
		}

		protected override ValueObjectDataAdapter<AccChargeCode, Xsd.ChargeCodesChargeCode> GetNewBizObjXmlDataAdapter()
		{
			return new ChargeCodeDataAdapter();
		}

		#endregion

		#region Implementation

		void SetupXmlWithCorrectData()
		{
			Value.Code = "TESTCODE";
			Value.Description = "Test Description!";
			Value.DepartmentFilterList = "FEA, FIA, FES, FIS, CEA, CIA, CES";
			Value.ChargeType = Core.Constants.ChargeType.Margin;
			Value.MarginPercentage = "50";

			TaxRate = GetSavedTaxRateForTest("TESTCODE");
			Value.GSTRate = "TESTCODE";

			WHTRate = GetSavedAccWitholdingForTest("TESTWHCODE");
			Value.WithholdingTaxRate = "TESTWHCODE";

			SalesAccGroup = GetSavedAccGroupsForTest("SALESTEST");
			Value.SalesGroup = "SALESTEST";

			ExpenseGroup = GetSavedAccGroupsForTest("EXPTEST");
			Value.ExpenseGroup = "EXPTEST";

			RevenueAccount = GetSavedAGHeader("9999.99.99");
			Value.RevenueAccount = "9999.99.99";

			WIPAccount = GetSavedAGHeader("1111.11.11");
			Value.WIPAccount = "1111.11.11";

			CostAccount = GetSavedAGHeader("2222.22.22");
			Value.CostAccount = "2222.22.22";

			AccrualAccount = GetSavedAGHeader("3333.33.33");
			Value.AccrualAccount = "3333.33.33";

			Value.ChargeGroup = "CSH";
			Value.IsGroupageCharge = Core.Constants.BooleanTrueString;
			Value.SubGroup = "STG";
			Value.RateCalculator = "AGY";
			Value.ShowOnQuotation = Core.Constants.BooleanTrueString;
			Value.SuppressOnQuoteIfZero = Core.Constants.BooleanFalseString;
			Value.IATA_ChargeCodeMap = "AT";
			Value.GovtChargeCode = "S201.256.32";
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.GLAccountFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "XXXX.XX.XX");
			NewChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Value = new Xsd.ChargeCodesChargeCode();
			SetupXmlWithCorrectData();
			Buffer = new NotificationBuffer();
			Context = new ValueObjectImportContext(Factory, Buffer);
		}

		AccChargeCode NewChargeCode;
		Xsd.ChargeCodesChargeCode Value;
		NotificationBuffer Buffer;
		ValueObjectImportContext Context;
		AccTaxRate TaxRate;
		AccWithholding WHTRate;
		AccGroups SalesAccGroup;
		AccGroups ExpenseGroup;
		AccGLHeader RevenueAccount;
		AccGLHeader WIPAccount;
		AccGLHeader CostAccount;
		AccGLHeader AccrualAccount;

		AccTaxRate GetSavedTaxRateForTest(ZString codeValue)
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_Code = codeValue;
			return taxRate;
		}

		AccWithholding GetSavedAccWitholdingForTest(ZString codeValue)
		{
			AccWithholding wHTRate = Factory.NewWithValidTestData<AccWithholding>();
			wHTRate.AW_GC = GlbCompany.CurrentCompany.PK;
			wHTRate.AW_Code = codeValue;
			return wHTRate;
		}

		AccGroups GetSavedAccGroupsForTest(ZString codeValue)
		{
			AccGroups accGroup = Factory.NewWithValidTestData<AccGroups>();
			accGroup.AR_Code = codeValue;
			return accGroup;
		}

		AccGLHeader GetSavedAGHeader(ZString accountNumber)
		{
			AccGLHeader account = Factory.NewWithValidTestData<AccGLHeader>();
			account.AG_AccountNum = accountNumber;
			return account;
		}

		AccGLHeader GetSavedAGHeader(ZString accountNumber, ZString accountType)
		{
			AccGLHeader account = GetSavedAGHeader(accountNumber);
			account.AG_AccountType = accountType;
			return account;
		}

		ChargeCodeDataAdapter fAdapter;

		ChargeCodeDataAdapter ChargeCodeAdapter
		{
			get
			{
				if (fAdapter == null)
				{
					fAdapter = new ChargeCodeDataAdapter();
				}

				return fAdapter;
			}
		}

		#endregion
	}
}
