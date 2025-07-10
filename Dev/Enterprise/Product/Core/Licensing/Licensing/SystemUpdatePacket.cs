using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Enterprise.Licensing
{
	public class SystemUpdatePacket
	{
		public SystemUpdatePacket()
		{
		}

		public SystemUpdatePacket(string xMLPacket)
		{
			LoadFromXML(xMLPacket);
		}

		#region Load From XML

		void LoadFromXML(string xMLPacket)
		{
			StringReader stringReader = null;
			try
			{
				stringReader = new StringReader(xMLPacket);
				using (var keyParser = new XmlTextReader(stringReader))
				{
					stringReader = null;
					while (keyParser.Read())
					{
						HandleSysRegKey(keyParser);
						HandleLicenceKey(keyParser);
						HandlePacketHash(keyParser);
					}
				}

				CheckHashCodeInPacket();
			}
			catch (Exception e)
			{
				throw new InvalidOperationException(PacketWasCorrupt, e);
			}
			finally
			{
				if (stringReader != null)
				{
					stringReader.Dispose();
				}
			}
		}

		#region Node Handlers

		void HandleSysRegKey(XmlTextReader keyParser)
		{
			if (keyParser.Name == "ReferenceData")
			{
				EncryptedSysRegKey = keyParser.ReadString();
			}
		}

		void HandleLicenceKey(XmlTextReader keyParser)
		{
			if (keyParser.Name == "SystemData")
			{
				EncryptedLicenceKey = keyParser.ReadString();
			}
		}

		void HandlePacketHash(XmlTextReader keyParser)
		{
			if (keyParser.Name == "ValidityCheck")
			{
				fPacketHash = keyParser.ReadString();
			}
		}

		#endregion

		#endregion

		#region To XML Key String

		public string ToXMLString()
		{
			string packetForHashing = String.Format(CultureInfo.InvariantCulture, XMLTemplate, EncryptedSysRegKey, EncryptedLicenceKey, String.Empty);
			return String.Format(CultureInfo.InvariantCulture, XMLTemplate, EncryptedSysRegKey, EncryptedLicenceKey, GenerateHash(packetForHashing));
		}

		#endregion

		#region XML Template

		internal string XMLTemplate =
			"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
			"	<ReferenceDataUpdate>" + System.Environment.NewLine +
			"		<ReferenceData>{0}</ReferenceData>" + System.Environment.NewLine + // This is the SystemRegistrationKey
			"		<SystemData>{1}</SystemData>" + System.Environment.NewLine + // This is any Licence Key deployed to the client
			"		<ValidityCheck>{2}</ValidityCheck>" + System.Environment.NewLine + // This is an MD5 Hash of the packet. (this field is assumed empty while calculating the hash)
			"	</ReferenceDataUpdate>";

		#endregion

		#region Packet Hashing

		void CheckHashCodeInPacket()
		{
			string packetForHashing = String.Format(CultureInfo.InvariantCulture, XMLTemplate, EncryptedSysRegKey, EncryptedLicenceKey, String.Empty);
			string expectedhash = GenerateHash(packetForHashing);

			if (expectedhash != fPacketHash)
			{
				throw new InvalidOperationException(PacketHashInvalid);
			}
		}

		internal string GenerateHash(string xMLData)
		{
			using var provider = MD5.Create();
			return BitConverter.ToString(provider.ComputeHash(Encoding.UTF8.GetBytes(xMLData))).Replace("-", "");
		}

		#endregion

		public string EncryptedSysRegKey;
		public string EncryptedLicenceKey;
		string fPacketHash;

		const string PacketHashInvalid = "The packet hash does not match its original contents";
		internal const string PacketWasCorrupt = "The System Information Packet Was Corrupt";
	}
}
