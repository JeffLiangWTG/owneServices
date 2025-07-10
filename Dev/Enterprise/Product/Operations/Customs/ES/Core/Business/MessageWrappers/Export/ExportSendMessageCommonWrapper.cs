using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public abstract class ExportSendMessageCommonWrapper : IExportMessageDataProviderCommon
	{
		public ExportSendMessageCommonWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData)
		{
			entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
			certificate = Argument.NotNull(certificateData, nameof(certificateData));
			declaration = entryHeader.Declaration;
			Argument.GreaterThan(entryHeader.MergedLines.Count, 0, nameof(entryHeader.MergedLines));

			invoiceHeader = Argument.NotNull(entryHeader.RandomHeader, nameof(entryHeader.RandomHeader));
		}

		protected readonly CusEntryHeader entryHeader;
		protected readonly JobDeclaration declaration;
		protected readonly ICertificateProvider certificate;
		protected readonly JobComInvoiceHeader invoiceHeader;

		public ZString LocalReferenceNumber => LocalReferenceNumberCore;

		protected abstract ZString LocalReferenceNumberCore { get; }

		public IExportDeclarantPartyIdProvider Declarant => declarant ?? (declarant = ExportDeclarantPartyIdWrapper.New(declaration.Declarant?.Header, declaration.JE_DeclarantType, declaration.DeclEmailAddr, declaration.ZG_AuthPerDeclaration));
		ExportDeclarantPartyIdWrapper declarant;

		public ZString TermsOfDeliveryCode => !invoiceHeader.JZ_IncoTerm.IsEmpty ? invoiceHeader.JZ_IncoTerm : declaration.JE_ShipmentIncoTerm;

		public ZString DeliveryLocation => !invoiceHeader.JZ_IncoTermPlace.IsEmpty ? invoiceHeader.JZ_IncoTermPlace : declaration.JE_ShipmentIncoTermPlace;

		public ZDecimal TotalAmount
		{
			get
			{
				if (totalAmount == null)
				{
					totalAmount = new CachedProperty<ZDecimal>(entryHeader.Factory, () =>
					{
						var amount = ZDecimal.Zero;
						amount += entryHeader.InvoiceLines.Sum(line => line.JI_LinePrice);

						return amount;
					});
				}
				return totalAmount.Value;
			}
		}
		CachedProperty<ZDecimal> totalAmount;

		public ZString TotalAmountCurrencyCode => invoiceHeader.JZ_RX_NKInvoice_Currency;

		public ZInt TotalNumberOfGoods => entryHeader.MergedLines.Count;

		public ZBool IsTest => declaration.ZG_IsTrainingDeclaration;

		public EDIMessageCollection Messages => messages ?? (messages = entryHeader.Messages);
		EDIMessageCollection messages;

		public BusinessObjectFactory Factory => entryHeader.Factory;

		public ZString BrokerCode => certificate.BrokerCode;

		public ZString CertificateName => certificate.CertificateName;

		public ZString CertificateThumbPrint => certificate.CertificateThumbPrint;

		public ZBlob CertificateBytes => certificate.CertificateBytes;

		public ZString DecryptedCertificatePassphrase => certificate.DecryptedCertificatePassphrase;

		public ZGuid CertificatePK => certificate.CertificatePK;

		public ZString CertificateID => certificate.CertificateID;

		public ZString DeclarantIdForUNBSegment
		{
			get
			{
				if (declarantIdForUNBSegment == null)
				{
					declarantIdForUNBSegment = new CachedProperty<ZString>(declaration.Factory, () =>
					{
						return OrgHeaderExtension.GetIDCode(declaration.Declarant?.Header);
					});
				}
				return declarantIdForUNBSegment.Value;
			}
		}
		CachedProperty<ZString> declarantIdForUNBSegment;

		public ZString BusinessObjectReference => entryHeader.CH_BGMReference;
	}
}
