using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionBoolWithSingleTrueCollection : CodeDescriptionBoolCollection
	{
		public CodeDescriptionBoolWithSingleTrueCollection()
		{
		}

		public CodeDescriptionBoolWithSingleTrueCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new CodeDescriptionBoolWithSingleTrue this[int i]
		{
			get
			{
				return (CodeDescriptionBoolWithSingleTrue)base[i];
			}
		}

		public new CodeDescriptionBoolWithSingleTrue AddNew()
		{
			return (CodeDescriptionBoolWithSingleTrue)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CodeDescriptionBoolWithSingleTrue();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new CodeDescriptionBoolWithSingleTrueCollection(CurrentFallbackLevel);
		}
	}
}
