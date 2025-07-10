using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryLineFee : TypeSafeCusEntryLineFee, Integration.Customs.AU.ICusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsPayableToCustomsForLine
		{
			get
			{
				foreach (string payableCharge in PayableToCustoms)
				{
					if (CF_ChargeType == payableCharge)
					{
						return true;
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Line Level charges that are payable. These charges will be aggregated later in the message processor to determine Total payable
		/// as Customs sends the additional or refundable amount for amendment message for Total Payable segment.
		/// Note that GST deferred is not in the list
		/// </summary>
		public readonly static string[] PayableToCustoms = new string[]
		{
			CusEntryChargeTypeList.Codes.DutyAmount,
			CusEntryChargeTypeList.Codes.CountervailingDuty,
			CusEntryChargeTypeList.Codes.DumpingDuty,
			CusEntryChargeTypeList.Codes.WetAmount,
			CusEntryChargeTypeList.Codes.GSTAmount,
			CusEntryChargeTypeList.Codes.LCTAmount,
			CusEntryChargeTypeList.Codes.StandardDutyOverriden
		};

		protected override bool ShouldResetDataOnMergingCore
		{
			get { return !IsNotCalculated; }
		}

		bool IsNotCalculated
		{
			get
			{
				return CF_ChargeType == CusEntryChargeTypeList.Codes.TotalDutyTaxForLine
					|| CF_ChargeType == CusEntryChargeTypeList.Codes.DumpingDuty
					|| CF_ChargeType == CusEntryChargeTypeList.Codes.CountervailingDuty
					|| CF_ChargeType == CusEntryChargeTypeList.Codes.SecurityConcession;
			}
		}

		public bool IsDeferrable => IsFeeTypeDeferrable(CF_ChargeType);

		public static bool IsFeeTypeDeferrable(string feeType)
		{
			switch (feeType)
			{
				case CusEntryChargeTypeList.Codes.CountervailingDuty:
				case CusEntryChargeTypeList.Codes.DumpingDuty:
				case CusEntryChargeTypeList.Codes.DutyAmount:
				case CusEntryChargeTypeList.Codes.LCTAmount:
				case CusEntryChargeTypeList.Codes.WetAmount:
					return true;
				default:
					return false;
			}
		}
	}
}
