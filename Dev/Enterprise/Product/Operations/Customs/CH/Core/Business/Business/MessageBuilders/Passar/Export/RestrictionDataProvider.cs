using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class RestrictionDataProvider : IRestriction
{
	public static IEnumerable<RestrictionDataProvider> NewCollection(RestrictionCollection restrictions) => restrictions?.Cast<Restriction>().Select((p, index) => new RestrictionDataProvider(p, index + 1)) ?? Enumerable.Empty<RestrictionDataProvider>();

	RestrictionDataProvider(Restriction restriction, int sequenceNumber)
	{
		this.restriction = restriction;
		this.sequenceNumber = sequenceNumber;
	}
	readonly Restriction restriction;
	readonly int sequenceNumber;

	public int SequenceNumber => sequenceNumber;

	public int Code => int.TryParse(restriction.CSI_Code, out var value) ? value : 0;

	public string PermitNumber => restriction.CSI_ReferenceNumber.ReturnNullIfEmpty();

	public string PermitExceptionReason => restriction.CSI_Description.ReturnNullIfEmpty();

	public IPermitOwner PermitOwner => permitOwner ??= PermitNumber == null ? null : PermitOwnerDataProvider.New(restriction.PermitOwnerDocAddress);
	IPermitOwner permitOwner;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??=
		RestrictionAdditionalInformationDataProvider.NewCollection(restriction.AdditionalInformations).ToArray();

	IReadOnlyCollection<IAdditionalInformation> additionalInformations;
}
