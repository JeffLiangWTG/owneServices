using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.JP.Business
{
	public class JPJobDocAddressDependentCollection : JobDocAddressDependentCollection
	{
		public JPJobDocAddressDependentCollection(IDocAddresses parent)
			: base(parent)
		{
		}

		public new JPJobDocAddress this[int index]
		{
			get { return (JPJobDocAddress)base[index]; }
		}

		public new JPJobDocAddress AddNew()
		{
			return (JPJobDocAddress)base.AddNew();
		}

		public new JPJobDocAddress AddNew(DocAddressType docAddressType)
		{
			return (JPJobDocAddress)base.AddNew(docAddressType);
		}

		public new JPJobDocAddress AddNew(OrgAddress orgAddress)
		{
			return (JPJobDocAddress)base.AddNew(orgAddress);
		}

		public new JPJobDocAddress AddNew(DocAddressType docAddressType, int sequence)
		{
			return (JPJobDocAddress)base.AddNew(docAddressType, sequence);
		}

		public new JPJobDocAddress AddNew(OrgAddress orgAddress, DocAddressType docAddressType)
		{
			return (JPJobDocAddress)base.AddNew(orgAddress, docAddressType);
		}

		public new JPJobDocAddress CreateWithAddressType(DocAddressType docAddressType)
		{
			return (JPJobDocAddress)base.AddNew(docAddressType);
		}

		public new JPJobDocAddress CreateWithRequirement(JobDocAddressRequirement requirement)
		{
			return (JPJobDocAddress)base.CreateWithRequirement(requirement);
		}

		public new JPJobDocAddress FindByDocAddressType(DocAddressType docAddressType)
		{
			return (JPJobDocAddress)base.FindByDocAddressType(docAddressType);
		}

		public new JPJobDocAddress FindByDocAddressType(DocAddressType docAddressType, int sequence)
		{
			return (JPJobDocAddress)base.FindByDocAddressType(docAddressType, sequence);
		}

		public new JPJobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType)
		{
			return new List<JPJobDocAddress>(new TypedEnumerable<JPJobDocAddress>(base.FindDocAddressesByType(docAddressType))).ToArray();
		}

		public JPJobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType, JPJobDocAddress ignoreDocAddress)
		{
			return new List<JPJobDocAddress>(new TypedEnumerable<JPJobDocAddress>(base.FindDocAddressesByType(docAddressType, ignoreDocAddress))).ToArray();
		}

		public new JPJobDocAddress FindOrCreateDummyAddress(int sequence)
		{
			return (JPJobDocAddress)base.FindOrCreateDummyAddress(sequence);
		}

		public new JPJobDocAddress FindOrCreateWithDocAddressType(DocAddressType docAddressType)
		{
			return (JPJobDocAddress)base.FindOrCreateWithDocAddressType(docAddressType);
		}

		public new JPJobDocAddress FindOrCreateWithDocAddressType(ZGuid orgAddressPK, DocAddressType docAddressType)
		{
			return (JPJobDocAddress)base.FindOrCreateWithDocAddressType(orgAddressPK, docAddressType);
		}

		public new JPJobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement)
		{
			return (JPJobDocAddress)base.FindOrCreateWithRequirement(requirement);
		}

		public new JPJobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement, int sequence)
		{
			return (JPJobDocAddress)base.FindOrCreateWithRequirement(requirement, sequence);
		}
	}
}
