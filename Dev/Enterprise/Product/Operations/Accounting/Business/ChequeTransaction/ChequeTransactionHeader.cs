using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ChequeTransaction
{
	[CodeProperty(AccPaymentBatchSchema.Constants.APB_BatchNumber)]
	[DescriptionProperty("HumanReadableName")]
	public partial class ChequeTransactionHeader : AccPaymentBatch
	{
		public static ChequeTransactionHeader Create(BusinessObjectFactory factory)
		{
			return factory.New<ChequeTransactionHeader>();
		}

		public ChequeTransactionHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			APB_PaymentType = ChequeTransactionTypes.ChequeEntryTransaction;
		}

		#region Public Members

		new public void RefreshBinding()
		{
			base.RefreshBinding();
			APB_PaymentTypeInfo.RefreshBinding();
			APB_OH_DebtorOrCreditorInfo.RefreshBinding();
			APB_ABInfo.RefreshBinding();
			APB_PaymentDateInfo.RefreshBinding();
			PaymentApprovalCollection.RefreshBinding();
		}

		#endregion

		#region GUIBindable Members

		#region APB_OH_DebtorOrCreditor

		public ZString DebtorOrCreditorFullName => DebtorOrCreditor != null ? DebtorOrCreditor.OH_FullNameTruncated : ZString.Empty;

		#endregion

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal AmountTotal => PaymentApprovalCollection.Cast<PaymentApprovalBase>().Sum(x => x.AV_Amount);

		public ZPropertyInfo AmountTotalInfo => GetZPropertyInfo(nameof(AmountTotal));

		#endregion

		#region PaymentBatch

		[ChildEditable]
		public override PaymentApprovalBaseCollection PaymentApprovalCollection
		{
			get
			{
				if (paymentApprovalCollection == null)
				{
					paymentApprovalCollection = new APPaymentApprovalWithAuthorisationCollection(Factory);
					paymentApprovalCollection.Load(new ZQuery(AccPaymentApprovalSchema.AV_APB_PaymentBatch, PK));
					paymentApprovalCollection.ForEach(x => ((PaymentApprovalBase)x).InitializeForPaymentBatch());
				}

				return paymentApprovalCollection;
			}
		}
		PaymentApprovalBaseCollection paymentApprovalCollection;

		#endregion

		#region Decimals

		public int OSDecimals => RefCurrency.LoadFromCurrencyCode(Factory, APB_RX_NKBatchCurrency)?.Decimals ?? LocalDecimals;

		#endregion

		#region Implementation

		#region Lookups

		protected override AccPaymentBatchLookups GetNewLookups() => new ChequeTransactionHeaderLookups(this);

		#endregion

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

#pragma warning disable IDE0051 // Remove unused private members
		new ChequeTransactionHeaderValidation Validation => base.Validation as ChequeTransactionHeaderValidation;
#pragma warning restore IDE0051 // Remove unused private members

		public void SetupChildObjects()
		{
			RegisterEditableChildObject(PaymentApprovalCollection);
			PaymentApprovalCollection.SetReadOnlyIncludingChildren(false);
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			base.BankAccount.Delete();

			var bankAccount = Factory.LoadTop1<AccBankAccount>(new ZQuery(AccBankAccountSchema.AB_Code, "ZHSBCAUD"))
				?? new TestObjectCreator(Factory).AUDBankAccount;
			APB_AB = bankAccount.PK;
		}

		public override void ClearPaymentApprovalCollection_ForTestOnly()
		{
			paymentApprovalCollection = null;
		}
#endif
	}
}
