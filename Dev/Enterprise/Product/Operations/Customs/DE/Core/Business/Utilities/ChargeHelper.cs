using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Codes = Enterprise.Customs.DE.Business.ImportChargeCodeList.Codes;

namespace Enterprise.Customs.DE.Business
{
	public static class ChargeHelper
	{
		public static bool IsJ7_IsDutiable_ReadOnlyRate(ZString chargeCode)
		{
			return chargeCode.In(new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
			Codes._011, Codes._012, Codes._014, Codes._015, Codes._016, Codes._017, Codes._019, Codes.AIR, Common.CustomsChargeTypeList.Codes.Discount,
			Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE });
		}

		public static bool IsJ7_IsStatisticalValueApplicable_ReadOnlyRate(ZString chargeCode)
		{
			return chargeCode.In(new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
			Codes._011, Codes._012, Codes._014, Codes._015, Codes._016, Codes._017, Codes._019, Codes.AIR, Common.CustomsChargeTypeList.Codes.Discount,
			Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE });
		}

		public static bool IsJ7_IsGSTApplicable_ReadOnlyRate(ZString chargeCode)
		{
			return chargeCode.In(new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
			Codes._011, Codes._012, Codes._014, Codes._015, Codes._016, Codes._017, Codes._019, Codes.AIR, Common.CustomsChargeTypeList.Codes.Discount,
			Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE });
		}

		public static bool IsJ7_Calc_IsIncludedInInvoiceAmount_ReadOnlyRate(ZString chargeCode)
		{
			return chargeCode.In(new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
			Codes._011, Codes._012, Codes._014, Codes._015, Codes._016, Codes._017, Codes._019, Codes.AIR,
			Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE });
		}

		public static bool IsJ7_IsIncludedInITOT_ReadOnlyRate(ZString chargeCode)
		{
			return chargeCode.In(new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
			Codes._011, Codes._012, Codes._014, Codes._015, Codes._016, Codes._017, Codes._019, Codes.AIR, Common.CustomsChargeTypeList.Codes.Discount,
			Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE });
		}

		public static bool IsJ7_ExchangeRateIATA_ReadOnlyRate(ZString chargeCode)
		{
			return chargeCode.In(new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009,
			Codes._012, Codes._015, Codes._016, Codes._017, Codes._019, Common.CustomsChargeTypeList.Codes.Discount,
			Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE });
		}

		public static bool IsJ7_Percentage_ReadOnlyRate(ZString chargeCode)
		{
			return chargeCode.In(new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009,
			Codes._011, Codes._012, Codes._015, Codes._016, Codes._017, Codes._019, Codes.AIR, Common.CustomsChargeTypeList.Codes.Discount,
			Codes.INP, Codes.SRC, Codes.SRN, Codes.SRS, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.TCE });
		}

		public static bool IsJ7_IsIncludedInITOT_NoValidation(ZString chargeCode)
		{
			return chargeCode.In(new ZString[] { Codes._014, Codes._015, Codes._016, Codes._017 });
		}

		public static bool IsFreightChargeToEUBorderByAirInsideEU(this BaseJobComInvHeaderCharge charge)
		{
			if (charge.J7_ChargeType == Codes._010)
			{
				JobComInvoiceHeader invoice = null;
				if (charge is InvoiceCharge invoiceCharge)
				{
					invoice = invoiceCharge.Invoice;
				}
				else if (charge is InvoiceLineCharge lineCharge)
				{
					invoice = lineCharge.InvoiceLine?.InvoiceHeader;
				}
				else if (charge is GroupInvoiceCharge groupInvoiceCharge)
				{
					invoice = groupInvoiceCharge.GroupInvoice?.JobComInvoiceHeaders.Cast<JobComInvoiceHeader>().FirstOrDefault(
						x => x.ZG_AgreedPlaceCode == UniversalReferenceConstants.AgreedPlaceCodes._3);
				}
				else if (charge is InvoiceApportionCharge invoiceApportionCharge)
				{
					invoice = invoiceApportionCharge.Invoice;
				}
				else if (charge is InvoiceLineApportionCharge invoiceLineApportionCharge)
				{
					invoice = invoiceLineApportionCharge.InvoiceLine?.InvoiceHeader;
				}

				return invoice != null && invoice.ZG_AgreedPlaceCode == UniversalReferenceConstants.AgreedPlaceCodes._3 && (invoice.JobDeclaration?.JE_TransportMode ?? ZString.Empty) == TransportTypeList.Codes.Air;
			}

			return false;
		}

		public static bool IsFreightChargeAfterEUBorderByAirOutsideEU(this CommonNonApportionedCharge charge)
		{
			if (charge.J7_ChargeType == Codes._014 || charge.J7_ChargeType == ChargeCodeList.Codes.OverseasFreight)
			{
				JobComInvoiceHeader invoice = null;
				if (charge is InvoiceCharge invoiceCharge)
				{
					invoice = invoiceCharge.Invoice;
				}
				else if (charge is InvoiceLineCharge lineCharge)
				{
					invoice = lineCharge.InvoiceLine?.InvoiceHeader;
				}
				else if (charge is GroupInvoiceCharge groupInvoiceCharge)
				{
					invoice = groupInvoiceCharge.GroupInvoice?.JobComInvoiceHeaders.Cast<JobComInvoiceHeader>().FirstOrDefault(
						x => x.ZG_AgreedPlaceCode == UniversalReferenceConstants.AgreedPlaceCodes._1);
				}

				return invoice != null && invoice.ZG_AgreedPlaceCode == UniversalReferenceConstants.AgreedPlaceCodes._1 && (invoice.JobDeclaration?.JE_TransportMode ?? ZString.Empty) == TransportTypeList.Codes.Air;
			}

			return false;
		}

		public static void SetValuesIfNeeded(this CommonNonApportionedCharge charge)
		{
			var chargeType = charge.J7_ChargeType;
			if (!chargeType.IsEmpty)
			{
				var isNotFreightChargeToEUBorderByAirInsideEU = !charge.IsFreightChargeToEUBorderByAirInsideEU();
				var isNotFreightChargeAfterEUBorderByAirOutsideEU = !charge.IsFreightChargeAfterEUBorderByAirOutsideEU();
				charge.J7_IsDutiable = isNotFreightChargeToEUBorderByAirInsideEU && NeedSetJ7_IsDutiableToTrue(chargeType) ? ZBool.True : ZBool.False;
				charge.J7_IsStatisticalValueApplicable = isNotFreightChargeToEUBorderByAirInsideEU && NeedSetJ7_IsStatisticalValueApplicableToTrue(chargeType) ? ZBool.True : ZBool.False;
				charge.J7_IsGSTApplicable = isNotFreightChargeToEUBorderByAirInsideEU && NeedSetJ7_IsGSTApplicableToTrue(chargeType) ? ZBool.True : ZBool.False;
				if (charge.GetType() != typeof(GroupInvoiceCharge))
				{
					charge.J7_Calc_IsIncludedInInvoiceAmount = isNotFreightChargeAfterEUBorderByAirOutsideEU && NeedSetJ7_Calc_IsIncludedInInvoiceAmountToTrue(chargeType) ? ZBool.True : ZBool.False;
				}
				charge.J7_IsIncludedInITOT = isNotFreightChargeAfterEUBorderByAirOutsideEU && NeedSetJ7_IsIncludedInITOTToTrue(chargeType) ? ZBool.True : ZBool.False;

				if (charge is GroupInvoiceCharge groupInvoiceCharge)
				{
					groupInvoiceCharge.IsJ7_ExchangeRateIATA = NeedSetIsJ7_ExchangeRateIATAToTrue(chargeType) ? ZBool.True : ZBool.False;
				}
				else if (charge is InvoiceCharge invoiceCharge)
				{
					invoiceCharge.IsJ7_ExchangeRateIATA = NeedSetIsJ7_ExchangeRateIATAToTrue(chargeType) ? ZBool.True : ZBool.False;
				}
				else if (charge is InvoiceLineCharge lineCharge)
				{
					lineCharge.IsJ7_ExchangeRateIATA = NeedSetIsJ7_ExchangeRateIATAToTrue(chargeType) ? ZBool.True : ZBool.False;
				}
			}
		}

		public static bool NeedSetJ7_IsDutiableToTrue(ZString chargeCode)
		{
			return chargeCode.In(new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
			Codes._011, Codes._012, Codes._019, Codes.AIR,
			Codes.INP });
		}

		public static bool NeedSetJ7_IsStatisticalValueApplicableToTrue(ZString chargeType)
		{
			return chargeType.In(new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
			Codes._011, Codes._012, Codes._019, Codes.AIR,
			Codes.INP, EU.Business.ChargeTypeList.Codes.StatisticalValue, Codes.OPF });
		}

		public static bool NeedSetJ7_IsGSTApplicableToTrue(ZString chargeType)
		{
			return chargeType.In(new ZString[] { Codes._001, Codes._002, Codes._003, Codes._004, Codes._005, Codes._006, Codes._007, Codes._008, Codes._009, Codes._010,
			Codes._011, Codes._012, Codes._014, Codes._019, Codes.AIR,
			Codes.INP, Codes.TCE, Codes.OPF  });
		}

		public static IEnumerable<InvoiceLineCharge> GetCustomsValueAdditionDeductionChargesForImport(this JobComInvoiceLine invoiceLine)
		{
			var additionDeductionChargesTypes = invoiceLine.Declaration.JE_TransportMode == TransportTypeGenericList.Codes.Air ? AdditionDeductionChargesTypesWithoutAir.Value : AdditionDeductionChargesTypes.Value;

			return invoiceLine.Charges.Cast<InvoiceLineCharge>().Where(c => additionDeductionChargesTypes.Contains(c.J7_ChargeType)).ToArray();
		}

		public static IEnumerable<InvoiceLineApportionCharge> GetCustomsValueAdditionDeductionApportionedChargesForImport(this JobComInvoiceLine invoiceLine)
		{
			var additionDeductionChargesTypes = invoiceLine.Declaration.JE_TransportMode == TransportTypeGenericList.Codes.Air ? AdditionDeductionChargesTypesWithoutAir.Value : AdditionDeductionChargesTypes.Value;

			return invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Where(c => additionDeductionChargesTypes.Contains(c.J7_ChargeType)).ToArray();
		}

		public static IEnumerable<InvoiceLineCharge> GetCustomsValueAirFreightCostsChargesForImport(this JobComInvoiceLine invoiceLine)
			=> invoiceLine.Charges.Cast<InvoiceLineCharge>().Where(c => c.J7_ChargeType == ImportChargeCodeList.Codes._010 || c.J7_ChargeType == ImportChargeCodeList.Codes._014).ToArray();

		public static IEnumerable<InvoiceLineApportionCharge> GetCustomsValueAirFreightCostsApportionedChargesForImport(this JobComInvoiceLine invoiceLine)
			=> invoiceLine.ApportionedCharges.Cast<InvoiceLineApportionCharge>().Where(c => c.J7_ChargeType == ImportChargeCodeList.Codes._010 || c.J7_ChargeType == ImportChargeCodeList.Codes._014).ToArray();

		public static void SetValuesForApportionChargesIfNeeded(this GroupInvoiceCharge groupInvoiceCharge)
		{
			var exchangeRateType = groupInvoiceCharge.J7_ExchangeRateType;
			if (groupInvoiceCharge.J7_ChargeType.SupportsIATA())
			{
				var exchangeRate = groupInvoiceCharge.J7_ExchangeRate;
				var jobComInvoiceHeaders = groupInvoiceCharge.GroupInvoice.JobComInvoiceHeaders;
				foreach (JobComInvoiceHeader invoice in jobComInvoiceHeaders)
				{
					var invoiceApportionCharges = groupInvoiceCharge.GetInvoiceApportionCharges(invoice);
					SetExchangeRateDataIfNeeded(invoiceApportionCharges, exchangeRateType, exchangeRate);

					var invoiceLineApportionCharges = groupInvoiceCharge.GetInvoiceLineApportionCharges(invoice);
					SetExchangeRateDataIfNeeded(invoiceLineApportionCharges, exchangeRateType, exchangeRate);
				}
			}
		}

		public static void SetValuesForApportionChargesIfNeeded(this InvoiceCharge invoiceCharge)
		{
			if (invoiceCharge.J7_ChargeType.SupportsIATA())
			{
				var invoiceLineApportionCharges = invoiceCharge.GetInvoiceLineApportionCharges(invoiceCharge.Invoice);
				SetExchangeRateDataIfNeeded(invoiceLineApportionCharges, invoiceCharge.J7_ExchangeRateType, invoiceCharge.J7_ExchangeRate);
			}
		}

		public static void SetValuesForApportionChargesIfNeeded(this InvoiceLineCharge invoiceLineCharge)
		{
			if (invoiceLineCharge.J7_ChargeType.SupportsIATA())
			{
				var invoiceApportionCharges = invoiceLineCharge.GetInvoiceApportionCharges(invoiceLineCharge.InvoiceLine.InvoiceHeader);
				SetExchangeRateDataIfNeeded(invoiceApportionCharges, invoiceLineCharge.J7_ExchangeRateType, invoiceLineCharge.J7_ExchangeRate);
			}
		}

		static bool NeedSetJ7_Calc_IsIncludedInInvoiceAmountToTrue(ZString chargeType)
		{
			return chargeType.In(new ZString[] { Codes._014, Codes._015, Codes._016, Codes._017, Codes.OPF });
		}

		static bool NeedSetJ7_IsIncludedInITOTToTrue(ZString chargeType)
		{
			return chargeType.In(new ZString[] { Codes._014, Codes._015, Codes._016, Codes._017, });
		}

		static bool NeedSetIsJ7_ExchangeRateIATAToTrue(ZString chargeType)
		{
			return chargeType == Codes.AIR;
		}

		static InvoiceApportionCharge[] GetInvoiceApportionCharges(this JobComInvCharge charge, JobComInvoiceHeader invoice) => invoice.GroupCharges.Find(charge.ApportionChargeKey);

		static InvoiceLineApportionCharge[] GetInvoiceLineApportionCharges(this JobComInvCharge charge, JobComInvoiceHeader invoice) => invoice.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.ApportionedCharges.Find(charge.ApportionChargeKey)).ToArray();

		static void SetExchangeRateDataIfNeeded(CommonApportionedCharge[] apportionedCharges, ZString exchangeRateType, ZDecimal exchangeRate)
		{
			apportionedCharges.ForEach(x => SetExchangeRateData(x));

			void SetExchangeRateData(CommonApportionedCharge apportionedCharge)
			{
				if (apportionedCharge.J7_ExchangeRateType != exchangeRateType)
				{
					apportionedCharge.J7_ExchangeRateType = exchangeRateType;
				}
				if (apportionedCharge.J7_ExchangeRate != exchangeRate)
				{
					apportionedCharge.J7_ExchangeRate = exchangeRate;
				}
			}
		}

		static System.Lazy<HashSet<string>> AdditionDeductionChargesTypes => Lazy.Create(() => new HashSet<string>
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
				ImportChargeCodeList.Codes._019
			}, true);

		static System.Lazy<HashSet<string>> AdditionDeductionChargesTypesWithoutAir => Lazy.Create(() => new HashSet<string>(AdditionDeductionChargesTypes.Value.Except(new[] { ImportChargeCodeList.Codes._010, ImportChargeCodeList.Codes._014 })), true);
	}
}
