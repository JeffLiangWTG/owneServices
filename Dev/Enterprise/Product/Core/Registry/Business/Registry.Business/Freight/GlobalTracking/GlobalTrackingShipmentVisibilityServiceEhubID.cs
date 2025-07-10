using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class GlobalTrackingShipmentVisibilityServiceEhubID : RegistryBusinessObject, ICanDelete
	{
		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string EhubID = "EhubID";
			public const string Service = "Service";
		}

		public GlobalTrackingShipmentVisibilityServiceEhubID()
		{
		}

		public GlobalTrackingShipmentVisibilityServiceEhubID(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		#region Property

		public ZString EhubID
		{
			get { return ehubID; }
			set
			{
				SetNonPersistentPropertyValue(EhubIDInfo, ref ehubID, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateEhubID();
				}
			}
		}

		public ZPropertyInfo EhubIDInfo
		{
			get { return GetZPropertyInfo(Schema.EhubID); }
		}
		ZString ehubID;

		public ZString Service
		{
			get { return service; }
			set
			{
				SetNonPersistentPropertyValue(ServiceInfo, ref service, value);
			}
		}

		public ZPropertyInfo ServiceInfo
		{
			get { return GetZPropertyInfo(Schema.Service); }
		}
		ZString service;

		#endregion

		#region Read Only Members

		public bool Code_ReadOnly
		{
			get { return true; }
		}

		protected override int CodeMaxLengthDefaultValue
		{
			get { return 4; }
		}

		public bool Service_ReadOnly
		{
			get { return true; }
		}

		public bool EnglishService_ReadOnly
		{
			get { return true; }
		}
		#endregion

		#region XML Serialisation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var options = new GlobalTrackingShipmentVisibilityServiceEhubID();
			options.EhubID = EhubID;
			options.Service = Service;
			return options;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EhubID, EhubID.ToString());
			writer.WriteElementString(Schema.Service, Service.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			EhubID = reader.ReadElementString(Schema.EhubID);
			Service = reader.ReadElementString(Schema.Service);
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete => false;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("70450416-c442-4d42-ae01-ffd44c4f2ea8", "This service cannot be deleted.");
		#endregion

		#region Validation

		public GlobalTrackingShipmentVisibilityServiceEhubIDValidation Validation => validation ?? (validation = GetNewValidation());
		GlobalTrackingShipmentVisibilityServiceEhubIDValidation validation;

		protected virtual GlobalTrackingShipmentVisibilityServiceEhubIDValidation GetNewValidation() => new GlobalTrackingShipmentVisibilityServiceEhubIDValidation(this);

		#endregion
	}
}
