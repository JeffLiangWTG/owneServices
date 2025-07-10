using System.Collections.Generic;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging
{
	public interface IIE881ManualClosureResponse
	{
		IEMCSEvent ExciseMovementEad { get; }
		ZDateTime? DateOfArrivalOfExciseProducts { get; }
		ZBool DateOfArrivalOfExciseProductsSpecified { get; }
		ZString GlobalConclusionOfReceipt { get; }
		ZBool GlobalConclusionOfReceiptSpecified { get; }
		ITextAndLanguage ComplementaryInformation { get; }
		ZString ManualClosureRequestReasonCode { get; }
		ITextAndLanguage ManualClosureRequestReasonCodeComplement { get; }
		ZBool ManualClosureRequestAccepted { get; }
		ZString ManualClosureRejectionReasonCode { get; }
		ITextAndLanguage ManualClosureRejectionComplement { get; }
		IReadOnlyCollection<IIE881SupportingDocument> SupportingDocuments { get; }
		IReadOnlyCollection<IIE881BodyManualClosure> BodyManualClosure { get; }
	}
}
