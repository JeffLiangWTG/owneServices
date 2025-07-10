using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.xTMessaging.Business
{
	public interface IEDIMessageCreator
	{
		ZString CreateEDIMessagesForInterchange(Stream payload, BusinessObjectFactory factory);
		ZString MessageProcessNoteType { get; }
	}
}
