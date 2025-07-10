using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.SWL.Business
{
	[XmlSerializerAssembly("ZClientSWL.XmlSerializers")]
	[XmlRootAttribute("ChargeGroups")]
	public class ShipnetChargeGroupCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ShipnetChargeGroupCollection()
		{
		}

		public ShipnetChargeGroupCollection(ShipnetSetupBusinessObject parent, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Parent = parent;
		}

		public new ShipnetChargeGroup this[int index]
		{
			get { return (ShipnetChargeGroup)(Elements[index]); }
		}

		public new ShipnetChargeGroup AddNew()
		{
			return (ShipnetChargeGroup)base.AddNew();
		}

		public override void Add(BusinessObject businessObject)
		{
			ShipnetChargeGroup chargeGroup = businessObject as ShipnetChargeGroup;
			if (chargeGroup != null)
			{
				chargeGroup.Parent = Parent;
			}
			base.Add(businessObject);
		}

		public bool IsDuplicateEntry(ShipnetChargeGroup entryToCheck)
		{
			bool result = false;

			foreach (ShipnetChargeGroup entry in this)
			{
				if (entry != entryToCheck && entry.ChargeGroupCode == entryToCheck.ChargeGroupCode)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		#region Implementation

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ShipnetChargeGroup(Parent, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ShipnetChargeGroupCollection(Parent, factory);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			HasChanges = true; //This is a work-around for the bug causes HasChanges not to get set when removing an entry from this collection
		}

		#endregion

		readonly ShipnetSetupBusinessObject Parent;

		#endregion
	}
}
