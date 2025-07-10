using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(AsycudaBillSimilarAddressViewModel))]
	sealed class AsycudaBillSimilarAddressViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestThrowExceptionIfAddressTypeIsNotConsigneeOrShipper()
		{
			var exception = AssertExceptionThrown<ArgumentException>(() => new AsycudaBillSimilarAddressViewModel(Factory.New<AsycudaBill>(), ManifestBase.AsycudaBillAddress.AddressType.Buyer));
			AssertEquals("Only Consignee and Shipper type are supported.", exception.Message);
		}

		public void TestViewModelBuildSuccessfully()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_BillNumber = "BN001";
			bill.ABL_ConsigneeName = "Name";
			bill.ABL_ConsigneeStreet1 = "street1";
			bill.ABL_ConsigneeStreet2 = "street2";
			bill.ABL_ConsigneeCity = "city";
			bill.ABL_ConsigneeState = "state";
			bill.ABL_ConsigneePostcode = "3053";
			bill.ABL_RN_NKConsigneeCountry = "IE";

			var viewModel = new AsycudaBillSimilarAddressViewModel(bill, ManifestBase.AsycudaBillAddress.AddressType.Consignee);

			CombineAssertions(() =>
			{
				AssertEquals("BN001", viewModel.BillNumber);
				AssertEquals("Importer", viewModel.AddressType);
				AssertEquals("Name", viewModel.OrgName);
				AssertEquals("street1", viewModel.Address1);
				AssertEquals("street2", viewModel.Address2);
				AssertEquals("city", viewModel.City);
				AssertEquals("state", viewModel.State);
				AssertEquals("3053", viewModel.Postcode);
				AssertEquals("IE", viewModel.Country);
			});
		}

		public void TestFindSimilarAddressSuccessfully()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "COCONUT ENTERTAINMENT";
			orgHeader.MainAddress.OA_Address1 = "Cardigan Street";
			Factory.Save();

			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeName = "COCONUT ENTERTAINMENT";
			bill.ABL_ConsigneeStreet1 = "Cardigan Street";

			var viewModel = new AsycudaBillSimilarAddressViewModel(bill, ManifestBase.AsycudaBillAddress.AddressType.Consignee);

			AssertEquals(orgHeader.MainAddress.PK, ((OrgPatternMatch)viewModel.SimilarOrgMatches.Single()).OS_OA);
		}

		public void TestBillAddressChangedOnlyWhenLinkAddressIsCalled()
		{
			var bill = Factory.New<AsycudaBill>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "COCONUT ENTERTAINMENT";
			orgHeader.MainAddress.OA_Address1 = "Cardigan Street";

			var viewModel = new AsycudaBillSimilarAddressViewModel(bill, ManifestBase.AsycudaBillAddress.AddressType.Consignee);
			viewModel.SetLinkingAddress(orgHeader.MainAddress.PK);

			AssertEquals(Guid.Empty, bill.ABL_OA_Consignee);

			viewModel.ApplyLinkAddress();
			AssertEquals(orgHeader.MainAddress.PK, bill.ABL_OA_Consignee);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AsycudaBillSimilarAddressViewModel(Factory.NewWithValidTestData<AsycudaBill>(), ManifestBase.AsycudaBillAddress.AddressType.Consignee);
		}
	}
}
