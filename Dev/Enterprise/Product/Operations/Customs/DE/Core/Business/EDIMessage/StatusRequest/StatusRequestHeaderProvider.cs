using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business
{
	public abstract class StatusRequestHeaderProvider : IStatusRequestHeader
	{
		protected readonly StatusRequest statusRequest;

		protected StatusRequestHeaderProvider(StatusRequest statusRequest)
		{
			this.statusRequest = Argument.NotNull(statusRequest, nameof(statusRequest));
		}

		public IPartyID InterchangeSender => interchangeSender ?? (interchangeSender = new StatusRequestInterchangeSenderProvider());
		IPartyID interchangeSender;

		public abstract string InterchangeRecipientID { get; }

		public string AuthorizationNumber =>
			CachedValueHelper.GetValue(ref authorisationNumberCached,
				() => DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.GetFallBackValueAtAllLevels(
					GlbCompany.CurrentCompany.PK.ToGuid(),
					GlbBranch.CurrentBranch.PK.ToGuid(),
					Guid.Empty));

		CachedValue<ZString> authorisationNumberCached;

		public string MRN => statusRequest.MovementReferenceNumber;

		public string MessageIdentification => EDIMessage.SendersReferencePlaceHolder;

		public IPartyID Party => CachedValueHelper.GetValue(ref party, () => PartyIDProvider.NewOrNull(statusRequest.IdentificationOrg));

		CachedValue<IPartyID> party;

		public abstract PartyType PartyType { get; }

		public IDateAndTime PreparationDateAndTimeCET => preparationDateAndTimeCET ?? (preparationDateAndTimeCET = new CentralEuropeanStandardDateAndTimeProvider(true));
		IDateAndTime preparationDateAndTimeCET;

		public IDateAndTime PreparationDateAndTimeUtc => preparationDateAndTimeUtc ?? (preparationDateAndTimeUtc = new UniversalDateAndTimeProvider(true));
		IDateAndTime preparationDateAndTimeUtc;
	}
}
