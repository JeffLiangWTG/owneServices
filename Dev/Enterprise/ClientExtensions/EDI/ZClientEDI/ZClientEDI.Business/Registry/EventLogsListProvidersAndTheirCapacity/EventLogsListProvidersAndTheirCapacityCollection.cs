using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class EventLogsListProvidersAndTheirCapacityCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new EventLogsListProvidersAndTheirCapacity this[int index]
		{
			get { return (EventLogsListProvidersAndTheirCapacity)Elements[index]; }
		}

		public IEnumerable<EventLogsListProvidersAndTheirCapacity> Collection
		{
			get { return from EventLogsListProvidersAndTheirCapacity collection in this select collection; }
		}

		#region New

		public new EventLogsListProvidersAndTheirCapacity AddNew()
		{
			return (EventLogsListProvidersAndTheirCapacity)base.AddNew();
		}

		EventLogsListProvidersAndTheirCapacity AddNew(ZString provider, ZInt level)
		{
			var result = AddNew();
			using (result.GetValidationSuspender())
			{
				result.ProviderCode = provider.Trim();
				result.LevelCode = level;
			}

			return result;
		}

		public EventLogsListProvidersAndTheirCapacity AddNewOrGetExisting(ZString provider, ZInt level)
		{
			provider = provider.Trim();
			var existingProviderAndLevel = (from EventLogsListProvidersAndTheirCapacity registryItem in this
											where
												registryItem.ProviderCode == provider &&
												registryItem.LevelCode == level
											select registryItem).FirstOrDefault();

			return existingProviderAndLevel ?? AddNew(provider, level);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EventLogsListProvidersAndTheirCapacity();
		}

		#endregion

		#region GetProviders

		public EventLogsListProvidersAndTheirCapacity GetProvider(ZString provider, ZInt level)
		{
			return
				(from EventLogsListProvidersAndTheirCapacity collection in this
				where
					collection.ProviderCode == provider &&
					collection.LevelCode == level
				select collection).FirstOrDefault();
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EventLogsListProvidersAndTheirCapacityCollection();
		}

		#endregion
	}
}

