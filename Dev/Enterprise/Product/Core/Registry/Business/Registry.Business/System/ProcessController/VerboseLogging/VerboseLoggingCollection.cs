using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class VerboseLoggingCollection : CodeDescriptionCollection<VerboseLoggingBusinessObject, ZDateTime>
	{
		public VerboseLoggingCollection()
			: this(null, null, default)
		{
		}

		public VerboseLoggingCollection(
			FallbackLevel fallbackLevel,
			ReadOnlyCodeDescriptionPairList list,
			ZDateTime defaultValueForNewChild)
			: base(fallbackLevel, null, list, defaultValueForNewChild, 4)
		{
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new VerboseLoggingCollection(fallbackLevel, null, DefaultValueForNewChild);
		}

		public bool VerboseLoggingByCode(string code)
		{
			return (FindByCode(code) as VerboseLoggingBusinessObject)
				?.VerboseLogging ?? false;
		}
	}
}
