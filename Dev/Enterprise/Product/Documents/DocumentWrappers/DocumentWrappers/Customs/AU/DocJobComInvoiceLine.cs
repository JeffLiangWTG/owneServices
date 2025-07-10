using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocJobComInvoiceLine : DocBaseJobComInvoiceLine
	{
		protected DocJobComInvoiceLine(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceLine, factoryToWrap)
		{
		}

		#region New

		public static DocJobComInvoiceLine New(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
		{
			DocJobComInvoiceLine result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(jobComInvoiceLine, factoryToWrap);
			}
			else if (jobComInvoiceLine != null)
			{
				result = new DocJobComInvoiceLine(jobComInvoiceLine, factoryToWrap);
			}
			return result;
		}

		protected delegate DocJobComInvoiceLine NewDelegate(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Related Business Objects

		public DocOrder Order
		{
			get
			{
				Order order = JobComInvoiceLine.Order;
				return order == null ? null : DocOrder.New(order, Factory);
			}
		}

		#endregion

		#region Overrides

		public override ZBool MotorVehiclePlan
		{
			get { return JobComInvoiceLine.JI_MotorVehiclePlan; }
		}

		public override ZBool Texco
		{
			get { return JobComInvoiceLine.JI_Texco; }
		}

		public override ZBool Drawback
		{
			get { return JobComInvoiceLine.JI_Drawback; }
		}

		public override ZString TempImportNum
		{
			get { return JobComInvoiceLine.JI_TempImportNum; }
		}

		public override ZString PermitNumber1
		{
			get { return JobComInvoiceLine.PermitsIncludingHeader.Count > 0 ? JobComInvoiceLine.PermitsIncludingHeader[0] : ZString.Empty; }
		}

		public override ZString PermitNumber2
		{
			get { return JobComInvoiceLine.PermitsIncludingHeader.Count > 1 ? JobComInvoiceLine.PermitsIncludingHeader[1] : ZString.Empty; }
		}

		public override ZString PermitNumber3
		{
			get { return JobComInvoiceLine.PermitsIncludingHeader.Count > 2 ? JobComInvoiceLine.PermitsIncludingHeader[2] : ZString.Empty; }
		}

		public ZString TariffNumber
		{
			get { return JobComInvoiceLine.TariffNumber; }
		}

		public override ZDecimal DutyAmountForInvoiceReport
		{
			get { return FlatDutyPortion + DutyAmount; }
		}

		public ZDecimal DutyAmountIncludingAllTheOtherDuties
		{
			get { return DutyAmount + JobComInvoiceLine.JI_Calc_AllOtherDuties; }
		}

		public override ZDecimal EffectiveDuty
		{
			get
			{
				ZDecimal result = 0M;

				if (JobComInvoiceLine.CusEntryLine != null)
				{
					Money mergedPriceIncludingAdjustment = JobComInvoiceLine.CusEntryLine.PriceIncludingAdjustment;
					if (mergedPriceIncludingAdjustment.Amount != 0m)
					{
						result = (JobComInvoiceLine.LinePriceIncludingAdjustment / mergedPriceIncludingAdjustment.Amount).Amount;
					}
				}

				return result;
			}
		}

		protected override ZDecimal VOTICore
		{
			get { return JobComInvoiceLine.JI_Calc_VOTI; }
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(Enterprise.Customs.Business.BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseCusEntryLine CreateCusEntryLine(Enterprise.Customs.Business.CusEntryLine entryLineToWrap)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineToWrap, Factory);
		}

		#endregion

		#region ZString Fields

		public ZString WarehouseNameAndAddress
		{
			get
			{
				OrgAddress address = JobComInvoiceLine.WarehouseAddress;
				return address == null ? ZString.Empty : (ZString)(address.Header.OH_FullName + " (" + address.OA_Address1 + ")");
			}
		}

		public ZString WarehouseCCP
		{
			get { return JobComInvoiceLine.WarehouseCCP; }
		}

		public ZString SortString
		{
			get { return JobComInvoiceLine.Declaration.JE_DeclarationReference + "__" + JobComInvoiceLine.InvoiceHeader.JZ_InvoiceNumber + "__" + JobComInvoiceLine.JI_LineNo.ToString().PadLeft(10, '0'); }
		}

		public ZString ParentLineCode
		{
			get { return JobComInvoiceLine.JI_ParentLineCode; }
		}

		public ZString AUState
		{
			get { return JobComInvoiceLine.JI_AUState; }
		}

		public ZString InstrumentType
		{
			get { return JobComInvoiceLine.InstrumentType; }
		}

		public ZString InstrumentCode
		{
			get { return JobComInvoiceLine.InstrumentCode; }
		}

		public ZString LinePrefix
		{
			get { return JobComInvoiceLine.JI_LinePrefix; }
		}

		public ZString DrawbackEDN
		{
			get { return JobComInvoiceLine.ExportDeclarationNumber; }
		}

		public ZString DrawbackImportDec
		{
			get { return JobComInvoiceLine.DrawbackImportDeclarationNumber; }
		}

		public ZString DrawbackImportDecLine
		{
			get
			{
				return JobComInvoiceLine.DrawbackImportDeclarationLine == 0 ? "" : " / " + JobComInvoiceLine.DrawbackImportDeclarationLine.ToString().Trim();
			}
		}

		public ZString DrawbackImportDecLineAlone
		{
			get
			{
				return JobComInvoiceLine.DrawbackImportDeclarationLine == 0 ? "" : JobComInvoiceLine.DrawbackImportDeclarationLine.ToString().Trim();
			}
		}

		public ZString DrawbackMethod
		{
			get { return JobComInvoiceLine.DrawbackAssesmentMethod; }
		}

		public ZString IsDrawbackDeclarationMethodA
		{
			get { return JobComInvoiceLine.DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.ActualShipment ? "Y" : ""; }
		}

		public ZString IsDrawbackDeclarationMethodB
		{
			get { return JobComInvoiceLine.DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment ? "Y" : ""; }
		}

		public ZString IsDrawbackDeclarationMethodC
		{
			get { return JobComInvoiceLine.DrawbackAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.Imputation ? "Y" : ""; }
		}

		public ZString IsDrawbackLineAmberReasonC
		{
			get { return JobComInvoiceLine.DrawbackLineAmberReasonCode == JobDeclaration.DrawbackAmberReasonTypes.Calculation ? "Y" : ""; }
		}

		public ZString IsDrawbackLineAmberReasonD
		{
			get { return JobComInvoiceLine.DrawbackLineAmberReasonCode == JobDeclaration.DrawbackAmberReasonTypes.Declaration ? "Y" : ""; }
		}

		public ZString IsDrawbackLineAmberReasonT
		{
			get { return JobComInvoiceLine.DrawbackLineAmberReasonCode == JobDeclaration.DrawbackAmberReasonTypes.Time ? "Y" : ""; }
		}

		public ZString DrawbackExceptionComment
		{
			get
			{
				ZString result;
				if (JobComInvoiceLine.DrawbackDutyAmount <= 0)
				{
					if (JobComInvoiceLine.DrawbackCustomsValue == 0)
					{
						result = "Cannot calculate claim amount";
					}
					else
					{
						result = "Duty free";
					}
				}
				else if (JobComInvoiceLine.IsDrawbackLineValueOverriden)
				{
					result = "Claim amount manual override";
				}
				else if (JobComInvoiceLine.IsDrawbackClaimAmountUnusual)
				{
					ZStringBuilder builder = new ZStringBuilder();
					builder.Append("Unusual claim amount, average customs value (times this quantity), over the preceding ");
					builder.Append(CustomsDataRegistry.Instance.HistoryWindowForExceptionReportOfUnusualUnitValue.Value.ToString().Trim());
					builder.Append(" months, is ");
					builder.Append(decimal.Round(JobComInvoiceLine.AverageCustomsValue * JobComInvoiceLine.JI_CustomsQuantity, 2).ToString().Trim());
					result = builder.ToString();
				}
				else
				{
					result = JobComInvoiceLine.DrawbackCalculationMethodComment;
				}
				return result;
			}
		}

		#endregion

		#region Wrapper Fields

		public DocJobComInvoiceHeader ComInvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)base.BaseComInvoiceHeader; }
		}

		public DocCusEntryLine CusEntryLine
		{
			get { return EntryLine; }
		}

		public DocCusEntryLine EntryLine
		{
			get { return (DocCusEntryLine)base.RateEntryLine; }
		}

		//ToDo: Remove
		public DocCusEntryHeader EntryHeader
		{
			get
			{
				DocCusEntryHeader result = null;

				if (CusEntryLine != null)
				{
					result = CusEntryLine.CusEntryHeader;
				}

				return result;
			}
		}

		protected override ZString CountryOfOriginCodeCore
		{
			get { return AggregatedOriginCode; }
		}

		public DocCountry AggregatedOrigin
		{
			get { return DocCountry.New(Factory, JobComInvoiceLine.AggregatedZA_ORG); }
		}

		public ZString AggregatedOriginCode
		{
			get { return JobComInvoiceLine.AggregatedZA_ORG; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal WeightedCostByLocation
		{
			get { return JobComInvoiceLine.JI_WeightedCostByLocation; }
		}

		public ZDecimal InterimAntiDumpingDuty
		{
			get { return JobComInvoiceLine.JI_Calc_InterimAntiDumpingDuty; }
		}

		protected override ZDecimal FlatDutyPortionCore
		{
			get { return JobComInvoiceLine.JI_Calc_FlatDutyPortion; }
		}

		public ZDecimal LCTAmount
		{
			get { return JobComInvoiceLine.JI_Calc_LCTAmount; }
		}

		public ZDecimal WETAmount
		{
			get { return JobComInvoiceLine.JI_Calc_WETAmount; }
		}

		public ZDecimal DrawbackCustomsValue
		{
			get { return JobComInvoiceLine.DrawbackCustomsValue; }
		}

		public ZDecimal DrawbackDutyRate
		{
			get { return JobComInvoiceLine.DrawbackDutyRate; }
		}

		public ZDecimal DrawbackClaimAmount
		{
			get { return JobComInvoiceLine.DrawbackDutyAmount; }
		}

		#endregion

		#region ZBool Fields

		public ZBool IsPackToBondForLine
		{
			get { return JobComInvoiceLine.JI_IsPackToBondForLine; }
		}

		#endregion

		#region Implementation

		protected JobComInvoiceLine JobComInvoiceLine
		{
			get { return (JobComInvoiceLine)WrappedObject; }
		}

		#endregion

	}
}
