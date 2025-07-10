using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class CusTempStorageRegPremisesValidation : AutoCusTempStorageRegPremisesValidation
{
	public CusTempStorageRegPremisesValidation(AutoCusTempStorageRegPremises parent) : base(parent)
	{
	}

	protected override void CheckSRP_Code()
	{
		base.CheckSRP_Code();

		var parent = Parent;

		var codeInfo = parent.SRP_CodeInfo;
		MandatoryValidation.CheckEntered(codeInfo);

		var code = parent.SRP_Code;

		ZQuery GetCodeQuery()
		{
			var query = new ZQuery(CusTempStorageRegPremisesSchema.SRP_Code, code);
			return query.AddToFilter(CusTempStorageRegPremisesSchema.PK, SQLComparisonOperator.NotEqual, parent.PK);
		}

		if (!code.IsEmpty && parent.Factory.ExistsInDatabase(CusTempStorageRegPremisesSchema.Constants.TableName, GetCodeQuery()))
		{
			codeInfo.AddError(Res.GetString("B254FD08-56D9-4E0A-90D1-290BDB0E5661", "Code must be unique, please enter a different code"));
		}
	}

	protected override void CheckSRP_Description()
	{
		base.CheckSRP_Description();

		MandatoryValidation.CheckEntered(Parent.SRP_DescriptionInfo);
	}

	protected override void CheckSRP_CustomsLocation()
	{
		base.CheckSRP_CustomsLocation();

		MandatoryValidation.WarnIfNotEntered(Parent.SRP_CustomsLocationInfo);
	}

	protected override void CheckSRP_Type()
	{
		base.CheckSRP_Type();

		ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.SRP_TypeInfo);
	}
}
