using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class OrgAddressCollection : Xsd.AutoOrgAddressCollection, ISequencedValueObjectCollection
	{
		public Xsd.OrgAddress AddNew(Xsd.OrgAddressAddressType addressType)
		{
			Xsd.OrgAddress result = AddNew();
			result.AddressType = addressType;
			result.AddressTypeSpecified = true;

			return result;
		}

		public Xsd.OrgAddress AddNew(Xsd.AddressCapabilityAddressType addressType)
		{
			Xsd.OrgAddress result = AddNew();
			AddressCapability capability = result.AddressCapabilities.AddNew();
			capability.AddressType = addressType;
			capability.AddressTypeSpecified = true;
			return result;
		}

		public Xsd.OrgAddress GetOrCreateMainAddress()
		{
			Xsd.OrgAddress result = GetMainAddress();
			if (result == null)
			{
				result = AddNew();
				AddressCapability capability = result.AddressCapabilities.AddNew();
				capability.AddressType = Xsd.AddressCapabilityAddressType.MAIN;
				capability.AddressTypeSpecified = true;
				capability = result.AddressCapabilities.AddNew();
				capability.AddressType = Xsd.AddressCapabilityAddressType.OFC;
				capability.AddressTypeSpecified = true;
			}
			return result;
		}

		public Xsd.OrgAddress GetAddressWithMainCapability()
		{
			Xsd.OrgAddress result = null;
			foreach (Xsd.OrgAddress address in this)
			{
				if (HasMainInCapabilities(address))
				{
					result = address;
				}
			}
			return result;
		}

		public Xsd.OrgAddress GetMainAddress()
		{
			Xsd.OrgAddress result = GetAddressWithMainCapability();
			if (result == null && Count > 0 && !this[0].AddressCapabilityTypeSpecified)
			{
				result = this[0];
			}
			return result;
		}

		protected bool HasMainInCapabilities(Xsd.OrgAddress address)
		{
			bool result = false;
			if (address.AddressType == OrgAddressAddressType.MAIN && address.AddressTypeSpecified)
			{
				result = true;
			}
			foreach (AddressCapability capability in address.AddressCapabilities)
			{
				if (capability.AddressType == Xsd.AddressCapabilityAddressType.MAIN &&
					capability.AddressTypeSpecified)
				{
					result = true;
				}
			}
			return result;
		}

		public Xsd.OrgAddress GetMainOrFirstAddress()
		{
			return GetMainAddress() ?? ((Count == 0) ? null : this[0]);
		}

		protected override void OnInsertComplete(int index, object value)
		{
			base.OnInsertComplete(index, value);
			new XsdSequenceIncrementHelper(this).AssignNewIncrementedSequence((ISequencedValueObject)value);
		}
	}
}
