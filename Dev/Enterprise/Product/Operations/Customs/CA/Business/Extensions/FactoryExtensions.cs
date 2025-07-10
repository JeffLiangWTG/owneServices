using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public static class FactoryExtensions
	{
		public static ZString CanadianCarrierCode(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CA.Business.CandianCarrierCode", () =>
			{
				var result = GlbCompany.CurrentCompany.OrgProxy?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Canada) ?? ZString.Empty;
				if (result.IsEmpty)
				{
					foreach (var candianCompany in GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.Canada, factory).OrderBy(x => x.GC_Code))
					{
						result = candianCompany.OrgProxy?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Canada) ?? ZString.Empty;
						if (!result.IsEmpty)
						{
							break;
						}
					}
				}
				return result;
			});
		}

		public static BusinessObjectFactory GetFactoryFromBusinessObject(BusinessObject businessObject)
		{
			Argument.NotNull(businessObject, "businessObject");
			return businessObject.Factory;
		}
	}
}
