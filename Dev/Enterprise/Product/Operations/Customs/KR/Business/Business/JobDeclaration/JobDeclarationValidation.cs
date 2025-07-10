using CargoWise.EntityFramework;
using Enterprise.Customs.Common.KR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationValidation : AutoKRJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration => Parent;
		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;
		protected JobDeclarationLookups Lookups => Parent.Lookups;
		protected virtual bool IsJE_ContainerPackModeMandatory => true;

		public override void ValidateAll()
		{
			base.ValidateAll();
			foreach (JobDocAddress docAddress in Declaration.DocAddresses)
			{
				docAddress.MarkParentAsNeedingValidation = false;
			}

			foreach (JobService service in Declaration.DocsAndCartage.Services)
			{
				service.MarkParentAsNeedingValidation = false;
			}

			foreach (Common.CusEntryNumber entryNumber in Declaration.AdditionalReferenceNumbers)
			{
				entryNumber.MarkParentAsNeedingValidation = false;
			}
			ValidateMRNNo();
		}

		public void ValidateMRNNo()
		{
			ValidateCalculatedProperty(Parent.MRNJ3_ReferenceNumberInfo);
		}

		public virtual void ValidateStevedoreCompanyAddress()
		{
		}

		protected override void CheckJE_ContainerPackMode()
		{
			if (IsJE_ContainerPackModeMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_ContainerPackModeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Declaration.JE_ContainerPackModeInfo);
		}

		protected override void CheckJE_CustomsOffice()
		{
			base.CheckJE_CustomsOffice();
			if (Declaration.IsImport || Declaration.IsLocalExport || Declaration.Is5SM)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JE_CustomsOfficeInfo);
			}
		}

		protected override void CheckJE_CustomsDivision()
		{
			base.CheckJE_CustomsDivision();
			if (Declaration.IsImport || Declaration.Is5SM)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_CustomsDivisionInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsDivisionInfo);
		}

		protected virtual void CheckMRNJ3_ReferenceNumber()
		{
		}

		protected override void CheckJE_ShipmentIncoTerm()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_ShipmentIncoTermInfo);
		}

		protected override void CheckJE_MessageTypeIsEnteredOrValid()
		{
			if (!Parent.IsSeparateDeclaration)
			{
				MandatoryValidation.CheckEntered(Parent.JE_MessageTypeInfo);

				if (Parent.JE_MessageType == KRJobMessageTypeList.Codes.Import || Parent.JE_MessageType == KRJobMessageTypeList.Codes.LocalExport)
				{
					if (!Parent.Lookups.MessageTypeList.ContainsCode(Parent.JE_MessageType))
					{
						Parent.JE_MessageTypeInfo.AddError(Res.GetString("1W8407Q-D937-4AC1-8686-ADD5A21CBAEF", "This declaration type is being developed. You will not able to save it now."));
					}
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(Parent.JE_MessageTypeInfo);
				}
			}
		}

		protected override void CheckJE_LocationOtherInformation()
		{
			base.CheckJE_LocationOtherInformation();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOtherInformationInfo);
		}

		protected override void CheckJE_ApplicationCode()
		{
			base.CheckJE_ApplicationCode();
			ListValidation.WarnIfInvalidCode(Parent.JE_ApplicationCodeInfo);
		}

		public static string GetMissingCompanyMessage(string missingTitle) => Res.GetString("14BB9ABB-ED0A-46CD-A25C-FB540C8A5645", "The {0} of this company is missing. Please press F3 here and on the Organization Proxy field again on the Branch form or its Company form. Please enter a {0} for the proxy organization.", missingTitle);
		public static string MissingCompanyNameMessage => Res.GetString("1755B178-F689-41EF-89E8-354B77A9910F", "The name of this company is missing. Press F3 here and enter the company name.");
		public static string MissingRepresentativeMessage => Res.GetString("67A34066-F42B-44F2-84D0-A31E62BBF604", "The name of this company's representative is missing. Please press F3 here and add a 'KRC' Allocation setting to this company's representative on the tab Contact > Allocated Contact.");

		public static string GetMissingRegistrationNumberMessage(string registrationNumberType, string registrationNumberCode, string location = "here") =>
			Res.GetString("5F64AEAD-81DD-473C-86C9-FEBDAB74BEAB", "There is no {0} for this organization. Please press F3 {1} and add a number of type '{2}' in Config > Registration Numbers/Codes on the Organization form.", registrationNumberType, location, registrationNumberCode);

		public static string NotEnteredOrgAddressMessage => JobComInvoiceHeaderValidation.NotEnteredOrgAddressMessage;
		public static string MissingPhoneNumberMessage => Res.GetString("6630C7DE-70AB-47BD-8B68-890E15F34807", "This phone number is missing. Press F3 here and enter the phone number.");

		protected override bool ShouldValidatePackagesActualPackageCount => false;
	}
}
