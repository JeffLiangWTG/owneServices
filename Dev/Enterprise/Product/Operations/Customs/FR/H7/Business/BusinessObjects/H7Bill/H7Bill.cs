using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.H7.Business
{
	[DependentBusinessObject(typeof(H7ManifestHeader), nameof(H7ManifestHeader.Bills))]
	public class H7Bill : EU.H7.Business.AsycudaBill
	{
		public H7Bill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new H7ManifestHeader Header => (H7ManifestHeader)base.Header;

		public new H7BillLookups Lookups => (H7BillLookups)base.Lookups;

		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new H7BillLookups(this);

		public new CusGoodsLocation CusGoodsLocation => (CusGoodsLocation)base.CusGoodsLocation;
	}
}
