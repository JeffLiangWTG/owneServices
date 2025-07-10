using System;
using System.Collections.Generic;

namespace Enterprise.ServiceManager.Business
{
	public class SendToAddress : AutoSendToAddress
	{
		public SendToAddress() { }

		public IReadOnlyList<string> Recipients
		{
			get => Array.FindAll(Array.ConvertAll(Address.Split(';'), recipient => (string)recipient.Trim()), recipient => !string.IsNullOrEmpty(recipient));
		}
	}
}
