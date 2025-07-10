using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessagePacking
{
	public interface IMessagePackRule
	{
		ZString MessageSubType { get; }
		ZString PackMessages(IEnumerable<ZString> messageTextList);
	}
}
