using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EntityPrecedenceRule : RegistryBusinessObjectTemplate
	{
		public EntityPrecedenceRule()
		{
		}

		public EntityPrecedenceRule(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		#region Properties

		public EntityPrecedenceRuleItemCollection AvailableItems
		{
			get
			{
				if (availableItems == null)
				{
					availableItems = new EntityPrecedenceRuleItemCollection(CurrentFallbackLevel);
					RegisterEditableChildObject(availableItems);
				}

				return availableItems;
			}
		}
		EntityPrecedenceRuleItemCollection availableItems;

		public EntityPrecedenceRuleItemCollection SelectedItems
		{
			get
			{
				if (selectedItems == null)
				{
					selectedItems = new EntityPrecedenceRuleItemCollection(CurrentFallbackLevel);
					RegisterEditableChildObject(selectedItems);
				}

				return selectedItems;
			}
		}
		EntityPrecedenceRuleItemCollection selectedItems;

		public EntityPrecedenceRuleItemCollection DefaultItems
		{
			get { return defaultItems ?? (defaultItems = new EntityPrecedenceRuleItemCollection()); }
		}
		EntityPrecedenceRuleItemCollection defaultItems;

		#endregion

		#region Clone/Copy

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EntityPrecedenceRule(fallbackLevel);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var entityPrecedenceRule = (EntityPrecedenceRule)clone;
			entityPrecedenceRule.AvailableItems.AddRange(AvailableItems);
			entityPrecedenceRule.SelectedItems.AddRange(SelectedItems);
			entityPrecedenceRule.DefaultItems.AddRange(DefaultItems);
		}

		#endregion

		#region Serialization

		ZXmlSerializer ItemsSerialiser
		{
			get
			{
				return precedenceSelectionSerialiser ??
					   (precedenceSelectionSerialiser = ZXmlSerializer.New(typeof(EntityPrecedenceRuleItemCollection)));
			}
		}
		ZXmlSerializer precedenceSelectionSerialiser;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			ItemsSerialiser.Serialize(writer, SelectedItems);
			ItemsSerialiser.Serialize(writer, DefaultItems);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			selectedItems = (EntityPrecedenceRuleItemCollection)ItemsSerialiser.Deserialize(reader);
			defaultItems = (EntityPrecedenceRuleItemCollection)ItemsSerialiser.Deserialize(reader);

			AvailableItems.AddRange(
				DefaultItems
					.Cast<EntityPrecedenceRuleItem>()
					.Where(x => SelectedItems.Cast<EntityPrecedenceRuleItem>().All(y => y.Code != x.Code))
					.Select(x => new EntityPrecedenceRuleItem { Code = x.Code, Description = x.Description }));
		}

		#endregion

		public void ResetItems()
		{
			AvailableItems.RemoveAndDeleteAll();
			SelectedItems.RemoveAndDeleteAll();

			AvailableItems.AddRange(DefaultItems
				.Cast<EntityPrecedenceRuleItem>()
				.Where(x => !x.Bool)
				.Select(x => new EntityPrecedenceRuleItem { Code = x.Code, Description = x.Description }));

			SelectedItems.AddRange(
				DefaultItems
					.Cast<EntityPrecedenceRuleItem>()
					.Where(x => x.Bool)
					.Select(x => new EntityPrecedenceRuleItem { Code = x.Code, Description = x.Description }));
		}
	}
}
