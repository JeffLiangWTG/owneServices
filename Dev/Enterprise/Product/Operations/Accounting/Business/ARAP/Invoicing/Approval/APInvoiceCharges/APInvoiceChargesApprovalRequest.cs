using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.EmailNotification;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[CodeProperty(Schema.XP_RequestID), DescriptionProperty(Schema.XP_ReasonDescription)]
	public class APInvoiceChargesApprovalRequest : InvoicingBaseApprovalRequest<APInvoiceChargesApprovalRequestDetails>, IAccountingNumberFountainDataSource
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new abstract class Schema : InvoicingBaseApprovalRequest<APInvoiceChargesApprovalRequestDetails>.Schema
		{
			public const string RequisitionStatus = "RequisitionStatus";
			public const string RequisitionDate = "RequisitionDate";
		}

		#endregion

		public APInvoiceChargesApprovalRequest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void InitializeJobRelated(APInvoiceCharges invoiceCharges, ZGuid parentID, string parentTableCode)
		{
			Argument.NotNull(invoiceCharges, "invoiceCharges");

			base.Initialize(parentID, parentTableCode);

			PostingDetails.Creditor = invoiceCharges.Creditor;
			PostingDetails.TransactionNumber = invoiceCharges.InvoiceNumber;

			var tempFactoryForInvoice = new BusinessObjectFactory() { RefreshEnabled = false };
			tempFactoryForInvoice.SuspendValidation();
			var temporaryInvoiceToGetCorrectValues = tempFactoryForInvoice.New<APInvoice>();
			APInvoiceCreator.SetInvoiceHeadeValues(temporaryInvoiceToGetCorrectValues, invoiceCharges);
			BranchPK = temporaryInvoiceToGetCorrectValues.AH_GB;
			DepartmentPK = temporaryInvoiceToGetCorrectValues.AH_GE;
			CreditorPK = temporaryInvoiceToGetCorrectValues.AH_OH;
			DueDate = temporaryInvoiceToGetCorrectValues.AH_DueDate;
			InvoiceDate = temporaryInvoiceToGetCorrectValues.AH_InvoiceDate;
			DocumentReceivedDate = temporaryInvoiceToGetCorrectValues.AH_DocumentReceivedDate;
			InvoiceCurrency = temporaryInvoiceToGetCorrectValues.AH_RX_NKTransactionCurrency;
			InvoiceLocalTotalAmount = -invoiceCharges.AH_LocalTotalAmount;
			InvoiceOSTotalAmount = InvoiceCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency ? InvoiceLocalTotalAmount : (ZDecimal)(-invoiceCharges.AH_OSTotalAmount);
			InvoiceLocalTaxAmount = -invoiceCharges.AH_LocalTaxAmount;
			InvoiceLocalExTaxAmount = -invoiceCharges.AH_LocalExTaxAmount;

			PostingDetails.Charges.RemoveAndDeleteAll();

			foreach (Charge invoiceCharge in invoiceCharges.Charges)
			{
				var charge = PostingDetails.Charges.AddNew();
				charge.JobNumber = invoiceCharge.Job.JH_JobNum;
				charge.ChargeCode = invoiceCharge.ChargeCode.AC_Code;
				charge.Branch = invoiceCharge.Branch.GB_Code;
				charge.Department = invoiceCharge.Department.GE_Code;
				if (!invoiceCharge.JR_Desc.IsEmpty)
				{
					charge.Description = invoiceCharge.JR_Desc;
				}
				if (!invoiceCharge.JR_CostTaxDate.IsEmpty)
				{
					charge.TaxDate = invoiceCharge.JR_CostTaxDate;
				}
				charge.AccInvMsgPK = invoiceCharge.JR_A9_CostVATClass;
				charge.CostCurrency = invoiceCharge.JR_RX_NKCostCurrency;
				charge.OSCostAmount = invoiceCharge.JR_OSCostAmtWithGSTAmt;
				charge.LocalCostAmount = invoiceCharge.JR_Calc_LocalCostAmtWithGST;
				charge.PlaceOfSupply = invoiceCharge.JR_CostPlaceOfSupply;
				charge.PlaceOfSupplyType = invoiceCharge.JR_CostPlaceOfSupplyType;
			}

			PostingDetails.MaxAmountToApprove = invoiceCharges.AH_LocalTotalAmount;

			if (!temporaryInvoiceToGetCorrectValues.AH_Desc.IsEmpty)
			{
				PostingDetails.Description = temporaryInvoiceToGetCorrectValues.AH_Desc;
			}
			if (!temporaryInvoiceToGetCorrectValues.AH_InvoiceTerm.IsEmpty)
			{
				PostingDetails.InvoiceTerm = temporaryInvoiceToGetCorrectValues.AH_InvoiceTerm;
				PostingDetails.InvoiceTermDays = temporaryInvoiceToGetCorrectValues.AH_InvoiceTermDays;
			}
		}

		public void InitializeInvoiceRelated(InvoicingBase invoice)
		{
			Argument.NotNull(invoice, "invoice");

			base.Initialize(invoice.PK, invoice.TablePrefix);

			invoiceToLink = invoice;

			string orgCode = "";
#if DEBUG
			if (!Globals.IsTest || invoice.Header != null)
#endif
			{
				orgCode = invoice.Header.OH_Code;
			}

			PostingDetails.Creditor = orgCode;
			PostingDetails.TransactionNumber = invoice.AH_TransactionNum;

			PostingDetails.Charges.RemoveAndDeleteAll();

			foreach (InvoicingLineBase line in invoice.Lines)
			{
				var charge = PostingDetails.Charges.AddNew();
				charge.JobNumber = line.JobNumber;
				string chargeCode = "";
#if DEBUG
				if (!Globals.IsTest || line.GenericChargeBizO != null)
#endif
				{
					chargeCode = line.GenericChargeBizO.VC_Code;
				}
				charge.ChargeCode = chargeCode;
				charge.Branch = line.Branch.GB_Code;
				charge.Department = line.Department.GE_Code;
				if (!line.AL_Desc.IsEmpty)
				{
					charge.Description = line.AL_Desc;
				}
				if (!line.AL_TaxDate.IsEmpty)
				{
					charge.TaxDate = line.AL_TaxDate;
				}
				charge.AccInvMsgPK = line.AL_A9_VATClass;
				charge.CostCurrency = line.AL_RX_NKTransactionCurrency;
				charge.OSCostAmount = -line.AL_OSAmount;
				charge.LocalCostAmount = -(line.AL_LineAmount + line.AL_GSTVAT);
				charge.PlaceOfSupply = line.AL_PlaceOfSupply;
				charge.PlaceOfSupplyType = line.AL_PlaceOfSupplyType;
			}

			PostingDetails.MaxAmountToApprove = invoice.AH_LocalTotalAmount;
			if (!invoice.AH_Desc.IsEmpty)
			{
				PostingDetails.Description = invoice.AH_Desc;
			}
			if (!invoice.AH_InvoiceTerm.IsEmpty)
			{
				PostingDetails.InvoiceTerm = invoice.AH_InvoiceTerm;
				PostingDetails.InvoiceTermDays = invoice.AH_InvoiceTermDays;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Combining Cache key, not related to GUI")]
		public bool IsAllowedtoViewTransactionOutsideLoginPermission()
		{
			var result = true;
			var invoice = LinkedIncompleteInvoiceHeaderOnly;
			if (invoice != null)
			{
				var isAllowedtoViewTransaction = Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed;
				if (!isAllowedtoViewTransaction && invoice.Branch != null && invoice.Department != null)
				{
					if (invoice.Branch != GlbBranch.CurrentBranch || invoice.Department != GlbDepartment.CurrentDepartment)
					{
						result = Factory.GetCachedValue("Login BRN:" + invoice.Branch.GB_Code + " DEP:" + invoice.Department.GE_Code, delegate
						{
							var security = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, invoice.AH_GB.ToGuid(), invoice.AH_GE.ToGuid());
							return security.Login.IsAllowed;
						});
					}
				}
			}
			return result;
		}

		#region Properties

		#region Branch

		public ZString Branch => BranchBizo?.GB_Code ?? ZString.Empty;

		GlbBranch BranchBizo => Factory.Load<GlbBranch>(IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null ? LinkedIncompleteInvoiceHeaderOnly.AH_GB : BranchPK);

		public ZPropertyInfo BranchInfo
		{
			get { return GetZPropertyInfo(nameof(Branch)); }
		}

		#endregion

		#region TaxBranch

		public ZString TaxBranch => TaxBranchBizo?.GB_Code ?? ZString.Empty;

		GlbBranch TaxBranchBizo => Factory.Load<GlbBranch>(IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null ? LinkedIncompleteInvoiceHeaderOnly.AH_GB_TaxBranch : TaxBranchPK);

		public ZPropertyInfo TaxBranchInfo
		{
			get { return GetZPropertyInfo(nameof(TaxBranch)); }
		}

		#endregion

		#region Department

		public ZString Department => DepartmentBizo?.GE_Code ?? ZString.Empty;

		GlbDepartment DepartmentBizo => Factory.Load<GlbDepartment>(IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null ? LinkedIncompleteInvoiceHeaderOnly.AH_GE : DepartmentPK);

		public ZPropertyInfo DepartmentInfo
		{
			get { return GetZPropertyInfo(nameof(Department)); }
		}

		#endregion

		#region FormattedChargeDetails

		[ResourceStringData("ChargeDetails", Caption = "Charge Codes")]
		public ZString FormattedChargeDetails
		{
			get
			{
				if (!FormattedChargeDetails_cached.HasValue)
				{
					var takeAmount = 10;
					var amounts = (
						from APInvoiceChargesApprovalRequestChargeDetails charge in PostingDetails.Charges
						group charge by charge.ChargeCode into chargeGroup
						select string.Format("{0} {1}", chargeGroup.Key, ((ZDecimal)chargeGroup.Sum(charge => charge.LocalCostAmount)).ToString(Env.CurrentCompany.LocalCurrency.Decimals))
						).ToArray();
					var amountsString = new ZStringBuilder(amounts.Take(takeAmount)).ToStringWithDelimiterBetweenAppends(", ");

					var stringBuilder = new ZStringBuilder(string.Format("{0}{1}", amountsString, amounts.Length > takeAmount ? "..." : ""));
					FormattedChargeDetails_cached = stringBuilder.ToString();
				}
				return FormattedChargeDetails_cached ?? ZString.Empty;
			}
		}
		ZString? FormattedChargeDetails_cached;

		void ResetFormattedChargeDetails()
		{
			FormattedChargeDetails_cached = null;
		}

		#endregion

		protected override ZString PostingOptionCore
		{
			get
			{
				return PostingDetails.PostingOption;
			}
		}

		[ResourceStringData("JobNumberAsRefNumber", Caption = "Reference Number", ShortCaption = "Ref. Num.", MediumCaption = "Ref. Number")]
		public override ZString JobNumber
		{
			get
			{
				if (!JobNumber_cached.HasValue)
				{
					if (IsTransactionRelated)
					{
						var invoice = LinkedIncompleteInvoiceHeaderOnly;
						if (invoice != null)
						{
							JobNumber_cached = invoice.AH_TransactionNum;
						}
					}
				}

				return base.JobNumber;
			}
		}

		[ResourceStringData("TransactionType", Caption = "Transaction Type", ShortCaption = "Tran. Type")]
		public ZString TransactionType
		{
			get
			{
				var type = "";
				switch (XP_ParentTableCode)
				{
					case AccTransactionHeaderSchema.Constants.Prefix:
						var invoice = LinkedIncompleteInvoiceHeaderOnly;
						if (invoice != null)
						{
							var invoiceTypes = new[] { TransactionTypes.IncompleteInvoice, TransactionTypes.Invoice };
							var creditNoteTypes = new[] { TransactionTypes.IncompleteCreditNote, TransactionTypes.CreditNote };

							type = invoice.AH_TransactionType;

							if (XP_ApprovalStatus != Constants.GenApprovalRequestApprovalStatus.Posted)
							{
								if (invoiceTypes.Contains(type))
								{
									type = TransactionTypes.UAInvoice;
								}
								if (creditNoteTypes.Contains(type))
								{
									type = TransactionTypes.UACreditNote;
								}
							}
							else
							{
								if (invoiceTypes.Contains(type))
								{
									type = TransactionTypes.Invoice;
								}
								if (creditNoteTypes.Contains(type))
								{
									type = TransactionTypes.CreditNote;
								}
							}
						}
						break;
					default:
						type = TransactionTypes.UAInvoice;
						if (XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Posted)
						{
							type = TransactionTypes.Invoice;
						}
						break;
				}

				return type;
			}
		}

		public ZString FormatedRequestId
		{
			get
			{
				if (IsTransactionRelated)
				{
					return Res.GetString("AB6E52B8-D346-4D69-9EA4-CB0F2195DF35", "Creditor: {0}, Transaction Number: {1}", PostingDetails.Creditor, PostingDetails.TransactionNumber);
				}
				else
				{
					return Res.GetString("69a2b3de-39a1-4836-89fd-31b29418ccf6", "Job: {0}, Creditor: {1}, Transaction Number: {2}", JobNumber, PostingDetails.Creditor, PostingDetails.TransactionNumber);
				}
			}
		}

		ZGuid BranchPK
		{
			get { return GetAddOnColumnValueAsZGuid(AddOnColumnNames.BranchPK) ?? ZGuid.Empty; }
			set { SetAddOnColumnValue(AddOnColumnNames.BranchPK, value); }
		}

		ZGuid TaxBranchPK
		{
			get { return GetAddOnColumnValueAsZGuid(AddOnColumnNames.TaxBranchPK) ?? ZGuid.Empty; }
			set { SetAddOnColumnValue(AddOnColumnNames.TaxBranchPK, value); }
		}

		ZGuid DepartmentPK
		{
			get { return GetAddOnColumnValueAsZGuid(AddOnColumnNames.DepartmentPK) ?? ZGuid.Empty; }
			set { SetAddOnColumnValue(AddOnColumnNames.DepartmentPK, value); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Keep for usage consistency with BranchPK, TaxBranchPK, DepartmentPK properties")]
		ZGuid CreditorPK
		{
			get { return GetAddOnColumnValueAsZGuid(AddOnColumnNames.CreditorPK) ?? ZGuid.Empty; }
			set { SetAddOnColumnValue(AddOnColumnNames.CreditorPK, value); }
		}

		[ResourceStringData("DueDate", Caption = "Due Date")]
		public ZDateTime DueDate
		{
			get
			{
				return IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null ? LinkedIncompleteInvoiceHeaderOnly.AH_DueDate :
						(GetAddOnColumnValueAsZDateTime(AddOnColumnNames.DueDate) ?? ZDateTime.Empty);
			}
			private set
			{
				if (!IsTransactionRelated)
				{
					SetAddOnColumnValue(AddOnColumnNames.DueDate, value);
				}
			}
		}

		[ResourceStringData("InvoiceDate", Caption = "Invoice Date")]
		public ZDateTime InvoiceDate
		{
			get
			{
				return IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null ? LinkedIncompleteInvoiceHeaderOnly.AH_InvoiceDate :
						(GetAddOnColumnValueAsZDateTime(AddOnColumnNames.InvoiceDate) ?? ZDateTime.Empty);
			}
			private set
			{
				if (!IsTransactionRelated)
				{
					SetAddOnColumnValue(AddOnColumnNames.InvoiceDate, value);
				}
			}
		}

		[ResourceStringData("DocumentReceivedDate", Caption = "Document Received Date")]
		public ZDateTime DocumentReceivedDate
		{
			get
			{
				return IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null ? LinkedIncompleteInvoiceHeaderOnly.AH_DocumentReceivedDate :
						(GetAddOnColumnValueAsZDateTime(AddOnColumnNames.DocumentReceivedDate) ?? ZDateTime.Empty);
			}
			private set
			{
				if (!IsTransactionRelated)
				{
					SetAddOnColumnValue(AddOnColumnNames.DocumentReceivedDate, value);
				}
			}
		}

		[ResourceStringData("InvoiceCurrency", Caption = "Invoice Currency", ShortCaption = "Currency")]
		public ZString InvoiceCurrency
		{
			get
			{
				return IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null ? LinkedIncompleteInvoiceHeaderOnly.AH_RX_NKTransactionCurrency :
						(GetAddOnColumnValue(AddOnColumnNames.InvoiceCurrency) ?? ZString.Empty);
			}
			private set
			{
				if (!IsTransactionRelated)
				{
					SetAddOnColumnValue(AddOnColumnNames.InvoiceCurrency, value);
				}
			}
		}

		[ResourceStringData("InvoiceOSTotalAmount", Caption = "Transaction Amount", ShortCaption = "Trans. Amount")]
		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal InvoiceOSTotalAmount
		{
			get
			{
				return IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null ? LinkedIncompleteInvoiceHeaderOnly.AH_OSTotal :
					(GetAddOnColumnValueAsZDecimal(AddOnColumnNames.InvoiceOSTotalAmount) ?? ZDecimal.Zero);
			}
			private set
			{
				if (!IsTransactionRelated)
				{
					SetAddOnColumnValue(AddOnColumnNames.InvoiceOSTotalAmount, value);
				}
			}
		}

		[ResourceStringData("InvoiceLocalTotalAmount", Caption = "Local Amount")]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal InvoiceLocalTotalAmount
		{
			get
			{
				return IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null ? LinkedIncompleteInvoiceHeaderOnly.AH_LocalTotal :
					(GetAddOnColumnValueAsZDecimal(AddOnColumnNames.InvoiceLocalTotalAmount) ?? ZDecimal.Zero);
			}
			private set
			{
				if (!IsTransactionRelated)
				{
					SetAddOnColumnValue(AddOnColumnNames.InvoiceLocalTotalAmount, value);
				}
			}
		}

		[ResourceStringData("InvoiceLocalTaxAmount", Caption = "Local Tax Amount")]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal InvoiceLocalTaxAmount
		{
			get
			{
				return IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null ? LinkedIncompleteInvoiceHeaderOnly.AH_GSTAmount :
					(GetAddOnColumnValueAsZDecimal(AddOnColumnNames.InvoiceLocalTaxAmount) ?? ZDecimal.Zero);
			}
			private set
			{
				if (!IsTransactionRelated)
				{
					SetAddOnColumnValue(AddOnColumnNames.InvoiceLocalTaxAmount, value);
				}
			}
		}

		[ResourceStringData("InvoiceLocalExTaxAmount", Caption = "Local Excluding Tax Amount", ShortCaption = "Local Ex. Tax Amount")]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal InvoiceLocalExTaxAmount
		{
			get
			{
				return IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null ? LinkedIncompleteInvoiceHeaderOnly.AH_InvoiceAmount :
					(GetAddOnColumnValueAsZDecimal(AddOnColumnNames.InvoiceLocalExTaxAmount) ?? ZDecimal.Zero);
			}
			private set
			{
				if (!IsTransactionRelated)
				{
					SetAddOnColumnValue(AddOnColumnNames.InvoiceLocalExTaxAmount, value);
				}
			}
		}

		public int LocalDecimals => (IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null) ? LinkedIncompleteInvoiceHeaderOnly.Company.GetLocalDecimals() : GlbCompany.CurrentCompany.GetLocalDecimals();

		public int OSDecimals => (IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly?.TransactionCurrency != null) ? LinkedIncompleteInvoiceHeaderOnly.TransactionCurrency.Decimals : LocalDecimals;

		#region RequisitionStatus

		[ResourceStringData("APInvoiceChargesApprovalRequest|RequisitionStatus", Caption = "Requisition Status", MediumCaption = "Req. Status", ShortCaption = "Req. Stat.")]
		[List(nameof(RequisitionStatusList))]
		[MaxLength(nameof(RequisitionStatusMaxLength))]
		[ReadOnly(true)]
		public ZString RequisitionStatus
		{
			get
			{
				return IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null ? LinkedIncompleteInvoiceHeaderOnly.AH_RequisitionStatus :
						(GetAddOnColumnValue(AddOnColumnNames.RequisitionStatus) ?? PostingDetails.RequisitionStatus);
			}
			internal set
			{
				if (!IsTransactionRelated)
				{
					CheckMaximumLength(RequisitionStatusInfo, value);
					SetAddOnColumnValue(AddOnColumnNames.RequisitionStatus, value);
					RequisitionStatusInfo.RefreshBinding();
				}
			}
		}

		int RequisitionStatusMaxLength => AccTransactionHeaderSchema.AH_RequisitionStatus.MaxLength;

		public ZPropertyInfo RequisitionStatusInfo => GetZPropertyInfo(Schema.RequisitionStatus);

		public ICodeDescriptionPairList RequisitionStatusList => AccountingConfigurationRegistry.Instance.PaymentRequisitionStatuses.Value;

		#endregion

		#region RequisitionDate

		[ResourceStringData("APInvoiceChargesApprovalRequest|RequisitionDate", Caption = "Requisition Date", MediumCaption = "Req. Date", ShortCaption = "Req. Date")]
		[ReadOnly(true)]
		public ZDateTime RequisitionDate
		{
			get
			{
				ZDateTime result;
				if (IsTransactionRelated && LinkedIncompleteInvoiceHeaderOnly != null)
				{
					result = LinkedIncompleteInvoiceHeaderOnly.AH_RequisitionDate;
				}
				else if (isRequisitionDateInvalid)
				{
					result = ZDateTime.Invalid;
				}
				else
				{
					result = GetAddOnColumnValueAsZDateTime(AddOnColumnNames.RequisitionDate) ?? PostingDetails.RequisitionDate;
				}

				return result;
			}
			internal set
			{
				if (!IsTransactionRelated)
				{
					isRequisitionDateInvalid = value == ZDateTime.Invalid;
					SetAddOnColumnValue(AddOnColumnNames.RequisitionDate, value);
					RequisitionDateInfo.RefreshBinding();
				}
			}
		}
		bool isRequisitionDateInvalid;

		public ZPropertyInfo RequisitionDateInfo => GetZPropertyInfo(Schema.RequisitionDate);

		#endregion

		#region AddOn fields

		GenAddOnColumnCollection AddOnColumns => addOnColumns ?? (addOnColumns = new GenAddOnColumnCollection(this));
		GenAddOnColumnCollection addOnColumns;

		public static ZString GetAddOnColumnValue(ZGuid value) => value.ToString();

		ZGuid? GetAddOnColumnValueAsZGuid(string columnName)
		{
			var resultAsString = GetAddOnColumnValue(columnName);
			if (!resultAsString.HasValue)
			{
				return null;
			}

			ZGuid result;
			if (!ZGuid.TryParse(resultAsString, out result))
			{
				result = ZGuid.Empty;
			}

			return result;
		}

		void SetAddOnColumnValue(string columnName, ZGuid value)
		{
			SetAddOnColumnValue(columnName, GetAddOnColumnValue(value));
		}

		static ZString GetAddOnColumnValue(ZDateTime value) => value.IsValid ? value.SqlFormat : ZString.Empty;

		ZDateTime? GetAddOnColumnValueAsZDateTime(string columnName)
		{
			var resultAsString = GetAddOnColumnValue(columnName);
			if (!resultAsString.HasValue)
			{
				return null;
			}

			var result = ZDateTime.Empty;
			DateTime parseResult;
			if (SqlFormatInfo.TryParseFromSqlDateTime(resultAsString, out parseResult))
			{
				result = parseResult;
			}

			return result;
		}

		void SetAddOnColumnValue(string columnName, ZDateTime value)
		{
			SetAddOnColumnValue(columnName, GetAddOnColumnValue(value));
		}

		static ZString GetAddOnColumnValue(ZDecimal value) => value.ToString("G", CultureInfo.InvariantCulture);

		ZDecimal? GetAddOnColumnValueAsZDecimal(string columnName)
		{
			var resultAsString = GetAddOnColumnValue(columnName);
			if (!resultAsString.HasValue)
			{
				return null;
			}

			decimal result;
			if (!decimal.TryParse(resultAsString, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
			{
				result = 0m;
			}

			return result;
		}

		void SetAddOnColumnValue(string columnName, ZDecimal value)
		{
			SetAddOnColumnValue(columnName, GetAddOnColumnValue(value));
		}

		ZString? GetAddOnColumnValue(string columnName) => AddOnColumns.Find(columnName)?.XA_Data;

		void SetAddOnColumnValue(string columnName, ZString value)
		{
			var column = AddOnColumns.Find(columnName);
			if (column != null && value.IsEmpty)
			{
				AddOnColumns.Delete(column);
			}
			else if (!value.IsEmpty)
			{
				if (column == null)
				{
					column = AddOnColumns.AddNew();
					column.XA_Name = columnName;
				}
				column.XA_Data = value;
			}
		}

		public static class AddOnColumnNames
		{
			public const string BranchPK = "BranchPK";
			public const string TaxBranchPK = "TaxBranchPK";
			public const string DepartmentPK = "DepartmentPK";
			public const string CreditorPK = "CreditorPK";
			public const string RequisitionStatus = "RequisitionStatus";
			public const string RequisitionDate = "RequisitionDate";
			public const string DueDate = "DueDate";
			public const string InvoiceDate = "InvoiceDate";
			public const string DocumentReceivedDate = "DocumentReceivedDate";
			public const string InvoiceCurrency = "InvoiceCurrency";
			public const string InvoiceOSTotalAmount = "InvoiceOSTotalAmount";
			public const string InvoiceLocalTotalAmount = "InvoiceLocalTotalAmount";
			public const string InvoiceLocalTaxAmount = "InvoiceLocalTaxAmount";
			public const string InvoiceLocalExTaxAmount = "InvoiceLocalExTaxAmount";
		}

		#endregion

		#endregion

		public (InvoicingBase Invoice, InvoicingBase.RestoreSavedDataResult RestoreSavedDataResult) GetLinkedInvoice()
		{
			InvoicingBase linkedInvoice = null;

			InvoicingBase.RestoreSavedDataResult restoreResult = null;

			if (IsTransactionRelated)
			{
				linkedInvoice = Factory.Load<InvoicingBase>(XP_ParentID);
				if (linkedInvoice != null && linkedInvoice.IsIncompleteInvoice)
				{
					restoreResult = linkedInvoice.RestoreSavedData(false);

					if (restoreResult.Result != InvoicingBase.RestoreSavedDataResult.ResultType.Success)
					{
						linkedInvoice = null;
					}
				}
			}

			return (linkedInvoice, restoreResult);
		}

		#region Overrides

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new APInvoiceChargesApprovalRequestFetchStrategy(this);
		}

		protected override ZBool SetXP_RequestIDFromNumberFoutain()
		{
			ZBool result = ZBool.False;
			if (XP_RequestID.IsEmpty)
			{
				XP_RequestID = AccountingNumberFountainWrapperFactory.Instance.APInvoiceApproval.Generate(this);
				result = ZBool.True;
			}
			return result;
		}

		#region IAccountingNumberFountainDataSource members

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		ZDateTime IAccountingNumberFountainDataSource.PostDate => InvoiceDate;

		GlbBranch IAccountingNumberFountainDataSource.Branch => BranchBizo ?? GlbBranch.CurrentBranch;

		GlbDepartment IAccountingNumberFountainDataSource.Department => DepartmentBizo ?? GlbDepartment.CurrentDepartment;

		#endregion

		public override ZString XP_ParentTableCode
		{
			get { return base.XP_ParentTableCode; }
			set
			{
				base.XP_ParentTableCode = value;

				SetIsTransactionOnPostingDetails(PostingDetails);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			XP_ApprovalType = Constants.GenApprovalRequestApprovalType.APInvoiceCharges;
		}

		protected override void OnAfterReadPostingDetails()
		{
			ResetFormattedChargeDetails();
		}

		protected override APInvoiceChargesApprovalRequestDetails CreatePostingApprovalDetails()
		{
			var details = new APInvoiceChargesApprovalRequestDetails(Factory);
			SetIsTransactionOnPostingDetails(details);

			return details;
		}

		void SetIsTransactionOnPostingDetails(APInvoiceChargesApprovalRequestDetails details)
		{
			details.IsTransactionRelated = IsTransactionRelated;
		}

		protected override Type ApprovingTransactionTypeCore
		{
			get { return typeof(APInvoice); }
		}

		protected override TransactionApprovalRequestEmail CreateEmail()
		{
			return new APInvoiceChargesApprovalRequestEmail(this);
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (invoiceToLink != null)
			{
				invoiceToLink.AH_TransactionCount = (ZByte)(TransactionCountDefaultValue + 1);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (invoiceToLink != null)
			{
				Factory.ChildFactories.Remove(invoiceToLink.Factory);
				if (!saveSucceeded)
				{
					invoiceToLink.AH_TransactionCount = TransactionCountDefaultValue;
				}
			}
		}

		protected override ZString RequestApprovedReference => (NoResString)"Approved For Posting";
		protected override ZString RequestCancelledByUserReference => (NoResString)"Cancelled By User";
		protected override ZString RequestCancelledForEditReference => (NoResString)"Cancelled Due To Edit/Update";
		protected override ZString RequestRejectedReference => (NoResString)"Rejected By User";
		protected override ZString RequestCancelledAsTransactionAlreadyCancelledOrPostedReference => (NoResString)"Cancelled As Transaction Already Cancelled/Posted";

		protected override void PrepareFoSavingCore()
		{
			base.PrepareFoSavingCore();

			if (invoiceToLink != null)
			{
				invoiceToLink.MakeAsIncomplete(out var subTypeMessage);
				Factory.ChildFactories.Add(invoiceToLink.Factory);
			}
		}

		protected override void FinalizeCancellingCore()
		{
			base.FinalizeCancellingCore();
			CancelAndIncrementTransactionCount(LinkedIncompleteInvoiceHeaderOnly);
		}

		#endregion

		InvoicingBase invoiceToLink;

		InvoicingBase LinkedIncompleteInvoiceHeaderOnly
		{
			get { return invoiceToLink ?? Factory.Load<InvoicingBase>(XP_ParentID); }
		}

		public void RegisterInvoiceAndRelatedBizoNotToBeSaved()
		{
			PreviewInvoiceIsNotSavedByFactoryServiceProvider.Register(Factory);
		}

		public enum Context
		{
			Posting,
			Editing
		}
	}
}
