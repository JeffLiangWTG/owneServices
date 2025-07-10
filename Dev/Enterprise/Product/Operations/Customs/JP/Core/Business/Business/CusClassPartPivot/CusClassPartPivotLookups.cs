using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Business
{
	public class CusClassPartPivotLookups : Customs.Business.CusClassPartPivotLookups
	{
		public CusClassPartPivotLookups(AutoCusClassPartPivot parent) : base(parent)
		{
		}

		public ICodeDescriptionPairList TradeControlOrderAppendixList
		{
			get
			{
				var parent = Parent;
				if (parent.CI_ChildType.IsEmpty || parent.CI_ChildType == ClassificationTypeList.Codes.HTB)
				{
					return new CodeDescriptionPairList();
				}
				return ReferenceDataProvider.GetTradeControlOrderAppendixList(Factory, Parent.CI_ChildType == ClassificationTypeList.Codes.HTE);
			}
		}

		public FEFTAArticle48List FEFTAArticle48List => Factory.GetCachedValue<FEFTAArticle48List>();

		public CodeDescriptionPairList ConsumptionTaxExemptionIDList => ReferenceDataProvider.GetExportConsumptionTaxExemptionCode(Factory);

		public CodeDescriptionPairList StorageTypeList => Factory.GetCachedValue("CusClassPartPivotLookups.StorageTypeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(new StorageTypeListWhenDeclarationTypeIsA());
			result.AddRange(new StorageTypeListWhenDeclarationTypeIsG());
			result.AddRange(new StorageTypeListWhenTransportModeIsSea());
			return result;
		});
	}
}
