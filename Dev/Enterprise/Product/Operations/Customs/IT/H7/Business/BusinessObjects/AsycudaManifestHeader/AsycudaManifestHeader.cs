using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.H7.Business;

public class AsycudaManifestHeader : EU.H7.Business.AsycudaManifestHeader, Integration.Customs.ITH7.IAsycudaManifestHeader
{
	public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Italy;

	public new EU.H7.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (EU.H7.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;

	public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;

	protected override EU.H7.Business.IAsycudaBillCollection<EU.H7.Business.AsycudaBill, EU.H7.Business.AsycudaManifestHeader> CreateNewEUH7AsycudaBillCollection() => new EU.H7.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);

	protected override Type GetBillTypeCore() => typeof(AsycudaBill);

	protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

	public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

	public new H7ApplicationBusinessProvider ApplicationBusinessProvider => (H7ApplicationBusinessProvider)base.ApplicationBusinessProvider;

	[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.SubmitTypeList))]
	public override ZString AMA_ApplicationCode
	{
		get => base.AMA_ApplicationCode;
		set => base.AMA_ApplicationCode = value;
	}

	[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.MethodOfPaymentList))]
	public override ZString AMA_PaymentMethod
	{
		get => base.AMA_PaymentMethod;
		set => base.AMA_PaymentMethod = value;
	}
}
