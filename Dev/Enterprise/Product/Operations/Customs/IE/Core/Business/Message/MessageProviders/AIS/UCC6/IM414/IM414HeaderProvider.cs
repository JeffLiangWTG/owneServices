using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM414HeaderProvider : IM413_414_415_432_433HeaderProvider, IIM414Header
	{
		public IM414HeaderProvider(AISMessageSendingAction sendingAction) : base(sendingAction)
		{
		}

		public string CustomsRegistrationNumber => entryHeader.CRN;

		public DateTime InvalidationRequestDateAndTime => PreparationDateAndTime;

		public string InvalidationReason => SendingAction.Annotation;

		public string LRN => entryHeader.CH_BGMReference;

		public string MRN => entryHeader.MovementReferenceNumber;
	}
}
