using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public abstract class TS313And315MessageProvider : MessageProvider, ITS313And315Header
	{
		protected TS313And315MessageProvider(TemporaryStorageMessageSendingObject messageSendingObject)
		{
			sendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
			temporaryStorageHeader = sendingObject.Header;
			Consignment = ConsignmentProvider.New(temporaryStorageHeader);
		}

		protected readonly TemporaryStorageMessageSendingObject sendingObject;
		protected readonly TemporaryStorageHeader temporaryStorageHeader;

		public IFallbackProcedure FallbackProcedure => CachedValueHelper.GetValue(ref fallbackProcedureCached, () => FallbackProcedureProvider.New(sendingObject));
		CachedValue<IFallbackProcedure> fallbackProcedureCached;

		public string SupervisingCustomsOffice => temporaryStorageHeader.AMA_CustomsOffice;

		public string CustomsOfficeLodgement => temporaryStorageHeader.CustomsOfficeOfLodgement;

		public IRepresentative Representative => CachedValueHelper.GetValue(ref representativeCached, () => RepresentativeProvider.New(temporaryStorageHeader));
		CachedValue<IRepresentative> representativeCached;

		public IDeclarant03 Declarant => CachedValueHelper.GetValue(ref declarantCached, () => DeclarantProvider.New(temporaryStorageHeader.Declarant));
		CachedValue<IDeclarant03> declarantCached;

		public string PresentationOffice => temporaryStorageHeader.PresentationCustomsOffice;

		public IConsignment Consignment { get; }
	}
}
