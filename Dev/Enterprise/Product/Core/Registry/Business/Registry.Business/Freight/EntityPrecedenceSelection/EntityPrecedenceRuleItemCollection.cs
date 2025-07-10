using System.Collections;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EntityPrecedenceRuleItemCollection : RegistryBusinessObjectCollectionTemplate
	{
		public EntityPrecedenceRuleItemCollection()
		{
		}

		public EntityPrecedenceRuleItemCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new EntityPrecedenceRuleItem this[int i]
		{
			get { return (EntityPrecedenceRuleItem)Elements[i]; }
		}

		public new EntityPrecedenceRuleItem AddNew()
		{
			return (EntityPrecedenceRuleItem)base.AddNew();
		}

		public void MoveItem(int fromIndex, int toIndex)
		{
			if (Count < fromIndex || Count < toIndex)
			{
				return;
			}

			var list = (IList)this;
			var itemToMove = list[fromIndex];
			list.RemoveAt(fromIndex);
			list.Insert(toIndex, itemToMove);
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EntityPrecedenceRuleItem(CurrentFallbackLevel);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EntityPrecedenceRuleItemCollection(fallbackLevel);
		}
	}
}
