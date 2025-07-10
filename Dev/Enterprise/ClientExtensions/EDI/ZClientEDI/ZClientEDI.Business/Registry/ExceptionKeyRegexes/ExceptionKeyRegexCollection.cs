using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ExceptionKeyRegexCollection : RegistryBusinessObjectCollectionTemplate<ExceptionKeyRegex>
	{
		public ExceptionKeyRegexCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public ExceptionKeyRegexCollection()
			: base(null, null)
		{
		}

		public ExceptionKeyRegex Get(string regex)
		{
			return base.Elements.Cast<ExceptionKeyRegex>().FirstOrDefault(e => e.Regex == regex);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ExceptionKeyRegex(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ExceptionKeyRegexCollection(fallbackLevel, factory);
		}
	}
}

