using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.GUI;

public interface ICustomsMessageExporter
{
	void SaveToFile(ITEDIMessage message);
}
