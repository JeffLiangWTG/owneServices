using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.DocumentWrappers
{
	public class DocCASSBilling : DocBaseWrapper
	{
		DocCASSBilling(CASSBilling cASSBilling, BusinessObjectFactory factoryToWrap)
			: base(cASSBilling, factoryToWrap)
		{
		}

		public static DocCASSBilling New(CASSBilling cASSBilling, BusinessObjectFactory factoryToWrap)
		{
			if (cASSBilling == null)
			{
				return null;
			}
			else
			{
				return new DocCASSBilling(cASSBilling, factoryToWrap);
			}
		}

		CASSBilling CASSBilling
		{
			get { return (CASSBilling)WrappedObject; }
		}

		public ZBool IsRejectedClaimLinesExpected
		{
			get { return CASSBilling.IsRejectedClaimLinesExpected; }
		}

		public ZBool IsImportBilling
		{
			get { return CASSBilling.IsImportBilling; }
		}

		public ZBool IsExportBilling
		{
			get { return CASSBilling.IsExportBilling; }
		}

		public ZString HOTFileName
		{
			get { return CASSBilling.HOTFileName.RemoveSafe(0, CASSBilling.HOTFileName.LastIndexOf('\\') + 1); }
		}

		public ZDateTime BillingPeriodStart
		{
			get { return CASSBilling.BillingPeriodStart; }
		}

		public ZDateTime BillingPeriodEnd
		{
			get { return CASSBilling.BillingPeriodEnd; }
		}

		public ZDateTime BillingDate
		{
			get { return CASSBilling.BillingDate; }
		}

		#region Invoice Lines

		DocCASSBillingLineCollection fLines;

		public DocCASSBillingLineCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new DocCASSBillingLineCollection(CASSBilling.Factory);
					foreach (CASSBillingLine line in CASSBilling.Lines)
					{
						DocCASSBillingLine lineToAdd = DocCASSBillingLine.New(line, Factory);
						fLines.Add(lineToAdd);
					}
				}

				return fLines;
			}
		}

		DocCASSBillingLineCollection fHiddenLines;
		public DocCASSBillingLineCollection HiddenLines
		{
			get
			{
				if (fHiddenLines == null)
				{
					fHiddenLines = new DocCASSBillingLineCollection(CASSBilling.Factory);
					foreach (CASSBillingLine hiddenLine in CASSBilling.HiddenLines)
					{
						DocCASSBillingLine lineToAdd = DocCASSBillingLine.New(hiddenLine, Factory);
						fHiddenLines.Add(lineToAdd);
					}
				}

				return fHiddenLines;
			}
		}

		#endregion

		public ZDecimal TotalSystemCostAccrualValue
		{
			get { return CASSBilling.TotalSystemCostAccrualValue; }
		}

		public ZDecimal TotalCASSCostValue
		{
			get { return CASSBilling.TotalCASSCostValue; }
		}

		public ZDecimal TotalCASSRejectedClaimValue
		{
			get { return CASSBilling.TotalCASSRejectedClaimValue; }
		}

		public ZDecimal TotalCostDifferenceValue
		{
			get { return CASSBilling.TotalCostDifferenceValue; }
		}

		public ZDecimal TotalHiddenSystemCostAccrualValue
		{
			get { return CASSBilling.TotalHiddenSystemCostAccrualValue; }
		}

		public ZDecimal TotalAllSystemCostAccrualValue
		{
			get { return CASSBilling.TotalAllSystemCostAccrualValue; }
		}

		public ZDecimal TotalCASSCostAdjustedValue
		{
			get { return CASSBilling.TotalCASSCostAdjustedValue; }
		}

		public ZDecimal TotalHiddenCASSCostAdjustedValue
		{
			get { return CASSBilling.TotalHiddenCASSCostAdjustedValue; }
		}

		public ZDecimal TotalAllCASSCostAdjustedValue
		{
			get { return CASSBilling.TotalAllCASSCostAdjustedValue; }
		}

		public ZDecimal TotalHiddenCASSCostValue
		{
			get { return CASSBilling.TotalHiddenCASSCostValue; }
		}

		public ZDecimal TotalHiddenCASSRejectedClaimValue
		{
			get { return CASSBilling.TotalHiddenCASSRejectedClaimValue; }
		}

		public ZDecimal TotalAllCASSCostValue
		{
			get { return CASSBilling.TotalAllCASSCostValue; }
		}

		public ZDecimal TotalAllCASSRejectedClaimValue
		{
			get { return CASSBilling.TotalAllCASSRejectedClaimValue; }
		}

		public ZDecimal TotalNetCASSCostValue
		{
			get { return CASSBilling.TotalNetCASSCostValue; }
		}

		public ZDecimal TotalHiddenNetCASSCostValue
		{
			get { return CASSBilling.TotalHiddenNetCASSCostValue; }
		}

		public ZDecimal TotalAllNetCASSCostValue
		{
			get { return CASSBilling.TotalAllNetCASSCostValue; }
		}

		public ZDecimal TotalHiddenCostDifferenceValue
		{
			get { return CASSBilling.TotalHiddenCostDifferenceValue; }
		}

		public ZDecimal TotalAllCostDifferenceValue
		{
			get { return CASSBilling.TotalAllCostDifferenceValue; }
		}

		public ZString TotalCostDifferenceMargin
		{
			get { return TotalSystemCostAccrualValue == 0 ? "100%" : ((ZDecimal)ZArchitecture.Core.Utilities.Round(TotalCostDifferenceValue / TotalSystemCostAccrualValue, 2)).ToStringTrimZeros() + '%'; }
		}

		public ZString TotalHiddenCostDifferenceMargin
		{
			get { return CASSBilling.TotalHiddenSystemCostAccrualValue == 0 ? "100%" : ((ZDecimal)ZArchitecture.Core.Utilities.Round(CASSBilling.TotalHiddenCostDifferenceValue / CASSBilling.TotalHiddenSystemCostAccrualValue, 2)).ToStringTrimZeros() + '%'; }
		}

		public ZString TotalAllCostDifferenceMargin
		{
			get { return CASSBilling.TotalAllSystemCostAccrualValue == 0 ? "100%" : ((ZDecimal)ZArchitecture.Core.Utilities.Round(CASSBilling.TotalAllCostDifferenceValue / CASSBilling.TotalAllSystemCostAccrualValue, 2)).ToStringTrimZeros() + '%'; }
		}
	}
}
