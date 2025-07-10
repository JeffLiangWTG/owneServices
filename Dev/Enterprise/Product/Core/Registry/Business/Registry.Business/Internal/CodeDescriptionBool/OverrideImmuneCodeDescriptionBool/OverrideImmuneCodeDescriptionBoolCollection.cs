using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OverrideImmuneCodeDescriptionBoolCollection : CodeDescriptionBoolCollection
	{
		public OverrideImmuneCodeDescriptionBoolCollection()
		{
		}

		public OverrideImmuneCodeDescriptionBoolCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new OverrideImmuneCodeDescriptionBool this[int i]
		{
			get
			{
				return (OverrideImmuneCodeDescriptionBool)base[i];
			}
		}

		public new OverrideImmuneCodeDescriptionBool AddNew()
		{
			return (OverrideImmuneCodeDescriptionBool)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OverrideImmuneCodeDescriptionBool();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new OverrideImmuneCodeDescriptionBoolCollection(CurrentFallbackLevel);
		}
	}
}
