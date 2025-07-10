using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.EU.Business.XmlSerializers")]
	public class NctsDefaultConsignorConsignee : AutoNctsDefaultConsignorConsignee
	{
		public NctsDefaultConsignorConsignee() : base()
		{
		}

		public NctsDefaultConsignorConsignee(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public override ZBool LeaveBlank
		{
			get => base.LeaveBlank;
			set
			{
				base.LeaveBlank = value;
				if (value && base.ValueFrom)
				{
					base.ValueFrom = false;
				}
			}
		}

		public override ZBool ValueFrom
		{
			get => base.ValueFrom;
			set
			{
				base.ValueFrom = value;
				if (value)
				{
					base.Consignor = true;
					base.Consignee = true;
					if (base.LeaveBlank)
					{
						base.LeaveBlank = false;
					}
				}
				else
				{
					base.Consignor = false;
					base.Consignee = false;
				}
			}
		}

		public override ZBool Consignor
		{
			get => base.Consignor;
			set
			{
				base.Consignor = value;
				if (!value && !base.Consignee)
				{
					base.ValueFrom = false;
				}
			}
		}

		public override ZBool Consignee
		{
			get => base.Consignee;
			set
			{
				base.Consignee = value;
				if (!value && !base.Consignor)
				{
					base.ValueFrom = false;
				}
			}
		}

		protected override bool Consignor_ReadOnly => !ValueFrom;
		protected override bool Consignee_ReadOnly => !ValueFrom;

		#region Cloning, XML Reading and Writing

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new NctsDefaultConsignorConsignee(fallbackLevel, factory);

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			LeaveBlank = reader.ReadElementStringAsZBool(Schema.LeaveBlank);
			ValueFrom = reader.ReadElementStringAsZBool(Schema.ValueFrom);
			Consignor = reader.ReadElementStringAsZBool(Schema.Consignor);
			Consignee = reader.ReadElementStringAsZBool(Schema.Consignee);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.LeaveBlank, LeaveBlank.ToString());
			writer.WriteElementString(Schema.ValueFrom, ValueFrom.ToString());
			writer.WriteElementString(Schema.Consignor, Consignor.ToString());
			writer.WriteElementString(Schema.Consignee, Consignee.ToString());
		}

		#endregion
	}
}
