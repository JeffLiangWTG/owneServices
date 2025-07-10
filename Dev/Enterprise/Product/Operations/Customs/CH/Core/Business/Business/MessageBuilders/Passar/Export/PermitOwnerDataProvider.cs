using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

internal class PermitOwnerDataProvider : BaseParticipentDataProvider, IPermitOwner
{
	public static PermitOwnerDataProvider New(JobDocAddress jobDocAddress) => jobDocAddress == null ? null : new PermitOwnerDataProvider(jobDocAddress);

	PermitOwnerDataProvider(JobDocAddress jobDocAddress) : base(jobDocAddress, checkCountryForAeoReferenceNumber: false) { }

	protected override string GetIdentificationNumber()
	{
		if (docAddress.Parent is Restriction)
		{
			var restriction = (Restriction)docAddress.Parent;
			return !restriction.CSI_ReferenceNumber2.IsEmpty ? restriction.CSI_ReferenceNumber2 : base.GetIdentificationNumber();
		}
		else
		{
			return base.GetIdentificationNumber();
		}
	}
}
