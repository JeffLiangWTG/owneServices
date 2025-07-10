using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class GlowOpportunityScopePrioritySequence : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Description = "Description";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new GlowOpportunityScopePrioritySequence();
			clone.Description = Description;

			return clone;
		}

		#endregion

		#region Properties

		public ZString Description
		{
			get { return description; }
			set
			{
				description = value;
				DescriptionInfo.RefreshBinding();
			}
		}

		ZString description;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Description, Description);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Description = reader.ReadElementString(Schema.Description);
		}

		#endregion
	}
}
