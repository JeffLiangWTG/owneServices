using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageProcessors
{
	public interface ICUSRESMessageProvider
	{
		ZString DocumentMessageName { get; }
		ZDateTime AdmissionDate { get; }
		ZString MessageFunction { get; }
		List<ErrorMessage> FreeTextErrors { get; }
	}

	public class ErrorMessage
	{
		public ZString Code;
		public ZString Location;
		public ZString Description;
	}
}
