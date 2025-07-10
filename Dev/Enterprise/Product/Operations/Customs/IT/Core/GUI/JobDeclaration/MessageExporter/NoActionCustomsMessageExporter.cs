using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.GUI;

public class NoActionCustomsMessageExporter : ICustomsMessageExporter
{
	void ICustomsMessageExporter.SaveToFile(ITEDIMessage message)
	{
	}
}
