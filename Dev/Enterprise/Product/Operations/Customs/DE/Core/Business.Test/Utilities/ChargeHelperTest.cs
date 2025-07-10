using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using Codes = Enterprise.Customs.DE.Business.ImportChargeCodeList.Codes;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ChargeHelperTest : TestCaseWithFactory
	{
		public void TestIsJ7_IsDutiable_ReadOnlyRate()
		{
			CombineAssertions(() =>
			{
				foreach (var chargeCode in new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
							Codes._011, Codes._012, Codes._014, Codes._015, Codes._016, Codes._017, Codes._019, Codes.AIR, Common.CustomsChargeTypeList.Codes.Discount,
							Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE })
				{
					AssertEquals($"ChargeCode is {chargeCode}", true, ChargeHelper.IsJ7_IsDutiable_ReadOnlyRate(chargeCode));
				}
			});
		}

		public void TestIsJ7_IsStatisticalValueApplicable_ReadOnlyRate()
		{
			CombineAssertions(() =>
			{
				foreach (var chargeCode in new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
							Codes._011, Codes._012, Codes._014, Codes._015, Codes._016, Codes._017, Codes._019, Codes.AIR, Common.CustomsChargeTypeList.Codes.Discount,
							Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE })
				{
					AssertEquals($"ChargeCode is {chargeCode}", true, ChargeHelper.IsJ7_IsStatisticalValueApplicable_ReadOnlyRate(chargeCode));
				}
			});
		}

		public void TestIsJ7_IsGSTApplicable_ReadOnlyRate()
		{
			CombineAssertions(() =>
			{
				foreach (var chargeCode in new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
							Codes._011, Codes._012, Codes._014, Codes._015, Codes._016, Codes._017, Codes._019, Codes.AIR, Common.CustomsChargeTypeList.Codes.Discount,
							Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE })
				{
					AssertEquals($"ChargeCode is {chargeCode}", true, ChargeHelper.IsJ7_IsGSTApplicable_ReadOnlyRate(chargeCode));
				}
			});
		}

		public void TestIsJ7_Calc_IsIncludedInInvoiceAmount_ReadOnlyRate()
		{
			CombineAssertions(() =>
			{
				foreach (var chargeCode in new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
						Codes._011, Codes._012, Codes._014, Codes._015, Codes._016, Codes._017, Codes._019, Codes.AIR,
						Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE })
				{
					AssertEquals($"ChargeCode is {chargeCode}", true, ChargeHelper.IsJ7_Calc_IsIncludedInInvoiceAmount_ReadOnlyRate(chargeCode));
				}
			});
		}

		public void TestIsJ7_IsIncludedInITOT_ReadOnlyRate()
		{
			CombineAssertions(() =>
			{
				foreach (var chargeCode in new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
						Codes._011, Codes._012, Codes._014, Codes._015, Codes._016, Codes._017, Codes._019, Codes.AIR,
						Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE })
				{
					AssertEquals($"ChargeCode is {chargeCode}", true, ChargeHelper.IsJ7_IsIncludedInITOT_ReadOnlyRate(chargeCode));
				}
			});
		}

		public void TestIsJ7_ExchangeRateIATA_ReadOnlyRate()
		{
			CombineAssertions(() =>
			{
				foreach (var chargeCode in new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009,
						Codes._012, Codes._015, Codes._016, Codes._017, Codes._019, Common.CustomsChargeTypeList.Codes.Discount,
						Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE })
				{
					AssertEquals($"ChargeCode is {chargeCode}", true, ChargeHelper.IsJ7_ExchangeRateIATA_ReadOnlyRate(chargeCode));
				}
			});
		}

		public void TestIsJ7_Percentage_ReadOnlyRate()
		{
			CombineAssertions(() =>
			{
				foreach (var chargeCode in new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009,
			Codes._011, Codes._012, Codes._015, Codes._016, Codes._017, Codes._019, Codes.AIR, Common.CustomsChargeTypeList.Codes.Discount,
			Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE })
				{
					AssertEquals($"ChargeCode is {chargeCode}", true, ChargeHelper.IsJ7_Percentage_ReadOnlyRate(chargeCode));
				}
			});
		}

		public void TestIsJ7_IsIncludedInITOT_NoValidation()
		{
			CombineAssertions(() =>
			{
				foreach (var chargeCode in new ZString[] { Codes._014, Codes._015, Codes._016, Codes._017 })
				{
					AssertEquals($"ChargeCode is {chargeCode}", true, ChargeHelper.IsJ7_IsIncludedInITOT_NoValidation(chargeCode));
				}
			});
		}

		public void TestSetValuesIfNeeded_J7_IsDutiable()
		{
			AssertSetValuesIfNeeded(new[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
											Codes._011, Codes._012,Codes._019,Codes.AIR,
											Codes.INP }, "J7_IsDutiable");
		}

		public void TestSetValuesIfNeeded_J7_IsDutiable_IsFreightChargeToEUBorderByAirInsideEU()
		{
			AssertSetValuesIfNeeded_IsFreightChargeToEUBorderByAirInsideEU("J7_IsDutiable");
		}

		public void TestSetValuesIfNeeded_J7_IsStatisticalValueApplicable()
		{
			AssertSetValuesIfNeeded(new[] { Codes._001,Codes._002,Codes._003,Codes._004,Codes._005,Codes._006,Codes._007,Codes._008,Codes._009,Codes._010,
											Codes._011,Codes._012,Codes._019,Codes.AIR,
											Codes.INP, EU.Business.ChargeTypeList.Codes.StatisticalValue }, "J7_IsStatisticalValueApplicable");
		}

		public void TestSetValuesIfNeeded_J7_IsStatisticalValueApplicable_IsFreightChargeToEUBorderByAirInsideEU()
		{
			AssertSetValuesIfNeeded_IsFreightChargeToEUBorderByAirInsideEU("J7_IsStatisticalValueApplicable");
		}

		public void TestSetValuesIfNeeded_J7_IsGSTApplicable()
		{
			AssertSetValuesIfNeeded(new[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
											Codes._011, Codes._012, Codes._014, Codes._019, Codes.AIR,
											Codes.INP ,Codes.TCE }, "J7_IsGSTApplicable");
		}

		public void TestSetValuesIfNeeded_J7_IsGSTApplicable_IsFreightChargeToEUBorderByAirInsideEU()
		{
			AssertSetValuesIfNeeded_IsFreightChargeToEUBorderByAirInsideEU("J7_IsGSTApplicable");
		}

		public void TestSetValuesIfNeeded_J7_Calc_IsIncludedInInvoiceAmount()
		{
			AssertSetValuesIfNeeded(new[] { Codes._014, Codes._015, Codes._016, Codes._017 }, "J7_Calc_IsIncludedInInvoiceAmount", true);
		}

		public void TestSetValuesIfNeeded_J7_Calc_IsIncludedInInvoiceAmount_IsFreightChargeAfterEUBorderByAirOutsideEU()
		{
			AssertSetValuesIfNeeded_IsFreightChargeAfterEUBorderByAirOutsideEU("J7_Calc_IsIncludedInInvoiceAmount", true);
		}

		public void TestSetValuesIfNeeded_J7_IsIncludedInITOT()
		{
			AssertSetValuesIfNeeded(new[] { Codes._014, Codes._015, Codes._016, Codes._017 }, "J7_IsIncludedInITOT");
		}

		public void TestSetValuesIfNeeded_J7_IsIncludedInITOT_IsFreightChargeAfterEUBorderByAirOutsideEU()
		{
			AssertSetValuesIfNeeded_IsFreightChargeAfterEUBorderByAirOutsideEU("J7_IsIncludedInITOT");
		}

		public void TestSetValuesIfNeeded_IsJ7_ExchangeRateIATA()
		{
			AssertSetValuesIfNeeded(new[] { Codes.AIR }, "IsJ7_ExchangeRateIATA");
		}

		public void TestIsFreightChargeToEUBorderByAirInsideEU()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoice = declaration.Invoices.AddNew();
			invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._3;
			var invoiceCharge = invoice.Charges.AddNew(Codes._010);
			var invoiceApportionCharge = invoice.GroupCharges.AddNew(Codes._010);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLineCharge = invoiceLine.Charges.AddNew(Codes._010);
			var invoiceLineApportionCharge = invoiceLine.ApportionedCharges.AddNew(Codes._010);
			var groupInvoiceCharge = declaration.TopGroupInvoice.Charges.AddNew(Codes._010);

			CombineAssertions(() =>
			{
				AssertEquals("InvoiceCharge IsFreightChargeToEUBorderByAirInsideEU", true, invoiceCharge.IsFreightChargeToEUBorderByAirInsideEU());
				AssertEquals("InvoiceLineCharge IsFreightChargeToEUBorderByAirInsideEU", true, invoiceLineCharge.IsFreightChargeToEUBorderByAirInsideEU());
				AssertEquals("GroupInvoiceCharge IsFreightChargeToEUBorderByAirInsideEU", true, groupInvoiceCharge.IsFreightChargeToEUBorderByAirInsideEU());
				AssertEquals("InvoiceApportionCharge IsFreightChargeToEUBorderByAirInsideEU", true, invoiceApportionCharge.IsFreightChargeToEUBorderByAirInsideEU());
				AssertEquals("InvoiceLineApportionCharge IsFreightChargeToEUBorderByAirInsideEU", true, invoiceLineApportionCharge.IsFreightChargeToEUBorderByAirInsideEU());

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("InvoiceCharge, Invalid JE_TransportMode", false, invoiceCharge.IsFreightChargeToEUBorderByAirInsideEU());
				AssertEquals("InvoiceLineCharge, Invalid JE_TransportMode", false, invoiceLineCharge.IsFreightChargeToEUBorderByAirInsideEU());
				AssertEquals("GroupInvoiceCharge, Invalid JE_TransportMode", false, groupInvoiceCharge.IsFreightChargeToEUBorderByAirInsideEU());
				AssertEquals("InvoiceApportionCharge, Invalid JE_TransportMode", false, invoiceApportionCharge.IsFreightChargeToEUBorderByAirInsideEU());
				AssertEquals("InvoiceLineApportionCharge, Invalid JE_TransportMode", false, invoiceLineApportionCharge.IsFreightChargeToEUBorderByAirInsideEU());

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._1;
				AssertEquals("InvoiceCharge, Invalid ZG_AgreedPlaceCode", false, invoiceCharge.IsFreightChargeToEUBorderByAirInsideEU());
				AssertEquals("InvoiceLineCharge, Invalid ZG_AgreedPlaceCode", false, invoiceLineCharge.IsFreightChargeToEUBorderByAirInsideEU());
				AssertEquals("GroupInvoiceCharge, Invalid ZG_AgreedPlaceCode", false, groupInvoiceCharge.IsFreightChargeToEUBorderByAirInsideEU());
				AssertEquals("InvoiceApportionCharge, Invalid ZG_AgreedPlaceCode", false, invoiceApportionCharge.IsFreightChargeToEUBorderByAirInsideEU());
				AssertEquals("InvoiceLineApportionCharge, Invalid ZG_AgreedPlaceCode", false, invoiceLineApportionCharge.IsFreightChargeToEUBorderByAirInsideEU());

				invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._3;
				invoiceCharge.J7_ChargeType = Codes._011;
				AssertEquals("InvoiceCharge, Invalid J7_ChargeType", false, invoiceCharge.IsFreightChargeToEUBorderByAirInsideEU());
				invoiceLineCharge.J7_ChargeType = Codes._011;
				AssertEquals("InvoiceLineCharge, Invalid J7_ChargeType", false, invoiceLineCharge.IsFreightChargeToEUBorderByAirInsideEU());
				groupInvoiceCharge.J7_ChargeType = Codes._011;
				AssertEquals("GroupInvoiceCharge, Invalid J7_ChargeType", false, groupInvoiceCharge.IsFreightChargeToEUBorderByAirInsideEU());
				invoiceApportionCharge.J7_ChargeType = Codes._011;
				AssertEquals("InvoiceApportionCharge, Invalid ZG_AgreedPlaceCode", false, invoiceApportionCharge.IsFreightChargeToEUBorderByAirInsideEU());
				invoiceLineApportionCharge.J7_ChargeType = Codes._011;
				AssertEquals("InvoiceLineApportionCharge, Invalid ZG_AgreedPlaceCode", false, invoiceLineApportionCharge.IsFreightChargeToEUBorderByAirInsideEU());
			});
		}

		public void TestIsFreightChargeAfterEUBorderByAirOutsideEU()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoice = declaration.Invoices.AddNew();
			invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._1;
			var invoiceCharge = invoice.Charges.AddNew(Codes._014);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLineCharge = invoiceLine.Charges.AddNew(Codes._014);
			var groupInvoiceCharge = declaration.TopGroupInvoice.Charges.AddNew(Codes._014);

			CombineAssertions(() =>
			{
				AssertEquals("InvoiceCharge IsFreightChargeAfterEUBorderByAirOutsideEU", true, invoiceCharge.IsFreightChargeAfterEUBorderByAirOutsideEU());
				AssertEquals("InvoiceLineCharge IsFreightChargeAfterEUBorderByAirOutsideEU", true, invoiceLineCharge.IsFreightChargeAfterEUBorderByAirOutsideEU());
				AssertEquals("GroupInvoiceCharge IsFreightChargeAfterEUBorderByAirOutsideEU", true, groupInvoiceCharge.IsFreightChargeAfterEUBorderByAirOutsideEU());

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("InvoiceCharge, Invalid JE_TransportMode", false, invoiceCharge.IsFreightChargeAfterEUBorderByAirOutsideEU());
				AssertEquals("InvoiceLineCharge, Invalid JE_TransportMode", false, invoiceLineCharge.IsFreightChargeAfterEUBorderByAirOutsideEU());
				AssertEquals("GroupInvoiceCharge, Invalid JE_TransportMode", false, groupInvoiceCharge.IsFreightChargeAfterEUBorderByAirOutsideEU());

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._3;
				AssertEquals("InvoiceCharge, Invalid ZG_AgreedPlaceCode", false, invoiceCharge.IsFreightChargeAfterEUBorderByAirOutsideEU());
				AssertEquals("InvoiceLineCharge, Invalid ZG_AgreedPlaceCode", false, invoiceLineCharge.IsFreightChargeAfterEUBorderByAirOutsideEU());
				AssertEquals("GroupInvoiceCharge, Invalid ZG_AgreedPlaceCode", false, groupInvoiceCharge.IsFreightChargeAfterEUBorderByAirOutsideEU());

				invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._1;
				invoiceCharge.J7_ChargeType = Codes._011;
				AssertEquals("InvoiceCharge, Invalid J7_ChargeType", false, invoiceCharge.IsFreightChargeAfterEUBorderByAirOutsideEU());
				invoiceLineCharge.J7_ChargeType = Codes._011;
				AssertEquals("InvoiceLineCharge, Invalid J7_ChargeType", false, invoiceLineCharge.IsFreightChargeAfterEUBorderByAirOutsideEU());
				groupInvoiceCharge.J7_ChargeType = Codes._011;
				AssertEquals("GroupInvoiceCharge, Invalid J7_ChargeType", false, groupInvoiceCharge.IsFreightChargeAfterEUBorderByAirOutsideEU());
			});
		}

		public void TestGetCustomsValueAdditionDeductionChargesForImport_TransportModeAIR()
		{
			var invoiceLine = PrepareAdditionDeductionChargesAndRelatedBOs(TransportTypeList.Codes.Air);
			var result = invoiceLine.GetCustomsValueAdditionDeductionChargesForImport();
			AssertContainsExactElementsInAnyOrder(chargesTypesForTest.Except(new[] { ImportChargeCodeList.Codes._010, ImportChargeCodeList.Codes._014, ImportChargeCodeList.Codes.TCE }), result.Select(x => x.J7_ChargeType));
		}

		public void TestGetCustomsValueAdditionDeductionChargesForImport_TransportModeNotAIR()
		{
			var invoiceLine = PrepareAdditionDeductionChargesAndRelatedBOs(TransportTypeList.Codes.Road);
			var result = invoiceLine.GetCustomsValueAdditionDeductionChargesForImport();
			AssertContainsExactElementsInAnyOrder(chargesTypesForTest.Except(ImportChargeCodeList.Codes.TCE), result.Select(x => x.J7_ChargeType));
		}

		public void TestGetCustomsValueAdditionDeductionApportionedChargesForImport_TransportModeAIR()
		{
			var invoiceLine = PrepareAdditionDeductionChargesAndRelatedBOs(TransportTypeList.Codes.Air);
			var result = invoiceLine.GetCustomsValueAdditionDeductionApportionedChargesForImport();
			AssertContainsExactElementsInAnyOrder(chargesTypesForTest.Except(new[] { ImportChargeCodeList.Codes._010, ImportChargeCodeList.Codes._014, ImportChargeCodeList.Codes.TCE }), result.Select(x => x.J7_ChargeType));
		}

		public void TestGetCustomsValueAdditionDeductionApportionedChargesForImport_TransportModeNotAIR()
		{
			var invoiceLine = PrepareAdditionDeductionChargesAndRelatedBOs(TransportTypeList.Codes.Road);
			var result = invoiceLine.GetCustomsValueAdditionDeductionApportionedChargesForImport();
			AssertContainsExactElementsInAnyOrder(chargesTypesForTest.Except(ImportChargeCodeList.Codes.TCE), result.Select(x => x.J7_ChargeType));
		}

		public void TestGetCustomsValueAirFreightCostsChargesForImport()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			CreateCharges(invoiceLine);
			var result = invoiceLine.GetCustomsValueAirFreightCostsChargesForImport();
			AssertContainsExactElementsInAnyOrder(new[] { ImportChargeCodeList.Codes._010, ImportChargeCodeList.Codes._014 }, result.Select(x => x.J7_ChargeType));
		}

		public void TestGetCustomsValueAirFreightCostsApportionedChargesForImport()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			foreach (var type in chargesTypesForTest)
			{
				var charge = invoiceLine.ApportionedCharges.AddNew();
				charge.J7_ChargeType = type;
			}
			var result = invoiceLine.GetCustomsValueAirFreightCostsApportionedChargesForImport();
			AssertContainsExactElementsInAnyOrder(new[] { ImportChargeCodeList.Codes._010, ImportChargeCodeList.Codes._014 }, result.Select(x => x.J7_ChargeType));
		}

		public void TestSetValuesForApportionChargesIfNeeded_GroupInvoiceCharge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10000m;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 25000m;
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 10000m;

			var groupCharge = invoice.GroupHeader.Charges.AddNew(Codes._010, 1000m, declaration.LocalCurrencyCode);
			groupCharge.IsJ7_ExchangeRateIATA = true;
			groupCharge.J7_ExchangeRate = 1.0066m;
			var groupCharge2 = invoice.GroupHeader.Charges.AddNew(Codes._011, 1100m, declaration.LocalCurrencyCode);
			groupCharge2.IsJ7_ExchangeRateIATA = true;
			groupCharge2.J7_ExchangeRate = 1.0066m;
			var groupCharge3 = invoice.GroupHeader.Charges.AddNew(Codes._014, 1200m, declaration.LocalCurrencyCode);
			groupCharge3.J7_ExchangeRate = 0.9778m;
			var groupCharge4 = invoice.GroupHeader.Charges.AddNew(Codes._012, 1300m, declaration.LocalCurrencyCode);

			CombineAssertions(() =>
			{
				declaration.ResumeApportionment();
				groupCharge.SetValuesForApportionChargesIfNeeded();
				var invoiceCharge010ForGroupCharge = invoice.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == Codes._010);
				AssertEquals("invoiceCharge010ForGroupCharge.IsJ7_ExchangeRateIATA", true, invoiceCharge010ForGroupCharge.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceCharge010ForGroupCharge.J7_ExchangeRate", 1.0066m, invoiceCharge010ForGroupCharge.J7_ExchangeRate);
				var invoice2Charge010ForGroupCharge = invoice2.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == Codes._010);
				AssertEquals("invoice2Charge010ForGroupCharge.IsJ7_ExchangeRateIATA", true, invoice2Charge010ForGroupCharge.IsJ7_ExchangeRateIATA);
				AssertEquals("invoice2Charge010ForGroupCharge.J7_ExchangeRate", 1.0066m, invoice2Charge010ForGroupCharge.J7_ExchangeRate);
				var invoiceLineCharge010ForGroupCharge = invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._010);
				AssertEquals("invoiceLineCharge010ForGroupCharge.IsJ7_ExchangeRateIATA", true, invoiceLineCharge010ForGroupCharge.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceLineCharge010ForGroupCharge.J7_ExchangeRate", 1.0066m, invoiceLineCharge010ForGroupCharge.J7_ExchangeRate);
				var invoiceLine2Charge010ForGroupCharge = invoiceLine2.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._010);
				AssertEquals("invoiceLine2Charge010ForGroupCharge.IsJ7_ExchangeRateIATA", true, invoiceLine2Charge010ForGroupCharge.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceLine2Charge010ForGroupCharge.J7_ExchangeRate", 1.0066m, invoiceLine2Charge010ForGroupCharge.J7_ExchangeRate);
				var invoiceLine3Charge010ForGroupCharge = invoiceLine3.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._010);
				AssertEquals("invoiceLine3Charge010ForGroupCharge.IsJ7_ExchangeRateIATA", true, invoiceLine3Charge010ForGroupCharge.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceLine3Charge010ForGroupCharge.J7_ExchangeRate", 1.0066m, invoiceLine3Charge010ForGroupCharge.J7_ExchangeRate);

				groupCharge2.SetValuesForApportionChargesIfNeeded();
				var invoiceCharge010ForGroupCharge2 = invoice.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == Codes._010);
				AssertEquals("invoiceCharge010ForGroupCharge2.IsJ7_ExchangeRateIATA", true, invoiceCharge010ForGroupCharge2.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceCharge010ForGroupCharge2.J7_ExchangeRate", 1.0066m, invoiceCharge010ForGroupCharge2.J7_ExchangeRate);
				var invoiceCharge011ForGroupCharge2 = invoice.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == Codes._011);
				AssertEquals("invoiceCharge011ForGroupCharge2.IsJ7_ExchangeRateIATA", true, invoiceCharge011ForGroupCharge2.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceCharge011ForGroupCharge2.J7_ExchangeRate", 1.0066m, invoiceCharge011ForGroupCharge2.J7_ExchangeRate);
				var invoiceLineCharge010ForGroupCharge2 = invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._010);
				AssertEquals("invoiceLineCharge010ForGroupCharge2.IsJ7_ExchangeRateIATA", true, invoiceLineCharge010ForGroupCharge2.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceLineCharge010ForGroupCharge2.J7_ExchangeRate", 1.0066m, invoiceLineCharge010ForGroupCharge2.J7_ExchangeRate);
				var invoiceLineCharge011ForGroupCharge2 = invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._011);
				AssertEquals("invoiceLineCharge011ForGroupCharge2.IsJ7_ExchangeRateIATA", true, invoiceLineCharge011ForGroupCharge2.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceLineCharge011ForGroupCharge2.J7_ExchangeRate", 1.0066m, invoiceLineCharge011ForGroupCharge2.J7_ExchangeRate);

				groupCharge3.SetValuesForApportionChargesIfNeeded();
				var invoiceCharge010ForGroupCharge3 = invoice.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == Codes._010);
				AssertEquals("invoiceCharge010ForGroupCharge3.IsJ7_ExchangeRateIATA", true, invoiceCharge010ForGroupCharge3.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceCharge010ForGroupCharge3.J7_ExchangeRate", 1.0066m, invoiceCharge010ForGroupCharge3.J7_ExchangeRate);
				var invoiceCharge014ForGroupCharge3 = invoice.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == Codes._014);
				AssertEquals("invoiceCharge014ForGroupCharge3.IsJ7_ExchangeRateIATA", false, invoiceCharge014ForGroupCharge3.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceCharge014ForGroupCharge3.J7_ExchangeRate", 0.9778m, invoiceCharge014ForGroupCharge3.J7_ExchangeRate);
				var invoiceLineCharge010ForGroupCharge3 = invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._010);
				AssertEquals("invoiceLineCharge010ForGroupCharge3.IsJ7_ExchangeRateIATA", true, invoiceLineCharge010ForGroupCharge3.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceLineCharge010ForGroupCharge3.J7_ExchangeRate", 1.0066m, invoiceLineCharge010ForGroupCharge3.J7_ExchangeRate);
				var invoiceLineCharge014ForGroupCharge3 = invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._014);
				AssertEquals("invoiceLineCharge014ForGroupCharge3.IsJ7_ExchangeRateIATA", false, invoiceLineCharge014ForGroupCharge3.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceLineCharge014ForGroupCharge3.J7_ExchangeRate", 0.9778m, invoiceLineCharge014ForGroupCharge3.J7_ExchangeRate);

				groupCharge4.SetValuesForApportionChargesIfNeeded();
				var invoiceCharge010ForGroupCharge4 = invoice.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == Codes._010);
				AssertEquals("invoiceCharge010ForGroupCharge4.IsJ7_ExchangeRateIATA", true, invoiceCharge010ForGroupCharge4.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceCharge010ForGroupCharge4.J7_ExchangeRate", 1.0066m, invoiceCharge010ForGroupCharge4.J7_ExchangeRate);
				var invoiceCharge012ForGroupCharge4 = invoice.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == Codes._012);
				AssertEquals("invoiceCharge012ForGroupCharge4.IsJ7_ExchangeRateIATA", false, invoiceCharge012ForGroupCharge4.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceCharge012ForGroupCharge4.J7_ExchangeRate", 1m, invoiceCharge012ForGroupCharge4.J7_ExchangeRate);
				var invoiceLineCharge010ForGroupCharge4 = invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._010);
				AssertEquals("invoiceLineCharge010ForGroupCharge4.IsJ7_ExchangeRateIATA", true, invoiceLineCharge010ForGroupCharge4.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceLineCharge010ForGroupCharge4.J7_ExchangeRate", 1.0066m, invoiceLineCharge010ForGroupCharge4.J7_ExchangeRate);
				var invoiceLineCharge012ForGroupCharge4 = invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._012);
				AssertEquals("invoiceLineCharge012ForGroupCharge4.IsJ7_ExchangeRateIATA", false, invoiceLineCharge012ForGroupCharge4.IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceLineCharge012ForGroupCharge4.J7_ExchangeRate", 1m, invoiceLineCharge012ForGroupCharge4.J7_ExchangeRate);
			});
		}

		public void TestSetValuesForApportionChargesIfNeeded_InvoiceCharge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10000m;

			var invoiceCharge = invoice.Charges.AddNew(Codes._010, 1000m, declaration.LocalCurrencyCode);
			invoiceCharge.IsJ7_ExchangeRateIATA = true;
			var invoiceCharge2 = invoice.Charges.AddNew(Codes._014, 1100m, declaration.LocalCurrencyCode);

			CombineAssertions(() =>
			{
				declaration.ResumeApportionment();
				invoiceCharge.SetValuesForApportionChargesIfNeeded();
				AssertEquals("invoiceLineCharge010ForInvoiceCharge", true, invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._010).IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceLine2Charge010ForInvoiceCharge", true, invoiceLine2.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._010).IsJ7_ExchangeRateIATA);

				invoiceCharge2.SetValuesForApportionChargesIfNeeded();
				AssertEquals("invoiceLineCharge010ForInvoiceCharge2", true, invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._010).IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceLine2Charge010ForInvoiceCharge2", false, invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Single(x => x.J7_ChargeType == Codes._014).IsJ7_ExchangeRateIATA);
			});
		}

		public void TestSetValuesForApportionChargesIfNeeded_InvoiceLineCharge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			var invoiceLineCharge = invoiceLine.Charges.AddNew(Codes._010, 1000m, declaration.LocalCurrencyCode);
			invoiceLineCharge.IsJ7_ExchangeRateIATA = true;
			var invoiceLineCharge2 = invoiceLine.Charges.AddNew(Codes._014, 1100m, declaration.LocalCurrencyCode);

			CombineAssertions(() =>
			{
				declaration.ResumeApportionment();
				invoiceLineCharge.SetValuesForApportionChargesIfNeeded();
				AssertEquals("invoiceCharge010ForInvoiceLineCharge", true, invoice.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == Codes._010).IsJ7_ExchangeRateIATA);

				invoiceLineCharge2.SetValuesForApportionChargesIfNeeded();
				AssertEquals("invoiceCharge010ForInvoiceLineCharge2", true, invoice.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == Codes._010).IsJ7_ExchangeRateIATA);
				AssertEquals("invoiceCharge014ForInvoiceLineCharge2", false, invoice.GroupCharges.Cast<InvoiceApportionCharge>().Single(x => x.J7_ChargeType == Codes._014).IsJ7_ExchangeRateIATA);
			});
		}

		void AssertSetValuesIfNeeded_IsFreightChargeToEUBorderByAirInsideEU(ZString propertyName)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoice = declaration.Invoices.AddNew();
			invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._3;
			var invoiceCharge = invoice.Charges.AddNew(Codes._010);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLineCharge = invoiceLine.Charges.AddNew(Codes._010);
			var groupInvoiceCharge = declaration.TopGroupInvoice.Charges.AddNew(Codes._010);

			CombineAssertions(() =>
			{
				ChargeHelper.SetValuesIfNeeded(invoiceCharge);
				AssertEquals("InvoiceCharge", false, invoiceCharge.FindPropertyInfo(propertyName).Value);

				ChargeHelper.SetValuesIfNeeded(invoiceLineCharge);
				AssertEquals("InvoiceLineCharge", false, invoiceLineCharge.FindPropertyInfo(propertyName).Value);

				ChargeHelper.SetValuesIfNeeded(groupInvoiceCharge);
				AssertEquals("GroupInvoiceCharge", false, groupInvoiceCharge.FindPropertyInfo(propertyName).Value);
			});
		}

		void AssertSetValuesIfNeeded_IsFreightChargeAfterEUBorderByAirOutsideEU(ZString propertyName, bool excludeGroupInvoiceCharge = false)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoice = declaration.Invoices.AddNew();
			invoice.ZG_AgreedPlaceCode = UniversalReferenceConstants.AgreedPlaceCodes._1;
			var invoiceCharge = invoice.Charges.AddNew(Codes._014);
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLineCharge = invoiceLine.Charges.AddNew(Codes._014);

			CombineAssertions(() =>
			{
				ChargeHelper.SetValuesIfNeeded(invoiceCharge);
				AssertEquals("InvoiceCharge", false, invoiceCharge.FindPropertyInfo(propertyName).Value);

				ChargeHelper.SetValuesIfNeeded(invoiceLineCharge);
				AssertEquals("InvoiceLineCharge", false, invoiceLineCharge.FindPropertyInfo(propertyName).Value);

				if (!excludeGroupInvoiceCharge)
				{
					var groupInvoiceCharge = declaration.TopGroupInvoice.Charges.AddNew(Codes._014);
					ChargeHelper.SetValuesIfNeeded(groupInvoiceCharge);
					AssertEquals("GroupInvoiceCharge", false, groupInvoiceCharge.FindPropertyInfo(propertyName).Value);
				}
			});
		}

		void AssertSetValuesIfNeeded(string[] chargeTypes, ZString propertyName, bool excludeGroupInvoiceCharge = false)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var groupInvoiceCharge = declaration.TopGroupInvoice.Charges.AddNew();
			var needSetToFalseGroupInvoiceCharges = GetNeedSetToFalseChargeTypes(groupInvoiceCharge.Lookups.ChargeTypeList, chargeTypes);

			var invoice = declaration.Invoices.AddNew();
			var invoiceCharge = invoice.Charges.AddNew();
			var needSetToFalseInvoiceCharges = GetNeedSetToFalseChargeTypes(invoiceCharge.Lookups.ChargeTypeList, chargeTypes);

			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLineCharge = invoiceLine.Charges.AddNew();
			var needSetToFalseInvoiceLineCharges = GetNeedSetToFalseChargeTypes(invoiceLineCharge.Lookups.ChargeTypeList, chargeTypes);

			CombineAssertions(() =>
			{
				foreach (var type in chargeTypes)
				{
					if (!excludeGroupInvoiceCharge)
					{
						groupInvoiceCharge.J7_ChargeType = type;
						groupInvoiceCharge.FindPropertyInfo(propertyName).Value = ZBool.False;
						ChargeHelper.SetValuesIfNeeded(groupInvoiceCharge);
						AssertEquals($"J7_ChargeType is {type}, groupInvoiceCharge.{propertyName}", true, groupInvoiceCharge.FindPropertyInfo(propertyName).Value);
					}

					invoiceCharge.J7_ChargeType = type;
					invoiceCharge.FindPropertyInfo(propertyName).Value = ZBool.False;
					ChargeHelper.SetValuesIfNeeded(invoiceCharge);
					AssertEquals($"J7_ChargeType is {type}, invoiceCharge.{propertyName}", true, invoiceCharge.FindPropertyInfo(propertyName).Value);

					invoiceLineCharge.J7_ChargeType = type;
					invoiceLineCharge.FindPropertyInfo(propertyName).Value = ZBool.False;
					ChargeHelper.SetValuesIfNeeded(invoiceLineCharge);
					AssertEquals($"J7_ChargeType is {type}, invoiceLineCharge.{propertyName}", true, invoiceLineCharge.FindPropertyInfo(propertyName).Value);
				}

				foreach (var type in needSetToFalseGroupInvoiceCharges)
				{
					if (!excludeGroupInvoiceCharge)
					{
						groupInvoiceCharge.J7_ChargeType = type;
						groupInvoiceCharge.FindPropertyInfo(propertyName).Value = ZBool.True;
						ChargeHelper.SetValuesIfNeeded(groupInvoiceCharge);
						AssertEquals($"J7_ChargeType is {type}, groupInvoiceCharge.{propertyName}", false, groupInvoiceCharge.FindPropertyInfo(propertyName).Value);
					}
				}

				foreach (var type in needSetToFalseInvoiceCharges)
				{
					invoiceCharge.J7_ChargeType = type;
					invoiceCharge.FindPropertyInfo(propertyName).Value = ZBool.True;
					ChargeHelper.SetValuesIfNeeded(invoiceCharge);
					AssertEquals($"J7_ChargeType is {type}, invoiceCharge.{propertyName}", false, invoiceCharge.FindPropertyInfo(propertyName).Value);
				}

				foreach (var type in needSetToFalseInvoiceLineCharges)
				{
					invoiceLineCharge.J7_ChargeType = type;
					invoiceLineCharge.FindPropertyInfo(propertyName).Value = ZBool.True;
					ChargeHelper.SetValuesIfNeeded(invoiceLineCharge);
					AssertEquals($"J7_ChargeType is {type}, invoiceLineCharge.{propertyName}", false, invoiceLineCharge.FindPropertyInfo(propertyName).Value);
				}
			});
		}

		JobComInvoiceLine PrepareAdditionDeductionChargesAndRelatedBOs(string transportMode)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = transportMode;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			CreateCharges(invoiceLine);
			CreateApportionedCharges(invoiceLine);
			return invoiceLine;
		}

		void CreateCharges(JobComInvoiceLine invoiceLine)
		{
			foreach (var type in chargesTypesForTest)
			{
				var charge = invoiceLine.Charges.AddNew();
				charge.J7_ChargeType = type;
			}
		}

		void CreateApportionedCharges(JobComInvoiceLine invoiceLine)
		{
			foreach (var type in chargesTypesForTest)
			{
				var charge = invoiceLine.ApportionedCharges.AddNew();
				charge.J7_ChargeType = type;
			}
		}

		List<ZString> GetNeedSetToFalseChargeTypes(CodeDescriptionPairList chargeTypeList, string[] needSetToTrueChargeTypes)
		{
			var result = new List<ZString>();
			foreach (ICodeDescription codeDescription in chargeTypeList)
			{
				var chargeType = codeDescription.Code;
				if (!chargeType.In(needSetToTrueChargeTypes))
				{
					result.Add(chargeType);
				}
			}
			return result;
		}

		List<string> chargesTypesForTest => new List<string>
		{
			ImportChargeCodeList.Codes._001,
			ImportChargeCodeList.Codes._002,
			ImportChargeCodeList.Codes._003,
			ImportChargeCodeList.Codes._004,
			ImportChargeCodeList.Codes._005,
			ImportChargeCodeList.Codes._006,
			ImportChargeCodeList.Codes._007,
			ImportChargeCodeList.Codes._008,
			ImportChargeCodeList.Codes._009,
			ImportChargeCodeList.Codes._010,
			ImportChargeCodeList.Codes._011,
			ImportChargeCodeList.Codes._012,
			ImportChargeCodeList.Codes._014,
			ImportChargeCodeList.Codes._015,
			ImportChargeCodeList.Codes._016,
			ImportChargeCodeList.Codes._017,
			ImportChargeCodeList.Codes._019,
			ImportChargeCodeList.Codes.TCE
		};
	}
}
