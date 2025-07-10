using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class AsycudaManifestHeaderValidation : EU.H7.Business.AsycudaManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent) : base(parent)
		{
		}

		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		IValidationInternals ZValidationInternals => this;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateEntryLineNumber();
		}

		public void ValidateEntryLineNumber()
		{
			ZValidationInternals.Validate(Parent.EntryLineNumberInfo, GetEntryLineNumberAtValidationInvoker());
		}

		RunValidationInvoker GetEntryLineNumberAtValidationInvoker()
		{
			return delegate
			{
				CheckEntryLineNumber();
			};
		}

		protected override void CheckAMA_CustomsProfile()
		{
			base.CheckAMA_CustomsProfile();
			ES.Business.CertificateHelper.CheckCustomsProfile(Parent.AMA_CustomsProfileInfo, Parent.AMA_CustomsProfile, Parent.CustomsAgent);
		}

		public void ValidateGoodsLocationDescription()
		{
			ValidateCalculatedProperty(Parent.GoodsLocationDescriptionInfo);
		}

		protected override bool ShouldIncludeMessageErrorIfNotEnteredForAMA_OA_Representative => false;

		protected override bool ShouldValidateRepresentativeContactPhoneAndEmail => false;

		protected override void CheckAMA_OA_Representative()
		{
			base.CheckAMA_OA_Representative();

			if (Parent.Representative != null && Parent.IsAgentTypeDIROrICA)
			{
				var addMsgErrorForNotHavingEmail = true;
				var cusContact = Parent.Representative.Header.Contacts.GetContactForAllocation(OrgConstants.ContactAllocationType.CUS);
				if (cusContact == null || !cusContact.OC_Email.IsEmpty)
				{
					addMsgErrorForNotHavingEmail = false;
				}

				if (addMsgErrorForNotHavingEmail)
				{
					Parent.AMA_OA_RepresentativeInfo.AddMessageError(NoEmailForCusContactMessage);
				}
			}
		}

		protected override void CheckAMA_CustomsOffice()
		{
			base.CheckAMA_CustomsOffice();
			if (string.IsNullOrEmpty(Parent.AMA_CustomsOffice))
			{
				Parent.AMA_CustomsOfficeInfo.AddMessageError(NoCustomsOfficeMessage);
			}
		}

		protected override void CheckAMA_OA_Presenter()
		{
			base.CheckAMA_OA_Presenter();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_PresenterInfo);

			if (Parent.Presenter != null && string.IsNullOrEmpty(Parent.Presenter.GetEOROrNIFCode()))
			{
				Parent.AMA_OA_PresenterInfo.AddMessageError(PresenterIdentificationNumberEmptyMessageError);
			}
		}

		protected override void CheckAMA_MasterInformation()
		{
			base.CheckAMA_OA_Presenter();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_MasterInformationInfo);

			if (!Parent.AMA_MasterInformation.IsLettersAndNumbersOnlyOrEmpty)
			{
				Parent.AMA_MasterInformationInfo.AddMessageError(InvalidMasterInformationMessage);
			}
		}

		protected void CheckEntryLineNumber()
		{
			if (!Parent.MasterBill.ABL_BillNumber.IsEmpty && !Parent.EntryLineNumber.IsEmpty)
			{
				Parent.EntryLineNumberInfo.AddMessageError(AsycudaBillValidationForMasterChild.BothBillNumberAndEntryLineNumberEnteredMessageError);
			}
			else if (Parent.MasterBill.ABL_BillNumber.IsEmpty && Parent.EntryLineNumber.IsEmpty)
			{
				Parent.EntryLineNumberInfo.AddMessageError(AsycudaBillValidationForMasterChild.BothBillNumberAndEntryLineNumberEmptyMessageError);
			}
		}

		protected override ZString? GetIdentificationNumber(OrgAddress orgAddress) => orgAddress.GetEOROrNIFCode();

		protected override string DeclarantIdentificationNumberEmptyMessageError => Res.GetString("1ec0eb6c-7812-4ecd-b0ff-2fd525e863f2", "You have not entered an EORI or NIF number for the Declarant.");
		protected override string RepresentativeIdentificationNumberEmptyMessageError => Res.GetString("9d2c39c3-ca9b-462b-888d-83272a1443f1", "You have not entered an EORI or NIF number for the Representative.");

		protected string NoEmailForCusContactMessage => Res.GetString("d6e74e24-9a96-4dcd-88a1-54d1c3c79d4a", "You have not entered a Contact Email for the ‘CUS - Customs’ type contact. It can be configured within the Organization > Contact.");

		protected string NoCustomsOfficeMessage => Res.GetString("83EF869C-6F06-4321-9EFC-352027A8F752", "You have not entered an office of type Office of Lodgement.");

		protected string PresenterIdentificationNumberEmptyMessageError => Res.GetString("4593d740-72a2-4b8e-b9c3-493ba058b74a", "You have not entered an EORI or NIF number for the Presenter.");

		protected string InvalidMasterInformationMessage => Res.GetString("310881f2-fe9f-4a16-a5ce-bd6ca66d9fd9", "DSDT MRN/Flight No. must be alphanumeric.");
	}
}
