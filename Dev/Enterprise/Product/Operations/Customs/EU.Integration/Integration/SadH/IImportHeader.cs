using CargoWise.Types;

namespace Enterprise.Customs.EU.Integration.SadH
{
	/// <summary>
	/// Import specific fields for entry header
	/// </summary>
	public interface IImportHeader : IHeader
	{
		ZString FirstDeferredPayment { get; } // FIR-DAN
		ZString FirstDeferredPaymentPFX { get; } // FIR-DAN-PFX

		ZString SecondDeferredPayment { get; } // SCND-DAN
		ZString SecondDeferredPaymentPFX { get; } // SCND-DAN-PFX

		ZString RegisteredConsigneeTurn { get; }    // RCNSGE-TURN
		ZDecimal TotalAmountInvoiced { get; }   // INV-TOT-AC

		IOrganisation GovernmentContractor { get; } // GCON-TURN
		ZString CarrierName { get; } // CARRIER-NAME
		ZString PlaceOfArrivalInTheEU { get; }  // EU-ARR-LOCN-CODE
		ZDateTime IntendedDateAndTimeOfArrivalAtThePlaceOfArrival { get; } // INTD-ARR-DTM

		ZDecimal OSAirTransportAmount { get; }
		ZString OSAirTransportLoad { get; } // FARP-CODE
		ZString FreightApportionmentIndicator { get; } // FRGT-APRT-CODE
		ZString FreightChargesCurrency { get; } // FRGT-CHGE-CRRN
		ZDecimal FreightCharges { get; } // FRGT-CHGE-AC
		ZString InsuranceCurrency { get; } // INS-AMT-CRRN
		ZDecimal InsuranceAmount { get; } // INS-AMT-AC
		ZString DiscountAmountCurrency { get; } // INVD-AMT-CRRN
		ZDecimal DiscountAmount { get; } // INVD-AMT-AC
		ZDecimal DiscountPercentage { get; } // INVD-PCT
		ZString OtherChargesDeductionsCurrency { get; } // OCD-CRRN
		ZDecimal OtherChargesDeductionsValue { get; } // OCD-AC
		ZString AdjustmentForVATValueCurrency { get; } // VAT-ADJT-CRRN
		ZDecimal AdjustmentForVATValue { get; } // VAT-ADJT-AC

		ZBool FECCountryOfExport { get; } // PROC-INST "DSP"

		ZString PlaceOfLoading { get; } // PLA-LDG-CODE
		ZString PlaceOfUnLoading { get; } // PLA-ULDG-CODE        
	}
}
