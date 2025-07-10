using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.EInvoicing.India
{
	public class IndiaAccTransactionHeaderAuthorisationRecord : AccTransactionHeaderAuthorisationRecord
	{
		public IndiaAccTransactionHeaderAuthorisationRecord(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AHF_RecordType = "INI";
			AHF_IDType = "GVT";
		}

		public InvoicingBase InvoiceBase => invoiceBase ?? (invoiceBase = LoadParent<InvoicingBase>());
		InvoicingBase invoiceBase;

		public AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper AdditionaInfo => additionaInfo ?? (additionaInfo = new IndiaAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper(this));
		AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper additionaInfo;

		#region GetInvoiceJwtParts

		public (string headerJson, string payloadJson, byte[] signature) GetInvoiceJwtParts()
			=> GetJwtParts(AHF_AuthorisationData.ToAscii());

		#endregion

		#region QRCode

		public (string headerJson, string payloadJson, byte[] signature) GetQRCodeJwtParts()
			=> GetJwtParts(AHF_VerificationUrl);

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AHF_Counter = "INI123";
			AHF_Number = "TSTINI";
		}
#endif

		#region Doc Wrapper

		public class IndiaAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper : AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper
		{
			public IndiaAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper(IndiaAccTransactionHeaderAuthorisationRecord parent)
			{
				Parent = parent;
			}

			IndiaAccTransactionHeaderAuthorisationRecord Parent { get; }

			InvoicingBase OriginalTransaction => Parent.InvoiceBase.AH_TransactionBelongsToGroup.IsValid ?
				(originalTransaction ?? (originalTransaction = Parent.Factory.Load<InvoicingBase>(Parent.InvoiceBase.AH_TransactionBelongsToGroup)))
				: null;
			InvoicingBase originalTransaction;

			public override ZString OriginalTransactionReferenceNumber => OriginalTransaction?.AH_TransactionReference ?? ZString.Empty;
		}

		#endregion

		#region Implementation

		static (string headerJson, string payloadJson, byte[] signature) GetJwtParts(ZString jwt)
		{
			if (jwt.IsEmpty)
			{
				return (string.Empty, string.Empty, Array.Empty<byte>());
			}

			var parts = jwt.Split('.');
			if (parts.Length != 3)
			{
				return (string.Empty, string.Empty, Array.Empty<byte>());
			}

			var headerJson = Encoding.UTF8.GetString(Convert.FromBase64String(Base64UrlUtility.FromUrlSafeBase64(parts[0])));
			var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Base64UrlUtility.FromUrlSafeBase64(parts[1])));
			var signature = Convert.FromBase64String(Base64UrlUtility.FromUrlSafeBase64(parts[2]));
			return (headerJson, payloadJson, signature);
		}

		#endregion
	}
}
