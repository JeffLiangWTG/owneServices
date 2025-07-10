using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IDutyDataWrapper
	{
		BusinessObjectFactory Factory { get; }
		ZString Preference { get; }
		ZString ImportTariffCode { get; }
		ZString TreatmentCode { get; }
		AUAddInfo EffectiveAddInfo { get; }
		bool IsInDatabase { get; }
	}

	public class DutyDataWrapper : IAggregatedAddInfo, ICMRDutyData
	{
		public DutyDataWrapper(IDutyDataWrapper parent)
		{
			this.parent = parent;
		}

		readonly IDutyDataWrapper parent;

		public IDisposable SuspendSettingHasChanges()
		{
			return new DisposableObject();
		}

		ZString TariffNumber
		{
			get
			{
				ZString[] split = parent.ImportTariffCode.Split(' ');
				return split.Length >= 1 ? split[0].Replace(".", "") : ZString.Empty;
			}
		}

		ZString StatCode
		{
			get
			{
				ZString[] split = parent.ImportTariffCode.Split(' ');
				return split.Length >= 2 ? split[1] : ZString.Empty;
			}
		}

		ZString TreatmentCode
		{
			get { return parent.TreatmentCode; }
		}

		ZString CustomsUnitQty
		{
			get
			{
				var wrapper = ClassificationPeriodSnapshotWrapper.Load(Factory, TariffNumber, StatCode, EffectiveDutyDate);
				return wrapper?.QuantityUnit ?? ZString.Empty;
			}
		}

		public bool IsInDatabase
		{
			get { return parent.IsInDatabase; }
		}

		#region IDutyData Members

		public BusinessObjectFactory Factory
		{
			get { return parent.Factory; }
		}

		public ZDateTime DateOfValuation
		{
			get { return ZDateTime.Now; }
		}

		public ZDateTime EffectiveDutyDate
		{
			get { return ZDateTime.Now; }
		}

		public ZBool IsNature20 => ZBool.False;

		#region IAggregatedAddInfo Members

		public bool HasChanges
		{
			get { return false; }
			set { }
		}

		public bool IsDeleted
		{
			get { return false; }
		}

		public ZString AggregatedZA_ORG
		{
			get { return ZString.Empty; }
		}

		public ZString AggregatedZA_PRF
		{
			get { return ZString.Empty; }
		}

		public IZType AggregatedValue(string propertyName)
		{
			return ZString.Empty;
		}

		public bool IsCopying
		{
			get { return false; }
		}

		public bool ReadOnly
		{
			get { return false; }
		}

		#endregion

		#endregion

		#region ICMRDutyData Members

		public ZDecimal DumpingDuty
		{
			get { return new ZDecimal(); }
		}

		public ZDecimal CountervailingDuty
		{
			get { return new ZDecimal(); }
		}

		public ZDecimal FirstQty
		{
			get { return new ZDecimal(); }
		}

		public ZDecimal SecondQty
		{
			get { return new ZDecimal(); }
		}

		public ZDecimal OtherDutyFactor
		{
			get { return new ZDecimal(); }
		}

		public Money ManualDutyAmount
		{
			get { return Money.Empty; }
		}

		public DutyDataFromInvoiceLine RandomLineDutyData
		{
			get
			{
				DutyDataFromInvoiceLine result = new DutyDataFromInvoiceLine();

				result.EffectiveDutyDate = EffectiveDutyDate;
				result.StatCode = StatCode;
				result.IsGSTExempt = parent.EffectiveAddInfo.ZA_GSTE.IsEmpty;
				result.IsLCTPayable = true;
				result.IsLCTExempt = !parent.EffectiveAddInfo.ZA_LCTE.IsEmpty || parent.EffectiveAddInfo.ZA_LCTQ == "Y";
				result.IsWETExempt = parent.EffectiveAddInfo.ZA_WETE.IsEmpty || parent.EffectiveAddInfo.ZA_WETQ == "Y";
				result.FirstTariffNumber = TariffNumber;
				result.FirstTreatmentCode = TreatmentCode;
				result.FirstUQ = CustomsUnitQty;
				result.SecondUQ = parent.EffectiveAddInfo.ZA_UQ2;

				result.Preference = parent.EffectiveAddInfo.ZA_PST.IsEmpty ? new ZString("GEN") : parent.EffectiveAddInfo.ZA_PST;
				result.RateNumber = parent.EffectiveAddInfo.ZA_RNO;
				result.SecondTariffNumber = parent.EffectiveAddInfo.ZA_CL2.Replace(".", "");
				result.SecondTreatmentCode = parent.EffectiveAddInfo.ZA_TR2;
				result.TreatmentRateNumber = parent.EffectiveAddInfo.ZA_TRN;

				return result;
			}
		}

		public bool IsSubjectToDutyAndTax
		{
			get { return true; }
		}

		bool ICMRDutyData.IsNotLowValueShipment
		{
			get { return true; }
		}

		bool ICMRDutyData.IsDutyAndTaxEstimatedForWH
		{
			get { return false; }
		}

		public bool IsGSTDeferred
		{
			get { return false; }
		}

		ZDecimal ICMRDutyData.CustomsValue
		{
			get { return new ZDecimal(); }
		}

		public ZDecimal TransportAndInsuranceInAUD
		{
			get { return new ZDecimal(); }
		}

		#endregion

		void IAggregatedAddInfo.MarkAsNeedingValidation()
		{
		}

		bool IAggregatedAddInfo.LightValidationIsValid
		{
			get { return false; }
		}

		bool IAggregatedAddInfo.LightValidationEnabled
		{
			get { return false; }
		}

		bool IAggregatedAddInfo.IsMarkingAsNeedingValidationSuspended
		{
			get { return false; }
		}

		IDisposable IAggregatedAddInfo.SuspendMarkingAsNeedingValidation()
		{
			return null;
		}

		#region ILineDutyData Members

		void Common.ILineDutyData.SetDutyResult(Common.DutyResult dutyResult)
		{
		}

		void Common.ILineDutyData.SetFeeResult(ZString feeType, ZDecimal feeAmount)
		{
		}

		#endregion
	}
}
