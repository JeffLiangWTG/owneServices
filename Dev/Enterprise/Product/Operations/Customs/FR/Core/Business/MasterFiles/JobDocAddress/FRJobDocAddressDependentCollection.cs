using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class FRJobDocAddressDependentCollection : JobDocAddressDependentCollection
	{
		public FRJobDocAddressDependentCollection(IDocAddresses parent)
			: base(parent)
		{
		}

		public new FRJobDocAddress this[int index] => (FRJobDocAddress)base[index];

		public new FRJobDocAddress AddNew() => (FRJobDocAddress)base.AddNew();

		public new FRJobDocAddress AddNew(DocAddressType docAddressType) => (FRJobDocAddress)base.AddNew(docAddressType);

		public new FRJobDocAddress AddNew(OrgAddress orgAddress) => (FRJobDocAddress)base.AddNew(orgAddress);

		public new FRJobDocAddress AddNew(DocAddressType docAddressType, int sequence) => (FRJobDocAddress)base.AddNew(docAddressType, sequence);

		public new FRJobDocAddress AddNew(OrgAddress orgAddress, DocAddressType docAddressType) => (FRJobDocAddress)base.AddNew(orgAddress, docAddressType);

		public new FRJobDocAddress CreateWithAddressType(DocAddressType docAddressType) => (FRJobDocAddress)base.AddNew(docAddressType);

		public new FRJobDocAddress CreateWithRequirement(JobDocAddressRequirement requirement) => (FRJobDocAddress)base.CreateWithRequirement(requirement);

		public new FRJobDocAddress FindByDocAddressType(DocAddressType docAddressType) => (FRJobDocAddress)base.FindByDocAddressType(docAddressType);

		public new FRJobDocAddress FindByDocAddressType(DocAddressType docAddressType, int sequence) => (FRJobDocAddress)base.FindByDocAddressType(docAddressType, sequence);

		public new FRJobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType) => new List<FRJobDocAddress>(new TypedEnumerable<FRJobDocAddress>(base.FindDocAddressesByType(docAddressType))).ToArray();

		public FRJobDocAddress[] FindDocAddressesByType(DocAddressType docAddressType, FRJobDocAddress ignoreDocAddress) => new List<FRJobDocAddress>(new TypedEnumerable<FRJobDocAddress>(base.FindDocAddressesByType(docAddressType, ignoreDocAddress))).ToArray();

		public new FRJobDocAddress FindOrCreateDummyAddress(int sequence) => (FRJobDocAddress)base.FindOrCreateDummyAddress(sequence);

		public new FRJobDocAddress FindOrCreateWithDocAddressType(DocAddressType docAddressType) => (FRJobDocAddress)base.FindOrCreateWithDocAddressType(docAddressType);

		public new FRJobDocAddress FindOrCreateWithDocAddressType(ZGuid orgAddressPK, DocAddressType docAddressType) => (FRJobDocAddress)base.FindOrCreateWithDocAddressType(orgAddressPK, docAddressType);

		public new FRJobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement) => (FRJobDocAddress)base.FindOrCreateWithRequirement(requirement);

		public new FRJobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement, int sequence) => (FRJobDocAddress)base.FindOrCreateWithRequirement(requirement, sequence);
	}
}
