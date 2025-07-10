using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.Business.Declaration;

public partial class JobDeclarationValidation : AutoNLJobDeclarationValidation
{
	public JobDeclarationValidation(JobDeclaration parent)
		: base(parent)
	{
	}

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

	protected override void CheckJE_PaymentMethodLogicForEU()
	{
		var paymentMethod = Parent.JE_PaymentMethod;
		var targetInfo = Parent.JE_PaymentMethodInfo;

		if (paymentMethod == DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14)
		{
			if (!HasEoriRegNo(Parent.Importer))
			{
				targetInfo.AddMessageError(Res.GetString("875B72D9-E6DC-4EFE-9C6A-FB0611A4C2AB", "The Importer requires a Economic Operators Registration and Identification number (EORI), (Edit Organization > Details > Details > Config > Registration Numbers / Codes)"));
			}
		}
		else if (paymentMethod == DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority)
		{
			CheckControllingAgentEoriRegNo();

			if (Parent.IsImport() && !HasEoriRegNo(Parent.Representative?.Header))
			{
				targetInfo.AddMessageError(Res.GetString("9C26DF81-980D-4847-BA30-C35E279ABC32", "EORI number from representative is required."));
			}
		}
		else if (paymentMethod == DefermentMethodList.Codes.ConsigneesAccountStandingAuthority || paymentMethod == DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration)
		{
			CheckControllingAgentEoriRegNo();
		}

		static ZBool HasEoriRegNo(OrgHeader orgHeader) => orgHeader != null && orgHeader.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).Any(x => !x.OK_CustomsRegNo.IsEmpty);

		void CheckControllingAgentEoriRegNo()
		{
			if (!HasEoriRegNo(Parent.ControllingAgent))
			{
				targetInfo.AddMessageError(Res.GetString("827715A8-6117-4DE9-A975-0192EB04F48A", "The Controlling agent requires a Economic Operators Registration and Identification number (EORI), (Edit Organization > Details > Details > Config > Registration Numbers / Codes)"));
			}
		}
	}

	protected override void CheckJE_PaymentMethod()
	{
		base.CheckJE_PaymentMethod();

		if (Parent.IsImport())
		{
			if (Parent.VATPartyTaxNumber.IsEmpty && Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_Procedure.SubstringSafe(4, 3).Equals(NLConstants.FormattedProcedureCodes.F48)))
			{
				Parent.JE_PaymentMethodInfo.AddWarning(Res.GetString("9DA8BF1B-0684-48D1-B918-19BB3DFFAA1B", "No deferment party found in the declaration or the deferment party does not have a VAT number configured (Edit Organization > Details > Details > Config > Registration Numbers / Codes)."));
			}

			if (Parent.PaymentPartyEORINumber.IsEmpty)
			{
				Parent.JE_PaymentMethodInfo.AddMessageError(Res.GetString("44561613-DBCB-46E3-B295-7105FE03324F", "EORI number from Controlling Agent or EORI from Deferment party is required here."));
			}
		}
	}

	protected override void CheckJE_LocationQualifier()
	{
		base.CheckJE_LocationQualifier();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationQualifierInfo, Parent.Lookups.LocationQualifierList);
	}

	protected override void CheckJE_CustomsOffice()
	{
		base.CheckJE_CustomsOffice();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_CustomsOfficeInfo);
	}

	protected override void CheckJE_OH_ControllingCustomer()
	{
		base.CheckJE_OH_ControllingCustomer();
		if (Parent.ControllingCustomer is OrgHeader controllingCustomer && controllingCustomer.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).IsEmpty)
		{
			Parent.JE_OH_ControllingCustomerInfo.AddMessageError(Res.GetString("CB64E366-65E6-4E31-9B8B-69DDDDD284BE", "The Guarantee Provider Party requires a Economic Operators Registration and Identification number (EORI), (Edit Organization > Details > Details > Config > Registration Numbers / Codes)"));
		}
	}

	protected override void CheckJE_OH_ControllingAgent()
	{
		base.CheckJE_OH_ControllingAgent();

		if (!Parent.JE_OH_ControllingAgent.IsEmpty)
		{
			CheckTheAbsenceOfEoriNumberAndAddressDetail(Parent.JE_OH_ControllingAgentInfo, null, Parent.ControllingAgent, Res.GetString("2AB68B44-469D-43ED-8240-805E6BB596E9", "Controlling Agent"));
		}

		var types = new List<ZString>
		{
			RepresentationTypeList.Codes._2Direct
		};
		CheckHTGSender(Parent.JE_OH_ControllingAgent, types, Parent.JE_OH_ControllingAgentInfo, Res.GetString("D9A5184B-80FB-4D68-AA40-398588B097B8", "Controlling Agent"));
	}

	protected override void CheckJE_OH_Exporter()
	{
		base.CheckJE_OH_Exporter();

		if (!Parent.JE_OH_Exporter.IsEmpty)
		{
			CheckTheAbsenceOfEoriNumberAndAddressDetail(Parent.JE_OH_ExporterInfo, null, Parent.Exporter, Res.GetString("440127C2-1532-4BA8-B7D8-3F2682306053", "Exporter"));
		}
	}

	protected override void CheckJE_OA_DeclarantAddress()
	{
		base.CheckJE_OA_DeclarantAddress();
		CheckTheAbsenceOfEoriNumberAndAddressDetail(Parent.JE_OA_DeclarantAddressInfo, Parent.DeclarantAddress, Parent.DeclarantAddress?.Header, Res.GetString("71584A79-8216-41F4-96B4-E6FE9995CA06", "Declarant"));

		var types = new List<ZString>
		{
			RepresentationTypeList.Codes._3Indirect,
			RepresentationTypeList.Codes._1Self
		};
		CheckHTGSender(Parent.JE_OA_DeclarantAddress, types, Parent.JE_OA_DeclarantAddressInfo, Res.GetString("6D03A9E4-0159-4E66-9B0D-FC92751966D9", "Declarant"));
	}

	protected override void CheckJE_OA_ConsigneeAddress()
	{
		base.CheckJE_OA_ConsigneeAddress();

		if (Parent.IsImport && Parent.JE_OA_ConsigneeAddress.IsEmpty && Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_ValuationCode == "1" && x.BuyerDocAddress.IsEmpty))
		{
			Parent.JE_OA_ConsigneeAddressInfo.AddMessageError(Res.GetString("BD5E47F7-1685-4EDB-B388-6F4A44AC9C42", "Fill buyer details"));
		}
	}

	protected override void CheckJE_OA_SellerAddress()
	{
		base.CheckJE_OA_SellerAddress();

		if (Parent.IsImport && Parent.JE_OA_SellerAddress.IsEmpty && Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_ValuationCode == "1" && x.SellerDocAddress.IsEmpty))
		{
			Parent.JE_OA_SellerAddressInfo.AddMessageError(Res.GetString("5EC1F261-6A11-4A6C-A8B5-F8A05DFAEEB1", "Fill seller details"));
		}
	}

	public override void ValidateImporterDocumentaryAddress(JobDocAddressValidation validation)
	{
		base.ValidateImporterDocumentaryAddress(validation);
		var orgName = Res.GetString("e0165574-723d-4f8f-941b-b9dadb8463ce", "Importer");
		if (Parent.IsExport)
		{
			CheckAddressDetails(Parent.ImporterDocumentaryAddress.E2_OA_AddressInfo, Parent.ImporterDocumentaryAddress.Address, orgName);
		}
		else
		{
			CheckTheAbsenceOfEoriNumberAndAddressDetail(Parent.ImporterDocumentaryAddress.E2_OA_AddressInfo, Parent.ImporterDocumentaryAddress.Address, Parent.ImporterDocumentaryAddress.Address?.Header, orgName);
		}

		if (Parent.ImporterDocumentaryAddress?.Organisation != null)
		{
			var lfrAuthorisation = Parent.ImporterDocumentaryAddress.Organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode, Core.Constants.CountryCodes.Netherlands);
			if (lfrAuthorisation.IsEmpty)
			{
				var vatNumbers = Parent.ImporterDocumentaryAddress.Organisation.GetAllVatNumbers();
				if (vatNumbers.Length > 0)
				{
					var foreignVat = vatNumbers.Where(x => !x.OK_RN_NKCodeCountry.Equals(CountryCodes.Netherlands));
					if (foreignVat.Any())
					{
						Parent.ImporterDocumentaryAddress?.E2_OA_AddressInfo.AddWarning(Res.GetString("5a5dd0f7-26a3-4b7e-b476-6b3b2a4a29a2", "Fiscal representation may be applicable."));
					}
				}
				else if (!Parent.IsExport)
				{
					Parent.ImporterDocumentaryAddress?.E2_OA_AddressInfo.AddMessageError(Res.GetString("6e912c4c-63c7-494c-a114-729dccdbfbad", "Please add VAT number for this organization."));
				}
			}
		}
	}

	public override void ValidateSupplierDocumentaryAddress(JobDocAddressValidation validation)
	{
		base.ValidateSupplierDocumentaryAddress(validation);
		CheckTheAbsenceOfEoriNumberAndAddressDetail(Parent.SupplierDocumentaryAddress.E2_OA_AddressInfo, Parent.SupplierDocumentaryAddress.Address, Parent.SupplierDocumentaryAddress.Address?.Header, Res.GetString("73FF5E2F-93D3-409E-A1AB-C93CDFDC5AA8", "Supplier"));
	}

	protected override void CheckJE_GoodsOrigin()
	{
		base.CheckJE_GoodsOrigin();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_GoodsOriginInfo);
	}

	protected override void CheckJE_RL_NKOrigin()
	{
		base.CheckJE_RL_NKOrigin();
		foreach (JobComInvoiceLine invoiceLine in Parent.InvoiceLines)
		{
			invoiceLine.MarkAsNeedingValidation();
		}
	}

	protected override void CheckJE_GoodsDestination()
	{
		base.CheckJE_GoodsDestination();
		foreach (JobComInvoiceLine invoiceLine in Parent.InvoiceLines)
		{
			invoiceLine.AddInfo.MarkAsNeedingValidation();
		}
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateEarliestCustomsEntryIssueDate();
	}

	public void ValidateEarliestCustomsEntryIssueDate()
	{
		ValidateCalculatedProperty(Parent.EarliestCustomsEntryIssueDateInfo);
	}

	protected void CheckTheAbsenceOfEoriNumberAndAddressDetail(ZPropertyInfo orgAddressPropertyInfo, OrgAddress orgAddress, OrgHeader orgHeader, ZString orgName)
	{
		bool MissingEoriNumber() => EU.Business.Extensions.GetCustomsRegNoIgnoringCountry(orgHeader, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) == ZString.Empty;

		var isAddressFilled = JobDeclarationValidationHelper.IsAddressFilled(orgAddress);

		if (!isAddressFilled && MissingEoriNumber())
		{
			orgAddressPropertyInfo.AddMessageError(Res.GetString("19CC4A24-319B-4F35-9CC6-3200DE461ABF", "Please add either EORI number for {0} OR complete the address details (Name, address, postal code, city and country)", orgName));
		}
		else if (isAddressFilled && MissingEoriNumber())
		{
			orgAddressPropertyInfo.AddMessageError(Res.GetString("E4EF00AD-8CF2-4869-B69F-DC1F2AF1F07F", "EORI number is missing in organization details of {0}", orgName));
		}
	}

	protected void CheckHTGSender(ZGuid orgHeaderId, IEnumerable<ZString> types, ZPropertyInfo orgPropertyInfo, ZString orgName)
	{
		if (orgHeaderId.IsValid
			&& Parent.JE_DeclarantType.In(types) && Parent.CustomsAccount.IsEmpty)
		{
			orgPropertyInfo.AddMessageError(Res.GetString("235F93FF-3160-4EE7-991A-2C41624F03ED", "The {0} must have a HTG Sender ID in the registry", orgName));
		}
	}

	protected override void CheckJE_RN_NKTransportNationality()
	{
		base.CheckJE_RN_NKTransportNationality();
		if (Parent.JE_RN_NKTransportNationality.IsEmpty
			&& Parent.IsImport
			&& Parent.JE_TransportMode != TransportModes.Rail
			&& Parent.JE_TransportMode != TransportModes.Mail
			&& Parent.JE_TransportMode != TransportModes.OwnPropulsion)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_RN_NKTransportNationalityInfo);
		}

		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(Parent.JE_RN_NKTransportNationalityInfo, Parent.JE_TransportModeInfo, new IZType[] { (ZString)TransportModes.Air, (ZString)TransportModes.InlandWaterwayTransport, (ZString)TransportModes.OwnPropulsion, (ZString)TransportModes.Road, (ZString)TransportModes.Sea }, Res.GetString("7C46EDF9-91EB-4D16-8375-73552679A6E9", "[UCC 7/8] Nationality is mandatory if transport mode is AIR, IWT, OWN, ROA or SEA."));
	}

	protected override void CheckJE_TransportMeans()
	{
		base.CheckJE_TransportMeans();

		var declaration = Parent;
		var transportMeansInfo = declaration.JE_TransportMeansInfo;
		if (declaration.JE_TransportMeans.IsEmpty)
		{
			switch (declaration.JE_TransportModeInland)
			{
				case TransportTypeList.Codes.Air:
				case TransportTypeList.Codes.InlandWaterwayTransport:
				case TransportTypeList.Codes.OwnPropulsion:
				case TransportTypeList.Codes.Rail:
				case TransportTypeList.Codes.Road:
				case TransportTypeList.Codes.Sea:
					transportMeansInfo.AddMessageError(Res.GetString("767D375D-525F-412A-8F7F-DFB372B3B5E3", "When '[UCC 7/5] Inland M.O.T' = AIR, IWT, OWN, RAI, ROA or SEA, then 'Code' must be specified"));
					break;
			}
		}
		else
		{
			switch (declaration.JE_TransportModeInland)
			{
				case TransportTypeList.Codes.Mail:
				case TransportTypeList.Codes.FixedTransportInstallations:
					transportMeansInfo.AddMessageError(Res.GetString("EDCBACB4-F7BE-4E4A-B2F6-1921048F3CF9", "When '[UCC 7/5] Inland M.O.T' = MAI or FIX, then 'Code' must be blank'"));
					break;
			}
		}
	}

	void CheckAddressDetails(ZPropertyInfo orgAddressPropertyInfo, OrgAddress orgAddress, ZString orgName)
	{
		var error = Res.GetString("78825fb8-fbb7-4e22-a7b4-05ac3069e46b", "Please complete the address details (Name, address, postal code, city and country) for {0}", orgName);
		if (!JobDeclarationValidationHelper.IsAddressFilled(orgAddress))
		{
			orgAddressPropertyInfo.AddMessageError(error);
		}
	}
}
