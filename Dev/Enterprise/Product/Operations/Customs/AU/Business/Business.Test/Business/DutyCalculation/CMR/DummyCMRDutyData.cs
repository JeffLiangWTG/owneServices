using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DummyCMRDutyData : ICMRDutyData
	{
		public DummyCMRDutyData(BusinessObjectFactory factory)
		{
			fFactory = factory;
		}
		#region ICMRDutyData Members

		readonly BusinessObjectFactory fFactory;
		public BusinessObjectFactory Factory
		{
			get { return fFactory; }
		}

		public ZDecimal CountervailingDutyExposed;
		public ZDecimal CountervailingDuty
		{
			get { return CountervailingDutyExposed; }
		}

		public ZDecimal DumpingDutyExposed;
		public ZDecimal DumpingDuty
		{
			get { return DumpingDutyExposed; }
		}

		public ZDecimal CustomsValueExposed;
		public ZDecimal CustomsValue
		{
			get { return CustomsValueExposed; }
		}

		public ZBool IsNature20Exposed;
		public ZBool IsNature20
		{
			get { return IsNature20Exposed; }
		}

		public ZDecimal TransportAndInsuranceInAUDExposed;
		public ZDecimal TransportAndInsuranceInAUD
		{
			get { return TransportAndInsuranceInAUDExposed; }
		}

		public ZDecimal FirstQtyExposed;
		public ZDecimal FirstQty
		{
			get
			{
				return FirstQtyExposed;
			}
		}

		public ZDecimal SecondQtyExposed;
		public ZDecimal SecondQty
		{
			get
			{
				return SecondQtyExposed;
			}
		}

		public ZDecimal OtherDutyFactorExposed;
		public ZDecimal OtherDutyFactor
		{
			get
			{
				return OtherDutyFactorExposed;
			}
		}

		public DutyDataFromInvoiceLine RandomLineDutyDataExposed;
		public DutyDataFromInvoiceLine RandomLineDutyData
		{
			get
			{
				return RandomLineDutyDataExposed;
			}
		}

		public bool IsSACExposed;
		public bool IsSACWithoutLines
		{
			get { return IsSACExposed; }
		}

		public bool IsSubjectToDutyAndTaxExposed = true;
		public bool IsSubjectToDutyAndTax
		{
			get { return IsSubjectToDutyAndTaxExposed; }
		}

		public bool IsNotLowValueShipmentExposed = true;
		public bool IsNotLowValueShipment
		{
			get { return IsNotLowValueShipmentExposed; }
		}

		public bool IsDutyAndTaxEstimatedForWHExposed;
		public bool IsDutyAndTaxEstimatedForWH
		{
			get { return IsDutyAndTaxEstimatedForWHExposed; }
		}

		public bool IsGSTDeferredExposed;
		public bool IsGSTDeferred
		{
			get { return IsGSTDeferredExposed; }
		}

		public Money ManualDutyAmountExposed = Money.Empty;
		public Money ManualDutyAmount
		{
			get { return ManualDutyAmountExposed; }
		}

		#endregion

		#region ILineDutyData Members

		void ILineDutyData.SetDutyResult(DutyResult dutyResult)
		{
		}

		void ILineDutyData.SetFeeResult(ZString feeType, ZDecimal feeAmount)
		{
		}

		#endregion
	}
}
