using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business
{
	public partial class VatCanaForAI2List : CodeDescriptionPairList
	{
		public VatCanaForAI2List(BusinessObjectFactory factory)
		{
			var filter = new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, JoinCondition.And, GuaranteeTypeList.Codes.AI2) };
			var canaList = ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.France, UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, ZDateTime.Today, filter);

			foreach (var cana in canaList)
			{
				AddPair(cana.ZZD_Code, cana.ZZD_Description);
			}
		}
	}
}
