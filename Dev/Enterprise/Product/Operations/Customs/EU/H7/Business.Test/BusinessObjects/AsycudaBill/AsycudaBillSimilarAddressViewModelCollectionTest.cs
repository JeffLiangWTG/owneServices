using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AsycudaBillSimilarAddressViewModelCollection))]
	sealed class AsycudaBillSimilarAddressViewModelCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AsycudaBillSimilarAddressViewModelCollection>
	{
		public void TestBuildCollectionSuccessfully()
		{
			var bill1 = Factory.New<AsycudaBill>();
			bill1.ABL_BillNumber = "BN001";
			var bill2 = Factory.New<AsycudaBill>();
			bill2.ABL_BillNumber = "BN002";
			var bill3 = Factory.New<AsycudaBill>();
			bill3.ABL_BillNumber = "BN003";
			bill3.ABL_OA_Consignee = Factory.New<OrgAddress>().PK;
			bill3.ABL_OA_Shipper = Factory.New<OrgAddress>().PK;

			var viewModels = new AsycudaBillSimilarAddressViewModelCollection(new List<AsycudaBill> { bill1, bill2, bill3 });
			AssertEquals(4, viewModels.Count);
			var bill1ViewModelAddressTypes = viewModels.Where(w => w.BillNumber == "BN001").Select(v => v.AddressType);
			var bill2ViewModelAddressTypes = viewModels.Where(w => w.BillNumber == "BN002").Select(v => v.AddressType);
			CombineAssertions(() =>
			{
				Assert(bill1ViewModelAddressTypes.Contains("Importer"));
				Assert(bill1ViewModelAddressTypes.Contains("Exporter"));
				Assert(bill2ViewModelAddressTypes.Contains("Importer"));
				Assert(bill2ViewModelAddressTypes.Contains("Exporter"));
			});
		}

		public void TestShouldLinkBillAddressesOnlyWhenLinkBillAddressesIsCalled()
		{
			var bill1 = Factory.New<AsycudaBill>();
			var bill2 = Factory.New<AsycudaBill>();
			var consigneeOrgAddress = Factory.New<OrgAddress>();

			var viewModels = new AsycudaBillSimilarAddressViewModelCollection(new List<AsycudaBill> { bill1, bill2 });
			Assert(!viewModels.AnyBillLinkedWithOrg);

			var bill1ConsigneeViewModel = viewModels.Where(w => w.AddressType == "Importer").FirstOrDefault();
			bill1ConsigneeViewModel.SetLinkingAddress(consigneeOrgAddress.PK);
			viewModels.Remove(bill1ConsigneeViewModel);
			AssertEquals(Guid.Empty, bill1.ABL_OA_Consignee);

			viewModels.ApplyLinkBillAddresses();
			CombineAssertions(() =>
			{
				AssertEquals(consigneeOrgAddress.PK, bill1.ABL_OA_Consignee);
				Assert(viewModels.AnyBillLinkedWithOrg);
			});
		}

		protected override AsycudaBillSimilarAddressViewModelCollection GetCollectionToTest()
		{
			return new AsycudaBillSimilarAddressViewModelCollection(new List<AsycudaBill> { Factory.NewWithValidTestData<AsycudaBill>() });
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AsycudaBillSimilarAddressViewModel(Factory.NewWithValidTestData<AsycudaBill>(), ManifestBase.AsycudaBillAddress.AddressType.Consignee);
		}
	}
}
