using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort;
using Enterprise.MasterFiles.Business;
using Enterprise.ResourceStrings.Business;

namespace Enterprise.DocumentWrappers.Accounting.DocRollUpSort
{
	[AllowPublicConstructor]
	[AllowNoStaticNew]
	public class DocARInvoiceLineForRollUp : DocumentWrapper, IDocARInvoiceLine, IGenericTransactionLinePlugIn, IRolledUpDocLine
	{
		protected DocARInvoiceLineForRollUp(BusinessObjectFactory factory) : base(null, factory)
		{
			fAmountSplittedByChargeCode = new DocAmountByChargeCodeCollection(factory);
		}

		public static DocARInvoiceLineForRollUp New(BusinessObjectFactory factory)
		{
			return new DocARInvoiceLineForRollUp(factory);
		}

		public override string ToString()
		{
			return NoDefaultPropertyErrorMessage;
		}

		#region IGenericTransactionLinePlugIn members

		GenericTransactionLineSupporter IGenericTransactionLinePlugIn.LineSupporter
		{
			get { return fGenericTransactionSupporter ?? (fGenericTransactionSupporter = new DocARInvoiceLineForRollUpGenericTransactionSupporter(this)); }
		}
		DocARInvoiceLineForRollUpGenericTransactionSupporter fGenericTransactionSupporter;

		#endregion

		class DocARInvoiceLineForRollUpGenericTransactionSupporter : GenericTransactionLineSupporter
		{
			public DocARInvoiceLineForRollUpGenericTransactionSupporter(DocARInvoiceLineForRollUp parent)
			{
				this.Parent = parent;
			}
			protected readonly DocARInvoiceLineForRollUp Parent;

			protected internal override ZDecimal GetOSAmount()
			{
				return Parent.OSAmount;
			}

			protected internal override ZDecimal GetOSExTaxAmount()
			{
				return Parent.OSExTaxAmount;
			}

			protected internal override ZString GetOSTaxDisplayNoAsterisksWithRegistryRule()
			{
				return Parent.OSTaxDisplayNoAsterisksWithRegistryRule;
			}

			protected internal override ZString GetGovernmentReportingCode()
			{
				return Parent.GovernmentReportingCode;
			}

			protected internal override ZString GetGovernmentReportingCodeHeading()
			{
				return Parent.GovernmentReportingCodeHeading;
			}

			protected internal override ZString GetTaxRateAsterisksAsNumbers()
			{
				return Parent.TaxRateAsterisksAsNumbers;
			}

			protected internal override ZString GetOSTaxDisplay()
			{
				return Parent.OSTaxDisplay;
			}

			protected internal override ZBool GetIsSpacerLine()
			{
				return Parent.IsSpacerLine;
			}

			protected internal override ZString GetDescription()
			{
				return Parent.LineDescription;
			}

			protected internal override DocChargeCode GetChargeCode()
			{
				return Parent.ChargeCode;
			}

			protected internal override DocJobHeader GetJobHeader()
			{
				return Parent.JobHeader;
			}

			protected internal override DocShipment GetShipment()
			{
				return Parent.Shipment;
			}
		}

		#region Properties

		public ZString LineDescription
		{
			get { return GetLineDescriptionFromHolder(); }
		}
		protected IZType LineDescriptionHolder;

		public void SetLineDescription(IZType description)
		{
			LineDescriptionHolder = description;
		}

		[CodeStringFinderHint(typeof(BaseDocRollUpper<ZString, ZString, DocARInvoiceLineCollection>), "GetDescriptionForRolledUpLine")]
		protected virtual ZString GetLineDescriptionFromHolder()
		{
			return LineDescriptionHolder == null ? ZString.Empty : new ZString(LineDescriptionHolder.ToString());
		}

		public ZString InvoiceLineDescriptionForPeriodicInvoice
		{
			get { return LineDescription; } //For Roll up invoice exchange rate is not displayed as different line could possibly have different exchange rate
		}

		public ZString LineDescriptionAndExchangeRate
		{
			get { return LineDescription + " " + ExchangeRateAndAmount; }
		}

		public ZDecimal OSExTaxAmount { get; set; }
		public ZShort Sequence { get; set; }

		[CodeStringFinderHint(typeof(DocARBaseInvoice), "PrepareRollUpLineForGrouping")]
		public ZString OSTaxDisplay { get; set; }

		[CodeStringFinderHint(typeof(DocARBaseInvoice), "PrepareRollUpLineForGrouping")]
		public ZString TaxAmountDisplay { get; set; }

		public ZString ExchangeRateAndAmount { get; set; }
		public ZString ChargeOSAmountAndCurrency { get; set; }
		public ZDecimal TaxRateInDecimal { get; set; }
		public ZDecimal OSGSTAmount { get; set; }
		public ZDecimal OSQSTAmount { get; set; }
		public ZDecimal OSSBCAmount { get; set; }
		public ZDecimal OSKKCAmount { get; set; }
		public ZBool IsExtraTaxSBCAndKKC { get; set; }
		public ZBool IsExtraTaxSBCOrKKC { get; set; }
		public ZDecimal OSEDUAmount { get; set; }
		public ZDecimal OSRETAmount { get; set; }
		public ZDecimal OSSPVAmount { get; set; }

		public ZBool IncludeTaxAmountInOsTaxDisplay
		{
			get { return !AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.Value; }
		}

		protected ZBool fShowPercentInGSTDisplay = ZBool.True;
		public ZBool ShowPercentInGSTDisplay
		{
			get { return fShowPercentInGSTDisplay; }
			set { fShowPercentInGSTDisplay = value; }
		}

		public ZString TaxRateAsterisks
		{
			get { return ZString.Empty; }
		}

		public ZString OSTaxDisplayNoAsterisks
		{
			get { return OSTaxDisplay; }
		}

		public ZString TaxRateAsterisksAsNumbers
		{
			get; set;
		}

		public ZString OSTaxDisplayNoAsterisksWithRegistryRule
		{
			get; set;
		}

		#endregion

		#region IDocARInvoiceLine Members

		public ZString CreatingUserName
		{
			get { return ZString.Empty; }
		}

		public ZDateTime CreatedDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZString FixedPlaceOfSupply { get; set; }

		public ZString FixedPlaceOfSupplyLabel { get; set; }

		public DocChargeCode ChargeCode { get; set; }

		public DocGlobalChargeCodeCollection ARGlobalChargeCodes
		{
			get
			{
				if (ChargeCode != null)
				{
					return new DocGlobalChargeCodeCollection(Invoice.Invoice.GetGlobalChargeCodes(ChargeCode.ChargeCodePK), Factory);
				}
				else
				{
					return new DocGlobalChargeCodeCollection(Factory);
				}
			}
		}

		public DocGLAccount GLAccount
		{
			get { return null; }
		}

		public DocGLAccount PercentOfGLAccount
		{
			get { return null; }
		}

		public DocARInvoice Invoice { get; set; }

		public DocWithholdingTaxRate WithholdingTaxRate
		{
			get { return null; }
		}

		public ZDecimal ExchangeRate { get; set; }

		public DocBranch Branch
		{
			get { return null; }
		}

		public DocDepartment Department
		{
			get { return null; }
		}

		public DocTaxRate TaxRate { get; set; }

		public ZDecimal TaxRateAmount_Raw { get; set; }

		public ZDecimal TaxExtraRateAmount { get; set; }

		public ZDecimal GSTVAT { get; set; }

		public ZDecimal OSTaxAmount { get; set; }

		public ZDecimal OSSERAmount { get; set; }

		public DocJobHeader JobHeader { get; set; }

		public ZString JobNumber { get { return JobHeader != null ? JobHeader.JobNumber : ZString.Empty; } }

		public ZString JobTypeForPeriodicInvoice
		{
			get { return ZString.Empty; }
		}

		public FreightWrapper OperationsJob
		{
			get
			{
				return OpJobHelper.OperationsJob;
			}
		}

		public ZDecimal LineAmount { get; set; }

		public ZString LineType
		{
			get { return ZString.Empty; }
		}

		public DocOrganisation Organisation
		{
			get { return null; }
		}

		public ZDecimal OSAmount
		{
			get { return OSTaxAmount + OSExTaxAmount; }
		}

		public ZDecimal OSUnitPrice
		{
			get { return 0M; }
		}

		public ZInt PercentageOfPeriod
		{
			get { return 0; }
		}

		public ZDateTime PostDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZInt PostPeriod
		{
			get { return 0; }
		}

		public ZBool PostToGL
		{
			get { return ZBool.False; }
		}

		public ZBool PreventInvoicePrintGrouping
		{
			get { return ZBool.False; }
		}

		public ZDateTime ReverseDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZInt ReversePeriod
		{
			get { return 0; }
		}

		public ZBool ReverseToGL
		{
			get { return ZBool.False; }
		}

		public ZDecimal UnitPrice
		{
			get { return 0M; }
		}

		public ZInt UnitQty
		{
			get { return 0; }
		}

		public ZDecimal WithholdingTax
		{
			get { return 0M; }
		}

		public DocCurrency Currency { get; set; }
		public DocCurrency DisplayCurrency
		{
			get { return Currency; }
		}
		public ZDecimal ChargeExchangeRate { get; set; }
		public ZDecimal ChargeOSAmount { get; set; }
		public ZDecimal ChargeOSAmountForCLC { get; set; }
		public ZString ChargeCurrency { get; set; }
		public DocShipment Shipment { get; set; }
		public ZString FKToShipment { get; set; }

		public DocOrganisation HeaderOrganisation
		{
			get { return null; }
		}

		public DocCurrency HeaderCurrency
		{
			get { return null; }
		}

		public ZDecimal LocalAmountAndTax
		{
			get { return LineAmount + GSTVAT; }
		}

		public ZBool HasBeenSubTotalled
		{
			get { return false; }
		}

		public ZBool IsSubTotalLine { get; set; }

		public ZBool IsSpacerLine { get; set; }

		public ZBool IsCommentLine
		{
			get
			{
				return (ChargeCode != null && ChargeCode.ChargeType == "CMT");
			}
		}

		public ZBool IsRollUpLine
		{
			get; set;
		}

		public DocJobInvoicingJobCharge Charge
		{
			get { return null; }
		}

		public ZString TaxRateDisplay
		{
			get { return ZString.Empty; }
		}

		public ZString OSAmountDisplay
		{
			get { return ZString.Empty; }
		}

		public ZString ExchangeRateDisplay
		{
			get { return ZString.Empty; }
		}

		public ZString ClientReference
		{
			get { return ZString.Empty; }
		}

		public ZString ContainerNumbers
		{
			get { return ZString.Empty; }
		}

		public ZString SubInvoiceRef
		{
			get { return ZString.Empty; }
		}

		OperationsJobHelper OpJobHelper
		{
			get
			{
				if (jobHelper == null)
				{
					jobHelper = new OperationsJobHelper(JobHeader, Shipment, Charge, Factory);
				}
				return jobHelper;
			}
		}
		OperationsJobHelper jobHelper;

		public ZBool IsCustomJob
		{
			get { return ZBool.False; }
		}

		public ZBool IsNCTSJob
		{
			get { return ZBool.False; }
		}

		public ZBool IsLocalCartage
		{
			get { return ZBool.False; }
		}

		public ZBool IsCFSShipmentJob
		{
			get { return ZBool.False; }
		}

		public ZBool IsLoadListJob
		{
			get { return ZBool.True; }
		}

		public DocLoadListConsol LoadList
		{
			get { return null; }
		}

		public ZString OtherReference
		{
			get { return ZString.Empty; }
		}

		public ZString ShippersReference
		{
			get { return ZString.Empty; }
		}

		public DocBaseJobDeclaration Customs
		{
			get { return null; }
		}

		public DocCommonCartage LocalTransport
		{
			get { return null; }
		}

		// The Below "ForTotal" properties are ugly but have to be done this way to work-around DocEngine totalling using a filter.
		// When sub-totalling, we do not want the totals to be included in the calculation as the "original" lines are still there
		// However, when rolling-up, we DO need the totals on the "rolled-up" lines for total purposes as the "original" lines are not shown.
		public ZDecimal OSAmountForTotal
		{
			get { return NotUsedForTotal ? ZDecimal.Zero : OSAmount; }
		}

		public ZDecimal OSExTaxAmountForTotal
		{
			get { return NotUsedForTotal ? ZDecimal.Zero : OSExTaxAmount; }
		}

		public ZDecimal LineAmountForTotal
		{
			get { return NotUsedForTotal ? ZDecimal.Zero : LineAmount; }
		}

		public ZDecimal GSTVATForTotal
		{
			get { return NotUsedForTotal ? ZDecimal.Zero : GSTVAT; }
		}

		public ZDecimal LocalAmountAndTaxForTotal
		{
			get { return NotUsedForTotal ? ZDecimal.Zero : LocalAmountAndTax; }
		}

		public ZString TaxAmountDisplayForTotal
		{
			get { return NotUsedForTotal ? ZString.Empty : TaxAmountDisplay; }
		}

		public ZString OSTaxDisplayForTotal
		{
			get { return NotUsedForTotal ? ZString.Empty : OSTaxDisplay; }
		}

		public ZString TaxGroupCode => ZString.Empty;

		public DocAmountByChargeCodeCollection AmountSplittedByChargeCode
		{
			get { return fAmountSplittedByChargeCode; }
		}

		readonly DocAmountByChargeCodeCollection fAmountSplittedByChargeCode;

		bool NotUsedForTotal
		{
			get { return IsSpacerLine || IsCommentLine || IsSubTotalLine; }
		}

		public ZString SellRecognition
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString CostRecognition
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString ProductName
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZBool IsApproved
		{
			get
			{
				return ZBool.False;
			}
		}

		public ZString SellAccount
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString CostAccount
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZBool IsApportioned
		{
			get
			{
				return ZBool.False;
			}
		}

		public ZBool CostPosted
		{
			get
			{
				return ZBool.False;
			}
		}

		public ZBool SellPosted
		{
			get
			{
				return ZBool.False;
			}
		}

		public ZDecimal CFXJnl
		{
			get
			{
				return new ZDecimal();
			}
		}

		public ZShort ChargeCodePrintSequence
		{
			get
			{
				return new ZShort();
			}
		}

		public ZInt DisplaySequence
		{
			get
			{
				return new ZInt();
			}
		}

		public ZString EstimatedCostWithCurrency
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZDecimal EstimatedCost
		{
			get
			{
				return new ZDecimal();
			}
		}

		public ZString EstimatedRevenueWithCurrency
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZDecimal EstimatedRevenue
		{
			get
			{
				return new ZDecimal();
			}
		}

		public ZString LocalCostAmountWithCurrency
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZDecimal LocalCostAmount
		{
			get
			{
				return new ZDecimal();
			}
		}

		public ZString LocalSellAmountWithCurrency
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZDecimal LocalSellAmount
		{
			get
			{
				return new ZDecimal();
			}
		}

		public ZString CostOSAmountAndCurrency
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZDecimal CostOSAmount
		{
			get
			{
				return new ZDecimal();
			}
		}

		public ZDecimal CostExchangeRate
		{
			get
			{
				return new ZDecimal();
			}
		}

		public ZString CostCurrency
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString LayoutWhenPrintedInPeriodicInvoice
		{
			get { return ZString.Empty; }
		}

		public ZString SecondaryLayoutWhenPrintedInPeriodicInvoice
		{
			get { return ZString.Empty; }
		}

		public ZDecimal OSIntegratedGSTAmount { get; set; }
		public ZDecimal OSCentreGSTAmount { get; set; }
		public ZDecimal OSStateGSTAmount { get; set; }
		public ZString Quantity => string.Empty;
		public ZBool DisplayTaxGroupCode { get; set; }

		#endregion

		#region ISortableDocLine Members

		int ISortableDocLine.OrgLevelSortOrder
		{
			get
			{
				int result = 0;
				DocOrganisation org = Organisation ?? Invoice.Organisation;
				if (org != null)
				{
					foreach (AccClientInvoiceOrder invoiceOrder in org.InvoiceOrders)
					{
						if ((invoiceOrder.AI_InvoiceType == Invoice.TransactionCategory || invoiceOrder.AI_InvoiceType == "ALL" || invoiceOrder.AI_InvoiceType.IsEmpty) &&
							ChargeCode != null && invoiceOrder.AI_AC == ChargeCode.ChargeCodePK)
						{
							result = invoiceOrder.AI_PrintOrder;
							break;
						}
					}
				}
				return result;
			}
		}

		int ISortableDocLine.ChargePrintSeqSortOrder
		{
			get { return ChargeCode != null ? (int)ChargeCode.Sequence : 0; }
		}

		int ISortableDocLine.UserEnteredSortOrder
		{
			get { return Sequence; }
		}

		string ISortableDocLine.AlphabeticalSortOrder
		{
			get { return LineDescription; }
		}

		#endregion

		#region Governemnt Reporting

		public ZString GovernmentReportingCode => ZString.Empty;

		public ZString GovernmentReportingCodeHeading => ZString.Empty;

		#endregion

		public ZDateTime TaxDate => ZDateTime.Empty;
	}
}
