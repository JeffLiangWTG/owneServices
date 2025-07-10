using System;
using CargoWise.BrandManager;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.GB.Ccsuk.Connection.Exceptions
{
	[Serializable]
	public class CcsukException : HostedServiceException
	{
		public CcsukException(string message, Exception inner) : base(message, inner)
		{
		}

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		public CcsukException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public CcsukException()
		{
		}
	}
}

namespace Enterprise.Customs.GB.Ccsuk.Connection.Exceptions.Sockets
{
	[Serializable]
	public class CannotConnectException : CcsukException
	{
		public CannotConnectException(Exception inner) : base("Could not connect to CCSUK", inner) { }
		public CannotConnectException() { }

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected CannotConnectException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public override string Message
		{
			get { return EnvProxy.IsHostedWithCargowise ? "Could not connect the initial outbound socket to CCSUK." : $"Could not connect the initial outbound socket to CCSUK. This is not a fault in {BrandingFactory.Instance.ProductName}. Either the endpoint in the registry is incorrect, CCS-UK refused your connection, or your NAT/routing is faulty. Perform [NETSTAT -a -n -p tcp] on the service task host to look for a(n attempted) connection to the CCSUK endpoint. If you see one in status SYN_SENT then it's likely that your NAT is faulty (either it is not NAT'ing the outbound packet so that packet is not routable to CCSUK, or it's NAT'ing to the wrong source address and CCSUK is ignoring the packet, or you're not NAT'ing the response and the initial SYN-ACK packet can't get back to {BrandingFactory.Instance.ProductName}). This is not a fault in {BrandingFactory.Instance.ProductName}. You should show message this to your network manager, who should understand exactly what to do (first step: read the setup chapter in the CCSUK fuctional guide on the CargoWise website)."; }
		}
	}

	[Serializable]
	public class CannotListenNoFreePortException : CcsukException
	{
		public CannotListenNoFreePortException() { }

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected CannotListenNoFreePortException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public override string Message
		{
			get { return string.Format("Could not start a listening server, there are no free TCP ports in the CCSUK-allocated range (usually 5000-5005). This is not a fault in {0}. This could mean that too many connections are already open. If you need to use another range, adjust the registry and ensure you NAT the source port too. If you need several simultaneous connections all on the standard port range, ensure that each is bound to a separate source IP and that the NAT reflects this.", BrandingFactory.Instance.ProductName); }
		}
	}
}

namespace Enterprise.Customs.GB.Ccsuk.Connection.Exceptions.Misc
{
	[Serializable]
	public class NoMoreAcceptableProfilesLeftException : CcsukException
	{
		public NoMoreAcceptableProfilesLeftException(Exception inner) : base("No more valid connection profiles remain and all tried so far were unsuccessful. Cannot log on.", inner) { }
		public NoMoreAcceptableProfilesLeftException() { }

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected NoMoreAcceptableProfilesLeftException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public override string Message
		{
			get { return "All valid connection profiles were tried and none were successful. Valid means that the local IP address is availble for binding (i.e. applicable to this server). Unsuccessful means the full cycle of connections and logon did not complete successfully - see other log entries for details."; }
		}
	}

	[Serializable]
	public class CannotSaveInboundException : CcsukException
	{
		public CannotSaveInboundException(Exception inner) : base("Could not save inbound message from CCSUK", inner) { }
		public CannotSaveInboundException() { }

#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected CannotSaveInboundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
		{ }
#endif

		public override string Message
		{
			get { return "Could not save inbound data despite multiple attempts. This exception will cause a reconnect."; }
		}
	}
}

namespace Enterprise.Customs.GB.Ccsuk.Connection.Exceptions.ShortMessage
{
	[Serializable]
	public class UnknownShortMessageCode : CcsukException
	{
#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected UnknownShortMessageCode(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public UnknownShortMessageCode(string information)
		{
			this.information = information;
		}

		readonly string information;
		public override string Message
		{
			get
			{
				return "ShortMessageResponse parser could not understand this message type.  Was: " + information;
			}
		}
	}

	[Serializable]
	public class CannotLogonException : CcsukException
	{
#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected CannotLogonException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		public CannotLogonException(LogonResponse information)
		{
			this.information = information;
		}

		readonly LogonResponse information;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public override string Message
		{
			get
			{
				return string.Format("Logon was unsuccessful. This is not a fault in {0}. You should contact CCSUK to check your credentials are good and that your host is not barred. Reason={1}; Payload={2}", BrandingFactory.Instance.ProductName, information.ReasonForFailure, information.PayloadAsString);
			}
		}
	}

	[Serializable]
	public class UnknownResponseCode : CcsukException
	{
#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected UnknownResponseCode(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
		{ }
#endif

		public UnknownResponseCode(string information)
		{
			this.information = information;
		}

		readonly string information;
		public override string Message
		{
			get
			{
				return "ShortMessageResponse parser could not understand this response code.  Was: " + information;
			}
		}
	}

	[Serializable]
	public class UnknownResponseReasonCode : CcsukException
	{
#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected UnknownResponseReasonCode(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
		{ }
#endif

		public UnknownResponseReasonCode(string information)
		{
			this.information = information;
		}

		readonly string information;
		public override string Message
		{
			get
			{
				return "ShortMessageResponse parser could not understand this response reason code.  Was: " + information;
			}
		}
	}
}

namespace Enterprise.Customs.GB.Ccsuk.Connection.Exceptions.Handshake
{
	[Serializable]
	public class BadHandshakeIdentifier : CcsukException
	{
#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected BadHandshakeIdentifier(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
		{ }
#endif

		public BadHandshakeIdentifier(string information)
		{
			this.information = information;
		}

		readonly string information;
		public override string Message
		{
			get
			{
				return "HandShakeResponse parser was passed something to parse that was not parsable as a handshake. Its first 16 chars were: " + information;
			}
		}
	}

	[Serializable]
	public class NoInboundConnectionMadeException : CcsukException
	{
#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected NoInboundConnectionMadeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
		{ }
#endif

		public NoInboundConnectionMadeException(int information)
		{
			this.information = information;
		}

		readonly int information;
		public override string Message
		{
			get
			{
				return string.Format("The inbound listener did not receive any connection request after {0} tries.  Perhaps the handshake was bad? Perhaps the hostname or callback IP was bad? Perhaps your NAT is bad?", information);
			}
		}
	}

	[Serializable]
	public class BadHandshakeMessageType : CcsukException
	{
#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected BadHandshakeMessageType(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
		{ }
#endif

		public BadHandshakeMessageType(string information)
		{
			this.information = information;
		}

		readonly string information;
		public override string Message
		{
			get
			{
				return "HandShakeResponse parser cannot understand the response as it was not a HandShakeResponse.  It did not have 0002 as the message type. It was: " + information;
			}
		}
	}

	[Serializable]
	public class UnexpectedResponseCode : CcsukException
	{
#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected UnexpectedResponseCode(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
		{ }
#endif

		public UnexpectedResponseCode(string information)
		{
			this.information = information;
		}

		readonly string information;
		public override string Message
		{
			get
			{
				return "HandShakeResponse parser could not understand handshake response code.  Was: " + information;
			}
		}
	}
	[Serializable]
	public class HandshakeNotAcceptedException : CcsukException
	{
#if NETFRAMEWORK
		[Obsolete("SYSLIB0051: Legacy serialization support APIs are obsolete")]
		protected HandshakeNotAcceptedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		: base(info, context)
		{ }
#endif

		public HandshakeNotAcceptedException(string information)
		{
			this.information = information;
		}

		readonly string information;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public override string Message
		{
			get
			{
				return string.Format("Handshake was not accepted by the CCSUK host. The credentials in your registry (e.g. host mnemonic, IP addresses) are possibly incorrect.  " +
					"It is also possible that the network address translation (NAT) is bad such that the CCSUK host sees an incorrect source address which mismatches " +
					"the claimed handshake address. More detail follows. This is an error in configuration, either in the {0} registry, the NAT rules, or otherwise; " +
					"it is not a fault in the {0} software. Please raise a new eRequest and make sure to include details of this exception, " +
					"the preceding and following service task log entries, and details of the registry configuration (IP addresses and host mnemonic). Further detail: {1}",
					Core.Constants.ProductName, information);
			}
		}
	}
}
