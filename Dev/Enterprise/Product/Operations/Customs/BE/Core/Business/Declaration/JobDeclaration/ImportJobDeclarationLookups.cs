using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration;

public class ImportJobDeclarationLookups : JobDeclarationLookups
{
	public ImportJobDeclarationLookups(JobDeclaration parent) : base(parent)
	{
	}

	public override ZZRefCusCodeListCombinedCollection JE_CustomsOfficeList
	{
		get
		{
			var date = ZDateTime.Today;
			var declaration = Parent;
			var isUcc6 = declaration.Configuration.IsUCC6(declaration);
			return Factory.GetCachedValue("BE.CustomsOfficeList|" + (isUcc6 ? "UCC6" : (NoResString)"no UCC6") + "|" + date, // Cache Key
			() => isUcc6
				? CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(
					Factory,
					Core.Constants.CountryCodes.Belgium,
					EuOfficeCodesTypes.Codes.AuthorityControlCode)
				: base.JE_CustomsOfficeList);
		}
	}
}
