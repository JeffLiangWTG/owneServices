using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IE.H7.Business
{
	public class AsycudaManifestHeaderValidation : EU.H7.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override void CheckAMA_OA_Representative()
		{
			var representative = Parent.Representative;
			var representativeInfo = Parent.AMA_OA_RepresentativeInfo;

			if (representative != null)
			{
				var contact = representative.Header?.AllocatedContacts?.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
				if (contact == null)
				{
					representativeInfo.AddMessageError(RepresentativeContactEmptyMessageError);
				}
				else
				{
					CheckContactProperty(contact.OC_Phone, RepresentativePhoneNumberEmptyMessageError, representativeInfo);
					CheckContactProperty(contact.OC_Email, RepresentativeEmailAddressEmptyMessageError, representativeInfo);
				}
			}
		}

		protected override void CheckAMA_OA_Declarant()
		{
			base.CheckAMA_OA_Declarant();

			var declarant = Parent.Declarant;
			var declarantInfo = Parent.AMA_OA_DeclarantInfo;

			if (declarant != null)
			{
				var countryCode = declarant.RelatedCountry?.RN_Code ?? ZString.Empty;
				if (!CountryCustomsSecurityAgreementAreaList().Contains(countryCode))
				{
					declarantInfo.AddMessageError(CountryNotAcceptableError);
				}
			}
		}

		ICollection<ZString> CountryCustomsSecurityAgreementAreaList()
		{
			var refCusCodeList = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Parent.Factory,
				CountryCodes.Ireland,
				UniversalReferenceConstants.RefCusCodeListType.Code.Code_AI008,
				ZDateTime.Today);

			if (!refCusCodeList.IsLoaded)
			{
				refCusCodeList.Load();
			}

			return refCusCodeList.Select(x => x.ZZD_Code).ToList();
		}

		protected override void CheckAMA_AgentType()
		{
			base.CheckAMA_AgentType();
			if (Parent.AMA_AgentType.IsEmpty)
			{
				Parent.AMA_AgentTypeInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetC0638RuleMessage(Parent.AMA_AgentTypeInfo));
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_AgentTypeInfo, Parent.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage);
		}

		protected override void CheckAMA_CustomsOffice()
		{
			if (Parent.AMA_CustomsOffice.IsEmpty)
			{
				Parent.AMA_CustomsOfficeInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(OfficeOfLodgementString));
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_CustomsOfficeInfo, Parent.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage);
		}

		protected override void CheckAMA_PaymentMethod()
		{
			base.CheckAMA_PaymentMethod();
			ListValidation.MessageErrorIfInvalidCode(Parent.AMA_PaymentMethodInfo, Parent.ValidationConfiguration.ValidationMessage.InvalidValueRuleMessage);
		}

		string OfficeOfLodgementString => Res.GetString("2526852d-9a59-4fa2-84a4-cdd039a86b34", "office of type Office of Lodgement");

		string CountryNotAcceptableError => Res.GetString("E9098B35-FB05-4C52-A091-5456C31B0D4A", "The Country/Region of Declarant is not valid.");
	}
}
