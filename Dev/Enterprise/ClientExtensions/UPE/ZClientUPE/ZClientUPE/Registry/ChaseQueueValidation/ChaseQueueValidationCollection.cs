using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	[XmlSerializerAssembly("ZClientUPE.XmlSerializers")]
	public class ChaseQueueValidationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new ChaseQueueValidation this[int i]
		{
			get { return (ChaseQueueValidation)Elements[i]; }
		}

		public new ChaseQueueValidation AddNew()
		{
			return (ChaseQueueValidation)base.AddNew();
		}

		public bool IsDuplicateItem(ChaseQueueValidation itemToCheck)
		{
			bool result = false;
			foreach (ChaseQueueValidation item in this)
			{
				if (item != itemToCheck && item.DayOfTheWeek == itemToCheck.DayOfTheWeek)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ChaseQueueValidation();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChaseQueueValidationCollection();
		}
	}
}
