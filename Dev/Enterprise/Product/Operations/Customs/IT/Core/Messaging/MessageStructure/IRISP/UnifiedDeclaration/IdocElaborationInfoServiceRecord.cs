using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Messaging.MessageStructure.IRISP;

public class IdocElaborationInfoServiceRecord
{
	public ZString InnerText { get; protected set; }
	public ZDateTime IdocDateTimeOfReceipt { get; protected set; }
	public ZString IdocName { get; protected set; }
	internal void Load(ZString line)
	{
		InnerText = line;
		IdocDateTimeOfReceipt = line.SubStringAndTrim(11, 14).ParseToDateTimeWithFormat((NoResString)"dd/MM/yy HH:mm");
		IdocName = line.SubStringAndTrim(26, 12);
	}
}
