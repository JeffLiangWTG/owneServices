using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;
public class CusTempStorageRegPremises : EU.TemporaryStorage.Business.CusTempStorageRegPremises
{
	public CusTempStorageRegPremises(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesLookups GetNewLookups() => new CusTempStorageRegPremisesLookups(this);

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesValidation GetNewValidation() => new CusTempStorageRegPremisesValidation(this);

	protected override ZString DefaultAuthorizationCodeCore => SRP_Type == CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility ? ESCusAuthorisationHeaderTypeList.Codes.PremisesAuthorizedForExport : AuthorizationTypeList.Codes.TST;

	public override ZString SRP_Type
	{
		get => base.SRP_Type;
		set
		{
			base.SRP_Type = value;
			Authorization.AGC_Code = DefaultAuthorizationCode;
		}
	}

	protected override Type AuthorizationType => typeof(ES.Business.CusAuthorizationUsage);
}
