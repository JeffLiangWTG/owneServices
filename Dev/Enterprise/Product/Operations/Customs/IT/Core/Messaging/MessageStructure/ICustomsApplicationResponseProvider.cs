using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageStructure;

public interface ICustomsApplicationResponseProvider<T>
	where T : IMrnProvider
{
	IList<T> ApplicationResponses { get; }
}

public interface IMrnProvider
{
	ZString Mrn { get; }
}
