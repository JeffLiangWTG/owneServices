using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDutyWrapperForInvoiceLine : ICMRDutyData
	{
		public CMRDutyWrapperForInvoiceLine(JobComInvoiceLine invoiceLine, bool excludeTreatmentCode)
		{
			this.invoiceLine = invoiceLine;
			this.excludeTreatmentCode = excludeTreatmentCode;
		}

		readonly JobComInvoiceLine invoiceLine;
		readonly bool excludeTreatmentCode;

		#region ICMRDutyData Members

		public ZDecimal DumpingDuty
		{
			get { return invoiceLine.JI_Calc_DumpingDuty; }
		}

		public ZDecimal CountervailingDuty
		{
			get { return invoiceLine.JI_Calc_CountervailingDuty; }
		}

		public BusinessObjectFactory Factory
		{
			get { return invoiceLine.Factory; }
		}

		public ZDecimal FirstQty
		{
			get { return invoiceLine.JI_CustomsQuantity; }
		}

		public ZDecimal SecondQty
		{
			get { return invoiceLine.AddInfo.ZA_QT2; }
		}

		public ZDecimal OtherDutyFactor
		{
			get { return invoiceLine.AddInfo.ZA_ODF; }
		}

		public Money ManualDutyAmount
		{
			get { return new Money(invoiceLine.AddInfo.ZA_DTY, JobDeclaration.GetLocalCurrency()); }
		}

		public DutyDataFromInvoiceLine RandomLineDutyData
		{
			get
			{
				DutyDataFromInvoiceLine result = new DutyDataFromInvoiceLine();
				result.EffectiveDutyDate = invoiceLine.EffectiveDutyDate;
				result.StatCode = invoiceLine.StatCode;
				result.IsGSTExempt = !invoiceLine.AddInfo.AggregatedValue(AUAddInfoSchema.ZA_GSTE.Name).IsEmpty ||
									invoiceLine.DoesTariffRateOrTreatmentCodeDeemGSTExemption;

				result.IsLCTPayable = invoiceLine.AddInfo.ZA_LCTI == "Y";
				result.IsLCTExempt = invoiceLine.AddInfo.ZA_LCTQ == "Y";
				result.IsWETExempt = !invoiceLine.AddInfo.ZA_WETE.IsEmpty || invoiceLine.AddInfo.ZA_WETQ == "Y";
				result.FirstTariffNumber = invoiceLine.TariffNumber.Replace(".", "");
				result.FirstUQ = invoiceLine.JI_CustomsUnitQty;
				result.SecondUQ = invoiceLine.AddInfo.ZA_UQ2;
				result.ICN = invoiceLine.AddInfo.ZA_ICN;
				result.LCTE = invoiceLine.AddInfo.ZA_LCTE;

				result.Preference = invoiceLine.IsGeneralRate ? new ZString("GEN") : invoiceLine.AggregatedZA_PST;
				result.RateNumber = invoiceLine.AddInfo.ZA_RNO;

				result.SecondTariffNumber = invoiceLine.AddInfo.ZA_CL2.Replace(".", "");

				if (!excludeTreatmentCode)
				{
					result.FirstTreatmentCode = invoiceLine.TreatmentCode;
					result.SecondTreatmentCode = invoiceLine.AddInfo.ZA_TR2;
					result.TreatmentRateNumber = invoiceLine.AddInfo.ZA_TRN;
				}
				return result;
			}
		}

		public ZBool IsNature20
		{
			get { return invoiceLine.JI_IsPackToBondForLine; }
		}

		public bool IsSubjectToDutyAndTax
		{
			get { return !SecurityRequiredTreatmentCodes.Contains((string)invoiceLine.TreatmentCode); }
		}

		string[] SecurityRequiredTreatmentCodes
		{
			get { return _securityRequiredTreatmentCodes ?? (_securityRequiredTreatmentCodes = AUCustomsDataRegistry.Instance.SecurityRequiredTreatmentCodes.Value.Split(',')); }
		}
		string[] _securityRequiredTreatmentCodes;

		bool ICMRDutyData.IsNotLowValueShipment
		{
			get { return true; }
		}

		bool ICMRDutyData.IsDutyAndTaxEstimatedForWH
		{
			get { return invoiceLine.IsDutyAndTaxEstimatedForWH; }
		}

		public bool IsGSTDeferred
		{
			get
			{
				OrgHeader importer = invoiceLine.Declaration == null ? null : invoiceLine.Declaration.Importer;
				return importer != null && importer.MiscServ.IsGSTVATDeferred;
			}
		}

		public decimal PercentageOfFOBForCustomsValue = 100m;

		public ZDecimal CustomsValue
		{
			get
			{
				return (PercentageOfFOBForCustomsValue / 100) * invoiceLine.JI_Calc_FOB_InLocalCurrency + invoiceLine.JI_PriceAdjustmentInLocalCurrency;
			}
		}

		public ZDecimal TransportAndInsuranceInAUD
		{
			get
			{
				return invoiceLine.TransportAndInsuranceInLocalCurrency;
			}
		}

		#endregion

		#region ILineDutyData Members

		void ILineDutyData.SetDutyResult(DutyResult dutyResult)
		{
			throw new System.NotSupportedException("This is a duty data wrapper for an invoice line. InvoiceLines do not store duty calculation result.");
		}

		void ILineDutyData.SetFeeResult(ZString feeType, ZDecimal feeAmount)
		{
			throw new System.NotSupportedException("This is a duty data wrapper for an invoice line. InvoiceLines do not store fee calculation result.");
		}

		#endregion
	}
}
