using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2JobDocAddressCollection : JobDocAddressDependentCollection
	{
		public ICS2JobDocAddressCollection(IDocAddresses parent)
		: base(parent)
		{
		}

		public new ICS2JobDocAddress AddNew() => (ICS2JobDocAddress)base.AddNew();

		public new ICS2JobDocAddress this[int index] => (ICS2JobDocAddress)base[index];

		public new ICS2JobDocAddress FindOrCreateWithRequirement(JobDocAddressRequirement requirement) => (ICS2JobDocAddress)base.FindOrCreateWithRequirement(requirement);
	}
}
