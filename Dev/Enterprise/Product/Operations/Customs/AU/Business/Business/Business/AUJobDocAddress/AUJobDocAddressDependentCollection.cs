using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUJobDocAddressDependentCollection : JobDocAddressDependentCollection
	{
		public AUJobDocAddressDependentCollection(IDocAddresses parent)
			: base(parent)
		{
		}

		public new AUJobDocAddress this[int index] => (AUJobDocAddress)base[index];

		public new AUJobDocAddress AddNew() => (AUJobDocAddress)base.AddNew();

		public new AUJobDocAddress AddNew(DocAddressType docAddressType) => (AUJobDocAddress)base.AddNew(docAddressType);

		public new AUJobDocAddress AddNew(OrgAddress orgAddress) => (AUJobDocAddress)base.AddNew(orgAddress);

		public new AUJobDocAddress AddNew(DocAddressType docAddressType, int sequence) => (AUJobDocAddress)base.AddNew(docAddressType, sequence);

		public new AUJobDocAddress AddNew(OrgAddress orgAddress, DocAddressType docAddressType) => (AUJobDocAddress)base.AddNew(orgAddress, docAddressType);

		public new AUJobDocAddress CreateWithAddressType(DocAddressType docAddressType) => (AUJobDocAddress)base.AddNew(docAddressType);

		public new AUJobDocAddress CreateWithRequirement(JobDocAddressRequirement requirement) => (AUJobDocAddress)base.CreateWithRequirement(requirement);

		public new AUJobDocAddress FindByDocAddressType(DocAddressType docAddressType) => (AUJobDocAddress)base.FindByDocAddressType(docAddressType);

		public new AUJobDocAddress FindByDocAddressType(DocAddressType docAddressType, int sequence) => (AUJobDocAddress)base.FindByDocAddressType(docAddressType, sequence);

		public new AUJobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType) => new List<AUJobDocAddress>(new TypedEnumerable<AUJobDocAddress>(base.FindDocAddressesByType(docAddressType))).ToArray();

		public AUJobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType, AUJobDocAddress ignoreDocAddress) => new List<AUJobDocAddress>(new TypedEnumerable<AUJobDocAddress>(base.FindDocAddressesByType(docAddressType, ignoreDocAddress))).ToArray();

		public new AUJobDocAddress FindOrCreateDummyAddress(int sequence) => (AUJobDocAddress)base.FindOrCreateDummyAddress(sequence);

		public new AUJobDocAddress FindOrCreateWithDocAddressType(DocAddressType docAddressType) => (AUJobDocAddress)base.FindOrCreateWithDocAddressType(docAddressType);

		public new AUJobDocAddress FindOrCreateWithDocAddressType(ZGuid orgAddressPK, DocAddressType docAddressType) => (AUJobDocAddress)base.FindOrCreateWithDocAddressType(orgAddressPK, docAddressType);

		public new AUJobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement) => (AUJobDocAddress)base.FindOrCreateWithRequirement(requirement);

		public new AUJobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement, int sequence) => (AUJobDocAddress)base.FindOrCreateWithRequirement(requirement, sequence);
	}
}
