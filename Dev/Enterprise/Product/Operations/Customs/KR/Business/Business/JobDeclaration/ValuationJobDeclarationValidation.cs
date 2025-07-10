using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public partial class ValuationJobDeclarationValidation : JobDeclarationValidation
	{
		public ValuationJobDeclarationValidation(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override void CheckJE_AuthorJobTitle()
		{
			base.CheckJE_AuthorJobTitle();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_AuthorJobTitleInfo);
		}

		protected override void CheckJE_AuthorName()
		{
			base.CheckJE_AuthorName();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_AuthorNameInfo);
		}

		protected override void CheckJE_AuthorPhone()
		{
			base.CheckJE_AuthorPhone();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_AuthorPhoneInfo);
		}

		protected override void CheckJE_AuditorJobTitle()
		{
			base.CheckJE_AuditorJobTitle();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_AuditorJobTitleInfo);
		}

		protected override void CheckJE_AuditorName()
		{
			base.CheckJE_AuditorName();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_AuditorNameInfo);
		}

		protected override void CheckJE_AuditorPhone()
		{
			base.CheckJE_AuditorPhone();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_AuditorPhoneInfo);
		}

		protected override void CheckJE_OH_DutyPayer()
		{
			base.CheckJE_OH_DutyPayer();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_OH_DutyPayerInfo);

			var payer = Declaration.DutyPayer;
			if (payer != null)
			{
				var wrapper = new OrganizationDocWrapper(payer);
				if (wrapper.CompanyName.IsEmpty)
				{
					Declaration.JE_OH_DutyPayerInfo.AddMessageError(JobComInvoiceHeaderValidation.MissingCompanyNameMessage);
				}

				if (wrapper.BusinessRegNoOrIndividualID.IsEmpty)
				{
					if (wrapper.IsIndividual)
					{
						Declaration.JE_OH_DutyPayerInfo.AddMessageError(Res.GetString("45295BA4-D62A-4B08-9621-53E2DE8959D6", "There is no Identification ID for this organization. Please press F3 here and add a number of type '01', 'PAS', '03', '05' in Config > Registration Numbers/Codes on the Organization form."));
					}
					else
					{
						Declaration.JE_OH_DutyPayerInfo.AddMessageError(GetMissingRegistrationNumberMessage((NoResString)"Business Registration Number", IdentificationType.BusinessRegNo));
					}
				}
			}
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();
			MandatoryValidation.MessageErrorIfNotEntered(Declaration.JE_OH_ImporterInfo);

			var importer = Declaration.Importer;
			if (importer != null)
			{
				var wrapper = new OrganizationDocWrapper(importer);
				if (wrapper.CompanyName.IsEmpty)
				{
					Declaration.JE_OH_ImporterInfo.AddMessageError(JobComInvoiceHeaderValidation.MissingCompanyNameMessage);
				}
				if (wrapper.RepresentativeName.IsEmpty)
				{
					Declaration.JE_OH_ImporterInfo.AddMessageError(JobComInvoiceHeaderValidation.MissingRepresentativeMessage);
				}
				if (wrapper.AddressDetails.IsEmpty)
				{
					Declaration.JE_OH_ImporterInfo.AddMessageError(MissingAddressDetailsMessage);
				}
			}
		}

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_OH_SupplierInfo);
			var supplier = Parent.Supplier;
			if (supplier != null)
			{
				var wrapper = new OrganizationDocWrapper(supplier);
				if (wrapper.CompanyName.IsEmpty)
				{
					Parent.JE_OH_SupplierInfo.AddMessageError(MissingCompanyNameMessage);
				}
				if (wrapper.CountryCode.IsEmpty)
				{
					Parent.JE_OH_SupplierInfo.AddMessageError(JobComInvoiceHeaderValidation.MissingCountryCodeMessage);
				}
				if (wrapper.RepresentativeName.IsEmpty)
				{
					Parent.JE_OH_SupplierInfo.AddMessageError(MissingRepresentativeMessage);
				}
				if (wrapper.AddressDetails.IsEmpty)
				{
					Parent.JE_OH_SupplierInfo.AddMessageError(MissingAddressDetailsMessage);
				}
			}
		}

		protected override bool IsJE_ContainerPackModeMandatory => false;

		public static string MissingAddressDetailsMessage => Res.GetString("D2611F88-7ECA-4396-83C3-819D5313FF52", "The address of this company is missing. Press F3 here and enter the address 1, 2.");
	}
}
