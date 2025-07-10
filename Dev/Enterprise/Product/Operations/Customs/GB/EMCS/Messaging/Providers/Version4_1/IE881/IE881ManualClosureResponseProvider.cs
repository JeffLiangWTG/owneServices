using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie881;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.tcl;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE881ManualClosureResponseProvider : IIE881ManualClosureResponse
	{
		public static IE881ManualClosureResponseProvider NewOrNull(ManualClosureResponseType manualClosureResponse) => manualClosureResponse != null ? new IE881ManualClosureResponseProvider(manualClosureResponse) : null;

		IE881ManualClosureResponseProvider(ManualClosureResponseType manualClosureResponse)
		{
			this.manualClosureResponse = manualClosureResponse;
		}
		readonly ManualClosureResponseType manualClosureResponse;

		public IEMCSEvent ExciseMovementEad => exciseMovementEad ?? (exciseMovementEad = new IE881EventProvider(manualClosureResponse.Attributes));
		IEMCSEvent exciseMovementEad;

		public ZDateTime? DateOfArrivalOfExciseProducts => DateOfArrivalOfExciseProductsSpecified ? manualClosureResponse.Attributes.DateOfArrivalOfExciseProducts.Value : ZDateTime.Empty;

		public ZBool DateOfArrivalOfExciseProductsSpecified => manualClosureResponse.Attributes.DateOfArrivalOfExciseProductsValueSpecified;

		public ZString GlobalConclusionOfReceipt => GlobalConclusionOfReceiptSpecified ? manualClosureResponse.Attributes.GlobalConclusionOfReceipt.Value.XmlEnumToString() : ZString.Empty;

		public ZBool GlobalConclusionOfReceiptSpecified => manualClosureResponse.Attributes.GlobalConclusionOfReceiptValueSpecified;

		public ITextAndLanguage ComplementaryInformation => complementaryInformation ?? (complementaryInformation = new IE881ComplementaryInformationProvider(manualClosureResponse.Attributes.ComplementaryInformation));
		ITextAndLanguage complementaryInformation;

		public ZString ManualClosureRequestReasonCode => manualClosureResponse.Attributes.ManualClosureRequestReasonCode;

		public ITextAndLanguage ManualClosureRequestReasonCodeComplement => manualClosureRequestReasonCodeComplement ?? (manualClosureRequestReasonCodeComplement = IE881RequestReasonCodeComplementProvider.NewOrNull(manualClosureResponse.Attributes.ManualClosureRequestReasonCodeComplement));
		ITextAndLanguage manualClosureRequestReasonCodeComplement;

		public ZBool ManualClosureRequestAccepted => manualClosureResponse.Attributes.ManualClosureRequestAccepted == Flag.Item1;

		public ZString ManualClosureRejectionReasonCode => manualClosureResponse.Attributes.ManualClosureRejectionReasonCode;

		public ITextAndLanguage ManualClosureRejectionComplement => manualClosureRejectionComplement ?? (manualClosureRejectionComplement = IE881RejectionComplementProvider.NewOrNull(manualClosureResponse.Attributes.ManualClosureRejectionComplement));
		ITextAndLanguage manualClosureRejectionComplement;

		public IReadOnlyCollection<IIE881SupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments = manualClosureResponse.SupportingDocuments?.Select(x => IE881SupportingDocumentProvider.NewOrNull(x)).ToArray() ?? Array.Empty<IIE881SupportingDocument>());
		IReadOnlyCollection<IIE881SupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IIE881BodyManualClosure> BodyManualClosure => bodyManualClosure ?? (bodyManualClosure = manualClosureResponse.BodyManualClosure?.Select(x => IE881BodyManualClosureProvider.NewOrNull(x)).ToArray() ?? Array.Empty<IIE881BodyManualClosure>());
		IReadOnlyCollection<IIE881BodyManualClosure> bodyManualClosure;
	}
}
