using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class AccessCodeWrapper : IAccessCode
	{
		public AccessCodeWrapper(string accessCode)
		{
			this.AccessCode = accessCode;
		}

		public static AccessCodeWrapper New(string accessCode) => accessCode == null ? null : new AccessCodeWrapper(accessCode);

		public string AccessCode { get; }
	}
}
