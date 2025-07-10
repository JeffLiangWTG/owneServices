using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocCASSBillingLine : DocBaseWrapper
	{
		DocCASSBillingLine(CASSBillingLine cASSBillingLine, BusinessObjectFactory factoryToWrap)
			: base(cASSBillingLine, factoryToWrap)
		{
		}

		public static DocCASSBillingLine New(CASSBillingLine cASSBillingLine, BusinessObjectFactory factoryToWrap)
		{
			if (cASSBillingLine == null)
			{
				return null;
			}
			else
			{
				return new DocCASSBillingLine(cASSBillingLine, factoryToWrap);
			}
		}

		CASSBillingLine CASSBillingLine
		{
			get { return (CASSBillingLine)WrappedObject; }
		}

		public ZString ConsolID
		{
			get { return CASSBillingLine.ConsolID; }
		}

		public ZString MAWBNumber
		{
			get { return CASSBillingLine.MAWBNumber; }
		}

		public ZDateTime IssueDate
		{
			get { return CASSBillingLine.IssueDate; }
		}

		public ZDateTime DateOfArrival
		{
			get { return CASSBillingLine.DateOfArrival; }
		}

		public ZDateTime DateOfDelivery
		{
			get { return CASSBillingLine.DateOfDelivery; }
		}

		public ZString Airline2LetterCode
		{
			get { return CASSBillingLine.Airline2LetterCode; }
		}

		public ZString CreditorCode
		{
			get { return CASSBillingLine.CreditorCode; }
		}

		public ZString LoadPort
		{
			get { return CASSBillingLine.LoadPort; }
		}

		public ZString DischargePort
		{
			get { return CASSBillingLine.DischargePort; }
		}

		public ZDecimal SystemWeight
		{
			get { return CASSBillingLine.SystemWeight; }
		}

		public ZDecimal CASSWeight
		{
			get { return CASSBillingLine.CASSWeight; }
		}

		public ZDecimal WeightDifference
		{
			get { return CASSBillingLine.WeightDifference; }
		}

		public ZString WeightDifferenceMargin
		{
			get { return ((ZDecimal)ZArchitecture.Core.Utilities.Round(CASSBillingLine.WeightDifferenceMargin, 2)).ToStringTrimZeros() + '%'; }
		}

		public ZDecimal SystemCostAccrualValue
		{
			get { return CASSBillingLine.SystemCostAccrualValue; }
		}

		public ZDecimal CASSCostValue
		{
			get { return CASSBillingLine.CASSCostValueInLocalCurrencyForDisplay; }
		}

		public ZDecimal CASSRejectedClaimValue
		{
			get { return CASSBillingLine.CASSRejectedClaimValueInLocalCurrencyForDisplay; }
		}

		public ZDecimal CostDifference
		{
			get { return CASSBillingLine.CostDifference; }
		}

		public ZString CostDifferenceMargin
		{
			get { return ((ZDecimal)ZArchitecture.Core.Utilities.Round(CASSBillingLine.CostDifferenceMargin, 2)).ToStringTrimZeros() + '%'; }
		}

		public ZString Status
		{
			get { return CASSBillingLine.StatusForBinding; }
		}

		public ZString BranchCode
		{
			get { return CASSBillingLine.BranchCode; }
		}

		public ZString Adjustment
		{
			get { return CASSBillingLine.IsCASSAmendment ? Res.GetString("9ecd0127-8974-4c8c-bfbc-ec2833459910", "Amended") : ""; }
		}

		public ZDecimal SystemCostPostedValue
		{
			get { return CASSBillingLine.SystemCostPostedValue; }
		}

		public ZDecimal CASSCostAdjustedValue
		{
			get { return CASSBillingLine.CASSCostAdjustedValue; }
		}

		public ZDecimal NetCASSCost
		{
			get { return CASSBillingLine.NetCASSCost; }
		}
	}
}
