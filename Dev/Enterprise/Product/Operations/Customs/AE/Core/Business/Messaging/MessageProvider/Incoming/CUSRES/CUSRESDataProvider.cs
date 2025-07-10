using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D23A;
using Enterprise.Edifact.D23A.Messages.CUSRES;

namespace Enterprise.Customs.AE.Business;

sealed class CUSRESDataProvider : ICUSRESDataProvider
{
	readonly Lazy<CUSRESMessage> cusresMessage;

	public CUSRESDataProvider(ZString messageText)
	{
		Argument.NotNullOrEmpty(messageText, nameof(messageText));
		cusresMessage = new Lazy<CUSRESMessage>(() => (CUSRESMessage)new D23AMessageFactory().GetMessage(AECharacterSet.New(), messageText));
	}

	public ZString OutgoingAccessReference => DocumentIdentifier;

	public ZString EntryStatus => CachedValueHelper.GetValue(ref entryStatus, GetEntryStatus);
	CachedValue<string> entryStatus;

	public ZString DocumentIdentifier => CachedValueHelper.GetValue(ref documentIdentifier, GetDocumentIdentifier);
	CachedValue<string> documentIdentifier;

	public IReadOnlyCollection<IInformationRequest> InformationRequests => informationRequests ??= GetInformationRequests();
	IReadOnlyCollection<IInformationRequest> informationRequests;

	public bool IsParsed => cusresMessage.Value != null;

	string GetDocumentIdentifier() => cusresMessage.Value?.Group3[0].RFF[0].Reference.ReferenceIdentifier;

	string GetEntryStatus() => cusresMessage.Value?.GEI[0].ProcessingIndicator.ProcessingIndicatorDescriptionCode;

	List<InformationRequestProvider> GetInformationRequests()
		=> cusresMessage.Value?.Group4.Cast<SegmentGroup4>().Select(x => new InformationRequestProvider(x)).ToList() ?? [];
}
