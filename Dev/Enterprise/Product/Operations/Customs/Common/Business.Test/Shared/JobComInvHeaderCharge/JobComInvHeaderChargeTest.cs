using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class BaseJobComInvHeaderChargeTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSetDefaultValuesForSystem()
		{
			TestCharge charge = Factory.New<TestCharge>();
			NUnit.Framework.Assert.That(charge.J7_IsSystem, Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsAMMV()
		{
			#region SetUp Data

			var declaration = Factory.NewWithValidTestData<TestDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge.J7_IsNotIncludedInInvoice = true;
			charge.J7_IsSystem = true;
			charge.J7_IsDutiable = true;

			#endregion

			NUnit.Framework.Assert.That(charge.IsAMMV(), Is.EqualTo(true));
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			NUnit.Framework.Assert.That(charge.IsAMMV(), Is.EqualTo(false));

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge.J7_IsNotIncludedInInvoice = false;
			NUnit.Framework.Assert.That(charge.IsAMMV(), Is.EqualTo(false));

			charge.J7_IsDutiable = false;
			charge.J7_IsNotIncludedInInvoice = true;
			NUnit.Framework.Assert.That(charge.IsAMMV(), Is.EqualTo(false));

			charge.J7_IsDutiable = true;
			charge.J7_IsSystem = false;
			NUnit.Framework.Assert.That(charge.IsAMMV(), Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestJ7_ExchangeRateDateDescription()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			JobComInvCharge invoiceCharge = invoice.Charges.AddNew();
			NUnit.Framework.Assert.That(invoiceCharge.J7_ExchangeRateDateInfo.Description, Is.EqualTo("Exchange Rate Date"));
		}

		[ExpectNoExceptions]
		public void TestJ7_ExchangeRateDateReadOnlyWhenFixed()
		{
			CombineAssertions(() =>
			{
				TestDeclaration declaration = Factory.New<TestDeclaration>();
				TestInvoice invoice = declaration.Invoices.AddNew();
				JobComInvCharge invoiceCharge = invoice.Charges.AddNew();
				NUnit.Framework.Assert.That(invoiceCharge.J7_ExchangeRateDateInfo.ReadOnly, Is.EqualTo(true), "Default -> ReadOnly");
				invoiceCharge.IsJ7_ExchangeRateUserEnterable = true;
				NUnit.Framework.Assert.That(invoiceCharge.J7_ExchangeRateDateInfo.ReadOnly, Is.EqualTo(false), "Not fixed -> Editable");
				invoiceCharge.IsJ7_ExchangeRateUserEnterable = false;
				NUnit.Framework.Assert.That(invoiceCharge.J7_ExchangeRateDateInfo.ReadOnly, Is.EqualTo(true), "Fixed -> ReadOnly");
			});
		}

		[ExpectNoExceptions]
		public void TestUntickFixedRateResetsExchangeRateData()
		{
			CombineAssertions(() =>
			{
				testCharge.IsJ7_ExchangeRateUserEnterable = true;
				testCharge.J7_ExchangeRate = 1.25m;
				testCharge.J7_ExchangeRateDate = ZDate.Today;

				testCharge.IsJ7_ExchangeRateUserEnterable = false;
				NUnit.Framework.Assert.That(testCharge.J7_ExchangeRate, Is.EqualTo(0m).Using(CustomComparers.TypeComparison), "Exchange Rate");
				NUnit.Framework.Assert.That(testCharge.J7_ExchangeRateDate, Is.EqualTo(ZDate.Empty), "Exchange Rate Date");
			});
		}

		[ExpectNoExceptions]
		public void TestChangeCurrencyResetsExchangeRateData()
		{
			CombineAssertions(() =>
			{
				testCharge.J7_RX_NKCurrency = Constants.CurrencyCodes.Afghanistan;
				testCharge.IsJ7_ExchangeRateUserEnterable = true;
				testCharge.J7_ExchangeRate = 1.25m;
				testCharge.J7_ExchangeRateDate = ZDate.Today;

				testCharge.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
				NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterable, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Fixed Rate");
				NUnit.Framework.Assert.That(testCharge.J7_ExchangeRate, Is.EqualTo(1m).Using(CustomComparers.TypeComparison), "Exchange Rate");
				NUnit.Framework.Assert.That(testCharge.J7_ExchangeRateDate, Is.EqualTo(ZDate.Empty), "Exchange Rate Date");
			});
		}

		[ExpectNoExceptions]
		public void TestJ7_Calc_IsIncludedInInvoiceAmountAndReadOnly()
		{
			TestDeclaration declaration = Factory.New<TestDeclaration>();
			TestInvoice invoice = declaration.Invoices.AddNew();
			JobComInvCharge invoiceCharge = invoice.Charges.AddNew();

			invoiceCharge.J7_IsNotIncludedInInvoice = true;
			NUnit.Framework.Assert.That(invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "J7_Calc_IsIncludedInInvoiceAmount");

			invoiceCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			NUnit.Framework.Assert.That(invoiceCharge.J7_IsNotIncludedInInvoice, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "J7_IsNotIncludedInInvoice");

			TestInvoiceLine invoiceLine = invoice.InvoiceLines.AddNew();
			TestCharge invoiceLineCharge = invoiceLine.Charges.AddNew();

			invoiceLineCharge.J7_IsNotIncludedInInvoice = true;
			NUnit.Framework.Assert.That(invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "J7_Calc_IsIncludedInInvoiceAmount");

			invoiceLineCharge.J7_Calc_IsIncludedInInvoiceAmount = true;
			NUnit.Framework.Assert.That(invoiceLineCharge.J7_IsNotIncludedInInvoice, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "J7_IsNotIncludedInInvoice");
		}

		[ExpectNoExceptions]
		public void TestIsJ7_ExchangeRateUserEnterable()
		{
			testCharge.IsJ7_ExchangeRateUserEnterable = true;
			NUnit.Framework.Assert.That(testCharge.J7_ExchangeRateType, Is.EqualTo(ChargeExchangeRateTypeList.Codes.FixedRate).Using(CustomComparers.TypeComparison));

			testCharge.IsJ7_ExchangeRateUserEnterable = false;
			NUnit.Framework.Assert.That(testCharge.J7_ExchangeRateType, Is.EqualTo(ZString.Empty));

			testCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterable, Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			testCharge.J7_ExchangeRateType = ZString.Empty;
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterable, Is.EqualTo(false).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestJ7_ChargeType()
		{
			var charge011 = new CustomsChargeCode("011", (NoResString)"011")
			{
				IsDutiable = true,
				IsVATible = true,
				IsStatisticalValueApplicable = true,
				IsDutiableDeemedForThisCharge = true,
				IsIncludedInITOTDeemedForThisCharge = true,
				IsVATibleDeemedForThisCharge = true
			};

			var charge014 = new CustomsChargeCode("014", (NoResString)"014")
			{
				IsDutiable = false,
				IsVATible = true,
				IsStatisticalValueApplicable = false,
				IsDutiableDeemedForThisCharge = true,
				IsIncludedInITOTDeemedForThisCharge = true,
				IsVATibleDeemedForThisCharge = true
			};

			var incoTermAndChargeFactoryMock = new Mock<CommonIncoTermAndCustomsChargeFactory>();
			incoTermAndChargeFactoryMock.Protected().Setup<ICustomsChargeCode[]>("GetCharges").Returns(new ICustomsChargeCode[] { charge011, charge014 });
			var declaration = Factory.New<TestDeclaration>();
			declaration.IncoTermAndChargeFactory = incoTermAndChargeFactoryMock.Object;
			var invoice = declaration.Invoices.AddNew();
			var charge = invoice.Charges.AddNew();

			charge.J7_ChargeType = charge011.Code;
			NUnit.Framework.Assert.That(charge.J7_IsStatisticalValueApplicable, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(charge.J7_IsDutiable, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(charge.J7_IsGSTApplicable, Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			charge.J7_ChargeType = charge014.Code;
			NUnit.Framework.Assert.That(!charge.J7_IsStatisticalValueApplicable, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(!charge.J7_IsDutiable, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(charge.J7_IsGSTApplicable, Is.EqualTo(true).Using(CustomComparers.TypeComparison));

			var chargeIsVATibleValidated = 0;
			var chargeIsVATibleChanged = 0;
			charge.J7_IsGSTApplicableInfo.ValueChanged += (s, e) => chargeIsVATibleChanged++;
			charge.J7_IsGSTApplicableInfo.AdditionalValidation += () => chargeIsVATibleValidated++;
			charge011.IsVATible = false;
			charge.J7_ChargeType = charge011.Code;
			NUnit.Framework.Assert.That(chargeIsVATibleChanged, Is.GreaterThan(0), "J7_IsGSTApplicable should have changed.");
			NUnit.Framework.Assert.That(chargeIsVATibleValidated, Is.EqualTo(0), "Should not run validation on J7_IsGSTApplicableInfo.");
			incoTermAndChargeFactoryMock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestIsDiscount()
		{
			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
			NUnit.Framework.Assert.That(testCharge.IsDiscount, Is.EqualTo(true), "IsDiscount");

			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			NUnit.Framework.Assert.That(testCharge.IsDiscount, Is.EqualTo(false), "IsDiscount");
		}

		[ExpectNoExceptions]
		public void TestJ7_ChargeDescription()
		{
			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			NUnit.Framework.Assert.That(testCharge.J7_ChargeDescription, Is.EqualTo(CustomsChargeTypeList.Descriptions.Commission).Using(CustomComparers.TypeComparison), "J7_ChargeDescription");

			testCharge.J7_ChargeDescription = "Blah Blah";
			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			NUnit.Framework.Assert.That(testCharge.J7_ChargeDescription, Is.EqualTo("Blah Blah").Using(CustomComparers.TypeComparison), "J7_ChargeDescription");

			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			var expectedAdditionChargeDescription = new ZString(CustomsChargeTypeList.Descriptions.AdditionCharge).Left(testCharge.J7_ChargeDescriptionInfo.MaxLength).TrimEnd(' ');
			NUnit.Framework.Assert.That(testCharge.J7_ChargeDescription, Is.EqualTo(expectedAdditionChargeDescription), "J7_ChargeDescription");
		}

		[ExpectNoExceptions]
		public void TestIsJ7_ExchangeRateUserEnterableInfoReadOnly()
		{
			testCharge.J7_RX_NKCurrency = ZString.Empty;
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly, Is.EqualTo(true));

			testCharge.J7_RX_NKCurrency = Constants.CurrencyCodes.Afghanistan;
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly, Is.EqualTo(false));

			testCharge.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly, Is.EqualTo(true));

			testCharge.J7_RX_NKCurrency = Constants.CurrencyCodes.Afghanistan;
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly, Is.EqualTo(false));

			testCharge.J7_Percentage = 1m;
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly, Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestJ7_ExchangeRateInfoReadOnly()
		{
			SetIsJ7_ExchangeRateUserEnterable(testCharge, false);
			NUnit.Framework.Assert.That(testCharge.J7_ExchangeRateInfo.ReadOnly, Is.EqualTo(true));

			SetIsJ7_ExchangeRateUserEnterable(testCharge, true);
			NUnit.Framework.Assert.That(testCharge.J7_ExchangeRateInfo.ReadOnly, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestJ7_ExchangeRateTypeInfoReadOnly()
		{
			testCharge.J7_RX_NKCurrency = ZString.Empty;
			NUnit.Framework.Assert.That(testCharge.J7_ExchangeRateTypeInfo.ReadOnly, Is.EqualTo(true));

			testCharge.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			NUnit.Framework.Assert.That(testCharge.J7_ExchangeRateTypeInfo.ReadOnly, Is.EqualTo(true));

			testCharge.J7_RX_NKCurrency = Constants.CurrencyCodes.Afghanistan;
			NUnit.Framework.Assert.That(testCharge.J7_ExchangeRateTypeInfo.ReadOnly, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestFixedExchangeRate()
		{
			testCharge.J7_RX_NKCurrency = Constants.CurrencyCodes.Bulgaria;
			testCharge.J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
			testCharge.J7_ExchangeRate = 0.1m;
			testCharge.J7_Amount = 100m;
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterable, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "IsJ7_ExchangeRateUserEnterable");
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly, Is.EqualTo(false), "IsJ7_ExchangeRateUserEnterableInfo.ReadOnly");
			NUnit.Framework.Assert.That(testCharge.Money.Amount, Is.EqualTo(1000m).Using(CustomComparers.TypeComparison), "Money.Amount");
			NUnit.Framework.Assert.That(testCharge.Money.Currency.Code, Is.EqualTo(invoice.LocalCurrencyCode).Using(CustomComparers.TypeComparison), "Money.Currency");

			testCharge.J7_ExchangeRateType = ZString.Empty;
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterable, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IsJ7_ExchangeRateUserEnterable");
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly, Is.EqualTo(false), "IsJ7_ExchangeRateUserEnterableInfo.ReadOnly");
			NUnit.Framework.Assert.That(testCharge.Money.Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "Money.Amount");
			NUnit.Framework.Assert.That(testCharge.Money.Currency.Code, Is.EqualTo(Constants.CurrencyCodes.Bulgaria), "Money.Currency");

			testCharge.J7_RX_NKCurrency = ZString.Empty;
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterable, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IsJ7_ExchangeRateUserEnterable");
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterableInfo.ReadOnly, Is.EqualTo(true), "IsJ7_ExchangeRateUserEnterableInfo.ReadOnly");
			NUnit.Framework.Assert.That(testCharge.Money.Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "Money.Amount");
			NUnit.Framework.Assert.That(testCharge.Money.Currency, Is.EqualTo(default(Integration.ZArchitecture.ICurrency)), "Money.Currency - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestSupportClone()
		{
			testCharge.Clone();
		}

		[ExpectNoExceptions]
		public void TestTypeDecider()
		{
			NUnit.Framework.Assert.That(JobComInvCharge.TypeDecider.GetType(), Is.EqualTo(typeof(JobComInvHeaderChargeTypeDecider)), "TypeDecider");
		}

		[ExpectNoExceptions]
		public void TestAccessingParentAfterDeleteDoesNotCauseExceptions()
		{
			JobComInvCharge charge = invoice.Charges.AddNew();
			NUnit.Framework.Assert.That(charge.Parent, Is.EqualTo(invoice).Using(CustomComparers.TypeComparison), "Parent");
			charge.Delete();
			NUnit.Framework.Assert.That(charge.Parent, Is.EqualTo(default(ICommonInvoice)), "Parent of Charge after deleted. It does not access J7_ParentID - should be [null]");
		}

		[ExpectNoExceptions]
		public void TestIsEmpty()
		{
			JobComInvCharge charge = invoice.Charges.AddNew();
			NUnit.Framework.Assert.That(charge.IsEmpty, Is.EqualTo(true), "IsEmpty");

			charge.J7_Amount = 10m;
			NUnit.Framework.Assert.That(charge.IsEmpty, Is.EqualTo(false), "IsEmpty");

			charge.J7_Amount = 0m;
			charge.J7_Percentage = 10m;
			NUnit.Framework.Assert.That(charge.IsEmpty, Is.EqualTo(false), "IsEmpty");
		}

		[ExpectNoExceptions]
		public void TestIsIncludedInITOTDeemedForThisCharge()
		{
			JobComInvCharge charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge);
			NUnit.Framework.Assert.That(charge.ChargeCode.IsIncludedInITOTDeemedForThisCharge, Is.EqualTo(false), "Customs Charge ADD deems 'Is Included In Lines'");
			NUnit.Framework.Assert.That(charge.J7_IsIncludedInITOT, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "ADD should not be included in ITOT");

			charge = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge);
			NUnit.Framework.Assert.That(charge.ChargeCode.IsIncludedInITOTDeemedForThisCharge, Is.EqualTo(false), "Customs Charge DED deems 'IsIncludedInLines");
			NUnit.Framework.Assert.That(charge.J7_IsIncludedInITOT, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "DED should be included in ITOT");
		}

		[ExpectNoExceptions]
		public void TestDefaultCurrencyToInvoiceCurr()
		{
			invoice.InvoiceCurrencyCode = invoice.LocalCurrencyCode;
			testCharge.J7_Amount = 100m;
			NUnit.Framework.Assert.That(testCharge.J7_RX_NKCurrency, Is.EqualTo(invoice.LocalCurrencyCode), "Currency set");
		}

		[ExpectNoExceptions]
		public void TestMoney()
		{
			testCharge.J7_Amount = 100m;
			testCharge.J7_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;

			NUnit.Framework.Assert.That(testCharge.Money.Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "Amount");
			NUnit.Framework.Assert.That(testCharge.Money.Currency.Code, Is.EqualTo(Constants.CurrencyCodes.UnitedStates), "Currency");

			testCharge.J7_ExchangeRateType = "FIX";
			NUnit.Framework.Assert.That(testCharge.IsJ7_ExchangeRateUserEnterable, Is.EqualTo(true).Using(CustomComparers.TypeComparison));
			testCharge.J7_ExchangeRate = 0.5m;
			NUnit.Framework.Assert.That(testCharge.Money.Amount, Is.EqualTo(200m).Using(CustomComparers.TypeComparison), "Amount");
			NUnit.Framework.Assert.That(testCharge.Money.Currency.Code, Is.EqualTo(invoice.LocalCurrencyCode).Using(CustomComparers.TypeComparison), "Currency");

			testCharge.Parent = null;
			NUnit.Framework.Assert.That(testCharge.Money.Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "Amount");
			NUnit.Framework.Assert.That(testCharge.Money.Currency.Code, Is.EqualTo(Constants.CurrencyCodes.UnitedStates), "Currency");
		}

		[ExpectNoExceptions]
		public void TestChargeKey()
		{
			TestCharge charge = Factory.New<TestCharge>();

			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
			NUnit.Framework.Assert.That(charge.ChargeKey.ChargeCode, Is.EqualTo(charge.J7_ChargeType).Using(CustomComparers.TypeComparison), "ChargeKey");

			charge.J7_IsDutiable = !charge.J7_IsDutiable;
			NUnit.Framework.Assert.That(charge.ChargeKey.IsDutiable, Is.EqualTo(charge.J7_IsDutiable).Using(CustomComparers.TypeComparison), "ChargeKey");

			charge.J7_IsGSTApplicable = !charge.J7_IsGSTApplicable;
			NUnit.Framework.Assert.That(charge.ChargeKey.IsVATible, Is.EqualTo(charge.J7_IsGSTApplicable).Using(CustomComparers.TypeComparison), "ChargeKey");

			charge.J7_IsStatisticalValueApplicable = true;
			NUnit.Framework.Assert.That(charge.ChargeKey.IsStatisticalValueApplicable, Is.EqualTo(true));
			charge.J7_IsStatisticalValueApplicable = false;
			NUnit.Framework.Assert.That(charge.ChargeKey.IsStatisticalValueApplicable, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAdditionDeductionChargeReadOnly()
		{
			JobComInvCharge addition = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 100);
			JobComInvCharge deduction = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.DeductionCharge, 100);

			NUnit.Framework.Assert.That(addition.J7_ChargeTypeInfo.ReadOnly, Is.EqualTo(false), "ChargeType");
			NUnit.Framework.Assert.That(addition.J7_IsDutiableInfo.ReadOnly, Is.EqualTo(true), "Duty");
			NUnit.Framework.Assert.That(addition.J7_IsGSTApplicableInfo.ReadOnly, Is.EqualTo(true), "GST");

			NUnit.Framework.Assert.That(addition.J7_IsIncludedInITOT, Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Included in ITOT");
			NUnit.Framework.Assert.That(addition.J7_IsIncludedInITOTInfo.ReadOnly, Is.EqualTo(false), "Included readonly");

			NUnit.Framework.Assert.That(deduction.J7_ChargeTypeInfo.ReadOnly, Is.EqualTo(false), "ChargeType");
			NUnit.Framework.Assert.That(deduction.J7_IsDutiableInfo.ReadOnly, Is.EqualTo(true), "Duty");
			NUnit.Framework.Assert.That(deduction.J7_IsGSTApplicableInfo.ReadOnly, Is.EqualTo(true), "GST");

			NUnit.Framework.Assert.That(deduction.J7_IsIncludedInITOT, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Included in ITOT");
			NUnit.Framework.Assert.That(deduction.J7_IsIncludedInITOTInfo.ReadOnly, Is.EqualTo(false), "Included readonly");
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			testCharge.Parent = null;
			NUnit.Framework.Assert.That(testCharge.J7_ParentID, Is.EqualTo(ZGuid.Empty), "J7_ParentID removed");
			NUnit.Framework.Assert.That(testCharge.J7_ParentTableCode, Is.EqualTo("").Using(CustomComparers.TypeComparison), "J7_ParentTableCode removed");
			NUnit.Framework.Assert.That(testCharge.Parent, Is.EqualTo(default(ICommonInvoice)), "Parent object");

			testCharge.Parent = invoice;
			NUnit.Framework.Assert.That(testCharge.J7_ParentID, Is.EqualTo(invoice.PK), "J7_ParentID set");
			NUnit.Framework.Assert.That(testCharge.J7_ParentTableCode, Is.EqualTo("Z0").Using(CustomComparers.TypeComparison), "J7_ParentTableCode set");
			NUnit.Framework.Assert.That(testCharge.Parent, Is.EqualTo(invoice).Using(CustomComparers.TypeComparison), "Parent object");
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var testDec = Factory.New<TestDeclaration>();
			var testCharge = testDec.Invoices.AddNew().Charges.AddNew();
			NUnit.Framework.Assert.That(testCharge.J7_DistributeBy, Is.EqualTo(ChargeDistributeByList.Codes.Value).Using(CustomComparers.TypeComparison), "Distribute-by should be 'VAL'");
			NUnit.Framework.Assert.That(testCharge.J7_FullOrPartialApportionment, Is.EqualTo(ApportionmentTypeList.Codes.PartialApportionment).Using(CustomComparers.TypeComparison), "Apportionment type should be 'PUA'");
		}

		[ExpectNoExceptions]
		public void TestDistributeBy()
		{
			var testDec = Factory.New<TestDeclaration>();
			var testCharge = testDec.Invoices.AddNew().Charges.AddNew();
			NUnit.Framework.Assert.That(testCharge.J7_DistributeBy, Is.EqualTo(ChargeDistributeByList.Codes.Value).Using(CustomComparers.TypeComparison), "Distribute by should be 'VAL'");

			testCharge.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			NUnit.Framework.Assert.That(testCharge.J7_DistributeBy, Is.EqualTo(ChargeDistributeByList.Codes.Weight).Using(CustomComparers.TypeComparison), "Distribute by should retain what users have entered");
		}

		[ExpectNoExceptions]
		public void TestRefreshChargeCodeChargeKeyAndApportionChargeKey()
		{
			var testDec = Factory.New<TestDeclaration>();
			var testCharge = testDec.Invoices.AddNew().Charges.AddNew();
			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			NUnit.Framework.Assert.That(testCharge.ChargeKey.ChargeCode, Is.EqualTo(CustomsChargeTypeList.Codes.OverseasFreight), "ChargeKey.ChargeType refreshed");
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ChargeKey.ChargeCode, Is.EqualTo(CustomsChargeTypeList.Codes.OverseasFreight), "ApportionmentChargeKey.ChargeType refreshed");

			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			NUnit.Framework.Assert.That(testCharge.ChargeKey.ChargeCode, Is.EqualTo(CustomsChargeTypeList.Codes.OverseasInsurance), "ChargeKey.ChargeType refreshed");
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ChargeKey.ChargeCode, Is.EqualTo(CustomsChargeTypeList.Codes.OverseasInsurance), "ApportionmentChargeKey.ChargeType refreshed");

			testCharge.J7_IsDutiable = !testCharge.J7_IsDutiable;
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ChargeKey.IsDutiable, Is.EqualTo(testCharge.J7_IsDutiable).Using(CustomComparers.TypeComparison), "IsDutiable is refreshed");
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ChargeKey.ChargeCode, Is.EqualTo(CustomsChargeTypeList.Codes.OverseasInsurance), "ApportionmentChargeKey.ChargeType");
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ApportionType, Is.EqualTo(ApportionmentTypeList.Codes.PartialApportionment), "ApportionmentChargeKey.ApportionType");

			testCharge.J7_IsGSTApplicable = !testCharge.J7_IsGSTApplicable;
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ChargeKey.IsVATible, Is.EqualTo(testCharge.J7_IsGSTApplicable).Using(CustomComparers.TypeComparison), "IsDutiable is refreshed");
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ChargeKey.IsDutiable, Is.EqualTo(testCharge.J7_IsDutiable).Using(CustomComparers.TypeComparison), "IsDutiable");
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ChargeKey.ChargeCode, Is.EqualTo(CustomsChargeTypeList.Codes.OverseasInsurance), "ApportionmentChargeKey.ChargeType");
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ApportionType, Is.EqualTo(ApportionmentTypeList.Codes.PartialApportionment), "ApportionmentChargeKey.ApportionType");

			testCharge.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ChargeKey.ChargeCode, Is.EqualTo(CustomsChargeTypeList.Codes.OverseasInsurance), "ApportionmentChargeKey.ChargeType");
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ApportionType, Is.EqualTo(ApportionmentTypeList.Codes.FullApportionment), "ApportionmentChargeKey.ApportionType refreshed");

			testCharge.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.PartialApportionment;
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ChargeKey.ChargeCode, Is.EqualTo(CustomsChargeTypeList.Codes.OverseasInsurance), "ApportionmentChargeKey.ChargeType");
			NUnit.Framework.Assert.That(testCharge.ApportionChargeKey.ApportionType, Is.EqualTo(ApportionmentTypeList.Codes.PartialApportionment), "ApportionmentChargeKey.ApportionType refreshed");
		}

		[ExpectNoExceptions]
		public void TestWithKey()
		{
			var testDec = Factory.New<TestDeclaration>();
			var testCharge = testDec.Invoices.AddNew().Charges.AddNew();
			testCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			testCharge.J7_IsDutiable = false;
			testCharge.J7_IsGSTApplicable = true;
			testCharge.J7_FullOrPartialApportionment = "";
			testCharge.J7_IsIncludedInITOT = false;
			testCharge.J7_IsStatisticalValueApplicable = false;

			NUnit.Framework.Assert.That(testCharge.WithKey(testCharge.ChargeKey), Is.EqualTo(true), "WithKey with ChargeKey");
			NUnit.Framework.Assert.That(testCharge.WithKey(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, true, true, false)), Is.EqualTo(false), "WithKey with AnotherChargeKey");

			NUnit.Framework.Assert.That(testCharge.WithKey(testCharge.ApportionChargeKey), Is.EqualTo(true), "WithKey with ApportionChargeKey");
			NUnit.Framework.Assert.That(testCharge.WithKey(new ApportionChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true, ApportionmentTypeList.Codes.FullApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.No, ChargeDistributeByList.Codes.Value, 0m, false, "", false, false)), Is.EqualTo(false), "WithKey with Another Apportion ChargeKey");

			NUnit.Framework.Assert.That(testCharge.WithKey(testCharge.MessageChargeKey), Is.EqualTo(true), "WithKey with MessageChargeKey");
			NUnit.Framework.Assert.That(testCharge.WithKey(new MessageChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, true, false, true, true)), Is.EqualTo(false), "WithKey with Another MessageChargeKey");
		}

		[ExpectNoExceptions]
		public void TestIsFullApportionment()
		{
			var testDec = Factory.New<TestDeclaration>();
			var testCharge = testDec.Invoices.AddNew().Charges.AddNew();
			NUnit.Framework.Assert.That(testCharge.IsFullApportionment, Is.EqualTo(false), "IsFullApportionment");

			testCharge.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.PartialApportionment;
			NUnit.Framework.Assert.That(testCharge.IsFullApportionment, Is.EqualTo(false), "IsFullApportionment");

			testCharge.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;
			NUnit.Framework.Assert.That(testCharge.IsFullApportionment, Is.EqualTo(true), "IsFullApportionment");
		}

		[ExpectNoExceptions]
		public void TestNoOfDecimalsForPercentage()
		{
			NUnit.Framework.Assert.That(testCharge.NoOfDecimalsForPercentage, Is.EqualTo(5).Using(CustomComparers.TypeComparison), "Number of decimals for percentage");
		}

		[ExpectNoExceptions]
		public void TestShouldSetCurrencyToLinePriceRefCurrency()
		{
			CombineAssertions("Non-Apportioned Charges", () =>
			{
				var testDec = Factory.New<TestDeclaration>();
				var testCharge = testDec.Invoices.AddNew().Charges.AddNew();
				testCharge.J7_Percentage = 0;
				testCharge.J7_Amount = 0;

				NUnit.Framework.Assert.That(testCharge.ShouldSetCurrencyFromParent, Is.EqualTo(false));

				testCharge.J7_Percentage = 1;
				NUnit.Framework.Assert.That(testCharge.ShouldSetCurrencyFromParent, Is.EqualTo(true));

				testCharge.J7_Percentage = 0;
				testCharge.J7_Amount = 1;
				testCharge.J7_RX_NKCurrency = "USD";
				NUnit.Framework.Assert.That(testCharge.ShouldSetCurrencyFromParent, Is.EqualTo(false));

				testCharge.J7_Percentage = 0;
				testCharge.J7_Amount = 1;
				testCharge.J7_RX_NKCurrency = "";
				NUnit.Framework.Assert.That(testCharge.ShouldSetCurrencyFromParent, Is.EqualTo(true));
			});

			CombineAssertions("Apportioned Charges", () =>
			{
				var testDec = Factory.New<TestDeclaration>();
				var testCharge = testDec.Invoices.AddNew().Charges.AddNew();
				testCharge.J7_IsApportionedCharge = true;

				testCharge.J7_Percentage = 0;
				testCharge.J7_Amount = 0;
				NUnit.Framework.Assert.That(testCharge.ShouldSetCurrencyFromParent, Is.EqualTo(false));

				testCharge.J7_Percentage = 1;
				NUnit.Framework.Assert.That(testCharge.ShouldSetCurrencyFromParent, Is.EqualTo(false));

				testCharge.J7_Percentage = 0;
				testCharge.J7_Amount = 1;
				testCharge.J7_RX_NKCurrency = "USD";
				NUnit.Framework.Assert.That(testCharge.ShouldSetCurrencyFromParent, Is.EqualTo(false));

				testCharge.J7_Percentage = 0;
				testCharge.J7_Amount = 1;
				testCharge.J7_RX_NKCurrency = "";
				NUnit.Framework.Assert.That(testCharge.ShouldSetCurrencyFromParent, Is.EqualTo(false));
			});
		}

		[ExpectNoExceptions]
		public void TestAllowNonWesternEuropeanCharacterForChargeDescription()
		{
			var testDec = Factory.New<TestDeclaration>();
			var testCharge = testDec.Invoices.AddNew().Charges.AddNew();
			NUnit.Framework.Assert.That(!testCharge.AllowNonWesternEuropeanCharacterForChargeDescription, Is.EqualTo(true).Using(CustomComparers.TypeComparison), "AllowNonWesternEuropeanCharacterForChargeDescription should be false");
		}

		[ExpectNoExceptions]
		public void TestSetJ7_ChargeDescription()
		{
			var chargeWithNonWesternEuropeanCharacterAllowed = Factory.New<JobComInvChargeWithNonWesternEuropeanCharacterAllowed>();
			chargeWithNonWesternEuropeanCharacterAllowed.J7_ChargeDescription = "你好";
			NUnit.Framework.Assert.That(chargeWithNonWesternEuropeanCharacterAllowed.J7_ChargeDescription, Is.EqualTo("你好").Using(CustomComparers.TypeComparison), "Non Western European characters can be set when AllowNonWesternEuropeanCharacterForChargeDescription is true.");

			chargeWithNonWesternEuropeanCharacterAllowed.J7_ChargeDescriptionInfo.ClearValue();
			chargeWithNonWesternEuropeanCharacterAllowed.J7_ChargeDescription = "Hello";
			NUnit.Framework.Assert.That(chargeWithNonWesternEuropeanCharacterAllowed.J7_ChargeDescription, Is.EqualTo("Hello").Using(CustomComparers.TypeComparison), "Western European characters can be set when AllowNonWesternEuropeanCharacterForChargeDescription is true.");

			var chargeWithNonWesternEuropeanCharacterDisallowed = Factory.New<JobComInvChargeWithNonWesternEuropeanCharacterDisallowed>();
			chargeWithNonWesternEuropeanCharacterDisallowed.J7_ChargeDescription = "你好";
			NUnit.Framework.Assert.That(chargeWithNonWesternEuropeanCharacterDisallowed.J7_ChargeDescription, Is.EqualTo(ZString.Empty), "Non Western European characters can NOT be set when AllowNonWesternEuropeanCharacterForChargeDescription is true.");

			chargeWithNonWesternEuropeanCharacterDisallowed.J7_ChargeDescriptionInfo.ClearValue();
			chargeWithNonWesternEuropeanCharacterDisallowed.J7_ChargeDescription = "Hello";
			NUnit.Framework.Assert.That(chargeWithNonWesternEuropeanCharacterDisallowed.J7_ChargeDescription, Is.EqualTo("Hello").Using(CustomComparers.TypeComparison), "Western European characters can be set when AllowNonWesternEuropeanCharacterForChargeDescription is true.");
		}

		[ExpectNoExceptions]
		public void TestITypeDeciderContext()
		{
			CombineAssertions(() =>
			{
				testCharge.Parent = null;
				NUnit.Framework.Assert.That((testCharge as ITypeDeciderContext).Country, Is.EqualTo("AU"), "From CurrentCompany");

				testCharge.Parent = invoice;
				NUnit.Framework.Assert.That((testCharge as ITypeDeciderContext).Country, Is.EqualTo("XX"), "From Parent");
			});
		}

		[ExpectNoExceptions]
		public void TestDefaultDistributeBy()
		{
			NUnit.Framework.Assert.That(testCharge.J7_DistributeBy, Is.EqualTo("VAL").Using(CustomComparers.TypeComparison), "Default");
		}

		public void TestAmountInLocalCurrency()
		{
			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.Japan;
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exchangeRate.RE_StartDate = ZDateTime.Today;
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			exchangeRate.RE_SellRate = 1.5;
			exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;

			testCharge.J7_Amount = 300m;
			testCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Japan;

			AssertEquals(300m / 1.5m, testCharge.AmountInLocalCurrency);
		}

		public void TestJ7_Percentage() => CombineAssertions(() =>
		{
			var resourceData = DataBoundResourceStrings.GetDataForProperty(testCharge.J7_PercentageInfo);
			AssertEquals("Caption", "% of Line Price", resourceData.Caption);
			AssertEquals("ShortCaption", "% of Price", resourceData.ShortCaption);
			AssertEquals("FullDescription", "Is only used for some charges such as commission or discount. If you enter a value in this field it will be defaulted to every invoice line under group invoices or invoices and the amount is then calculated based on the line price.", resourceData.FullDescription);
		});

		public void TestJ7_IsIncludedInITOT() => CombineAssertions(() =>
		{
			var resourceData = DataBoundResourceStrings.GetDataForProperty(testCharge.J7_IsIncludedInITOTInfo);
			AssertEquals("Caption", "Included in Invoice Lines", resourceData.Caption);
			AssertEquals("ShortCaption", "Incl. in Inv.Lines?", resourceData.ShortCaption);
			AssertEquals("FullDescription", "Identify if this charge code is included in the invoice lines values or not. Entry of charge codes and this identifier allow the system to do value calculations. Example: CIF invoice, where OFT and ONS are NOT Included in Lines values, shows the lines are already at an FOB level, so the system disregards OFT and ONS in the calculation of the customs value.", resourceData.FullDescription);
		});

		#region Implementation

		JobComInvCharge testCharge;
		TestInvoice invoice;

		protected override void SetUp()
		{
			base.SetUp();
			invoice = Factory.New<TestInvoice>();
			invoice.Z0_Guid = Factory.New<TestDeclaration>().PK;
			invoice.IncoTerm = Constants.IncoTerms.FreeOnBoard;

			testCharge = invoice.Charges.AddNew();
		}

		void SetIsJ7_ExchangeRateUserEnterable(JobComInvCharge charge, bool value)
		{
			charge.IsJ7_ExchangeRateUserEnterable = value;
		}

		#endregion
	}

	class JobComInvChargeWithNonWesternEuropeanCharacterAllowed : JobComInvCharge
	{
		public JobComInvChargeWithNonWesternEuropeanCharacterAllowed(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZBool AllowNonWesternEuropeanCharacterForChargeDescription => true;
	}

	class JobComInvChargeWithNonWesternEuropeanCharacterDisallowed : JobComInvCharge
	{
		public JobComInvChargeWithNonWesternEuropeanCharacterDisallowed(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZBool AllowNonWesternEuropeanCharacterForChargeDescription => false;
	}
}
