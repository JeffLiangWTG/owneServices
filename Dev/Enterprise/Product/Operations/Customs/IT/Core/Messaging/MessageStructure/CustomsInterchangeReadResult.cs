using System;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.MessageStructure;

public class CustomsInterchangeReadResult<T> where T : CustomsInterchange
{
	public Exception Exception { get; set; }

	public ZString ErrorText => Exception?.Message;

	public T Interchange { get; set; }

	public ZBool IsValid => ErrorText.IsEmpty;
}
