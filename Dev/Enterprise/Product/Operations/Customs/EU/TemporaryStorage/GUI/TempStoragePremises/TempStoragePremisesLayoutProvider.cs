using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class TempStoragePremisesLayoutProvider : ITempStoragePremisesLayoutProvider
	{
		IPanelLayoutProvider ITempStoragePremisesLayoutProvider.GetTempStoragePremisesDetailsLayout() => new TempStoragePremisesDetailsLayout();
	}
}
