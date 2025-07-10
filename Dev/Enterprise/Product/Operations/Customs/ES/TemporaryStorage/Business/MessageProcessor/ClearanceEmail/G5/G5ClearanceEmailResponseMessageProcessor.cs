using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using static Enterprise.Customs.Common.CusEntryNumber;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;

public class G5ClearanceEmailResponseMessageProcessor : ESCommonResponseMessageProcessor<TemporaryStorageHeader, IG5ClearanceEmailProvider>
{
	public G5ClearanceEmailResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	const int G4MRNLength = 18;
	const string G4MRNString = "sehaasignadoelMRN";

	protected override string MessageFriendlyNameCore => (NoResString)"G5 Clearance Email Message Processor";

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => [DeclarationMessageTypeList.Codes.G5ClearanceEmail];
	protected override ZBool ShouldHaveSentInterchange => false;

	protected override IG5ClearanceEmailProvider GetMessageProviderCore(EDIMessage message)
	{
		var messageText = message.EM_MessageText.Replace(" ", "");

		var g4MRN = GetG4MRNData(messageText);
		if (g4MRN.IsEmpty)
		{
			throw new InvalidOperationException(ResString.GetMultilingualString("0AACAD68-32DB-440E-BDC7-F876E7CA449D", "Email Body doesn't have the correct data"));
		}

		return new G5ClearanceEmailObject(g4MRN);
	}

	ZString GetG4MRNData(ZString fullText)
	{
		if (fullText.Contains(G4MRNString))
		{
			var startIndex = fullText.IndexOf(G4MRNString, StringComparison.OrdinalIgnoreCase) + G4MRNString.Length;
			return fullText.SubstringSafe(startIndex, G4MRNLength);
		}
		return ZString.Empty;
	}

	protected override TemporaryStorageHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
		=> MessageProcessorHelper.GetRelevantBusinessObjectForEmailResponse<TemporaryStorageHeader>(message, CusEntryNumberTypes.Standard.MovementReferenceNumber, ExtraSubQueryForGetRelevantBusinessObjectForEmailResponse());

	ZDBOnlyQuery ExtraSubQueryForGetRelevantBusinessObjectForEmailResponse()
	{
		var query = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		query.AddToFilter(AsycudaManifestHeaderSchema.AMA_MessageType, "G5R");
		return query;
	}

	protected override void ProcessMessageCore(EDIMessage message, TemporaryStorageHeader linkedBusinessObject, IG5ClearanceEmailProvider provider)
	{
		var newEntryNumber = LoadOrCreate(linkedBusinessObject, CusEntryNumberTypes.Spain.SummaryEntryNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = provider.G4MRN;
		linkedBusinessObject.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

		var logTypeCode = (NoResString)"TS Guarantee";
		var log = linkedBusinessObject.Logs.Find(c => c.SL_SE_NKEvent == AutoEvents.ErrorReport.Code && c.Parameters.GetValueOrDefault(EventReferenceParameters.Codes.Type) == logTypeCode).LastOrDefault();
		if (log != null)
		{
			Logger.LogError(log.Parameters.GetValueOrDefault(EventReferenceParameters.Codes.Reason));
		}

		SetMessageStatusAsReceived(message);
		SetMessageSubTypeAsAccepted(message);
	}
}
