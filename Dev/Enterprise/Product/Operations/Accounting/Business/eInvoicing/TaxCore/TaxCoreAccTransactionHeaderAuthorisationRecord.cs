using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.EInvoicing.TaxCore
{
	public abstract class TaxCoreAccTransactionHeaderAuthorisationRecord : AccTransactionHeaderAuthorisationRecordWithGenAddOnColumns, ISupportAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper
	{
		protected TaxCoreAccTransactionHeaderAuthorisationRecord(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString BusinessName
		{
			get { return GetAddOnColumnValue(AddOnColumnNames.BusinessName) ?? ZString.Empty; }
			set { SetAddOnColumnValue(AddOnColumnNames.BusinessName, value); }
		}

		public ZString LocationName
		{
			get { return GetAddOnColumnValue(AddOnColumnNames.LocationName) ?? ZString.Empty; }
			set { SetAddOnColumnValue(AddOnColumnNames.LocationName, value); }
		}

		public ZString Address
		{
			get { return GetAddOnColumnValue(AddOnColumnNames.Address) ?? ZString.Empty; }
			set { SetAddOnColumnValue(AddOnColumnNames.Address, value); }
		}

		public ZString District
		{
			get { return GetAddOnColumnValue(AddOnColumnNames.District) ?? ZString.Empty; }
			set { SetAddOnColumnValue(AddOnColumnNames.District, value); }
		}

		public InvoicingBase InvoiceBase => invoiceBase ?? (invoiceBase = LoadParent<InvoicingBase>());
		InvoicingBase invoiceBase;

		public AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper AdditionaInfo => additionaInfo ?? (additionaInfo = GetAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper());
		AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper additionaInfo;

		protected abstract AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper GetAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper();

		#region Inner Classes

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "AddOnColumn Name")]
		static class AddOnColumnNames
		{
			public const string BusinessName = "BusinessName";
			public const string LocationName = "LocationName";
			public const string Address = "Address";
			public const string District = "District";
		}

		public class TaxCoreAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper : AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper
		{
			public TaxCoreAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper(TaxCoreAccTransactionHeaderAuthorisationRecord parent)
			{
				Parent = parent;
			}
			TaxCoreAccTransactionHeaderAuthorisationRecord Parent { get; }

			public override ZString BusinessName => Parent.BusinessName;

			public override ZString LocationName => Parent.LocationName;

			public override ZString Address => Parent.Address;

			public override ZString District => Parent.District;

			public override ZString EInvoicePaymentMethod
			{
				get
				{
					var result = Res.GetString("2ccac4b0-a95a-4e01-a263-89d80c96cfd0", "Other");

					switch (Parent.InvoiceBase?.AH_AgreedPaymentMethodOverride ?? ZString.Empty)
					{
						case OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck:
							result = Res.GetString("5c3fb906-8a35-4940-8020-dc98521feb0b", "Cash");
							break;
						case OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard:
						case OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard:
							result = Res.GetString("a7068513-2d35-4911-b74e-541a72949403", "Card");
							break;
					}
					return result;
				}
			}

			public override ZString OriginalTransactionReferenceNumber
			{
				get
				{
					var result = ZString.Empty;

					if (OriginalTransaction != null)
					{
						result = OriginalTransaction.IsPreEInvoicingTransaction()
								? ElectronicInvoicingHelper.TaxCorePreComplianceOriginalTransactionReferenceNumber
								: originalTransaction.EInvoicingGovernmentAllocatedNumber;
					}

					return result;
				}
			}

			InvoicingBase OriginalTransaction => Parent.InvoiceBase.AH_TransactionBelongsToGroup.IsValid ?
				(originalTransaction ?? (originalTransaction = Parent.Factory.Load<InvoicingBase>(Parent.InvoiceBase.AH_TransactionBelongsToGroup)))
				: null;
			InvoicingBase originalTransaction;
		}

		#endregion
	}
}
