using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public class UniversalTariffExportFindBox : Universal.GUI.TariffFindBox
	{
		public UniversalTariffExportFindBox() : base()
		{
			TariffType = Universal.Constants.TariffTypes.Export;
		}

		protected override ITariffFormatter GetTariffFormatter() => new AUExportTariffUniversalFormatter();
	}
}
