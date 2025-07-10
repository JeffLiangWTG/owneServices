using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCusEntryLine : DocBaseCusEntryLine
	{
		DocCusEntryLine(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
			: base(cusEntryLine, factoryToWrap)
		{
		}

		public static DocCusEntryLine New(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
		{
			if (cusEntryLine == null)
			{
				return null;
			}
			else
			{
				return new DocCusEntryLine(cusEntryLine, factoryToWrap);
			}
		}

		#region Overrides

		protected override DocBaseJobComInvoiceLine CreateJobComInvoiceLine(Enterprise.Customs.Business.BaseJobComInvoiceLine invoiceLineToWrap)
		{
			return DocJobComInvoiceLine.New((JobComInvoiceLine)invoiceLineToWrap, Factory);
		}

		#endregion

		#region Collections

		public DocJobComInvoiceLine InvoiceLine
		{
			get { return (DocJobComInvoiceLine)InvoiceLineInternal; }
		}

		public DocCMRCusEntryCPDecCollection Questions
		{
			get
			{
				if (fQuestions == null)
				{
					fQuestions = new DocCMRCusEntryCPDecCollection(CusEntryLine.Questions, Factory);
					fQuestions.Sort(DocCMRCusEntryCPDec.Schema.QuestionNum, ListSortDirection.Ascending);
				}
				return fQuestions;
			}
		}
		DocCMRCusEntryCPDecCollection fQuestions;

		#endregion

		#region Wrapper Fields

		public DocCusEntryHeader CusEntryHeader
		{
			get { return DocCusEntryHeader.New(CusEntryLine.Header, Factory); }
		}

		#endregion

		#region ZString Fields

		public ZString FormattedContainers
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				foreach (var container in Containers)
				{
					result.AppendFormat("{0}({1})", container.CO_FCL_LCL_AIR, container.CO_ContainerNumber);
					result.Append(",");
				}
				return result.ToString().TrimEnd(',');
			}
		}

		List<CusContainer> Containers
		{
			get
			{
				if (containerList == null)
				{
					var containersPivot = CusEntryLine.ContainersPivot;
					containerList = new List<CusContainer>();
					if (containersPivot != null && !CusEntryLine.IsNonAQISAEPLine)
					{
						containerList.AddRange(CusEntryLine.ContainersPivot.Cast<CusContainerInvoiceLinePivot>().Select(x => x.Container));
					}
				}
				return containerList;
			}
		}
		List<CusContainer> containerList;

		public ZString TariffNumber
		{
			get { return CusEntryLine.TariffNumber; }
		}

		public ZString StatCode
		{
			get { return CusEntryLine.StatCode; }
		}

		public ZString TreatmentCode
		{
			get { return CusEntryLine.TreatmentCode; }
		}

		public ZString TCNumber
		{
			get { return CusEntryLine.InvoiceLineAddInfo.ZA_TC2; }
		}

		public ZString ORG
		{
			get { return CusEntryLine.ORG; }
		}

		public ZString PRF
		{
			get { return CusEntryLine.PRF; }
		}

		public ZDecimal LineQuantity
		{
			get { return CusEntryLine.Quantity; }
		}

		public ZString LineQuantityUnit
		{
			get { return CusEntryLine.UnitOfQuantity; }
		}

		public ZString POC
		{
			get { return CusEntryLine.POC; }
		}

		public ZString PST
		{
			get { return CusEntryLine.PST; }
		}

		public ZString PRT
		{
			get { return CusEntryLine.PRT; }
		}

		public ZBool ShouldShowPreferenceDetails
		{
			get { return !POC.IsEmpty || !PST.IsEmpty || !PRT.IsEmpty; }
		}

		public ZString PreferenceDetails => PST == AUAddInfo.GeneralPreferenceRate ? string.Format(CultureInfo.CurrentCulture, "({0})", PST) :
			string.Format(CultureInfo.CurrentCulture, "({0},{1},{2})", POC, PST, PRT);

		public ZString VID
		{
			get
			{
				ZString[] vIDs = CusEntryLine.OrderedVIDs;

				ZStringBuilder result = new ZStringBuilder();
				foreach (ZString vID in vIDs)
				{
					result.Append(vID);
					result.Append(",");
				}
				return result.ToString().TrimEnd(',');
			}
		}

		public ZString MultipleClearanceCode
		{
			get { return CusEntryLine.WMC; }
		}

		public ZString WarehouseQuantityUnit
		{
			get { return CusEntryLine.WRU; }
		}

		public ZString WarehouseQuantityUnitForATD
		{
			get
			{
				ZString result;
				if (WarehouseQuantity > 0 && WarehouseQuantityUnit != CustomsUnitQty)
				{
					result = WarehouseQuantityUnit;
				}
				else
				{
					result = CustomsUnitQty;
				}

				return result;
			}
		}

		public ZString WarehouseReferenceNumber
		{
			get { return CusEntryLine.WRN; }
		}

		public ZString FormattedLineNumber
		{
			get { return LineNumber.ToString().PadLeft(3, '0'); }
		}

		public ZDecimal PriceAmount
		{
			get { return CusEntryLine.Price.Amount; }
		}

		public ZDecimal DutyAmountIncludingWHEstimate
		{
			get { return CusEntryLine.DutyAmountIncludingWHEstimate; }
		}

		public ZString FormattedDutyRateIncludingWHEstimate
		{
			get
			{
				ZStringBuilder dutyRates = new ZStringBuilder();
				dutyRates.Append(CusEntryLine.CL_DutyPercent.ToString(2));
				dutyRates.Append("%");
				if (CusEntryLine.CL_FlatAmount != 0m && !CusEntryLine.CL_FlatAmountUQ.IsEmpty)
				{
					dutyRates.Append("+");
					dutyRates.Append(CusEntryLine.CL_FlatAmount.ToString(5));
					dutyRates.Append("/");
					dutyRates.Append(CusEntryLine.CL_FlatAmountUQ);
				}
				return dutyRates.ToString();
			}
		}

		public ZDecimal GSTAmount
		{
			get
			{
				return CusEntryLine.GSTVATAmount.IsEmpty ? CusEntryLine.GSTVATDeferred : CusEntryLine.GSTVATAmount;
			}
		}

		public ZDecimal GSTIncludingWHEstimateAmount
		{
			get
			{
				return CusEntryLine.GSTVATAmountIncludingWHEstimate.IsEmpty ? CusEntryLine.GSTVATDeferred : CusEntryLine.GSTVATAmountIncludingWHEstimate;
			}
		}

		public ZString FormattedInstrument
		{
			get
			{
				return CusEntryLine.InstrumentType + " " + CusEntryLine.InstrumentCode;
			}
		}

		public ZBool IsDutyAndTaxEstimatedForWH
		{
			get
			{
				return CusEntryLine.IsDutyAndTaxEstimatedForWH;
			}
		}

		public ZBool IsNature20 => CusEntryLine.IsNature20;

		public ZString FormattedValuationBasis
		{
			get
			{
				string valuationBasis = CusEntryLine.RandomLine.AddInfo.AggregatedZA_ValuationBasis_Hidden;
				if (!CusEntryLine.PriceAdjustment.IsEmpty)
				{
					valuationBasis += "+ADJ";
				}
				return valuationBasis;
			}
		}

		public ZString FormattedValuationBasisForCMR
		{
			get
			{
				string valuationBasis = (CusEntryLine != null && CusEntryLine.REL == "Y") ? "RT/" : "UT/";
				valuationBasis += CusEntryLine.ValuationBasisForCMR;
				if (!CusEntryLine.PriceAdjustment.IsEmpty)
				{
					valuationBasis += "+ADJ";
				}
				return valuationBasis;
			}
		}

		public ZString AddInfoLine
		{
			get
			{
				return CusEntryLine.AddInfoLine;
			}
		}

		public ZString VOTILine
		{
			get
			{
				return CusEntryHeader.GetVOTILine(CusEntryLine);
			}
		}

		public ZString FormattedWarehouseUnitValue
		{
			get
			{
				ZString result = ZString.Empty;
				if (CusEntryLine.IsNature20 || CusEntryLine.IsNature30)
				{
					result = CusEntryLine.WarehouseUnitValue.Amount.ToString(4).PadLeft(12);
				}
				return result;
			}
		}

		public ZString SupplierCustomsClientID
		{
			get
			{
				return CusEntryLine.SupplierCode;
			}
		}

		public ZString SupplierFullNameTruncated
		{
			get
			{
				return CusEntryLine.Supplier != null ? CusEntryLine.Supplier.OH_FullNameTruncated : ZString.Empty;
			}
		}

		public ZString FormattedpermitNumbers
		{
			get
			{
				return CusEntryLine.OrderedAQISPermitIds.ReBuildAQISElements();
			}
		}

		#endregion

		#region ZInt Fields
		public ZInt WarehouseRelatedLineNo
		{
			get { return CusEntryLine.WRL; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal FlatDutyPortion
		{
			get { return CusEntryLine.FlatDutyPortion; }
		}

		public ZDecimal InterimAntiDumpingDuty
		{
			get { return CusEntryLine.InterimAntiDumpingDuty; }
		}

		public ZDecimal InterimCountervailingDuty
		{
			get { return CusEntryLine.InterimCountervailingDuty; }
		}

		public ZDecimal LCTAmount
		{
			get { return CusEntryLine.LCTAmount; }
		}

		public ZDecimal WETAmount
		{
			get { return CusEntryLine.WETAmount; }
		}

		public ZDecimal WarehouseQuantity
		{
			get { return CusEntryLine.WRQ; }
		}

		public ZDecimal WarehouseQuantityForATD
		{
			get
			{
				ZDecimal result = 0M;

				if (WarehouseQuantity > 0 && WarehouseQuantityUnit != CustomsUnitQty)
				{
					result = WarehouseQuantity;
				}
				else
				{
					result = CustomsQuantity;
				}

				return result;
			}
		}

		#endregion

		#region ZBool Fields
		public ZBool IsTrailer
		{
			get { return CusEntryLine.IsTrailer; }
		}

		#endregion

		#region Implementation

		CusEntryLine CusEntryLine
		{
			get { return (CusEntryLine)WrappedObject; }
		}

		#endregion
	}
}
