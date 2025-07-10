using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot("CodeDescriptionBoolWithExtraBool")]
	public class CodeDescriptionBoolWithExtraBool : CodeDescriptionBool
	{
		public CodeDescriptionBoolWithExtraBool() : base()
		{
		}

		#region Schema

		protected new abstract class Schema : CodeDescriptionBool.Schema
		{
			public const string Bool2 = "Bool2";
		}

		#endregion

		#region Bool2

		ZBool fBool2;

		public virtual ZBool Bool2
		{
			get { return fBool2; }
			set
			{
				if (fBool2 != value)
				{
					SetNonPersistentPropertyValue<ZBool>(Bool2Info, ref fBool2, value);
				}
			}
		}

		public ZPropertyInfo Bool2Info
		{
			get { return GetZPropertyInfo(nameof(Bool2)); }
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CodeDescriptionBoolWithExtraBool();
		}

		#endregion

		#region Implementation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			Bool2 = new ZBool(reader.ReadElementString(Schema.Bool2));
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.Bool2, Bool2.ToString());
		}

		protected override bool UseDefaultSystemDefinedHandlingForXml
		{
			get { return false; }
		}

		#endregion
	}
}
