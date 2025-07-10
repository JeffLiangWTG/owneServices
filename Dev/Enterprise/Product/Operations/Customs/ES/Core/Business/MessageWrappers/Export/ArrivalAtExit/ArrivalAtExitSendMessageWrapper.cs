using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ArrivalAtExitSendMessageWrapper : IArrivalAtExitExportMessageDataProvider
	{
		public ArrivalAtExitSendMessageWrapper(CusExitDetail cusExitDetail, ICertificateProvider certificateData)
		{
			exitDetail = Argument.NotNull(cusExitDetail, nameof(cusExitDetail));
			certificate = Argument.NotNull(certificateData, nameof(certificateData));
			exitHeader = (CusExitControlHeader)exitDetail.Header;
		}

		readonly CusExitDetail exitDetail;
		readonly CusExitControlHeader exitHeader;
		readonly ICertificateProvider certificate;

		const string DeclarantTypeCode = "1";

		public ZString LocalReferenceNumber => GetMRNReferenceNumber();

		public ZString CustomsProcedureCategory5 => exitDetail.CED_MovementReferenceNumber;

		public ZString CustomsOfficeofExitCountryCode => exitDetail.CED_CustomsOffice.Left(2);

		public ZString CustomsOfficeofExit => exitDetail.CED_CustomsOffice.SubstringSafe(2);

		public ZString LocationOfGoodsExamCustomsOffice => GetTrimmedCED_ArrivalNotificationPlace().Left(4);

		public ZString LocationOfGoodsExam => GetTrimmedCED_ArrivalNotificationPlace().SubstringSafe(4);

		public ZDateTime DateOfArrival => exitDetail.CED_ArrivalNotificationDate;

		public IExportDeclarantPartyIdProvider Declarant => declarant ?? (declarant = ExportDeclarantPartyIdWrapper.New(GetDeclarant(), DeclarantTypeCode, exitHeader.DeclEmailAddr, ZBool.False));
		ExportDeclarantPartyIdWrapper declarant;

		public ZString DeclarantIdForUNBSegment => CachedValueHelper.GetValue(ref declarantIdForUNBSegment, () => OrgHeaderExtension.GetIDCode(GetDeclarant()));
		CachedValue<ZString> declarantIdForUNBSegment;

		public ZBool IsTest => (exitHeader.CEH_Parent as JobDeclaration)?.ZG_IsTrainingDeclaration ?? false;

		public EDIMessageCollection Messages => messages ?? (messages = exitDetail.Messages);
		EDIMessageCollection messages;

		public BusinessObjectFactory Factory => exitDetail.Factory;

		public ZString BrokerCode => certificate.BrokerCode;

		public ZString CertificateName => certificate.CertificateName;

		public ZString CertificateThumbPrint => certificate.CertificateThumbPrint;

		public ZBlob CertificateBytes => certificate.CertificateBytes;

		public ZString DecryptedCertificatePassphrase => certificate.DecryptedCertificatePassphrase;

		public ZGuid CertificatePK => certificate.CertificatePK;

		public ZString CertificateID => certificate.CertificateID;

		public ZString BusinessObjectReference => exitDetail.CED_MovementReferenceNumber;

		ZString GetMRNReferenceNumber()
		{
			var mrn = exitDetail.CED_MovementReferenceNumber;
			return !mrn.IsEmpty ? mrn.Left(2) + mrn.SubstringSafe(6) : string.Empty;
		}

		ZString GetTrimmedCED_ArrivalNotificationPlace()
		{
			if (trimmedArrivalPlace == null)
			{
				trimmedArrivalPlace = new CachedProperty<ZString>(exitDetail.Factory, () =>
				{
					var arrivalPlace = exitDetail.CED_ArrivalNotificationPlace;
					return arrivalPlace.Length > 10 ? arrivalPlace.SubstringSafe(4) : arrivalPlace;
				});
			}
			return trimmedArrivalPlace.Value;
		}
		CachedProperty<ZString> trimmedArrivalPlace;

		OrgHeader GetDeclarant()
		{
			var agentOrgHeader = exitHeader.Agent?.Header;
			var headerCarrierOrgHeader = exitHeader.Carrier?.Header;
			var detailCarrierOrgHeader = exitDetail.Carrier?.Header;

			return agentOrgHeader ?? detailCarrierOrgHeader ?? headerCarrierOrgHeader;
		}
	}
}
