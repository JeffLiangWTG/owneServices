using System.Collections;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public class CusAuthorizationUsageLookups : EU.Business.CusAuthorizationUsageLookups
{
	public CusAuthorizationUsageLookups(CusAuthorizationUsage parent) : base(parent)
	{
	}

	public override ICollection CodeList
	{
		get
		{
			var declaration = JobDeclaration;
			return Factory.GetCachedValue("BE.CusAuthorizationUsage.CodeList|" + (declaration?.JE_MessageType ?? ZString.Empty),
				() => (declaration?.IsImport ?? false)
					? new ImportBECusAuthorisationUsageCodeList()
					: (declaration?.IsExport ?? false)
						? new ExportBECusAuthorisationUsageCodeList()
						: new CodeDescriptionPairList());
		}
	}
}
