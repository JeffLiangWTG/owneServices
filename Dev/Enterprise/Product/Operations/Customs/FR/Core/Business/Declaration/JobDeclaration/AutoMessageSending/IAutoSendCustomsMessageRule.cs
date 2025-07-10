using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Interfaces
{
	public interface IAutoSendCustomsMessageRule
	{
		ZBool CanSendMessage(CusEntryHeader entry);
		ZString newMessageType { get; }
	}
}
