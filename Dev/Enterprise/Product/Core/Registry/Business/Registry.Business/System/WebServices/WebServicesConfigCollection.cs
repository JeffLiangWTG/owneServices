using System.Xml.Serialization;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WebServicesConfigCollection : RegistryBusinessObjectCollectionTemplate<WebServicesConfig>
	{
		public WebServicesConfigCollection()
		: base(null, null)
		{
		}

		public WebServicesConfigCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public WebServicesConfig AddNew(string name)
		{
			var mapping = AddNew();
			mapping.Name = name;

			return mapping;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WebServicesConfig();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WebServicesConfigCollection();
		}

		public static WebServicesConfigCollection DefaultValue
		{
			get
			{
				var defaultValue = new WebServicesConfigCollection();
				foreach (var service in WebServiceRegistry.All)
				{
					defaultValue.AddNew((NoResString)service);
				}

				return defaultValue;
			}
		}
	}
}
