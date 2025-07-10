using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ChargeDescriptionOverrideAdaptor : NonPersistentBusinessObject
	{
		public ChargeDescriptionOverrideAdaptor(BusinessObjectFactory factory)
			: base(factory)
		{
			allWrappedCharges = new Dictionary<ZGuid, ChargeWithAppendedDescription>();
		}

		public WrapperCollection<ChargeWithAppendedDescription> SelectedWrappedCharges
		{
			get
			{
				if (selectedWrappedCharges == null)
				{
					selectedWrappedCharges = new WrapperCollection<ChargeWithAppendedDescription>(Factory);
					RegisterEditableChildObject(selectedWrappedCharges);
				}
				return selectedWrappedCharges;
			}
		}
		WrapperCollection<ChargeWithAppendedDescription> selectedWrappedCharges;

		public void ApplyOrCancelChanges(bool apply)
		{
			if (allWrappedCharges != null)
			{
				var removedCharges = new List<ChargeWithAppendedDescription>();

				foreach (ChargeWithAppendedDescription wrappedCharge in SelectedWrappedCharges)
				{
					if (!wrappedCharge.IsDeleted)
					{
						if (apply)
						{
							wrappedCharge.AppendText();
						}
						else
						{
							wrappedCharge.CancelAppendText();
						}
					}
					else
					{
						removedCharges.Add(wrappedCharge);
					}
				}

				foreach (ChargeWithAppendedDescription item in removedCharges)
				{
					allWrappedCharges.Remove(item.PK);
					SelectedWrappedCharges.Remove(item);
				}
			}

			SelectedWrappedCharges.HasChanges = false;
		}

		public void AddToWrappedObjects(IEnumerable<Charge> selectedCharges)
		{
			SelectedWrappedCharges.RemoveAll();
			if (selectedCharges != null)
			{
				foreach (Charge charge in selectedCharges)
				{
					ChargeWithAppendedDescription chargeWithAppendedDescription;
					if (!allWrappedCharges.TryGetValue(charge.PK, out chargeWithAppendedDescription))
					{
						chargeWithAppendedDescription = new ChargeWithAppendedDescription(charge, Factory);
						allWrappedCharges.Add(charge.PK, chargeWithAppendedDescription);
					}
					SelectedWrappedCharges.Add(chargeWithAppendedDescription);
				}
			}
		}

		readonly Dictionary<ZGuid, ChargeWithAppendedDescription> allWrappedCharges;
	}
}