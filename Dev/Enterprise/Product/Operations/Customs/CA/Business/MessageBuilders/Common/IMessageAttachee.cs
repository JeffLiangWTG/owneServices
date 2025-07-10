using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public interface IMessageAttachee
	{
		ZString MessageType { get; }
		ZString MessageStatus { get; set; }
		EDIMessageCollection Messages { get; }
		BusinessObjectFactory Factory { get; }
	}
}
