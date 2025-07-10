using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection : CodeDescriptionBoolCollection
	{
		public CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection()
			: base()
		{
		}

		public CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list, false, 20)
		{
		}

		protected override bool AllowNewCore => false;

		public new CodeDescriptionBoolDisallowNewWithDefaultDisabled AddNew()
		{
			return (CodeDescriptionBoolDisallowNewWithDefaultDisabled)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CodeDescriptionBoolDisallowNewWithDefaultDisabled();
		}

		public new CodeDescriptionBoolDisallowNewWithDefaultDisabled this[int i]
		{
			get { return (CodeDescriptionBoolDisallowNewWithDefaultDisabled)base[i]; }
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(CurrentFallbackLevel);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			SuspendValidation();
			base.SetDefaultsForNewChild(child);
			ResumeValidation();
		}
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolDisallowNewWithDefaultDisabled : CodeDescriptionBool
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CodeDescriptionBoolDisallowNewWithDefaultDisabled();
		}
	}
}
