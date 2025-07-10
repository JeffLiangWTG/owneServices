using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
	public abstract class TransactionLineTest : AccTransactionLinesTestCase
	{
		public virtual void TestOSTotalSplitParts_ZeroVAT()
		{
			AccTaxRate fgstFree = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST"));
			if (fgstFree == null)
			{
				fgstFree = Factory.New<AccTaxRate>();
				fgstFree.AT_Code = "FREEGST";
				fgstFree.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
			fgstFree.AT_Type = AccTaxRate.Types.Rated;
			fgstFree.SetRateNumerator_ForTestOnly(0);

			Line.AL_RX_NKTransactionCurrency = "UAH";
			Line.AL_ExchangeRate = 177.4937;
			Line.AL_AT = fgstFree.PK;
			Line.AL_OSExTaxAmount = 30380329.38;

			Factory.Save();

			var factoryForNoCache = new BusinessObjectFactory();
			AssertOSValues((TransactionLine)factoryForNoCache.Load(Line.GetType(), Line.PK), 30380329.3800, 0, 30380329.3800);
		}

		public void TestShowGLAccountsForImportAction()
		{
			AssertNull(Line.GLHeaderCollection.ShowGLAccountsForImportAction);
			Line.ShowGLAccountsForImportAction = (glHeaderCollection, glHeaderList) => { glHeaderList.Add(Factory.New<AccGLHeader>()); };
			AssertEquals(Line.ShowGLAccountsForImportAction, Line.GLHeaderCollection.ShowGLAccountsForImportAction);
			AssertNotNull(Line.GLHeaderCollection.ShowGLAccountsForImportAction);
		}

		public virtual void TestOSTotalSplitParts_NonZeroVAT()
		{
			AccTaxRate gST = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST"));
			if (gST == null)
			{
				gST = Factory.New<AccTaxRate>();
				gST.AT_Code = "GST";
				gST.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			}
			gST.AT_Type = AccTaxRate.Types.Rated;
			gST.SetRate_ForTestOnly(175, 10);

			Line.AL_RX_NKTransactionCurrency = "UAH";
			Line.AL_ExchangeRate = 177.4937;
			Line.AL_AT = gST.PK;
			Line.AL_OSExTaxAmount = 30380329.38;
			Factory.Save();

			var factoryForNoCache = new BusinessObjectFactory();
			AssertOSValues((TransactionLine)factoryForNoCache.Load(Line.GetType(), Line.PK), 30380329.38, 5316557.6400, 35696887.0200);
		}

		public void TestITransactionLineMembers()
		{
			Line.AL_LocalExTaxAmount = 50m;
			AssertEquals(50m, Line.AL_LocalTotalAmount);
			AssertEquals(50m, (Line as ITransactionLine).LocalTotalAmount);
			AssertEquals(0m, (Line as ITransactionLine).LocalTaxAmount);
			AssertEquals(50m, (Line as ITransactionLine).LocalExTaxAmount);
			AssertEquals(string.Empty, (Line as ITransactionLine).TaxRateCode);

			Line.AL_AT = TestObjectCreator.GST1.PK;
			Line.AL_OSExTaxAmount = 100m;

			AssertEquals("ZZGST1", (Line as ITransactionLine).TaxRateCode);
			AssertEquals(110m, (Line as ITransactionLine).LocalTotalAmount);
			AssertEquals(10m, (Line as ITransactionLine).LocalTaxAmount);
			AssertEquals(100m, (Line as ITransactionLine).LocalExTaxAmount);
		}

		public void TestExTaxAmountDBSignedPropertiesReturnSameSigns()
		{
			var line = (TransactionLine)GetNewBusinessObject();
			line.AL_OSExTaxAmount = 120M;

			Assert("OS and Local DB signed properties are set to non zero amounts", line.AL_OSExTaxAmount_DBSigned != 0 && line.AL_LineAmount != 0);
			AssertEquals("OS and Local DB signed properties have the same sign", Math.Sign(line.AL_OSExTaxAmount_DBSigned), Math.Sign(line.AL_LineAmount));
		}

		public virtual void TestAL_OSTaxAmountCalculatedUsingLineTaxRatesWhenOSExTaxAmountSet()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Line.AL_AT = Factory.New<AccTaxRate>().PK;
			Line.AL_RX_NKTransactionCurrency = "UAH";
			Line.AL_TaxRateNumerator = 10;
			Line.AL_TaxRateDenominator = 2;
			Line.AL_TaxExtraRateNumerator = 6;
			Line.AL_TaxExtraRateDenominator = 3;
			Line.AL_OSExTaxAmount = 100;
			AssertEquals(7.1m, Line.AL_OSTaxAmount);
		}

		public virtual void TestAL_LocalTaxAmountCalculatedUsingLineTaxRatesWhenLocalExTaxAmountSet()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Line.AL_AT = Factory.New<AccTaxRate>().PK;
			Line.AL_TaxRateNumerator = 10;
			Line.AL_TaxRateDenominator = 2;
			Line.AL_TaxExtraRateNumerator = 6;
			Line.AL_TaxExtraRateDenominator = 3;
			Line.AL_LocalExTaxAmount = 100;
			AssertEquals(7.1m, Line.AL_LocalTaxAmount);
		}

		public void TestAL_LocalTaxAmountCalculatedUsingLineTaxRatesWhenOSTaxAmountSet()
		{
			AccountingConfigurationRegistry.Instance.UseLocalExTaxAmountWhileCalculatingLocalTaxAmount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Line.AL_AT = Factory.New<AccTaxRate>().PK;
			Line.AL_RX_NKTransactionCurrency = "UAH";
			Line.AL_TaxRateNumerator = 10;
			Line.AL_TaxRateDenominator = 2;
			Line.AL_TaxExtraRateNumerator = 6;
			Line.AL_TaxExtraRateDenominator = 3;
			Line.AL_OSExTaxAmount = 300;
			using (Line.GetOSAmountCalculationSuspender())
			{
				Line.AL_LocalExTaxAmount = 200;
				Line.AL_LocalTaxAmount = 0;
			}
			AssertEquals("Precondition: AL_OSExTaxAmount", 300m, Line.AL_OSExTaxAmount);
			AssertEquals("Precondition: AL_LocalExTaxAmount", 200m, Line.AL_LocalExTaxAmount);
			AssertEquals("Precondition: AL_OSTaxAmount", 21.3m, Line.AL_OSTaxAmount);
			var expectedValue = 14.2m;
			AssertNotEquals("Precondition: AL_LocalTaxAmount", expectedValue, Line.AL_LocalTaxAmount);
			Line.AL_OSTaxAmount = Line.AL_OSTaxAmount;
			AssertEquals(expectedValue, Line.AL_LocalTaxAmount);
		}

		public void TestPlaceOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var line = CreateNewLine();

				var placeOfSupplyCode = PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany)[0].Code;
				var placeOfSupplyTypeCode = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(GlbCompany.CurrentCompany, placeOfSupplyCode);

				line.AL_PlaceOfSupply = placeOfSupplyCode;
				AssertEquals("AL_PlaceOfSupply", placeOfSupplyCode, line.AL_PlaceOfSupply);
				AssertEquals("AL_PlaceOfSupplyType", placeOfSupplyTypeCode, line.AL_PlaceOfSupplyType);

				line.AL_PlaceOfSupply = string.Empty;
				AssertEquals("AL_PlaceOfSupply", string.Empty, line.AL_PlaceOfSupply);
				AssertEquals("AL_PlaceOfSupplyType", string.Empty, line.AL_PlaceOfSupplyType);
			}
		}

		public void TestPlacesOfSupplyLocation()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				Line.AL_PlaceOfSupply = "DL";
				Line.AL_PlaceOfSupplyType = PlaceOfSupplyTypes.State.Code;
				var location = Line.PlaceOfSupplyLocation_ForTestOnly;
				AssertNotNull(location);
				AssertEquals("DL", location.Code);
				AssertEquals("DL", location.State.RW_Code);
				Assert(!location.IsLocationRule());

				Line.AL_PlaceOfSupply = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;
				Line.AL_PlaceOfSupplyType = PlaceOfSupplyTypes.PredefinedRule.Code;
				location = Line.PlaceOfSupplyLocation_ForTestOnly;
				AssertNotNull(location);
				AssertEquals("ALX", location.Code);
				AssertNull(location.State);
				Assert(location.IsLocationRule());

				Line.AL_PlaceOfSupply = "";
				location = Line.PlaceOfSupplyLocation_ForTestOnly;
				AssertNull(location);
			}
		}

		public void TestNew()
		{
			AssertNotNull("Should have created new Transaction Line", Line);
		}

		protected abstract bool LineCanHaveTaxComponent
		{
			get;
		}

		protected abstract bool LineCanHaveForeignCurrency
		{
			get;
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesTransactionLine()
		{
			var localList = new List<string>
				{
					nameof(Line.AL_LineAmount),
					nameof(Line.AL_LocalExTaxAmount),
					nameof(Line.AL_LocalTotalAmount),
					nameof(Line.AL_UnitPrice),
					nameof(Line.AL_LocalTaxAmount),
					nameof(Line.AL_LocalWHTAmount),
					nameof(Line.AL_LocalTaxAmount_Recoverable),
					nameof(Line.AL_LocalTaxAmount_NotRecoverable)
				};

			var osList = new List<string>
				{
					nameof(Line.AL_OSTaxAmount),
					nameof(Line.AL_DBAH_OSExTaxAmount),
					nameof(Line.AL_DBAH_OSTaxAmount),
					nameof(Line.AL_GSTVATExtra),
					nameof(Line.AL_OSUnitPrice),
					nameof(Line.AL_OSWHTAmount),
					nameof(Line.AL_OverseasTotal),
					nameof(Line.AL_OSExTaxAmount),
					nameof(Line.AL_InputGSTVATRecoverable),
					nameof(Line.AL_OSTaxAmount_Recoverable),
					nameof(Line.AL_OSTaxAmount_NotRecoverable),
				};

			var percentList = new List<string>
				{
					nameof(Line.AL_Calc_InputGSTVATRecoverablePercentage)
				};

			var tester = new DecimalPlacesAttributeTester(Line, Line.Company);
			tester.CheckLocalCurrency(localList, nameof(Line.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(Line.CurrencyDecimals), nameof(Line.AL_RX_NKTransactionCurrency), Line);
			tester.CheckConstant(percentList, nameof(Line.PercentageDecimals), Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages);
		}

		public void TestAL_LocalTaxCorrelatesToAL_OSTaxAmount()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.India))
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = true;

				Line.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
				Line.AL_AT = TestObjectCreator.STAGST.PK;
				Line.AL_OSExTaxAmount = 100.70M;

				AssertEquals("Total OS tax amount", 18.12M, Line.AL_OSTaxAmount);
				AssertEquals("Total Local tax amount should be equal to Total OS tax amount since its a local currency invoice", 18.12M, Line.AL_LocalTaxAmount);

				Line.AL_OSTaxAmount = 18.13M;
				AssertEquals("Total Local tax amount should be equal to Total OS tax amount since its a local currency invoice", 18.13M, Line.AL_LocalTaxAmount);

				Line.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				Line.AL_ExchangeRate = 63.0001M;
				Line.AL_OSExTaxAmount = 100.70M;

				AssertEquals("Total OS tax amount", 18.12M, Line.AL_OSTaxAmount);
				AssertEquals("Total Local tax amount", 1141.94M, Line.AL_LocalTaxAmount);

				var expectedLocalTaxAmount = Math.Round(Line.AL_OSTaxAmount * Line.AL_ExchangeRate, 2);
				AssertNotEquals("Local tax amount is not calculated after applying exchange rate to the OS tax amount" +
						 ", rather it was calculated by applying main tax rate (9%) and extra tax rate (9%) on local ex tax amount", expectedLocalTaxAmount, Line.AL_LocalTaxAmount);

				Line.AL_OSTaxAmount = 18.13M;
				expectedLocalTaxAmount = Math.Round(Line.AL_OSTaxAmount * Line.AL_ExchangeRate, 2);
				AssertEquals("Since OS tax amount is not the default amount, rather was changed; this time local tax amount will be calculated from OS tax amount", expectedLocalTaxAmount, Line.AL_LocalTaxAmount);
			}
		}

		public void TestVATRecoverablePercentageAndValidation()
		{
			var line = (TransactionLine)GetNewBusinessObject();

			if (line.SupportsInputTaxRecoverable)
			{
				line.AL_Calc_InputGSTVATRecoverablePercentage = 100000000.00m;
				AssertVATRecoverableFields(line, "101.00%", 100000000.00m, 1000000.00m, "GST Recoverable % must be between 0 and 100.");

				line.AL_Calc_InputGSTVATRecoverablePercentage = 101.00m;
				AssertVATRecoverableFields(line, "101.00%", 101.00m, 1.0100m, "GST Recoverable % must be between 0 and 100.");

				line.AL_Calc_InputGSTVATRecoverablePercentage = 100.00m;
				AssertVATRecoverableFields(line, "100.00%", 100.00m, 1.0000m, null);

				TestObjectCreator.CC1.AC_ChargeType = Constants.ChargeType.Overhead;
				line.AL_AC = TestObjectCreator.CC1.PK;
				line.AL_Calc_InputGSTVATRecoverablePercentage = 99.99m;
				AssertVATRecoverableFields(line, "99.99%", 99.99m, 0.9999m, null);

				line.AL_Calc_InputGSTVATRecoverablePercentage = 0.01m;
				AssertVATRecoverableFields(line, "0.01%", 0.01m, 0.0001m, null);

				line.AL_Calc_InputGSTVATRecoverablePercentage = 0.00m;
				AssertVATRecoverableFields(line, "0.00%", 0.00m, 0.0000m, null);

				line.AL_Calc_InputGSTVATRecoverablePercentage = -0.01m;
				AssertVATRecoverableFields(line, "-0.01%", -0.01m, -0.0001m, "GST Recoverable % must be between 0 and 100.");

				line.AL_Calc_InputGSTVATRecoverablePercentage = -100000000.00m;
				AssertVATRecoverableFields(line, "-100000000.00%", -100000000.00m, -1000000.00m, "GST Recoverable % must be between 0 and 100.");

				var decimalPlacesAttributeForUserEditableField = line.GetType().GetProperty(TransactionLine.Schema.AL_Calc_InputGSTVATRecoverablePercentage)
					.GetCustomAttributes(typeof(DecimalPlacesAttribute), true)[0] as DecimalPlacesAttribute;
				var decimalPlacesForAttribute = line.GetType().GetProperty(decimalPlacesAttributeForUserEditableField.DecimalPlacesMember).GetValue(line);
				AssertEquals("Decimal Places for AL_Calc_InputGSTVATRecoverablePercentage", 2, decimalPlacesForAttribute);
			}
			else
			{
				Assert("Functionality is not supported", true);
			}
		}

		public void TestDefaultGovtChargeCodeIsBeingSet()
		{
			TestObjectCreator.CC1.AC_GovtChargeCode = "GVTCC1";
			TestObjectCreator.SetupOrCreateGovtChargeCodeOverride(TestObjectCreator.CC1, "COS", "ALL", "ALL", "ALL", "GVTCC2ForCost");
			TestObjectCreator.SetupOrCreateGovtChargeCodeOverride(TestObjectCreator.CC1, "REV", "ALL", "ALL", "ALL", "GVTCC2ForRevenue");
			Factory.Save();

			foreach (var enableGovtChargeCode in new[] { false, true })
			{
				AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode);

				var line = (TransactionLine)GetNewBusinessObject();
				line.AL_JH = ZGuid.Empty;
				line.AL_AC = ZGuid.Empty;
				AssertEquals("Before(Reset): AL_GovtChargeCode", string.Empty, line.AL_GovtChargeCode);

				line.AL_GovtChargeCode = enableGovtChargeCode ? "Override" : string.Empty;
				AssertEquals("Override: AL_GovtChargeCode", enableGovtChargeCode ? "Override" : string.Empty, line.AL_GovtChargeCode);

				line.AL_JH = ZGuid.Empty;
				line.AL_AC = TestObjectCreator.CC1.PK;
				AssertEquals("After(Reset): AL_GovtChargeCode", (enableGovtChargeCode && line.IsGovtChargeCodeApplicable) ? "GVTCC1" : string.Empty, line.AL_GovtChargeCode);

				line.AL_GovtChargeCode = enableGovtChargeCode ? "Override" : string.Empty;
				AssertEquals("Override: AL_GovtChargeCode", enableGovtChargeCode ? "Override" : string.Empty, line.AL_GovtChargeCode);

				line.AL_JH = new Job.Loader(QuotedBooking.New(QuoteBookingType.QuickBooking, Factory)).TryLoadOrCreateWithoutMutexForTestOnly().PK;
				line.AL_AC = ZGuid.Empty;
				AssertEquals("Before(Reset): AL_GovtChargeCode", string.Empty, line.AL_GovtChargeCode);

				line.AL_GovtChargeCode = enableGovtChargeCode ? "Override" : string.Empty;
				AssertEquals("Override: AL_GovtChargeCode", enableGovtChargeCode ? "Override" : string.Empty, line.AL_GovtChargeCode);

				line.AL_JH = line.AL_JH;
				line.AL_AC = TestObjectCreator.CC1.PK;
				var expectedCode = "GVTCC1";
				if (AccTransactionLines.IsCostLine(line))
				{
					expectedCode = "GVTCC2ForCost";
				}
				else if (AccTransactionLines.IsRevenueLine(line))
				{
					expectedCode = "GVTCC2ForRevenue";
				}
				AssertEquals("After(Reset): AL_GovtChargeCode", (enableGovtChargeCode && line.IsGovtChargeCodeApplicable) ? expectedCode : string.Empty, line.AL_GovtChargeCode);
			}
		}

		public void TestIsGovtChargeCodeApplicable()
		{
			var line = (TransactionLine)GetNewBusinessObject();

			if (line.TransactionHeader != null)
			{
				if (
					(line.TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable
						|| line.TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable
						|| line.TransactionHeader.AH_Ledger == LedgerTypes.IncompleteTransactions
						|| line.TransactionHeader.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions
					)
					|| (line.TransactionHeader.AH_Ledger == LedgerTypes.CashBook
						&& (line.TransactionHeader.AH_TransactionType == TransactionTypes.DirectPayment
								|| line.TransactionHeader.AH_TransactionType == TransactionTypes.DirectReceipt
							)
						)
					)
				{
					Assert(string.Format("Govt charge code is applicable for {0}-{1}", line.TransactionHeader.AH_Ledger, line.TransactionHeader.AH_TransactionType), line.IsGovtChargeCodeApplicable);
				}
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public void TestVATRecoverablePercentageOverrideValidation()
		{
			TestObjectCreator.CC1.AC_ChargeType = Constants.ChargeType.Overhead;
			var chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_Calc_InputGSTVATRecoverablePercentage = 50m;

			var glAccount = TestObjectCreator.GLHeader1;

			var line = CreateNewLine();

			var security = line.OverrideInputVatRecoverableSecurityCheckPoint_ForTestOnly;

			if (line.SupportsInputTaxRecoverable)
			{
				AssertNotEquals("Should not be Security.None", security.DisplayTextPathToSecurityRight, Env.Security.None.DisplayTextPathToSecurityRight);

				security.IsAllowed = false;
				AssertEquals("IsUserAllowedToOverrideVATRecoverablePercentage", false, line.IsUserAllowedToOverrideVATRecoverablePercentage);
				AssertEquals("AL_InputGSTVATRecoverableInfo", true, line.AL_InputGSTVATRecoverableInfo.ReadOnly);
				AssertEquals("AL_Calc_InputGSTVATRecoverablePercentage_ReadOnly", true, line.AL_Calc_InputGSTVATRecoverablePercentageInfo.ReadOnly);

				security.IsAllowed = true;
				AssertEquals("IsUserAllowedToOverrideVATRecoverablePercentage", true, line.IsUserAllowedToOverrideVATRecoverablePercentage);

				if (line.AL_LineType != TransactionLineTypes.Cost)
				{
					AssertEquals("AL_InputGSTVATRecoverableInfo", false, line.AL_InputGSTVATRecoverableInfo.ReadOnly);
					AssertEquals("AL_Calc_InputGSTVATRecoverablePercentage_ReadOnly", false, line.AL_Calc_InputGSTVATRecoverablePercentageInfo.ReadOnly);
				}
				else
				{
					AssertEquals("AL_InputGSTVATRecoverableInfo", true, line.AL_InputGSTVATRecoverableInfo.ReadOnly);
					AssertEquals("AL_Calc_InputGSTVATRecoverablePercentage_ReadOnly", true, line.AL_Calc_InputGSTVATRecoverablePercentageInfo.ReadOnly);

					line.AL_AG = TestObjectCreator.GLHeader1.PK;
					AssertEquals("AL_InputGSTVATRecoverableInfo", true, line.AL_InputGSTVATRecoverableInfo.ReadOnly);
					AssertEquals("AL_Calc_InputGSTVATRecoverablePercentage_ReadOnly", true, line.AL_Calc_InputGSTVATRecoverablePercentageInfo.ReadOnly);

					line.AL_AC = TestObjectCreator.CC2.PK;
					AssertEquals("AL_InputGSTVATRecoverableInfo", true, line.AL_InputGSTVATRecoverableInfo.ReadOnly);
					AssertEquals("AL_Calc_InputGSTVATRecoverablePercentage_ReadOnly", true, line.AL_Calc_InputGSTVATRecoverablePercentageInfo.ReadOnly);

					line.ChargeCode.AC_ChargeType = Constants.ChargeType.Overhead;
					AssertEquals("AL_InputGSTVATRecoverableInfo", false, line.AL_InputGSTVATRecoverableInfo.ReadOnly);
					AssertEquals("AL_Calc_InputGSTVATRecoverablePercentage_ReadOnly", false, line.AL_Calc_InputGSTVATRecoverablePercentageInfo.ReadOnly);
				}

				security.IsAllowed = false;
				line.TransactionHeader.AH_IsCancelled = true;
				line.AL_Calc_InputGSTVATRecoverablePercentage = 25m;
				AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);
				line.TransactionHeader.AH_IsCancelled = false;
				line.AL_Calc_InputGSTVATRecoverablePercentage = 25m;
				AssertHasError(line.AL_Calc_InputGSTVATRecoverablePercentageInfo, "You do not have sufficient security rights to override the Tax Recoverable Percentage, please set it back to 100.00%");

				security.IsAllowed = true;
				line.AL_Calc_InputGSTVATRecoverablePercentage = 25m;
				AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);
				security.IsAllowed = false;
				line.AL_Calc_InputGSTVATRecoverablePercentage = 100m;
				AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);

				if (line.AL_LineType == TransactionLineTypes.Cost)
				{
					security.IsAllowed = false;
					line.AL_AC = chargeCode.PK;
					line.AL_Calc_InputGSTVATRecoverablePercentage = 25m;
					AssertHasError(line.AL_Calc_InputGSTVATRecoverablePercentageInfo, "You do not have sufficient security rights to override the Tax Recoverable Percentage, please set it back to 50.00%");
					security.IsAllowed = true;
					line.AL_Calc_InputGSTVATRecoverablePercentage = 25m;
					AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);
					security.IsAllowed = false;
					line.AL_Calc_InputGSTVATRecoverablePercentage = 50m;
					AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);
				}
				else
				{
					line.AL_AC = ZGuid.Empty;
					line.AL_AG = glAccount.PK;
					line.AL_Calc_InputGSTVATRecoverablePercentage = 75m;
					AssertHasError(line.AL_Calc_InputGSTVATRecoverablePercentageInfo, "You do not have sufficient security rights to override the Tax Recoverable Percentage, please set it back to 100.00%");
					security.IsAllowed = true;
					line.AL_Calc_InputGSTVATRecoverablePercentage = 75m;
					AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);
					security.IsAllowed = false;
					line.AL_Calc_InputGSTVATRecoverablePercentage = 100m;
					AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);
				}

				security.IsAllowed = true;
				line.AL_Calc_InputGSTVATRecoverablePercentage = 30m;
				line.Factory.Save();
				security.IsAllowed = false;
				line.Validation.ValidateAll();
				AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);
			}
			else
			{
				AssertEquals("Should be Security.None", security.DisplayTextPathToSecurityRight, Env.Security.None.DisplayTextPathToSecurityRight);
				line.AL_Calc_InputGSTVATRecoverablePercentage = 25m;
				AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);
			}
		}

		public void TestVATRecoverablePercentageIsResetWhenChargeCodeOrGLAccountIsEntered()
		{
			var chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_Calc_InputGSTVATRecoverablePercentage = 50m;

			var glAccount = TestObjectCreator.GLHeader1;

			var line = (TransactionLine)GetNewBusinessObject();

			if (line.SupportsInputTaxRecoverable)
			{
				line.AL_Calc_InputGSTVATRecoverablePercentage = 25m;
				AssertEquals("VAT Recoverable % has been set manually", 25m, line.AL_Calc_InputGSTVATRecoverablePercentage);

				line.AL_AC = chargeCode.PK;
				AssertEquals("VAT Recoverable % should be defaulted from charge code", 50m, line.AL_Calc_InputGSTVATRecoverablePercentage);

				line.AL_AC = ZGuid.Empty;
				line.AL_AG = ZGuid.Empty;
				AssertEquals("VAT Recoverable % when no charge code and no gl account are set ", 100m, line.AL_Calc_InputGSTVATRecoverablePercentage);

				line.AL_Calc_InputGSTVATRecoverablePercentage = 75m;
				line.AL_AG = glAccount.PK;
				AssertEquals("VAT Recoverable % when gl account is set (and no charge code)", 100m, line.AL_Calc_InputGSTVATRecoverablePercentage);
			}
			else
			{
				line.AL_AC = chargeCode.PK;
				AssertEquals("VAT Recoverable %", 100m, line.AL_Calc_InputGSTVATRecoverablePercentage);

				line.AL_AC = glAccount.PK;
				AssertEquals("VAT Recoverable %", 100m, line.AL_Calc_InputGSTVATRecoverablePercentage);
			}
		}

		public void TestVATRecoverableAmounts()
		{
			var line = (TransactionLine)GetNewBusinessObject();

			if (line.SupportsInputTaxRecoverable)
			{
				line.AL_AC = TestObjectCreator.CC1.PK;
				line.AL_AT = TestObjectCreator.GST1.PK;
				line.AL_OSExTaxAmount = 100m;
				line.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				line.AL_ExchangeRate = 0.8m;

				AssertEquals("Precondition: AL_Calc_InputGSTVATRecoverablePercentage", 100m, line.AL_Calc_InputGSTVATRecoverablePercentage);
				AssertEquals("AL_OSTaxAmount with 100% tax recoverable", 10m, line.AL_OSTaxAmount);
				AssertEquals("AL_OSTaxAmount_Recoverable with 100% tax recoverable", 10m, line.AL_OSTaxAmount_Recoverable);
				AssertEquals("AL_OSTaxAmount_NotRecoverable with 100% tax recoverable", 0m, line.AL_OSTaxAmount_NotRecoverable);
				AssertEquals("AL_LocalTaxAmount with 100% tax recoverable", 12.5m, line.AL_LocalTaxAmount);
				AssertEquals("AL_LocalTaxAmount_Recoverable with 100% tax recoverable", 12.5m, line.AL_LocalTaxAmount_Recoverable);
				AssertEquals("AL_LocalTaxAmount_NotRecoverable with 100% tax recoverable", 0m, line.AL_LocalTaxAmount_NotRecoverable);

				line.AL_Calc_InputGSTVATRecoverablePercentage = 40m;
				AssertEquals("Precondition: AL_Calc_InputGSTVATRecoverablePercentage", 40m, line.AL_Calc_InputGSTVATRecoverablePercentage);
				AssertEquals("AL_OSTaxAmount with 40% tax recoverable", 10m, line.AL_OSTaxAmount);
				AssertEquals("AL_OSTaxAmount_Recoverable with 40% tax recoverable", 4m, line.AL_OSTaxAmount_Recoverable);
				AssertEquals("AL_OSTaxAmount_NotRecoverable with 40% tax recoverable", 6m, line.AL_OSTaxAmount_NotRecoverable);
				AssertEquals("AL_LocalTaxAmount with 40% tax recoverable", 12.5m, line.AL_LocalTaxAmount);
				AssertEquals("AL_LocalTaxAmount_Recoverable with 40% tax recoverable", 5m, line.AL_LocalTaxAmount_Recoverable);
				AssertEquals("AL_LocalTaxAmount_NotRecoverable with 40% tax recoverable", 7.5m, line.AL_LocalTaxAmount_NotRecoverable);
			}
			else
			{
				Assert(true);
			}
		}

		void AssertVATRecoverableFields(TransactionLine line, string description, decimal userEditableValue, decimal databaseValue, params string[] errorMessages)
		{
			CombineAssertions(description, delegate
			{
				AssertEquals("User Editable Field", userEditableValue, line.AL_Calc_InputGSTVATRecoverablePercentage);
				AssertEquals("Database Field", databaseValue, line.AL_InputGSTVATRecoverable);

				if (errorMessages == null)
				{
					AssertNoErrors(line.AL_Calc_InputGSTVATRecoverablePercentageInfo);
					AssertNoErrors(line.AL_InputGSTVATRecoverableInfo);
				}
				else
				{
					foreach (var message in errorMessages)
					{
						AssertHasError(line.AL_Calc_InputGSTVATRecoverablePercentageInfo, message);
						AssertHasError(line.AL_InputGSTVATRecoverableInfo, message);
					}
				}
			});
		}

		public virtual void TestAL_AGIsReadOnly()
		{
			Assert(Line.AL_AGInfo.ReadOnly);
		}

		public void TestAL_AGIsReadOnlyForPropertyDescriptor()
		{
			ZString errorMessage = @"You can't mix [ReadOnly(true)] attribute with AL_AG_ReadOnly property.
Now AL_AG_ReadOnly is used for AL_AG field and it doesn't have influence to Property Descriptor so IsReadonly for property descriptor should remain false.
If you want to use [ReadOnly(true)] attribute then you need to remove all AL_AG_ReadOnly properties and rewrite this test to check that ((PropertyDescriptor)Line.AL_AGInfo).IsReadOnly
is always equal to Line.AL_AGInfo.ReadOnly";
			Assert(errorMessage, !((PropertyDescriptor)Line.AL_AGInfo).IsReadOnly);
		}

		public virtual void TestTransactionLineFetchHints()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			TransactionLinesCollection collection = new TransactionLinesCollection(Factory);
			collection.Add(Line);
			collection.Add(CreateNewLine());
			collection.Add(CreateNewLine());
			collection.Add(CreateNewLine());
			collection.Add(CreateNewLine());

			collection[0].AL_AC = creator.CC1.PK;
			collection[1].AL_AC = creator.CC2.PK;
			collection[2].AL_AC = creator.CC3.PK;
			collection[3].AL_AC = creator.CC4.PK;
			collection[4].AL_AC = creator.CC5.PK;

			collection[0].AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			collection[1].AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			collection[2].AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			collection[3].AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			collection[4].AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			collection[0].AL_JH = Factory.NewJobWithValidTestDataForTesting<Job>().PK;
			collection[1].AL_JH = Factory.NewJobWithValidTestDataForTesting<Job>().PK;
			collection[2].AL_JH = Factory.NewJobWithValidTestDataForTesting<Job>().PK;
			collection[3].AL_JH = Factory.NewJobWithValidTestDataForTesting<Job>().PK;
			collection[4].AL_JH = Factory.NewJobWithValidTestDataForTesting<Job>().PK;

			collection[0].AL_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			collection[1].AL_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			collection[2].AL_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			collection[3].AL_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			collection[4].AL_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			collection[0].AL_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			collection[1].AL_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			collection[2].AL_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			collection[3].AL_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			collection[4].AL_GB = Factory.NewWithValidTestData<GlbBranch>().PK;

			collection[0].Branch.GB_GC = GlbCompany.CurrentCompany.PK;
			collection[1].Branch.GB_GC = GlbCompany.CurrentCompany.PK;
			collection[2].Branch.GB_GC = GlbCompany.CurrentCompany.PK;
			collection[3].Branch.GB_GC = GlbCompany.CurrentCompany.PK;
			collection[4].Branch.GB_GC = GlbCompany.CurrentCompany.PK;

			collection[0].AL_GC = GlbCompany.CurrentCompany.PK;
			collection[1].AL_GC = GlbCompany.CurrentCompany.PK;
			collection[2].AL_GC = GlbCompany.CurrentCompany.PK;
			collection[3].AL_GC = GlbCompany.CurrentCompany.PK;
			collection[4].AL_GC = GlbCompany.CurrentCompany.PK;

			collection[0].AL_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;
			collection[1].AL_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;
			collection[2].AL_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;
			collection[3].AL_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;
			collection[4].AL_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;

			SetupRelatedObjectsForFetchHintTest(collection);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TransactionLine[] loadedLines = (TransactionLine[])newFactory.Load(GetExpectedBusinessObjectType(), new ZQuery().AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("Should be 5 items loaded", 5, loadedLines.Length);
			int preLoadCount = newFactory.DatabaseLoadCount;
			AccChargeCode chargeCode = loadedLines[0].ChargeCode;
			int postLoadCount = newFactory.DatabaseLoadCount;
			Assert("Should have been 1 database load", preLoadCount + 1 <= postLoadCount);
			chargeCode = loadedLines[1].ChargeCode;
			postLoadCount = newFactory.DatabaseLoadCount;
			Assert("Should not be another load", preLoadCount + 1 <= postLoadCount);

			preLoadCount = newFactory.DatabaseLoadCount;
			GlbBranch branch = loadedLines[0].Branch;
			postLoadCount = newFactory.DatabaseLoadCount;
			Assert("Should have been 1 database load", preLoadCount + 1 <= postLoadCount);
			branch = loadedLines[1].Branch;
			postLoadCount = newFactory.DatabaseLoadCount;
			Assert("Should not be another load", preLoadCount + 1 <= postLoadCount);

			preLoadCount = newFactory.DatabaseLoadCount;
			OrgHeader org = loadedLines[0].Header;
			postLoadCount = newFactory.DatabaseLoadCount;
			Assert("Should have been 1 database load", preLoadCount + 1 <= postLoadCount);
			org = loadedLines[1].Header;
			postLoadCount = newFactory.DatabaseLoadCount;
			Assert("Should not be another load", preLoadCount + 1 <= postLoadCount);

			preLoadCount = newFactory.DatabaseLoadCount;
			AccGLHeader glHeader = loadedLines[0].GLHeader;
			postLoadCount = newFactory.DatabaseLoadCount;
			Assert("Should have been 1 database load", preLoadCount + 1 <= postLoadCount);
			glHeader = loadedLines[1].GLHeader;
			postLoadCount = newFactory.DatabaseLoadCount;
			Assert("Should not be another load", preLoadCount + 1 <= postLoadCount);

			preLoadCount = newFactory.DatabaseLoadCount;
			GlbDepartment dept = loadedLines[0].Department;
			postLoadCount = newFactory.DatabaseLoadCount;
			Assert("Should have been 1 database load", preLoadCount + 1 <= postLoadCount);
			dept = loadedLines[1].Department;
			postLoadCount = newFactory.DatabaseLoadCount;
			Assert("Should not be another load", preLoadCount + 1 <= postLoadCount);

			preLoadCount = newFactory.DatabaseLoadCount;
			Job job = loadedLines[0].InvoicingJob;
			postLoadCount = newFactory.DatabaseLoadCount;
			Assert("Shouldn't be more than 2 loads", preLoadCount + 1 <= postLoadCount);
			job = loadedLines[1].InvoicingJob;
			postLoadCount = newFactory.DatabaseLoadCount;
			Assert("Should not be another load", preLoadCount + 1 <= postLoadCount);
		}

		public void TestRelatedChargeUsesFetchHints()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			TransactionLine line2 = CreateNewLine();

			if (Line.AL_JH.IsEmpty && line2.AL_JH.IsEmpty)
			{
				Job job = creator.CreateJob("S007", creator.LocalClient, 0, creator.Agent, 0);
				Line.AL_JH = job.PK;
				line2.AL_JH = job.PK;
			}

			if (Line.AL_AC.IsEmpty && line2.AL_AC.IsEmpty)
			{
				Line.AL_AC = creator.CC1.PK;
				line2.AL_AC = creator.CC2.PK;
			}

			bool chargeLinked = false;

			if (Line.RelatedJobCharge == null && line2.RelatedJobCharge == null)
			{
				Charge charge1 = creator.CreateCharge((Job)Line.Job, Line.ChargeCode, "Test Charge 1", creator.AUD, 100m, creator.Creditor1, creator.AUD, 100m, creator.ABIGAS);
				Charge charge2 = creator.CreateCharge((Job)line2.Job, line2.ChargeCode, "Test Charge 2", creator.AUD, 200m, creator.Creditor1, creator.AUD, 200m, creator.ABIGAS);
				chargeLinked = LinkChargeToLine(charge1, Line) && LinkChargeToLine(charge2, line2);
			}

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;

			TransactionLine line_ = (TransactionLine)newFactory.Load(ExpectedBusinessObjectType, Line.PK);
			TransactionLine line2_ = (TransactionLine)newFactory.Load(ExpectedBusinessObjectType, line2.PK);
			AssertNotNull("Should be loaded", line_);
			line_.AL_Desc = "Blah 1";
			Assert("HasChanges", line_.HasChanges);

			if (line2.IsInDatabase) // A special case for the BankTransferChargeLine
			{
				AssertNotNull("Should be loaded", line2_);
				line2_.AL_Desc = "Blah 2";
				Assert("HasChanges", line2_.HasChanges);
			}

			ZQuery loadedChargesFilter = new ZQuery();
			loadedChargesFilter.FetchOnlyFromLocalCache = true;
			Charge[] loadedCharges = newFactory.Load<Charge>(loadedChargesFilter);
			AssertEquals("Pre-condition: No charges should be loaded", 0, loadedCharges.Length);

			if (chargeLinked || Line.AL_LineType == TransactionLineTypes.WIP || Line.AL_LineType == TransactionLineTypes.Accrual)
			{
				AssertNotNull("Access RelatedCharge", line_.RelatedJobCharge);
			}
			else
			{
				AssertNull("RelatedCharge was not linked", line_.RelatedJobCharge);
			}

			loadedCharges = newFactory.Load<Charge>(loadedChargesFilter);
			int loaded = (from Charge charge in loadedCharges select charge.PK).Distinct().Count();
			if (Line.AL_LineType == TransactionLineTypes.WIP || Line.AL_LineType == TransactionLineTypes.Accrual)
			{
				AssertEquals("Both charges should be loaded", 2, loaded);
			}
			else if (!chargeLinked)
			{
				AssertEquals("No charges should be loaded", 0, loaded);
			}
		}

		protected virtual bool LinkChargeToLine(Charge charge, TransactionLine line)
		{
			bool result = true;
			switch (line.AL_LineType)
			{
				case TransactionLineTypes.Accrual:
				case TransactionLineTypes.Cost:
				case TransactionLineTypes.UnapprovedCost:
					charge.JR_AL_APLine = line.PK;
					break;
				case TransactionLineTypes.Revenue:
				case TransactionLineTypes.WIP:
					charge.JR_AL_ARLine = line.PK;
					break;
				default:
					result = false;
					break;
			}
			charge.SetAmountsToLinkedLinesForTests();
			return result;
		}

		protected virtual void SetupRelatedObjectsForFetchHintTest(TransactionLinesCollection collection)
		{
		}

		public void TestDeveloperErrorIsReportedIfHighPrecisionExchangeRateIsZero()
		{
			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);

			Line.AL_RX_NKTransactionCurrency = uSD.RX_Code;
			SetAmountsAndCalculateExchangeRate(false, 0.7m, 1m, 0m, 0m);
			SetAmountsAndCalculateExchangeRate(false, 0.7m, 0.7m, 100m, 70m);
		}

		void SetAmountsAndCalculateExchangeRate(bool expected,
			ZDecimal initialExchangeRate, ZDecimal expectedExchangeRate,
			ZDecimal aL_LocalExTaxAmount, ZDecimal aL_OSExTaxAmount)
		{
			Line.AL_ExchangeRate = initialExchangeRate;
			Line.AL_OSExTaxAmount = aL_OSExTaxAmount;
			Line.AL_LocalExTaxAmount = aL_LocalExTaxAmount;

			AssertEquals("Precondition: InitialExchangeRate", initialExchangeRate, Line.AL_ExchangeRate);
			AssertEquals("Precondition: AL_OSExTaxAmount", aL_OSExTaxAmount, Line.AL_OSExTaxAmount);
			AssertEquals("Precondition: AL_OSExTaxAmount", aL_LocalExTaxAmount, Line.AL_LocalExTaxAmount);

			ErrorReporter.Clear(); // This will report other errors, we just want our one created in the method below.

			Line.CalculateHighPrecisionExchangeRate();
			AssertEquals("Developer Error should be reported", expected, !string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			AssertEquals("ExpectedExchangeRate", expectedExchangeRate, Line.AL_ExchangeRate);
			AssertEquals("AL_OSExTaxAmount", aL_OSExTaxAmount, Line.AL_OSExTaxAmount);
			AssertEquals("AL_LocalExTaxAmount", aL_LocalExTaxAmount, Line.AL_LocalExTaxAmount);
		}

		public virtual void TestCalculateOSAmountsWhenZeroGST()
		{
			if (LineCanHaveForeignCurrency)
			{
				var companyFactory = new BusinessObjectFactory();
				var currentCompany = companyFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				var originalReciprocal = currentCompany.GC_IsReciprocal;

				try
				{
					currentCompany.GC_IsReciprocal = true;
					companyFactory.Save();

					using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
					{
						ExchangeRateReader.GetReaderInstance().ClearCache();

						var line = CreateNewLine();
						if (line.TransactionHeader != null)
						{
							line.TransactionHeader.AH_FullyPaidDate = ZDateTime.Empty;
							line.TransactionHeader.AH_RX_NKTransactionCurrency = ForeignCurrency.Code;
						}
						line.AL_GB = GlbBranch.CurrentBranch.PK;
						line.AL_GE = GlbDepartment.CurrentDepartment.PK;
						line.AL_ExchangeRate = 0.000106006m;
						line.AL_OSExTaxAmount = 852500m;
						line.AL_AG = TestObjectCreator.GLHeader1.PK;
						Factory.Save();
						var factoryForLoading = new BusinessObjectFactory();
						line = (TransactionLine)factoryForLoading.Load(GetExpectedBusinessObjectType(), line.PK);
						AssertEquals(852500m, line.AL_OSExTaxAmount);
					}
				}
				finally
				{
					currentCompany.GC_IsReciprocal = originalReciprocal;
					companyFactory.Save();
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestGetDefaultGLPostingAccounts()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(-1);
			consol1.JK_TransportMode = Constants.TransportModes.Sea;
			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol1.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now;
			consol2.JK_TransportMode = Constants.TransportModes.Sea;
			var consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol3.Transports.MostInterestingTransport.JW_ETD = ZDateTime.Now.AddDays(-2);
			consol3.JK_TransportMode = Constants.TransportModes.Air;

			var gatewayConsol = testObjectCreator.CreateGatewayConsol(consolNum: "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			gatewayConsol.JK_TransportMode = Constants.TransportModes.Air;
			gatewayConsol.JK_ConsolMode = Constants.ContainerModes.LCL;
			gatewayConsol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			Factory.Save();

			var gatewayJob = testObjectCreator.CreateJob(gatewayConsol, createWithMutex: false);
			var gatewayCharge = testObjectCreator.CreateCharge(gatewayJob, testObjectCreator.CC1, 15m, 15m);

			testObjectCreator.CC1.AC_AG_CostAccount = testObjectCreator.ExchangeGainLossControlAccount.PK;
			var gLPostingOverrides = testObjectCreator.CC1.GLPostingOverrides;
			gLPostingOverrides.DeleteAll();
			AddGLPostingOverride(gLPostingOverrides, gatewayCharge.Department.PK, testObjectCreator.GLHeader1.PK, jobType: "SHP", direction: "OTH", transportMode: "SEA", consolContainerMode: "FCL", masterPaymentType: "CCX", housePaymentType: "PPD");
			AddGLPostingOverride(gLPostingOverrides, gatewayCharge.Department.PK, testObjectCreator.GLHeader2.PK, jobType: "GCN", direction: "EXP", transportMode: "AIR", consolContainerMode: "LCL", masterPaymentType: "PPD");

			Factory.Save();

			var shipment = consol1.Shipments.AddNew();

			try
			{
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
				consol2.Shipments.Add(shipment);
				consol3.Shipments.Add(shipment);

				var apportionments1 = new ApportionmentListing(Factory, consol1);
				var consolCost1 = CreateConsolCost(testObjectCreator, apportionments1, testObjectCreator.CC1.PK, testObjectCreator.GSTFREE1.PK, 780.00m, 0.4000m, "SHP");

				var invoice1 = Factory.New<APInvoice>();
				invoice1.AH_OH = testObjectCreator.AALSHI.PK;
				invoice1.ConsolCosting.ConsolCosts.Add(consolCost1);
				invoice1.ImportAllApportionmentsFromCosting();

				var line1 = invoice1.Lines[0];
				var gLPostingAccounts1 = line1.GetDefaultGLPostingAccounts();

				AssertEquals(testObjectCreator.GLHeader1.PK, gLPostingAccounts1.CostAccount);

				var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
				invoice2.IsConvertedFromARInvoice = true;
				invoice2.SubmittedFromInvoicingForm = true;

				var invoiceLineImportedFromGatewayCharge = (InvoicingLineBase)invoice2.Lines.AddNew();
				invoice2.ImportJobChargesIntoInvoice(new[] { gatewayCharge }, invoiceLineImportedFromGatewayCharge, false);
				var line2 = invoice2.Lines[0];

				var gLPostingAccounts2 = line2.GetDefaultGLPostingAccounts();
				AssertEquals(testObjectCreator.GLHeader2.PK, gLPostingAccounts2.CostAccount);
			}
			finally
			{
				shipment.Job.Dispose();
				gatewayJob.Dispose();
			}
		}

		public void TestGetDefaultGLPostingAccountsWhenConsolIsNotForwardingConsol()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "ABC";

			var job = Factory.NewWithValidTestData<Job>();
			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.ConsumerType).Returns(JobInvoicingConsumerTypes.GatewayConsol);
			mockSupporter.Setup(m => m.TransportMode).Returns("AIR");
			mockSupporter.Setup(m => m.IsDomestic).Returns(true);

			var mockInvoicingPlugIn = new Mock<IJobInvoicingPlugIn>();
			mockInvoicingPlugIn.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);
			mockInvoicingPlugIn.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			mockInvoicingPlugIn.Setup(m => m.IsDeleted).Returns(false);
			job.Parent = mockInvoicingPlugIn.Object;

			var mockLine = Factory.NewMoq<APInvoiceLine>();
			mockLine.Setup(m => m.Job).Returns(job);
			mockLine.Setup(m => m.ChargeCode).Returns(chargeCode);
			AssertNoExceptionThrown(() => mockLine.Object.GetDefaultGLPostingAccounts());
			ErrorReporter.Clear();
		}

		AccChargeGLPostingOverride AddGLPostingOverride(AccChargeGLPostingOverrideCollection collection, ZGuid departmentPK, ZGuid account, string categoryClass = "ALL", string jobType = "ALL", string transportMode = "ALL", string direction = "ALL", string consolContainerMode = "ALL", string masterPaymentType = "ALL", string housePaymentType = "ALL")
		{
			var glPostingOverride = collection.AddNew();
			glPostingOverride.Y1_ConsolidationAccountingCategoryClass = categoryClass;
			if (!departmentPK.IsEmpty)
			{
				glPostingOverride.Y1_GE = departmentPK;
			}
			glPostingOverride.Y1_JobType = jobType;
			glPostingOverride.Y1_TransportMode = transportMode;
			glPostingOverride.Y1_Direction = direction;
			glPostingOverride.Y1_ConsolContainerMode = consolContainerMode;
			glPostingOverride.Y1_MasterPaymentType = masterPaymentType;
			glPostingOverride.Y1_HousePaymentType = housePaymentType;
			glPostingOverride.Y1_AG_CST = account;
			glPostingOverride.Y1_AG_ACR = account;
			glPostingOverride.Y1_AG_WIP = account;
			glPostingOverride.Y1_AG_REV = account;

			return glPostingOverride;
		}

		[SuspendCriticalValidation]
		public void TestOverseasAmountsAreCalculatedCorrectlyWhenReloaded_LocalCurrency_NoTax()
		{
			RefCurrency currency = GlbCompany.CurrentCompany.LocalCurrency;
			ZDecimal initialExchangeRate = 1M;

			ZDecimal oSExTaxAmount = 2500000M;
			ZDecimal oSTaxAmount = 0M;

			CoreTestForTestOverseasAmountsAreCalculatedCorrectly(false, currency, initialExchangeRate, oSExTaxAmount, oSTaxAmount);

			CoreTestForTestOverseasAmountsAreCalculatedCorrectly(true, currency, initialExchangeRate, oSExTaxAmount, oSTaxAmount);
		}

		public void TestOverseasAmountsAreCalculatedCorrectlyWhenReloaded_LocalCurrency_WithTax()
		{
			if (LineCanHaveTaxComponent)
			{
				RefCurrency currency = GlbCompany.CurrentCompany.LocalCurrency;
				ZDecimal initialExchangeRate = 1M;

				ZDecimal oSExTaxAmount = 2500000M;
				ZDecimal oSTaxAmount = 250000M;

				CoreTestForTestOverseasAmountsAreCalculatedCorrectly(false, currency, initialExchangeRate, oSExTaxAmount, oSTaxAmount);

				CoreTestForTestOverseasAmountsAreCalculatedCorrectly(true, currency, initialExchangeRate, oSExTaxAmount, oSTaxAmount);
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestOverseasAmountsAreCalculatedCorrectlyWhenReloaded_ForeignCurrency_NoTax()
		{
			if (LineCanHaveForeignCurrency)
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);

				ZDecimal oSExTaxAmount = 2500000M;
				ZDecimal oSTaxAmount = 0M;

				ZDecimal localExTaxAmount = 275.63M;

				CoreTestForTestOverseasAmountsAreCalculatedCorrectly(false, currency, oSExTaxAmount / localExTaxAmount, oSExTaxAmount, oSTaxAmount);

				oSExTaxAmount = 275.63M;
				localExTaxAmount = 2500000M;

				CoreTestForTestOverseasAmountsAreCalculatedCorrectly(true, currency, localExTaxAmount / oSExTaxAmount, oSExTaxAmount, oSTaxAmount);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestOverseasAmountsAreCalculatedCorrectlyWhenReloaded_ForeignCurrency_WithTax()
		{
			if (LineCanHaveForeignCurrency && LineCanHaveTaxComponent)
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);

				ZDecimal oSExTaxAmount = 2500000M;
				ZDecimal oSTaxAmount = 250000M;

				ZDecimal localExTaxAmount = 275.63M;

				CoreTestForTestOverseasAmountsAreCalculatedCorrectly(false, currency, oSExTaxAmount / localExTaxAmount, oSExTaxAmount, oSTaxAmount);

				oSExTaxAmount = 275.63M;
				oSTaxAmount = 27.56M;
				localExTaxAmount = 2500000M;

				CoreTestForTestOverseasAmountsAreCalculatedCorrectly(true, currency, localExTaxAmount / oSExTaxAmount, oSExTaxAmount, oSTaxAmount);
			}
			else
			{
				Assert(true);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected virtual void CoreTestForTestOverseasAmountsAreCalculatedCorrectly(bool companyIsReciprocal, RefCurrency currency, ZDecimal initialExchangeRate, ZDecimal oSExTaxAmount, ZDecimal oSTaxAmount)
		{
			ZDecimal overseasTotal = oSExTaxAmount + oSTaxAmount;

			var initialUserContext = Env.CurrentUserContext;

			try
			{
				TestObjectCreator.SetCurrentCompanyReciprocal(companyIsReciprocal);

				Line = CreateNewLine();

				if (Line.TransactionHeader != null)
				{
					Line.TransactionHeader.AH_ExchangeRate = initialExchangeRate;
					Line.TransactionHeader.AH_RX_NKTransactionCurrency = currency.RX_Code;
				}
				Line.AL_RX_NKTransactionCurrency = currency.RX_Code;
				Line.AL_ExchangeRate = initialExchangeRate;
				Line.AL_OSExTaxAmount = oSExTaxAmount;
				Line.AL_OSTaxAmount = oSTaxAmount;
				Line.AL_OverseasTotal = overseasTotal;
				if (Line is GLJournalLine)
				{
					var gljournal = Factory.Load<GLJournal>(Line.TransactionHeader.PK);
					var line2 = TestObjectCreator.CreateGLJournalLine(gljournal, oSExTaxAmount, DebitCredit.CR, Line.AL_AG);
				}

				ZDecimal localExTaxAmount = Line.AL_LocalExTaxAmount;
				ZDecimal localTaxAmount = Line.AL_LocalTaxAmount;
				ZDecimal localTotalAmount = localExTaxAmount + localTaxAmount;

				TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(Line);

				AssertOSValues(Line, oSExTaxAmount, oSTaxAmount, overseasTotal);
				AssertLocalValues(Line, localExTaxAmount, localTaxAmount, localTotalAmount);

				Line.Factory.Save();

				AssertOSValues(Line, oSExTaxAmount, oSTaxAmount, overseasTotal);
				AssertLocalValues(Line, localExTaxAmount, localTaxAmount, localTotalAmount);

				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				TransactionLine reloadedLine = (TransactionLine)newFactory.Load(GetExpectedBusinessObjectType(), Line.PK);

				AssertOSValues(reloadedLine, oSExTaxAmount, oSTaxAmount, overseasTotal);
				AssertLocalValues(reloadedLine, localExTaxAmount, localTaxAmount, localTotalAmount);
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		protected void AssertOSValues(TransactionLine line, ZDecimal oSExTaxAmount, ZDecimal oSTaxAmount, ZDecimal overseasTotal)
		{
			AssertEquals("AL_OSExTaxAmount", oSExTaxAmount, line.AL_OSExTaxAmount);
			AssertEquals("AL_OSTaxAmount", oSTaxAmount, line.AL_OSTaxAmount);
			AssertEquals("AL_OverseasTotal", overseasTotal, line.AL_OverseasTotal);
		}

		void AssertLocalValues(TransactionLine line, ZDecimal localExTaxAmount, ZDecimal localTaxAmount, ZDecimal localTotalAmount)
		{
			Assert("LocalExTaxAmount is not zero", line.AL_LocalExTaxAmount != 0);
			AssertEquals("LocalExTaxAmount", localExTaxAmount, line.AL_LocalExTaxAmount);

			AssertEquals("LocalTaxAmount", localTaxAmount, line.AL_LocalTaxAmount);

			Assert("LocalTotalAmount is not zero", line.AL_LocalTotalAmount != 0);
			AssertEquals("LocalTotalAmount", localTotalAmount, line.AL_LocalTotalAmount);
		}

		void SetCurrentCompanyCountryCode(string code)
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = code;
		}

		#region CollectionTests

		public void TestChargeCodeCollection()
		{
			AssertNotNull("Collection should be instantiated", Line.ChargeCodeCollection);
		}

		public void TestOrganisationsCollection()
		{
			AssertNotNull("Organisations should be instantiated", Line.OrganisationsCollection);
		}

		public void TestJobsCollection()
		{
			AssertNotNull("Job Collection should be instantiated", Line.JobCollection);
		}

		public void TestBranchCollection()
		{
			AssertNotNull("Branch Collection should be instantiated", Line.Lookups.Branches);
		}

		public void TestDepartmentCollection()
		{
			AssertNotNull("Department Collection should be instantiated", Line.DepartmentCollection);
		}

		#endregion

		#region CreatingUser and CreateDate Field Tests

		public void TestCreatingUserID()
		{
			SetupAndSaveLine();
			AssertEquals("Creating User ID", GlbStaff.CurrentUser.GS_LoginName, Line.CreatingUserID);
		}

		public void TestCreatingUser()
		{
			SetupAndSaveLine();
			AssertEquals("Creating User", GlbStaff.CurrentUser.GS_FullName, Line.AL_Calc_CreatingUserName);
		}

		[TestDate(2024, 9, 3)]
		public void TestCreateDate()
		{
			SetupAndSaveLine();
			AssertEquals("CreatedDate", new ZDate(2024, 9, 3), Line.AL_Calc_CreatedDate.Date);
		}

		public void TestCreatingUserReadOnly()
		{
			Assert("Creating User should be readonly", Line.AL_Calc_CreatingUserNameInfo.ReadOnly);
		}

		public void TestCreatedDateReadOnly()
		{
			Assert("Created Date should be readonly", Line.AL_Calc_CreatedDateInfo.ReadOnly);
		}

		#endregion

		#region Department Filter Test

		public void TestChargeCodeDepartmentFilter()
		{
			Line.AL_GE = CurrentDepartment.PK;
			Line.ChargeCodeCollection.Load();
			Assert("Should have current dept charge code", Line.ChargeCodeCollection.Contains(CurrentDepartmentChargeCode.PK));
			Assert("Should have all dept charge code", Line.ChargeCodeCollection.Contains(AllDepartmentsChargeCode.PK));
			Assert("Should not have non-current dept charge code", !Line.ChargeCodeCollection.Contains(NonCurrentDepartmentChargeCode.PK));

			Line.AL_GE = NonCurrentDepartment.PK;
			Line.ChargeCodeCollection.Load();
			Assert("Should have non-current dept charge code", Line.ChargeCodeCollection.Contains(NonCurrentDepartmentChargeCode.PK));
			Assert("Should have all dept charge code", Line.ChargeCodeCollection.Contains(AllDepartmentsChargeCode.PK));
			Assert("Should not have current dept charge code", !Line.ChargeCodeCollection.Contains(CurrentDepartmentChargeCode.PK));

			Line.AL_GE = ZGuid.Empty;
			Line.ChargeCodeCollection.Load();
			Assert("Charge Code collection should be emtpy", Line.ChargeCodeCollection.Count == 0);
		}

		public void TestDepartmentFilter()
		{
			GlbDepartmentCollection allDeptCollection = new GlbDepartmentCollection(Factory);

			allDeptCollection[0].GE_IsActive = false;

			allDeptCollection.AdditionalFilter = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
			int activeDeptCount = allDeptCollection.Count;

			AssertEquals(activeDeptCount, Line.DepartmentCollection.Count);
		}

		#endregion

		#region TestPostDateValidation

		public void TestPostDateValidation()
		{
			Assert("Date should not have errors to begin", !Line.AL_PostDateInfo.HasErrors());
			Line.AL_PostDate = PreviousGLClosedPeriod.AM_StartDate.AddMonths(-6);
			Assert("Invalid Date should validate", Line.AL_PostDateInfo.HasErrors());
		}

		#endregion TestPostDateValidation

		#region TestIndexBusinessObjectWithCalculatedFieldName

		public void TestIndexBusinessObjectWithCalculatedFieldName()
		{
			Line[TransactionLine.Schema.AL_LocalExTaxAmount] = 50m; // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
			AssertEquals(50m, Line[TransactionLine.Schema.AL_LocalExTaxAmount]);
		}

		#endregion TestIndexBusinessObjectWithCalculatedFieldName

		#region Test Calculated TAX/WHT

		[ExpectNoExceptions]
		public void TestAL_OSExTaxAmountValidateRange()
		{
			Line.AL_OSExTaxAmount = 1000000000000000.00m;
			Assert(Line.AL_OSExTaxAmountInfo.HasErrors());
			AssertEquals(0m, Line.AL_OverseasTotal);
			Line.AL_OSExTaxAmount = 999999999999999.00m;
			Assert(!Line.AL_OSExTaxAmountInfo.HasErrors());
			AssertEquals(999999999999999.00m, Line.AL_OverseasTotal);
		}

		[ExpectNoExceptions]
		public void TestAL_OSTaxAmountValidateRange()
		{
			Line.AL_OSExTaxAmount = 100m;
			Line.AL_OSTaxAmount = 1000000000000000.00m;
			Assert(Line.AL_OSTaxAmountInfo.HasErrors());
			AssertEquals(100m, Line.AL_OverseasTotal);

			Line.AL_OSTaxAmount = 999999999999999.00m;
			Assert(!Line.AL_OSTaxAmountInfo.HasErrors());
			AssertEquals(1000000000000099.00m, Line.AL_OverseasTotal);
		}

		public void TestAH_OSExTaxAmountCalculatesLocalAndBack()
		{
			SetupLineExRatesAndAmounts(0.57m, 1000, 200.453m, 0m);

			AssertEquals("Local Amount", 351.67m, Line.AL_LocalExTaxAmount);

			AssertEquals("OS Ex Tax Amount", 200.453m, Line.AL_OSExTaxAmount);
		}

		public void TestOSTaxAmountCalculatesLocalAndBack()
		{
			SetupLineExRatesAndAmounts(0.5728m, 100, 0m, 53.75m);

			AssertEquals("Local Tax Amount", 93.84m, Line.AL_LocalTaxAmount);
			AssertEquals("OS Tax Amount", 53.75m, Line.AL_OSTaxAmount);
		}

		public void TestOSAmountUpdatesOnOSExTaxAndOSTax()
		{
			Line.AL_ExchangeRate = 1m;

			Line.AL_OSExTaxAmount = 50.00m;

			SetupLineExRatesAndAmounts(1m, 100, 50.00m, 0m);

			AssertEquals("OS Total should update", 50.00m, Line.AL_OverseasTotal);

			Line.AL_OSTaxAmount = 20.00m;
			AssertEquals("OS Total should update", 70.00m, Line.AL_OverseasTotal);

			Line.AL_OSTaxAmount = 0;
			AssertEquals("OS Total should update", 50.00m, Line.AL_OverseasTotal);
		}

		public void Test_OSExTaxAmount_Updates_OSTaxAmount_When_TaxRate_IsNotReverseCharge()
		{
			string originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);

			try
			{
				AccTaxRate gST = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST"));
				gST.SetRate_ForTestOnly(10, 1);

				BusinessObjectFactory tempFactory = new BusinessObjectFactory();

				AccTaxRate gSTREV = tempFactory.New<AccTaxRate>();
				gSTREV.AT_Code = "GSTREV";
				gSTREV.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				gSTREV.AT_Type = AccTaxRate.Types.ReverseRated;
				gSTREV.SetRate_ForTestOnly(175, 10);

				AccTaxRate fREEGSTREV = tempFactory.New<AccTaxRate>();
				fREEGSTREV.AT_Code = "FREEGSTREV";
				fREEGSTREV.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				fREEGSTREV.AT_Type = AccTaxRate.Types.ReverseRated;
				fREEGSTREV.SetRateNumerator_ForTestOnly(0);

				tempFactory.Save();

				Line.AL_AT = gSTREV.PK;
				Line.AL_OSExTaxAmount = 100m;
				AssertEquals("AL_OSTaxAmount", 0m, Line.AL_OSTaxAmount);

				Line.AL_AT = fREEGSTREV.PK;
				Line.AL_OSExTaxAmount = 200m;
				AssertEquals("AL_OSTaxAmount", 0m, Line.AL_OSTaxAmount);

				Line.AL_AT = gST.PK;
				Line.AL_OSExTaxAmount = 300m;
				AssertEquals("AL_OSTaxAmount", true, Line.AL_OSTaxAmount != 0m);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public virtual void TestInternalOSAmountFieldsSetOnLoadCorrectly()
		{
			SetupLineExRatesAndAmounts(0.57m, 1000, 200.453m, 15.02m);
			AssertEquals("OS Ex Tax Amount on Load", 200.453m, Line.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 15.02m, Line.AL_OSTaxAmount);

			Line.Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();

			TransactionLine loadedLine = (TransactionLine)newFactoryForLoad.Load(GetExpectedBusinessObjectType(), Line.PK);

			AssertEquals("OS Ex Tax Amount on Load", 200.453m, loadedLine.AL_OSExTaxAmount);
			AssertEquals("OS Tax Amount on Load", 15.02m, loadedLine.AL_OSTaxAmount);
		}

		#endregion Test Calculated TAX/WHT

		#region OS Amount Rounding Tests

		public void TestSettingOSValuesRoundsThem()
		{
			// set line amount with currency
			RefCurrency currency2DP = Factory.NewWithValidTestData<RefCurrency>();
			currency2DP.RX_SubUnitRatio = 100;
			Line.AL_RX_NKTransactionCurrency = currency2DP.RX_Code;
			Line.AL_OSExTaxAmount = 100.968m;
			Line.AL_OSTaxAmount = 33.457m;
			AssertEquals("OS Ex Tax amount should be rounded to 2 decimal places", 100.97m, Line.AL_OSExTaxAmount);
			AssertEquals("OSTax amount should be rounded to 2 decimal places", 33.46m, Line.AL_OSTaxAmount);
			AssertEquals("OverseasTotal should be rounded to 2 decimal places", 134.43m, Line.AL_OverseasTotal);

			// set line amount with currency and different decimals
			RefCurrency currency0DP = Factory.NewWithValidTestData<RefCurrency>();
			currency0DP.RX_SubUnitRatio = 0;
			Line.AL_RX_NKTransactionCurrency = currency0DP.RX_Code;
			Line.AL_OSExTaxAmount = 200.854m;
			Line.AL_OSTaxAmount = 44.568m;
			AssertEquals("OS Ex Tax amount should be rounded to 0 decimal places", 201m, Line.AL_OSExTaxAmount);
			AssertEquals("OSTax amount should be rounded to 0 decimal places", 45m, Line.AL_OSTaxAmount);
			AssertEquals("OverseasTotal should be rounded to 0 decimal places", 246m, Line.AL_OverseasTotal);

			// set line amount with no currency
			int originalSubUnitRatio = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			try
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 100;
				Line.AL_RX_NKTransactionCurrency = ZString.Empty;
				Line.AL_OSExTaxAmount = 130.345m;
				Line.AL_OSTaxAmount = 13.4234m;
				AssertEquals("OSTax amount should be rounded to decimals in local currency", 13.42m, Line.AL_OSTaxAmount);
				AssertEquals("OS Ex Tax amount should be rounded to decimals in local currency", 130.35m, Line.AL_OSExTaxAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = originalSubUnitRatio;
			}

			//set current country and currency to iceland
			bool originalIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;

			string currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			try
			{
				SetCurrentCompanyCountryCode(Core.Constants.CountryCodes.Iceland);
				Line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Iceland;
				Line.AL_LineType = TransactionLineTypes.Revenue;
				Line.AL_OSExTaxAmount = 22.22m;
				AssertEquals("AL_OSExTaxAmount should be rounded to int for icelandic company.", 22m, Line.AL_OSExTaxAmount);
				Line.AL_OSExTaxAmount = 44.50m;
				AssertEquals("AL_OSExTaxAmount should be rounded to int.", 45.00m, Line.AL_OSExTaxAmount);

				Line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
				Line.AL_OSExTaxAmount = 22.22m;
				AssertEquals("AL_OSExTaxAmount shouldn't be rounded to int for not icelandic countries .", 22.22m, Line.AL_OSExTaxAmount);

				Line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Iceland;
				SetCurrentCompanyCountryCode(Core.Constants.CountryCodes.Australia);
				Line.AL_OSExTaxAmount = 22.22m;
				AssertEquals("AL_OSExTaxAmount should be rounded to int for not icelandic countries .", 22m, Line.AL_OSExTaxAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = originalIsGSTRegistered;
				SetCurrentCompanyCountryCode(currentCountry);
			}
		}

		public void TestRoundTaxAmountForIcelandCompanyAndDifferentLineTypes()
		{
			//set current country and currency to iceland
			bool originalIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			string currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			try
			{
				SetCurrentCompanyCountryCode(Core.Constants.CountryCodes.Iceland);
				Line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Iceland;
				Line.AL_LineType = TransactionLineTypes.Accrual;
				Line.AL_OSExTaxAmount = 22.22m;
				AssertEquals("AL_OSExTaxAmount should be rounded when line type is Accural.", 22m, Line.AL_OSExTaxAmount);
				Line.AL_LineType = TransactionLineTypes.Cost;
				Line.AL_OSExTaxAmount = 33.33m;
				AssertEquals("AL_OSExTaxAmount should be rounded when line type is Cost.", 33m, Line.AL_OSExTaxAmount);
				Line.AL_LineType = TransactionLineTypes.Revenue;
				Line.AL_OSExTaxAmount = 44.44m;
				AssertEquals("AL_OSExTaxAmount should be rounded to int when line type is Revenue.", 44m, Line.AL_OSExTaxAmount);
				Line.AL_LineType = TransactionLineTypes.WIP;
				Line.AL_OSExTaxAmount = 55.55m;
				AssertEquals("AL_OSExTaxAmount should be rounded to int when line type is WIP.", 56m, Line.AL_OSExTaxAmount);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = originalIsGSTRegistered;
				SetCurrentCompanyCountryCode(currentCountry);
			}
		}

		public virtual void TestRoundAmountToCurrencyDecimals()
		{
			RefCurrency currency3DP = Factory.New<RefCurrency>();
			currency3DP.RX_Code = "ZZ3";
			currency3DP.RX_SubUnitRatio = 1000;
			Line.AL_RX_NKTransactionCurrency = currency3DP.RX_Code;
			AssertEquals("Should be rounded to 3 dec.pl.", 234.553m, Line.RoundAmountToCurrencyDecimals_ForTestOnly(234.5534m));
			AssertEquals("Should be rounded to 3 dec.pl.", 234.553m, Line.RoundAmountToCurrencyDecimals_ForTestOnly(234.5525m));
		}

		public void TestOSExTaxAmountRoundsGSTVATCorrectly()
		{
			AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRateNumerator_ForTestOnly(10);
			RefCurrency currency2DP = Factory.New<RefCurrency>();
			currency2DP.RX_Code = "XX2";
			currency2DP.RX_SubUnitRatio = 100;
			Line.AL_RX_NKTransactionCurrency = currency2DP.RX_Code;
			Line.AL_AT = taxRate.PK;
			Line.AL_OSExTaxAmount = 454.545454m;
			AssertEquals("Should be rounded to 2 dec.pl.", 454.55m, Line.AL_OSExTaxAmount);
			AssertEquals("Should be rounded to 2 dec.pl.", 45.45m, Line.AL_OSTaxAmount);
		}

		public void TestSettingCurrencyRoundsOSAmounts()
		{
			AccTaxRate taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRateNumerator_ForTestOnly(10);
			RefCurrency currency1DP = Factory.New<RefCurrency>();
			currency1DP.RX_Code = "XYZ";
			currency1DP.RX_SubUnitRatio = 10;
			Line.AL_AT = taxRate.PK;
			Line.AL_OSExTaxAmount = 109.2435m;
			Assert("Precondition: Not rounded to 1 decimal place", Line.AL_OSExTaxAmount != 109.2m);
			Assert("Precondition: Not rounded to 1 decimal place", Line.AL_OSTaxAmount != 10.9m);
			Line.AL_RX_NKTransactionCurrency = currency1DP.RX_Code; // causes recalculation of tax amount in InvoicingLineBase
			AssertEquals("OS Ex Tax amount on the line should be rounded to 1 decimal place", 109.2m, Line.AL_OSExTaxAmount);
			AssertEquals("OS Tax amount on the line should be rounded to 1 decimal place", 10.9m, Line.AL_OSTaxAmount);
		}

		#endregion

		#region TestConcurrency

		[ExpectNoExceptions()]
		public void TestConcurrency()
		{
			Factory.RefreshEnabled = false;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;

			AccTransactionLines loadedLine = (AccTransactionLines)newFactory.Load(GetExpectedBusinessObjectType(), Line.PK);
			loadedLine.AL_PostDate = new ZDateTime(2001, 1, 4);

			Line.AL_PostToGL = "Y";

			Factory.Save();
			newFactory.Save();//shouldn't give me a concurrency exception
		}

		#endregion TestConcurrency

		#region IDataExportBatchSource Members

		public void TestIsDataExportBatchSupported()
		{
			var transaction = CreateNewLine();

			if (transaction.AL_LineType == TransactionLineTypes.Accrual ||
				transaction.AL_LineType == TransactionLineTypes.WIP)
			{
				AssertEquals("IsDataExportBatchSupported", true, ((IDataExportBatchSource)transaction).IsDataExportBatchSupported);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestRelatedBatchCollection()
		{
			var batch = TestObjectCreator.CreateDataExportBatchForLine(Line);
			Factory.Save();
			AssertCollectionContains("Public collection contains batch", batch, Line.DataExportBatchCollection);
		}

		#endregion

		public void TestErrorMessageIfInvalidAL_AC_AL_AG()
		{
			var chargeCode = TestObjectCreator.CC1;
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("1", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();

			chargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			invoiceLine.AL_AC = chargeCode.PK;
			AssertEquals("GL Account is not filled in for CMT charge type", ZGuid.Empty, invoiceLine.AL_AG);
			AssertEquals("No errors expected.", "", invoiceLine.ErrorMessageIfInvalidAL_AC_AL_AG());

			chargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			invoiceLine.ChargeCode.SetGLAccountDataForTesting(TestObjectCreator.GLHeader1);

			invoiceLine.AL_AC = ZGuid.Empty;
			invoiceLine.AL_AG = ZGuid.Empty;
			AssertEquals("AL_AG or AL_AC must be entered.", TransactionLine.EmptyChargeCodeAndGLHeaderError, invoiceLine.ErrorMessageIfInvalidAL_AC_AL_AG());

			invoiceLine.AL_AC = chargeCode.PK;
			invoiceLine.AL_AG = ZGuid.Empty;
			AssertEquals("Charge Code must have GL Account entered.", TransactionLine.GetInvalidChargeCodeError(invoiceLine.ChargeCode.AC_Code, invoiceLine.AL_LineType), invoiceLine.ErrorMessageIfInvalidAL_AC_AL_AG());

			invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			AssertEquals("No errors expected.", "", invoiceLine.ErrorMessageIfInvalidAL_AC_AL_AG());

			invoiceLine.AL_AG = TestObjectCreator.GLHeader2.PK;
			AssertEquals("Actual reason for the error is that GL account isn't the same as the corresponding the GL Account on the charge code",
				TransactionLine.GetInvalidChargeCodeError(invoiceLine.ChargeCode.AC_Code, invoiceLine.AL_LineType),
				invoiceLine.ErrorMessageIfInvalidAL_AC_AL_AG());

			invoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			TestObjectCreator.AttachJobToAPLine(invoiceLine);
			TestObjectCreator.AttachChargeToAPLine(invoiceLine);
			Factory.Save();

			invoiceLine.AL_AG = TestObjectCreator.GLHeader2.PK;
			AssertEquals("No errors expected since it is in database.", "", invoiceLine.ErrorMessageIfInvalidAL_AC_AL_AG());

			invoice.IsReverseTransaction = true;
			var invoiceLine2 = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine2.AL_AC = chargeCode.PK;
			invoiceLine2.AL_AG = TestObjectCreator.GLHeader2.PK;
			AssertNotEquals("Line GL Account is different from Charge Code GL Account", invoiceLine2.AL_AG, chargeCode.AC_AG_CostAccount);
			AssertEquals("No errors expected since it is reverse transaction", "", invoiceLine2.ErrorMessageIfInvalidAL_AC_AL_AG());
		}

		public virtual void TestAL_AG_AL_ACSettersValidateBoth()
		{
			var chargeCode = TestObjectCreator.CC1;
			chargeCode.AC_AG_AccrualAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_RevenueAccount = TestObjectCreator.GLHeader1.PK;
			chargeCode.AC_AG_WIPAccount = TestObjectCreator.GLHeader1.PK;

			TransactionLine transactionLine = (TransactionLine)GetNewBusinessObject();

			transactionLine.AL_AC = ZGuid.Empty;
			transactionLine.AL_AG = ZGuid.Empty;
			transactionLine.AL_LineType = TransactionLineTypes.Cost;
			Assert("AL_AC should have the error: " + ExpectedEmptyAL_ACErrorMessage, !AcceptAL_AC || transactionLine.AL_ACInfo.HasError(ExpectedEmptyAL_ACErrorMessage));
			Assert("AL_AG should have the error: " + ExpectedEmptyAL_AGErrorMessage, !AcceptAL_AG || transactionLine.AL_AGInfo.HasError(ExpectedEmptyAL_AGErrorMessage));

			if (AcceptAL_AC)
			{
				transactionLine.AL_AC = chargeCode.PK;
				Assert("AL_AG should not be empty", !transactionLine.AL_AG.IsEmpty);
				Assert("AL_AC should not have the error: " + ExpectedEmptyAL_ACErrorMessage, !(AcceptAL_AC && transactionLine.AL_ACInfo.HasError(ExpectedEmptyAL_ACErrorMessage)));
				Assert("AL_AG should not have the error: " + ExpectedEmptyAL_AGErrorMessage, !(AcceptAL_AG && transactionLine.AL_AGInfo.HasError(ExpectedEmptyAL_AGErrorMessage)));

				transactionLine.AL_AC = ZGuid.Empty;
				transactionLine.AL_AG = ZGuid.Empty;
				Assert("AL_AC should have the error: " + ExpectedEmptyAL_ACErrorMessage, !AcceptAL_AC || transactionLine.AL_ACInfo.HasError(ExpectedEmptyAL_ACErrorMessage));
				Assert("AL_AG should have the error: " + ExpectedEmptyAL_AGErrorMessage, !AcceptAL_AG || transactionLine.AL_AGInfo.HasError(ExpectedEmptyAL_AGErrorMessage));
			}

			if (AcceptAL_AG)
			{
				transactionLine.AL_AG = TestObjectCreator.GLHeader1.PK;
				Assert("AL_AC should be empty", transactionLine.AL_AC.IsEmpty);
				Assert("AL_AC should not have the error: " + ExpectedEmptyAL_ACErrorMessage, !(AcceptAL_AC && transactionLine.AL_ACInfo.HasError(ExpectedEmptyAL_ACErrorMessage)));
				Assert("AL_AG should not have the error: " + ExpectedEmptyAL_AGErrorMessage, !(AcceptAL_AG && transactionLine.AL_AGInfo.HasError(ExpectedEmptyAL_AGErrorMessage)));
			}
		}

		public void TestUpdateAL_ReverseDateShouldNotValidateAL_ReverseDate()
		{
			InvoicingLineBase jobInvoiceLine = Line as InvoicingLineBase;
			if (jobInvoiceLine == null)
			{
				Assert("UpdateAL_ReverseDate this logic is applicable only for AR/AP Invoices and Credit Notes", true);
				return;
			}

			jobInvoiceLine.AL_PostDate = new ZDateTime(1899, 1, 1);
			jobInvoiceLine.UpdateAL_ReverseDate();
			AssertNoErrors("AL_ReverseDate shouldn't be validated.", jobInvoiceLine.AL_ReverseDateInfo);
		}

		public virtual void TestUpdateAL_ReverseDate()
		{
			InvoicingLineBase jobInvoiceLine = Line as InvoicingLineBase;
			if (jobInvoiceLine == null)
			{
				Assert("UpdateAL_ReverseDate this logic is applicable only for AR/AP Invoices and Credit Notes", true);
				return;
			}

			ZDateTime job1Date = new ZDateTime(2005, 2, 10);
			ZDateTime job2Date = new ZDateTime(2005, 5, 10);
			ZDateTime postDate = new ZDateTime(2004, 1, 10);

			Job testJob1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			TestObjectCreator.CreateJobChargeRevRecognition(testJob1, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, job1Date);

			Job testJob2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			TestObjectCreator.CreateJobChargeRevRecognition(testJob2, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, job2Date);

			Job testJob3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			TestObjectCreator.CreateJobChargeRevRecognition(testJob3, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, AccountingConstants.RevenueRecognitionDateConstants.Immediate);

			Job testJob4 = Factory.NewJobWithValidTestDataForTesting<Job>();
			TestObjectCreator.CreateJobChargeRevRecognition(testJob4, RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob, AccountingConstants.RevenueRecognitionDateConstants.JobClosure);

			jobInvoiceLine.AL_PostDate = postDate;
			jobInvoiceLine.AL_AC = TestObjectCreator.CC1.PK;

			jobInvoiceLine.AL_JH = testJob1.PK;
			jobInvoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			jobInvoiceLine.UpdateAL_ReverseDate();
			AssertEquals("AL_ReverseDate must be equal Empty when no periods found.", ZDateTime.Empty, jobInvoiceLine.AL_ReverseDate);

			PeriodManager manager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2005, 01, 01));

			Period periodWithClosedSubLeger = manager.Periods[1]; // Job 1 Date's Period
			periodWithClosedSubLeger.AM_IsSubLedgerClosed = true;

			Period nextOpenPeriod = manager.Periods[2]; // Job 1 Date's Period
			nextOpenPeriod.AM_IsSubLedgerClosed = false;

			Period periodWithOpenedSubLeger = manager.Periods[4]; // Job 2 Date's Period
			periodWithOpenedSubLeger.AM_IsSubLedgerClosed = false;

			bool isRevLine = jobInvoiceLine.LineType_ForTestOnly == TransactionLineTypes.Revenue;
			var jobCharge = testJob1.Charges.AddNew();
			if (isRevLine)
			{
				jobCharge.JR_AL_ARLine = jobInvoiceLine.PK;
			}
			else
			{
				jobCharge.JR_AL_APLine = jobInvoiceLine.PK;
			}
			jobInvoiceLine.UpdateAL_ReverseDate();
			AssertEquals("AL_ReverseDate must be equal Today's for closed period.", ZDateTime.Today, jobInvoiceLine.AL_ReverseDate.Date);
			AssertNotNull(jobInvoiceLine.RelatedJobCharge);
			if (isRevLine)
			{
				AssertNull(jobInvoiceLine.RelatedJobCharge.WIP);
			}
			else
			{
				AssertNull(jobInvoiceLine.RelatedJobCharge.Accrual);
			}
			AssertEquals("No previous values for relater JR_AL_ARLine", 0, jobInvoiceLine.RelatedJobCharge.GetARLineValueHistory().Length);

			Func<BaseWIPAccrual> createNewWipAccrial = () => isRevLine ? Factory.NewWithValidTestData<WIP>() : Factory.NewWithValidTestData<Accrual>();
			var wipAccrual = createNewWipAccrial();
			Factory.NewWithValidTestData<WIP>().AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;

			Action attachToChargeAndReverseWipAccrual = () =>
			{
				if (isRevLine)
				{
					var charge = jobInvoiceLine.RelatedJobCharge;
					charge.ClearRevenueLink();
					charge.JR_AL_ARLine = wipAccrual.PK;
					charge.ReverseWIP(ZDateTime.Now);
					charge.JR_AL_ARLine = jobInvoiceLine.PK;
				}
				else
				{
					var charge = jobInvoiceLine.RelatedJobCharge;
					charge.ClearCostLink();
					charge.JR_AL_APLine = wipAccrual.PK;
					charge.ReverseAccrual(ZDateTime.Now);
					charge.JR_AL_APLine = jobInvoiceLine.PK;
				}
			};
			bool originalRegistryValue = AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.Value;
			AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			try
			{
				attachToChargeAndReverseWipAccrual();

				jobInvoiceLine.AL_ReverseDate = ZDateTime.Empty;
				jobInvoiceLine.UpdateAL_ReverseDate();
				AssertEquals("Must equal the end date of period 2", manager.Periods[2].AM_EndDate, jobInvoiceLine.AL_ReverseDate);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalRegistryValue);
			}

			jobInvoiceLine.AL_JH = testJob2.PK;
			jobInvoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			wipAccrual = createNewWipAccrial();
			wipAccrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			attachToChargeAndReverseWipAccrual();
			jobInvoiceLine.UpdateAL_ReverseDate();
			AssertEquals("AL_ReverseDate must be equal Revenue Recognition Date", job2Date, jobInvoiceLine.AL_ReverseDate);
			AssertEquals("WIP AL_ReverseDate must be equal Invoice Line AL_ReverseDate", jobInvoiceLine.AL_ReverseDate, wipAccrual.AL_ReverseDate);

			jobInvoiceLine.AL_JH = testJob3.PK;
			jobInvoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			jobInvoiceLine.UpdateAL_ReverseDate();
			AssertEquals("AL_ReverseDate must be equal AL_PostDate", postDate, jobInvoiceLine.AL_ReverseDate);
			AssertEquals("AL_ReverseDate must be equal AL_PostDate", postDate, wipAccrual.AL_ReverseDate);

			jobInvoiceLine.AL_JH = testJob4.PK;
			jobInvoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			wipAccrual = createNewWipAccrial();
			wipAccrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			attachToChargeAndReverseWipAccrual();
			jobInvoiceLine.UpdateAL_ReverseDate();
			AssertEquals("AL_ReverseDate must be empty", ZDateTime.Empty, jobInvoiceLine.AL_ReverseDate);
			AssertEquals("WIP AL_ReverseDate should not be empty because it cannot be un-reversed", false, wipAccrual.AL_ReverseDate.IsEmpty);

			jobInvoiceLine.AL_JH = Guid.Empty;
			jobInvoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			jobInvoiceLine.UpdateAL_ReverseDate();
			AssertEquals("AL_ReverseDate must be equal AL_PostDate", postDate, jobInvoiceLine.AL_ReverseDate);
			AssertEquals("WIP AL_ReverseDate must be equal Invoice Line AL_ReverseDate", jobInvoiceLine.AL_ReverseDate, wipAccrual.AL_ReverseDate);

			jobInvoiceLine.AL_JH = testJob2.PK;
			jobInvoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			wipAccrual = createNewWipAccrial();
			wipAccrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			attachToChargeAndReverseWipAccrual();
			using (DisposableEnvironment.ForBranch(TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid()))
			{
				jobInvoiceLine.UpdateAL_ReverseDate();
			}
			AssertEquals("UpdateAL_ReverseDate should still set AL_ReverseDate to Revenue Recognition Date when used with different company", job2Date, jobInvoiceLine.AL_ReverseDate);
			AssertEquals("WIP AL_ReverseDate must equal Invoice Line AL_ReverseDate", jobInvoiceLine.AL_ReverseDate, wipAccrual.AL_ReverseDate);
		}

		[TestDate(2005, 03, 01)]
		public virtual void TestUpdateAL_ReverseDate_WithCurrentDateRegistry()
		{
			InvoicingLineBase invoiceLine = Line as InvoicingLineBase;
			if (invoiceLine == null)
			{
				Assert("UpdateAL_ReverseDate this logic is applicable only for AR/AP Invoices and Credit Notes", true);
				return;
			}

			TestObjectCreator.CreateTestPeriods(new ZDateTime(2005, 01, 01));
			Factory.Save();

			AssertPeriodNotFound();

			AssertPeriodFoundButClosed();

			AssertPeriodFoundAndOpening();

			AssertRecogTypeIMM();

			AssertRecogTypeJOB();

			void AssertPeriodNotFound()
			{
				var recognitionDate = new ZDateTime(2015, 01, 01);
				AssertNull(new AccountingPeriodCalculator(Factory).GetPeriodManagementFromDate(recognitionDate));

				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, recognitionDate);

				var jobInvoiceLine = GetTestLine(job);
				jobInvoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
				jobInvoiceLine.UpdateAL_ReverseDate();
				AssertEquals("AL_ReverseDate must be equal Empty when no periods found.", ZDateTime.Empty, jobInvoiceLine.AL_ReverseDate);
			}

			void AssertPeriodFoundButClosed()
			{
				var recognitionDate = new ZDateTime(2005, 1, 10);
				var periodClosed = new AccountingPeriodCalculator(Factory).GetPeriodManagementFromDate(recognitionDate);
				periodClosed.AM_IsSubLedgerClosed = true;

				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, recognitionDate);

				var jobInvoiceLine = GetTestLine(job);
				jobInvoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
				jobInvoiceLine.UpdateAL_ReverseDate();
				AssertEquals("AL_ReverseDate must be equal Today's for closed period.", ZDateTime.Today, jobInvoiceLine.AL_ReverseDate.Date);

				using (AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var nextPeriod = new AccountingPeriodCalculator(Factory).GetPeriodManagementFromDate(periodClosed.AM_EndDate.AddDays(1));
					nextPeriod.AM_IsSubLedgerClosed = false; // Job 1 Date's Period

					var wipAccrual = AttachToChargeAndReverseWipAccrual(jobInvoiceLine);

					jobInvoiceLine.AL_ReverseDate = ZDateTime.Empty;
					jobInvoiceLine.UpdateAL_ReverseDate();
					AssertEquals("Must equal the end date of period 200502", nextPeriod.AM_EndDate, jobInvoiceLine.AL_ReverseDate);
					AssertEquals("WIP AL_ReverseDate must be equal Invoice Line AL_ReverseDate", jobInvoiceLine.AL_ReverseDate, wipAccrual.AL_ReverseDate);
				}
			}

			void AssertPeriodFoundAndOpening()
			{
				using (AccountingConfigurationRegistry.Instance.RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					AssertPeriodFoundAndOpening_Inner(new ZDateTime(2005, 05, 10), new ZDateTime(2005, 05, 10));
					AssertPeriodFoundAndOpening_Inner(new ZDateTime(2005, 01, 10), new ZDateTime(2005, 01, 10));
				}

				using (AccountingConfigurationRegistry.Instance.RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertPeriodFoundAndOpening_Inner(new ZDateTime(2005, 05, 10), new ZDateTime(2005, 05, 10));
					AssertPeriodFoundAndOpening_Inner(new ZDateTime(2005, 01, 10), new ZDateTime(2005, 01, 10));
				}
			}

			void AssertPeriodFoundAndOpening_Inner(ZDateTime recognitionDate, ZDateTime expectedRecognitionDate)
			{
				var period = new AccountingPeriodCalculator(Factory).GetPeriodManagementFromDate(recognitionDate);
				period.AM_IsSubLedgerClosed = false;

				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate, recognitionDate);

				var jobInvoiceLine = GetTestLine(job);
				jobInvoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
				var wipAccrual = AttachToChargeAndReverseWipAccrual(jobInvoiceLine);
				jobInvoiceLine.UpdateAL_ReverseDate();
				AssertEquals("Recognition Date should always equal to first recognition date in same recognition type", expectedRecognitionDate.Date, jobInvoiceLine.AL_ReverseDate.Date);
				AssertEquals("WIP AL_ReverseDate must to equal Invoice Line AL_ReverseDate", jobInvoiceLine.AL_ReverseDate, wipAccrual.AL_ReverseDate);
			}

			void AssertRecogTypeIMM()
			{
				var postDate = new ZDateTime(2005, 3, 10);
				var period = new AccountingPeriodCalculator(Factory).GetPeriodManagementFromDate(postDate);
				period.AM_IsSubLedgerClosed = false;

				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate, AccountingConstants.RevenueRecognitionDateConstants.Immediate);

				var jobInvoiceLine = GetTestLine(job);
				var wipAccrual = AttachToChargeAndReverseWipAccrual(jobInvoiceLine);
				jobInvoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

				jobInvoiceLine.AL_PostDate = postDate;
				jobInvoiceLine.UpdateAL_ReverseDate();
				AssertEquals(postDate, jobInvoiceLine.AL_PostDate);
				AssertEquals("AL_ReverseDate must be equal AL_PostDate", postDate, jobInvoiceLine.AL_ReverseDate);
				AssertEquals("AL_ReverseDate must be equal AL_PostDate", postDate, wipAccrual.AL_ReverseDate);
			}

			void AssertRecogTypeJOB()
			{
				var job = Factory.NewJobWithValidTestDataForTesting<Job>();
				TestObjectCreator.CreateJobChargeRevRecognition(job, RevenueRecognitionLookups.RecognitionDateOptionCodes.OldJob, AccountingConstants.RevenueRecognitionDateConstants.JobClosure);

				var jobInvoiceLine = GetTestLine(job);
				jobInvoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;

				var wipAccrual = AttachToChargeAndReverseWipAccrual(jobInvoiceLine);
				wipAccrual.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;

				jobInvoiceLine.UpdateAL_ReverseDate();
				AssertEquals("AL_ReverseDate must be empty", ZDateTime.Empty, jobInvoiceLine.AL_ReverseDate);
				AssertEquals("WIP AL_ReverseDate should not be empty because it cannot be un-reversed", false, wipAccrual.AL_ReverseDate.IsEmpty);

				var postDate = new ZDateTime(2005, 3, 10);
				var period = new AccountingPeriodCalculator(Factory).GetPeriodManagementFromDate(postDate);
				period.AM_IsSubLedgerClosed = false;

				jobInvoiceLine.AL_JH = Guid.Empty;
				jobInvoiceLine.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
				jobInvoiceLine.AL_PostDate = postDate;
				jobInvoiceLine.UpdateAL_ReverseDate();
				AssertEquals(postDate, jobInvoiceLine.AL_PostDate);
				AssertEquals("AL_ReverseDate must be equal AL_PostDate", postDate, jobInvoiceLine.AL_ReverseDate);
				AssertEquals("WIP AL_ReverseDate must be equal Invoice Line AL_ReverseDate", jobInvoiceLine.AL_ReverseDate, wipAccrual.AL_ReverseDate);
			}

			TransactionLine GetTestLine(Job job)
			{
				var jobInvoiceLine = CreateNewLine();
				jobInvoiceLine.AL_PostDate = new ZDateTime(2005, 1, 10);
				jobInvoiceLine.AL_AC = TestObjectCreator.CC1.PK;
				jobInvoiceLine.AL_JH = job.PK;

				var jobCharge = job.Charges.AddNew();
				if (jobInvoiceLine.LineType_ForTestOnly == TransactionLineTypes.Revenue)
				{
					jobCharge.JR_AL_ARLine = jobInvoiceLine.PK;
					AssertNull(jobInvoiceLine.RelatedJobCharge.WIP);
				}
				else
				{
					jobCharge.JR_AL_APLine = jobInvoiceLine.PK;
					AssertNull(jobInvoiceLine.RelatedJobCharge.Accrual);
				}
				AssertNotNull(jobInvoiceLine.RelatedJobCharge);
				AssertEquals("No previous values for relater JR_AL_ARLine", 0, jobInvoiceLine.RelatedJobCharge.GetARLineValueHistory().Length);

				return jobInvoiceLine;
			}

			BaseWIPAccrual AttachToChargeAndReverseWipAccrual(TransactionLine line)
			{
				BaseWIPAccrual wipAccrual;

				var charge = line.RelatedJobCharge;
				if (line.LineType_ForTestOnly == TransactionLineTypes.Revenue)
				{
					charge.ClearRevenueLink();

					wipAccrual = Factory.NewWithValidTestData<WIP>();
					charge.JR_AL_ARLine = wipAccrual.PK;
					charge.ReverseWIP(ZDateTime.Now);
					charge.JR_AL_ARLine = line.PK;
				}
				else
				{
					charge.ClearCostLink();

					wipAccrual = Factory.NewWithValidTestData<Accrual>();
					charge.JR_AL_APLine = wipAccrual.PK;
					charge.ReverseAccrual(ZDateTime.Now);
					charge.JR_AL_APLine = line.PK;
				}

				return wipAccrual;
			}
		}

		public void TestAL_ReverseDateSetWhenPosted()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			testJob.JH_JobNum = "J0000456";

			Charge charge = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, "Charge 1", TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 1m, TestObjectCreator.Agent);
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_ChequeNo = "10001";

			InvoicingPostManager postManager = new InvoicingPostManager(testJob);
			InvoicingBase[] invoicelist = postManager.CreateTransactions(JobInvoicingPostingOption.All).GetAllAPInvoicesAndCreditNotes();
			Factory.Save();

			AssertEquals("AL_ReverseDate must be equal AL_PostDate", ZDateTime.Today, invoicelist[0].Lines[0].AL_ReverseDate.Date);
		}

		public virtual void TestAL_JHValidationOnAL_RevRecognitionType()
		{
			var shipment = TestObjectCreator.CreateShipment("SHP001");
			var testJob = TestObjectCreator.CreateJob(shipment, false);

			Line.AL_JH = testJob.PK;
			SetChargeCode(Line, TestObjectCreator.CC1.PK);
			AssertNoErrors(Line.AL_JHInfo);

			Line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			AssertHasErrors(Line.AL_JHInfo);
		}

		public void TestAL_RevRecognitionType()
		{
			AssertEquals("Line.AL_RevRecognitionType default value", "IMM", Line.AL_RevRecognitionType);

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			AccountingConfigurationRegistry.Instance.RevenueRecognitionByChargeGroupSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new RevenueRecognitionByChargeGroupCollection());

			Line.AL_AC = ZGuid.Empty;
			Line.AL_JH = ZGuid.Empty;
			AssertEquals("Line.AL_RevRecognitionType", "IMM", Line.AL_RevRecognitionType);

			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
				Line.AL_JH = job.PK;
				AssertEquals("Line.AL_RevRecognitionType", "IMM", Line.AL_RevRecognitionType);

				Line.AL_AC = TestObjectCreator.CC1.PK;
				AssertEquals("Line.AL_RevRecognitionType", RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate, Line.AL_RevRecognitionType);

				Line.AL_JH = ZGuid.Empty;
				AssertEquals("Line.AL_RevRecognitionType", "IMM", Line.AL_RevRecognitionType);

				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new RevenueRecognitionCollection());
				job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).ClearServiceCache();

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Line.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeSetFromValidToEmpty);
				Line.AL_JH = job.PK;
				var info = CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(Line.PK, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeSetFromValidToEmpty);
				var expectedJobRevRecognitionDetails = @"RevenueRecognitionTypeSetFromValidToEmpty:
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)";
				AssertContains(expectedJobRevRecognitionDetails, info);
			}
		}

		public void TestJobType()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();

			APInvoice invoice = Factory.New<APInvoice>();
			InvoicingLineBase jobInvoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			jobInvoiceLine.AL_JH = job.PK;
			Assert("Job should have Job type", jobInvoiceLine.Job is Job);
		}

		[ExpectNoExceptions]
		public void TestJobDoesNotThrowShouldNotBeAccessingPropertyOnDeletedBizO()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();

			APInvoice invoice = Factory.New<APInvoice>();
			InvoicingLineBase jobInvoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			jobInvoiceLine.AL_JH = job.PK;
			Assert("Job should have Job type", jobInvoiceLine.Job is Job);

			jobInvoiceLine.Delete();

			JobHeader jobFromDeletedLine = jobInvoiceLine.Job;
		}

		public void TestRelatedJobCharge()
		{
			if ((string.IsNullOrEmpty(ARLineType) && string.IsNullOrEmpty(APLineType)) ||
				!(Line.AL_LineType == ARLineType || Line.AL_LineType == APLineType))
			{
				AssertNull("RelatedJobCharge is not applicable for this line type: " + Line.GetType().ToString(), Line.RelatedJobCharge);
			}
			else
			{
				JobCharge charge;
				if (Line.RelatedJobCharge != null)
				{
					charge = Factory.Load<JobCharge>(Line.RelatedJobCharge.PK);
				}
				else
				{
					charge = Factory.NewWithValidTestData<JobCharge>();

					Line.AL_AC = charge.JR_AC;
					Line.AL_JH = charge.JR_JH;
				}

				JobCharge otherCharge = Factory.NewWithValidTestData<JobCharge>();
				otherCharge.JR_JH = charge.JR_JH;

				if (Line.AL_LineType == ARLineType)
				{
					if (Line.RelatedJobCharge != null)
					{
						Line.RelatedJobCharge.ReverseWIP(ZDateTime.Now);
					}
					else
					{
						Line.AL_ReverseDate = ZDateTime.Now;
						charge.JR_AL_ARLine = ZGuid.Empty;
					}

					Line.AL_ReverseDate = ZDateTime.Empty;
					otherCharge.JR_AL_ARLine = Line.PK;

					otherCharge.SetChargeValuesFromLinkedARLineForTests();
				}
				else if (Line.AL_LineType == APLineType)
				{
					if (Line.RelatedJobCharge != null)
					{
						Line.RelatedJobCharge.ReverseAccrual(ZDateTime.Now);
					}
					else
					{
						Line.AL_ReverseDate = ZDateTime.Now;
						charge.JR_AL_APLine = ZGuid.Empty;
					}
					Line.AL_ReverseDate = ZDateTime.Empty;
					otherCharge.JR_AL_APLine = Line.PK;

					otherCharge.SetChargeValuesFromLinkedAPLineForTests();
				}
				Factory.Save();

				AssertEquals("RelatedJobCharge must be correct even if the charge linked Lines is cleared in other object type.", otherCharge.PK, Line.RelatedJobCharge.PK);

				if (Line.AL_LineType == ARLineType)
				{
					otherCharge.SetARLineForcedForTest(ZGuid.NewZGuid());
				}
				else if (Line.AL_LineType == APLineType)
				{
					otherCharge.SetAPLineForcedForTest(ZGuid.NewZGuid());
				}

				AssertNull("Should be null because it was changed on Charge", Line.RelatedJobCharge);

				if (Line.AL_LineType == ARLineType)
				{
					charge.JR_AL_ARLine = Line.PK;
				}
				else if (Line.AL_LineType == APLineType)
				{
					charge.JR_AL_APLine = Line.PK;
				}

				AssertEquals("RelatedJobCharge must be correct", charge.PK, Line.RelatedJobCharge.PK);

				charge.Delete();
				AssertNull("Should be null because Charge was deleted", Line.RelatedJobCharge);
			}
		}

		public virtual void TestRelatedJobChargeWithoutDbHit()
		{
			if ((string.IsNullOrEmpty(ARLineType) && string.IsNullOrEmpty(APLineType)) ||
				!(Line.AL_LineType == ARLineType || Line.AL_LineType == APLineType))
			{
				AssertNull("RelatedJobCharge is not applicable for this line type: " + Line.GetType().ToString(), Line.RelatedJobCharge);
			}
			else
			{
				AssertDbHitCount(0);

				Factory.Save();

				AssertDbHitCount(1);
			}
		}

		protected void AssertDbHitCount(int count)
		{
			Factory.ResetDatabaseLoadCount();
			_ = Line.RelatedJobCharge;
			AssertEquals($"there should be {count} DB hit", count, Factory.DatabaseLoadCount);
		}

		protected virtual string ARLineType { get { return null; } }
		protected virtual string APLineType { get { return null; } }

		protected virtual Type TypeOfEmptyValidation
		{
			get { return typeof(TransactionLineEmptyValidation); }
		}

		public virtual void TestSettingAL_ACSetsAL_AG()
		{
			TransactionLine transactionLine = (TransactionLine)GetNewBusinessObject();
			transactionLine.AL_LineType = TransactionLineTypes.Accrual;
			TestObjectCreator.CC1.AC_AG_AccrualAccount = TestObjectCreator.GLHeader1.PK;
			transactionLine.AL_AG = ZGuid.Empty;

			transactionLine.AL_AC = TestObjectCreator.CC1.PK;
			AssertEquals("AL_AG is set", TestObjectCreator.GLHeader1.PK, transactionLine.AL_AG);

			transactionLine.AL_LineType = TransactionLineTypes.Cost;
			TestObjectCreator.CC2.AC_AG_CostAccount = TestObjectCreator.GLHeader2.PK;
			transactionLine.AL_AG = ZGuid.Empty;

			transactionLine.AL_AC = TestObjectCreator.CC2.PK;
			AssertEquals("AL_AG is set", TestObjectCreator.GLHeader2.PK, transactionLine.AL_AG);

			transactionLine.AL_LineType = TransactionLineTypes.UnapprovedCost;
			TestObjectCreator.CC1.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
			transactionLine.AL_AG = ZGuid.Empty;

			transactionLine.AL_AC = TestObjectCreator.CC1.PK;
			AssertEquals("AL_AG is set", TestObjectCreator.GLHeader1.PK, transactionLine.AL_AG);

			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			TestObjectCreator.CC2.AC_AG_RevenueAccount = TestObjectCreator.GLHeader2.PK;
			transactionLine.AL_AG = ZGuid.Empty;

			transactionLine.AL_AC = TestObjectCreator.CC2.PK;
			AssertEquals("AL_AG is set", TestObjectCreator.GLHeader2.PK, transactionLine.AL_AG);

			transactionLine.AL_LineType = TransactionLineTypes.WIP;
			TestObjectCreator.CC1.AC_AG_WIPAccount = TestObjectCreator.GLHeader1.PK;
			transactionLine.AL_AG = ZGuid.Empty;

			transactionLine.AL_AC = TestObjectCreator.CC1.PK;
			AssertEquals("AL_AG is set", TestObjectCreator.GLHeader1.PK, transactionLine.AL_AG);
		}

		JobConsolCost CreateConsolCost(TestObjectCreator testObjectCreator, ApportionmentListing apportionments, ZGuid chargeCode, ZGuid taxRate, ZDecimal oSCostAmount, ZDecimal exchangeRate, ZString apportionmentMethod)
		{
			JobConsolCost consolCost = apportionments.CostsCollection.TryAddNew();
			if (consolCost != null)
			{
				consolCost.E6_OH_Creditor = testObjectCreator.AALSHI.PK;
				consolCost.E6_AC_ChargeCode = chargeCode;
				consolCost.E6_AT_TaxRate = taxRate;
				consolCost.E6_OSCostAmount = oSCostAmount;
				consolCost.E6_RX_NKCurrency = "USD";
				consolCost.E6_ExchangeRate = exchangeRate;
				consolCost.E6_ApportionmentMethod = apportionmentMethod;
			}
			return consolCost;
		}

		Charge CreateJobCharge(ForwardingShipment shipment, ZGuid headerPK, ZGuid chargeCode, ZGuid taxRate, ZString currencyCode, ZDecimal oSCostAmount, ZDecimal exchangeRate)
		{
			Charge jobCharge = Factory.New<Charge>();

			jobCharge.JR_RX_NKCostCurrency = currencyCode;
			jobCharge.JR_OH_CostAccount = headerPK;
			jobCharge.JR_AC = chargeCode;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge.JR_RX_NKCostCurrency = currencyCode;
			jobCharge.JR_OSCostExRate = exchangeRate;
			jobCharge.JR_AT_CostGSTRate = taxRate;
			jobCharge.JR_OSCostAmt = oSCostAmount;
			if (shipment.ShipmentJobHeader == null)
			{
				shipment.CreateShipmentJobHeaderWithMutex();
			}

			jobCharge.JR_JH = shipment.ShipmentJobHeader.PK;

			return jobCharge;
		}

		public void TestAL_LocalTaxAmount()
		{
			ForwardingConsol consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingConsol consol3 = Factory.NewWithValidTestData<ForwardingConsol>();
			Factory.Save();

			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			ForwardingShipment shipment3 = consol3.Shipments.AddNew();

			try
			{
				ApportionmentListing apportionments1 = new ApportionmentListing(Factory, consol1);
				ApportionmentListing apportionments2 = new ApportionmentListing(Factory, consol2);
				ApportionmentListing apportionments3 = new ApportionmentListing(Factory, consol3);

				TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

				JobConsolCost consolCost1 = CreateConsolCost(testObjectCreator, apportionments1, testObjectCreator.CC1.PK, testObjectCreator.GSTFREE1.PK, 780.00m, 0.4000m, "SHP");
				JobConsolCost consolCost2 = CreateConsolCost(testObjectCreator, apportionments1, testObjectCreator.CC2.PK, testObjectCreator.GST1.PK, 433.00m, 0.4000m, "SHP");
				JobConsolCost consolCost3 = CreateConsolCost(testObjectCreator, apportionments2, testObjectCreator.CC1.PK, testObjectCreator.GSTFREE1.PK, 780.00m, 0.5000m, "SHP");
				JobConsolCost consolCost4 = CreateConsolCost(testObjectCreator, apportionments2, testObjectCreator.CC2.PK, testObjectCreator.GST1.PK, 433.00m, 0.5000m, "SHP");
				Charge jobCharge1 = CreateJobCharge(shipment3, testObjectCreator.AALSHI.PK, testObjectCreator.CC1.PK, testObjectCreator.GSTFREE1.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 700.00m, 1m);
				Charge jobCharge2 = CreateJobCharge(shipment3, testObjectCreator.AALSHI.PK, testObjectCreator.CC2.PK, testObjectCreator.GST1.PK, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, 433.00m, 1m);

				APInvoice invoice = Factory.New<APInvoice>();
				invoice.AH_OH = testObjectCreator.AALSHI.PK;
				invoice.ExchangeRate.Currency = testObjectCreator.USD.RX_Code;
				invoice.ExchangeRate.Rate = 0.5000m;
				invoice.AH_PostedToEFT = true;

				APInvoice invoice2 = Factory.New<APInvoice>();
				invoice2.AH_OH = testObjectCreator.AALSHI.PK;
				invoice2.ExchangeRate.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				invoice2.ExchangeRate.Rate = 1m;
				invoice2.AH_PostedToEFT = true;

				invoice.ConsolCosting.ConsolCosts.Add(consolCost1);
				invoice.ConsolCosting.ConsolCosts.Add(consolCost2);
				invoice.ConsolCosting.ConsolCosts.Add(consolCost3);
				invoice.ConsolCosting.ConsolCosts.Add(consolCost4);
				invoice.ImportAllApportionmentsFromCosting();

				invoice2.ImportJobChargesIntoInvoice(new[] { jobCharge1, jobCharge2 }, (InvoicingLineBase)invoice2.Lines.AddNew());

				AssertEquals("Invoice Lines Count", 4, invoice.Lines.Count);
				AssertEquals("Invoice2 Lines Count", 2, invoice2.Lines.Count);

				InvoicingLineBase line1 = invoice.Lines[0];
				InvoicingLineBase line2 = invoice.Lines[1];
				InvoicingLineBase line3 = invoice.Lines[2];
				InvoicingLineBase line4 = invoice.Lines[3];
				InvoicingLineBase line21 = invoice2.Lines[0];
				InvoicingLineBase line22 = invoice2.Lines[1];

				AssertEquals("Local Amount 1", 1950.00m, line1.AL_LocalExTaxAmount);
				AssertEquals("Local Amount 2", 1082.50m, line2.AL_LocalExTaxAmount);
				AssertEquals("Local Amount 3", 1560.00m, line3.AL_LocalExTaxAmount);
				AssertEquals("Local Amount 4", 866.00m, line4.AL_LocalExTaxAmount);
				AssertEquals("Local Amount 21", 700m, line21.AL_LocalExTaxAmount);
				AssertEquals("Local Amount 22", 433m, line22.AL_LocalExTaxAmount);

				AssertEquals("Local Tax Amount 1", 0.00m, line1.AL_LocalTaxAmount);
				AssertEquals("Local Tax Amount 2", 108.25m, line2.AL_LocalTaxAmount);
				AssertEquals("Local Tax Amount 3", 0.00m, line3.AL_LocalTaxAmount);
				AssertEquals("Local Tax Amount 4", 86.60m, line4.AL_LocalTaxAmount);
				AssertEquals("Local Tax Amount 21", 0m, line21.AL_LocalTaxAmount);
				AssertEquals("Local Tax Amount 22", 43.3m, line22.AL_LocalTaxAmount);

				// Changes in AL_LocalExTaxAmount should be reflected in AL_LocalTaxAmount
				line21.AL_LocalExTaxAmount = 1200m;
				line22.AL_LocalExTaxAmount = 900m;
				AssertEquals("Local Tax Amount 21 should be 0", 0m, line21.AL_LocalTaxAmount);
				AssertEquals("Local Tax Amount 22 should be updated", 90m, line22.AL_LocalTaxAmount);
			}
			finally
			{
				shipment1.Job.Dispose();
				shipment2.Job.Dispose();
				shipment3.Job.Dispose();
			}
		}

		public virtual void TestGetNewValidation()
		{
			Assert("Validation should not be TransactionLineEmptyValidation", !typeof(TransactionLineEmptyValidation).IsAssignableFrom(Line.Validation.GetType()));
			Line.AL_AC = ZGuid.Empty;
			Line.AL_JH = ZGuid.Empty;
			Line.AL_ReverseDate = ZDateTime.Now;

			Factory.Save();
			Assert("Validation should be TransactionLineEmptyValidation", typeof(TransactionLineEmptyValidation).IsAssignableFrom(Line.Validation.GetType()));
		}

		public void TestPostingGroupID()
		{
			Line.AL_AT = ZGuid.Empty;
			AssertEquals(null, Line.TaxRate);
			AssertEquals(Line.PostingGroupID, AccTaxRate.DefaultPostingGroupID);

			var taxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST"));
			taxRate.AT_PostingGroupId = 1;
			Line.AL_AT = taxRate.PK;
			AssertEquals(taxRate.PK, Line.TaxRate.PK);
			AssertEquals(Line.PostingGroupID, (ZShort)1);
		}

		public void TestValidationTypeWhenValidationSuspended()
		{
			var transaction = CreateNewLine();
			using (transaction.GetValidationSuspender())
			{
				var validation = transaction.Validation;
				Assert("Expect empty validation as just some not null value as actual validation calls will be skipped anyway.", TypeOfEmptyValidation == validation.GetType());
			}
		}

		public void TestGLHeaderCollectionForMultipleTransactionLines()
		{
			var transactionLine2 = CreateNewLine();
			transactionLine2.AL_AG = TestObjectCreator.GLHeader2.PK;

			AssertEquals(Line.GLHeaderCollection.TransactionLines_ForTestOnly.PK, Line.PK);
			AssertEquals(transactionLine2.GLHeaderCollection.TransactionLines_ForTestOnly.PK, transactionLine2.PK);
		}

		#region Implementation

		protected TransactionLine Line;

		protected AccChargeCode CurrentDepartmentChargeCode;
		protected AccChargeCode AllDepartmentsChargeCode;
		protected AccChargeCode NonCurrentDepartmentChargeCode;

		protected GlbDepartment CurrentDepartment;
		protected GlbDepartment NonCurrentDepartment;

		protected virtual bool AcceptAL_AC
		{
			get { return true; }
		}

		protected virtual bool AcceptAL_AG
		{
			get { return true; }
		}

		protected virtual string ExpectedEmptyAL_ACErrorMessage
		{
			get { return TransactionLine.EmptyChargeCodeAndGLHeaderError; }
		}

		protected virtual string ExpectedEmptyAL_AGErrorMessage
		{
			get { return TransactionLine.EmptyChargeCodeAndGLHeaderError; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetupPeriods();
			Line = CreateNewLine();
			Line.AL_AG = TestObjectCreator.GLHeader1.PK;
			SetupDepartments();
			SetupChargeCodes();
		}

		protected virtual TransactionLine CreateNewLine()
		{
			return (TransactionLine)Factory.New(GetExpectedBusinessObjectType());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Line;
		}

		protected AccountingPeriodTestHelper PeriodTestHelper;
		protected AccPeriodManagement PreviousGLClosedPeriod;
		protected AccPeriodManagement PreviousSubLedgerClosedPeriod;
		protected AccPeriodManagement PreviousOpenPeriod;
		protected AccPeriodManagement CurrentPeriod;
		protected AccPeriodManagement FuturePeriod;

		protected void SetupPeriods()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);

			PeriodTestHelper = new AccountingPeriodTestHelper(Factory);
			PeriodTestHelper.SetupPeriods();

			PreviousGLClosedPeriod = PeriodTestHelper.PreviousGLClosedPeriod;
			PreviousSubLedgerClosedPeriod = PeriodTestHelper.PreviousSubLedgerClosedPeriod;
			PreviousOpenPeriod = PeriodTestHelper.PreviousOpenPeriod;
			CurrentPeriod = PeriodTestHelper.CurrentPeriod;
			FuturePeriod = PeriodTestHelper.FuturePeriod;
		}

		protected void SetupDepartments()
		{
			GlbDepartmentCollection departmentCollection = new GlbDepartmentCollection(Factory, new ZQuery());
			CurrentDepartment = (GlbDepartment)departmentCollection.FindByPK(GlbDepartment.CurrentDepartment.PK);
			ZQuery filter = new ZQuery(departmentCollection.AdditionalFilter);
			filter.AddToFilter((new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, GlbDepartment.CurrentDepartment.PK)), JoinCondition.Or);
			departmentCollection.AdditionalFilter = filter;
			NonCurrentDepartment = departmentCollection[0];
		}

		protected void SetupChargeCodes()
		{
			AccChargeCodeCollection chargeCodeCollection = new AccChargeCodeCollection(Factory, new ZQuery(Line.ChargeCodeCollection.CompleteFilter));
			chargeCodeCollection.Load();

			Assert(chargeCodeCollection.Count >= 3);

			CurrentDepartmentChargeCode = chargeCodeCollection[0];
			AllDepartmentsChargeCode = chargeCodeCollection[1];
			NonCurrentDepartmentChargeCode = chargeCodeCollection[2];

			CurrentDepartmentChargeCode.AC_DepartmentFilterList = GlbDepartment.CurrentDepartment.GE_Code;
			AllDepartmentsChargeCode.AC_DepartmentFilterList = "ALL";
			NonCurrentDepartmentChargeCode.AC_DepartmentFilterList = NonCurrentDepartment.GE_Code;
		}

		void SetChargeCode(TransactionLine line, ZGuid chargeCodePK)
		{
			if (line is InvoicingLineBase invoiceLine)
			{
				invoiceLine.GenericCharge = chargeCodePK;
			}
			else
			{
				line.AL_AC = chargeCodePK;
			}
		}

		protected void LoadForeignCurrency(int subUnitRatio)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ZQuery filter = new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, "AUD");
			filter.AddToFilter(JoinCondition.And, RefCurrencySchema.RX_SubUnitRatio, SQLComparisonOperator.Equal, subUnitRatio);
			RefCurrency[] foreignCurrencies = (RefCurrency[])newFactory.Load(typeof(RefCurrency), filter);
			if (foreignCurrencies.Length > 0)
			{
				fForeignCurrency = foreignCurrencies[0];
			}
		}

		protected void SetupLineExRatesAndAmounts(ZDecimal exchangeRate, ZInt currencySubUnitRatio, ZDecimal aH_OSExTaxAmount, ZDecimal oSTaxAmount)
		{
			LoadForeignCurrency(currencySubUnitRatio);
			if (Line.TransactionHeader != null)
			{
				Line.TransactionHeader.AH_ExchangeRate = exchangeRate;
				Line.TransactionHeader.AH_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			}
			Line.AL_RX_NKTransactionCurrency = ForeignCurrency.RX_Code;
			Line.AL_ExchangeRate = exchangeRate;
			Line.AL_OSExTaxAmount = aH_OSExTaxAmount;
			Line.AL_OSTaxAmount = oSTaxAmount;
			if (exchangeRate == 1M)
			{
				Line.AL_LocalExTaxAmount = aH_OSExTaxAmount;
				Line.AL_LocalTaxAmount = oSTaxAmount;
			}

			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(Line);
		}

		protected virtual void SetupAndSaveLine()
		{
			Line.AL_GB = GlbBranch.CurrentBranch.PK;
			Line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			Line.AL_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			Line.AL_SystemCreateTimeUtc = ZDateTime.Today;
			Factory.Save();
		}

		protected RefCurrency fForeignCurrency;

		protected RefCurrency ForeignCurrency
		{
			get
			{
				if (fForeignCurrency == null)
				{
					LoadForeignCurrency(100);
				}
				return fForeignCurrency;
			}
		}

		AccChargeCode fCommentChargeCode;
		protected AccChargeCode CommentChargeCode
		{
			get
			{
				if (fCommentChargeCode == null)
				{
					fCommentChargeCode = new TestObjectCreator(Factory).CreateChargeCode("CMT", "Comment", Constants.ChargeType.Comment, 0, null, null, "ALL");
				}

				return fCommentChargeCode;
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		TestObjectCreator fTestObjectCreator;

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;

		#endregion

		public class MockableTransactionLine : TransactionLine
		{
			public MockableTransactionLine(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override ZString LineType
			{
				get { return ZString.Empty; }
			}

			protected override bool InvertSigns
			{
				get { return false; }
			}
		}
	}
}
