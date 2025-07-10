using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class TransportCoDefaultingRules : RegistryBusinessObjectTemplate
	{
		public TransportCoDefaultingRules()
		{
		}

		#region Validator

		SequenceValidator Validator => validator ??= new SequenceValidator(GetPropertyInfosToValidate());

		SequenceValidator validator;

		ZPropertyInfo[] GetPropertyInfosToValidate()
		{
			return new ZPropertyInfo[]
			{
				ConsigneeInfo,
				ClientInfo,
				WarehouseInfo
			};
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransportCoDefaultingRules();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Consignee = new ZByte(reader.ReadElementString(Schema.Consignee));
			Client = new ZByte(reader.ReadElementString(Schema.Client));
			Warehouse = new ZByte(reader.ReadElementString(Schema.Warehouse));
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateConsignee();
			ValidateClient();
			ValidateWarehouse();
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			Consignee = 1;
			Client = 2;
			Warehouse = 3;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Consignee, Consignee.ToString());
			writer.WriteElementString(Schema.Client, Client.ToString());
			writer.WriteElementString(Schema.Warehouse, Warehouse.ToString());
		}

		#region Consignee

		public ZByte Consignee
		{
			get { return consignee; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(ConsigneeInfo, ref consignee, value);
				if (!IsValidationSuspended)
				{
					ValidateConsignee();
					ConsigneeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ConsigneeInfo
		{
			get { return GetZPropertyInfo(Schema.Consignee); }
		}

		public void ValidateConsignee()
		{
			ConsigneeInfo.ClearAllNotifications();
			Validator.Validate(ConsigneeInfo);
		}

		ZByte consignee;

		#endregion

		#region Client

		public ZByte Client
		{
			get { return client; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(ClientInfo, ref client, value);
				if (!IsValidationSuspended)
				{
					ValidateClient();
					ClientInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ClientInfo
		{
			get { return GetZPropertyInfo(Schema.Client); }
		}

		public void ValidateClient()
		{
			ClientInfo.ClearAllNotifications();
			Validator.Validate(ClientInfo);
		}

		ZByte client;

		#endregion

		#region Warehouse

		public ZByte Warehouse
		{
			get { return warehouse; }
			set
			{
				SetNonPersistentPropertyValue<ZByte>(WarehouseInfo, ref warehouse, value);
				if (!IsValidationSuspended)
				{
					ValidateWarehouse();
					WarehouseInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo WarehouseInfo
		{
			get { return GetZPropertyInfo(Schema.Warehouse); }
		}

		public void ValidateWarehouse()
		{
			WarehouseInfo.ClearAllNotifications();
			Validator.Validate(WarehouseInfo);
		}

		ZByte warehouse;

		#endregion

		public static class Schema
		{
			public const string Consignee = "Consignee";
			public const string Client = "Client";
			public const string Warehouse = "Warehouse";
		}
	}
}
