using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsBill))]
	sealed class NctsBillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGoodsItems()
		{
			AssertType<NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>(nctsBill.GoodsItems);
		}

		public void TestCusInBondCargoDescType()
		{
			AssertEquals(typeof(NctsDepartureCargoDesc), ((ICusInBondCargoDescTypeProvider)nctsBill).CusInBondCargoDescType);
		}

		public void TestCusSupportingInfoTypes()
		{
			var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)nctsBill).GetCusSupportingInfoTypes();
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalInfo", typeof(NctsBillAdditionalDocument), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
				AssertEquals("PreviousDocument", typeof(CommonPreviousDocument), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.PreviousDocument]);
				AssertEquals("SupportingDocument", typeof(NctsSupportingDocument), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.SupportingDocument]);
			});
		}

		public void TestAdditionalDocuments()
		{
			AssertType<NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>>(nctsBill.AdditionalDocuments);
		}

		public void TestPreviousDocuments()
		{
			AssertType<CommonPreviousDocumentCollection<CommonPreviousDocument>>(nctsBill.PreviousDocuments);
		}

		public void TestSupportingDocuments()
		{
			AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>(nctsBill.SupportingDocuments);
		}

		public void TestSupportingDocuments_ReadOnlyForArrival()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalBill = header.Bills.AddNew();
			AssertEquals(true, arrivalBill.SupportingDocuments.ReadOnly);
		}

		public void TestArrivalGoodsItems()
		{
			AssertType<NctsArrivalCargoDescCollection<NctsArrivalCargoDesc>>(nctsBill.ArrivalGoodsItems);
		}

		public void TestEffectiveConsignor_Null()
		{
			AssertNull(nctsBill.EffectiveConsignor);
		}

		public void TestEffectiveConsignor_Bill()
		{
			var consignorBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsignorJobDocAddressRequirement);
			consignorBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorBill").PK;
			var consignorHeader = nctsHeader.DocAddresses.CreateWithRequirement(nctsHeader.ConsignorJobDocAddressRequirement);
			consignorHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorHeader").PK;
			var result = nctsBill.EffectiveConsignor;
			CombineAssertions(() =>
			{
				AssertEquals("result", "ConsignorBill", result.CompanyName);
				AssertSame("Cached", result, nctsBill.EffectiveConsignor);
			});
		}

		public void TestEffectiveConsignor_Header()
		{
			var consignorHeader = nctsHeader.DocAddresses.CreateWithRequirement(nctsHeader.ConsignorJobDocAddressRequirement);
			consignorHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsignorHeader").PK;
			AssertEquals("ConsignorHeader", nctsBill.EffectiveConsignor.CompanyName);
		}

		public void TestEffectiveConsignee_Null()
		{
			AssertNull(nctsBill.EffectiveConsignee);
		}

		public void TestEffectiveConsignee_Bill()
		{
			var consigneeBill = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsigneeJobDocAddressRequirement);
			consigneeBill.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeBill").PK;
			var consigneeHeader = nctsHeader.DocAddresses.CreateWithRequirement(nctsHeader.ConsigneeJobDocAddressRequirement);
			consigneeHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeHeader").PK;
			var result = nctsBill.EffectiveConsignee;
			CombineAssertions(() =>
			{
				AssertEquals("result", "ConsigneeBill", result.CompanyName);
				AssertSame("Cached", result, nctsBill.EffectiveConsignee);
			});
		}

		public void TestEffectiveConsignee_Header()
		{
			var consigneeHeader = nctsHeader.DocAddresses.CreateWithRequirement(nctsHeader.ConsigneeJobDocAddressRequirement);
			consigneeHeader.E2_OA_Address = TestHelper.GetTestOrgAddress(Factory, "ConsigneeHeader").PK;
			nctsHeader.Consignee.E2_OA_Address = consigneeHeader.E2_OA_Address;
			AssertEquals("ConsigneeHeader", nctsBill.EffectiveConsignee.CompanyName);
		}

		public void TestEffectiveCountryOfDispatch() => CombineAssertions(() =>
		{
			AssertNull(nctsBill.EffectiveCountryOfDispatch);

			nctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch = "DE";
			AssertEquals("On header", "DE", nctsBill.EffectiveCountryOfDispatch);

			nctsBill.B0_RN_NKCountryOfExport = "FR";
			AssertEquals("On bill", "FR", nctsBill.EffectiveCountryOfDispatch);
		});

		public void TestEffectiveCountryOfDestination() => CombineAssertions(() =>
		{
			AssertNull(nctsBill.EffectiveCountryOfDestination);

			nctsHeader.MovementHeader.BM_RL_NKDestinationPort = "DE";
			AssertEquals("On header", "DE", nctsBill.EffectiveCountryOfDestination);

			nctsBill.B0_RN_NKCountryOfDestination = "FR";
			AssertEquals("On bill", "FR", nctsBill.EffectiveCountryOfDestination);
		});

		public void TestEffectiveReferenceNumberUCR() => CombineAssertions(() =>
		{
			AssertNull(nctsBill.EffectiveReferenceNumberUCR);

			nctsHeader.MovementHeader.BM_UniqueConsignmentReference = "DE";
			AssertEquals("On header", "DE", nctsBill.EffectiveReferenceNumberUCR);

			nctsBill.B0_ReferenceID = "FR";
			AssertEquals("On bill", "FR", nctsBill.EffectiveReferenceNumberUCR);
		});

		public void TestCustomsEntryIntegrator()
		{
			AssertType<NctsBillCustomsEntryIntegrator>("CustomsEntryIntegrator", nctsBill.GetCustomsEntryIntegrator());
		}

		public void TestBM_NoChangesToReportBoolean_AdditionalDocuments_ReadOnly()
		{
			CombineAssertions(() =>
			{
				var arrivalMovementHeader = Factory.New<NctsHeader>();
				arrivalMovementHeader.SetMovementType(NctsMovementType.Codes.Arrival);
				arrivalMovementHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsBill = arrivalMovementHeader.Bills.AddNew();
				var additionalDocumentsBills = nctsBill.AdditionalDocuments.AddNew();
				var arrivalGoodsItems = nctsBill.ArrivalGoodsItems.AddNew();
				var additionalDocumentsGoodsItems = arrivalGoodsItems.AdditionalInfos.AddNew();

				arrivalMovementHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
				AssertEquals("BM_NoChangesToReport is equal to false, AdditionalDocuments in bills are readonly", true, additionalDocumentsBills.ReadOnly);
				AssertEquals("BM_NoChangesToReport is equal to false, AdditionalDocuments in goods items are readonly", true, additionalDocumentsGoodsItems.ReadOnly);

				arrivalMovementHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
				AssertEquals("BM_NoChangesToReport is equal to true, AdditionalDocuments in bills are readonly", true, additionalDocumentsBills.ReadOnly);
				AssertEquals("BM_NoChangesToReport is equal to true, AdditionalDocuments in goods items are readonly", true, additionalDocumentsGoodsItems.ReadOnly);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => nctsBill;

		protected override BusinessObject GetNewBusinessObject() => nctsBill;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => nctsBill;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsBill = nctsHeader.Bills.AddNew();
		}
		NctsHeader nctsHeader;
		NctsBill nctsBill;
	}
}
