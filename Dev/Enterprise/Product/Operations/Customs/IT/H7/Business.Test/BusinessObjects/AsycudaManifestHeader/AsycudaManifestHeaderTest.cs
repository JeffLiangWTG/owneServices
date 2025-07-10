using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(AsycudaManifestHeader))]
sealed class AsycudaManifestHeaderTest : EU.H7.Business.Testing.AsycudaManifestHeaderAbstractTest
{
	public void TestGetDefaultCountryCode()
	{
		var manifestHeader = Factory.New<AsycudaManifestHeaderForTest>();
		AssertEquals(Core.Constants.CountryCodes.Italy, manifestHeader.GetDefaultCountryCode());
	}

	public void TestBills()
	{
		var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
		AssertType<EU.H7.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(manifestHeader.Bills);
	}

	public void TestMasterBill()
	{
		var manifestHeader = (AsycudaManifestHeader)GetNewBusinessObject();
		AssertType<AsycudaBill>(manifestHeader.MasterBill);
	}

	public void TestCreateNewEUH7AsycudaBillCollection()
	{
		var manifestHeader = Factory.New<AsycudaManifestHeaderForTest>();
		AssertType<EU.H7.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(manifestHeader.CreateNewEUH7AsycudaBillCollection());
	}

	public void TestGetBillTypeCore()
	{
		var manifestHeader = Factory.New<AsycudaManifestHeaderForTest>();
		AssertEquals(typeof(AsycudaBill), manifestHeader.GetBillTypeCore());
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<AsycudaManifestHeader>();

	sealed class AsycudaManifestHeaderForTest : AsycudaManifestHeader
	{
		public AsycudaManifestHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new ZString GetDefaultCountryCode() => base.GetDefaultCountryCode();

		public new Type GetBillTypeCore() => base.GetBillTypeCore();

		public new EU.H7.Business.IAsycudaBillCollection<EU.H7.Business.AsycudaBill, EU.H7.Business.AsycudaManifestHeader> CreateNewEUH7AsycudaBillCollection() => base.CreateNewEUH7AsycudaBillCollection();
	}
}
