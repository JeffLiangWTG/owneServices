using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;

public interface ISumARegisterFormLayoutProvider
{
	IPanelLayoutProvider GetDetailsHeaderLayout();

	IPanelLayoutProvider GetLinesDetailsLayout();
}
