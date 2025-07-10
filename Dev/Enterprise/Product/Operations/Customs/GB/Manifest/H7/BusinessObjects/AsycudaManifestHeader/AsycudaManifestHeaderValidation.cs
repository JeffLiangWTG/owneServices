using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AsycudaManifestHeaderValidation : EU.H7.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override void CheckAMA_CustomsProfile()
		{
			base.CheckAMA_CustomsProfile();

			var profile = Parent.AMA_CustomsProfile;
			var profileList = Parent.Lookups.ProfileList;

			if (profileList.Count == 0)
			{
				var error = Parent.AMA_RL_NKPortOfFirstArrival.IsEmpty
					? Parent.AMA_RL_NKPortOfDischarge.IsEmpty
						? EmptyProfileListErrorWithNotSpecifiedPort
						: EmptyBadgeCodeErrorForPortOfDischarge
					: EmptyBadgeCodeErrorForPortOfFirstArrival;

				Parent.AMA_CustomsProfileInfo.AddMessageError(error);
			}
			else if (profile.IsEmpty || !profileList.ContainsCode(profile))
			{
				Parent.AMA_CustomsProfileInfo.AddMessageError(InvalidBadgeCodeError);
			}
			else if (Parent.CSP == GatewayList.Codes.CDS)
			{
				var notificationCollection = new MessageSendingNotificationCollection();
				var checker = new AsycudaManifestHeaderCdsGlbExternalPasswordChecker(Parent, notificationCollection);
				_ = checker.PasswordExistsAndOkToSendToCds;
				var warnings = notificationCollection.WarningNotificationsAsString();
				var errors = notificationCollection.ErrorNotificationsAsString();

				if (!warnings.IsEmpty)
				{
					Parent.AMA_CustomsProfileInfo.AddWarning(warnings);
				}

				if (!errors.IsEmpty)
				{
					Parent.AMA_CustomsProfileInfo.AddMessageError(errors);
				}
			}

			ValidateCSP();
		}

		protected override void CheckAMA_AgentType()
		{
			base.CheckAMA_AgentType();

			if (!Parent.AMA_AgentType.IsEmpty && IsDeclarantInBillConsignee)
			{
				Parent.AMA_AgentTypeInfo.AddMessageError(AgentTypeMustBeEmptyError);
			}
		}

		protected override void CheckAMA_OA_Representative()
		{
			var representative = Parent.Representative;
			var representativeInfo = Parent.AMA_OA_RepresentativeInfo;

			if (representative != null)
			{
				if (IsDeclarantInBillConsignee)
				{
					representativeInfo.AddMessageError(RepresentativeShouldBeEmptyError);
				}

				if (Parent.Representative?.Header != null && Parent.Declarant?.Header != null  && Parent.Representative.Header.PK == Parent.Declarant.Header.PK)
				{
					representativeInfo.AddMessageError(DeclarantCannotBeRepresentativeError);
				}

				var contact = representative.Header?.AllocatedContacts?.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
				if (contact != null)
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
				var identificationNumber = declarant.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

				if (string.IsNullOrEmpty(identificationNumber))
				{
					declarantInfo.AddMessageError(DeclarantIdentificationNumberEmptyMessageError);
					MessageErrorIfNotEntered(declarant.OA_PostCode, declarantInfo, declarant.OA_PostCodeInfo);
				}

				var contact = declarant.Header?.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS);
				if (contact != null)
				{
					CheckContactProperty(contact.OC_Phone, DeclarantPhoneNumberEmptyMessageError, declarantInfo);
					CheckContactProperty(contact.OC_Email, DeclarantEmailAddressEmptyMessageError, declarantInfo);
				}
			}
		}

		protected override void CheckAMA_VesselName()
		{
			base.CheckAMA_VesselName();
			if (Parent.IsSea && Parent.IsForBIRDS)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_VesselNameInfo);
			}
		}

		void ValidateCSP()
		{
			base.ValidateCalculatedProperty(Parent.CSPInfo);
		}

		protected void CheckCSP()
		{
			if (Parent.CSP.IsEmpty)
			{
				Parent.CSPInfo.AddMessageError(EmptyCSPError);
			}
		}

		bool IsDeclarantInBillConsignee => Parent.Bills.Any(bill => bill.Consignee?.Header != null && Parent.Declarant?.Header != null && bill.Consignee?.Header.PK == Parent.Declarant.Header.PK);

		static ResourceString InvalidBadgeCodeError => ResString.GetMultilingualString("27d3f78c-aa0d-4e9e-b0e7-932ce5c3d28f", "You must have a valid Badge Code. Badge Codes are setup under Admin -> System -> Registry -> {0}", GBCustomsDataRegistry.Instance.BadgeCodes.Inner.Location);

		static ResourceString EmptyBadgeCodeErrorForPortOfDischarge => ResString.GetMultilingualString("10ff8aa9-c72f-4f6d-b29d-045620584079", "No badge code was found in the registry for the specified Discharge Port code. If this is correct then have your administrator check the badges in the registry.");

		static ResourceString EmptyBadgeCodeErrorForPortOfFirstArrival => ResString.GetMultilingualString("1518304c-0f8b-45d4-9333-eb29bcf67efa", "No badge code was found in the registry for the specified Port of First Arrival code. If this is correct then have your administrator check the badges in the registry.");

		static ResourceString EmptyCSPError => ResString.GetMultilingualString("00b9c639-c904-4cda-95f0-468e402a8634", "It is not possible to transmit to Customs when the gateway field is blank. Set this field indirectly by selecting a valid profile.");

		static ResourceString DeclarantCannotBeRepresentativeError => ResString.GetMultilingualString("fdc6214e-3a0f-4855-bd32-f13e8dffbb74", "A representative must only be declared where the representative differs from the declarant. Remove the representative, or select a different declarant.");

		static ResourceString RepresentativeShouldBeEmptyError => ResString.GetMultilingualString("1927c7da-88dc-4767-bdca-f8922e985f1a", "When the declarant is the importer, a representative is not allowed. Remove the representative, or select a different importer or declarant.");

		static ResourceString AgentTypeMustBeEmptyError => ResString.GetMultilingualString("f7c02eb0-1b2f-428d-bc9e-974df75d77cf", "For self-representation, representative status must not be declared.");

		static ResourceString EmptyProfileListErrorWithNotSpecifiedPort => ResString.GetMultilingualString("f73a0f53-3030-4238-b3c6-9e0ada6bcad3", "No badge code was found in the registry for the specified criteria. Check the port codes and direction. If these are correct then have your administrator check the badges in the registry.");
	}
}
