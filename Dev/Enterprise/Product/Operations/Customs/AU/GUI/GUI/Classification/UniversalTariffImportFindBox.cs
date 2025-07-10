using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public class UniversalTariffImportFindBox : Universal.GUI.TariffFindBox
	{
		public UniversalTariffImportFindBox() : base()
		{
			TariffType = Universal.Constants.TariffTypes.Import;
		}

		protected override ITariffFormatter GetTariffFormatter() => new AUImportTariffUniversalFormatter();
	}
}
