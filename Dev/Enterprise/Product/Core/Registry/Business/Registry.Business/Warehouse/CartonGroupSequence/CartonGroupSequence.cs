using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CartonGroupSequence : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string Product = "Product";
			public const string Carrier = "Carrier";
			public const string Consignee = "Consignee";
			public const string Client = "Client";
			public const string Warehouse = "Warehouse";
		}

		#endregion

		#region Validator

		SequenceValidator Validator => validator ??= new SequenceValidator(GetPropertyInfosToValidate(), mustHaveAtLeastOneNonZeroValue: true);

		SequenceValidator validator;

		ZPropertyInfo[] GetPropertyInfosToValidate()
		{
			return new []
			{
				ProductInfo,
				CarrierInfo,
				ConsigneeInfo,
				ClientInfo,
				WarehouseInfo
			};
		}

		#endregion

		#region Properties

		#region Product

		public ZByte Product
		{
			get { return product; }
			set
			{
				SetNonPersistentPropertyValue(ProductInfo, ref product, value);
				if (!IsValidationSuspended)
				{
					ValidateProduct();
					ProductInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ProductInfo
		{
			get { return GetZPropertyInfo(Schema.Product); }
		}

		void ValidateProduct()
		{
			ProductInfo.ClearAllNotifications();
			Validator.Validate(ProductInfo);
		}

		ZByte product;

		#endregion

		#region Carrier

		public ZByte Carrier
		{
			get { return carrier; }
			set
			{
				SetNonPersistentPropertyValue(CarrierInfo, ref carrier, value);
				if (!IsValidationSuspended)
				{
					ValidateCarrier();
					CarrierInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo CarrierInfo
		{
			get { return GetZPropertyInfo(Schema.Carrier); }
		}

		void ValidateCarrier()
		{
			CarrierInfo.ClearAllNotifications();
			Validator.Validate(CarrierInfo);
		}

		ZByte carrier;

		#endregion

		#region Consignee

		public ZByte Consignee
		{
			get { return consignee; }
			set
			{
				SetNonPersistentPropertyValue(ConsigneeInfo, ref consignee, value);
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

		void ValidateConsignee()
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
				SetNonPersistentPropertyValue(ClientInfo, ref client, value);
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

		void ValidateClient()
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
				SetNonPersistentPropertyValue(WarehouseInfo, ref warehouse, value);
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

		void ValidateWarehouse()
		{
			WarehouseInfo.ClearAllNotifications();
			Validator.Validate(WarehouseInfo);
		}

		ZByte warehouse;

		#endregion

		#endregion

		#region GetClone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CartonGroupSequence();
		}

		#endregion

		#region SetCustomDefaultValuesCore

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			Product = 1;
			Carrier = 2;
			Consignee = 3;
			Client = 4;
			Warehouse = 5;
		}

		#endregion

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Product = new ZByte(reader.ReadElementString(Schema.Product));
			Carrier = new ZByte(reader.ReadElementString(Schema.Carrier));
			Consignee = new ZByte(reader.ReadElementString(Schema.Consignee));
			Client = new ZByte(reader.ReadElementString(Schema.Client));
			Warehouse = new ZByte(reader.ReadElementString(Schema.Warehouse));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Product, Product.ToString());
			writer.WriteElementString(Schema.Carrier, Carrier.ToString());
			writer.WriteElementString(Schema.Consignee, Consignee.ToString());
			writer.WriteElementString(Schema.Client, Client.ToString());
			writer.WriteElementString(Schema.Warehouse, Warehouse.ToString());
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateProduct();
			ValidateCarrier();
			ValidateConsignee();
			ValidateClient();
			ValidateWarehouse();
		}
	}
}
