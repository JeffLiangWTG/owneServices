namespace Enterprise.Customs.EU.NCTS.Business
{
	public class BillConfiguration
	{
		public INctsBillValidationDecider GetValidationDecider(NctsHeader header) => GetValidationDeciderCore(header);

		protected virtual INctsBillValidationDecider GetValidationDeciderCore(NctsHeader header)
		{
			return header switch
			{
				{ IsPhase5Departure: true } => GetBillDeparturePhase5ValidationDecider(),
				{ IsPhase5Arrival: true } => GetBillArrivalPhase5ValidationDecider(),
				_ => null,
			};
		}

		protected virtual INctsBillPhase5ValidationDecider GetBillDeparturePhase5ValidationDecider() => new NctsBillDeparturePhase5ValidationDecider();

		protected virtual INctsBillArrivalPhase5ValidationDecider GetBillArrivalPhase5ValidationDecider() => new NctsBillArrivalPhase5ValidationDecider();

		#region Additional Document Validation Decider

		public INctsBillAdditionalDocumentValidationDecider GetBillAdditionalDocumentValidationDecider(NctsHeader header) => GetBillAdditionalDocumentValidationDeciderCore(header);

		protected virtual INctsBillAdditionalDocumentValidationDecider GetBillAdditionalDocumentValidationDeciderCore(NctsHeader header)
		{
			if (header?.IsPhase5Departure ?? false)
			{
				return GetBillAdditionalDocumentPhase5ValidationDecider();
			}

			return null;
		}

		protected virtual INctsBillAdditionalDocumentValidationDecider GetBillAdditionalDocumentPhase5ValidationDecider() => new NctsBillAdditionalDocumentPhase5ValidationDecider();

		#endregion
	}
}
