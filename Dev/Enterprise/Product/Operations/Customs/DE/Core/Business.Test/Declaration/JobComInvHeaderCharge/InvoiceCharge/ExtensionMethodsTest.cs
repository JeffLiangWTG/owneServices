using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class ExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetIncoTermChargeFactoryCacheKey_JobDeclaration()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "IMP", declaration.GetIncoTermChargeFactoryCacheKey());
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export", ZString.Empty, declaration.GetIncoTermChargeFactoryCacheKey());
			});
		}

		public void TestGetIncoTermChargeFactoryCacheKey_JobComInvoiceGroupHeader()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "IMP", groupInvoice.GetIncoTermChargeFactoryCacheKey());
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export", ZString.Empty, groupInvoice.GetIncoTermChargeFactoryCacheKey());
			});
		}

		public void TestGetIncoTermChargeFactoryCacheKey_JobComInvoiceHeader()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Import", "IMP", invoice.GetIncoTermChargeFactoryCacheKey());
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Export", ZString.Empty, invoice.GetIncoTermChargeFactoryCacheKey());
			});
		}

		public void TestHasChargeCodeWithDifferentCurrency_JobComInvoiceGroupHeader()
		{
			CombineAssertions(() =>
			{
				var groupCharge1 = groupInvoice.Charges.AddNew();
				groupCharge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				groupCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertEquals("First Charge", false, groupInvoice.HasChargeCodeWithDifferentCurrency(groupCharge1.J7_ChargeType, groupCharge1.J7_RX_NKCurrency));

				var groupCharge2 = groupInvoice.Charges.AddNew();
				groupCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				AssertEquals("Same charge code with empty currency", false, groupInvoice.HasChargeCodeWithDifferentCurrency(groupCharge2.J7_ChargeType, groupCharge2.J7_RX_NKCurrency));

				groupCharge2.J7_ChargeType = ZString.Empty;
				groupCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
				AssertEquals("Empty charge code with different currency", false, groupInvoice.HasChargeCodeWithDifferentCurrency(groupCharge2.J7_ChargeType, groupCharge2.J7_RX_NKCurrency));

				groupCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				AssertEquals("Same charge code with different currency", true, groupInvoice.HasChargeCodeWithDifferentCurrency(groupCharge2.J7_ChargeType, groupCharge2.J7_RX_NKCurrency));

				groupCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertEquals("Same charge code with same currency", false, groupInvoice.HasChargeCodeWithDifferentCurrency(groupCharge2.J7_ChargeType, groupCharge2.J7_RX_NKCurrency));

				groupCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
				groupCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
				AssertEquals("Different charge code with different currency", false, groupInvoice.HasChargeCodeWithDifferentCurrency(groupCharge2.J7_ChargeType, groupCharge2.J7_RX_NKCurrency));
			});
		}

		public void TestHasChargeCodeWithDifferentCurrency_JobComInvoiceHeader()
		{
			var headerCharge1 = invoice.Charges.AddNew();
			headerCharge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			headerCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			AssertEquals("First Charge", false, invoice.HasChargeCodeWithDifferentCurrency(headerCharge1.J7_ChargeType, headerCharge1.J7_RX_NKCurrency));

			var headerCharge2 = invoice.Charges.AddNew();
			headerCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("Same charge code with empty currency", false, invoice.HasChargeCodeWithDifferentCurrency(headerCharge2.J7_ChargeType, headerCharge2.J7_RX_NKCurrency));

			headerCharge2.J7_ChargeType = ZString.Empty;
			headerCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertEquals("Empty charge code with different currency", false, invoice.HasChargeCodeWithDifferentCurrency(headerCharge2.J7_ChargeType, headerCharge2.J7_RX_NKCurrency));

			headerCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("Same charge code with different currency", true, invoice.HasChargeCodeWithDifferentCurrency(headerCharge2.J7_ChargeType, headerCharge2.J7_RX_NKCurrency));

			headerCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			AssertEquals("Same charge code with same currency", false, invoice.HasChargeCodeWithDifferentCurrency(headerCharge2.J7_ChargeType, headerCharge2.J7_RX_NKCurrency));

			headerCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			headerCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertEquals("Different charge code with different currency", false, invoice.HasChargeCodeWithDifferentCurrency(headerCharge2.J7_ChargeType, headerCharge2.J7_RX_NKCurrency));
		}

		public void TestHasChargeCodeWithDifferentCurrency_JobComInvoiceLine()
		{
			var lineCharge1 = invoiceLine.Charges.AddNew();
			lineCharge1.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			lineCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			AssertEquals("First Charge", false, invoiceLine.HasChargeCodeWithDifferentCurrency(lineCharge1.J7_ChargeType, lineCharge1.J7_RX_NKCurrency));

			var lineCharge2 = invoiceLine.Charges.AddNew();
			lineCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("Same charge code with empty currency", false, invoiceLine.HasChargeCodeWithDifferentCurrency(lineCharge2.J7_ChargeType, lineCharge2.J7_RX_NKCurrency));

			lineCharge2.J7_ChargeType = ZString.Empty;
			lineCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertEquals("Empty charge code with different currency", false, invoiceLine.HasChargeCodeWithDifferentCurrency(lineCharge2.J7_ChargeType, lineCharge2.J7_RX_NKCurrency));

			lineCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			AssertEquals("Same charge code with different currency", true, invoiceLine.HasChargeCodeWithDifferentCurrency(lineCharge2.J7_ChargeType, lineCharge2.J7_RX_NKCurrency));

			lineCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			AssertEquals("Same charge code with same currency", false, invoiceLine.HasChargeCodeWithDifferentCurrency(lineCharge2.J7_ChargeType, lineCharge2.J7_RX_NKCurrency));

			lineCharge2.J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
			lineCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Australia;
			AssertEquals("Different charge code with different currency", false, invoiceLine.HasChargeCodeWithDifferentCurrency(lineCharge2.J7_ChargeType, lineCharge2.J7_RX_NKCurrency));
		}

		public void TestSupportsIATA()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var headerCharge = invoice.Charges.AddNew();
			var supportedChargeCodes = new[] { ImportChargeCodeList.Codes._010, ImportChargeCodeList.Codes._011, ImportChargeCodeList.Codes._014, ImportChargeCodeList.Codes.AIR };
			var unsupportedChargeCodes = headerCharge.Lookups.AllChargeTypeList.GetAllCodes().Except(supportedChargeCodes).ToArray();

			CombineAssertions(() =>
			{
				foreach (ZString chargeCode in supportedChargeCodes)
				{
					AssertEquals($"Charge code {chargeCode}", true, chargeCode.SupportsIATA());
				}

				foreach (ZString chargeCode in unsupportedChargeCodes)
				{
					AssertEquals($"Charge code {chargeCode}", false, chargeCode.SupportsIATA());
				}
			});
		}

		public void TestHasChargesWithCode_JobComInvoiceHeader()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No charges", false, invoice.HasChargesWithCode(CustomsChargeTypeList.Codes.LandingCharges));

				invoice.Charges.AddNew();
				AssertEquals("Added charge, no code", false, invoice.HasChargesWithCode(CustomsChargeTypeList.Codes.LandingCharges));

				invoice.Charges.AddNew().J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				AssertEquals("Added charge, code OFT", false, invoice.HasChargesWithCode(CustomsChargeTypeList.Codes.LandingCharges));

				invoice.Charges.AddNew().J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
				AssertEquals("Added charge, code LCH", true, invoice.HasChargesWithCode(CustomsChargeTypeList.Codes.LandingCharges));
			});
		}

		public void TestHasChargesWithCode_JobComInvoiceGroupHeader()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No charges", false, groupInvoice.HasChargesWithCode(CustomsChargeTypeList.Codes.LandingCharges));

				groupInvoice.Charges.AddNew();
				AssertEquals("Added charge, no code", false, groupInvoice.HasChargesWithCode(CustomsChargeTypeList.Codes.LandingCharges));

				groupInvoice.Charges.AddNew().J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				AssertEquals("Added charge, code OFT", false, groupInvoice.HasChargesWithCode(CustomsChargeTypeList.Codes.LandingCharges));

				groupInvoice.Charges.AddNew().J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
				AssertEquals("Added charge, code LCH", true, groupInvoice.HasChargesWithCode(CustomsChargeTypeList.Codes.LandingCharges));
			});
		}

		public void TestHasChargesWithCode_JobComInvoiceLine()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No charges", false, invoiceLine.HasChargesWithCode(CustomsChargeTypeList.Codes.LandingCharges));

				invoiceLine.Charges.AddNew();
				AssertEquals("Added charge, no code", false, invoiceLine.HasChargesWithCode(CustomsChargeTypeList.Codes.LandingCharges));

				invoiceLine.Charges.AddNew().J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
				AssertEquals("Added charge, code OFT", false, invoiceLine.HasChargesWithCode(CustomsChargeTypeList.Codes.LandingCharges));

				invoiceLine.Charges.AddNew().J7_ChargeType = CustomsChargeTypeList.Codes.LandingCharges;
				AssertEquals("Added charge, code LCH", true, invoiceLine.HasChargesWithCode(CustomsChargeTypeList.Codes.LandingCharges));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			groupInvoice = declaration.JobComInvoiceGroupHeaders[0];
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceGroupHeader groupInvoice;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
