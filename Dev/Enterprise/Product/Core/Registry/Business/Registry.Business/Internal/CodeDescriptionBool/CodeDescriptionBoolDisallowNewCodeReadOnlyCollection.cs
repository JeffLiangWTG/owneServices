using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolDisallowNewCodeReadOnlyCollection : CodeDescriptionBoolDisallowNewCollection
	{
		public CodeDescriptionBoolDisallowNewCodeReadOnlyCollection()
			: base()
		{
		}

		public CodeDescriptionBoolDisallowNewCodeReadOnlyCollection(int codeMaxLength = default)
			: base()
		{
			if (codeMaxLength > 0)
			{
				CodeMaxLength = codeMaxLength;
			}
		}

		public CodeDescriptionBoolDisallowNewCodeReadOnlyCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public CodeDescriptionBoolDisallowNewCodeReadOnlyCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list)
		{
		}

		protected override bool AllowNewCore => false;

		public new CodeDescriptionBoolDisallowNewCodeReadOnly AddNew() => (CodeDescriptionBoolDisallowNewCodeReadOnly)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CodeDescriptionBoolDisallowNewCodeReadOnly();

		public new CodeDescriptionBoolDisallowNewCodeReadOnly this[int i] => (CodeDescriptionBoolDisallowNewCodeReadOnly)base[i];

		protected override CodeDescriptionBoolCollection GetNewCollection() => new CodeDescriptionBoolDisallowNewCodeReadOnlyCollection(CurrentFallbackLevel);
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolDisallowNewCodeReadOnly : CodeDescriptionBoolDisallowNew
	{
		protected virtual ZBool CodeReadOnly => IsCodeReadOnly ?? CodeAndDescriptionReadOnly;

		public bool? IsCodeReadOnly { get; set; }

		[ReadOnlyMember(nameof(CodeReadOnly))]
		public override ZString Code { get => base.Code; set => base.Code = value; }

		[ReadOnlyMember(nameof(SystemDefined))]
		public override ZBool Bool { get => base.Bool; set => base.Bool = value; }

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			if (reader.IsStartElement("IsCodeReadOnly") && bool.TryParse(reader.ReadElementString("IsCodeReadOnly"), out var isCodeReadOnly))
			{
				IsCodeReadOnly = isCodeReadOnly;
			}
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			if (IsCodeReadOnly != null)
			{
				writer.WriteElementString("IsCodeReadOnly", IsCodeReadOnly.ToString());
			}
		}

		#endregion

		#region Clone

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			if (CallBaseCopy)
			{
				base.CopyValuesToClone(clone);
			}

			var codeDescriptionBoolDisallowNewCodeReadOnly = (CodeDescriptionBoolDisallowNewCodeReadOnly)clone;
			codeDescriptionBoolDisallowNewCodeReadOnly.IsCodeReadOnly = IsCodeReadOnly;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CodeDescriptionBoolDisallowNewCodeReadOnly();

		#endregion
	}
}
