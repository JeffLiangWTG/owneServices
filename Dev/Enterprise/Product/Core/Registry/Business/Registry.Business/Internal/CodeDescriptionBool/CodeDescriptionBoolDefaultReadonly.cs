using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolDefaultReadonly : CodeDescriptionBool, IMultilingualDescription
	{
		#region Schema

		protected new abstract class Schema : CodeDescriptionBool.Schema
		{
			public const string DefaultColumnReadonly = "DefaultColumnReadonly";
		}

		#endregion

		#region DefaultColumnReadOnly

		public ZBool DefaultColumnReadOnly
		{
			get { return defaultColumnReadOnly; }
			set { defaultColumnReadOnly = value; }
		}
		ZBool defaultColumnReadOnly;

		#endregion

		#region Bool_ReadOnly

		protected bool Bool_ReadOnly
		{
			get { return DefaultColumnReadOnly; }
		}

		#endregion

		#region IMultilingualDescription Members

		MultilingualString IMultilingualDescription.MultilingualDescription
		{
			get { return Description; }
		}

		#endregion

		#region Overrides

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((CodeDescriptionBoolDefaultReadonly)clone).DefaultColumnReadOnly = DefaultColumnReadOnly;
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			DefaultColumnReadOnly = new ZBool(reader.ReadElementString(Schema.DefaultColumnReadonly));
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.DefaultColumnReadonly, DefaultColumnReadOnly.ToString());
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CodeDescriptionBoolDefaultReadonly();
		}

		#endregion
	}
}
