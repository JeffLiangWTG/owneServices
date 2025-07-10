using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public partial class JobDeclarationValidation : AutoBEJobDeclarationValidation
{
	public JobDeclarationValidation(JobDeclaration parent)
		: base(parent)
	{
	}

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

	const int DefermentAccountNumberMinimumLength = 9;

	protected override void CheckJE_DefermentAccountNumber()
	{
		base.CheckJE_DefermentAccountNumber();
		if (!Parent.JE_DefermentAccountNumber.IsEmpty && (Parent.JE_DefermentAccountNumber.Length < DefermentAccountNumberMinimumLength || !Parent.JE_DefermentAccountNumber.IsLettersAndNumbersOnlyOrEmpty))
		{
			Parent.JE_DefermentAccountNumberInfo.AddError(Res.GetString("97faa3fc-94aa-4d19-9bd1-7e0fb7adcc92", "Deferment Account Number should be only alphanumeric characters and be at least {0} characters long.", DefermentAccountNumberMinimumLength));
		}
	}

	protected override void CheckJE_PaymentMethod()
	{
		base.CheckJE_PaymentMethod();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_PaymentMethodInfo);
	}

	protected override void CheckJE_PaymentMethodLogicForEU()
	{
		// this validation should not trigger for BE
	}

	protected override void CheckJE_IATALoadPort()
	{
		base.CheckJE_IATALoadPort();

		if (Parent.IsImport && Parent.JE_TransportMode == "AIR" && Parent.JE_IATALoadPort.IsEmpty)
		{
			Parent.JE_IATALoadPortInfo.AddMessageError(Res.GetString("468CCC64-5E7F-4580-A76A-1B142162F44D", "IATA should be filled in for Import Shipments when [25. Transport] is \"AIR\"."));
		}
	}

	protected override void CheckJE_LocationOfGoods()
	{
		base.CheckJE_LocationOfGoods();

		ListValidation.MessageErrorIfInvalidCode(Parent.JE_LocationOfGoodsInfo);
	}

	protected override void ValidatePowerOfAttorney()
	{
		var organisationPK = ZGuid.Empty;
		var importerExporterText = ZString.Empty;

		switch (Parent.JE_MessageType)
		{
			case MessageTypeList.Codes.Export:
				organisationPK = Parent.ExporterDocAddress.OrganisationPK;
				importerExporterText = Res.GetString("FEE2FA22-0512-499E-9E44-FDD61426AFC8", "Exporter");
				break;
			case MessageTypeList.Codes.Import:
				organisationPK = Parent.Importer?.PK ?? ZGuid.Empty;
				importerExporterText = Res.GetString("7D851ECC-D2DF-4C53-A2E2-F7480C3A8667", "Importer");
				break;
		}

		if (!organisationPK.IsEmpty && organisationPK == GlbCompany.CurrentCompany.GC_OH_OrgProxy && Parent.JE_DeclarantType != RepresentationTypeList.Codes._1Self)
		{
			Parent.JE_DeclarantTypeInfo.AddMessageError(Res.GetString("14995DE9-0C14-401D-95EC-A0A0399389B7", "When {0} is same organization as the 'Organization Proxy', then there is no representation and option SEL should be used", importerExporterText));
		}
		else if (!HasValidPowerOfAttorney())
		{
			base.ValidatePowerOfAttorney();
		}
	}

	protected override void CheckJE_OA_Representative()
	{
		base.CheckJE_OA_Representative();
		if (Parent.JE_OA_Representative.IsEmpty && Parent.JE_OA_DeclarantAddress.IsEmpty)
		{
			Parent.JE_OA_RepresentativeInfo.AddMessageError(DeclarantAndRepresentativeEmptyMessageError);
		}
		else if (Parent.Representative is OrgAddress representative)
		{
			var representativeEori = representative.Header.GetEoriDetails();

			if (representativeEori.IsEmpty)
			{
				Parent.JE_OA_RepresentativeInfo.AddMessageError(Res.GetString("0DBA008C-1607-4329-BBD7-D645633EEBD2", "Representative must have an EORI number."));
			}
			else
			{
				var loginCompany = GlbCompany.CurrentCompany.OrgProxy;
				if (representativeEori != loginCompany.GetEoriDetails())
				{
					Parent.JE_OA_RepresentativeInfo.AddMessageError(Res.GetString("19C39647-7E33-4DA9-96EA-E2D622AD1CE1", "Representative EORI number must be same as the logon company."));
				}

				if (ImporterDeclarantAndReperesentativeEoriNumbersMatch)
				{
					Parent.JE_OA_RepresentativeInfo.AddMessageError(ImporterDeclarantAndReperesentativeEoriNumbersMatchMessageError);
				}

				if (SupplierDeclarantAndReperesentativeEoriNumbersMatch)
				{
					Parent.JE_OA_RepresentativeInfo.AddMessageError(SupplierDeclarantAndReperesentativeEoriNumbersMatchMessageError);
				}
			}
		}
	}

	protected override void CheckJE_OA_DeclarantAddress()
	{
		base.CheckJE_OA_DeclarantAddress();
		if (Parent.JE_OA_DeclarantAddress.IsEmpty && Parent.JE_OA_Representative.IsEmpty)
		{
			Parent.JE_OA_DeclarantAddressInfo.AddMessageError(DeclarantAndRepresentativeEmptyMessageError);
		}
		else if (Parent.DeclarantAddress is OrgAddress declarant)
		{
			var declarantEori = declarant.Header.GetEoriDetails();

			if (declarantEori.IsEmpty)
			{
				Parent.JE_OA_DeclarantAddressInfo.AddMessageError(Res.GetString("6B392644-75F6-4FF3-B8A1-17383079ECFD", "Declarant must have an EORI number."));
			}
			else
			{
				if (Parent.JE_OA_Representative.IsEmpty)
				{
					var loginCompany = GlbCompany.CurrentCompany.OrgProxy;
					if (declarantEori != loginCompany.GetEoriDetails())
					{
						Parent.JE_OA_DeclarantAddressInfo.AddMessageError(Res.GetString("492DCFAB-FFF2-43B2-8897-007800FA5752", "Declarant EORI number must be same as the logon company."));
					}
				}
				else
				{
					if (ImporterDeclarantAndReperesentativeEoriNumbersMatch)
					{
						Parent.JE_OA_DeclarantAddressInfo.AddMessageError(ImporterDeclarantAndReperesentativeEoriNumbersMatchMessageError);
					}

					if (SupplierDeclarantAndReperesentativeEoriNumbersMatch)
					{
						Parent.JE_OA_DeclarantAddressInfo.AddMessageError(SupplierDeclarantAndReperesentativeEoriNumbersMatchMessageError);
					}
				}
			}
		}
	}

	public override void ValidateImporterDocumentaryAddress(JobDocAddressValidation validation)
	{
		base.ValidateImporterDocumentaryAddress(validation);

		if (ImporterDeclarantAndReperesentativeEoriNumbersMatch)
		{
			Parent.ImporterDocumentaryAddress.E2_OA_AddressInfo.AddMessageError(ImporterDeclarantAndReperesentativeEoriNumbersMatchMessageError);
		}
	}

	public override void ValidateSupplierDocumentaryAddress(JobDocAddressValidation validation)
	{
		base.ValidateSupplierDocumentaryAddress(validation);

		if (SupplierDeclarantAndReperesentativeEoriNumbersMatch)
		{
			Parent.SupplierDocumentaryAddress.E2_OA_AddressInfo.AddMessageError(SupplierDeclarantAndReperesentativeEoriNumbersMatchMessageError);
		}
	}

	protected override void CheckJE_DeclarantType()
	{
		base.CheckJE_DeclarantType();

		if (Parent.JE_DeclarantType != RepresentationTypeList.Codes._3Indirect && EORINumbersAreAvailableAndMatch(Parent.Representative, Parent.DeclarantAddress))
		{
			Parent.JE_DeclarantTypeInfo.AddMessageError(Res.GetString("1E4E661E-E382-4280-B4A0-E51832613D65", "The Representative Type must be IND since Declarant and Representative have the same EORI number."));
		}
		else if (Parent.JE_DeclarantType != RepresentationTypeList.Codes._2Direct && EORINumbersAreAvailableAndDifferent(Parent.Representative, Parent.DeclarantAddress))
		{
			Parent.JE_DeclarantTypeInfo.AddMessageError(Res.GetString("93E98111-DE3F-4F64-84EE-10533856AA9B", "The Representative Type must be DIR as the Declarant and Representative do not have the same EORI number."));
		}
	}

	bool EORINumbersAreAvailableAndDifferent(OrgAddress address1, OrgAddress address2)
	{
		var eori1 = address1?.Header.GetEoriDetails() ?? ZString.Empty;
		var eori2 = address2?.Header.GetEoriDetails() ?? ZString.Empty;

		return !eori1.IsEmpty && !eori2.IsEmpty && eori1 != eori2;
	}

	bool EORINumbersAreAvailableAndMatch(OrgAddress address1, OrgAddress address2)
	{
		var eori1 = address1?.Header.GetEoriDetails() ?? ZString.Empty;
		var eori2 = address2?.Header.GetEoriDetails() ?? ZString.Empty;

		return !eori1.IsEmpty && !eori2.IsEmpty && eori1 == eori2;
	}

	bool EORINumbersAreAvailableAndMatch(OrgHeader header, OrgAddress address1, OrgAddress address2)
	{
		var eori1 = header?.GetEoriDetails() ?? ZString.Empty;
		var eori2 = address1?.Header.GetEoriDetails() ?? ZString.Empty;
		var eori3 = address2?.Header.GetEoriDetails() ?? ZString.Empty;

		return !eori1.IsEmpty && !eori2.IsEmpty && !eori3.IsEmpty && eori1 == eori2 && eori2 == eori3;
	}

	bool SupplierDeclarantAndReperesentativeEoriNumbersMatch => EORINumbersAreAvailableAndMatch(Parent.SupplierDocumentaryAddress?.Organisation, Parent.DeclarantAddress, Parent.Representative);

	bool ImporterDeclarantAndReperesentativeEoriNumbersMatch => EORINumbersAreAvailableAndMatch(Parent.ImporterDocumentaryAddress?.Organisation, Parent.DeclarantAddress, Parent.Representative);

	string SupplierDeclarantAndReperesentativeEoriNumbersMatchMessageError => Res.GetString("1DA75545-11E8-43F2-BF96-A5F83EE3074C", "Supplier, Declarant and Representative cannot all have the same EORI number.");

	string ImporterDeclarantAndReperesentativeEoriNumbersMatchMessageError => Res.GetString("EC1A239A-F57C-4B5C-B6FD-E5280101BB2F", "Importer, Declarant and Representative cannot all have the same EORI number.");

	string DeclarantAndRepresentativeEmptyMessageError => Res.GetString("7FA08D99-1172-4963-9B31-709C68E88341", "A Declarant or Representative must be specified.");

	bool HasValidPowerOfAttorney()
	{
		return Parent.IsExport && (Parent.Exporter?.RequiredDocuments
			.Cast<JobRequiredDocument>()
			.Any(x => x.EQ_DocType == Core.Constants.RefDocTypes.PowerOfAttorneyCustoms
						&& (x.EQ_ValidToDate.IsEmpty || x.EQ_ValidToDate >= ZDate.Today)) ?? false);
	}
}
