using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CalculateInsuranceBizObj))]
	sealed class CalculateInsuranceBizObjTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAmountAndCurrency()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;

				var bizObj = new CalculateInsuranceBizObj(chargeFactory, invoice);
				AssertEquals("Empty - InsuranceAmount", 0m, bizObj.InsuranceAmount);
				AssertEquals("Empty - Currency", ZString.Empty, bizObj.Currency);

				invoice.JZ_InvoiceAmount = 100m;
				invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

				bizObj = new CalculateInsuranceBizObj(chargeFactory, invoice);

				bizObj.InsurancePercentage = 5m;

				AssertEquals("Entered - InsuranceAmount", 5.0m, bizObj.InsuranceAmount);
				AssertEquals("Entered - Currency", "EUR", bizObj.Currency);
			});
		}

		public void TestCalculate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;

			invoice.JZ_InvoiceAmount = 100;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var bizObj = new CalculateInsuranceBizObj(chargeFactory, invoice);
			bizObj.InsurancePercentage = 5;

			bizObj.Calculate();
			var charge = invoice.Charges.Single();
			CombineAssertionsForInvCharge("When default dutiable percent is 100", charge, 5.0m, true);

			bizObj.DutiablePercent = 0;
			bizObj.Calculate();

			charge = invoice.Charges.Single();
			CombineAssertionsForInvCharge("When dutiable percent is 0", charge, 5.0m, false);

			bizObj.DutiablePercent = 20;
			bizObj.Calculate();
			charge = invoice.Charges.FirstOrDefault(x => x.J7_IsDutiable);
			var charge_2 = invoice.Charges.FirstOrDefault(x => !x.J7_IsDutiable);

			AssertEquals("Number of added charges", 2, invoice.Charges.Count);

			CombineAssertionsForInvCharge("When dutiable percent is 20, first charge", charge, 1, true);

			CombineAssertionsForInvCharge("When dutiable percent is 20, second charge", charge_2, 4, false);

			void CombineAssertionsForInvCharge(string message, InvoiceCharge charge, ZDecimal expectedAmount, bool expectedIsDutiable)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals("Charge Amount", expectedAmount, charge.J7_Amount);
					AssertEquals("Charge Type", "ONS", charge.J7_ChargeType);
					AssertEquals("Is Dutiable", expectedIsDutiable, charge.J7_IsDutiable);
				});
			}
		}

		public void TestCalculate_IncludeInITOT()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;

			invoice.JZ_InvoiceAmount = 100;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var bizObj = new CalculateInsuranceBizObj(chargeFactory ,invoice);
			bizObj.InsurancePercentage = 5;

			_ = bizObj.Calculate();
			var charge = invoice.Charges.Single();
			AssertEquals("When checkbox is unchecked by default", false, charge.J7_IsIncludedInITOT);

			bizObj.IsInsuranceIncludedInLines = true;

			_ = bizObj.Calculate();
			charge = invoice.Charges.Single();
			AssertEquals("When checkbox is checked", true, charge.J7_IsIncludedInITOT);
		}

		[TestDate(2024, 11, 12)]
		public void TestGetFlatAmountFromCalculationRuleEngine()
		{
			(_, var declaration) = GetInsuranceData();
			var invoice = declaration.Invoices.AddNew();
			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;

			invoice.JZ_InvoiceAmount = 100;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var bizObj = new CalculateInsuranceBizObj(chargeFactory, invoice);
			AssertEquals("Flat Rate", 11m, bizObj.InsuranceAmount);
		}

		[TestDate(2024, 11, 12)]
		public void TestGetUpliftAmountFromCalculationRuleEngine()
		{
			(_, var declaration) = GetInsuranceData();
			var invoice = declaration.Invoices.AddNew();
			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;

			invoice.JZ_InvoiceAmount = 1001;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var bizObj = new CalculateInsuranceBizObj(chargeFactory, invoice);
			AssertEquals("Uplift Percent", 5.3m, bizObj.InsurancePercentage);
		}

		public void TestIsDutiablePercentEnabled()
		{
			(_, var declaration) = GetInsuranceData();
			var invoice = declaration.Invoices.AddNew();
			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;

			var bizObjMock = new Mock<CalculateInsuranceBizObj>(chargeFactory, invoice) { CallBase = true };

			bizObjMock.Object.DutiablePercent = 20;
			CombineAssertions("IsDutiablePercentEnabled is true", () =>
			{
				AssertEquals("By default", true, bizObjMock.Object.IsDutiablePercentEnabled);
				AssertEquals(20m, bizObjMock.Object.DutiablePercent);
			});

			bizObjMock.Setup(m => m.IsDutiablePercentEnabled).Returns(false);
			CombineAssertions("IsDutiablePercentEnabled is false", () =>
			{
				AssertEquals("DutiablePercent is always 100", 100m, bizObjMock.Object.DutiablePercent);
				AssertExceptionThrown<InvalidOperationException>("Not allowed to set DutiablePercent", () => bizObjMock.Object.DutiablePercent = 20);
			});
		}

		(CusCalculationRule, JobDeclaration) GetInsuranceData()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var insurance1 = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance1.CCR_StartDate = new ZDateTime(2024, 11, 01).ToOffset();
			insurance1.CCR_EndDate = new ZDateTime(2024, 11, 30).ToOffset();
			insurance1.CCR_OH_Importer = importer.PK;
			insurance1.CCR_TransportMode = TransportTypeList.Codes.Air;
			insurance1.CCR_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var rate = insurance1.CalculationRuleRateCollection.AddNew();
			rate.ValueFrom = 0;
			rate.FlatRate = 11;
			var rate2 = insurance1.CalculationRuleRateCollection.AddNew();
			rate2.ValueFrom = 1000;
			rate2.Uplift = 5.3;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			return (insurance1, declaration);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var chargeFactory = (EUIncoTermAndCustomsChargeFactory)declaration.IncoTermAndChargeFactory;
			var bizObj = new CalculateInsuranceBizObj(chargeFactory, invoice);
			return bizObj;
		}
	}
}
