using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.ManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}
		protected virtual bool ShouldIncludeMessageErrorIfNotEnteredForAMA_OA_Representative => true;

		protected virtual bool ShouldValidateRepresentativeContactPhoneAndEmail => true;

		protected override void CheckAMA_OA_Representative()
		{
			var representative = Parent.Representative;
			var representativeInfo = Parent.AMA_OA_RepresentativeInfo;

			if (ShouldIncludeMessageErrorIfNotEnteredForAMA_OA_Representative)
			{
				MandatoryValidation.MessageErrorIfNotEntered(representativeInfo);
			}

			if (representative != null)
			{
				var identificationNumber = GetIdentificationNumber(representative);
				if (string.IsNullOrEmpty(identificationNumber))
				{
					representativeInfo.AddMessageError(RepresentativeIdentificationNumberEmptyMessageError);
				}

				var contact = representative.Header?.AllocatedContacts?.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
				if (contact == null)
				{
					representativeInfo.AddMessageError(RepresentativeContactEmptyMessageError);
				}
				else if (ShouldValidateRepresentativeContactPhoneAndEmail)
				{
					CheckContactProperty(contact.OC_Phone, RepresentativePhoneNumberEmptyMessageError, representativeInfo);
					CheckContactProperty(contact.OC_Email, RepresentativeEmailAddressEmptyMessageError, representativeInfo);
				}
			}
		}

		protected override void CheckAMA_OA_Declarant()
		{
			var declarant = Parent.Declarant;
			var declarantInfo = Parent.AMA_OA_DeclarantInfo;

			MandatoryValidation.MessageErrorIfNotEntered(declarantInfo);

			if (declarant != null)
			{
				var identificationNumber = GetIdentificationNumber(declarant);

				if (string.IsNullOrEmpty(identificationNumber))
				{
					declarantInfo.AddMessageError(DeclarantIdentificationNumberEmptyMessageError);
					MessageErrorIfNotEntered(declarant.OA_PostCode, declarantInfo, declarant.OA_PostCodeInfo);
				}

				var contact =
					declarant.Header?.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
				if (contact == null)
				{
					declarantInfo.AddMessageError(DeclarantContactEmptyMessageError);
				}
				else
				{
					CheckContactProperty(contact.OC_Phone, DeclarantPhoneNumberEmptyMessageError, declarantInfo);
					CheckContactProperty(contact.OC_Email, DeclarantEmailAddressEmptyMessageError, declarantInfo);
				}
			}
		}

		protected virtual ZString? GetIdentificationNumber(OrgAddress orgAddress) => orgAddress.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

		#region Implementation

		protected void CheckContactProperty(string contactPropertyValue, string errorMessage, ZPropertyInfo addressPropertyInfo)
		{
			if (string.IsNullOrEmpty(contactPropertyValue))
			{
				addressPropertyInfo.AddMessageError(errorMessage);
			}
		}

		protected void MessageErrorIfNotEntered(string contactPropertyValue, ZPropertyInfo addressPropertyInfo, ZPropertyInfo contactPropertyInfo)
		{
			if (string.IsNullOrEmpty(contactPropertyValue))
			{
				var descriptor = "";
				if (string.IsNullOrEmpty(descriptor))
				{
					descriptor = MandatoryValidation.GetErrorFieldFromProperyInfo(contactPropertyInfo);
				}

				var errorMessage = string.Format(DeclarantSubPropertyEmptyMessageError, descriptor);
				addressPropertyInfo.AddMessageError(errorMessage);
			}
		}

		protected virtual string RepresentativeIdentificationNumberEmptyMessageError => Res.GetString("17f1d985-ba9b-4eef-a1c3-0a022b8027a8", "Identification Number is required");

		protected string RepresentativeContactEmptyMessageError => Res.GetString("cd55f1f0-7f76-45af-83f5-8c4afe4d3b66",
			"'CUS - Customs' type contact is required when Representative is provided. It can be configured within the Organization > Contact > Allocated Contact.");

		protected string RepresentativePhoneNumberEmptyMessageError => Res.GetString("21582fff-c010-4373-8921-c27a18699d2d",
			"Phone number is required for 'CUS - Customs' type contact.");

		protected string RepresentativeEmailAddressEmptyMessageError => Res.GetString("68fb5fa8-a8ea-4ff3-9d03-92ca78b9bd14",
			"Email address is required for 'CUS - Customs' type contact.");

		protected virtual string DeclarantIdentificationNumberEmptyMessageError => Res.GetString("CD136CA8-D353-461F-A7C4-A2F742B8433B", "You have not entered an EORI number for the Declarant.");

		protected string DeclarantSubPropertyEmptyMessageError => Res.GetString("B4262BC9-4ADA-4F80-A022-1AF8A276ED85", "You have not entered an {0} for the Declarant.");

		protected string DeclarantContactEmptyMessageError => Res.GetString("72D37ECC-6688-409F-B30C-B0DFE5ACDCB1",
			"'CUS - Customs' type contact is required when Declarant is provided. It can be configured within the Organization > Contact > Allocated Contact.");

		protected string DeclarantPhoneNumberEmptyMessageError => Res.GetString("534E0412-A7A8-49C5-99D2-1FA4F5D0D0C0",
			"Phone number is required for 'CUS - Customs' type contact.");

		protected string DeclarantEmailAddressEmptyMessageError => Res.GetString("8C97F3BE-91C8-4309-AC93-6E9BAF17EACE",
			"Email address is required for 'CUS - Customs' type contact.");
		#endregion
	}
}
