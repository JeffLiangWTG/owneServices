using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public static class DangerousGoodsHelper
	{
		public static ZString GetDangerousGoodsName(UNDGDataItem dangerousGoods)
		{
			ZString result = ZString.Empty;
			var names = dangerousGoods?.Substance?.Names;
			if (names != null)
			{
				result = names.FirstOrDefault(name => name.DA_Language == Core.Constants.Languages.ChineseSimplified)?.DA_Descriptor
								  ?? names.FirstOrDefault(name => name.DA_Language == Core.Constants.Languages.English)?.DA_Descriptor
								  ?? dangerousGoods.Substance.DG_PSN;
			}

			return result;
		}

		public static bool IsDangerousChemical(BusinessObjectFactory factory, AdditionalInformationHelper helper, ZDateTime effectiveAssessmentDate)
		{
			var nameOfGoods = helper.NameOfGoods;
			var cas = helper?.GetAdditionalElementValue(CASElementStrategy.AdditionalElementCode) ?? ZString.Empty;

			return (!cas.IsEmpty && CNRefCusCodeListTypes.GetDangerousChemicalCASList(factory, effectiveAssessmentDate).Contains(cas))
				|| (!nameOfGoods.IsEmpty && CNRefCusCodeListTypes.GetDangerousChemicalGoodsNameList(factory, effectiveAssessmentDate).Contains(nameOfGoods));
		}
	}
}
