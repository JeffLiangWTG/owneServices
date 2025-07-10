using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.H7.Business
{
	public class H7ManifestHeader : EU.H7.Business.AsycudaManifestHeader, Integration.Customs.FRH7.IH7ManifestHeader
	{
		public H7ManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.France;

		public new EU.H7.Business.IAsycudaBillCollection<H7Bill, H7ManifestHeader> Bills => (EU.H7.Business.IAsycudaBillCollection<H7Bill, H7ManifestHeader>)base.Bills;

		public new H7Bill MasterBill => (H7Bill)base.MasterBill;

		protected override EU.H7.Business.IAsycudaBillCollection<EU.H7.Business.AsycudaBill, EU.H7.Business.AsycudaManifestHeader> CreateNewEUH7AsycudaBillCollection() => new EU.H7.Business.AsycudaBillCollection<H7Bill, H7ManifestHeader>(this);

		protected override Type GetBillTypeCore() => typeof(H7Bill);

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new H7ManifestHeaderLookups(this);

		public new H7ManifestHeaderLookups Lookups => (H7ManifestHeaderLookups)base.Lookups;

		public new H7ApplicationBusinessProvider ApplicationBusinessProvider => (H7ApplicationBusinessProvider)base.ApplicationBusinessProvider;
	}
}
