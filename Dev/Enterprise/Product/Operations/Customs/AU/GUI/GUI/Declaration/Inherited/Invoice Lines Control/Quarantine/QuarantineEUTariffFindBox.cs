using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class QuarantineEUTariffFindBox : Universal.GUI.TariffFindBox
	{
		public QuarantineEUTariffFindBox() : base()
		{
			TariffType = Universal.Constants.TariffTypes.Export;
			GetDataGrouping = DataGroupingFunc;
			GetSelectNomenclatureModes = NomenclatureModesFunc;
		}

		ZString DataGroupingFunc() => Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

		List<SelectionStyle> NomenclatureModesFunc()
		{
			return new List<SelectionStyle>()
			{
				SelectionStyle.Subheading,
				SelectionStyle.EightCharNomenclature,
				SelectionStyle.Tariff
			};
		}
		protected override ITariffFormatter GetTariffFormatter()
		{
			return (CurrentItem as QuarantineExDocLine)?.EUTariffFormatter;
		}
	}
}
