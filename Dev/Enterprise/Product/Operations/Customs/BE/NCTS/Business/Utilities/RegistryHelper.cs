using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public static class RegistryHelper
{
	public static CustomsRegistry GetValidCustomsRegistryForCompany(CustomsRegistryCollection customsRegistries, ZString declarationType, ZGuid organisationPK) => customsRegistries.Cast<CustomsRegistry>()
			.Where(x => x.Organization == organisationPK && x.DeclarationType == declarationType && x.StartingDate <= ZDateTime.Now)
			.OrderByDescending(x => x.StartingDate)
			.ThenByDescending(x => x.CurrentNo)
			.FirstOrDefault();
}
