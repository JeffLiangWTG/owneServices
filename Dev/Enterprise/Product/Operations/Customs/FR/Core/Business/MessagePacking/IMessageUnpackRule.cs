using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.MessagePacking
{
	public interface IMessageUnpackRule
	{
		ZString MessageSubType { get; }
		IEnumerable<ZString> UnpackMessage(ZString messageText);
	}
}
