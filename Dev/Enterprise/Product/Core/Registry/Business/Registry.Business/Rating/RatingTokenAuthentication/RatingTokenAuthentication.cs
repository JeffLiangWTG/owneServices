using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RatingTokenAuthentication : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Endpoint = "Endpoint";
			public const string ClientId = "ClientId";
			public const string StaffCode = "StaffCode";
		}

		#endregion

		#region Properties

		public ZString Endpoint
		{
			get => endpoint;
			set
			{
				SetNonPersistentPropertyValue(EndpointInfo, ref endpoint, value);
				if (!IsValidationSuspended)
				{
					ValidateEndpoint();
				}
			}
		}
		ZString endpoint;

		void ValidateEndpoint()
		{
			EndpointInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(EndpointInfo);
			if (!Endpoint.IsEmpty && !Uri.TryCreate(Endpoint, UriKind.Absolute, out _))
			{
				EndpointInfo.AddError(Res.GetString("3A3CAB31-4009-4161-8537-205A457470AB", "Endpoint is in an invalid format"));
			}
		}

		public ZPropertyInfo EndpointInfo => GetZPropertyInfo(nameof(Endpoint));

		[MaxLength(100)]
		public ZString ClientId
		{
			get => clientId;
			set
			{
				SetNonPersistentPropertyValue(ClientIdInfo, ref clientId, value);
				if (!IsValidationSuspended)
				{
					ValidateClientId();
				}
			}
		}
		ZString clientId;

		void ValidateClientId()
		{
			ClientIdInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ClientIdInfo);
			if (!ZGuid.TryParse(clientId, out _))
			{
				ClientIdInfo.AddError(Res.GetString("B00A0899-82BE-456D-BC5D-5C7C21DC9CE4", "The client id should be in GUID format."));
			}
		}

		public ZPropertyInfo ClientIdInfo => GetZPropertyInfo(nameof(ClientId));

		[List("StaffCollection")]
		public ZString StaffCode
		{
			get => staffCode;
			set
			{
				SetNonPersistentPropertyValue(StaffCodeInfo, ref staffCode, value);
				if (!IsValidationSuspended)
				{
					ValidateStaffCode();
				}
			}
		}
		ZString staffCode;

		void ValidateStaffCode()
		{
			StaffCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(StaffCodeInfo);
			if (!staffCode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(StaffCodeInfo, StaffCollection);
			}
		}

		public ZPropertyInfo StaffCodeInfo => GetZPropertyInfo(nameof(StaffCode));

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateClientId();
			ValidateStaffCode();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RatingTokenAuthentication();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Endpoint = reader.ReadElementString(Schema.Endpoint);
			ClientId = reader.ReadElementString(Schema.ClientId);
			StaffCode = reader.ReadElementString(Schema.StaffCode);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Endpoint, Endpoint);
			writer.WriteElementString(Schema.ClientId, ClientId);
			writer.WriteElementString(Schema.StaffCode, StaffCode);
		}

		public IActiveBusinessObjectCollection StaffCollection => CurrentFactory.GetCachedValue("RatingTokenAuthentication.StaffCollection", () => (IActiveBusinessObjectCollection)ObjectFactory.Get<IGlbStaffCollection>("IGlbStaffCollection", CurrentFactory));

		public string GetLoginNameFromStaffCode()
		{
			if (StaffCode.IsEmpty)
			{
				return string.Empty;
			}

			var staff = StaffCollection.OfType<IGlbStaff>().FirstOrDefault(s => s.GS_Code == StaffCode);
			return staff?.GS_LoginName;
		}
	}
}
