using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AviationSecurityTrainingRestriction : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string Enabled = "Enabled";
			public const string ApplyCertificationRestriction = "ApplyCertificationRestriction";
		}

		#endregion

		public ZBool Enabled { get; set; }
		public ZBool ApplyCertificationRestriction { get; set; }

		public AviationSecurityTrainingRestriction()
		{
			base.SetCustomDefaultValuesCore();
		}

		public AviationSecurityTrainingRestriction(bool enabled, bool applyCertificationRestriction = false)
		{
			Enabled = enabled;
			ApplyCertificationRestriction = applyCertificationRestriction;
		}

		#region Override

		protected override void SetCustomDefaultValuesCore()
		{
			Enabled = true;
			ApplyCertificationRestriction = false;
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AviationSecurityTrainingRestriction(Enabled, ApplyCertificationRestriction);
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Enabled, Enabled.ToString());
			writer.WriteElementString(Schema.ApplyCertificationRestriction, ApplyCertificationRestriction.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Enabled = reader.ReadElementStringAsZBool(Schema.Enabled);
			ApplyCertificationRestriction = reader.ReadElementStringAsZBool(Schema.ApplyCertificationRestriction);
		}

		#endregion
	}
}
