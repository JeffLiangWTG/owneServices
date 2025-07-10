using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Licensing
{
	public class SystemDataUpdater
	{
		public const string InvalidSID = "Invalid SID";
		public const string NoCompanyFoundToUpdate = "Failed To Determine company When Updating";
		public const string DataPacketCorrupt = SystemUpdatePacket.PacketWasCorrupt;
		public const string InvalidLicence = "Invalid Licence";

		public SystemDataUpdater(string updateDataXMLPacket)
			: this(updateDataXMLPacket, null)
		{
		}

		internal SystemDataUpdater(string updateDataXMLPacket, ILicenceUpdateNotification response)
		{
			this.UpdateDataXMLPacket = updateDataXMLPacket;
			Response = response ?? new LicenceUpdateNotification();
		}

		internal readonly string UpdateDataXMLPacket;
		internal readonly ILicenceUpdateNotification Response;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory() { RefreshEnabled = false }); }
		}
		BusinessObjectFactory factory;

		public bool Process(List<ITransactionParticipant> participants = null)
		{
			bool result = false;
			try
			{
				SystemUpdatePacket updateDataPacket = new SystemUpdatePacket(UpdateDataXMLPacket);
				if (!string.IsNullOrEmpty(updateDataPacket.EncryptedSysRegKey))
				{
					UpdateSystemKey(updateDataPacket.EncryptedSysRegKey);
					SendResponse();
					result = true;
				}
			}
			catch (InvalidOperationException ex)
			{
				SendResponse(ex.Message);
			}

			if (factory != null)
			{
				if (participants != null)
				{
					participants.Add(factory);
				}
				else
				{
					factory.Save();
				}
			}

			return result;
		}

		void SendResponse(string errorMessage = null)
		{
			Response.Send(Factory, UpdateDataXMLPacket, errorMessage ?? "OK");
		}

		#region Update System Key

		void UpdateSystemKey(string encryptedSysRegKey)
		{
			ISystemRegistrationKey newKey = SystemRegistrationKey.NewFromEncryptedXmlKey(encryptedSysRegKey);

			if (newKey.ServerSid.Equals(AdminConnection.ServerSid))
			{
				Env.Registry.LegacyEncryptedSystemRegistrationKey = encryptedSysRegKey;
			}
			else
			{
				throw new InvalidOperationException(InvalidSID);
			}
		}

		#endregion
	}
}
