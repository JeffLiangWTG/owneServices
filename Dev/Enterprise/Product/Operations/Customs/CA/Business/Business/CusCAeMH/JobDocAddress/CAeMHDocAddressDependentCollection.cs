using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class CAeMHDocAddressDependentCollection : JobDocAddressDependentCollection
	{
		public CAeMHDocAddressDependentCollection(IDocAddresses parent)
			: base(parent)
		{
		}

		public new CAeMHDocAddress this[int index]
		{
			get { return (CAeMHDocAddress)base[index]; }
		}

		public new CAeMHDocAddress AddNew()
		{
			var newDocAddress = (CAeMHDocAddress)base.AddNew();
			newDocAddress.OverrideRequirement = new CAeMHDocAddressRequirement(Factory);
			return newDocAddress;
		}

		public new CAeMHDocAddress AddNew(DocAddressType docAddressType)
		{
			var newDocAddress = (CAeMHDocAddress)base.AddNew(docAddressType);
			newDocAddress.OverrideRequirement = new CAeMHDocAddressRequirement(Factory, docAddressType);
			return newDocAddress;
		}

		public new CAeMHDocAddress AddNew(OrgAddress orgAddress)
		{
			return (CAeMHDocAddress)base.AddNew(orgAddress);
		}

		public new CAeMHDocAddress AddNew(DocAddressType docAddressType, int sequence)
		{
			var newDocAddress = (CAeMHDocAddress)base.AddNew(docAddressType, sequence);
			newDocAddress.OverrideRequirement = new CAeMHDocAddressRequirement(Factory, docAddressType);
			return newDocAddress;
		}

		public new CAeMHDocAddress AddNew(OrgAddress orgAddress, DocAddressType docAddressType)
		{
			var newDocAddress = (CAeMHDocAddress)base.AddNew(orgAddress, docAddressType);
			newDocAddress.OverrideRequirement = new CAeMHDocAddressRequirement(Factory, docAddressType);
			return newDocAddress;
		}

		public new CAeMHDocAddress CreateWithAddressType(DocAddressType docAddressType)
		{
			var newDocAddress = (CAeMHDocAddress)base.AddNew(docAddressType);
			newDocAddress.OverrideRequirement = new CAeMHDocAddressRequirement(Factory, docAddressType);
			return newDocAddress;
		}

		public new CAeMHDocAddress CreateWithRequirement(JobDocAddressRequirement requirement)
		{
			return (CAeMHDocAddress)base.CreateWithRequirement(requirement);
		}

		public new CAeMHDocAddress FindByDocAddressType(DocAddressType docAddressType)
		{
			var newDocAddress = (CAeMHDocAddress)base.FindByDocAddressType(docAddressType);
			if (newDocAddress != null)
			{
				newDocAddress.OverrideRequirement = new CAeMHDocAddressRequirement(Factory, docAddressType);
			}
			return newDocAddress;
		}

		public new CAeMHDocAddress FindByDocAddressType(DocAddressType docAddressType, int sequence)
		{
			var newDocAddress = (CAeMHDocAddress)base.FindByDocAddressType(docAddressType, sequence);
			if (newDocAddress != null)
			{
				newDocAddress.OverrideRequirement = new CAeMHDocAddressRequirement(Factory, docAddressType);
			}
			return newDocAddress;
		}

		public new CAeMHDocAddress[] FindDocAddressesByType(DocAddressType docAddressType)
		{
			return new List<CAeMHDocAddress>(new TypedEnumerable<CAeMHDocAddress>(base.FindDocAddressesByType(docAddressType))).ToArray();
		}

		public CAeMHDocAddress[] FindDocAddressesByType(DocAddressType docAddressType, CAeMHDocAddress ignoreDocAddress)
		{
			return new List<CAeMHDocAddress>(new TypedEnumerable<CAeMHDocAddress>(base.FindDocAddressesByType(docAddressType, ignoreDocAddress))).ToArray();
		}

		public new CAeMHDocAddress FindOrCreateDummyAddress(int sequence)
		{
			return (CAeMHDocAddress)base.FindOrCreateDummyAddress(sequence);
		}

		public new CAeMHDocAddress FindOrCreateWithDocAddressType(DocAddressType docAddressType)
		{
			var newDocAddress = (CAeMHDocAddress)base.FindOrCreateWithDocAddressType(docAddressType);
			if (newDocAddress != null)
			{
				newDocAddress.OverrideRequirement = new CAeMHDocAddressRequirement(Factory, docAddressType);
			}
			return newDocAddress;
		}

		public new CAeMHDocAddress FindOrCreateWithDocAddressType(ZGuid orgAddressPK, DocAddressType docAddressType)
		{
			var newDocAddress = (CAeMHDocAddress)base.FindOrCreateWithDocAddressType(orgAddressPK, docAddressType);
			if (newDocAddress != null)
			{
				newDocAddress.OverrideRequirement = new CAeMHDocAddressRequirement(Factory, docAddressType);
			}
			return newDocAddress;
		}

		public new CAeMHDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement)
		{
			return (CAeMHDocAddress)base.FindOrCreateWithRequirement(requirement);
		}

		public new CAeMHDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement, int sequence)
		{
			return (CAeMHDocAddress)base.FindOrCreateWithRequirement(requirement, sequence);
		}

		public CAeMHDocAddress Find(ZString addressType)
		{
			return this.Cast<CAeMHDocAddress>().FirstOrDefault(x => x.E2_AddressType == addressType);
		}

		public void RemoveAndDelete(ZString addressType)
		{
			var docAddress = this.Cast<CAeMHDocAddress>().FirstOrDefault(x => x.E2_AddressType == addressType);
			if (docAddress != null)
			{
				this.RemoveAndDelete(docAddress);
			}
		}

		protected override bool AllowNewCore => true;
	}
}
