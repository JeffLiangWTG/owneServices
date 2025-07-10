using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.GUI;

public class SendH7ToCustomsActionMethod : OperationalActionMethod
{
	public SendH7ToCustomsActionMethod() : base(new Guid("1256be27-343d-42fc-9db0-783e79151c99"))
	{
	}

	public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
	{
		return GetApplicatorObjectFromCountry(CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
	}

	public override string Name
	{
		get { return Res.GetString("a79be3cf-cad7-46e9-8632-8ea73b024835", "Send H7 Jobs to Customs"); }
	}

	public override string Description
	{
		get { return Res.GetString("e66f342f-b332-43f0-a725-1aa44c25e4a3", "Send H7 Jobs to Customs in Bulk"); }
	}

	public override FilterRequirementList GetFilterRequirements()
	{
		var result = base.GetFilterRequirements();
		result.Add(new FilterIsInEuropeanCustomsUnionOrInheritsFromEUConstraint().Name, ["Y"]);
		return result;
	}

	SendH7ToCustomsApplicator GetApplicatorObjectFromCountry(string countryCode)
	{
		switch (countryCode)
		{
			case CountryCodes.Spain:
				return ObjectFactory.Get<SendH7ToCustomsApplicator>("ESSendH7ToCustomsApplicator");
			default:
				return new SendH7ToCustomsApplicator();
		}
	}
}
