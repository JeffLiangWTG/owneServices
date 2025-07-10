using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class DepartureGoodsItemWrapperTest : Customs.Business.Testing.DataProviderTestCase<DepartureGoodsItemWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new DepartureGoodsItemWrapper(null));
		}

		public void TestItemNumber()
		{
			goodsItem.BY_LineNo = 1;
			AssertEquals(1, wrapper.ItemNumber);
		}

		public void TestCommodityCode()
		{
			goodsItem.BY_HarmonisedTariff = "123.456.789";
			AssertEquals("123.456.789", wrapper.CommodityCode);
		}

		public void TestTypeOfDeclaration()
		{
			goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.SGI;
			AssertEquals(NctsConstants.NctsTypeOfDeclaration.Codes.SGI, wrapper.TypeOfDeclaration);
		}

		public void TestGoodsDescription()
		{
			goodsItem.BY_Description = "Books";
			AssertEquals("Books", wrapper.GoodsDescription);
		}

		public void TestGoodsDescriptionLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.GoodsDescriptionLanguage);
		}

		public void TestGrossMass()
		{
			goodsItem.BY_GrossWeight = new ZDecimal(2000);
			goodsItem.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			AssertEquals(907.18474m, wrapper.GrossMass);
		}

		public void TestNetMass()
		{
			goodsItem.BY_NetWeight = new ZDecimal(1000);
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Pounds;
			AssertEquals(453.59237m, wrapper.NetMass);
		}

		public void TestCountryOfDispatchExportCode()
		{
			goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.France;
			AssertEquals(Core.Constants.CountryCodes.France, wrapper.CountryOfDispatchExportCode);
		}

		public void TestCountryOfDestinationCode()
		{
			goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Italy;
			AssertEquals(Core.Constants.CountryCodes.Italy, wrapper.CountryOfDestinationCode);
		}

		public void TestTransportChargesMethodOfPayment()
		{
			goodsItem.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
			AssertEquals(TransportChargesModeOfPayment.Codes.CreditCard, wrapper.TransportChargesMethodOfPayment);
		}

		public void TestCommercialReferenceNumber()
		{
			goodsItem.BY_CommercialReferenceNumber = "HQDOV002";
			AssertEquals("HQDOV002", wrapper.CommercialReferenceNumber);
		}

		public void TestUNDangerousGoodsCode()
		{
			goodsItem.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals("0004a", wrapper.UNDangerousGoodsCode);
		}

		public void TestBillValue()
		{
			goodsItem.BY_MonetaryValue = 10;
			AssertEquals(10m, wrapper.BillValue);
		}

		public void TestPreviousAdministrativeReferences()
		{
			var previousDocument1 = goodsItem.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = "CODE1";
			var previousDocument2 = goodsItem.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "CODE2";
			AssertContainsExactElementsInAnyOrder(new[] { "CODE1", "CODE2" }, wrapper.PreviousAdministrativeReferences.Select(x => x.PreviousDocumentType));
		}

		public void TestProducedDocumentsCertificates()
		{
			var supportingDocument1 = goodsItem.SupportingDocuments.AddNew();
			supportingDocument1.CSI_ReferenceNumber = "PREV01";
			var supportingDocument2 = goodsItem.SupportingDocuments.AddNew();
			supportingDocument2.CSI_ReferenceNumber = "PREV02";
			AssertContainsExactElementsInAnyOrder(new[] { "PREV01", "PREV02" }, wrapper.ProducedDocumentsCertificates.Select(x => x.DocumentReference));
		}

		public void TestSpecialMentions()
		{
			var additionalInfo1 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "CODE1";
			var additionalInfo2 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "CODE2";
			AssertContainsExactElementsInAnyOrder(new[] { "CODE1", "CODE2" }, wrapper.SpecialMentions.Select(x => x.Statement));
		}

		public void TestConsignor()
		{
			var goodsItemConsignorOrg = Factory.NewWithValidTestData<OrgHeader>();
			goodsItemConsignorOrg.OH_FullName = "CONSIGNOR FROM GOODSITEM";

			var billConsignorOrg = Factory.NewWithValidTestData<OrgHeader>();
			billConsignorOrg.OH_FullName = "CONSIGNOR FROM BILL";

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				bill.Consignor.E2_OA_Address = billConsignorOrg.MainAddress.PK;
				wrapper = new DepartureGoodsItemWrapper(goodsItem);

				var consignor = wrapper.Consignor;

				AssertEquals("Correct Org wrapped", "CONSIGNOR FROM BILL", consignor.Name);
				AssertEquals("Populate TIR false", ZString.Empty, consignor.HolderIDTIR);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				goodsItem.Consignor.E2_OA_Address = goodsItemConsignorOrg.MainAddress.PK;
				wrapper = new DepartureGoodsItemWrapper(goodsItem);

				var consignor = wrapper.Consignor;

				AssertEquals("Correct Org wrapped", "CONSIGNOR FROM GOODSITEM", consignor.Name);
				AssertEquals("Populate TIR false", ZString.Empty, consignor.HolderIDTIR);
			}
		}

		public void TestConsignee()
		{
			var goodsItemConsigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			goodsItemConsigneeOrg.OH_FullName = "CONSIGNEE FROM GOODSITEM";

			var billConsigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			billConsigneeOrg.OH_FullName = "CONSIGNEE FROM BILL";

			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				bill.Consignee.E2_OA_Address = billConsigneeOrg.MainAddress.PK;
				wrapper = new DepartureGoodsItemWrapper(goodsItem);

				var consignee = wrapper.Consignee;

				AssertEquals("Correct Org wrapped", "CONSIGNEE FROM BILL", consignee.Name);
				AssertEquals("Populate TIR false", ZString.Empty, consignee.HolderIDTIR);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				goodsItem.Consignee.E2_OA_Address = goodsItemConsigneeOrg.MainAddress.PK;
				wrapper = new DepartureGoodsItemWrapper(goodsItem);

				var consignee = wrapper.Consignee;

				AssertEquals("Correct Org wrapped", "CONSIGNEE FROM GOODSITEM", consignee.Name);
				AssertEquals("Populate TIR false", ZString.Empty, consignee.HolderIDTIR);
			}
		}

		public void TestContainers()
		{
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1A";
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2A";

			var pivot1 = goodsItem.ContainersPivots[0];
			pivot1.ContainerNumber = "CONTAINER1A";
			pivot1.ContainerSelected = true;
			var pivot2 = goodsItem.ContainersPivots[1];
			pivot2.ContainerNumber = "CONTAINER2A";
			pivot2.ContainerSelected = true;

			AssertContainsExactElementsInAnyOrder(new[] { "CONTAINER1A", "CONTAINER2A" }, wrapper.Containers);
		}

		public void TestPackages()
		{
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_MarksAndNumbers = "PACK456";
			var package2 = goodsItem.Packages.AddNew();
			package2.B5_MarksAndNumbers = "PACK123";
			AssertContainsExactElementsInAnyOrder(new[] { "PACK123", "PACK456" }, wrapper.Packages.Select(x => x.MarksAndNumbersOfPackages));
		}

		public void TestConsignorSecurity()
		{
			var consignorOrg = Factory.NewWithValidTestData<OrgHeader>();
			consignorOrg.OH_FullName = "SECUTIRY CONSIGNOR ORGANISATION";
			goodsItem.SecurityConsignor.E2_OA_Address = consignorOrg.MainAddress.PK;
			CombineAssertions(() =>
			{
				var consignorSecurity = wrapper.ConsignorSecurity;
				AssertEquals("Correct Org wrapped", "SECUTIRY CONSIGNOR ORGANISATION", consignorSecurity.Name);
				AssertEquals("Populate TIR false", ZString.Empty, consignorSecurity.HolderIDTIR);
			});
		}

		public void TestConsigneeSecurity()
		{
			var consigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			consigneeOrg.OH_FullName = "SECURITY CONSIGNEE ORGANISATION";
			goodsItem.SecurityConsignee.E2_OA_Address = consigneeOrg.MainAddress.PK;
			CombineAssertions(() =>
			{
				var consigneeSecurity = wrapper.ConsigneeSecurity;
				AssertEquals("Correct Org wrapped", "SECURITY CONSIGNEE ORGANISATION", consigneeSecurity.Name);
				AssertEquals("Populate TIR false", ZString.Empty, consigneeSecurity.HolderIDTIR);
			});
		}

		public void TestDeclarationGoodsItemNumber()
		{
			goodsItem.BY_DeclarationGoodsItemNumber = 10;
			AssertEquals(10, wrapper.DeclarationItemNumber);
		}

		public void TestBillSequenceNumber()
		{
			var bill = Factory.New<NctsBill>();
			AssertEquals(ZShort.Zero, wrapper.BillSequenceNumber);
			bill.SequenceNumber = 2;
			goodsItem.BY_ParentID = bill.PK;
			AssertEquals(new ZShort(2), wrapper.BillSequenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = header.MovementHeader.GoodsItems.AddNew();
			wrapper = new DepartureGoodsItemWrapper(goodsItem);
		}
		NctsDepartureCargoDesc goodsItem;
		NctsHeader header;
		IDepartureGoodsItem wrapper;

		protected override DepartureGoodsItemWrapper GetProvider()
		{
			goodsItem.Consignor.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			goodsItem.Consignee.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			goodsItem.SecurityConsignor.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			goodsItem.SecurityConsignee.E2_OA_Address = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			return new DepartureGoodsItemWrapper(goodsItem);
		}

		protected override IEnumerable<Expression<Func<DepartureGoodsItemWrapper, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.Consignor;
			yield return x => x.Consignee;
			yield return x => x.ConsignorSecurity;
			yield return x => x.ConsigneeSecurity;
		}
	}
}
