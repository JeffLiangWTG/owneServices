using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public partial class ElectronicInvoicingEligibilityDecider
	{
		#region Internal interfaces

		internal interface ITransactionHeaderWrapperBase
		{
			IGlbCompanyWrapper Company { get; }
			IGlbBranchWrapper Branch { get; }
			ZString AH_Ledger { get; }
			ZString AH_TransactionType { get; }
			ZBool AH_IsDisbursementCalc { get; }
			ZString AH_ComplianceSubType { get; }
			ZDecimal AH_GSTAmount { get; }
			ZDecimal AH_InvoiceAmount { get; }
			ZString AH_TransactionReference { get; }
			ZString AH_GovernmentAllocatedID { get; }
			ZBool AH_IsCancelled { get; }
			ZDateTime AH_PostDate { get; }

			ZString EInvoicingStatus { get; }

			IComplianceSequenceWrapper ComplianceSequence { get; }
			IEnumerable<ITransactionLineWrapper> Lines { get; }
			IEnumerable<ITaxTransactionWrapper> TaxTransactions { get; }
			IOrgHeaderCusCodeInfo HeaderTaxNumber(string cusCode);
			IOrgHeaderCusCodeInfo HeaderGSTInfo { get; }
			IOrgHeaderCusCodeInfo HeaderVATInfo { get; }

			ZBool HasOriginalTransaction { get; }
		}

		internal interface ITransactionHeaderWrapper : ITransactionHeaderWrapperBase
		{
			ITransactionHeaderWrapperBase OriginalTransaction { get; }
			ZBool IsWritingOff { get; }
			ZBool IsReverseTransaction { get; }
			ZBool IsAmendingCreditNote { get; }
			ZBool HasSubmitPivotForOriginalTransaction { get; }

			ZString OH_Category { get; }
			ZString OrgCountryCode { get; }

			/// <summary>
			/// True if the original transaction, or any other amending transactions via AH_TransactionBelongsToGroup, were eligible for eInvoicing.
			/// </summary>
			bool GetAnyRelatedTransactionsWereEligible();

			/// <summary>
			/// Get the original transaction, and any other amending transactions via AH_TransactionBelongsToGroup, in AH_PostDate order.
			/// Excludes the current transaction.
			/// </summary>
			IReadOnlyCollection<ITransactionHeaderWrapper> GetOriginalAndRelatedTransactions();
		}

		internal interface ITaxTransactionWrapper
		{
			ZString TaxSystemCode { get; }
		}

		internal interface IComplianceDocumentHeaderWrapper
		{
			IGlbCompanyWrapper Company { get; }
			ZString ADH_Ledger { get; }
			ZString ADH_TransactionType { get; }
			ZString ADH_ComplianceSubType { get; }
		}

		public interface IGlbCompanyWrapper
		{
			ZGuid PK { get; }
			IRefCountryWrapper CountryCode { get; }
		}

		public interface IGlbBranchWrapper
		{
			ZGuid PK { get; }
		}

		public interface IRefCountryWrapper
		{
			ZString Code { get; }
		}

		internal interface ITransactionLineWrapper
		{
			ITaxRateWrapper TaxRate { get; }
		}

		internal interface ITaxRateWrapper
		{
			bool IsNotReportable { get; }
			bool IsRVS { get; }
			ZString TaxType { get; }
		}

		internal interface IOrgHeaderCusCodeInfo
		{
			string Code { get; }

			string Number { get; }

			string CountryCode { get; }

			string OrgCusCode { get; }
		}

		internal interface IComplianceSequenceWrapper
		{
			ZInt XD_MaximumNumberDigits { get; }
		}

		#endregion

		#region Private sealed classes

		internal class TransactionHeaderWrapperBase : ITransactionHeaderWrapperBase
		{
			protected AccTransactionHeader header;

			public TransactionHeaderWrapperBase(AccTransactionHeader header)
			{
				this.header = Argument.NotNull(header, nameof(header));
			}

			public IGlbCompanyWrapper Company => company ?? (company = new GlbCompanyWrapper(header.Company));
			IGlbCompanyWrapper company;

			public IGlbBranchWrapper Branch => branch ?? (branch = new GlbBranchWrapper(header.Branch));
			IGlbBranchWrapper branch;

			public ZString AH_Ledger => header.AH_Ledger;

			public ZString AH_TransactionType => header.AH_TransactionType;

			public ZString AH_TransactionReference => header.AH_TransactionReference;

			public ZString AH_GovernmentAllocatedID => header.AH_GovernmentAllocatedID;

			public ZBool AH_IsDisbursementCalc => header.AH_IsDisbursementCalc;

			public ZString AH_ComplianceSubType => header.AH_ComplianceSubType;

			public ZDateTime AH_PostDate => header.AH_PostDate;

			public ZDecimal AH_GSTAmount => header.AH_GSTAmount;

			public ZDecimal AH_InvoiceAmount => header.AH_InvoiceAmount;

			public ZString OH_Category => header.Header?.OH_Category ?? ZString.Empty;

			public ZString OrgCountryCode => header.Header?.MainAddress?.Country?.Code ?? ZString.Empty;

			public ZBool AH_IsCancelled => header.AH_IsCancelled;

			public ZString EInvoicingStatus => TransactionHeader?.EInvoicingStatus ?? ZString.Empty;

			public IComplianceSequenceWrapper ComplianceSequence => complianceSequence ?? (complianceSequence = new ComplianceSequenceWrapper(header.ComplianceSequence));
			IComplianceSequenceWrapper complianceSequence;

			public IEnumerable<ITransactionLineWrapper> Lines => lines ?? (lines = ((header as TransactionHeaderWithLines)?.Lines.OfType<TransactionLine>() ?? Enumerable.Empty<TransactionLine>()).Select(line => new TransactionLineWrapper(line) as ITransactionLineWrapper).ToList());
			List<ITransactionLineWrapper> lines;

			public IEnumerable<ITaxTransactionWrapper> TaxTransactions => taxTransactions ?? (taxTransactions = header.Factory.Load<AccTaxTransaction>(new ZQuery(AccTaxTransactionSchema.ATT_AH, header.PK)).Select(x => new TaxTransactionWrapper(x) as ITaxTransactionWrapper).ToList());
			List<ITaxTransactionWrapper> taxTransactions;

			public ZBool HasOriginalTransaction => LoadOriginalTransaction() != null;

			protected TransactionHeader LoadOriginalTransaction()
			{
				if ((header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote)
					&& TransactionHeader.OriginalTransaction == null
					&& TransactionHeader is InvoicingBase headerBase
					&& !headerBase.OriginalTransactionReference.IsEmpty)
				{
					TransactionHeader.OriginalTransaction = TransactionHeader.Factory.Load<TransactionHeader>(headerBase.OriginalTransactionReference);
				}
				return TransactionHeader.OriginalTransaction;
			}

			public IOrgHeaderCusCodeInfo HeaderTaxNumber(string cusCode)
			{
				if (HeaderTaxNumbersByCode.TryGetValue(cusCode, out var result))
				{
					return result;
				}

				result = new OrgHeaderCusCodeInfo(header.Header, header.Company.Country.Code, cusCode);
				HeaderTaxNumbersByCode[cusCode] = result;
				return result;
			}
			readonly Dictionary<string, IOrgHeaderCusCodeInfo> HeaderTaxNumbersByCode = new Dictionary<string, IOrgHeaderCusCodeInfo>();

			public IOrgHeaderCusCodeInfo HeaderGSTInfo => HeaderTaxNumber(OrgCusCode.CodeTypes.GSTCode);

			public IOrgHeaderCusCodeInfo HeaderVATInfo => vatInfo ?? (vatInfo = new OrgHeaderCusCodeInfo(header.Company.OrgProxy, header.Company.Country.Code, OrgCusCode.CodeTypes.VATCode));
			IOrgHeaderCusCodeInfo vatInfo;

			protected TransactionHeader TransactionHeader
				=> transactionHeader ?? (transactionHeader = (header as TransactionHeader) ?? Factory.GetCachedReadOnlyFactory().Load<TransactionHeader>(header.PK));

			TransactionHeader transactionHeader;

			protected BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
			BusinessObjectFactory factory;
		}

		internal sealed class TransactionHeaderWrapper : TransactionHeaderWrapperBase, ITransactionHeaderWrapper
		{
			public TransactionHeaderWrapper(TransactionHeader header)
				: this(header as AccTransactionHeader)
			{
			}

			public TransactionHeaderWrapper(GovernmentInvoice header)
				: this(header as AccTransactionHeader)
			{
			}

			TransactionHeaderWrapper(AccTransactionHeader header)
				: base(header)
			{
				var originalTransaction = LoadOriginalTransaction();
				OriginalTransaction = originalTransaction != null ?
					new TransactionHeaderWrapperBase(originalTransaction) : null;

				var headerBase = TransactionHeader as InvoicingBase;
				IsWritingOff = (headerBase as IBadDebtWritingOff)?.IsWritingOff ?? false;

				IsReverseTransaction = TransactionHeader.IsReversalTransaction;
				IsAmendingCreditNote = TransactionHeader.IsAmendingCreditNote;
				HasSubmitPivotForOriginalTransaction = originalTransaction != null &&
					originalTransaction.EInvoicingTransactionPivotSubmitted != null;
			}

			public ITransactionHeaderWrapperBase OriginalTransaction { get; }

			public ZBool IsWritingOff { get; }

			public ZBool IsReverseTransaction { get; }

			public ZBool IsAmendingCreditNote { get; }

			public ZBool HasSubmitPivotForOriginalTransaction { get; }

			public bool GetAnyRelatedTransactionsWereEligible()
			{
				var originalTransaction = LoadOriginalTransaction();
				if (originalTransaction == null)
				{
					return false;
				}

				var selectQuery = $@"SELECT DISTINCT 1 AS DoesExist
FROM dbo.AccEInvoicingTransactionPivot
INNER JOIN dbo.AccTransactionHeader
	ON AIP_ParentID = AH_PK
	AND AIP_ParentTableCode = 'AH'
WHERE AIP_Status <> '{Constants.EInvoicingPivotState.Discarded}'
AND AH_TransactionBelongsToGroup = @OriginalTransactionPK

UNION

SELECT DISTINCT 1 AS DoesExist
FROM dbo.AccEInvoicingTransactionPivot
INNER JOIN dbo.AccTransactionHeader
	ON AIP_ParentID = AH_PK
	AND AIP_ParentTableCode = 'AH'
WHERE AIP_Status <> '{Constants.EInvoicingPivotState.Discarded}'
AND AH_PK = @OriginalTransactionPK
";
				var sqlParameters = new[]
				{
					ZSqlParameter.New("@OriginalTransactionPK", originalTransaction.PK, AccTransactionHeaderSchema.AH_TransactionBelongsToGroup),
				};
				var anyRelatedTransactionsWereEligibleCollection = new DynamicBusinessObjectCollection(Factory);
				anyRelatedTransactionsWereEligibleCollection.Load(selectQuery, sqlParameters);
				return anyRelatedTransactionsWereEligibleCollection.Count > 0;
			}

			public IReadOnlyCollection<ITransactionHeaderWrapper> GetOriginalAndRelatedTransactions()
			{
				var originalTransaction = LoadOriginalTransaction();
				if (originalTransaction == null)
				{
					return Array.Empty<ITransactionHeaderWrapper>();
				}

				var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, originalTransaction.PK);
				var relatedTransactions = Factory.Load<TransactionHeader>(query);

				var result = new ITransactionHeaderWrapper[] { new TransactionHeaderWrapper(originalTransaction) }
					.Concat(relatedTransactions.Cast<TransactionHeader>()
								.Where(t => t.PK != header.PK)
								.OrderBy(t => t.AH_PostDate).ThenBy(t => t.CreatedDate)
								.Select(t => new TransactionHeaderWrapper(t))
					)
					.ToArray();
				return result;
			}
		}

		sealed class TaxTransactionWrapper : ITaxTransactionWrapper
		{
			readonly AccTaxTransaction taxTransaction;

			public TaxTransactionWrapper(AccTaxTransaction taxTransaction)
			{
				this.taxTransaction = taxTransaction;
			}

			public ZString TaxSystemCode => taxTransaction.ATT_TaxSystemCode;
		}

		sealed class ComplianceDocumentHeaderWrapper : IComplianceDocumentHeaderWrapper
		{
			readonly AccComplianceDocumentHeader header;

			public ComplianceDocumentHeaderWrapper(AccComplianceDocumentHeader header)
			{
				this.header = header;
			}

			public IGlbCompanyWrapper Company => company ?? (company = new GlbCompanyWrapper(header.Company));
			IGlbCompanyWrapper company;

			public ZString ADH_Ledger => header.ADH_Ledger;

			public ZString ADH_TransactionType => header.ADH_TransactionType;

			public ZString ADH_ComplianceSubType => header.ADH_ComplianceSubType;
		}

		public sealed class GlbCompanyWrapper : IGlbCompanyWrapper
		{
			readonly GlbCompany company;

			public GlbCompanyWrapper(GlbCompany company)
			{
				this.company = company;
			}

			public IRefCountryWrapper CountryCode => countryCode ?? (countryCode = new RefCountryWrapper(company.Country));
			IRefCountryWrapper countryCode;

			public ZGuid PK => company.PK;
		}

		public sealed class GlbBranchWrapper : IGlbBranchWrapper
		{
			readonly GlbBranch branch;

			public GlbBranchWrapper(GlbBranch branch)
			{
				this.branch = branch;
			}

			public ZGuid PK => branch.PK;
		}

		sealed class RefCountryWrapper : IRefCountryWrapper
		{
			readonly RefCountry country;

			public RefCountryWrapper(RefCountry country)
			{
				this.country = country;
			}

			public ZString Code => country.Code;
		}

		sealed class TransactionLineWrapper : ITransactionLineWrapper
		{
			readonly TransactionLine line;

			public TransactionLineWrapper(TransactionLine line)
			{
				this.line = line;
			}

			public ITaxRateWrapper TaxRate => taxRate ?? (taxRate = line.TaxRate != null ? new TaxRateWrapper(line.TaxRate) : null);
			ITaxRateWrapper taxRate;
		}

		sealed class TaxRateWrapper : ITaxRateWrapper
		{
			readonly AccTaxRate taxRate;

			public TaxRateWrapper(AccTaxRate taxRate)
			{
				this.taxRate = taxRate;
			}

			public bool IsNotReportable => taxRate.IsNonReportable;
			public bool IsRVS => taxRate.AT_Type == AccTaxRate.Types.ReverseRated;
			public ZString TaxType => taxRate.AT_Type;
		}

		sealed class OrgHeaderCusCodeInfo : IOrgHeaderCusCodeInfo
		{
			public OrgHeaderCusCodeInfo(OrgHeader org, string countryCode, string orgCusCode)
			{
				Org = org;
				CountryCode = countryCode;
				OrgCusCode = orgCusCode;
			}
			OrgHeader Org { get; }

			public string Code => Org?.OH_Code;

			public string Number => Org?.CustomsCodes?.OfType<OrgCusCode>()
														.FirstOrDefault(x => x.OK_CodeType == OrgCusCode && x.OK_RN_NKCodeCountry == CountryCode)?.OK_CustomsRegNo ?? string.Empty;
			public string CountryCode { get; }

			public string OrgCusCode { get; }
		}

		sealed class ComplianceSequenceWrapper : IComplianceSequenceWrapper
		{
			public ComplianceSequenceWrapper(AccComplianceSequence complianceSequence)
			{
				XD_MaximumNumberDigits = complianceSequence?.XD_MaximumNumberDigits ?? 0;
			}

			public ZInt XD_MaximumNumberDigits { get; }
		}

		#endregion
	}
}
