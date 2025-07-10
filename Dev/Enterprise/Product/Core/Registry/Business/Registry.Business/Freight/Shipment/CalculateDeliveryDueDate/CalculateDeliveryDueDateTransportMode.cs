using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CalculateDeliveryDueDateTransportMode : RegistryBusinessObject, ICanDelete
	{
		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string Enabled = "Enabled";
		}

		public CalculateDeliveryDueDateTransportMode()
		{
		}

		public CalculateDeliveryDueDateTransportMode(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		#region Property

		public ZBool Enabled
		{
			get { return enabled; }
			set
			{
				SetNonPersistentPropertyValue(EnabledInfo, ref enabled, value);
			}
		}

		public ZPropertyInfo EnabledInfo
		{
			get { return GetZPropertyInfo(Schema.Enabled); }
		}
		ZBool enabled;

		bool isSupportUser => User.SupportUserName.Equals(Env.CurrentUser.LoginName, StringComparison.OrdinalIgnoreCase);

		protected bool Enabled_ReadOnly => !isSupportUser;

		#endregion

		#region Read Only Members

		public bool Code_ReadOnly
		{
			get { return true; }
		}

		protected override int CodeMaxLengthDefaultValue
		{
			get { return 3; }
		}

		public bool Description_ReadOnly
		{
			get { return true; }
		}

		public bool EnglishDescription_ReadOnly
		{
			get { return true; }
		}

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}

		#endregion

		#region XML Serialisation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var options = new CalculateDeliveryDueDateTransportMode();
			options.Enabled = Enabled;
			return options;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Enabled, Enabled.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			Enabled = reader.ReadElementStringAsZBool(Schema.Enabled);
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get { return false; }
		}

		#endregion
	}
}
