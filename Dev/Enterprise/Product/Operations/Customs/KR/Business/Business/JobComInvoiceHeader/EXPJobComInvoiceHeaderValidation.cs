using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class EXPJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public EXPJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected JobDeclaration declaration => Parent.JobDeclaration;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateManufacturerIPCCode();
			ValidateManufacturerUnipassID();
			ValidateBuyerID();
		}

		protected override void CheckJZ_LetterOfCreditNumber()
		{
			base.CheckJZ_LetterOfCreditNumber();
			if (Parent.JZ_PaymentTerms == SettlementMethodCodeList.Codes.LS || Parent.JZ_PaymentTerms == SettlementMethodCodeList.Codes.LU)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_LetterOfCreditNumberInfo);
			}
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();
			MandatoryValidation.MessageErrorIfIsZero(Parent.JZ_InvoiceAmountInfo);
		}

		protected override void CheckJZ_Weight()
		{
			base.CheckJZ_Weight();
			CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.JZ_WeightInfo);

			if (Core.Constants.Weight.Codes.Contains((string)Parent.JZ_WeightUQ))
			{
				ZDecimal invoiceWeight = Parent.JZ_Weight;
				invoiceWeight = Core.Constants.Weight.Convert(Parent.JZ_Weight, Parent.JZ_WeightUQ, Core.Constants.Weight.Kilograms);
				if (invoiceWeight < Parent.TotalInvoiceLinesNetWeightInKG)
				{
					Parent.JZ_WeightInfo.AddMessageError(Res.GetString("CA75A980-AA0A-46ED-B602-841CBFFE7214", "Please enter the gross weight of invoice greater than or equal to the total net weight of its invoice lines."));
				}
			}
		}

		protected override void CheckJZ_WeightUQ()
		{
			base.CheckJZ_WeightUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JZ_WeightUQInfo);
		}

		protected override void CheckJZ_OH_Buyer()
		{
			base.CheckJZ_OH_Buyer();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_BuyerInfo);
		}

		public void ValidateManufacturerIPCCode()
		{
			ValidateCalculatedProperty(Parent.ManufacturerIPCCodeInfo);
		}

		protected void CheckManufacturerIPCCode()
		{
			if (Parent.ManufacturerIPCCode == Constants.ManufacturerDefaultCode.IndustrialParkCode)
			{
				if (declaration != null && !declaration.IsDeclarationProcedureTypeE)
				{
					Parent.ManufacturerIPCCodeInfo.AddWarning(GetWarningMessageAboutToSendDefaultValue((NoResString)"Manufacturer", (NoResString)"Industrial Park Code", Constants.ManufacturerDefaultCode.IndustrialParkCode));
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ManufacturerIPCCodeInfo);
			}
		}

		public void ValidateManufacturerUnipassID()
		{
			ValidateCalculatedProperty(Parent.ManufacturerUnipassIDInfo);
		}

		protected void CheckManufacturerUnipassID()
		{
			if (declaration != null && !declaration.IsDeclarationProcedureTypeE && Parent.ManufacturerUnipassID == Constants.ManufacturerDefaultCode.UnipassID)
			{
				Parent.ManufacturerUnipassIDInfo.AddWarning(GetWarningMessageAboutToSendDefaultValue((NoResString)"Manufacturer", (NoResString)"Unipass ID", Constants.ManufacturerDefaultCode.UnipassID));
			}
		}

		public void ValidateBuyerID()
		{
			ValidateCalculatedProperty(Parent.BuyerIDInfo);
		}

		protected void CheckBuyerID()
		{
			if (declaration != null && !declaration.IsDeclarationProcedureTypeE && Parent.Buyer != null)
			{
				var buyerId = Parent.Buyer.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(Constants.IdentificationType.ForeignCompanyID, Core.Constants.CountryCodes.KoreaSouth);
				if (buyerId == null)
				{
					Parent.BuyerIDInfo.AddWarning(GetWarningMessageAboutToSendDefaultValue((NoResString)"Buyer", (NoResString)"Buyer ID", BuyerDefaultCode.BuyerID));
				}
			}
		}

		protected override void CheckJZ_OA_ManufacturerAddress()
		{
			base.CheckJZ_OA_ManufacturerAddress();
			if (!Parent.ManufacturerOrgPK.IsEmpty && Parent.JZ_OA_ManufacturerAddress.IsEmpty)
			{
				Parent.JZ_OA_ManufacturerAddressInfo.AddError(NotEnteredOrgAddressMessage);
			}
			if (Parent.ManufacturerAddress != null)
			{
				if (Parent.ManufacturerAddress.CompanyName.IsEmpty)
				{
					Parent.JZ_OA_ManufacturerAddressInfo.AddMessageError(MissingCompanyNameMessage);
				}
				if (Parent.ManufacturerAddress.Postcode.IsEmpty)
				{
					Parent.JZ_OA_ManufacturerAddressInfo.AddMessageError(MissingPostCodeMessage);
				}
				else if (Parent.ManufacturerAddress.Postcode.Length > 5)
				{
					Parent.JZ_OA_ManufacturerAddressInfo.AddMessageError(MaxLengthPostCodeMessage);
				}
				var manufacturerUnipassID = Parent.ManufacturerAddress.Header?.CustomsCodes.Cast<OrgCusCode>()?.FirstOrDefault(x => x.OK_CodeType == IdentificationType.UnipassIDForOrganization)?.OK_CustomsRegNo ?? ZString.Empty;
				if (declaration.SellerAddress != null)
				{
					switch (declaration.JE_ExporterType)
					{
						case ExporterTypeCodeList.Codes.A:
							if (declaration.SellerAddress.Header != Parent.ManufacturerAddress.Header)
							{
								Parent.JZ_OA_ManufacturerAddressInfo.AddMessageError(Res.GetString("7BC759B6-FD1D-40F3-AB59-6F82F1E8AA5B", "You have indicated the exporter type as 'A - Manufacturer Export'. However the exporter and the manufacturer are set to the different organizations."));
							}
							break;
						case ExporterTypeCodeList.Codes.C:
							if (declaration.SellerAddress.Header == Parent.ManufacturerAddress.Header)
							{
								Parent.JZ_OA_ManufacturerAddressInfo.AddMessageError(Res.GetString("A30840D6-D8E9-48F3-836F-DD004064D6C8", "You have indicated the exporter type as 'C'. However the exporter and the manufacturer are set to the same organization."));
							}
							break;
						case ExporterTypeCodeList.Codes.D:
							var exporterUnipassID = declaration.SellerAddress.Header?.CustomsCodes?.Cast<OrgCusCode>()?.FirstOrDefault(x => x.OK_CodeType == IdentificationType.UnipassIDForOrganization)?.OK_CustomsRegNo ?? ZString.Empty;
							var exportCompanyID = exporterUnipassID.SubstringSafe(0, exporterUnipassID.Length - 3);
							var manufacturerCompanyID = manufacturerUnipassID.SubstringSafe(0, manufacturerUnipassID.Length - 3);
							if (exporterUnipassID == manufacturerUnipassID || exportCompanyID != manufacturerCompanyID)
							{
								Parent.JZ_OA_ManufacturerAddressInfo.AddMessageError(Res.GetString("295D6B06-FEE9-40C5-91AF-38E6D79F1D86", "You have indicated the exporter type as 'D'. In that case, the UNIPASS ID of the exporter and manufacturer are expected to have the same sequence number except for the last 3 digits."));
							}
							break;
					}
				}
				if (declaration.JE_SimpleDRWApp == ApplicationForSimpleDrawbackCodeList.Codes.AD && manufacturerUnipassID == ZString.Empty)
				{
					Parent.JZ_OA_ManufacturerAddressInfo.AddMessageError(Res.GetString("AAD7C5E0-2191-4FDC-BD48-726941CB8398", "A simple drawback is indicated as AD. In this case, this manufacturer should have a UNIPASS ID issued."));
				}
				if (manufacturerUnipassID.IsEmpty && Parent.JZ_DRWApplicantType == DrawbackApplicantTypeList.Codes.Manufacturer)
				{
					Parent.JZ_OA_ManufacturerAddressInfo.AddMessageError(Res.GetString("C2D6288C-A35D-4050-84D7-03963F5F5B57", "You have indicated that a drawback applicant type is '2 - Manufacturer', but this manufacturer does not have a UNIPASS ID entered."));
				}
			}
			else if (Parent.JZ_DRWApplicantType == DrawbackApplicantTypeList.Codes.Manufacturer)
			{
				Parent.JZ_OA_ManufacturerAddressInfo.AddMessageError(Res.GetString("75D0DF2B-D152-4C42-9EFB-58EFB932809E", "If 'Drawback Applicant Type' is '2', the Manufacturer's address must be entered."));
			}
		}

		protected override void CheckJZ_PaymentTerms()
		{
			base.CheckJZ_PaymentTerms();
			if (declaration.JE_ExportGoodsType == TransactionTypeCodeList.Codes._11 && Parent.JZ_PaymentTerms == InvoicePaymentTermCodeList.Codes.GN)
			{
				Parent.JZ_PaymentTermsInfo.AddMessageError(Res.GetString("F49BC55D-5326-479B-ADDC-D48E195803C2", "If Transaction Type Code is '11' then, Invoice Payment Term must not be 'GN'."));
			}
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JZ_PaymentTermsInfo);
		}

		protected override void CheckJZ_NoOfPacks()
		{
			base.CheckJZ_NoOfPacks();

			if (!PackageKindCodeList.IsBulk(Parent.NoOfPacksPackType))
			{
				if (Parent.JZ_NoOfPacks <= 0)
				{
					Parent.JZ_NoOfPacksInfo.AddMessageError(Res.GetString("04DEBDBE-BEAB-4A7F-AFBF-D489F8E0C36D", "If 'Pack Type' is not 'bulk', then 'Total Packages' needs to be bigger than 0."));
				}
			}
		}

		protected override void CheckJZ_DRWApplicantType()
		{
			if (InvoiceHeader.JobDeclaration.IsDeclarationProcedureTypeM && InvoiceHeader.JZ_DRWApplicantType != ZString.Empty)
			{
				InvoiceHeader.JZ_DRWApplicantTypeInfo.AddMessageError(Res.GetString("FA685298-5D09-40F1-88AF-6AA8F1A570BD", "If Declaration Type is 'M', then Drawback Applicant Type must be empty."));
			}
			else if (InvoiceHeader.ManufacturerAddress != null)
			{
				var manufacturerUnipassID = InvoiceHeader.ManufacturerAddress.Header?.CustomsCodes?.Cast<OrgCusCode>()?.FirstOrDefault(x => x.OK_CodeType == IdentificationType.UnipassIDForOrganization)?.OK_CustomsRegNo ?? ZString.Empty;
				if (manufacturerUnipassID.IsEmpty && InvoiceHeader.JZ_DRWApplicantType == DrawbackApplicantTypeList.Codes.Manufacturer)
				{
					InvoiceHeader.JZ_DRWApplicantTypeInfo.AddMessageError(Res.GetString("F2FB51DF-FD30-47BD-A8A2-2E41BEDCFDCD", "If Manufacturer does not have a UNIPASS ID, Drawback Applicant Type must not be '2'."));
				}
			}

			ListValidation.MessageErrorIfInvalidCode(InvoiceHeader.JZ_DRWApplicantTypeInfo);
		}

		protected override void CheckJZ_ImportCargoManagementNumber()
		{
			if (InvoiceHeader.JobDeclaration != null)
			{
				if (InvoiceHeader.JobDeclaration.IsDeclarationProcedureTypeM)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_ImportCargoManagementNumberInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.JZ_ImportCargoManagementNumberInfo);
				}
			}

			if (!InvoiceHeader.JZ_ImportCargoManagementNumber.IsEmpty)
			{
				ZInt length = InvoiceHeader.JZ_ImportCargoManagementNumber.Length;
				ZString lastDigit = ZString.Empty;

				if (length == 2)
				{
					if (InvoiceHeader.JZ_ImportCargoManagementNumber.ToUpper() != ImportCargoManagementNumber.No)
					{
						InvoiceHeader.JZ_ImportCargoManagementNumberInfo.AddMessageError(Res.GetString("9CE2C884-B452-43CF-AF4B-C03E5F068E30", "Only 'NO' is accepted when the length of the value is 2."));
					}
				}
				else if (length == 15)
				{
					if (!ZInt.CanParse(InvoiceHeader.JZ_ImportCargoManagementNumber.Right(4)))
					{
						InvoiceHeader.JZ_ImportCargoManagementNumberInfo.AddMessageError(Res.GetString("0C986FE3-1518-4714-B739-533073484D44", "The last 4 digits should be numeric."));
					}
				}
				else if (length == 19)
				{
					if (!ZInt.CanParse(InvoiceHeader.JZ_ImportCargoManagementNumber.Right(8)))
					{
						InvoiceHeader.JZ_ImportCargoManagementNumberInfo.AddMessageError(Res.GetString("AC9611D3-9459-47C5-A90A-3F02B17595CD", "The last 8 digits should be numeric."));
					}
				}
				else
				{
					InvoiceHeader.JZ_ImportCargoManagementNumberInfo.AddMessageError(Res.GetString("F5866CDD-6BBF-43BE-9F21-3347221FA8DA", "The length of the value should be 2, 15 or 19."));
				}
			}
		}

		public static string MaxLengthPostCodeMessage => Res.GetString("D1654049-E723-4FF5-B789-079D64AD7555", "The maximum number of digits in a zip code is 5 digits.");
		public static string MissingPostCodeMessage => Res.GetString("D20108A0-B54C-4D68-B46E-E28ACA41E810", "The post code of this company is missing. Press F3 here and enter the post code.");
	}
}
