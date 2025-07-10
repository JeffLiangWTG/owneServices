using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDeepCloneStrategy))]
	sealed class NctsDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCloneNctsHeader()
		{
			GlbCompany.CurrentCompany.SetCountry("LV");
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.BH_HeaderType = "XXX";
			nctsHeaderToClone.BH_JobReference = "JobReference";
			nctsHeaderToClone.BH_CustomsProfile = "customsProfile";
			nctsHeaderToClone.BH_GB = ZGuid.NewZGuid();

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			CombineAssertions(() =>
			{
				AssertEquals("BH_HeaderType is copied unchanged", "XXX", clonedNctsHeader.BH_HeaderType);
				AssertEquals("BH_JobReference is not copied", ZString.Empty, clonedNctsHeader.BH_JobReference);
				AssertEquals("BH_CustomsProfile is not copied", ZString.Empty, clonedNctsHeader.BH_CustomsProfile);
				AssertEquals("BH_GB is set to a current branch", nctsHeaderToClone.BH_GB, clonedNctsHeader.BH_GB);
			});
		}

		public void TestCloneNctsHeader_ShouldCopyAuthorizationUsages()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);

			var authorizationUsage = nctsHeaderToClone.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = "123";
			authorizationUsage.AGC_Number = "456";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			CombineAssertions(() =>
			{
				var clonedAuthorizationUsage = clonedNctsHeader.CusAuthorizationUsages.Single();
				AssertEquals("AGC_Code", "123", clonedAuthorizationUsage.AGC_Code);
				AssertEquals("AGC_Number", "456", clonedAuthorizationUsage.AGC_Number);
			});
		}

		public void TestCloneNctsHeader_ShouldNotCopyHeaderContainers()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);

			nctsHeaderToClone.DepartureHeaderContainers.AddNew();

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals(0, clonedNctsHeader.DepartureHeaderContainers.Count);
		}

		public void TestCloneNctsHeader_ShouldNotCopySeals()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.Seals.AddNew();

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals(0, clonedNctsHeader.Seals.Count);
		}

		public void TestCloneNctsHeader_ShouldCopySupportingDocuments()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeaderToClone = nctsHeaderToClone.MovementHeader;

			var supportingDocument = movementHeaderToClone.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "123";
			supportingDocument.CSI_ReferenceNumber = "456";
			supportingDocument.CSI_ItemNumber = 1;
			supportingDocument.CSI_ReferenceNumber2 = "789";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();
			var clonedMovementHeader = clonedNctsHeader.MovementHeader;

			CombineAssertions(() =>
			{
				var clonedSupportingDocument = clonedMovementHeader.SupportingDocuments.Single();
				AssertEquals("CSI_Code", "123", clonedSupportingDocument.CSI_Code);
				AssertEquals("CSI_ReferenceNumber", "456", clonedSupportingDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_ItemNumber", 1, clonedSupportingDocument.CSI_ItemNumber);
				AssertEquals("CSI_ReferenceNumber2", "789", clonedSupportingDocument.CSI_ReferenceNumber2);
			});
		}

		public void TestCloneNctsHeader_ShouldCopyPreviousDocuments()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);

			var previousDocument = nctsHeaderToClone.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "123";
			previousDocument.CSI_ReferenceNumber = "456";
			previousDocument.CSI_ReferenceNumber2 = "789";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			CombineAssertions(() =>
			{
				var clonedPreviousDocument = clonedNctsHeader.PreviousDocuments.Cast<CommonPreviousDocument>().Single();
				AssertEquals("CSI_Code", "123", clonedPreviousDocument.CSI_Code);
				AssertEquals("CSI_ReferenceNumber", "456", clonedPreviousDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_ReferenceNumber2", "789", clonedPreviousDocument.CSI_ReferenceNumber2);
			});
		}

		public void TestCloneNctsHeader_ShouldCopyAdditionalDocuments()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);

			var additionalDocument = nctsHeaderToClone.AdditionalDocuments.AddNew();
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalDocument.CSI_Code = "123";
			additionalDocument.CSI_ReferenceNumber = "456";
			additionalDocument.CSI_Description = "789";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			CombineAssertions(() =>
			{
				var clonedAdditionalDocument = clonedNctsHeader.AdditionalDocuments.Single();
				AssertEquals("CSI_SubType", AdditionalInfoSubTypeList.Codes.AdditionalReference, clonedAdditionalDocument.CSI_SubType);
				AssertEquals("CSI_Code", "123", clonedAdditionalDocument.CSI_Code);
				AssertEquals("CSI_ReferenceNumber", "456", clonedAdditionalDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_Description", "789", clonedAdditionalDocument.CSI_Description);
			});
		}

		public void TestCloneNctsHeader_ShouldCopyCountriesOfRouting()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);

			var countryOfRouting = nctsHeaderToClone.CountriesOfRouting.AddNew();
			countryOfRouting.CY_Order = 1;
			countryOfRouting.CY_Data = Core.Constants.CountryCodes.Australia;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			CombineAssertions(() =>
			{
				var clonedCountryOfRouting = clonedNctsHeader.CountriesOfRouting.Cast<CountryOfRouting>().Single();
				AssertEquals("CY_Order", (short)1, clonedCountryOfRouting.CY_Order);
				AssertEquals("CY_Data", Core.Constants.CountryCodes.Australia, clonedCountryOfRouting.CY_Data);
			});
		}

		public void TestCloneNctsHeader_ShouldCopySupplyChainActors()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var supplyChainActor = nctsHeaderToClone.CusSupplyChainActors.AddNew();
			supplyChainActor.CFR_Code = SupplyChainActorRoleList.Codes.FW;
			supplyChainActor.OwnerOrgPK = orgHeader.PK;
			supplyChainActor.CFR_Reference = "123";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			CombineAssertions(() =>
			{
				var clonedSupplyChainActor = clonedNctsHeader.CusSupplyChainActors.Cast<CusSupplyChainActorReference>().Single();
				AssertEquals("CFR_Code", SupplyChainActorRoleList.Codes.FW, clonedSupplyChainActor.CFR_Code);
				AssertEquals("OwnerOrgPK", orgHeader.PK, clonedSupplyChainActor.OwnerOrgPK);
				AssertEquals("CFR_Reference", "123", clonedSupplyChainActor.CFR_Reference);
			});
		}

		public void TestCloneMovementHeader_ShouldCopySupplyChainActors()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var supplyChainActor = nctsHeaderToClone.MovementHeader.CusSupplyChainActors.AddNew();
			supplyChainActor.CFR_Code = SupplyChainActorRoleList.Codes.FW;
			supplyChainActor.OwnerOrgPK = orgHeader.PK;
			supplyChainActor.CFR_Reference = "123";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();
			var clonedMoveHeader = clonedNctsHeader.MovementHeader;
			CombineAssertions(() =>
			{
				var clonedSupplyChainActor = clonedMoveHeader.CusSupplyChainActors.Cast<CusSupplyChainActorReference>().Single();
				AssertEquals("CFR_Code", SupplyChainActorRoleList.Codes.FW, clonedSupplyChainActor.CFR_Code);
				AssertEquals("OwnerOrgPK", orgHeader.PK, clonedSupplyChainActor.OwnerOrgPK);
				AssertEquals("CFR_Reference", "123", clonedSupplyChainActor.CFR_Reference);
			});
		}

		public void TestCloneNctsHeader_ShouldCopyTransportMeans()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderToClone.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			var additionalTransportAtBorder1 = nctsHeaderToClone.MovementHeader.AdditionalTransportAtBorderList.AddNew();
			additionalTransportAtBorder1.TPM_IdentificationNumber = "1234";
			additionalTransportAtBorder1.TPM_RN_NKTransportNationality = "DE";
			var additionalTransportAtBorder2 = nctsHeaderToClone.MovementHeader.AdditionalTransportAtBorderList.AddNew();
			additionalTransportAtBorder2.TPM_IdentificationNumber = "5678";
			additionalTransportAtBorder2.TPM_RN_NKTransportNationality = "DE";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			CombineAssertions(() =>
			{
				AssertEquals("Count rows copied", 2, clonedNctsHeader.MovementHeader.AdditionalTransportAtBorderList.Count);
				AssertEquals("Wagon 1 Number", "1234", clonedNctsHeader.MovementHeader.AdditionalTransportAtBorderList[0].WagonNumber);
				AssertEquals("Wagon 1 Nationality", "DE", clonedNctsHeader.MovementHeader.AdditionalTransportAtBorderList[0].WagonNationality);
				AssertEquals("Wagon 2 Number", "5678", clonedNctsHeader.MovementHeader.AdditionalTransportAtBorderList[1].WagonNumber);
				AssertEquals("Wagon 2 Nationality", "DE", clonedNctsHeader.MovementHeader.AdditionalTransportAtBorderList[1].WagonNationality);

				nctsHeaderToClone.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
				var clonedRoadTransportNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

				AssertEquals("Road Transport Header no additional transport means cloned", 0, clonedRoadTransportNctsHeader.MovementHeader.AdditionalTransportAtBorderList.Count);
			});
		}

		public void TestCloneNctsHeader_ShouldCopyHouseConsignment()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var houseConsignmentToClone = nctsHeaderToClone.Bills.AddNew();
			houseConsignmentToClone.SequenceNumber = 1;
			houseConsignmentToClone.Consignor.OrganisationPK = Factory.New<OrgHeader>().PK;
			houseConsignmentToClone.Consignee.OrganisationPK = Factory.New<OrgHeader>().PK;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();
			var clonedhouseConsignment = clonedNctsHeader.Bills.FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("House consignment sequence number", 1, clonedhouseConsignment.SequenceNumber.ToZInt());
				AssertEquals("Consignor is copied unchanged", houseConsignmentToClone.Consignor.OrganisationPK, clonedhouseConsignment.Consignor.OrganisationPK);
				AssertEquals("Consignee is copied unchanged", houseConsignmentToClone.Consignee.OrganisationPK, clonedhouseConsignment.Consignee.OrganisationPK);
			});
		}

		public void TestCloneNctsHeader_ShouldCopyCustomsOffices()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderToClone.MovementHeader.CustomsOffices.RemoveAndDeleteAll();
			var dep = nctsHeaderToClone.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "Off1");
			var des = nctsHeaderToClone.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "Off2");
			var enq = nctsHeaderToClone.MovementHeader.CustomsOffices.AddNew(OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry, "Off3");

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("clonedNctsHeader.MovementHeader.CustomsOffices.Count", 3, clonedNctsHeader.MovementHeader.CustomsOffices.Count);
			AssertArrayEqualsByElements("clonedNctsHeader.MovementHeader.CustomsOffices", new[] { "Off1|DEP", "Off2|DES", "Off3|ENQ" }, clonedNctsHeader.MovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().OrderBy(x => x.CY_Data).Select(x => $"{x.CY_Data}|{x.CY_Code}").ToArray());
		}

		public void TestCloneNctsHeader_ShouldCopyGuarantees()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeaderToClone.Guarantees.RemoveAndDeleteAll();
			var guarantee1 = nctsHeaderToClone.Guarantees.AddNew();
			guarantee1.PW_BondType = EUNctsGuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention;
			guarantee1.PW_BondNumber = "GUA1";
			var guarantee2 = nctsHeaderToClone.Guarantees.AddNew();
			guarantee2.PW_BondType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaived;
			guarantee2.PW_BondNumber = "GUA2";
			var guarantee3 = nctsHeaderToClone.Guarantees.AddNew();
			guarantee3.PW_BondType = EUNctsGuaranteeTypeList.Codes.GuaranteeWaiverByAgreement;
			guarantee3.PW_BondNumber = "GUA3";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("clonedNctsHeader.Guarantees.Count", 3, clonedNctsHeader.Guarantees.Count);
			AssertArrayEqualsByElements("clonedNctsHeader.Guarantees", new[] { "GUA1|B", "GUA2|6", "GUA3|A" }, clonedNctsHeader.Guarantees.Cast<NctsGuarantee>().OrderBy(x => x.PW_BondNumber).Select(x => $"{x.PW_BondNumber}|{x.PW_BondType}").ToArray());
		}

		public void TestCloneNctsHeader_ShouldCopyHouseConsignmentSupportingDocuments()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var houseConsignment = nctsHeaderToClone.Bills.AddNew();
			var supportingDocument = houseConsignment.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "123";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("House consignment supporting document code", "123", clonedNctsHeader.Bills[0].SupportingDocuments[0].CSI_Code);
		}

		public void TestCloneNctsHeader_ShouldCopyHouseConsignmentPreviousDocuments()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var houseConsignment = nctsHeaderToClone.Bills.AddNew();
			var previousDocument = houseConsignment.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "123";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("House consignment supporting document code", "123", clonedNctsHeader.Bills[0].PreviousDocuments[0].CSI_Code);
		}

		public void TestCloneNctsHeader_ShouldCopyHouseConsignmentAdditionalDocuments()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var houseConsignment = nctsHeaderToClone.Bills.AddNew();
			var additionalDocument = houseConsignment.AdditionalDocuments.AddNew();
			additionalDocument.CSI_Code = "123";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("House consignment additional document code", "123", clonedNctsHeader.Bills[0].AdditionalDocuments[0].CSI_Code);
		}

		public void TestCloneNctsHeader_ShouldCopyHouseConsignmentSupplyChainActorReferences()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			var houseConsignment = nctsHeaderToClone.Bills.AddNew();
			var supplyChainActorReference = houseConsignment.CusSupplyChainActorReferences.AddNew();
			supplyChainActorReference.CFR_Code = "123";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("House consignment supply chain actor code", "123", clonedNctsHeader.Bills[0].CusSupplyChainActorReferences[0].CFR_Code);
		}

		public void TestCloneNctsHeader_ShouldCopyHouseConsignmentGoodsItems()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var houseConsignment = nctsHeaderToClone.Bills.AddNew();
			var goodsItem1 = houseConsignment.GoodsItems.AddNew();
			goodsItem1.BY_LineNo = 1;
			var goodsItem2 = houseConsignment.GoodsItems.AddNew();
			goodsItem2.BY_LineNo = 2;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			CombineAssertions(() =>
			{
				AssertEquals("goods items copied", 2, clonedNctsHeader.Bills[0].GoodsItems.Count);
				AssertEquals("goods item row 1", 1, clonedNctsHeader.Bills[0].GoodsItems[0].BY_LineNo.ToZInt());
				AssertEquals("goods item row 2", 2, clonedNctsHeader.Bills[0].GoodsItems[1].BY_LineNo.ToZInt());
			});
		}

		public void TestCloneNctsHeader_ShouldCopyHouseConsignmentGoodsItemsPackages()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var houseConsignment = nctsHeaderToClone.Bills.AddNew();
			var goodsItem = houseConsignment.GoodsItems.AddNew();
			goodsItem.BY_LineNo = 1;
			var package = goodsItem.Packages.AddNew();
			package.B5_PackageID = "XY12";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("House Consignment Goods Item Package", "XY12", clonedNctsHeader.Bills[0].GoodsItems[0].Packages[0].B5_PackageID);
		}

		public void TestCloneNctsHeader_ShouldCopyHouseConsignmentGoodsItemsPreviousDocuments()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var houseConsignment = nctsHeaderToClone.Bills.AddNew();
			var goodsItem = houseConsignment.GoodsItems.AddNew();
			goodsItem.BY_LineNo = 1;
			var previousDocument = goodsItem.PreviousDocuments.AddNew();
			previousDocument.CSI_Description = "goods item previous doc";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("House Consignment Goods Item Previous Document", "goods item previous doc", clonedNctsHeader.Bills[0].GoodsItems[0].PreviousDocuments[0].CSI_Description);
		}

		public void TestCloneNctsHeader_ShouldCopyHouseConsignmentGoodsItemsSupportingDocuments()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var houseConsignment = nctsHeaderToClone.Bills.AddNew();
			var goodsItem = houseConsignment.GoodsItems.AddNew();
			goodsItem.BY_LineNo = 1;
			var supportingDocument = goodsItem.SupportingDocuments.AddNew();
			supportingDocument.CSI_Description = "goods item supporting document";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("House Consignment Goods Item Supporting Document", "goods item supporting document", clonedNctsHeader.Bills[0].GoodsItems[0].SupportingDocuments[0].CSI_Description);
		}

		public void TestCloneNctsHeader_ShouldCopyHouseConsignmentGoodsItemsSupplyChainActorReferences()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var houseConsignment = nctsHeaderToClone.Bills.AddNew();
			var goodsItem = houseConsignment.GoodsItems.AddNew();
			goodsItem.BY_LineNo = 1;
			var supplyChainActorReference = goodsItem.CusSupplyChainActorReferences.AddNew();
			supplyChainActorReference.CFR_Code = "X";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("House Consignment Goods Items Supply Chain Actors Reference", "X", clonedNctsHeader.Bills[0].GoodsItems[0].CusSupplyChainActorReferences[0].CFR_Code);
		}

		public void TestCloneNctsHeader_ShouldCopyHouseConsignmentGoodsItemsUNDGsAsString()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var houseConsignment = nctsHeaderToClone.Bills.AddNew();
			var goodsItem = houseConsignment.GoodsItems.AddNew();
			goodsItem.BY_LineNo = 1;

			goodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			goodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0005", "", "IMO").First().PK;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();
			AssertEquals("GoodsItem UNDGsAsString", "0004a,0005", clonedNctsHeader.Bills[0].GoodsItems[0].UNDGsAsString);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_DepartureMovementHeader_BM_BH()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertNotEquals("BM_BH not copied", nctsHeaderToClone.MovementHeader.BM_BH, clonedNctsHeader.MovementHeader.BM_BH);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_BH()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertNotEquals("BM_BH not copied", nctsHeaderToClone.ArrivalMovementHeader.BM_BH, clonedNctsHeader.ArrivalMovementHeader.BM_BH);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_DepartureMovementHeader_BM_GS_NKCusAgent()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.MovementHeader.BM_GS_NKCusAgent = "AG";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_GS_NKCusAgent not copied", ZString.Empty, clonedNctsHeader.MovementHeader.BM_GS_NKCusAgent);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_GS_NKCusAgent()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_GS_NKCusAgent = "AG";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_GS_NKCusAgent not copied", ZString.Empty, clonedNctsHeader.ArrivalMovementHeader.BM_GS_NKCusAgent);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_DepartureMovementHeader_BM_CustomsStatus()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.MovementHeader.BM_CustomsStatus = "PRS";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_CustomsStatus not copied", ZString.Empty, clonedNctsHeader.MovementHeader.BM_CustomsStatus);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_CustomsStatus()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_CustomsStatus = "PRS";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_CustomsStatus not copied", ZString.Empty, clonedNctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_DepartureMovementHeader_BM_PaperlessInbondNum()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.MovementHeader.BM_PaperlessInbondNum = "LRN";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_PaperlessInbondNum not copied", ZString.Empty, clonedNctsHeader.MovementHeader.BM_PaperlessInbondNum);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_PaperlessInbondNum()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_PaperlessInbondNum not copied", ZString.Empty, clonedNctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_DepartureMovementHeader_BM_Phase()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = Factory.New<NctsDepartureMovementHeader>();
			movementHeader.BM_Phase = "DRJ";
			nctsHeaderToClone.MovementHeaders.Add(movementHeader);

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_Phase not copied", ZString.Empty, clonedNctsHeader.MovementHeader.BM_Phase);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_Phase()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_Phase = "DRJ";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_Phase not copied", ZString.Empty, clonedNctsHeader.ArrivalMovementHeader.BM_Phase);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_DepartureMovementHeader_BM_EntryDate()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = Factory.New<NctsDepartureMovementHeader>();
			movementHeader.BM_EntryDate = ZDateTime.BrettsBirthday;
			nctsHeaderToClone.MovementHeaders.Add(movementHeader);

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_EntryDate not copied", ZDateTime.Empty, clonedNctsHeader.MovementHeader.BM_EntryDate);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_EntryDate()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_EntryDate = ZDateTime.BrettsBirthday;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_EntryDate not copied", ZDateTime.Empty, clonedNctsHeader.ArrivalMovementHeader.BM_EntryDate);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_DepartureMovementHeader_BM_BM_DepartureMovement()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.MovementHeader.BM_BM_DepartureMovement = new ZGuid("90267DA0-A314-48A0-B6BA-21D3BDF7D0E9");

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_BM_DepartureMovement not copied", ZGuid.Empty, clonedNctsHeader.MovementHeader.BM_BM_DepartureMovement);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_BM_DepartureMovement()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_BM_DepartureMovement = new ZGuid("90267DA0-A314-48A0-B6BA-21D3BDF7D0E9");

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_BM_DepartureMovement not copied", ZGuid.Empty, clonedNctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_DepartureCargoDesc_Consignee()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();

			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;

			var houseConsignment = nctsHeaderToClone.Bills.AddNew();
			var address = Factory.New<JobDocAddress>();
			var goodsItem1 = houseConsignment.GoodsItems.AddNew();
			goodsItem1.Consignee.E2_OA_Address = address.PK;
			goodsItem1.Consignee.E2_Contact = "TestContact";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
			{
				var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();
				var clonedGoodsItems = clonedNctsHeader.GetGoodsItems().Cast<NctsDepartureCargoDesc>();
				AssertEquals("Goods Items is copied", 1, clonedNctsHeader.Bills[0].GoodsItems.Count);
				var clonedGoodItem = clonedGoodsItems.ElementAt(0);

				CombineAssertions("When Transition period is ON", () =>
				{
					AssertEquals("OrganizationPK is copied", nctsHeaderToClone.Bills[0].GoodsItems[0].Consignee.OrganisationPK, clonedGoodItem.Consignee.OrganisationPK);
					AssertEquals("E2_OA_Address is copied", address.PK, clonedGoodItem.Consignee.E2_OA_Address);
					AssertEquals("E2_Contact is copied", "TestContact", clonedGoodItem.Consignee.E2_Contact);
				});
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
			{
				var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();
				var clonedGoodsItems = clonedNctsHeader.GetGoodsItems().Cast<NctsDepartureCargoDesc>();
				AssertEquals("Goods Items is copied", 1, clonedNctsHeader.Bills[0].GoodsItems.Count);
				var clonedGoodItem = clonedGoodsItems.ElementAt(0);

				CombineAssertions("When Transition period is OFF", () =>
				{
					AssertEquals("OrganizationPK is empty", ZGuid.Empty, clonedGoodItem.Consignee.OrganisationPK);
					AssertEquals("E2_OA_Address is empty", ZGuid.Empty, clonedGoodItem.Consignee.E2_OA_Address);
					AssertEquals("E2_Contact is empty", ZString.Empty, clonedGoodItem.Consignee.E2_Contact);
				});
			}
		}

		public void TestCloneNctsHeader_ShouldNotCopy_DepartureMovementHeader_BM_WarehouseTransactionStatus()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.MovementHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_WarehouseTransactionStatus not copied", ZString.Empty, clonedNctsHeader.MovementHeader.BM_WarehouseTransactionStatus);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_WarehouseTransactionStatus()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_WarehouseTransactionStatus not copied", ZString.Empty, clonedNctsHeader.ArrivalMovementHeader.BM_WarehouseTransactionStatus);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_DepartureMovementHeader_BM_ValuationDate()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderToClone.MovementHeader.BM_ValuationDate = new ZDateTime(2024, 12, 02);

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_ValuationDate not copied", ZDateTime.Empty, clonedNctsHeader.MovementHeader.BM_ValuationDate);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_ValuationDate()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_ValuationDate = new ZDateTime(2024, 12, 02);

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_ValuationDate not copied", ZDateTime.Empty, clonedNctsHeader.ArrivalMovementHeader.BM_ValuationDate);
		}

		public void TestCloneNctsHeader_ShouldNotCreateIfNotExist_DepartureMovementHeader_LocationOfGoods()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			CombineAssertions(() =>
			{
				AssertNull("GoodsLocation doesn't exist in source", Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(nctsHeaderToClone.MovementHeader, CusGoodsLocationUseList.Codes.Departure));
				AssertNull("GoodsLocation doesn't exist in target", Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(clonedNctsHeader.MovementHeader, CusGoodsLocationUseList.Codes.Departure));
			});
		}

		public void TestCloneNctsHeader_ShouldCopy_DepartureMovementHeader_LocationOfGoods_Qualifier_Y()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);

			var goodsLocation = nctsHeaderToClone.MovementHeader.GoodsLocation;
			var goodsLocationAddress = goodsLocation.Address;
			goodsLocation.CGL_Qualifier = "Y";
			goodsLocation.CGL_Type = "C";
			goodsLocationAddress.IdentificationHolderPK = ZGuid.Empty;
			goodsLocationAddress.AuthorisationNumber = "123456";
			SetUserContactDetails(goodsLocationAddress);

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			CombineAssertions("CGL_Qualifier is Y", () =>
			{
				var newGoodsLocation = clonedNctsHeader.MovementHeader.GoodsLocation;
				var newGoodsLocationAddress = newGoodsLocation.Address;
				AssertEquals("GoodsLocation CGL_Qualifier should be cloned", "Y", newGoodsLocation.CGL_Qualifier);
				AssertEquals("GoodsLocation CGL_Type should be cloned", "C", newGoodsLocation.CGL_Type);
				AssertEquals("GoodsLocation IdentificationHolderPK should be cloned", ZGuid.Empty, newGoodsLocationAddress.IdentificationHolderPK);
				AssertEquals("GoodsLocation Authorization Number should be cloned", "123456", newGoodsLocationAddress.AuthorisationNumber);
				AssertUserContactDetails(newGoodsLocationAddress);
			});
		}

		public void TestCloneNctsHeader_ShouldCopy_DepartureMovementHeader_LocationOfGoods_Qualifier_Z()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Departure);

			var goodsLocation = nctsHeaderToClone.MovementHeader.GoodsLocation;
			var goodsLocationAddress = goodsLocation.Address;
			goodsLocation.CGL_Qualifier = "Z";
			goodsLocation.CGL_Type = "C";
			goodsLocationAddress.E2_RN_NKCountryCode = "IT";
			goodsLocationAddress.E2_Address1 = "old_address";
			goodsLocationAddress.E2_AddressOverride = true;
			goodsLocationAddress.E2_City = "old_city";
			goodsLocationAddress.Postcode = "221106";
			SetUserContactDetails(goodsLocationAddress);

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			CombineAssertions("CGL_Qualifier is Z", () =>
			{
				var newGoodsLocation = clonedNctsHeader.MovementHeader.GoodsLocation;
				var newGoodsLocationAddress = newGoodsLocation.Address;
				AssertEquals("GoodsLocation CGL_Qualifier should be cloned", "Z", newGoodsLocation.CGL_Qualifier);
				AssertEquals("GoodsLocation CGL_Type should be cloned", "C", newGoodsLocation.CGL_Type);
				AssertEquals("GoodsLocation country code should be cloned", "IT", newGoodsLocationAddress.E2_RN_NKCountryCode);
				AssertEquals("GoodsLocation Address1 should be cloned", "old_address", newGoodsLocationAddress.E2_Address1);
				AssertEquals("GoodsLocation AddressOverride should be cloned", true, newGoodsLocationAddress.E2_AddressOverride);
				AssertEquals("GoodsLocation City should be cloned", "old_city", newGoodsLocationAddress.E2_City);
				AssertUserContactDetails(newGoodsLocationAddress);
				AssertEquals("GoodsLocation Postcode should be cloned", "221106", newGoodsLocationAddress.Postcode);
			});
		}

		public void TestCloneNctsHeader_ShouldCopy_ArrivalMovementHeader_LocationOfGoods()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);

			var goodsLocation = nctsHeaderToClone.ArrivalMovementHeader.GoodsLocation;
			goodsLocation.CGL_Qualifier = "Z";
			goodsLocation.Address.Postcode = "221106";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			CombineAssertions(() =>
			{
				var newGoodsLocation = clonedNctsHeader.ArrivalMovementHeader.GoodsLocation;
				var newGoodsLocationAddress = newGoodsLocation.Address;
				AssertEquals("GoodsLocation CGL_Qualifier should be cloned", "Z", newGoodsLocation.CGL_Qualifier);
				AssertEquals("GoodsLocation Postcode should be cloned", "221106", newGoodsLocationAddress.Postcode);
			});
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_UnloadingDate()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(2024, 12, 9, 11, 52, 45);

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_UnloadingDate not copied", ZDateTimeOffset.Today, clonedNctsHeader.ArrivalMovementHeader.BM_UnloadingDate);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_NoChangesToReport()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_NoChangesToReport = ZBool.False;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_NoChangesToReport not copied", ZBool.True, clonedNctsHeader.ArrivalMovementHeader.BM_NoChangesToReport);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_StateOfSealsBoolean()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_StateOfSealsBoolean = ZBool.False;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_StateOfSealsBoolean not copied", ZBool.True, clonedNctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_UnloadingCompleted()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_UnloadingCompleted = ZBool.False;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_UnloadingCompleted not copied", ZBool.True, clonedNctsHeader.ArrivalMovementHeader.BM_UnloadingCompleted);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_UnloadingRemarks()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_UnloadingRemarks = "Unloading Remarks";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_UnloadingRemarks not copied", ZString.Empty, clonedNctsHeader.ArrivalMovementHeader.BM_UnloadingRemarks);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_OtherThingsToReport()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.OtherThingsToReport = "Other Things";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("OtherThingsToReport not copied", ZString.Empty, clonedNctsHeader.ArrivalMovementHeader.OtherThingsToReport);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_GrossWeight()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_GrossWeight = 150;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_GrossWeight not copied", ZDecimal.Zero, clonedNctsHeader.ArrivalMovementHeader.BM_GrossWeight);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_GrossWeightUQ()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_GrossWeightUQ = "TN";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_GrossWeightUQ not copied", "KG", clonedNctsHeader.ArrivalMovementHeader.BM_GrossWeightUQ);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_GrossWeightUnloaded()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_GrossWeightUnloaded = 150;

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_GrossWeightUnloaded not copied", ZDecimal.Zero, clonedNctsHeader.ArrivalMovementHeader.BM_GrossWeightUnloaded);
		}

		public void TestCloneNctsHeader_ShouldNotCopy_ArrivalMovementHeader_BM_InlandTransportMode()
		{
			var nctsHeaderToClone = Factory.New<NctsHeader>();
			nctsHeaderToClone.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeaderToClone.ArrivalMovementHeader.BM_InlandTransportMode = "MOT";

			var clonedNctsHeader = (NctsHeader)new NctsDeepCloneStrategy(nctsHeaderToClone, ZGuid.Empty).Clone();

			AssertEquals("BM_InlandTransportMode not copied", ZString.Empty, clonedNctsHeader.ArrivalMovementHeader.BM_InlandTransportMode);
		}

		void SetUserContactDetails(CusGoodsLocationAddress address)
		{
			address.E2_Email = "abc@xyz.com";
			address.E2_Contact = "old_contactPerson";
			address.E2_Phone = "1234567890";
		}

		void AssertUserContactDetails(CusGoodsLocationAddress address)
		{
			AssertEquals("GoodsLocation Email should be cloned", "abc@xyz.com", address.E2_Email);
			AssertEquals("GoodsLocation Contact should be cloned", "old_contactPerson", address.E2_Contact);
			AssertEquals("GoodsLocation Phone should be cloned", "1234567890", address.E2_Phone);
		}
	}
}
