using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.IT.Business;

public class XTradeInterchangeProvider : InterchangeProviderBase
{
	public XTradeInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
	{
	}

	protected sealed override string GetCollationKey(EDIMessage message) => InterchangeProviderBase.DoNotCollateType;

	protected sealed override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

	protected sealed override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
	{
		const string interchangeTo = "ITCustoms";

		var message = GetOneAndOnlyMessageFromCollection(messages);
		var interchangeFrom = message.Company.LicenceKeyIdentifier;
		SetInterchangeValuesForTransmit(interchange, messages, message.EM_ApplicationReference, interchangeTo, interchangeFrom);
		interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
		interchange.EI_SessionGUID = ZGuid.NewZGuid();

		var interchangeEnricherCollection = GetEnricherCollectionForMessage(message);
		interchangeEnricherCollection.ForEach(x => x.Enrich(interchange));
	}

	protected sealed override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages) => ZString.Empty;

	EDIMessage GetOneAndOnlyMessageFromCollection(NonDependentEDIMessageCollection messages) => messages.Cast<EDIMessage>().Single();

	IEnumerable<IEDIInterchangeEnricher> GetEnricherCollectionForMessage(EDIMessage message)
	{
		var certificateProvider = GetCertificateProviderFromMessageCreatedUser(message);
		var requiresAutomaticSignature = DoesMessageRequireAutomaticSignature(message, certificateProvider);

		var result = new List<IEDIInterchangeEnricher> { CreateMessageTypeEnricher(message, requiresAutomaticSignature) };

		if (requiresAutomaticSignature)
		{
			var signatureOperation = DetermineSignatureOperation(message);
			result.Add(new AutomaticSignatureInterchangeMessageEnricher(certificateProvider.AutomaticSignaturePassword, signatureOperation));
		}

		return result.WhereNotNull();
	}

	IEDIInterchangeEnricher CreateMessageTypeEnricher(EDIMessage message, bool requiresAutomaticSignature)
	{
		return (string)message.EM_MessageType switch
		{
			EDIMessageTypeList.Codes.ElectronicFolderQuery => new ElectronicFolderInterchangeMessageEnricher(),
			EDIMessageTypeList.Codes.Cancellation when !requiresAutomaticSignature => new CancellationInterchangeMessageEnricher(),
			EDIMessageTypeList.Codes.Amendment when message.SignatureRequiresAmendmentMetadata() && !requiresAutomaticSignature => new AmendmentInterchangeMessageEnricher(),
			EDIMessageTypeList.Codes.ReleaseProspectusRequest => new ReleaseProspectusInterchangeMessageEnricher(),
			EDIMessageTypeList.Codes.IvistoRequest => new IvistoInterchangeMessageEnricher(),
			EDIMessageTypeList.Codes.IrildesRequest => new IrildesInterchangeMessageEnricher(),
			EDIMessageTypeList.Codes.Eur1Request => new Eur1InterchangeMessageEnricher(),
			EDIMessageTypeList.Codes.EadRequest => new EadInterchangeMessageEnricher(),
			EDIMessageTypeList.Codes.AccountingSummaryRequest => new AccountingSummaryInterchangeMessageEnricher(),
			EDIMessageTypeList.Codes.TadRequest => new TadInterchangeMessageEnricher(),
			EDIMessageTypeList.Codes.AccountingSummaryDownload => new AccountingSummaryDownloadInterchangeMessageEnricher(),
			EDIMessageTypeList.Codes.SummaryProspectusRequest => new SummaryProspectusInterchangeMessageEnricher(),
			EDIMessageTypeList.Codes.SummaryProspectusDownload => new SummaryProspectusDownloadInterchangeMessageEnricher(),
			_ => null,
		};
	}

	SignatureOperation DetermineSignatureOperation(EDIMessage message)
	{
		if (message.IsCancellationRequest())
		{
			return SignatureOperation.Cancellation;
		}
		if (message.SignatureRequiresAmendmentMetadata())
		{
			return SignatureOperation.Amendment;
		}
		return SignatureOperation.New;
	}

	GlbCertificateProvider GetCertificateProviderFromMessageCreatedUser(EDIMessage message)
	{
		if (message.UserWhoQueuedThisRecord is GlbStaff userWhoQueuedThisRecord)
		{
			return new GlbCertificateProvider(userWhoQueuedThisRecord);
		}
		return null;
	}

	bool DoesMessageRequireAutomaticSignature(EDIMessage message, GlbCertificateProvider certificateProvider)
	{
		return !message.EM_MessageType.ToString().In(EDIMessageTypeList.Codes.IvistoRequest, EDIMessageTypeList.Codes.IrildesRequest, EDIMessageTypeList.Codes.UniqueTransactionId)
			&& (certificateProvider?.HasValidAutomaticSignaturePassword ?? false);
	}
}
