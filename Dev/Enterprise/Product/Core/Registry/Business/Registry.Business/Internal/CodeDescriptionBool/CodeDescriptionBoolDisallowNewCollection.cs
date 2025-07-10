using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolDisallowNewCollection : CodeDescriptionBoolCollection
	{
		public CodeDescriptionBoolDisallowNewCollection()
			: base()
		{
		}

		public CodeDescriptionBoolDisallowNewCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public CodeDescriptionBoolDisallowNewCollection(int codeMaxLength)
			: base(codeMaxLength)
		{
		}

		public CodeDescriptionBoolDisallowNewCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list, false, 0)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		public new CodeDescriptionBoolDisallowNew AddNew()
		{
			return (CodeDescriptionBoolDisallowNew)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CodeDescriptionBoolDisallowNew();
		}

		public new CodeDescriptionBoolDisallowNew this[int i]
		{
			get { return (CodeDescriptionBoolDisallowNew)base[i]; }
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new CodeDescriptionBoolDisallowNewCollection(CurrentFallbackLevel);
		}
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolDisallowNew : CodeDescriptionBool
	{
		protected virtual bool BoolReadOnly
		{
			get { return SystemDefined; }
		}

		[ReadOnlyMember(nameof(BoolReadOnly))]
		public override ZBool Bool
		{
			get { return base.Bool; }
			set { base.Bool = value; }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CodeDescriptionBoolDisallowNew();
		}
	}
}
