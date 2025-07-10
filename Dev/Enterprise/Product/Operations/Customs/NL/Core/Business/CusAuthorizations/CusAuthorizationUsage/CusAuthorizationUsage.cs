using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class CusAuthorizationUsage : EU.Business.CusAuthorizationUsage, Integration.Customs.NL.ICusAuthorizationUsage
{
	public CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.Business.AutoCusAuthorizationUsage.Schema
	{
	}

	public new CusEntryInstruction Instruction => (CusEntryInstruction)base.Instruction;

	public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

	public new CusAuthorizationUsageLookups Lookups => (CusAuthorizationUsageLookups)base.Lookups;
	protected override EU.Business.CusAuthorizationUsageLookups GetNewLookups() => new CusAuthorizationUsageLookups(this);

	public new CusAuthorizationUsageValidation Validation => (CusAuthorizationUsageValidation)base.Validation;
	protected override EU.Business.CusAuthorizationUsageValidation GetNewValidation() => new CusAuthorizationUsageValidation(this);

	public override ZString AGC_Number
	{
		get => base.AGC_Number;
		set
		{
			base.AGC_Number = value;
			SetDefaultsForGoodsLocation();
		}
	}

	void SetDefaultsForGoodsLocation()
	{
		if (Parent is CusEntryInstruction parent)
		{
			parent.GoodsLocation.SetDefaultsFromAuthorizationIfNeeded(CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificNumberTypeAndCountryCode(Factory, AGC_Number, AGC_Code, parent.JobDeclaration.CountryCode).FirstOrDefault());
			parent.GoodsLocationDescriptionInfo.RefreshBinding();
		}
	}
}
