using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IITEDIMessagePrettyFormatter
{
	ZString GetFormattedText(ZString originalText);
}
