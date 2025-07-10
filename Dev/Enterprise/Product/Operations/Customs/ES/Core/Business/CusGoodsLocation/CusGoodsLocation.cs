using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants;
using CusGoodsLocationQualifierList = Enterprise.Customs.Business.CusGoodsLocationQualifierList;
using CusGoodsLocationTypeList = Enterprise.Customs.Business.CusGoodsLocationTypeList;

namespace Enterprise.Customs.ES.Business;

public class CusGoodsLocation : EU.Business.CusGoodsLocation, Integration.Customs.ES.ICusGoodsLocation
{
	public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public Declaration.CusEntryInstruction EntryInstruction => Parent as Declaration.CusEntryInstruction;

	public bool IsImportAndH2
	{
		get
		{
			var isImportAndH2 = false;
			var entryInstruction = EntryInstruction;
			var declaration = entryInstruction?.JobDeclaration;

			if (declaration != null)
			{
				isImportAndH2 = declaration.IsImport && entryInstruction.IsH2;
			}

			return isImportAndH2;
		}
	}

	public bool IsQualifierYAndTypeB => CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber && CGL_Type == CusGoodsLocationTypeList.Codes.AuthorizedPlace;

	public bool NamePhoneAndEmailVisible => !(IsImportAndH2 && (IsQualifierYAndTypeB || IsQualifierZAndTypeAOrCOrD)) &&
											!(IsExport && (IsQualifierV || (IsQualifierYAndTypeB && Address.AuthorisationNumber.StartsWith(CountryCodes.Spain))));

	bool IsQualifierV => CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;

	bool IsExport => EntryInstruction?.JobDeclaration?.IsExport ?? false;

	bool IsQualifierZAndTypeAOrCOrD => CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address && CGL_Type.In(TypesA_C_D);
	ZString[] TypesA_C_D => new ZString[] { CusGoodsLocationTypeList.Codes.DesignatedLocation, CusGoodsLocationTypeList.Codes.ApprovedPlace, CusGoodsLocationTypeList.Codes.Other };

	protected override Type AddressType => typeof(CusGoodsLocationAddress);

	protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

	protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);
}
