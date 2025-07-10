using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.SWL.Business
{
	[XmlSerializerAssembly("ZClientSWL.XmlSerializers")]
	[XmlRoot("Charges")]
	public class ShipnetChargeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ShipnetChargeCollection()
		{
		}

		public ShipnetChargeCollection(ShipnetSetupBusinessObject parent, BusinessObjectFactory factory)
			: base(factory)
		{
			this.Parent = parent;
		}

		public new ShipnetCharge this[int index]
		{
			get { return (ShipnetCharge)(Elements[index]); }
		}

		public override void Add(BusinessObject businessObject)
		{
			ShipnetCharge charge = businessObject as ShipnetCharge;
			if (charge != null)
			{
				charge.Parent = Parent;
			}
			base.Add(businessObject);
		}

		public new ShipnetCharge AddNew()
		{
			return (ShipnetCharge)base.AddNew();
		}

		public ShipnetCharge AddNew(ZGuid chargePK)
		{
			ShipnetCharge result = AddNew();
			result.ChargePK = chargePK;
			return result;
		}

		public bool IsDuplicateCharge(ShipnetCharge chargeToCheck)
		{
			bool result = false;

			foreach (ShipnetCharge charge in this)
			{
				if (charge != chargeToCheck && charge.ChargePK == chargeToCheck.ChargePK)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public bool ContainsChargePK(ZGuid chargePK)
		{
			bool result = false;

			foreach (ShipnetCharge charge in this)
			{
				if (charge.ChargePK == chargePK)
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
			return new ShipnetCharge(Parent, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ShipnetChargeCollection(Parent, factory);
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
