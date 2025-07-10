using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	class TS314DeclarationTypeProvider : MessageProvider, ITS314DeclarationType
	{
		public TS314DeclarationTypeProvider(TemporaryStorageMessageSendingObject sendingObject)
		{
			messageSendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
			header = Argument.NotNull(sendingObject.Header, nameof(header));
		}
		protected readonly TemporaryStorageMessageSendingObject messageSendingObject;
		protected readonly TemporaryStorageHeader header;

		public string MRN => header.MRN;

		public DateTime DateOfInvalidationRequest => PreparationDateAndTime;

		public string InvalidationReason => messageSendingObject.CustomsJustification;

		public string CustomsOfficeLodgement => header.CustomsOfficeOfLodgement;

		public string Declarant => header.Declarant.GetEORI();
	}
}
