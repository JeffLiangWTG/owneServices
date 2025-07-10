using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common.AU
{
	public interface ICMRDutyData : ILineDutyData
	{
		BusinessObjectFactory Factory { get; }

		ZDecimal FirstQty { get; }
		ZDecimal SecondQty { get; }
		ZDecimal OtherDutyFactor { get; }
		Money ManualDutyAmount { get; }

		DutyDataFromInvoiceLine RandomLineDutyData { get; }

		ZBool IsNature20 { get; }
		bool IsSubjectToDutyAndTax { get; }
		bool IsGSTDeferred { get; }
		bool IsNotLowValueShipment { get; }
		bool IsDutyAndTaxEstimatedForWH { get; }

		ZDecimal CustomsValue { get; }
		ZDecimal TransportAndInsuranceInAUD { get; }
		ZDecimal DumpingDuty { get; }
		ZDecimal CountervailingDuty { get; }
	}

	public struct DutyDataFromInvoiceLine
	{
		public ZString FirstTariffNumber;
		public ZString FirstTreatmentCode;
		public ZString SecondTariffNumber;
		public ZString SecondTreatmentCode;
		public ZString StatCode;
		public ZString RateNumber;
		public ZString Preference;
		public ZString FirstUQ;
		public ZString SecondUQ;
		public ZDateTime EffectiveDutyDate;
		public bool IsGSTExempt;
		public bool IsWETExempt;
		public bool IsLCTExempt;
		public bool IsLCTPayable;
		public ZString TreatmentRateNumber;
		public ZString ICN;
		public ZString LCTE;
	}

	public enum TransportModeEnum { Air, Sea, Post, Other, Undefined }

	public interface IDeclarationChargeProvider : IHeaderFeeData
	{
		bool IsS162ATemporaryImport { get; }
		bool IsSOFADeclaration { get; }
		TransportModeEnum TransportMode { get; }
		int NumberOfFCLContainers { get; }
		int NumberOfFCXContainers { get; }
		int NumberOfLCLContainers { get; }
		ZDate EffectiveDutyDate { get; }
		ZDecimal N10CustomsValue { get; }
		ZDecimal N20CustomsValue { get; }
		ZDecimal N30CustomsValue { get; }
		bool IsExemptedFromCustomsAndQuarantineFees { get; }
	}

	public interface IDutyDataFromInvoiceLineProvider
	{
		DutyDataFromInvoiceLine GetDutyDataFromInvoiceLine(ZDateTime dutyDate, ZGuid buyerPK, ZGuid supplierPK);
	}

	public interface IAUDutyCalculationManager : IDutyCalculationManager
	{
#if DEBUG
		void CreateTaxOrFee();
#endif
	}
}
