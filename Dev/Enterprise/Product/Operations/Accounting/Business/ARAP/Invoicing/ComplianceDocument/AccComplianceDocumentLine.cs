using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentLine : AutoAccComplianceDocumentLine
	{
		public AccComplianceDocumentLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		AccComplianceDocumentHeader fComplianceDocumentHeader;
		public AccComplianceDocumentHeader ComplianceDocumentHeader
		{
			get
			{
				if (fComplianceDocumentHeader == null)
				{
					fComplianceDocumentHeader = Factory.Load<AccComplianceDocumentHeader>(ADL_ADH);
				}
				return fComplianceDocumentHeader;
			}
		}

		AccTransactionLinesCollection fTransactionLines;
		public AccTransactionLinesCollection TransactionLines
		{
			get
			{
				if (fTransactionLines == null)
				{
					var query = new ZQuery(AccTransactionLinesSchema.PK, ComplianceDocumentPivots.Select(c => c.ADP_AL));
					fTransactionLines = new AccTransactionLinesCollection(Factory, query);
					fTransactionLines.Load();
				}
				return fTransactionLines;
			}
		}

		AccComplianceDocumentPivotCollection fComplianceDocumentPivots;
		public AccComplianceDocumentPivotCollection ComplianceDocumentPivots
		{
			get
			{
				if (fComplianceDocumentPivots == null)
				{
					var query = new ZQuery(AccComplianceDocumentPivotSchema.ADP_ADL, PK);
					fComplianceDocumentPivots = new AccComplianceDocumentPivotCollection(Factory, query);
					fComplianceDocumentPivots.Load();
				}

				return fComplianceDocumentPivots;
			}
		}

		AccTransactionHeader[] fTransactionHeaders;

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public AccTransactionHeader[] TransactionHeaders
		{
			get
			{
				if (fTransactionHeaders == null)
				{
					var query = new ZQuery(AccTransactionHeaderSchema.PK, TransactionLines.Select(x => x.AL_AH));
					query.OrderBy = AccTransactionHeaderSchema.Constants.AH_TransactionNum + " ASC";
					fTransactionHeaders = Factory.Load<AccTransactionHeader>(query);
				}
				return fTransactionHeaders;
			}
		}

		AccChargeCode fChargeCode;
		public AccChargeCode ChargeCode
		{
			get
			{
				if (fChargeCode == null)
				{
					fChargeCode = Factory.Load<AccChargeCode>(((AccTransactionLines)TransactionLines.FirstOrDefault()).AL_AC);
				}
				return fChargeCode;
			}
		}

		AccGLHeader fGLHeader;
		public AccGLHeader GLHeader
		{
			get
			{
				if (fGLHeader == null)
				{
					fGLHeader = Factory.Load<AccGLHeader>(((AccTransactionLines)TransactionLines.FirstOrDefault()).AL_AG);
				}
				return fGLHeader;
			}
		}

		AccTaxRate fTaxRate;
		public AccTaxRate TaxRate
		{
			get
			{
				if (fTaxRate == null)
				{
					fTaxRate = Factory.Load<AccTaxRate>(((AccTransactionLines)TransactionLines.FirstOrDefault()).AL_AT);
				}
				return fTaxRate;
			}
		}

		AccInvMsg fInvMsg;
		public AccInvMsg InvMsg
		{
			get
			{
				if (fInvMsg == null)
				{
					fInvMsg = Factory.LoadTop1<AccInvMsg>(new ZQuery(AccInvMsgSchema.PK, TaxRate.AT_A9_DefaultVatClass));
				}
				return fInvMsg;
			}
		}

		[RelatedBusinessObject("ComplianceDocumentHeader")]
		[RelatedBusinessObjectTestExclude("RelatedBusinessObject ComplianceDocumentHeader (AccComplianceDocumentHeader) is an abstract class")]
		public override ZGuid ADL_ADH { get => base.ADL_ADH; set => base.ADL_ADH = value; }

		public int LocalDecimals => GlbCompany.CurrentCompany.LocalCurrency.Decimals;

		public virtual ZString Currency => GlbCompany.CurrentCompany.LocalCurrency.RX_Code;

		public virtual ZString DocumentNumber => ComplianceDocumentHeader.ADH_DocumentNumber;

		public virtual ZString Charge => ChargeCode?.AC_Code ?? GLHeader.AccountNum;

		public virtual ZString TaxID => TaxRate.AT_Code;

		public virtual ZString TaxMessage => InvMsg?.A9_Code ?? ZString.Empty;

		public virtual ZInt PostingGroup => TaxRate.AT_PostingGroupId;

		[DecimalPlaces(nameof(LocalDecimals))]
		public virtual ZDecimal Amount => LineSummaries.Sum(x => (x as TransactionLineSummary).AL_OSExTaxAmount);

		[DecimalPlaces(nameof(LocalDecimals))]
		public virtual ZDecimal TaxAmount => LineSummaries.Sum(x => (x as TransactionLineSummary).AL_OSTaxAmount);

		[DecimalPlaces(nameof(LocalDecimals))]
		public virtual ZDecimal TotalAmount => LineSummaries.Sum(x => (x as TransactionLineSummary).AL_OverseasTotal);

		[DecimalPlaces(nameof(LocalDecimals))]
		public virtual ZDecimal LocalAmount => LineSummaries.Sum(x => (x as TransactionLineSummary).AL_LocalExTaxAmount);

		[DecimalPlaces(nameof(LocalDecimals))]
		public virtual ZDecimal LocalTaxAmount => LineSummaries.Sum(x => (x as TransactionLineSummary).AL_LocalTaxAmount);

		[DecimalPlaces(nameof(LocalDecimals))]
		public virtual ZDecimal LocalTotalAmount => LineSummaries.Sum(x => (x as TransactionLineSummary).AL_LocalTotalAmount);

		TransactionLineSummaryCollection fLinesSummaries;
		public TransactionLineSummaryCollection LineSummaries
		{
			get
			{
				if (fLinesSummaries == null)
				{
					var query = new ZQuery(AccTransactionHeaderSchema.PK, TransactionHeaders.Select(x => x.PK));
					InvoicingBaseCollection collection = new InvoicingBaseCollection(Factory, query);
					collection.Load();
					fLinesSummaries = new TransactionLineSummaryCollection(Factory);
					foreach (InvoicingBase invoice in collection)
					{
						foreach (InvoicingLineBase line in invoice.GetComplianceRelatedLines(TransactionLines))
						{
							fLinesSummaries.Add(new TransactionLineSummary(line));
						}
					}
				}

				return fLinesSummaries;
			}
		}

		#endregion

		#region Default Value

		#endregion
	}
}