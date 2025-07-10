using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public LocalExportJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		JobDeclaration declaration => Parent.JobDeclaration;

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_SupplierInfo);
			if (Parent.Supplier != null)
			{
				if (Parent.Supplier.OH_Category == OrgConstants.Category.NaturalPersonIndividual)
				{
					Parent.JZ_OH_SupplierInfo.AddMessageError(GetErrorMessageAboutToEnteredIndividualOrganization((NoResString)"Supplier"));
				}
				else
				{
					if (Parent.Supplier.GetRegistrationNumber(IdentificationType.BusinessRegNo).IsEmpty)
					{
						Parent.JZ_OH_SupplierInfo.AddMessageError(GetMissingRegistrationNumberMessage((NoResString)"Business Registration Number", Constants.IdentificationType.BusinessRegNo));
					}
					if (Parent.Supplier.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization).IsEmpty)
					{
						Parent.JZ_OH_SupplierInfo.AddMessageError(GetMissingRegistrationNumberMessage((NoResString)"Unipass ID", Constants.IdentificationType.UnipassIDForOrganization));
					}
				}
			}
		}

		protected override void CheckJZ_OH_Manufacturer()
		{
			base.CheckJZ_OH_Manufacturer();
			if (declaration != null && declaration.JE_ExportGoodsType == LocalExportGoodsTypeList.Codes.OriginalState)
			{
				MandatoryValidation.MessageErrorIfIsEntered(Parent.JZ_OH_ManufacturerInfo);
			}
			else
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_ManufacturerInfo);
				if (Parent.Manufacturer != null)
				{
					if (Parent.Manufacturer.OH_Category == OrgConstants.Category.NaturalPersonIndividual)
					{
						Parent.JZ_OH_ManufacturerInfo.AddMessageError(GetErrorMessageAboutToEnteredIndividualOrganization((NoResString)"Manufacturer"));
					}
					else
					{
						if (declaration != null && declaration.JE_ExportGoodsType == LocalExportGoodsTypeList.Codes.ManufacturingInProcess)
						{
							if (Parent.Manufacturer.GetRegistrationNumber(IdentificationType.BusinessRegNo).IsEmpty)
							{
								Parent.JZ_OH_ManufacturerInfo.AddMessageError(GetMissingRegistrationNumberMessage((NoResString)"Business Registration Number", Constants.IdentificationType.BusinessRegNo));
							}
							if (Parent.Manufacturer.GetRegistrationNumber(IdentificationType.UnipassIDForOrganization).IsEmpty)
							{
								Parent.JZ_OH_ManufacturerInfo.AddMessageError(GetMissingRegistrationNumberMessage((NoResString)"Unipass ID", Constants.IdentificationType.UnipassIDForOrganization));
							}
						}
					}
				}
			}
		}

		protected override void CheckJZ_OH_Buyer()
		{
			base.CheckJZ_OH_Buyer();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_BuyerInfo);
			if (Parent.Buyer != null)
			{
				if (Parent.Buyer.OH_Category == OrgConstants.Category.NaturalPersonIndividual)
				{
					Parent.JZ_OH_BuyerInfo.AddMessageError(GetErrorMessageAboutToEnteredIndividualOrganization((NoResString)"Importer"));
				}
				else
				{
					if (Parent.Buyer.GetRegistrationNumber(IdentificationType.BusinessRegNo).IsEmpty)
					{
						Parent.JZ_OH_BuyerInfo.AddMessageError(GetMissingRegistrationNumberMessage((NoResString)"Business Registration Number", Constants.IdentificationType.BusinessRegNo));
					}
					if (Parent.Buyer.OH_FullName.IsEmpty)
					{
						Parent.JZ_OH_BuyerInfo.AddMessageError(MissingCompanyNameMessage);
					}
					if (Parent.Buyer.GetRepresentativeName().IsEmpty)
					{
						Parent.JZ_OH_BuyerInfo.AddMessageError(MissingRepresentativeMessage);
					}

					if (declaration != null && LocalExportTransactionNatureCodeList.Is5DP(declaration.JE_MessageSubType))
					{
						if (Parent.Buyer.MainAddress.OA_Address1.IsEmpty)
						{
							Parent.JZ_OH_BuyerInfo.AddMessageError(Res.GetString("F7F7BC0F-F32A-4D30-A95C-9924C91C4589", "The main address is missing. Press F3 here and enter the address."));
						}
					}
				}
			}
		}

		protected override void CheckJZ_Weight()
		{
			base.CheckJZ_Weight();
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JZ_WeightInfo);
		}

		protected override void CheckJZ_DRWApplicantType()
		{
			MandatoryValidation.MessageErrorIfNotEntered(InvoiceHeader.JZ_DRWApplicantTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(InvoiceHeader.JZ_DRWApplicantTypeInfo);
			if (InvoiceHeader.JobDeclaration.JE_ExportGoodsType == LocalExportGoodsTypeList.Codes.OriginalState && InvoiceHeader.JZ_DRWApplicantType != LocalExportDrawbackApplicantTypeList.Codes.Supplier)
			{
				InvoiceHeader.JZ_DRWApplicantTypeInfo.AddMessageError(Res.GetString("8D567DF8-ADB5-47BA-B286-5DFBEC26A0F4", "Drawback Applicant Type must be '1' if Goods Type is 1."));
			}
		}
	}
}
