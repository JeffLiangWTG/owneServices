using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	[XmlSerializerAssembly("Enterprise.ClientSharedComponents.XmlSerializers")]
	public class EventRegistryBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new EventRegistryBusinessObject this[int i]
		{
			get { return (EventRegistryBusinessObject)Elements[i]; }
		}

		public new EventRegistryBusinessObject AddNew()
		{
			return (EventRegistryBusinessObject)base.AddNew();
		}

		public bool ContainsCode(ZString code)
		{
			return ContainsCode(code, ZString.Empty);
		}

		public bool ContainsCode(ZString code, ZString reference)
		{
			bool result = false;
			foreach (EventRegistryBusinessObject element in Elements)
			{
				bool checkReference = (reference.IsEmpty) || element.Reference.ToLower() == reference.ToLower();
				if (element.Code == code && checkReference)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public EventRegistryBusinessObject FindByCode(ZString eventCode)
		{
			foreach (EventRegistryBusinessObject registryObject in this)
			{
				if (registryObject.Code == eventCode)
				{
					return registryObject;
				}
			}

			return null;
		}

		public EventRegistryBusinessObject FindByCodeAndReference(ZString eventCode, ZString reference)
		{
			var referenceToMatch = reference.ToUpper();

			return this.Cast<EventRegistryBusinessObject>()
						.Where(x => x.Code == eventCode && (x.Reference.IsEmpty || x.Reference.ToUpper() == referenceToMatch))
						.OrderByDescending(x => x.Reference)
						.FirstOrDefault();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EventRegistryBusinessObjectCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EventRegistryBusinessObject();
		}
	}
}
