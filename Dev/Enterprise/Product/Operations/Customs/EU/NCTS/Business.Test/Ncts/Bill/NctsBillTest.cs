using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsBill))]
	sealed class NctsBillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("HumanReadableName", "House Consignment", Phase5DepartureNctsBill.HumanReadableName);
		}

		public void TestMovementDetail_Arrival()
		{
			AssertType<CusInBondMoveDetail>(Phase5ArrivalNctsBill.MovementDetail);
		}

		public void TestMovementDetail_Departure()
		{
			AssertType<CusInBondMoveDetail>(Phase5DepartureNctsBill.MovementDetail);
		}

		public void TestCheckConsignee_NR0069()
		{
			const string messageError = "[NR0069] Consignee must have EORI number if additional reference code Y023 is used.";
			using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMoveHeaderType.Codes.Departure);
				var bill = nctsHeader.Bills.AddNew();

				deciderTestContext.EnableRule(c => c.IsRuleNR0069Active);

				var orgWithEori = Factory.New<OrgHeader>();
				orgWithEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "54321", "DE");
				var orgWithoutEori = Factory.New<OrgHeader>();
				orgWithoutEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "12345", "IT");
				var consignee = bill.Consignee;
				var propertyInfo = consignee.OrganisationPKInfo;

				CombineAssertions(() =>
				{
					consignee.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("No Y023, No Consignee", propertyInfo, messageError);

					consignee.OrganisationPK = orgWithoutEori.PK;
					AssertNoMessageError("No Y023, Has Consignee (no eori)", propertyInfo, messageError);

					var additionalReference = bill.AdditionalDocuments.AddNew();
					additionalReference.CSI_SubType = "REF";
					additionalReference.CSI_Code = "Y023";
					consignee.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("Has Y023, No Consignee", propertyInfo, messageError);

					consignee.OrganisationPK = orgWithoutEori.PK;
					AssertHasMessageError("Has Y023, Has Consignee (no eori)", propertyInfo, messageError);

					consignee.OrganisationPK = orgWithEori.PK;
					AssertNoMessageError("Has Y023, Has Consignee (with eori)", propertyInfo, messageError);

					consignee.OrganisationPK = orgWithoutEori.PK;
					deciderTestContext.DisableRule(c => c.IsRuleNR0069Active);
					consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageError("Has Y023, Has Consignee (no eori), Rule is not active", propertyInfo, messageError);
				});
			}
		}

		public void TestConditionC0505Bill_Consignor()
		{
			const bool c0505Applied = true;

			CombineAssertions(() =>
			{
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, Phase5DepartureNctsBill.Consignor, c0505Applied);

				var phase4DepartureNctsBill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Departure);
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, phase4DepartureNctsBill.Consignor, !c0505Applied);

				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, Phase5ArrivalNctsBill.Consignor, !c0505Applied);

				var phase4ArrivalNctsBill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Departure);
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, phase4ArrivalNctsBill.Consignor, !c0505Applied);
			});
		}

		public void TestConditionC505Bill_Consignee()
		{
			const bool c0505Applied = true;

			CombineAssertions(() =>
			{
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, Phase5DepartureNctsBill.Consignee, c0505Applied);

				var phase4DepartureNctsBill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Departure);
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, phase4DepartureNctsBill.Consignee, !c0505Applied);

				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, Phase5ArrivalNctsBill.Consignee, !c0505Applied);

				var phase4ArrivalNctsBill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Departure);
				NCTSTestHelper.ExpectedCombinationOfJobDocAddressMessageErrorsAboutC0505(Factory, phase4ArrivalNctsBill.Consignee, !c0505Applied);
			});
		}

		public void TestCheckConsignorNR0068()
		{
			const string messageError = "[NR0068] Consignor must have EORI number if additional reference code Y022 is used.";
			using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var bill = nctsHeader.Bills.AddNew();

				deciderTestContext.EnableRule(c => c.IsRuleNR0068Active);

				var orgWithEori = Factory.New<OrgHeader>();
				orgWithEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "54321", "DE");
				var orgWithoutEori = Factory.New<OrgHeader>();
				orgWithoutEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "12345", "IT");
				var consignor = bill.Consignor;
				var propertyInfo = consignor.OrganisationPKInfo;

				CombineAssertions(() =>
				{
					consignor.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("No Y022, No Consignor", propertyInfo, messageError);

					consignor.OrganisationPK = orgWithoutEori.PK;
					AssertNoMessageError("No Y022, Has Consignor (no eori)", propertyInfo, messageError);

					var additionalReference = bill.AdditionalDocuments.AddNew();
					additionalReference.CSI_SubType = "REF";
					additionalReference.CSI_Code = "Y022";
					consignor.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("Has Y022, No Consignor", propertyInfo, messageError);

					consignor.OrganisationPK = orgWithoutEori.PK;
					AssertHasMessageError("Has Y022, Has Consignor (no eori)", propertyInfo, messageError);

					consignor.OrganisationPK = orgWithEori.PK;
					AssertNoMessageError("Has Y022, Has Consignor (with eori)", propertyInfo, messageError);

					consignor.OrganisationPK = orgWithoutEori.PK;
					deciderTestContext.DisableRule(c => c.IsRuleNR0068Active);
					consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("Has Y022, Has Consignor (no eori), Rule is not active", propertyInfo, messageError);
				});
			}
		}

		public void TestB0_SecurityIndicatorFromExport_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(Phase5DepartureNctsBill.B0_SecurityIndicatorFromExportInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Security", resourceStringData.Caption);
				AssertEquals("FullDescription", "Security Indicator from Export Declaration", resourceStringData.FullDescription);
			});
		}

		public void TestB0_WeightUQ_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(Phase5DepartureNctsBill.B0_WeightUQInfo);
			AssertEquals("Caption", "Units", resourceStringData.Caption);
		}

		public void TestGrossWeight()
		{
			var nctsBill = Phase5DepartureNctsBill;
			nctsBill.B0_Weight = new(1500m);
			nctsBill.B0_WeightUQ = Weight.Grams;

			AssertEquals(new ZWeight(1.5, Weight.Kilograms), nctsBill.GrossWeight);
		}

		public void TestGrossWeightInKilograms()
		{
			var nctsBill = Phase5DepartureNctsBill;
			nctsBill.B0_Weight = new ZDecimal(2000);

			CombineAssertions(() =>
			{
				nctsBill.B0_WeightUQ = Core.Constants.Weight.Pounds;
				AssertEquals("Equal to the converted value of B0_Weight", 907.18474m, nctsBill.GrossWeightInKilograms);

				nctsBill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("Equal to B0_Weight because the B0_WeightUQ is kilograms", nctsBill.B0_Weight, nctsBill.GrossWeightInKilograms);
			});
		}

		public void TestGrossWeightUnloaded()
		{
			var nctsBill = Phase5DepartureNctsBill;
			nctsBill.B0_GrossWeightUnloaded = new(1m);
			nctsBill.B0_WeightUQ = Weight.Decitons;

			AssertEquals(new ZWeight(0.1m, Weight.Tonnes), nctsBill.GrossWeightUnloaded);
		}

		public void TestGrossWeightUnloadedInKilograms()
		{
			var nctsBill = Phase5DepartureNctsBill;
			nctsBill.B0_GrossWeightUnloaded = new ZDecimal(2000);

			CombineAssertions(() =>
			{
				nctsBill.B0_WeightUQ = Weight.Pounds;
				AssertEquals(Weight.Pounds, nctsBill.MovementDetail.DifferenceWeightUnit);
				AssertEquals("Equal to the converted value of B0_GrossWeightUnloaded", 907.18474m, nctsBill.GrossWeightUnloadedInKilograms);

				nctsBill.B0_WeightUQ = Weight.Kilograms;
				AssertEquals(Weight.Kilograms, nctsBill.MovementDetail.DifferenceWeightUnit);
				AssertEquals("Equal to B0_GrossWeightUnloaded because the B0_WeightUQ is kilograms", nctsBill.B0_GrossWeightUnloaded, nctsBill.GrossWeightUnloadedInKilograms);
			});
		}

		public void TestMovementDetailQuery()
		{
			var moveDetail = Phase5ArrivalNctsBill.MovementDetail;
			moveDetail.B9_B9_InBondMoveDetail = Factory.New<CusInBondMoveDetail>().PK;
			AssertEquals(false, moveDetail.MatchesFilter(Phase5ArrivalNctsBill.MovementDetailQuery));
		}

		public void TestSequenceNumber_Arrival()
		{
			var nctsBill = Phase5ArrivalNctsBill;
			var header = nctsBill.Header;
			CombineAssertions(() =>
			{
				AssertEquals("SequenceNumber of nctsBill1 is 1", "1", nctsBill.MovementDetail.B9_SeqNo);

				var nctsBill2 = header.Bills.AddNew();
				AssertEquals("SequenceNumber of nctsBill2 is 2", "2", nctsBill2.MovementDetail.B9_SeqNo);

				nctsBill.Delete();
				AssertEquals("nctsBill is deleted, SequenceNumber of nctsBill2 is 1", "1", nctsBill2.MovementDetail.B9_SeqNo);
				AssertEquals("When nctsBill2 is deleted, SequenceNumber", (ZShort)1, nctsBill.SequenceNumber);

				nctsBill2.MovementDetail.B9_SeqNo = "243";

				Factory.Save();
				var anotherFactory = new BusinessObjectFactory();
				var nctsHeaderInAnotherFactory = anotherFactory.Load<NctsHeader>(header.PK);
				AssertEquals("Reloaded, SequenceNumber of nctsBill2 is 1", "1", nctsHeaderInAnotherFactory.Bills.First().MovementDetail.B9_SeqNo);
				AssertEquals("Reloaded, HasChanges because B9_SeqNo is recalculated", true, nctsHeaderInAnotherFactory.HasChanges);
			});
		}

		public void TestSequenceNumber_Departure()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var bill1 = header.Bills.AddNew();
			bill1.B0_Weight = 1m;
			bill1.B0_WeightUQ = "LT";

			var bill2 = header.Bills.AddNew();
			bill2.B0_Weight = 2m;
			bill2.B0_WeightUQ = "LT";

			var bill3 = header.Bills.AddNew();
			bill3.B0_Weight = 3m;
			bill3.B0_WeightUQ = "LT";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("bills (MovementDetail.B9_SeqNo, SequenceNumber)",
											new (ZShort, ZString, ZDecimal, ZString)[]
											{
												(1, "1", 1m, "LT"),
												(2, "2", 2m, "LT"),
												(3, "3", 3m, "LT"),
											}, header.Bills.Select(x => (x.SequenceNumber, x.MovementDetail.B9_SeqNo, x.B0_Weight, x.B0_WeightUQ)));
				bill2.Delete();
				Factory.Save();
				AssertContainsExactElementsInAnyOrder("bills (MovementDetail.B9_SeqNo, SequenceNumber)",
											new (ZShort, ZString, ZDecimal, ZString)[]
											{
												(1, "1", 1m, "LT"),
												(2, "2", 3m, "LT"),
											}, header.Bills.Select(x => (x.SequenceNumber, x.MovementDetail.B9_SeqNo, x.B0_Weight, x.B0_WeightUQ)));

				AssertEquals("When bill2 is Deleted, SequenceNumber of bill2", (ZShort)2, bill2.SequenceNumber);
			});
		}

		public void TestLookups()
		{
			AssertType<NctsBillLookups>(Phase5DepartureNctsBill.Lookups);
		}

		public void TestValidation()
		{
			AssertType<NctsBillValidation>(Phase5DepartureNctsBill.Validation);
		}

		public void TestArrivalGoodsItems()
		{
			AssertType<NctsArrivalCargoDescCollection<NctsArrivalCargoDesc>>(Phase5DepartureNctsBill.ArrivalGoodsItems);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(Weight.Kilograms, Phase5DepartureNctsBill.B0_WeightUQ);
		}

		public void TestOnLoad_B0_WeightUQ_Defaulted()
		{
			Phase5DepartureNctsBill.B0_Weight = new(0m);
			Phase5DepartureNctsBill.B0_WeightUQ = ZString.Empty;
			Factory.Save();

			var factory2 = Factory.CreateNewFactory();
			var nctsBill = factory2.Load<NctsBill>(Phase5DepartureNctsBill.PK);

			CombineAssertions(() =>
			{
				AssertEquals(0m, nctsBill.B0_Weight);
				AssertEquals(Weight.Kilograms, nctsBill.B0_WeightUQ);
			});
		}

		public void TestDelete_GoodsItems()
		{
			CombineAssertions(() =>
			{
				var departureGoodsItem = Phase5DepartureNctsBill.GoodsItems.AddNew();
				Phase5DepartureNctsBill.Delete();
				Assert("Departure GoodsItem should be deleted", departureGoodsItem.IsDeleted);

				var arrivalGoodsItem = Phase5ArrivalNctsBill.ArrivalGoodsItems.AddNew();
				Phase5ArrivalNctsBill.Delete();
				Assert("Arrival GoodsItem should be deleted", arrivalGoodsItem.IsDeleted);
			});
		}

		public void TestDelete()
		{
			CombineAssertions(() =>
			{
				Phase5DepartureNctsBill.GoodsItems.AddNew();
				Phase5DepartureNctsBill.Delete();
				AssertNoExceptionThrown("Simulate AddNew() triggered by UI when deleting a bill", () => Phase5DepartureNctsBill.GoodsItems.AddNew());

				Phase5ArrivalNctsBill.ArrivalGoodsItems.AddNew();
				Phase5ArrivalNctsBill.Delete();
				AssertNoExceptionThrown("Simulate AddNew() triggered by UI when deleting a bill", () => Phase5ArrivalNctsBill.ArrivalGoodsItems.AddNew());
			});
		}

		public void TestResetBY_DeclarationGoodsItemNumberOnDelete()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var bill1 = nctsHeader.Bills.AddNew();
			var goodItem11 = bill1.GoodsItems.AddNew();
			goodItem11.BY_DeclarationGoodsItemNumber = 1;
			var goodItem12 = bill1.GoodsItems.AddNew();
			goodItem12.BY_DeclarationGoodsItemNumber = 2;

			var bill2 = nctsHeader.Bills.AddNew();
			var goodItem21 = bill2.GoodsItems.AddNew();
			goodItem21.BY_DeclarationGoodsItemNumber = 3;
			var goodItem22 = bill2.GoodsItems.AddNew();
			goodItem22.BY_DeclarationGoodsItemNumber = 4;

			var bill3 = nctsHeader.Bills.AddNew();
			nctsHeader.ShouldResetGoodsItemNumbers = true;

			CombineAssertions(() =>
			{
				AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill with shouldResetGoodsItemNumbers true", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill with shouldResetGoodsItemNumbers true", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill with shouldResetGoodsItemNumbers true", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill with shouldResetGoodsItemNumbers true", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				bill1.Delete();
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill with goodsItems when it is not in the database with shouldResetGoodsItemNumbers true, bill", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill with goodsItems when it is not in the database with shouldResetGoodsItemNumbers true, bill", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				bill1 = nctsHeader.Bills.AddNew();
				goodItem11 = bill1.GoodsItems.AddNew();
				goodItem11.BY_DeclarationGoodsItemNumber = 1;
				goodItem12 = bill1.GoodsItems.AddNew();
				goodItem12.BY_DeclarationGoodsItemNumber = 2;

				Factory.Save();

				bill3.Delete();
				AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill without goodsItems when data is in database with shouldResetGoodsItemNumbers true, bill3", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill without goodsItems when data is in database with shouldResetGoodsItemNumbers true, bill3", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill without goodsItems when data is in database with shouldResetGoodsItemNumbers true, bill3", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill without goodsItems when data is in database with shouldResetGoodsItemNumbers true, bill3", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				bill1.Delete();
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is 0 after deleting a bill with goodsItems when data is in database with shouldResetGoodsItemNumbers true, bill1", 0, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is 0 after deleting a bill with goodsItems when data is in database with shouldResetGoodsItemNumbers true, bill1", 0, goodItem22.BY_DeclarationGoodsItemNumber);

				bill1 = nctsHeader.Bills.AddNew();
				goodItem11 = bill1.GoodsItems.AddNew();
				goodItem11.BY_DeclarationGoodsItemNumber = 1;
				goodItem12 = bill1.GoodsItems.AddNew();
				goodItem12.BY_DeclarationGoodsItemNumber = 2;
				goodItem21.BY_DeclarationGoodsItemNumber = 3;
				goodItem22.BY_DeclarationGoodsItemNumber = 4;
				nctsHeader.ShouldResetGoodsItemNumbers = false;

				Factory.Save();
				AssertEquals("goodItem11.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill with goodsItems when data is in database with UpdatePreDeclaration false", 1, goodItem11.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem12.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill with goodsItems when data is in database with UpdatePreDeclaration false", 2, goodItem12.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill with goodsItems when data is in database with UpdatePreDeclaration false", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 before deleting a bill with goodsItems when data is in database with UpdatePreDeclaration false", 4, goodItem22.BY_DeclarationGoodsItemNumber);

				bill1.Delete();
				AssertEquals("goodItem21.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill with goodsItems when data is in database with shouldResetGoodsItemNumbers false, bill1", 3, goodItem21.BY_DeclarationGoodsItemNumber);
				AssertEquals("goodItem22.BY_DeclarationGoodsItemNumber is not 0 after deleting a bill with goodsItems when data is in database with shouldResetGoodsItemNumbers false, bill1", 4, goodItem22.BY_DeclarationGoodsItemNumber);
			});
		}

		public void TestDataGroupingCode()
		{
			AssertEquals(Core.Constants.CountryCodes.Latvia, Phase5DepartureNctsBill.DataGroupingCode);
		}

		public void TestGoodsItems()
		{
			AssertType<NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>>(Phase5DepartureNctsBill.GoodsItems);
		}

		public void TestGoodItems_SequenceNumber()
		{
			var nctsBill = Phase5DepartureNctsBill;
			CombineAssertions(() =>
			{
				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				AssertEquals("Line 1", (ZShort)1, goodsItem1.BY_LineNo);
				var goodsItem2 = nctsBill.GoodsItems.AddNew();
				AssertEquals("Line 2", (ZShort)2, goodsItem2.BY_LineNo);
				var goodsItem3 = nctsBill.GoodsItems.AddNew();
				AssertEquals("Line 3", (ZShort)3, goodsItem3.BY_LineNo);
				nctsBill.GoodsItems.Delete(goodsItem2);
				AssertEquals("Renumbered 3 to 2", (ZShort)2, goodsItem3.BY_LineNo);
			});
		}

		public void TestArrivalGoodItems_SequenceNumber()
		{
			CombineAssertions(() =>
			{
				var nctsBill = Phase5ArrivalNctsBill;
				var goodsItem1 = nctsBill.ArrivalGoodsItems.AddNew();
				AssertEquals("Line 1", (ZShort)1, goodsItem1.BY_LineNo);
				var goodsItem2 = nctsBill.ArrivalGoodsItems.AddNew();
				AssertEquals("Line 2", (ZShort)2, goodsItem2.BY_LineNo);
				var goodsItem3 = nctsBill.ArrivalGoodsItems.AddNew();
				AssertEquals("Line 3", (ZShort)3, goodsItem3.BY_LineNo);
				nctsBill.ArrivalGoodsItems.Delete(goodsItem2);
				AssertEquals("Renumbered 3 to 2", (ZShort)2, goodsItem3.BY_LineNo);
			});
		}

		public void TestCusInBondCargoDescType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("NCTS Departure", typeof(NctsDepartureCargoDesc), ((ICusInBondCargoDescTypeProvider)Phase5DepartureNctsBill).CusInBondCargoDescType);

				var arrivalNctsHeader = CreateNctsHeaderToTest(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival);
				var bill = arrivalNctsHeader.Bills.AddNew();
				AssertEquals("NCTS Arrival", typeof(NctsArrivalCargoDesc), ((ICusInBondCargoDescTypeProvider)bill).CusInBondCargoDescType);

				var emptyHeader = Factory.New<NctsHeader>();
				bill = emptyHeader.Bills.AddNew();
				AssertEquals("Empty Header will not return null and should use NctsDepartureCargoDesc as default", typeof(NctsDepartureCargoDesc), ((ICusInBondCargoDescTypeProvider)bill).CusInBondCargoDescType);
			});
		}

		public void TestINctsCusInBondCargoDescMaster_Header()
		{
			AssertEquals(Phase5DepartureNctsBill.Header.PK, ((INctsCusInBondCargoDescMaster)Phase5DepartureNctsBill).Header.PK);
		}

		public void TestSequenceNumber()
		{
			var nctsBill = Phase5DepartureNctsBill;
			var nctsHeader = nctsBill.Header;
			CombineAssertions(() =>
			{
				AssertEquals("SequenceNumber of nctsBill1 is 1", (ZShort)1, nctsBill.SequenceNumber);

				var nctsBill2 = nctsHeader.Bills.AddNew();
				AssertEquals("SequenceNumber of nctsBill2 is 2", (ZShort)2, nctsBill2.SequenceNumber);

				nctsBill.Delete();
				AssertEquals("nctsBill is deleted, SequenceNumber of nctsBill2 is 1", (ZShort)1, nctsBill2.SequenceNumber);

				nctsBill2.SequenceNumber = 243;
				AssertEquals("Setting SequenceNumber of nctsBill2 to 243", (ZShort)243, nctsBill2.SequenceNumber);

				Factory.Save();
				var anotherFactory = new BusinessObjectFactory();
				var nctsHeaderInAnotherFactory = anotherFactory.Load<NctsHeader>(nctsHeader.PK);
				AssertEquals("Reloaded, SequenceNumber of nctsBill2 is 1", (ZShort)1, nctsHeaderInAnotherFactory.Bills.First().SequenceNumber);
				AssertEquals("Reloaded, HasChanges is true", true, nctsHeaderInAnotherFactory.HasChanges);
			});
		}

		public void TestSequenceNumber_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Phase5DepartureNctsBill.SequenceNumberInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Sequence Number", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Sequence No.", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Seq. No.", captionResourceString.ShortCaption);
			});
		}

		public void TestSequenceNumber_ReadOnly()
		{
			AssertEquals(true, Phase5DepartureNctsBill.SequenceNumberInfo.ReadOnly);
		}

		public void TestB0_RN_NKCountryOfExport_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(Phase5DepartureNctsBill.B0_RN_NKCountryOfExportInfo, multipleResourceKey: null, "Country/Region of Dispatch", "Disp. Ctry./Rgn.", "Dispatch Ctry./Rgn.");
		}

		public void TestB0_Weight_Caption()
		{
			NCTSTestHelper.AssertCaptions(Phase5ArrivalNctsBill.B0_WeightInfo, "Total Gross Weight", "Gross Weight", "Gross Wgt.");
		}

		public void TestB0_WeightUQ_ReadOnly_Departure_Phase5()
		{
			AssertEquals("Departure ReadOnly", true, Phase5DepartureNctsBill.B0_WeightUQInfo.ReadOnly);
			AssertEquals("Departure ReadOnly (MetaData)", true, MetaData.GetReadOnly(Phase5DepartureNctsBill, Phase5DepartureNctsBill.B0_WeightUQInfo.PropertyDescriptor));
		}

		public void TestB0_WeightUQ_ReadOnly_Arrival_Phase5()
		{
			var bill = Phase5ArrivalNctsBill;
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, bill.B0_WeightUQInfo.ReadOnly);
				AssertEquals("Editable (MetaData)", false, MetaData.GetReadOnly(bill, bill.B0_WeightUQInfo.PropertyDescriptor));
			});
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(bill.Header.ArrivalMovementHeader, x => ((NctsBill)x).B0_WeightUQInfo.ReadOnly, bill);
		}

		public void TestB0_WeightUQ_ReadOnly_Departure_Phase4()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Departure);
			CombineAssertions(() =>
			{
				AssertEquals("Departure ReadOnly", true, bill.B0_WeightUQInfo.ReadOnly);
				AssertEquals("Departure ReadOnly (MetaData)", true, MetaData.GetReadOnly(bill, bill.B0_WeightUQInfo.PropertyDescriptor));
			});
		}

		public void TestB0_WeightUQ_ReadOnly_Arrival_Phase4()
		{
			var bill = CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Arrival);

			CombineAssertions(() =>
			{
				AssertEquals("ReadOnly", true, bill.B0_WeightUQInfo.ReadOnly);
				AssertEquals("ReadOnly (MetaData)", true, MetaData.GetReadOnly(bill, bill.B0_WeightUQInfo.PropertyDescriptor));
			});
		}

		public void TestB0_ReferenceID_Maxlength()
		{
			var billDepartureP5 = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			var billArrivalP5 = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Arrival);
			var billDepartureP4 = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Departure);
			var billArrivalP4 = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Arrival);

			CombineAssertions(() =>
			{
				AssertEquals("Max length is 35 in Phase 5 and Departure", 35, billDepartureP5.B0_ReferenceIDInfo.MaxLength);
				AssertEquals("Max length is 50 in Phase 5 and Arrival", AutoCusInBondBill.Schema.B0_ReferenceIDMaxLength, billArrivalP5.B0_ReferenceIDInfo.MaxLength);
				AssertEquals("Max length is 50 in Phase 4 and Departure", AutoCusInBondBill.Schema.B0_ReferenceIDMaxLength, billDepartureP4.B0_ReferenceIDInfo.MaxLength);
				AssertEquals("Max length is 50 in Phase 4 and Arrival", AutoCusInBondBill.Schema.B0_ReferenceIDMaxLength, billArrivalP4.B0_ReferenceIDInfo.MaxLength);
			});
		}

		public void TestB0_ReferenceID_Caption_Departure()
		{
			var phase4Bill = CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Departure);

			NCTSTestHelper.AssertCaptions(phase4Bill.B0_ReferenceIDInfo
				, multipleResourceKey: NctsHeader.Phase4CaptionKey
				, expectedCaption: "Reference Number UCR"
				, expectedMediumCaption: "Ref. No. UCR"
				, expectedShortCaption: "UCR");
			var phase5Bill = Phase5DepartureNctsBill;

			NCTSTestHelper.AssertCaptionsAndFullDescription(phase5Bill.B0_ReferenceIDInfo
				, multipleResourceKey: NctsHeader.Phase5DepartureCaptionKey
				, expectedCaption: "Reference Number / UCR"
				, expectedMediumCaption: string.Empty
				, expectedShortCaption: "Ref. No. / UCR"
				, expectedFullDescription: "Indicate the Reference Number / Unique Consignment Reference (UCR)");
		}

		public void TestB0_TransportPaymentMethod_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(Phase5DepartureNctsBill.B0_TransportPaymentMethodInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Transport MoP", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Transp. MoP", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "MoP", captionResourceString.ShortCaption);
			});
		}

		public void TestB0_TransportPaymentMethod_ClearBY_TransportChargesMethodOfPayment_WhenC0337Active()
		{
			var bill = Phase5DepartureNctsBill;
			var goodsItem = bill.GoodsItems.AddNew();

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0337Active)))
			{
				CombineAssertions(() =>
				{
					goodsItem.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
					bill.B0_TransportPaymentMethod = ZString.Empty;
					AssertEquals($"Bill B0_TransportPaymentMethod {bill.B0_TransportPaymentMethod}, value", TransportChargesModeOfPayment.Codes.Cash, goodsItem.BY_TransportChargesMethodOfPayment);

					goodsItem.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
					bill.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;
					AssertEquals($"Bill B0_TransportPaymentMethod {bill.B0_TransportPaymentMethod}, value", ZString.Empty, goodsItem.BY_TransportChargesMethodOfPayment);
				});
			}
		}

		public void TestB0_TransportPaymentMethod_ClearBY_TransportChargesMethodOfPayment_WhenC0337NotActive()
		{
			var bill = Phase5DepartureNctsBill;
			var goodsItem = bill.GoodsItems.AddNew();

			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0337Active)))
			{
				goodsItem.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;
				bill.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;
				AssertEquals(TransportChargesModeOfPayment.Codes.Cash, goodsItem.BY_TransportChargesMethodOfPayment);
			}
		}

		public void TestB0_TransportPaymentMethod_ReadOnly_WhenC0186Active()
		{
			var bill = Phase5DepartureNctsBill;
			var movementHeader = bill.Header.MovementHeader;
			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0186Active)))
			{
				CombineAssertions(() =>
				{
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
					AssertEquals("Type Of Security is ENT.", false, bill.B0_TransportPaymentMethodInfo.ReadOnly);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
					AssertEquals("Type Of Security is NON.", true, bill.B0_TransportPaymentMethodInfo.ReadOnly);
				});
			}
		}

		public void TestB0_TransportPaymentMethod_ReadOnly_WhenC0186NotActive()
		{
			var bill = Phase5DepartureNctsBill;
			var movementHeader = bill.Header.MovementHeader;
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0186Active)))
			{
				AssertEquals(false, bill.B0_TransportPaymentMethodInfo.ReadOnly);
			}
		}

		public void TestB0_TransportPaymentMethod_ReadOnly_Phase5Arrival()
		{
			AssertEquals("TypePhase5ArrivalNctsBill doesn't have movement header.", false, Phase5ArrivalNctsBill.B0_TransportPaymentMethodInfo.ReadOnly);
		}

		public void TestIShortSequenceNumberLine()
		{
			var nctsBill = Phase5DepartureNctsBill;
			var shortSequenceNumberLine = (IShortSequenceNumberLine)nctsBill;
			CombineAssertions(() =>
			{
				AssertEquals("FKToHeader", nctsBill.B0_BH, shortSequenceNumberLine.FKToHeader);
				AssertEquals("SequenceNumber", nctsBill.SequenceNumber, shortSequenceNumberLine.SequenceNumber);
			});
		}

		public void TestIDocAddressesMembers()
		{
			CombineAssertions(() =>
			{
				var docAddresses = Phase5DepartureNctsBill as IDocAddresses;
				AssertNotNull("NctsBill as IDocAddresses", docAddresses);

				var jobDocAddresses = Factory.New<JobDocAddress>();
				AssertNull("PiggyBackedDocAddressValidation", docAddresses.PiggyBackedDocAddressValidation(jobDocAddresses));
				AssertEquals("GetCanOverrideCheckpoint", Environment.Env.Security.None, docAddresses.GetCanOverrideCheckpoint(jobDocAddresses));
				AssertEquals("CanDeleteAddress", true, docAddresses.CanDeleteAddress(jobDocAddresses));
				AssertType<OrgHeaderCollection>("GetOrgHeaderList", docAddresses.GetOrgHeaderList(DocAddressType.ConsignorDocumentaryAddress));
				AssertSequencesEqual("SupportedAddressTypes", new DocAddressType[] { DocAddressType.ConsignorDocumentaryAddress, DocAddressType.ConsigneeAddress }, docAddresses.SupportedAddressTypes);
			});
		}

		public void TestDocAddresses()
		{
			CombineAssertions(() =>
			{
				var docAddresses = Phase5DepartureNctsBill.DocAddresses;
				AssertNotNull("DocAddresses", docAddresses);
				AssertSame("DocAddresses cached", docAddresses, Phase5DepartureNctsBill.DocAddresses);
			});
		}

		public void TestConsignor()
		{
			var nctsBill = Phase5DepartureNctsBill;
			var oldConsignor = nctsBill.Consignor;
			oldConsignor.Delete();

			CombineAssertions(() =>
			{
				var consignor = nctsBill.Consignor;
				AssertNotEquals("New Consignor created", oldConsignor.PK, consignor.PK);
				AssertSame("Cached", consignor, nctsBill.Consignor);

				consignor.Delete();
				var consignor2 = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsignorJobDocAddressRequirement);
				AssertEquals("Consignor from DocAddresses", consignor2.PK, nctsBill.Consignor.PK);
			});
		}

		public void TestConsignorAdditionalValidation()
		{
			AssertNull(Phase5DepartureNctsBill.Consignor.AdditionalValidation);
		}

		public void TestConsignorJobDocAddressRequirement()
		{
			var nctsBill = Phase5DepartureNctsBill;
			CombineAssertions(() =>
			{
				AssertNotNull("ConsignorJobDocAddressRequirement", nctsBill.ConsignorJobDocAddressRequirement);
				var consignorJobDocAddressRequirement = nctsBill.ConsignorJobDocAddressRequirement;
				AssertSame("ConsignorJobDocAddressRequirement Cached", consignorJobDocAddressRequirement, nctsBill.ConsignorJobDocAddressRequirement);
			});
		}

		public void TestConsignorJobDocAddressRequirement_ValidateOrganisationPK_WhenRuleE1301Active()
		{
			var expectedError = "[E1301] In transition period, which is now, Consignor must be empty";
			var nctsHeader = CreateNctsHeaderToTest(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			var org = Factory.New<OrgHeader>();

			var bill = nctsHeader.Bills.AddNew();

			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				deciderTestContext.EnableRule(c => c.IsRuleE1301Active);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
					Enterprise.Customs.Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod,
					RefDataGroupingCodes.EuropeanUnionEUN,
					ZDate.Today,
					true))
				{
					bill.Consignor.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("When Consignor is empty", bill.Consignor.OrganisationPKInfo, expectedError);

					bill.Consignor.OrganisationPK = org.PK;
					AssertHasMessageError("When Consignor is filled", bill.Consignor.OrganisationPKInfo, expectedError);
				}
			});
		}

		public void TestConsignorJobDocAddressRequirement_ValidateOrganisationPK_WhenRuleE1301Inactive()
		{
			var expectedError = "[E1301] In transition period, which is now, Consignor must be empty";
			var nctsHeader = CreateNctsHeaderToTest(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			var org = Factory.New<OrgHeader>();

			var bill = nctsHeader.Bills.AddNew();

			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
					Enterprise.Customs.Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod,
					RefDataGroupingCodes.EuropeanUnionEUN,
					ZDate.Today,
					true))
				{
					deciderTestContext.EnableRule(c => c.IsRuleE1301Active);
					bill.Consignor.OrganisationPK = org.PK;
					AssertHasMessageError("When Consignor is filled", bill.Consignor.OrganisationPKInfo, expectedError);

					deciderTestContext.DisableRule(c => c.IsRuleE1301Active);
					bill.Consignor.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When Consignor is filled but rule E1301_1 is not active", bill.Consignor.OrganisationPKInfo, expectedError);
				}
			});
		}

		public void TestConsignorJobDocAddressRequirement_ValidateOrganisationPK_R0506()
		{
			var expectedError = "[R0506] Consignor must be different for at least one of the house consignment.";
			var nctsHeader = CreateNctsHeaderToTest(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			var org = Factory.New<OrgHeader>();

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.Consignor.OrganisationPK = org.PK;

			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				deciderTestContext.EnableRule(c => c.IsRuleR0506Active);

				AssertNoMessageError("Single bill", bill1.Consignor.OrganisationPKInfo, expectedError);

				var bill2 = nctsHeader.Bills.AddNew();
				bill1.Consignor.OrganisationPK = ZGuid.Empty;
				bill2.Consignor.OrganisationPK = ZGuid.Empty;
				AssertNoMessageError("Empty Consignor", bill2.Consignor.OrganisationPKInfo, expectedError);

				bill1.Consignor.OrganisationPK = org.PK;
				var org2 = Factory.New<OrgHeader>();
				bill2.Consignor.OrganisationPK = org2.PK;
				AssertNoMessageError("Bills with different Consignor", bill2.Consignor.OrganisationPKInfo, expectedError);

				bill2.Consignor.OrganisationPK = org.PK;
				AssertHasMessageError("Bills with the same Consignor", bill2.Consignor.OrganisationPKInfo, expectedError);

				deciderTestContext.DisableRule(c => c.IsRuleR0506Active);
				bill2.Consignor.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Bills with the same Consignor", bill2.Consignor.OrganisationPKInfo, expectedError);
			});
		}

		public void TestConsignee()
		{
			var nctsBill = Phase5DepartureNctsBill;
			var oldConsignee = nctsBill.Consignee;
			oldConsignee.Delete();

			CombineAssertions(() =>
			{
				var consignee = nctsBill.Consignee;
				AssertNotEquals("New Consignee created", oldConsignee.PK, consignee.PK);
				AssertSame("Cached", consignee, nctsBill.Consignee);

				consignee.Delete();
				var consignee2 = nctsBill.DocAddresses.CreateWithRequirement(nctsBill.ConsigneeJobDocAddressRequirement);
				AssertEquals("Consignee from DocAddresses", consignee2.PK, nctsBill.Consignee.PK);
			});
		}

		public void TestConsigneeAdditionalValidation()
		{
			AssertType<NctsBillConsigneeJobDocAddressValidation>(Phase5DepartureNctsBill.Consignee.AdditionalValidation);
		}

		public void TestConsigneeJobDocAddressRequirement()
		{
			var nctsBill = Phase5DepartureNctsBill;
			CombineAssertions(() =>
			{
				AssertNotNull("ConsigneeJobDocAddressRequirement", nctsBill.ConsigneeJobDocAddressRequirement);
				var consigneeJobDocAddressRequirement = nctsBill.ConsigneeJobDocAddressRequirement;
				AssertSame("ConsigneeJobDocAddressRequirement Cached", consigneeJobDocAddressRequirement, nctsBill.ConsigneeJobDocAddressRequirement);
			});
		}

		public void TestConsigneeJobDocAddressRequirement_ValidateOrganisationPK_WhenC0001_1_TPOff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			var message = $"[{ValidationRuleCodeConstants.C0001_1}] {MandatoryValidation.YouHaveNotEntered}";

			var bill = Phase5DepartureNctsBill;
			var nctsHeader = bill.Header;
			var movementHeader = nctsHeader.MovementHeader;
			var orgHeader = Factory.New<OrgHeader>();
			var goodsItem = bill.GoodsItems.AddNew();

			using var context = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory);
			context.ClearCachedValidationDecider(nctsHeader);
			context.EnableRule(r => r.IsRuleC0001_1Active);
			context.EnableRule(r => r.IsRuleB1823Active);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
			{
				CombineAssertions("When TP off", () =>
				{
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					bill.Consignee.Validation.ValidateOrganisationPK();
					AssertHasMessageErrorContaining("BM_TypeOfSecurity is EXI, no 30600 additional info", bill.Consignee.OrganisationPKInfo, message);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					var headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
					bill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("BM_TypeOfSecurity is EXI, has 30600 additional info on header level", bill.Consignee.OrganisationPKInfo, message);
					nctsHeader.AdditionalDocuments.RemoveAndDelete(headerAdditionalInfo);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					var billAdditionalInfo = Add30600AdditionalInfo(bill.AdditionalDocuments);
					bill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("BM_TypeOfSecurity is EXI, has 30600 additional info on bill level", bill.Consignee.OrganisationPKInfo, message);
					bill.AdditionalDocuments.RemoveAndDelete(billAdditionalInfo);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
					var goodItemAdditionalInfo = Add30600AdditionalInfo(goodsItem.AdditionalInfos);
					bill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("BM_TypeOfSecurity is EXI, has 30600 additional info on goods item level", bill.Consignee.OrganisationPKInfo, message);
					goodsItem.AdditionalInfos.RemoveAndDelete(goodItemAdditionalInfo);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
					bill.Consignee.Validation.ValidateOrganisationPK();
					AssertHasMessageErrorContaining("BM_TypeOfSecurity is BTH, no 30600 additional info", bill.Consignee.OrganisationPKInfo, message);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
					headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
					bill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("BM_TypeOfSecurity is BTH, has 30600 additional info on header level", bill.Consignee.OrganisationPKInfo, message);
					nctsHeader.AdditionalDocuments.RemoveAndDelete(headerAdditionalInfo);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
					billAdditionalInfo = Add30600AdditionalInfo(bill.AdditionalDocuments);
					bill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("BM_TypeOfSecurity is BTH, has 30600 additional info on bill level", bill.Consignee.OrganisationPKInfo, message);
					bill.AdditionalDocuments.RemoveAndDelete(billAdditionalInfo);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
					goodItemAdditionalInfo = Add30600AdditionalInfo(goodsItem.AdditionalInfos);
					bill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("BM_TypeOfSecurity is BTH, has 30600 additional info on goods item level", bill.Consignee.OrganisationPKInfo, message);
					goodsItem.AdditionalInfos.RemoveAndDelete(goodItemAdditionalInfo);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
					bill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("BM_TypeOfSecurity isn't EXI or BTH", bill.Consignee.OrganisationPKInfo, message);

					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
					bill.Consignee.Validation.ValidateOrganisationPK();
					AssertHasMessageErrorContaining("BM_RL_NKDestinationPort is C0009 code", bill.Consignee.OrganisationPKInfo, message);

					movementHeader.BM_RL_NKDestinationPort = ZString.Empty;
					bill.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Germany;
					bill.Consignee.Validation.ValidateOrganisationPK();
					AssertHasMessageErrorContaining("B0_RN_NKCountryOfDestination is C0009 code", bill.Consignee.OrganisationPKInfo, message);

					nctsHeader.Consignee.OrganisationPK = orgHeader.PK;
					bill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("header.Consignee isn't empty", bill.Consignee.OrganisationPKInfo, message);

					nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
					bill.Consignee.OrganisationPK = orgHeader.PK;
					AssertNoMessageErrorContaining("bill.Consignee isn't empty", bill.Consignee.OrganisationPKInfo, message);
				});
			}

			AdditionalInfo Add30600AdditionalInfo(ICusSupportingInfoCollection<AdditionalInfo> additionalInfoCollection)
			{
				var additionalInfo = additionalInfoCollection.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.CSI_Code = AdditionalDocumentTypes._30600;

				return additionalInfo;
			}
		}

		public void TestConsigneeJobDocAddressRequirement_ValidateOrganisationPK_WhenC0001_1_TPOn()
		{
			var bill = Phase5DepartureNctsBill;
			var movementHeader = bill.Header.MovementHeader;
			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;

			using (var context = new NctsHeaderValidationDeciderTestContext<INctsHeaderDeparturePhase5ValidationDecider>(Factory))
			{
				context.EnableRule(r => r.IsRuleC0001_1Active);
				context.EnableRule(r => r.IsRuleB1823Active);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					CombineAssertions("When TP on", () =>
					{
						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoNotifications("BM_TypeOfSecurity is EXI, no 30600 additional info", bill.Consignee.OrganisationPKInfo);

						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoNotifications("BM_TypeOfSecurity is BTH, no 30600 additional info", bill.Consignee.OrganisationPKInfo);

						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Australia;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoNotifications("BM_RL_NKDestinationPort is C0009 code", bill.Consignee.OrganisationPKInfo);

						movementHeader.BM_RL_NKDestinationPort = ZString.Empty;
						bill.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Germany;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoNotifications("B0_RN_NKCountryOfDestination is C0009 code", bill.Consignee.OrganisationPKInfo);
					});
				}
			}
		}

		public void TestConsigneeJobDocAddress_C0001_2_1_WhenActive() => CombineAssertions(() =>
		{
			var message = ValidationRuleMessages.C0001_2aMessage;
			var bill = Phase5DepartureNctsBill;
			var header = bill.Header;
			var address = Factory.New<JobDocAddress>();

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0001_2Active)))
			{
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Header Consignee empty, Consignment Consignee empty", bill.Consignee.OrganisationPKInfo, message);

				header.Consignee.E2_OA_Address = address.PK;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Header Consignee not empty, Consignment Consignee empty", bill.Consignee.OrganisationPKInfo, message);

				header.Consignee.E2_OA_Address = ZGuid.Empty;
				bill.Consignee.E2_OA_Address = address.PK;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Header Consignee empty, Consignment Consignee not empty", bill.Consignee.OrganisationPKInfo, message);

				header.Consignee.E2_OA_Address = address.PK;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Header Consignee not empty, Consignment Consignee not empty", bill.Consignee.OrganisationPKInfo, message);
			}
		});

		public void TestConsigneeJobDocAddress_C0001_2_2_SecurityType_WhenActive()
		{
			var message = ValidationRuleMessages.C0001_2bMessage;
			var bill = Phase5DepartureNctsBill;
			var header = bill.Header;
			var address = Factory.New<JobDocAddress>();

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0001_2Active)))
			{
				var movementHeader = header.MovementHeader;
				movementHeader.BM_TypeOfSecurity = string.Empty;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Header Consignee empty, Bill Consignee empty, Security Type empty", bill.Consignee.OrganisationPKInfo, message);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Header Consignee empty, Bill Consignee empty, Security Type NON", bill.Consignee.OrganisationPKInfo, message);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Header Consignee empty, Bill Consignee empty, Security Type ENT", bill.Consignee.OrganisationPKInfo, message);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Header Consignee empty, Bill Consignee empty, Security Type  not ENT/NON", bill.Consignee.OrganisationPKInfo, message);

				header.Consignee.E2_OA_Address = address.PK;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError($"Header Consignee not empty, Bill Consignee empty, Security Type BTH", bill.Consignee.OrganisationPKInfo, message);

				header.Consignee.E2_OA_Address = ZGuid.Empty;
				bill.Consignee.E2_OA_Address = address.PK;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError($"Header Consignee empty, Bill Consignee not empty, Security Type BTH", bill.Consignee.OrganisationPKInfo, message);

				header.Consignee.E2_OA_Address = address.PK;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError($"Header Consignee not empty, Bill Consignee not empty, Security Type BTH", bill.Consignee.OrganisationPKInfo, message);
			}
		}

		public void TestConsigneeJobDocAddress_C0001_2_2_DestinationPort_WhenActive()
		{
			var message = ValidationRuleMessages.C0001_2bMessage;
			var bill = Phase5DepartureNctsBill;
			var header = bill.Header;
			var address = Factory.New<JobDocAddress>();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var euctp = helper.CreateTradeGroup("EUN", "EUCTP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, Core.Constants.CountryCodes.Italy, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();

			using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRule(Factory, nameof(ValidationRuleConfiguration.IsRuleC0001_2Active)))
			{
				var movementHeader = header.MovementHeader;
				movementHeader.BM_TypeOfSecurity = string.Empty;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Header Consignee empty, Bill Consignee empty, DestinationPort empty", bill.Consignee.OrganisationPKInfo, message);

				movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
				header.Consignee.E2_OA_Address = ZGuid.Empty;
				bill.Consignee.E2_OA_Address = ZGuid.Empty;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertHasMessageError("Header Consignee empty, Bill Consignee empty, DestinationPort IsACountryEligibleToACommonTransitProcedure", bill.Consignee.OrganisationPKInfo, message);

				header.Consignee.E2_OA_Address = address.PK;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError($"Header Consignee not empty, Bill Consignee empty, DestinationPort IsACountryEligibleToACommonTransitProcedure", bill.Consignee.OrganisationPKInfo, message);

				header.Consignee.E2_OA_Address = ZGuid.Empty;
				bill.Consignee.E2_OA_Address = address.PK;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError($"Header Consignee empty, Bill Consignee not empty, DestinationPort IsACountryEligibleToACommonTransitProcedure", bill.Consignee.OrganisationPKInfo, message);

				header.Consignee.E2_OA_Address = address.PK;
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError($"Header Consignee not empty, Bill Consignee not empty, DestinationPort IsACountryEligibleToACommonTransitProcedure", bill.Consignee.OrganisationPKInfo, message);
			}
		}

		public void TestConsigneeJobDocAddress_C0001_2_WhenInactive() => CombineAssertions(() =>
		{
			var message1 = ValidationRuleMessages.C0001_2aMessage;
			var message2 = ValidationRuleMessages.C0001_2bMessage;
			var bill = Phase5DepartureNctsBill;

			ValidationRuleConfigurationTestHelper.AssertNoNotificationsWithInactiveRule(Factory, bill.Consignee.OrganisationPKInfo, message1, nameof(ValidationRuleConfiguration.IsRuleC0001_2Active), () => bill.Consignee.Validation.ValidateOrganisationPK());
			ValidationRuleConfigurationTestHelper.AssertNoNotificationsWithInactiveRule(Factory, bill.Consignee.OrganisationPKInfo, message2, nameof(ValidationRuleConfiguration.IsRuleC0001_2Active), () => bill.Consignee.Validation.ValidateOrganisationPK());
		});

		public void TestConsigneeJobDocAddress_WhenDestinationPortInCL009_RuleC0001_4()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			const string expectedMessageError = "[C0001-4] Consignee must be empty";

			var nctsBill = Phase5DepartureNctsBill;
			var nctsHeader = nctsBill.Header;
			var movementHeader = nctsHeader.MovementHeader;
			var address = Factory.New<JobDocAddress>();

			var propertyInfo = nctsBill.Consignee.OrganisationPKInfo;

			using (var context = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					CombineAssertions("Outside TP", () =>
					{
						context.EnableRule(r => r.IsRuleC0001_4Active);

						nctsBill.Consignee.E2_OA_Address = address.PK;
						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
						nctsHeader.Consignee.E2_OA_Address = address.PK;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining("Rule C0001-4 enabled, Header Consignee filled, DestinationPort filled, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						nctsBill.Consignee.E2_OA_Address = ZGuid.Empty;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, Header Consignee filled, DestinationPort filled, Bill Consignee empty", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						nctsBill.Consignee.E2_OA_Address = address.PK;
						nctsHeader.Consignee.E2_OA_Address = ZGuid.Empty;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, Header Consignee empty, DestinationPort filled, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						movementHeader.BM_RL_NKDestinationPort = ZString.Empty;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, Header Consignee empty, DestinationPort empty, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						context.DisableRule(r => r.IsRuleC0001_4Active);

						nctsHeader.Consignee.E2_OA_Address = address.PK;
						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
						nctsBill.Consignee.E2_OA_Address = address.PK;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 disabled, Header Consignee filled, DestinationPort filled, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);
					});
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					context.EnableRule(r => r.IsRuleC0001_4Active);

					nctsHeader.Consignee.E2_OA_Address = address.PK;
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Italy;
					nctsBill.Consignee.E2_OA_Address = address.PK;
					nctsBill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("During TP, Rule C0001-4 enabled, Header Consignee filled, DestinationPort filled, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);
				}
			}
		}

		public void TestConsigneeJobDocAddress_WhenTypeOfSecurityIsNONOrENT_RuleC0001_4()
		{
			const string expectedMessageError = "[C0001-4] Consignee must be empty";

			var nctsBill = Phase5DepartureNctsBill;
			var nctsHeader = nctsBill.Header;
			var movementHeader = nctsHeader.MovementHeader;
			var address = Factory.New<JobDocAddress>();

			var propertyInfo = nctsBill.Consignee.OrganisationPKInfo;

			using (var context = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					CombineAssertions("Outside TP", () =>
					{
						context.EnableRule(r => r.IsRuleC0001_4Active);

						nctsBill.Consignee.E2_OA_Address = address.PK;
						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
						nctsHeader.Consignee.E2_OA_Address = address.PK;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining("Rule C0001-4 enabled, Header Consignee filled, Type of Security is ENT, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
						AssertHasMessageErrorContaining("Rule C0001-4 enabled, Header Consignee filled, Type of Security is NON, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						nctsBill.Consignee.E2_OA_Address = ZGuid.Empty;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, Header Consignee filled, Type of Security is ENT, Bill Consignee empty", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						nctsBill.Consignee.E2_OA_Address = address.PK;
						nctsHeader.Consignee.E2_OA_Address = ZGuid.Empty;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, Header Consignee empty, Type of Security is ENT, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, Header Consignee empty, Type of Security is BTH, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						context.DisableRule(r => r.IsRuleC0001_4Active);

						nctsHeader.Consignee.E2_OA_Address = address.PK;
						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 disabled, Header Consignee filled, Type of Security is ENT, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);
					});
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					context.EnableRule(r => r.IsRuleC0001_4Active);

					nctsHeader.Consignee.E2_OA_Address = address.PK;
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
					nctsBill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("During TP, Rule C0001-4 enabled, Header Consignee filled, Type of Security is ENT, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);
				}
			}
		}

		public void TestConsigneeJobDocAddress_WithSecurityTypeDestinationCountryAndAdditionalDocuments_RuleC0001_6()
		{
			const string expectedWarning = "[C0001-6] This field will not be written in the message";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009);
			Factory.Save();

			var nctsBill = Phase5DepartureNctsBill;
			var nctsHeader = nctsBill.Header;
			var movementHeader = nctsHeader.MovementHeader;
			var consignee = Factory.New<OrgHeader>();

			var propertyInfo = nctsBill.Consignee.OrganisationPKInfo;

			using var context = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
			{
				context.EnableRule(r => r.IsRuleC0001_6Active);

				CombineAssertions("Rule C0001-6 active, outside TP", () =>
				{
					movementHeader.BM_TypeOfSecurity = "BTH";
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.India;
					Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
					nctsBill.Consignee.OrganisationPK = consignee.PK;
					AssertHasWarning("When BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, Header has AdditionalDocuments, NctsBill consignee filled", propertyInfo, expectedWarning);

					movementHeader.BM_RL_NKDestinationPort = CountryCodes.Australia;
					nctsBill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoWarning("When BM_TypeOfSecurity = 'BTH', DestinationPort is in C0009, Header has AdditionalDocuments, NctsBill consignee filled", propertyInfo, expectedWarning);

					movementHeader.BM_RL_NKDestinationPort = CountryCodes.India;
					movementHeader.BM_TypeOfSecurity = "ENT";
					nctsBill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoWarning("When BM_TypeOfSecurity = 'ENT', DestinationPort not in C0009, Header has AdditionalDocuments, NctsBill consignee filled", propertyInfo, expectedWarning);

					movementHeader.BM_TypeOfSecurity = "BTH";
					nctsBill.Consignee.OrganisationPK = ZGuid.Empty;
					AssertNoWarning("When BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, Header has AdditionalDocuments, NctsBill consignee empty", propertyInfo, expectedWarning);

					nctsHeader.AdditionalDocuments.RemoveAndDeleteAll();
					nctsBill.Consignee.OrganisationPK = consignee.PK;
					AssertNoWarning("When BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, Header not contains AdditionalDocuments, NctsBill consignee filled", propertyInfo, expectedWarning);

					movementHeader.BM_RL_NKDestinationPort = CountryCodes.India;
					movementHeader.BM_TypeOfSecurity = "BTH";
					Add30600AdditionalInfo(nctsBill.AdditionalDocuments);
					nctsBill.Consignee.Validation.ValidateOrganisationPK();
					AssertHasWarning("When BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, NctsBill has AdditionalDocuments, NctsBill consignee filled", propertyInfo, expectedWarning);
				});

				context.DisableRule(r => r.IsRuleC0001_6Active);

				movementHeader.BM_TypeOfSecurity = "BTH";
				movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.India;
				Add30600AdditionalInfo(nctsBill.AdditionalDocuments);
				nctsBill.Consignee.OrganisationPK = consignee.PK;
				nctsBill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoWarning("When Rule C0001-6 disabled, BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, NctsBill has AdditionalDocuments, NctsBill consignee filled", propertyInfo, expectedWarning);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
			{
				context.EnableRule(r => r.IsRuleC0001_6Active);

				movementHeader.BM_TypeOfSecurity = "BTH";
				movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.India;
				Add30600AdditionalInfo(nctsBill.AdditionalDocuments);
				nctsBill.Consignee.OrganisationPK = consignee.PK;
				nctsBill.Consignee.Validation.ValidateOrganisationPK();

				AssertNoWarning("During TP, When Rule C0001-6 enabled, BM_TypeOfSecurity = 'BTH', DestinationPort not in C0009, NctsBill has AdditionalDocuments, NctsBill consignee filled", propertyInfo, expectedWarning);
			}
		}

		public void TestCheckConsignee_WhenHeaderAndAllBillsEmpty_RuleC0001_6()
		{
			var expectedMessage = "[C0001-6] A Consignee Address: Organization must be declared at header or house level.";

			var nctsBill = Phase5DepartureNctsBill;
			var nctsHeader = nctsBill.Header;
			var movementHeader = nctsHeader.MovementHeader;
			var nctsBill2 = nctsHeader.Bills.AddNew();
			var consignee = Factory.New<OrgHeader>();

			using (var context = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					context.EnableRule(r => r.IsRuleC0001_6Active);

					CombineAssertions("Rule C0001-6 active, outside TP", () =>
					{
						movementHeader.BM_TypeOfSecurity = "BTH";
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						nctsBill2.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageError("When Header and all Bills consignee empty - Bill 1", nctsBill.Consignee.OrganisationPKInfo, expectedMessage);
						AssertHasMessageError("When Header and all Bills consignee empty - Bill 2", nctsBill2.Consignee.OrganisationPKInfo, expectedMessage);

						nctsBill.Consignee.OrganisationPK = consignee.PK;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						nctsBill2.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("When Header empty and Bill 1 consignee filled - Bill 1", nctsBill.Consignee.OrganisationPKInfo, expectedMessage);
						AssertNoMessageError("When Header empty and Bill 1 consignee filled - Bill 2", nctsBill2.Consignee.OrganisationPKInfo, expectedMessage);

						nctsHeader.Consignee.OrganisationPK = consignee.PK;
						nctsBill2.Consignee.OrganisationPK = consignee.PK;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("When Header and all Bills consignee filled - Bill 1", nctsBill.Consignee.OrganisationPKInfo, expectedMessage);
						AssertNoMessageError("When Header and all Bills consignee filled - Bill 2", nctsBill2.Consignee.OrganisationPKInfo, expectedMessage);
					});

					context.DisableRule(c => c.IsRuleC0001_6Active);

					nctsBill.Consignee.OrganisationPK = ZGuid.Empty;
					nctsBill2.Consignee.OrganisationPK = ZGuid.Empty;
					nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
					nctsBill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When rule C0001-6 disabled, Header empty and all Bills empty - Bill 1", nctsBill.Consignee.OrganisationPKInfo, expectedMessage);
					AssertNoMessageError("When rule C0001-6 disabled, Header empty and all Bills empty - Bill 2", nctsBill2.Consignee.OrganisationPKInfo, expectedMessage);
				}
			}
		}

		public void TestCheckConsignee_WhenHeaderEmptyAndSomeBillsFilled_RuleC0001_6()
		{
			var expectedMessage = "[C0001-6] You have not entered a Consignee Address: Organization.";

			var nctsBill = Phase5DepartureNctsBill;
			var nctsHeader = nctsBill.Header;
			var movementHeader = nctsHeader.MovementHeader;
			var nctsBill2 = nctsHeader.Bills.AddNew();
			var consignee = Factory.New<OrgHeader>();

			using (var context = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					context.EnableRule(r => r.IsRuleC0001_6Active);

					CombineAssertions("Rule C0001-6 active, outside TP", () =>
					{
						movementHeader.BM_TypeOfSecurity = "BTH";
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						nctsBill2.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("When Header and all Bills consignee empty - Bill 1", nctsBill.Consignee.OrganisationPKInfo, expectedMessage);
						AssertNoMessageError("When Header and all Bills consignee empty - Bill 2", nctsBill2.Consignee.OrganisationPKInfo, expectedMessage);

						nctsBill.Consignee.OrganisationPK = consignee.PK;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						nctsBill2.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("When Header empty and a Bill 1 consignee filled - Bill 1", nctsBill.Consignee.OrganisationPKInfo, expectedMessage);
						AssertHasMessageError("When Header empty and a Bill 1 consignee filled - Bill 2", nctsBill2.Consignee.OrganisationPKInfo, expectedMessage);

						nctsBill2.Consignee.OrganisationPK = consignee.PK;
						nctsBill2.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("When Header empty and all Bills consignee filled - Bill 1", nctsBill.Consignee.OrganisationPKInfo, expectedMessage);
						AssertNoMessageError("When Header empty and all Bill consignee filled - Bill 2", nctsBill2.Consignee.OrganisationPKInfo, expectedMessage);

						nctsHeader.Consignee.OrganisationPK = consignee.PK;
						nctsBill.Consignee.OrganisationPK = consignee.PK;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						nctsBill2.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("When Header filled and Bill 1 consignee filled - Bill 1", nctsBill.Consignee.OrganisationPKInfo, expectedMessage);
						AssertNoMessageError("When Header filled and  Bill 1 consignee filled - Bill 2", nctsBill2.Consignee.OrganisationPKInfo, expectedMessage);
					});

					context.DisableRule(c => c.IsRuleC0001_6Active);

					nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;
					nctsBill.Consignee.OrganisationPK = consignee.PK;
					nctsBill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageError("When Rule C0001-6 disabled, Header empty and Bill 1 consignee filled - Bill 1", nctsBill.Consignee.OrganisationPKInfo, expectedMessage);
					AssertNoMessageError("When Rule C0001-6 disabled, Header empty and Bill 1 consignee filled - Bill 2", nctsBill2.Consignee.OrganisationPKInfo, expectedMessage);
				}
			}
		}

		public void TestConsigneeJobDocAddress_WhenAdditionalDocumentType30600_RuleC0001_4Active()
		{
			const string expectedMessageError = "[C0001-4] Consignee must be empty";

			var nctsBill = Phase5DepartureNctsBill;
			var nctsHeader = nctsBill.Header;
			var movementHeader = nctsHeader.MovementHeader;
			var address = Factory.New<JobDocAddress>();

			var nctsBill2 = CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure);

			var propertyInfo = nctsBill.Consignee.OrganisationPKInfo;

			using (var context = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					CombineAssertions("Outside TP", () =>
					{
						context.EnableRule(r => r.IsRuleC0001_4Active);

						nctsBill.Consignee.E2_OA_Address = address.PK;
						var headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining("Rule C0001-4 enabled, Header has additional documents type 30600, Bill1 Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);
						nctsHeader.AdditionalDocuments.RemoveAndDelete(headerAdditionalInfo);

						var billAdditionalInfo = Add30600AdditionalInfo(nctsBill.AdditionalDocuments);
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining("Rule C0001-4 enabled, Bill1 has additional documents type 30600, Bill1 Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						nctsBill2.Consignee.E2_OA_Address = address.PK;
						nctsBill2.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, Bill1 has additional documents type 30600, Bill2 Consignee filled and has no additional documents type 30600", nctsBill2.Consignee.OrganisationPKInfo, expectedMessageError);

						nctsBill.Consignee.E2_OA_Address = ZGuid.Empty;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, Bill1 has additional documents type 30600, Bill1 Consignee empty", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);
						nctsBill.AdditionalDocuments.RemoveAndDelete(billAdditionalInfo);

						nctsBill.Consignee.E2_OA_Address = address.PK;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, No Additional documents, Bill1 Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						context.DisableRule(r => r.IsRuleC0001_4Active);

						nctsBill.Consignee.E2_OA_Address = address.PK;
						headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 disabled, Header has additional documents type 30600, Bill1 Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);
						nctsHeader.AdditionalDocuments.RemoveAndDelete(headerAdditionalInfo);

						nctsBill.Consignee.E2_OA_Address = address.PK;
						billAdditionalInfo = Add30600AdditionalInfo(nctsBill.AdditionalDocuments);
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 disabled, Bill1 has additional documents type 30600, Bill1 Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);
					});
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					context.EnableRule(r => r.IsRuleC0001_4Active);

					nctsBill.Consignee.E2_OA_Address = address.PK;
					var headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
					nctsBill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("During TP, Rule C0001-4 enabled, Header has additional documents type 30600, Bill1 Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);
				}
			}
		}

		public void TestConsigneeJobDocAddress_WhenHeaderConsigneeNotEmpty_RuleC0001_4Active()
		{
			const string expectedMessageError = "[C0001-4] Consignee must be empty";

			var nctsBill = Phase5DepartureNctsBill;
			var nctsHeader = nctsBill.Header;
			var movementHeader = nctsHeader.MovementHeader;
			var address = Factory.New<JobDocAddress>();

			var propertyInfo = nctsBill.Consignee.OrganisationPKInfo;

			using (var context = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					CombineAssertions("Outside TP", () =>
					{
						context.EnableRule(r => r.IsRuleC0001_4Active);

						movementHeader.BM_TypeOfSecurity = ZString.Empty;

						nctsBill.Consignee.E2_OA_Address = address.PK;
						nctsHeader.Consignee.E2_OA_Address = address.PK;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining("Rule C0001-4 enabled, Header Consignee filled, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						nctsBill.Consignee.E2_OA_Address = ZGuid.Empty;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, Header Consignee filled, Bill Consignee empty", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						nctsBill.Consignee.E2_OA_Address = address.PK;
						nctsHeader.Consignee.E2_OA_Address = ZGuid.Empty;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 enabled, Header Consignee empty, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						nctsBill.Consignee.E2_OA_Address = ZGuid.Empty;
						nctsHeader.Consignee.E2_OA_Address = ZGuid.Empty;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Header Consignee empty, Bill Consignee empty", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);

						context.DisableRule(r => r.IsRuleC0001_4Active);

						nctsHeader.Consignee.E2_OA_Address = address.PK;
						nctsBill.Consignee.E2_OA_Address = address.PK;
						nctsBill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Rule C0001-4 disabled, Header Consignee filled, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);
					});
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					context.EnableRule(r => r.IsRuleC0001_4Active);

					nctsHeader.Consignee.E2_OA_Address = address.PK;
					nctsBill.Consignee.E2_OA_Address = address.PK;
					nctsBill.Consignee.Validation.ValidateOrganisationPK();
					AssertNoMessageErrorContaining("During TP, Rule C0001-4 enabled, Header Consignee filled, Bill Consignee filled", nctsBill.Consignee.OrganisationPKInfo, expectedMessageError);
				}
			}
		}

		public void TestConsigneeJobDocAddress_RuleC0001_7()
		{
			const string messageError = "[C0001-7] You have not entered Consignee. It is required either on Declaration or House Consignment";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementHeader = nctsHeader.MovementHeader;
			var org = Factory.New<OrgHeader>();

			var bill = nctsHeader.Bills.AddNew();
			var consignee = bill.Consignee;
			var propertyInfo = consignee.OrganisationPKInfo;
			var additionalDocument = bill.AdditionalDocuments.AddNew();

			var bill2 = nctsHeader.Bills.AddNew();
			var additionalDocument2 = bill2.AdditionalDocuments.AddNew();

			using (var context = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					CombineAssertions("Outside TP", () =>
					{
						context.EnableRule(r => r.IsRuleC0001_7Active);
						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.India;
						additionalDocument2.CSI_Code = "30600";
						additionalDocument2.CSI_SubType = "INF";

						consignee.OrganisationPK = ZGuid.Empty;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageErrorContaining("current House Consignment has not 30600 and another house Consignment has 30600 and current house consignee is empty", propertyInfo, messageError);

						var headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
						additionalDocument2.CSI_Code = "30600";
						consignee.OrganisationPK = ZGuid.Empty;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("header's additional document has type 30600", propertyInfo, messageError);
						nctsHeader.AdditionalDocuments.RemoveAndDelete(headerAdditionalInfo);

						var address = Factory.New<JobDocAddress>();
						nctsHeader.Consignee.OrganisationPK = org.PK;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Header Consignee is not empty, No error expected", propertyInfo, messageError);
						nctsHeader.Consignee.OrganisationPK = ZGuid.Empty;

						consignee.OrganisationPK = ZGuid.Empty;
						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
						AssertNoMessageErrorContaining("Type of Security is NON, no validation error expected", propertyInfo, messageError);
						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;

						consignee.OrganisationPK = org.PK;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Current House Consignment has not 30600 and another house Consignment has 30600 and current house consignee is not empty", propertyInfo, messageError);

						additionalDocument2.CSI_Code = "12345";
						consignee.OrganisationPK = ZGuid.Empty;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Another house instance Additional Document Type is not 30600, no validation error expected", propertyInfo, messageError);

						context.DisableRule(r => r.IsRuleC0001_7Active);
						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.India;
						additionalDocument2.CSI_Code = "30600";
						additionalDocument2.CSI_SubType = "INF";
						consignee.OrganisationPK = ZGuid.Empty;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("C0001-7 Disabled, no error expected", propertyInfo, messageError);
					});
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					CombineAssertions("During TP", () =>
					{
						context.EnableRule(r => r.IsRuleC0001_7Active);
						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.India;
						additionalDocument2.CSI_Code = "30600";
						additionalDocument2.CSI_SubType = "INF";

						var headerAdditionalInfo = Add30600AdditionalInfo(nctsHeader.AdditionalDocuments);
						consignee.OrganisationPK = ZGuid.Empty;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("header's additional document has type 30600", propertyInfo, messageError);
						nctsHeader.AdditionalDocuments.RemoveAndDelete(headerAdditionalInfo);

						consignee.OrganisationPK = ZGuid.Empty;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("current House Consignment has not 30600 and another house Consignment has 30600 and current house consignee is empty", propertyInfo, messageError);

						consignee.OrganisationPK = org.PK;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("Current House has Consignment not 30600 and another house Consignment has 30600 and current house consignee is not empty", propertyInfo, messageError);

						context.DisableRule(r => r.IsRuleC0001_7Active);
						movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.India;
						additionalDocument2.CSI_Code = "30600";
						additionalDocument2.CSI_SubType = "INF";
						consignee.OrganisationPK = ZGuid.Empty;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageErrorContaining("C0001-7 Disabled, no error expected", propertyInfo, messageError);
					});
				}
			}
			AdditionalInfo Add30600AdditionalInfo(ICusSupportingInfoCollection<AdditionalInfo> additionalInfoCollection)
			{
				var additionalInfo = additionalInfoCollection.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.CSI_Code = AdditionalDocumentTypes._30600;

				return additionalInfo;
			}
		}

		public void TestConsigneeJobDocAddressRequirement_ValidateOrganisationPK_WhenRuleE1301Active()
		{
			var expectedError = "[E1301] In transition period, which is now, Consignee must be empty";
			var nctsHeader = CreateNctsHeaderToTest(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			var org = Factory.New<OrgHeader>();

			var bill = nctsHeader.Bills.AddNew();

			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
					Enterprise.Customs.Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod,
					RefDataGroupingCodes.EuropeanUnionEUN,
					ZDate.Today,
					true))
				{
					deciderTestContext.EnableRule(e => e.IsRuleE1301Active);
					bill.Consignee.OrganisationPK = ZGuid.Empty;
					AssertNoMessageError("When Consignee is empty", bill.Consignee.OrganisationPKInfo, expectedError);

					bill.Consignee.OrganisationPK = org.PK;
					AssertHasMessageError("When Consignee is filled", bill.Consignee.OrganisationPKInfo, expectedError);
				}
			});
		}

		public void TestConsigneeJobDocAddressRequirement_ValidateOrganisationPK_WhenRuleE1301Inactive()
		{
			var expectedError = "[E1301] In transition period, which is now, Consignee must be empty";
			var nctsHeader = CreateNctsHeaderToTest(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			var org = Factory.New<OrgHeader>();

			var bill = nctsHeader.Bills.AddNew();

			using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Enterprise.Customs.Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod,
				RefDataGroupingCodes.EuropeanUnionEUN,
				ZDate.Today,
				true))
			{
				deciderTestContext.EnableRule(e => e.IsRuleE1301Active);
				bill.Consignee.OrganisationPK = org.PK;
				AssertHasMessageError("When Consignee is filled", bill.Consignee.OrganisationPKInfo, expectedError);

				deciderTestContext.DisableRule(e => e.IsRuleE1301Active);
				bill.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError("When Consignee is filled but rule E1301_1 is not active", bill.Consignee.OrganisationPKInfo, expectedError);
			}
		}

		public void TestConsigneeJobDocAddressRequirement_ValidateOrganisationPK_R0506()
		{
			var expectedError = "[R0506] Consignee must be different for at least one of the house consignment.";
			var nctsHeader = CreateNctsHeaderToTest(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			var org = Factory.New<OrgHeader>();

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.Consignee.OrganisationPK = org.PK;

			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				deciderTestContext.EnableRule(c => c.IsRuleR0506Active);

				AssertNoMessageError("Single bill", bill1.Consignee.OrganisationPKInfo, expectedError);

				var bill2 = nctsHeader.Bills.AddNew();
				bill1.Consignee.OrganisationPK = ZGuid.Empty;
				bill2.Consignee.OrganisationPK = ZGuid.Empty;
				AssertNoMessageError("Empty Consignee", bill2.Consignee.OrganisationPKInfo, expectedError);

				bill1.Consignee.OrganisationPK = org.PK;
				var org2 = Factory.New<OrgHeader>();
				bill2.Consignee.OrganisationPK = org2.PK;
				AssertNoMessageError("Bills with different Consignee", bill2.Consignee.OrganisationPKInfo, expectedError);

				bill2.Consignee.OrganisationPK = org.PK;
				AssertHasMessageError("Bills with the same Consignee", bill2.Consignee.OrganisationPKInfo, expectedError);

				deciderTestContext.DisableRule(c => c.IsRuleR0506Active);

				bill2.Consignee.Validation.ValidateOrganisationPK();
				AssertNoMessageError("Bills with the same Consignee", bill2.Consignee.OrganisationPKInfo, expectedError);
			});
		}

		public void TestConsignee_ValidateOrganisationPK_G0001_1()
		{
			const string messageError = "[G0001-1] Consignee must be empty";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var org = Factory.New<OrgHeader>();
			var bill = nctsHeader.Bills.AddNew();
			var consignee = bill.Consignee;
			var propertyInfo = consignee.OrganisationPKInfo;
			var additionalDocument = bill.AdditionalDocuments.AddNew();

			using (var context = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					CombineAssertions("Outside TP", () =>
					{
						context.EnableRule(r => r.IsRuleG0001_1Active);
						consignee.OrganisationPK = org.PK;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("G0001-1 rule is active and Bill consignee is filled", propertyInfo, messageError);

						consignee.OrganisationPK = ZGuid.Empty;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("G0001-1 rule is active and Bill consignee is empty", propertyInfo, messageError);

						consignee.OrganisationPK = org.PK;
						additionalDocument.CSI_Code = "30600";
						additionalDocument.CSI_SubType = "INF";
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertHasMessageError("G0001-1 rule is active and Bill consignee is filled and HouseConsignment has AdditionalDocument where Doc Kind = INF and Doc.Type = 30600", propertyInfo, messageError);

						context.DisableRule(r => r.IsRuleG0001_1Active);
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("G0001-1 rule is inactive and Bill consignee is filled and HouseConsignment has AdditionalDocument  where Doc Kind = INF and Doc.Type = 30600", propertyInfo, messageError);
					});
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					CombineAssertions("During TP", () =>
					{
						context.EnableRule(r => r.IsRuleG0001_1Active);
						consignee.OrganisationPK = org.PK;
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("G0001-1 rule is active and Bill consignee is filled", propertyInfo, messageError);

						consignee.OrganisationPK = org.PK;
						additionalDocument.CSI_Code = "30600";
						additionalDocument.CSI_SubType = "INF";
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("G0001-1 rule is active and Bill consignee is filled and HouseConsignment has AdditionalDocument where Doc Kind = INF and Doc.Type = 30600", propertyInfo, messageError);

						context.DisableRule(r => r.IsRuleG0001_1Active);
						bill.Consignee.Validation.ValidateOrganisationPK();
						AssertNoMessageError("G0001-1 rule is inactive and Bill consignee is filled and HouseConsignment has AdditionalDocument  where Doc Kind = INF and Doc.Type = 30600", propertyInfo, messageError);
					});
				}
			}
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)Phase5DepartureNctsBill).GetCusSupportingInfoTypes();
			CombineAssertions(() =>
			{
				AssertEquals("#CusSupportingInfoTypes", 3, cusSupportingInfoTypes.Count);
				AssertEquals("SUP", typeof(NctsSupportingDocument), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument]);
				AssertEquals("PRE", typeof(CommonPreviousDocument), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
				AssertEquals("OTH", typeof(NctsBillAdditionalDocument), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
			});
		}

		public void TestSupportingDocuments_Properties()
		{
			var nctsBill = Phase5DepartureNctsBill;
			var supportingDocuments = nctsBill.SupportingDocuments;
			CombineAssertions(() =>
			{
				AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>("Type", supportingDocuments);
				AssertEquals("IsRegisteredEditableChildObject", true, nctsBill.IsRegisteredEditableChildObject(supportingDocuments));
				AssertSame("Cached", supportingDocuments, nctsBill.SupportingDocuments);
				AssertEquals("IsLoaded", true, supportingDocuments.IsLoaded);
			});
		}

		public void TestSupportingDocuments()
		{
			var nctsBill = Phase5DepartureNctsBill;
			var supportingDocs = nctsBill.SupportingDocuments;
			CombineAssertions(() =>
			{
				AssertEquals("Initially SupportingDocuments is empty", 0, supportingDocs.Count);
				var doc1 = supportingDocs.AddNew();
				var doc2 = supportingDocs.AddNew();
				AssertEquals("SupportingDocuments now has 2 docs", 2, supportingDocs.Count);

				Factory.Save();
				var anotherFactory = new BusinessObjectFactory();
				var nctsBillInAnotherFactory = anotherFactory.Load<NctsBill>(nctsBill.PK);
				AssertContainsExactElementsInAnyOrder("Reload nctsBill, SupportingDocuments contains 2 docs", new[] { doc1.PK, doc2.PK }, nctsBillInAnotherFactory.SupportingDocuments.Select(x => x.PK));
			});
		}

		public void TestArrivalSupportingDocuments()
		{
			var nctsBill = Phase5ArrivalNctsBill;
			nctsBill.Header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			AssertType<NctsSupportingDocumentCollection<NctsSupportingDocument>>("Type", nctsBill.SupportingDocuments);
		}

		public void TestArrivalSupportingDocuments_Load()
		{
			var nctsBill = Phase5ArrivalNctsBill;
			CombineAssertions(() =>
			{
				nctsBill.Header.BH_HeaderType = NctsMovementType.Codes.Arrival;
				var arrivalSupportingDocs = nctsBill.SupportingDocuments;
				AssertEquals("Initially ArrivalSupportingDocuments is empty", 0, arrivalSupportingDocs.Count);
				var doc1 = arrivalSupportingDocs.AddNew();
				var doc2 = arrivalSupportingDocs.AddNew();
				AssertEquals("SupportingDocuments now has 2 docs", 2, arrivalSupportingDocs.Count);

				Factory.Save();
				var anotherFactory = new BusinessObjectFactory();
				var nctsBillInAnotherFactory = anotherFactory.Load<NctsBill>(nctsBill.PK);
				AssertContainsExactElementsInAnyOrder("Reload nctsBill, ArrivalSupportingDocuments contains 2 docs", new[] { doc1.PK, doc2.PK }, nctsBillInAnotherFactory.SupportingDocuments.Select(x => x.PK));
			});
		}

		public void TestPreviousDocuments_Properties()
		{
			var nctsBill = Phase5DepartureNctsBill;
			var previousDocuments = nctsBill.PreviousDocuments;
			CombineAssertions(() =>
			{
				AssertType<CommonPreviousDocumentCollection<CommonPreviousDocument>>("Type", previousDocuments);
				AssertEquals("IsRegisteredEditableChildObject", true, nctsBill.IsRegisteredEditableChildObject(previousDocuments));
				AssertSame("Cached", previousDocuments, nctsBill.PreviousDocuments);
				AssertEquals("IsLoaded", true, previousDocuments.IsLoaded);
			});
		}

		public void TestPreviousDocuments()
		{
			var nctsBill = Phase5DepartureNctsBill;
			var previousDocs = nctsBill.PreviousDocuments;
			CombineAssertions(() =>
			{
				AssertEquals("Initially PreviousDocuments is empty", 0, previousDocs.Count);
				var doc1 = previousDocs.AddNew();
				var doc2 = previousDocs.AddNew();
				AssertEquals("PreviousDocuments now has 2 docs", 2, previousDocs.Count);

				Factory.Save();
				var anotherFactory = new BusinessObjectFactory();
				var nctsBillInAnotherFactory = anotherFactory.Load<NctsBill>(nctsBill.PK);
				AssertContainsExactElementsInAnyOrder("Reload nctsBill, PreviousDocuments contains 2 docs", new[] { doc1.PK, doc2.PK }, nctsBillInAnotherFactory.PreviousDocuments.Select(x => x.PK));
			});
		}

		public void TestCusSupplyChainActorReferences_Properties()
		{
			var nctsBill = Phase5DepartureNctsBill;
			var cusSupplyChainActorReferences = nctsBill.CusSupplyChainActorReferences;
			CombineAssertions(() =>
			{
				AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>("Type", cusSupplyChainActorReferences);
				AssertEquals("IsRegisteredEditableChildObject", true, nctsBill.IsRegisteredEditableChildObject(cusSupplyChainActorReferences));
				AssertSame("Cached", cusSupplyChainActorReferences, nctsBill.CusSupplyChainActorReferences);
				AssertEquals("IsLoaded", true, cusSupplyChainActorReferences.IsLoaded);
			});
		}

		public void TestCusSupplyChainActorReferences()
		{
			var nctsBill = Phase5DepartureNctsBill;
			var supplyChainActorReference = nctsBill.CusSupplyChainActorReferences;
			CombineAssertions(() =>
			{
				AssertEquals("Initially CusSupplyChainActorReferences is empty", 0, supplyChainActorReference.Count);
				var supplyChainActorReference1 = supplyChainActorReference.AddNew();
				supplyChainActorReference1.CFR_Code = SupplyChainActorRoleList.Codes.CS;
				supplyChainActorReference1.CFR_Reference = "Reference1";
				var supplyChainActorReference2 = supplyChainActorReference.AddNew();
				supplyChainActorReference2.CFR_Code = SupplyChainActorRoleList.Codes.WH;
				supplyChainActorReference2.CFR_Reference = "Reference2";
				AssertEquals("CusSupplyChainActorReferences now has 2 docs", 2, supplyChainActorReference.Count);

				Factory.Save();
				var anotherFactory = new BusinessObjectFactory();
				var nctsBillInAnotherFactory = anotherFactory.Load<NctsBill>(nctsBill.PK);
				AssertContainsExactElementsInAnyOrder("Reload nctsBill, CusSupplyChainActorReferences contains 2 elements", new[] { supplyChainActorReference1.PK, supplyChainActorReference2.PK }, nctsBillInAnotherFactory.CusSupplyChainActorReferences.Select(x => x.PK));
			});
		}

		public void TestAdditionalDocuments_Properties()
		{
			var nctsBill = Phase5DepartureNctsBill;
			var additionalDocuments = nctsBill.AdditionalDocuments;
			CombineAssertions(() =>
			{
				AssertType<NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>>("Type", additionalDocuments);
				AssertEquals("IsRegisteredEditableChildObject", true, nctsBill.IsRegisteredEditableChildObject(additionalDocuments));
				AssertSame("Cached", additionalDocuments, nctsBill.AdditionalDocuments);
				AssertEquals("IsLoaded", true, additionalDocuments.IsLoaded);
			});
		}

		public void TestAdditionalDocuments()
		{
			var nctsBill = Phase5DepartureNctsBill;
			var additionalDocs = nctsBill.AdditionalDocuments;
			CombineAssertions(() =>
			{
				AssertEquals("Initially AdditionalDocuments is empty", 0, additionalDocs.Count);
				var doc1 = additionalDocs.AddNew();
				var doc2 = additionalDocs.AddNew();
				AssertEquals("AdditionalDocuments now has 2 docs", 2, additionalDocs.Count);

				Factory.Save();
				var anotherFactory = new BusinessObjectFactory();
				var nctsBillInAnotherFactory = anotherFactory.Load<NctsBill>(nctsBill.PK);
				AssertContainsExactElementsInAnyOrder("Reload nctsBill, Additional Documents contains 2 docs", new[] { doc1.PK, doc2.PK }, nctsBillInAnotherFactory.AdditionalDocuments.Select(x => x.PK));
			});
		}

		public void TestB0_ReferenceID_Captions()
		{
			NCTSTestHelper.AssertCaptions(Phase5ArrivalNctsBill.B0_ReferenceIDInfo, NctsHeader.Phase5CaptionKey, "House Consignment ID", "House Consignment", "House Ref.");
		}

		public void TestB0_WeightUQ_List()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(NctsBill), nameof(NctsBill.B0_WeightUQ), false, attr => attr.ListDataSourceMember == (nameof(NctsBill.Lookups) + "." + nameof(NctsBill.Lookups.WeightUnitList)));
		}

		public void TestB0_WeightUQ_Captions_Phase5Arrival()
		{
			NCTSTestHelper.AssertCaptions(Phase5ArrivalNctsBill.B0_WeightUQInfo, "Units", string.Empty, string.Empty);
		}

		public void TestB0_Weight_ReadOnly()
		{
			Func<WeightReadOnlyVariation, bool> readOnlyFunction = variation => variation.applicationCode == CusInBondApplicationCodeList.Codes.NCTS5 && variation.movementType == NctsMovementType.Codes.Arrival && !variation.hasLinkedMoveDetail && (variation.unloadedState == NctsUnloadedStateList.Codes.MIS || variation.unloadedState == NctsUnloadedStateList.Codes.DIF || variation.unloadedState == NctsUnloadedStateList.Codes.DEC);
			CombineAssertions(() =>
			{
				foreach (var variation in GetWeightReadOnlyVariations(readOnlyFunction))
				{
					AssertEquals(variation.Message, variation.ExpectedReadOnly, variation.Bill.B0_WeightInfo.ReadOnly);
					AssertEquals(variation.Message + " (MetaData)", variation.ExpectedReadOnly, MetaData.GetReadOnly(variation.Bill, variation.Bill.B0_WeightInfo.PropertyDescriptor));
				}
			});
		}

		public void TestB0_ReferenceID_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Phase 4, Departure", false, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure).B0_ReferenceIDInfo.ReadOnly);
				AssertEquals("Phase 5, Departure", false, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure).B0_ReferenceIDInfo.ReadOnly);
				AssertEquals("Phase 4, Arrival", false, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival).B0_ReferenceIDInfo.ReadOnly);
				AssertEquals("Phase 5, Arrival", true, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival).B0_ReferenceIDInfo.ReadOnly);
			});
		}

		public void TestB0_SecurityIndicatorFromExport_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Phase 4, Departure", false, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure).B0_SecurityIndicatorFromExportInfo.ReadOnly);
				AssertEquals("Phase 5, Departure", false, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure).B0_SecurityIndicatorFromExportInfo.ReadOnly);
				AssertEquals("Phase 4, Arrival", false, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival).B0_SecurityIndicatorFromExportInfo.ReadOnly);
				AssertEquals("Phase 5, Arrival", true, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival).B0_SecurityIndicatorFromExportInfo.ReadOnly);
			});
		}

		public void TestB0_RN_NKCountryOfDestination_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(Phase5DepartureNctsBill.B0_RN_NKCountryOfDestinationInfo, multipleResourceKey: null, "Country/Region of Destination", "Destin. Ctry./Rgn.", "Destination Ctry./Rgn.");
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms_Arrival_Phase5()
		{
			var bill = Phase5ArrivalNctsBill;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(bill.Header.ArrivalMovementHeader, x => ((NctsBill)x).AreUnloadingRemarksFullyAccepted, bill);
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms_Arrival_Phase4()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Arrival);

			AssertEquals(false, bill.IsUnloadingRemarksReadOnly);
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms_Departure_Phase5()
		{
			AssertEquals(false, Phase5DepartureNctsBill.IsUnloadingRemarksReadOnly);
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms_Departure_Phase4()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Departure);

			AssertEquals(false, bill.IsUnloadingRemarksReadOnly);
		}

		public void TestB0_Weight_ReadOnly_Arrival_Phase5()
		{
			var arrivalHeader = CreateNctsHeaderToTest(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Arrival);
			var bill = arrivalHeader.Bills.AddNew();

			AssertEquals("Editable", false, bill.B0_WeightInfo.ReadOnly);
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(bill.Header.ArrivalMovementHeader, x => ((NctsBill)x).B0_WeightInfo.ReadOnly, bill);
		}

		public void TestUnloadingRemarksSentToCustoms()
		{
			var bill = Phase5ArrivalNctsBill;
			CombineAssertions(() =>
			{
				AssertEquals("Not sent", false, bill.IsUnloadingRemarksReadOnly);
				bill.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, bill.IsUnloadingRemarksReadOnly);
			});
		}

		public void TestB0_Weight_ReadOnlyForSentToCustoms()
		{
			var arrivalHeader = CreateNctsHeaderToTest(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Arrival);
			var bill = arrivalHeader.Bills.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, bill.B0_WeightInfo.ReadOnly);
				bill.Header.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, bill.B0_WeightInfo.ReadOnly);
			});
		}

		public void TestB0_Weight_ReadOnly_Departure_Phase5()
		{
			AssertEquals("ReadOnly", false, Phase5DepartureNctsBill.B0_WeightInfo.ReadOnly);
		}

		public void TestB0_Weight_ReadOnly_Arrival_Phase4()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Arrival);

			AssertEquals("ReadOnly", false, bill.B0_WeightInfo.ReadOnly);
		}

		public void TestB0_Weight_ReadOnly_Departure_Phase4()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS4, NctsMoveHeaderType.Codes.Departure);

			AssertEquals("ReadOnly", false, bill.B0_WeightInfo.ReadOnly);
		}

		public void TestArrivalTransportInfoList()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Arrival);
			var arrivalTransportInfos = bill.ArrivalTransportInfos;
			CombineAssertions(() =>
			{
				AssertType<ArrivalCusTransportMeansCollection<ArrivalCusTransportMeans>>("Type", arrivalTransportInfos);
				AssertEquals("IsRegisteredEditableChildObject", true, bill.IsRegisteredEditableChildObject(arrivalTransportInfos));
				AssertSame("Cached", arrivalTransportInfos, bill.ArrivalTransportInfos);
				AssertEquals("IsLoaded", true, arrivalTransportInfos.IsLoaded);
			});
		}

		public void TestDepartureTransportInfoList()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			var departureTransportInfos = bill.DepartureTransportInfos;
			CombineAssertions(() =>
			{
				AssertType<DepartureCusTransportMeansCollection<DepartureCusTransportMeans>>("Type", departureTransportInfos);
				AssertEquals("IsRegisteredEditableChildObject", true, bill.IsRegisteredEditableChildObject(departureTransportInfos));
				AssertSame("Cached", departureTransportInfos, bill.DepartureTransportInfos);
				AssertEquals("IsLoaded", true, departureTransportInfos.IsLoaded);
			});
		}

		public void TestCanDelete()
		{
			var nctsBill = Phase5ArrivalNctsBill;
			CombineAssertions(() =>
			{
				nctsBill.MovementDetail.B9_UnloadedState = ZString.Empty;
				AssertEquals("MovementDetail.B9_UnloadedState empty", false, nctsBill.CanDelete);

				nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("MovementDetail.B9_UnloadedState = 'DEC'", false, nctsBill.CanDelete);

				nctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				AssertEquals("MovementDetail.B9_UnloadedState = 'NEW'", true, nctsBill.CanDelete);
			});
		}

		public void TestReasonForNotAbleToDelete()
		{
			var nctsBill = Phase5ArrivalNctsBill;
			AssertEquals("Cannot delete House Consignment from Customs.", nctsBill.ReasonForNotAbleToDelete);
		}

		public void TestInlandTransportModeAtDeparture()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			bill.Header.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			AssertEquals("InlandTransportMode", ModeOfTransportList.Codes._3_RoadTransport, bill.InlandTransportModeAtDeparture);
		}

		public void TestInlandTransportModeAtDeparture_Caption()
		{
			NCTSTestHelper.AssertCaptions(Phase5DepartureNctsBill.InlandTransportModeAtDepartureInfo, "Inland M. O. T.", string.Empty, string.Empty);
		}

		public void TestInlandTransportModeDeparture_ReadOnly()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			Assert("Always ReadOnly", bill.InlandTransportModeAtDepartureInfo.ReadOnly);
		}

		public void TestFirstDepartureTransportMeansID()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, bill.FirstDepartureTransportMeansID);
				bill.FirstDepartureTransportMeansID = "ID 1";
				var transport = bill.DepartureTransportInfos.Cast<DepartureCusTransportMeans>().SingleOrDefault(x => x.TPM_SequenceNumber == 1);
				AssertEquals("Transport means ID with sequence number 1", "ID 1", transport.TPM_IdentificationNumber);
			});
		}

		public void TestFirstDepartureTransportMeansNationality()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, bill.FirstDepartureTransportMeansNationality);
				bill.FirstDepartureTransportMeansNationality = Core.Constants.CountryCodes.Germany;
				var transport = bill.DepartureTransportInfos.Cast<DepartureCusTransportMeans>().SingleOrDefault(x => x.TPM_SequenceNumber == 1);
				AssertEquals("Transport means Nationality with sequence number 1", Core.Constants.CountryCodes.Germany, transport.TPM_RN_NKTransportNationality);
			});
		}

		public void TestSecondDepartureTransportMeansID()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			bill.DepartureTransportInfos.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, bill.SecondDepartureTransportMeansID);
				bill.SecondDepartureTransportMeansID = "ID 2";
				var transport = bill.DepartureTransportInfos.Cast<DepartureCusTransportMeans>().SingleOrDefault(x => x.TPM_SequenceNumber == 2);
				AssertEquals("Transport means ID with sequence number 2", "ID 2", transport.TPM_IdentificationNumber);
			});
		}

		public void TestSecondDepartureTransportMeansNationality()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			bill.DepartureTransportInfos.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, bill.SecondDepartureTransportMeansNationality);
				bill.SecondDepartureTransportMeansNationality = Core.Constants.CountryCodes.Germany;
				var transport = bill.DepartureTransportInfos.Cast<DepartureCusTransportMeans>().SingleOrDefault(x => x.TPM_SequenceNumber == 2);
				AssertEquals("Transport means Nationality with sequence number 2", Core.Constants.CountryCodes.Germany, transport.TPM_RN_NKTransportNationality);
			});
		}

		public void TestIsPhase5Arrival()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Phase 5 Departure", false, Phase5DepartureNctsBill.IsPhase5Arrival);
				AssertEquals("Phase 5 Arrival", true, Phase5ArrivalNctsBill.IsPhase5Arrival);
				AssertEquals("Phase 5 Arrival And Departure", true, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.DepartureAndArrival).IsPhase5Arrival);
				AssertEquals("Phase 4 Departure", false, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure).IsPhase5Arrival);
				AssertEquals("Phase 4 Arrival", false, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure).IsPhase5Arrival);
				AssertEquals("Phase 4 Arrival And Departure", false, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.DepartureAndArrival).IsPhase5Arrival);
			});
		}

		public void TestIsPhase5Departure()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Phase 5 Departure", true, Phase5DepartureNctsBill.IsPhase5Departure);
				AssertEquals("Phase 5 Arrival", false, Phase5ArrivalNctsBill.IsPhase5Departure);
				AssertEquals("Phase 5 Arrival And Departure", true, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.DepartureAndArrival).IsPhase5Departure);
				AssertEquals("Phase 4 Departure", false, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure).IsPhase5Departure);
				AssertEquals("Phase 4 Arrival", false, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure).IsPhase5Departure);
				AssertEquals("Phase 4 Arrival And Departure", false, CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.DepartureAndArrival).IsPhase5Departure);
			});
		}

		public void TestConsigneeRequirementValidation()
		{
			var consignee = Phase5DepartureNctsBill.Consignee;
			consignee.E2_AddressOverride = true;
			JobDocAddressValidationHelperTest.AssertOverrideValidations(consignee);
		}

		public void TestConsignorRequirementValidation()
		{
			var consignee = Phase5DepartureNctsBill.Consignor;
			consignee.E2_AddressOverride = true;
			JobDocAddressValidationHelperTest.AssertOverrideValidations(consignee);
		}

		public void TestConsigneeRequirementValidation_WorkPhoneTR0079()
		{
			var consignee = Phase5DepartureNctsBill.Consignee;

			JobDocAddressValidationHelperTest.AssertTR0079Validation(consignee, Phase5DepartureNctsBill.Header);
		}

		public void TestConsignorRequirementValidation_WorkPhoneTR0079()
		{
			var consignor = Phase5DepartureNctsBill.Consignor;

			JobDocAddressValidationHelperTest.AssertTR0079Validation(consignor, Phase5DepartureNctsBill.Header);
		}

		public void TestJobDocAddressRequirementDefaultContactType_Consignee()
		{
			AssertEquals(ContactType.NoContactType, Phase5DepartureNctsBill.ConsigneeJobDocAddressRequirement.DefaultContactType);
		}

		public void TestJobDocAddressRequirementDefaultContactType_Consignor()
		{
			AssertEquals(ContactType.NoContactType, Phase5DepartureNctsBill.ConsignorJobDocAddressRequirement.DefaultContactType);
		}

		public void TestTransportDepartureAdditionalWagonNumbers()
		{
			var bill = CreateNctsBill(Factory, CusInBondApplicationCodeList.Codes.NCTS5, NctsMoveHeaderType.Codes.Departure);
			var additionalWagonNumbers = bill.TransportDepartureAdditionalWagonNumbers;
			CombineAssertions(() =>
			{
				AssertType<DepartureCusTransportMeansFilteredCollection>("Type", additionalWagonNumbers);
				AssertSame("Cached", additionalWagonNumbers, bill.TransportDepartureAdditionalWagonNumbers);
			});
		}

		public void TestTransportDepartureProperties_ReadOnly_WhenTransitionPeriodIsOn()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertTransportDeparturePropertiesReadOnly("departure, during transition period", Phase5DepartureNctsBill, expectedReadOnly: true);
					AssertTransportDeparturePropertiesReadOnly("arrival, during transition period", Phase5ArrivalNctsBill, expectedReadOnly: false);
				}
			});
		}

		public void TestTransportDepartureProperties_ReadOnly_WhenTransitionPeriodIsOff()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertTransportDeparturePropertiesReadOnly("departure, after transition period", Phase5DepartureNctsBill, expectedReadOnly: false);
					AssertTransportDeparturePropertiesReadOnly("arrival, after transition period", Phase5ArrivalNctsBill, expectedReadOnly: false);
				}
			});
		}

		public void TestIsTransportDepartureReadOnly_WhenTransitionPeriodIsOn()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("departure, during transition period", true, Phase5DepartureNctsBill.IsTransportDepartureReadOnly);
					AssertEquals("arrival, during transition period", false, Phase5ArrivalNctsBill.IsTransportDepartureReadOnly);
				}
			});
		}

		public void TestIsTransportDepartureReadOnly_WhenTransitionPeriodIsOff()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("departure, after transition period", false, Phase5DepartureNctsBill.IsTransportDepartureReadOnly);
					AssertEquals("arrival, after transition period", false, Phase5ArrivalNctsBill.IsTransportDepartureReadOnly);
				}
			});
		}

		public void TestMultipleKeysToUse()
		{
			var phase4Bill = CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival);

			CombineAssertions(() =>
			{
				AssertSequencesEqual("Phase 4 Bill uses Phase4CaptionKey", phase4Bill.MultipleKeysToUse, new[] { NctsHeader.Phase4CaptionKey });

				AssertSequencesEqual("Phase 5 Arrival Bill uses Phase5CaptionKey", Phase5ArrivalNctsBill.MultipleKeysToUse, new[] { NctsHeader.Phase5CaptionKey });

				AssertSequencesEqual("Phase 5 Departure Bill uses Phase5 and Phase5Departure CaptionKeys", Phase5DepartureNctsBill.MultipleKeysToUse, new[] { NctsHeader.Phase5DepartureCaptionKey, NctsHeader.Phase5CaptionKey });
			});
		}

		public void TestHasDifference() => CombineAssertions(() =>
		{
			AssertEquals("Initial", false, Phase5DepartureNctsBill.HasDifference);
			Phase5ArrivalNctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			AssertEquals($"B9_UnloadedState='{Phase5ArrivalNctsBill.MovementDetail.B9_UnloadedState}'", true, Phase5ArrivalNctsBill.HasDifference);
			Phase5ArrivalNctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals($"B9_UnloadedState='{Phase5ArrivalNctsBill.MovementDetail.B9_UnloadedState}'", false, Phase5ArrivalNctsBill.HasDifference);
			Phase5ArrivalNctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			AssertEquals($"B9_UnloadedState='{Phase5ArrivalNctsBill.MovementDetail.B9_UnloadedState}'", false, Phase5ArrivalNctsBill.HasDifference);
			Phase5ArrivalNctsBill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			AssertEquals($"B9_UnloadedState='{Phase5ArrivalNctsBill.MovementDetail.B9_UnloadedState}'", false, Phase5ArrivalNctsBill.HasDifference);
		});

		public void TestCustomsEntryIntegrator()
		{
			AssertType<NctsBillCustomsEntryIntegrator>("CustomsEntryIntegrator", Phase5DepartureNctsBill.GetCustomsEntryIntegrator());
		}

		public void TestNCTSPreviousDocumentsCount() => CombineAssertions(() =>
		{
			AssertEquals("No documents", 0, Phase5DepartureNctsBill.NCTSPreviousDocumentsCount);

			Phase5DepartureNctsBill.PreviousDocuments.AddNew().CSI_Code = "N1";
			AssertEquals("1 NCTS documents", 1, Phase5DepartureNctsBill.NCTSPreviousDocumentsCount);

			Phase5DepartureNctsBill.PreviousDocuments.AddNew().CSI_Code = "X1";
			AssertEquals("1 NCTS documents and 1 other document", 1, Phase5DepartureNctsBill.NCTSPreviousDocumentsCount);

			Phase5DepartureNctsBill.PreviousDocuments.AddNew().CSI_Code = "N2";
			AssertEquals("2 NCTS documents", 2, Phase5DepartureNctsBill.NCTSPreviousDocumentsCount);
		});

		public void TestMaxNCTSPreviousDocumentsCount()
		{
			Phase5DepartureNctsBill.PreviousDocuments.AddNew().CSI_Code = "N1";
			Phase5DepartureNctsBill.PreviousDocuments.AddNew().CSI_Code = "N2";
			Phase5DepartureNctsBill.PreviousDocuments.AddNew().CSI_Code = "X1";
			var goodsItem1 = Phase5DepartureNctsBill.GoodsItems.AddNew();
			goodsItem1.PreviousDocuments.AddNew().CSI_Code = "N0";
			var goodsItem2 = Phase5DepartureNctsBill.GoodsItems.AddNew();
			goodsItem2.PreviousDocuments.AddNew().CSI_Code = "N3";
			goodsItem2.PreviousDocuments.AddNew().CSI_Code = "N4";
			goodsItem2.PreviousDocuments.AddNew().CSI_Code = "N5";
			goodsItem2.PreviousDocuments.AddNew().CSI_Code = "X2";
			AssertEquals(5, Phase5DepartureNctsBill.MaxCountNCTSPreviousDocuments);
		}

		public void TestSetAllUnloadedStateToDEC_whenDIF()
		{
			AssertSetAllUnloadedStateToDEC("DIF");
		}

		public void TestSetAllUnloadedStateToDEC_whenMIS()
		{
			AssertSetAllUnloadedStateToDEC("MIS");
		}

		public void TestSetAllUnloadedStateToDEC_whenDEC()
		{
			AssertSetAllUnloadedStateToDEC("DEC");
		}

		void AssertSetAllUnloadedStateToDEC(string unloadedState)
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill = header.Bills.AddNew();
			var movementDetail = bill.MovementDetail;
			movementDetail.B9_UnloadedState = unloadedState;
			var supportingDocument = bill.SupportingDocuments.AddNew();
			supportingDocument.CSI_Status = "MIS";
			var additionalDocument = bill.AdditionalDocuments.AddNew();

			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			goodsItem.BY_UnloadedState = "DIF";
			var supportingDoc_GoodsItem = goodsItem.SupportingDocuments.AddNew();
			supportingDoc_GoodsItem.CSI_Status = "DIF";

			bill.SetAllUnloadedStateToDEC();
			CombineAssertions(() =>
			{
				AssertEquals("UnloadedState of bill has been changed to or is DEC", "DEC", movementDetail.B9_UnloadedState);
				AssertEquals("UnloadedState of supportingDocument has been update from 'MIS' to 'DEC'", "DEC", supportingDocument.CSI_Status);
				AssertEquals("When UnloadedState is NEW, additionalDocument will be delete", 0, bill.AdditionalDocuments.Count);
				AssertEquals("UnloadedState of goodsItem has been update from 'DIF' to 'DEC'", "DEC", goodsItem.BY_UnloadedState);
				AssertEquals("UnloadedState of supportingDocument under goodsItem has been update from 'DIF' to 'DEC'", "DEC", supportingDoc_GoodsItem.CSI_Status);
			});
		}

		public void TestIsOutwardOrderImported()
		{
			CombineAssertions(() =>
			{
				AssertEquals(false, Phase5DepartureNctsBill.IsOutwardOrderImported);

				Phase5DepartureNctsBill.IsOutwardOrderImported = true;

				AssertEquals(true, Phase5DepartureNctsBill.IsOutwardOrderImported);
			});
		}

		public void TestIsOutwardOrderImported_GenAddOnColumn()
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_Name, nameof(NctsBill.IsOutwardOrderImported));
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, CusInBondBillSchema.Constants.Prefix);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentID, Phase5DepartureNctsBill.PK);

			CombineAssertions(() =>
			{
				AssertEquals("No GenAddOn", false, Factory.Exists(typeof(GenAddOnColumn), query));
				Phase5DepartureNctsBill.IsOutwardOrderImported = true;
				AssertEquals("Column Exists True", true, Factory.Exists(typeof(GenAddOnColumn), query));
			});
		}

		public void TestClone()
		{
			var bill = Factory.New<NctsBill>();
			bill.SequenceNumber = 1;
			bill.TransportTypeAtDeparture = "30";
			bill.FirstDepartureTransportMeansID = "First Transport";
			bill.FirstDepartureTransportMeansNationality = "IT";
			bill.SecondDepartureTransportMeansID = "Second Transport";
			bill.SecondDepartureTransportMeansNationality = "FR";
			bill.ThirdDepartureTransportMeansID = "Third Transport";
			bill.ThirdDepartureTransportMeansNationality = "ES";

			var clone = bill.Clone() as NctsBill;

			AssertEquals("SequenceNumber", bill.SequenceNumber, clone.SequenceNumber);
			AssertEquals("TransportTypeAtDeparture", bill.TransportTypeAtDeparture, clone.TransportTypeAtDeparture);
			AssertEquals("FirstDepartureTransportMeansID", bill.FirstDepartureTransportMeansID, clone.FirstDepartureTransportMeansID);
			AssertEquals("FirstDepartureTransportMeansNationality", bill.FirstDepartureTransportMeansNationality, clone.FirstDepartureTransportMeansNationality);
			AssertEquals("SecondDepartureTransportMeansID", bill.SecondDepartureTransportMeansID, clone.SecondDepartureTransportMeansID);
			AssertEquals("SecondDepartureTransportMeansNationality", bill.SecondDepartureTransportMeansNationality, clone.SecondDepartureTransportMeansNationality);
			AssertEquals("ThirdDepartureTransportMeansID", bill.ThirdDepartureTransportMeansID, clone.ThirdDepartureTransportMeansID);
			AssertEquals("ThirdDepartureTransportMeansNationality", bill.ThirdDepartureTransportMeansNationality, clone.ThirdDepartureTransportMeansNationality);
		}

		public void TestB0_RX_NKLinePriceCurrency_Caption()
		{
			NCTSTestHelper.AssertCaptions(Phase5DepartureNctsBill.B0_RX_NKLinePriceCurrencyInfo, "Line Price Currency", "Line Price Curr.", "Curr.");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Phase5DepartureNctsBill;

		protected override BusinessObject GetNewBusinessObject() => Phase5DepartureNctsBill;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => Phase5DepartureNctsBill;

		AdditionalInfo Add30600AdditionalInfo(ICusSupportingInfoCollection<AdditionalInfo> additionalInfoCollection)
		{
			var additionalInfo = additionalInfoCollection.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Code = AdditionalDocumentTypes._30600;

			return additionalInfo;
		}

		ValidationRuleMessages ValidationRuleMessages => (validationRuleMessages ?? new ValidationRuleMessages());
		readonly ValidationRuleMessages validationRuleMessages;

		NctsBill Phase5DepartureNctsBill => phase5DepartureNctsBill ?? (phase5DepartureNctsBill = CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure));
		NctsBill phase5DepartureNctsBill;

		NctsBill Phase5ArrivalNctsBill => phase5ArrivalNctsBill ?? (phase5ArrivalNctsBill = CreateNctsBill(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival));
		NctsBill phase5ArrivalNctsBill;

		NctsBill CreateNctsBill(string applicationCode, string movementType) => CreateNctsBill(Factory, applicationCode, movementType);

		static NctsHeader CreateNctsHeaderToTest(BusinessObjectFactory factory, string applicationCode, string movementType)
		{
			var header = factory.New<NctsHeader>();
			header.SetMovementType(movementType);
			header.BH_ApplicationCode = applicationCode;
			return header;
		}

		static NctsBill CreateNctsBill(BusinessObjectFactory factory, string applicationCode, string movementType)
		{
			var nctsHeader = CreateNctsHeaderToTest(factory, applicationCode, movementType);
			return nctsHeader.Bills.AddNew();
		}

		static NctsBill CreateNctsBill(BusinessObjectFactory factory, string applicationCode, string movementType, bool hasLinkedMoveDetail, string unloadedState)
		{
			var bill = CreateNctsBill(factory, applicationCode, movementType);

			if (bill.MovementDetail is CusInBondMoveDetail movementDetail)
			{
				movementDetail.B9_UnloadedState = unloadedState;
				movementDetail.B9_B9_InBondMoveDetail = hasLinkedMoveDetail ? ZGuid.BrettsGuid : ZGuid.Empty;
			}

			return bill;
		}

		static IReadOnlyList<WeightReadOnlyVariation> GetWeightReadOnlyVariations(Func<WeightReadOnlyVariation, bool> readOnlyCheck)
		{
			var variations = new List<WeightReadOnlyVariation>();
			foreach (var applicationCode in new[] { CusInBondApplicationCodeList.Codes.NCTS4, CusInBondApplicationCodeList.Codes.NCTS5 })
			{
				foreach (var movementType in new[] { NctsMovementType.Codes.Departure, NctsMovementType.Codes.Arrival })
				{
					foreach (var hasLinkedMoveDetail in new[] { false, true })
					{
						foreach (var unloadedState in new[] { string.Empty, NctsUnloadedStateList.Codes.NEW, NctsUnloadedStateList.Codes.MIS, NctsUnloadedStateList.Codes.DIF, NctsUnloadedStateList.Codes.DEC })
						{
							variations.Add(new WeightReadOnlyVariation(applicationCode, movementType, hasLinkedMoveDetail, unloadedState, readOnlyCheck));
						}
					}
				}
			}
			return variations;
		}

		class WeightReadOnlyVariation
		{
			public readonly string applicationCode;
			public readonly string movementType;
			public readonly bool hasLinkedMoveDetail;
			public readonly string unloadedState;
			public NctsBill Bill { get; }
			public bool ExpectedReadOnly { get; }

			public WeightReadOnlyVariation(string applicationCode, string movementType, bool hasLinkedMoveDetail, string unloadedState, Func<WeightReadOnlyVariation, bool> readOnlyCheck)
			{
				this.applicationCode = applicationCode;
				this.movementType = movementType;
				this.hasLinkedMoveDetail = hasLinkedMoveDetail;
				this.unloadedState = unloadedState;
				ExpectedReadOnly = readOnlyCheck.Invoke(this);

				Bill = CreateNctsBill(new BusinessObjectFactory(), applicationCode, movementType, hasLinkedMoveDetail, unloadedState);
			}

			public string Message => $"ApplicationCode: '{applicationCode}' MovementType: '{movementType}' B9_B9_InBondMoveDetail?: '{hasLinkedMoveDetail}' B9_UnloadedState: '{unloadedState}' expectReadOnly?: '{ExpectedReadOnly}'";
		}

		static void AssertTransportDeparturePropertiesReadOnly(string assertionMessage, NctsBill bill, bool expectedReadOnly)
		{
			AssertEquals($"AircraftIDAtDeparture: {assertionMessage}", expectedReadOnly, bill.AircraftIDAtDepartureInfo.ReadOnly);
			AssertEquals($"TransportTypeAtDeparture: {assertionMessage}", expectedReadOnly, bill.TransportTypeAtDepartureInfo.ReadOnly);
			AssertEquals($"TransportAtDeparture: {assertionMessage}", expectedReadOnly, bill.TransportAtDepartureInfo.ReadOnly);
			AssertEquals($"TransportCountryAtDeparture: {assertionMessage}", expectedReadOnly, bill.TransportCountryAtDepartureInfo.ReadOnly);
			AssertEquals($"Trailer1IDAtDeparture: {assertionMessage}", expectedReadOnly, bill.Trailer1IDAtDepartureInfo.ReadOnly);
			AssertEquals($"Trailer2IDAtDeparture: {assertionMessage}", expectedReadOnly, bill.Trailer2IDAtDepartureInfo.ReadOnly);
			AssertEquals($"Trailer1NationalityAtDeparture: {assertionMessage}", expectedReadOnly, bill.Trailer1NationalityAtDepartureInfo.ReadOnly);
			AssertEquals($"Trailer2NationalityAtDeparture: {assertionMessage}", expectedReadOnly, bill.Trailer2NationalityAtDepartureInfo.ReadOnly);
			AssertEquals($"VesselNameAtDeparture: {assertionMessage}", expectedReadOnly, bill.VesselNameAtDepartureInfo.ReadOnly);
			AssertEquals($"VesselCountryAtDeparture: {assertionMessage}", expectedReadOnly, bill.VesselCountryAtDepartureInfo.ReadOnly);
		}

		public void TestHas30600AdditionalInformation()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bills = nctsHeader.Bills.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("When additionalDocument is null.", expected: false, bills.AdditionalDocuments.Has30600AdditionalInformation());

				var additionalDocument = bills.AdditionalDocuments.AddNew();
				additionalDocument.CSI_Code = "30601";
				additionalDocument.CSI_SubType = "INF";
				AssertEquals("When additionalDocument is not null and Doc Kind = INF and Doc.Type != 30600.", expected: false, bills.AdditionalDocuments.Has30600AdditionalInformation());

				additionalDocument.CSI_Code = "30600";
				AssertEquals("When additionalDocument is not null and Doc Kind = INF and Doc.Type = 30600.", expected: true, bills.AdditionalDocuments.Has30600AdditionalInformation());
			});
		}

		public void TestIsContainerised()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var bill = nctsHeader.Bills.AddNew();
			AssertEquals("Bill has no goods items", false, bill.IsContainerised);

			var goodsItem = bill.GoodsItems.AddNew();
			AssertEquals("None of bill goods items has package.", false, bill.IsContainerised);

			var package = goodsItem.Packages.AddNew();
			AssertEquals("None of bill goods item packages has any container selected.", false, bill.IsContainerised);

			var container = nctsHeader.DepartureHeaderContainers.AddNew();
			var containerPivot = package.ContainersPivot.AddNew();
			containerPivot.XX_Relation2ID = container.PK;

			AssertEquals("One of bill goods items package has linked container but it has no number.", false, bill.IsContainerised);

			container.BC_ContainerNum = "CONTAINER1";
			AssertEquals("One of bill goods items package has linked container with number", true, bill.IsContainerised);
		}

		public void TestSetArrivalTransportInfosReadOnly() => CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var bill = nctsHeader.Bills.AddNew();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
			{
				bill.SetArrivalTransportInfosReadOnly();
				AssertEquals("NC5TP active", bill.ArrivalTransportInfos.ReadOnly, true);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
			{
				bill.SetArrivalTransportInfosReadOnly();
				AssertEquals("NC5TP inactive", bill.ArrivalTransportInfos.ReadOnly, false);
			}
		});

		public void TestB0_CopyPreviousGoodLineToNewLines()
		{
			using (CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				var bill = header.Bills.AddNew();
				CombineAssertions("Copy from previous line - Registry is set to True", () =>
				{
					AssertEquals("B0_CopyLastGoodsLineToNewLines default true", true, bill.B0_CopyLastGoodsLineToNewLines);
					AssertEquals("Flag in collection default true", true, bill.GoodsItems.CopyLastGoodsItemToNewLines);

					bill.GoodsItems.CopyLastGoodsItemToNewLines = false;
					AssertEquals("B0_CopyLastGoodsLineToNewLines manual override to false", false, bill.B0_CopyLastGoodsLineToNewLines);
					AssertEquals("Flag in collection manual override to false", false, bill.GoodsItems.CopyLastGoodsItemToNewLines);
				});
			}

			using (CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Departure);
				var bill = header.Bills.AddNew();
				CombineAssertions("Copy from previous line - Registry is set to false", () =>
				{
					AssertEquals("Registry false, default false", false, bill.B0_CopyLastGoodsLineToNewLines);
					AssertEquals("Registry false, default false", false, bill.GoodsItems.CopyLastGoodsItemToNewLines);

					bill.GoodsItems.CopyLastGoodsItemToNewLines = true;
					AssertEquals("Registry false, manual override", true, bill.B0_CopyLastGoodsLineToNewLines);
					AssertEquals("Registry false, manual override", true, bill.GoodsItems.CopyLastGoodsItemToNewLines);
				});
			}
		}

		public void TestB0_GrossWeightUnloaded_ReadOnly()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMovement = header.ArrivalMovementHeader;
			var bill = header.Bills.AddNew();
			var movementDetail = bill.MovementDetail;

			CombineAssertions(() =>
			{
				AssertEquals("PreReq, UnloadedState NEW", NctsUnloadedStateList.Codes.NEW, movementDetail.B9_UnloadedState);
				AssertEquals("HasDifference: false as UnloadedState NEW and no DifferenceMoveDetail exists", expected: true, bill.B0_GrossWeightUnloadedInfo.ReadOnly);

				movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("HasDifference: false as UnloadedState DEC and no DifferenceMoveDetail exists", expected: true, bill.B0_GrossWeightUnloadedInfo.ReadOnly);

				movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				AssertNotNull("PreReq, DifferenceMoveDetail exists", movementDetail.DifferenceMoveDetail);
				AssertEquals("HasDifference: true as UnloadedState DIF and DifferenceMoveDetail exists", expected: false, bill.B0_GrossWeightUnloadedInfo.ReadOnly);

				arrivalMovement.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("HasDifference: false but UnloadingRemarksReadOnly", expected: true, bill.B0_GrossWeightUnloadedInfo.ReadOnly);
			});
		}

		public void TestOnFactorySaving_UpdateB0_GrossWeightUnloaded()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = header.ArrivalMovementHeader;

			var bill = header.Bills.AddNew();
			var movementDetail = bill.MovementDetail;
			bill.B0_WeightUQ = "KG";
			movementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var goodsItem1 = bill.ArrivalGoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 10;
			goodsItem1.BY_GrossWeightUnit = "KG";
			var goodsItem2 = bill.ArrivalGoodsItems.AddNew();
			goodsItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			goodsItem2.BY_GrossWeight = 2;
			goodsItem2.BY_GrossWeightUnit = "KG";
			var unloadedGoodsItem = goodsItem2.UnloadedGoodsItem;
			unloadedGoodsItem.BY_GrossWeight = 10;
			unloadedGoodsItem.BY_GrossWeightUnit = "KG";

			var goodsItem3 = bill.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
			goodsItem3.BY_GrossWeight = 60;
			goodsItem3.BY_GrossWeightUnit = "KG";

			var goodsItem4 = bill.ArrivalGoodsItems.AddNew();
			goodsItem4.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			goodsItem4.BY_GrossWeight = 6;
			goodsItem4.BY_GrossWeightUnit = "KG";
			Factory.Save();
			AssertEquals("B0_WeightUQ is 'KG'", new ZDecimal(26), bill.B0_GrossWeightUnloaded);
		}
	}
}
