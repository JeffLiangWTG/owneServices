using System.Collections.Generic;
using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public interface ICUSCARMessageProvider : IEDIFACTMessageProvider
{
	IMessageDetailsProvider MessageDetails { get; }

	IDateTimePeriodProvider BillIssueDate { get; }

	ILocationProvider BillIssueLocation { get; }

	IReadOnlyCollection<IReferenceProvider> References { get; }

	IReadOnlyCollection<IPartyProvider> RelatedParties { get; }

	IFreeTextProvider DocumentUpdates { get; }

	bool IsNegotiable { get; }

	string Payer { get;  }

	IReadOnlyCollection<ITransportEquipmentInfoProvider> ContainerInfos { get; }

	IConsignmentInfoProvider BillInfo { get; }
}
