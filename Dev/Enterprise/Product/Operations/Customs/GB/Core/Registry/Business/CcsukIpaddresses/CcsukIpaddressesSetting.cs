using System.Net;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class CcsukIpaddressesSetting : RegistryBusinessObjectTemplate
	{
		#region schema and constructors
		public abstract class Schema
		{
			public const string LocalIpAddress = "LocalIpAddress";
			public const string CcsukParticipantIpAddress = "CcsukParticipantIpAddress";
			public const string FriendlyName = "FriendlyName";
			public const string Sequence = "Sequence";
			public const string Transport = "Transport";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CcsukIpaddressesSetting(fallbackLevel, factory);
		}

		public CcsukIpaddressesSetting()
		{
		}

		public CcsukIpaddressesSetting(string local, string participant, string name, int sequence)
		{
			LocalIpAddress = local;
			CcsukParticipantIpAddress = participant;
			FriendlyName = name;
			Sequence = sequence;
		}

		public CcsukIpaddressesSetting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CcsukIpaddressesSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}
		#endregion

		#region LocalIpAddress
		[MaxLength(20)]
		public ZString LocalIpAddress
		{
			get { return fLocalIpAddress; }
			set
			{
				SetNonPersistentPropertyValue(LocalIpAddressInfo, ref fLocalIpAddress, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateLocalIpAddress();
				}
			}
		}
		ZString fLocalIpAddress;

		public ZPropertyInfo LocalIpAddressInfo
		{
			get { return GetZPropertyInfo(Schema.LocalIpAddress); }
		}

		public void ValidateLocalIpAddress()
		{
			LocalIpAddressInfo.ClearAllNotifications();
			ValidateIpAddress(LocalIpAddressInfo);
			ValidateTransport();
		}

		#endregion

		#region CcsukParticipantIpAddress
		[MaxLength(20)]
		public ZString CcsukParticipantIpAddress
		{
			get { return fCcsukParticipantIpAddress; }
			set
			{
				SetNonPersistentPropertyValue(CcsukParticipantIpAddressInfo, ref fCcsukParticipantIpAddress, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateCcsukParticipantIpAddress();
				}
			}
		}
		ZString fCcsukParticipantIpAddress;

		public ZPropertyInfo CcsukParticipantIpAddressInfo
		{
			get { return GetZPropertyInfo(Schema.CcsukParticipantIpAddress); }
		}

		public void ValidateCcsukParticipantIpAddress()
		{
			CcsukParticipantIpAddressInfo.ClearAllNotifications();
			ValidateIpAddress(CcsukParticipantIpAddressInfo);
			ValidateTransport();
		}

		#endregion

		#region FriendlyName
		[MaxLength(20)]
		public ZString FriendlyName
		{
			get { return fFriendlyName; }
			set
			{
				SetNonPersistentPropertyValue(FriendlyNameInfo, ref fFriendlyName, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateFriendlyName();
				}
			}
		}
		ZString fFriendlyName;

		public ZPropertyInfo FriendlyNameInfo
		{
			get { return GetZPropertyInfo(Schema.FriendlyName); }
		}

		public void ValidateFriendlyName()
		{
			ValidateTransport();
		}
		#endregion

		#region Sequence
		public ZInt Sequence
		{
			get { return fSequence; }
			set
			{
				SetNonPersistentPropertyValue(SequenceInfo, ref fSequence, value);
				if (!IsValidationSuspended)
				{
					ValidateSequence();
				}
			}
		}
		ZInt fSequence;

		public ZPropertyInfo SequenceInfo
		{
			get { return GetZPropertyInfo(Schema.Sequence); }
		}

		public void ValidateSequence()
		{
			ValidateTransport();
		}
		#endregion

		public CodeDescriptionPairList CcsukIpTransportListForLookup
		{
			get { return new CcsukIpTransportList(); }
		}

		#region Transport
		[MaxLength(3)]
		[List(nameof(CcsukIpTransportListForLookup))]
		public ZString Transport
		{
			get { return fTransport; }
			set
			{
				SetNonPersistentPropertyValue(TransportInfo, ref fTransport, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateTransport();
				}
			}
		}
		ZString fTransport;

		public ZString DefaultTransportBasedOnIfHostedWithCargoWiseAndTransportValue
		{
			get
			{
				string defaultTransport;
				var transport = Transport;

				if (EnvProxy.IsHostedWithCargowise)
				{
					defaultTransport = transport.IsEmpty ? (ZString)CcsukIpTransportList.Codes.VPN : transport;
				}
				else
				{
					defaultTransport = transport.IsEmpty ? (ZString)CcsukIpTransportList.Codes.IPC : transport;
				}
				return defaultTransport;
			}
		}

		public ZPropertyInfo TransportInfo
		{
			get { return GetZPropertyInfo(Schema.Transport); }
		}

		public void ValidateTransport()
		{
			TransportInfo.ClearAllNotifications();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(TransportInfo);
			if (EnvProxy.IsHostedWithCargowise && Transport == CcsukIpTransportList.Codes.IPC)
			{
				TransportInfo.AddError("IPConnect is not allowed for hosted clients.  Select only VPN.");
			}
		}
		#endregion

		public ZString FullDescription => $"Profile: {FriendlyName}; Participant:{CcsukParticipantIpAddress}; Local:{LocalIpAddress}; Transport:{Transport}";

		void ValidateIpAddress(ZPropertyInfo zPropInfo)
		{
			zPropInfo.ClearAllNotifications();
			if (!zPropInfo.Value.IsEmpty)
			{
				IPAddress result;
				if (!IPAddress.TryParse(zPropInfo.Value.ToString(), out result))
				{
					zPropInfo.AddError("That is not a valid IPv4 address");
				}
			}
			else
			{
				zPropInfo.AddError("An IPv4 address is required");
			}
		}

		#region XML Reading and Writing
		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.LocalIpAddress, LocalIpAddress);
			writer.WriteElementString(Schema.CcsukParticipantIpAddress, CcsukParticipantIpAddress);
			writer.WriteElementString(Schema.FriendlyName, FriendlyName);
			writer.WriteElementString(Schema.Sequence, Sequence.ToString());
			writer.WriteElementString(Schema.Transport, Transport.ToString());
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			LocalIpAddress = reader.ReadElementString(Schema.LocalIpAddress);
			CcsukParticipantIpAddress = reader.ReadElementString(Schema.CcsukParticipantIpAddress);
			FriendlyName = reader.ReadElementString(Schema.FriendlyName);
			Sequence = reader.ReadElementStringAsZInt(Schema.Sequence);
			Transport = reader.ReadElementString(Schema.Transport);
		}
		#endregion

	}
}
