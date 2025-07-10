using CargoWise.Customs.BE.MessageContracts;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class PartyProvider : IParty
	{
		public PartyProvider(JobDocAddress jobDocAddress, bool isTransitionPeriodAES30 = false)
		{
			this.jobDocAddress = jobDocAddress;
			isTransitionPeriod = isTransitionPeriodAES30;
		}
		protected readonly JobDocAddress jobDocAddress;
		protected readonly bool isTransitionPeriod;

		public string IdentificationNumber => jobDocAddress.Organisation?.GetIdentificationNumber();

		public string Name => IncludeName ? jobDocAddress.E2_CompanyName : null;

		public IAddress Address => CachedValueHelper.GetValue(ref address, () => IncludeAddress ? new AddressProvider(jobDocAddress, isTransitionPeriodAES30: isTransitionPeriod) : null);
		CachedValue<IAddress> address;

		public IContactPerson ContactPerson
		{
			get
			{
				if (contactPerson == null && IncludeContactPerson)
				{
					contactPerson = GetContactPersonProvider();
					if (ExcludeContactPersonBasedOnData(contactPerson))
					{
						contactPerson = null;
					}
				}
				return contactPerson;
			}
		}
		IContactPerson contactPerson;

		public int NameMaxlength => isTransitionPeriod ? MessageSchema.PartyNameMaxLengthInTransitionPeriod : MessageSchema.PartyNameMaxLength;

		protected virtual IContactPerson GetContactPersonProvider() => ContactPersonProvider.NewOrNull(jobDocAddress);

		protected virtual ZBool ExcludeContactPersonBasedOnData(IContactPerson contactPersonToValidate)
		{
			return contactPersonToValidate != null && (string.IsNullOrEmpty(contactPersonToValidate.Name) || string.IsNullOrEmpty(contactPersonToValidate.PhoneNumber));
		}

		protected ZBool IsAllContactPersonDataEmpty(IContactPerson contactPersonProvider)
		{
			return contactPersonProvider != null && string.IsNullOrEmpty(contactPersonProvider.Name) && string.IsNullOrEmpty(contactPersonProvider.PhoneNumber) && string.IsNullOrEmpty(contactPersonProvider.EMailAddress);
		}

		protected virtual ZBool IncludeContactPerson => true;

		protected virtual ZBool IncludeAddress => string.IsNullOrEmpty(IdentificationNumber);

		protected virtual ZBool IncludeName => string.IsNullOrEmpty(IdentificationNumber);
	}
}
