using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	#region ICodeDescriptionBool Interface

	public interface ICodeDescriptionBool : ICodeDescription<ZBool>
	{
		ZBool Bool { get; }
	}

	#endregion

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBool : CodeDescription<ZBool>, ICodeDescriptionBool
	{
		#region Schema

		protected new abstract class Schema : CodeDescription<ZBool>.Schema
		{
			public const string Bool = "Bool";
		}

		#endregion

		public CodeDescriptionBool()
		{
		}

		public CodeDescriptionBool(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public CodeDescriptionBool(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CodeDescriptionBool();
		}

		#endregion

		#region Bool

		public virtual ZBool Bool
		{
			get => Value;
			set => Value = value;
		}

		public virtual ZPropertyInfo BoolInfo
		{
			get => GetWrappedZPropertyInfo(nameof(Bool), x => ValueInfo);
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			if (ReadOrWriteBoolInXml)
			{
				Bool = new ZBool(reader.ReadElementString(Schema.Bool));
			}

			base.ReadMoreElements(reader);
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			if (ReadOrWriteBoolInXml)
			{
				writer.WriteElementString(Schema.Bool, Bool.ToString());
			}

			base.WriteMoreElements(writer);
		}

		protected virtual bool ReadOrWriteBoolInXml => true;
		protected override bool ReadOrWriteValueInXml => false;
		protected override ZBool ValueFromString(string value)
		{
			return new ZBool(value);
		}

		#endregion
	}
}
