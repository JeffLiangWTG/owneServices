using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class PartyProvider : IParty
	{
		public PartyProvider(JobDocAddress jobDocAddress, bool isInPhase5TransitionPeriod)
		{
			this.jobDocAddress = jobDocAddress;
			this.isInPhase5TransitionPeriod = isInPhase5TransitionPeriod;
		}

		public string IdentificationNumber
		{
			get
			{
				var euIdentificationNumber = EuEoriProviderAndValidator.GetEuIdentificationNumber(jobDocAddress);

				if (string.IsNullOrEmpty(euIdentificationNumber))
				{
					return null;
				}

				if (SuppressForeignEORIActive)
				{
					return IncludeEORI ? euIdentificationNumber : null;
				}

				return euIdentificationNumber;
			}
		}

		public virtual string Name => string.IsNullOrEmpty(IdentificationNumber) ? (string.IsNullOrEmpty(jobDocAddress.E2_CompanyName) ? null : jobDocAddress.E2_CompanyName.ToString().Trim()) : null;

		public virtual IAddress Address => SuppressForeignEORIActive && IncludeEORI ? null : (IsAddressEmpty ? null : (IncludeAddress ? address ?? (address = string.IsNullOrEmpty(IdentificationNumber) ? new AddressProvider(jobDocAddress, isInPhase5TransitionPeriod) : null) : null));

		IAddress address;

		ZBool IsAddressEmpty => (SuppressForeignEORIActive && IncludeEORI) || (string.IsNullOrEmpty(jobDocAddress.E2_Address1) && string.IsNullOrEmpty(jobDocAddress.E2_Address2) && string.IsNullOrEmpty(jobDocAddress.E2_Postcode) && string.IsNullOrEmpty(jobDocAddress.E2_City) && string.IsNullOrEmpty(jobDocAddress.E2_RN_NKCountryCode));

		public IContactPerson ContactPerson => null;

		public ZBool IsEmpty => string.IsNullOrEmpty(IdentificationNumber) && string.IsNullOrEmpty(Name) && IsAddressEmpty;

		public int PartyNameMaxLength => isInPhase5TransitionPeriod ? MessageSchemaInTransitionPeriod.PartyNameMaxLength : MessageSchema.PartyNameMaxLength;

		protected virtual ZBool IncludeAddress => true;

		public virtual ZBool IncludeEORI => false;

		protected virtual ZBool SuppressForeignEORIActive => false;

		protected readonly JobDocAddress jobDocAddress;
		readonly bool isInPhase5TransitionPeriod;
	}
}
