using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class BLCancelChileWrapper : IBLCancelRequest
	{
		public BLCancelChileWrapper(AsycudaBill bill, ZString reason)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.reason = reason;
		}
		readonly AsycudaBill bill;
		readonly ZString reason;

		string IBLCancelRequest.DocumentID => bill.CustomsEntryNumber;

		string IBLCancelRequest.ReferenceNumber => bill.ABL_BillNumber;

		IParticipantDocuments IBLCancelRequest.ParticipantDocuments => participants ?? (participants = new ParticipantsDocumentWrapper());
		IParticipantDocuments participants;

		IReadOnlyCollection<IDocumentObservation> IBLCancelRequest.DocumentObservations => new List<IDocumentObservation>() { new DocumentObservationsWrapper(reason) };
	}
}
