using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class RepresentativeProvider : PartyProvider, INCTSRepresentative
	{
		public static RepresentativeProvider New(JobDocAddress jobDocAddress) => jobDocAddress.IsValidAddress ? new RepresentativeProvider(jobDocAddress) : null;

		RepresentativeProvider(JobDocAddress jobDocAddress) : base(jobDocAddress)
		{
		}

		public int Status => 2;

		protected override ZBool ExcludeContactPersonBasedOnData(IContactPerson contactPersonToValidate) => IsAllContactPersonDataEmpty(contactPersonToValidate);

		protected override ZBool IncludeAddress => false;

		protected override IContactPerson GetContactPersonProvider() => RepresentativeContactPersonProvider.NewOrNull(jobDocAddress);
	}
}
