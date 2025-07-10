using System;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public abstract class ServiceMessage : Body
	{
		protected string incomingBody;

		public sealed override ZString PayloadAsString
		{
			get
			{
				if (IsInbound)
				{
					return this.incomingBody;
				}
				else
				{
					return ServiceMessageHeader + ServiceMessageBody;
				}
			}
		}

		string ServiceMessageHeader
		{
			get
			{
				string mtn = MessageTypeNumber;
				if (string.IsNullOrEmpty(mtn) || mtn.Length != 2)
				{
					throw new ArgumentException(string.Format("MessageTypeNumber in class derived from ServiceMessage is bad. Should be 2-alpha. Was '{0}'. Type={1}", mtn, this.GetType().Name));
				}

				string host = new Host().HostNameFormatted;
				return ServiceMessageIdentifier + mtn + host;
			}
		}

		bool IsInbound
		{
			get
			{
				switch (MessageTypeNumber)
				{
					case ShortMessageTypeCodes.Codes.CargoMessageResponse:
					case ShortMessageTypeCodes.Codes.LogoffResponse:
					case ShortMessageTypeCodes.Codes.LogonResponse:
					case ShortMessageTypeCodes.Codes.PasswordChangeResponse:
					case ShortMessageTypeCodes.Codes.ServiceErrorResponse:
						return true;
					default:
						return false;
				}
			}
		}

		protected abstract string MessageTypeNumber { get; }

		protected abstract string ServiceMessageBody { get; }

		public static string ServiceMessageIdentifier = "SM";
	}
}
