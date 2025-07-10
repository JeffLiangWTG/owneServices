using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class JobComInvHeaderChargeTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestJ7_IsIncludedInITOTDefaultWhenJ7_ChargeTypeIsSet()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				TestDeclaration declaration = Factory.New<TestDeclaration>();
				TestInvoice invoice = declaration.Invoices.AddNew();
				invoice.IncoTerm = Constants.IncoTerms.FreeOnBoard;
				TestCharge invoiceCharge = invoice.Charges.AddNew();
				TestInvoiceLine invoiceLine = invoice.InvoiceLines.AddNew();
				TestCharge invoiceLineCharge = invoiceLine.Charges.AddNew();

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
				invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
				NUnit.Framework.Assert.That(invoiceCharge.J7_IsIncludedInITOT, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "J7_IsIncludedInITOT");
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_IsIncludedInITOT, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "J7_IsIncludedInITOT");
				NUnit.Framework.Assert.That(invoiceCharge.J7_IsIncludedInITOTInfo.ReadOnly, Is.EqualTo(false), "J7_IsIncludedInITOT readonly");
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_IsIncludedInITOTInfo.ReadOnly, Is.EqualTo(false), "J7_IsIncludedInITOT readonly");

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
				invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
				NUnit.Framework.Assert.That(invoiceCharge.J7_IsIncludedInITOT, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "J7_IsIncludedInITOT");
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_IsIncludedInITOT, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "J7_IsIncludedInITOT");
				NUnit.Framework.Assert.That(invoiceCharge.J7_IsIncludedInITOTInfo.ReadOnly, Is.EqualTo(false), "J7_IsIncludedInITOT readonly");
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_IsIncludedInITOTInfo.ReadOnly, Is.EqualTo(false), "J7_IsIncludedInITOT readonly");

				invoice.IncoTerm = Constants.IncoTerms.FreeCarrierSeller;

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
				invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
				NUnit.Framework.Assert.That(invoiceCharge.J7_IsIncludedInITOT, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "J7_IsIncludedInITOT");
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_IsIncludedInITOT, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "J7_IsIncludedInITOT");
				NUnit.Framework.Assert.That(invoiceCharge.J7_IsIncludedInITOTInfo.ReadOnly, Is.EqualTo(false), "J7_IsIncludedInITOT readonly");
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_IsIncludedInITOTInfo.ReadOnly, Is.EqualTo(false), "J7_IsIncludedInITOT readonly");

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
				invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
				NUnit.Framework.Assert.That(invoiceCharge.J7_IsIncludedInITOT, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "J7_IsIncludedInITOT");
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_IsIncludedInITOT, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "J7_IsIncludedInITOT");
				NUnit.Framework.Assert.That(invoiceCharge.J7_IsIncludedInITOTInfo.ReadOnly, Is.EqualTo(false), "J7_IsIncludedInITOT readonly");
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_IsIncludedInITOTInfo.ReadOnly, Is.EqualTo(false), "J7_IsIncludedInITOT readonly");
			}
		}

		[ExpectNoExceptions]
		public void TestJ7_Calc_IncludedInInvoiceAmountReadonly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				TestDeclaration declaration = Factory.New<TestDeclaration>();
				TestInvoice invoice = declaration.Invoices.AddNew();
				invoice.IncoTerm = Constants.IncoTerms.FreeOnBoard;
				TestCharge invoiceCharge = invoice.Charges.AddNew();
				TestInvoiceLine invoiceLine = invoice.InvoiceLines.AddNew();
				TestCharge invoiceLineCharge = invoiceLine.Charges.AddNew();

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
				invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
				NUnit.Framework.Assert.That(invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(invoiceCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly, Is.EqualTo(false));
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly, Is.EqualTo(false));

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
				invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
				NUnit.Framework.Assert.That(invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount, Is.EqualTo(false).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount, Is.EqualTo(false).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(invoiceCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly, Is.EqualTo(false));
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly, Is.EqualTo(false));

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				NUnit.Framework.Assert.That(invoiceCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly, Is.EqualTo(true));
				invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly, Is.EqualTo(true));

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
				NUnit.Framework.Assert.That(invoiceCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly, Is.EqualTo(false));
				invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly, Is.EqualTo(false));

				invoice.IncoTerm = Constants.IncoTerms.FreeCarrierSeller;
				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
				invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
				NUnit.Framework.Assert.That(invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(invoiceCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly, Is.EqualTo(false));
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly, Is.EqualTo(false));

				invoiceCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
				invoiceLineCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
				NUnit.Framework.Assert.That(invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount, Is.EqualTo(false).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount, Is.EqualTo(false).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(invoiceCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly, Is.EqualTo(false));
				NUnit.Framework.Assert.That(invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly, Is.EqualTo(false));
			}
		}

		[ExpectNoExceptions]
		public void TestJ7_IsIncludedInLinesAndJ7_IsNotIncludedInInvoice()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.IncoTerm = Constants.IncoTerms.FreeOnBoard;
			TestCharge invoiceCharge = invoice.Charges.AddNew();

			invoiceCharge.J7_IsIncludedInITOT = true;
			NUnit.Framework.Assert.That(invoiceCharge.J7_IsNotIncludedInInvoice, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Should be included in invoice as well");

			invoiceCharge.J7_IsNotIncludedInInvoice = true;
			NUnit.Framework.Assert.That(invoiceCharge.J7_IsIncludedInITOT, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Should not be included in line");
		}

		[ExpectNoExceptions]
		public void TestMarkApportionmentDirtyWhenChanged()
		{
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_Amount.Name, new ZDecimal(100m));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_RX_NKCurrency.Name, new ZString("AUD"));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_ChargeType.Name, new ZString("OFT"));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, ZBool.True);
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ZBool.True);
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_IsStatisticalValueApplicable.Name, ZBool.True);
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_DistributeBy.Name, new ZString("XXX"));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_Percentage.Name, new ZDecimal(10m));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_FullOrPartialApportionment.Name, new ZString("XXX"));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_IsNotIncludedInInvoice.Name, ZBool.True);
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_IsIncludedInITOT.Name, ZBool.True);
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_DistributeBy.Name, new ZString("XXX"));
			AssertApportionmentDirty(JobComInvHeaderChargeSchema.J7_ChargeDescription.Name, new ZString("XXX"));
		}

		[ExpectNoExceptions]
		void AssertApportionmentDirty(string fieldNameToChangeApportionmentDirtiness, IZType valueToAssign)
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();

			TestInvoice invoice = declaration.Invoices.AddNew();
			TestCharge invoiceCharge = invoice.Charges.AddNew();
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(false), "Apportionment is not dirty yet");

			invoiceCharge[fieldNameToChangeApportionmentDirtiness] = valueToAssign;
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(true), "Apportionment is dirty yet");
		}

		[ExpectNoExceptions]
		public void TestSuspendMarkApportionmentDirty()
		{
			var declaration = Factory.New<TestDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			var invoiceCharge = invoice.Charges.AddNew();
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(false), "Apportionment is not dirty yet");

			using (invoiceCharge.SuspendMarkApportionmentDirty())
			{
				invoiceCharge.J7_DistributeBy = "VAL";
			}
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(false), "Apportionment is not marked as dirty, suspended marking apportionment as dirty");

			invoiceCharge.J7_DistributeBy = "VOL";
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(true), "Apportionment marked as dirty");
		}

		[ExpectNoExceptions]
		public void TestMarkApportionmentDirtyWhenIsJ7_ExchangeRateUserEnterableChanged()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();

			TestInvoice invoice = declaration.Invoices.AddNew();
			TestCharge invoiceCharge = invoice.Charges.AddNew();
			SetIsJ7_ExchangeRateUserEnterable(invoiceCharge, false);
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(false), "Apportionment is not dirty yet");

			SetIsJ7_ExchangeRateUserEnterable(invoiceCharge, true);
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(true), "Apportionment is dirty yet");

			declaration.ApportionmentDirty = false;
			SetIsJ7_ExchangeRateUserEnterable(invoiceCharge, false);
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(true), "Apportionment is not dirty yet");
		}

		[ExpectNoExceptions]
		public void TestMarkApportionmentDirtyWhenExchangeRateChanged()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();

			TestInvoice invoice = declaration.Invoices.AddNew();
			TestCharge invoiceCharge = invoice.Charges.AddNew();
			invoiceCharge.J7_ExchangeRateType = ZString.Empty;
			declaration.ApportionmentDirty = false;

			invoiceCharge.J7_ExchangeRateType = "BZS";
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(false), "Apportionment is dirty yet");

			invoiceCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(true), "Apportionment is dirty yet");

			declaration.ApportionmentDirty = false;
			invoiceCharge.J7_ExchangeRateType = "BZS";
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(true), "Apportionment is dirty yet");

			declaration.ApportionmentDirty = false;
			invoiceCharge.J7_ExchangeRateType = ZString.Empty;
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(false), "Apportionment is dirty yet");
		}

		[ExpectNoExceptions]
		public void TestMarkApportionmentDirtyWhenExchangeRateTypeChanged()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			TestCharge invoiceCharge = invoice.Charges.AddNew();

			SetIsJ7_ExchangeRateUserEnterable(invoiceCharge, false);
			invoiceCharge.J7_ExchangeRate = 0.323m;

			NUnit.Framework.Assert.That(invoiceCharge.IsJ7_ExchangeRateUserEnterable, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IsJ7_ExchangeRateUserEnterable");
			invoiceCharge.J7_ExchangeRate = 0.123m;
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(false), "Apportionment is dirty yet");

			invoiceCharge.J7_ExchangeRate = 0.323m;
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(false), "Apportionment is dirty yet");

			SetIsJ7_ExchangeRateUserEnterable(invoiceCharge, true);
			declaration.ApportionmentDirty = false;

			NUnit.Framework.Assert.That(invoiceCharge.IsJ7_ExchangeRateUserEnterable, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IsJ7_ExchangeRateUserEnterable");
			invoiceCharge.J7_ExchangeRate = 0.123m;
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(true), "Apportionment is dirty yet");

			declaration.ApportionmentDirty = false;
			invoiceCharge.J7_ExchangeRate = 0.323m;
			NUnit.Framework.Assert.That(declaration.ApportionmentDirty, Is.EqualTo(true), "Apportionment is dirty yet");
		}

		[ExpectNoExceptions]
		public void TestIsJ7_ExchangeRateUserEnterableInfoReadOnly()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			TestCharge charge = invoice.Charges.AddNew();

			charge.J7_Percentage = 10m;
			NUnit.Framework.Assert.That(charge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly, Is.EqualTo(true));

			charge.J7_Percentage = ZDecimal.Zero;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			NUnit.Framework.Assert.That(charge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly, Is.EqualTo(true));

			charge.J7_RX_NKCurrency = Constants.CurrencyCodes.Afghanistan;
			NUnit.Framework.Assert.That(charge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly, Is.EqualTo(false));

			charge.J7_RX_NKCurrency = ZString.Empty;
			NUnit.Framework.Assert.That(charge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly, Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestJ7_ExchangeRateInfoReadOnly()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			TestCharge charge = invoice.Charges.AddNew();

			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			charge.J7_Percentage = 10m;
			SetIsJ7_ExchangeRateUserEnterable(charge, true);
			NUnit.Framework.Assert.That(charge.J7_ExchangeRateInfo.ReadOnly, Is.EqualTo(true));

			charge.J7_Percentage = ZDecimal.Zero;
			SetIsJ7_ExchangeRateUserEnterable(charge, true);
			NUnit.Framework.Assert.That(charge.J7_ExchangeRateInfo.ReadOnly, Is.EqualTo(false));

			SetIsJ7_ExchangeRateUserEnterable(charge, false);
			NUnit.Framework.Assert.That(charge.J7_ExchangeRateInfo.ReadOnly, Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestJ7_ExchangeRateTypeInfoReadOnly()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			TestCharge charge = invoice.Charges.AddNew();

			charge.J7_Percentage = 10m;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			NUnit.Framework.Assert.That(charge.J7_ExchangeRateTypeInfo.ReadOnly, Is.EqualTo(true));

			charge.J7_Percentage = 0m;
			charge.J7_RX_NKCurrency = Constants.CurrencyCodes.Afghanistan;
			charge.IsJ7_ExchangeRateUserEnterable = true;
			NUnit.Framework.Assert.That(charge.J7_ExchangeRateTypeInfo.ReadOnly, Is.EqualTo(false));

			charge.J7_Percentage = ZDecimal.Zero;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			NUnit.Framework.Assert.That(charge.J7_ExchangeRateTypeInfo.ReadOnly, Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestIDefaultLandedCostInput()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			TestCharge charge = invoice.Charges.AddNew();

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).ChargeDescription, Is.EqualTo(CustomsChargeTypeList.Descriptions.OverseasFreight.ToString().ToUpper() + " from Entry").Using(CustomComparers.TypeComparison), "ChargeDescription");
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).AmountToDistribute.Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "AmountToDistribute Amount");
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).AmountToDistribute.Currency.Code, Is.EqualTo(declaration.LocalCurrencyCode).Using(CustomComparers.TypeComparison), "AmountToDistribute Currency");
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).FKToChargeCode, Is.EqualTo(ZGuid.Empty), "FKToChargeCode");
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).ExchangeRate, Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "ExchangeRate");
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).IsValidToImport, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IsValidToImport");

			charge.J7_AdjustedCharge = true;
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).IsValidToImport, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IsValidToImport");

			charge.J7_AdjustedCharge = false;
			DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.PullLandedCostingDataFromBillingTabOnly.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, true);
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).IsValidToImport, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IsValidToImport");
			DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.PullLandedCostingDataFromBillingTabOnly.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, false);

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).ChargeDescription, Is.EqualTo("Deduction (or Discount) from Entry").Using(CustomComparers.TypeComparison), "ChargeDescription");
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).AmountToDistribute.Amount, Is.EqualTo(-100m).Using(CustomComparers.TypeComparison), "AmountToDistribute Amount");

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).ChargeDescription, Is.EqualTo("Deduction (or Discount) from Entry").Using(CustomComparers.TypeComparison), "ChargeDescription");
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).AmountToDistribute.Amount, Is.EqualTo(-100m).Using(CustomComparers.TypeComparison), "AmountToDistribute Amount");
		}

		[ExpectNoExceptions]
		public void TestIsValidToImportForLC()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			TestCharge charge = invoice.Charges.AddNew();

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).IsValidToImport, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "PreCondition:IsValidToImport");

			DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.PullLandedCostingDataFromBillingTabOnly.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).IsValidToImport, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IsNotValid anymore");
		}

		[ExpectNoExceptions]
		public void TestIsValidToImportForLCForApportionedCharge()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestCharge charge = declaration.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

			TestInvoice invoice = declaration.Invoices.AddNew();
			invoice.InvoiceCurrencyCode = declaration.LocalCurrencyCode;
			invoice.InvoicePrice = 10000m;

			new ApportionManager(declaration, new ApportionStrategy()).ApportionAll();

			NUnit.Framework.Assert.That(invoice.ApportionedCharges.Count, Is.EqualTo(1), "precondition");

			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)charge).IsValidToImport, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IsValidToImport");
			NUnit.Framework.Assert.That(((IDefaultLandedCostInput)invoice.ApportionedCharges[0]).IsValidToImport, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IsValidToImport");
		}

		[ExpectNoExceptions]
		public void TestReadOnly()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			TestCharge charge = invoice.Charges.AddNew();

			charge.J7_IsDutiable = true;
			NUnit.Framework.Assert.That(charge.J7_IsGSTApplicableInfo.ReadOnly, Is.EqualTo(true), "IsGSTapplicable readonly");

			TestCharge charge2 = invoice.Charges.AddNew();
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			NUnit.Framework.Assert.That(charge2.J7_IsDutiableInfo.ReadOnly, Is.EqualTo(true), "IsDutiable Readonly");
			NUnit.Framework.Assert.That(charge2.J7_IsGSTApplicableInfo.ReadOnly, Is.EqualTo(true), "IsGSTapplicable readonly");

			TestCharge charge3 = invoice.Charges.AddNew();
			charge3.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			NUnit.Framework.Assert.That(charge3.J7_IsDutiableInfo.ReadOnly, Is.EqualTo(true), "IsDutiable Readonly");
			NUnit.Framework.Assert.That(charge3.J7_IsGSTApplicableInfo.ReadOnly, Is.EqualTo(true), "IsGSTapplicable readonly");
		}

		[ExpectNoExceptions]
		public void TestIsDutiable()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			TestCharge charge = invoice.Charges.AddNew();

			charge.J7_IsDutiable = true;
			NUnit.Framework.Assert.That(charge.J7_IsGSTApplicable, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IsGSTApplicable");
		}

		protected virtual void SetIsJ7_ExchangeRateUserEnterable(JobComInvCharge charge, bool value)
		{
			charge.IsJ7_ExchangeRateUserEnterable = value;
		}
	}
}
