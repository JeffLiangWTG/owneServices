using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.H7.Business
{
	public class IM414HeaderProvider : IM413_414_415HeaderProvider, IIM414Header
	{
		public IM414HeaderProvider(MessageSendingObject messageSendingObject) : base(messageSendingObject)
		{
		}

		public IOperation ImportOperation => CachedValueHelper.GetValue(ref importOperationCached, () => new IM413_414_415OperationProvider(messageSendingObject, true));
		CachedValue<IOperation> importOperationCached;

		public string LRN => messageSendingObject.Bill.LocalReferenceNumber;

		public string CustomsRegistrationNumber => string.Empty;

		public string MRN => messageSendingObject.Bill.MovementReferenceNumber;

		public DateTime InvalidationRequestDateAndTime => ZDateTime.Now.ToDateTime();

		public string InvalidationReason => messageSendingObject.AmendmentInvalidationReason;
	}
}
