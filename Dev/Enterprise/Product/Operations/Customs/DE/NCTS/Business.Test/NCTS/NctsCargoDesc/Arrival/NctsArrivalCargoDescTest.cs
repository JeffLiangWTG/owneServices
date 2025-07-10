using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsArrivalCargoDesc))]
	sealed class NctsArrivalCargoDescTest : NctsArrivalCargoDescAbstractTest<NctsHeader>
	{
		protected override ZString CountryCode => Core.Constants.CountryCodes.Germany;

		public void TestPackages()
		{
			AssertType<NctsPackageCollection>(arrivalCargoDesc.Packages);
		}

		public void TestSupportingDocuments()
		{
			AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>(arrivalCargoDesc.SupportingDocuments);
		}

		public void TestSupportingDocuments_ReadOnly()
		{
			AssertEquals(true, arrivalCargoDesc.SupportingDocuments.ReadOnly);
		}

		public void TestAdditionalInfos()
		{
			AssertType<NctsAdditionalInfoCollection<NctsAdditionalInfo>>(arrivalCargoDesc.AdditionalInfos);
		}

		public void TestAdditionalInfos_ReadOnly()
		{
			AssertEquals(true, arrivalCargoDesc.AdditionalInfos.ReadOnly);
		}

		public new void TestCorrectTypeDecideForLoad()
		{
			Assert("This test is temporarily disabled because NctsArrivalCargoDesc is not connected to NctsArrivalMovementHeader", true);
		}

		public void TestIsUnloadedCommodityCodeRequired()
		{
			CombineAssertions(() =>
			{
				arrivalCargoDesc.BY_HarmonisedTariff = ZString.Empty;
				AssertEquals("BY_HarmonisedTariff is empty", expected: false, arrivalCargoDesc.IsUnloadedCommodityCodeRequired);
				arrivalCargoDesc.BY_FormattedHarmonisedTariff = "123456";
				AssertEquals("BY_HarmonisedTariff isn't empty", expected: true, arrivalCargoDesc.IsUnloadedCommodityCodeRequired);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => arrivalCargoDesc;

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
			arrivalCargoDesc.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections, Array.Empty<PropertyDescriptor>());
			return arrivalCargoDesc;
		}

		protected override void SetUp()
		{
			base.SetUp();
			arrivalCargoDesc = Factory.New<NctsArrivalCargoDesc>();
		}
		NctsArrivalCargoDesc arrivalCargoDesc;
	}
}
