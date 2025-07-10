using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CN.Business
{
	public class CNJobDocAddressDependentCollection : JobDocAddressDependentCollection
	{
		public CNJobDocAddressDependentCollection(IDocAddresses parent)
			: base(parent)
		{
		}

		public new CNJobDocAddress this[int index] => (CNJobDocAddress)base[index];

		public new CNJobDocAddress AddNew() => (CNJobDocAddress)base.AddNew();

		public new CNJobDocAddress AddNew(DocAddressType docAddressType) => (CNJobDocAddress)base.AddNew(docAddressType);

		public new CNJobDocAddress AddNew(OrgAddress orgAddress) => (CNJobDocAddress)base.AddNew(orgAddress);

		public new CNJobDocAddress AddNew(DocAddressType docAddressType, int sequence) => (CNJobDocAddress)base.AddNew(docAddressType, sequence);

		public new CNJobDocAddress AddNew(OrgAddress orgAddress, DocAddressType docAddressType) => (CNJobDocAddress)base.AddNew(orgAddress, docAddressType);

		public new CNJobDocAddress CreateWithAddressType(DocAddressType docAddressType) => (CNJobDocAddress)base.AddNew(docAddressType);

		public new CNJobDocAddress CreateWithRequirement(JobDocAddressRequirement requirement) => (CNJobDocAddress)base.CreateWithRequirement(requirement);

		public new CNJobDocAddress FindByDocAddressType(DocAddressType docAddressType) => (CNJobDocAddress)base.FindByDocAddressType(docAddressType);

		public new CNJobDocAddress FindByDocAddressType(DocAddressType docAddressType, int sequence) => (CNJobDocAddress)base.FindByDocAddressType(docAddressType, sequence);

		public new CNJobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType) => new List<CNJobDocAddress>(new TypedEnumerable<CNJobDocAddress>(base.FindDocAddressesByType(docAddressType))).ToArray();

		public CNJobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType, CNJobDocAddress ignoreDocAddress) => new List<CNJobDocAddress>(new TypedEnumerable<CNJobDocAddress>(base.FindDocAddressesByType(docAddressType, ignoreDocAddress))).ToArray();

		public new CNJobDocAddress FindOrCreateDummyAddress(int sequence) => (CNJobDocAddress)base.FindOrCreateDummyAddress(sequence);

		public new CNJobDocAddress FindOrCreateWithDocAddressType(DocAddressType docAddressType) => (CNJobDocAddress)base.FindOrCreateWithDocAddressType(docAddressType);

		public new CNJobDocAddress FindOrCreateWithDocAddressType(ZGuid orgAddressPK, DocAddressType docAddressType) => (CNJobDocAddress)base.FindOrCreateWithDocAddressType(orgAddressPK, docAddressType);

		public new CNJobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement) => (CNJobDocAddress)base.FindOrCreateWithRequirement(requirement);

		public new CNJobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement, int sequence) => (CNJobDocAddress)base.FindOrCreateWithRequirement(requirement, sequence);
	}
}
