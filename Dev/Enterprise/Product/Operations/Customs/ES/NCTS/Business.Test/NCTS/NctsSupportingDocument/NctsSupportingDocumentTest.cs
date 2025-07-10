using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsSupportingDocument))]
	public class NctsSupportingDocumentTest : CusSupportingInfoTest<NctsSupportingDocument>
	{
		public void TestValidationType()
		{
			CombineAssertions(() =>
			{
				AssertType<NctsSupportingDocumentPhase4Validation>(GetSupportingDocument(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Departure).Validation);
				AssertType<NctsSupportingDocumentPhase4Validation>(GetSupportingDocument(CusInBondApplicationCodeList.Codes.NCTS4, NctsMovementType.Codes.Arrival).Validation);
				AssertType<NctsSupportingDocumentPhase5DepartureValidation>(GetSupportingDocument(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure).Validation);
				AssertType<NctsSupportingDocumentPhase5ArrivalValidation>(GetSupportingDocument(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival).Validation);
			});

			NctsSupportingDocument GetSupportingDocument(ZString phase, ZString movementType)
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = phase;
				nctsHeader.SetMovementType(movementType);
				return nctsHeader.Bills.AddNew().SupportingDocuments.AddNew();
			}
		}

		public void TestCSI_LineNo_ReadOnly()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var supDoc = Factory.New<NctsSupportingDocumentForTest>();
			goodsItem.SupportingDocuments.Add(supDoc);

			AssertEquals("CSI_LineNo_ReadOnly for ES, true", true, supDoc.CSI_LineNo_ReadOnly_Exposed);
		}

		public void TestAutomaticSequenceNumberEnabled_Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var supDoc = Factory.New<NctsSupportingDocumentForTest>();
			goodsItem.SupportingDocuments.Add(supDoc);

			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					AssertEquals("When transition period and phase5, false", false, supDoc.AutomaticSequenceNumberEnabled_Exposed);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					AssertEquals("When transition period and phase4, false", false, supDoc.AutomaticSequenceNumberEnabled_Exposed);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
				{
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					AssertEquals("When no transition period and phase5, true", true, supDoc.AutomaticSequenceNumberEnabled_Exposed);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					AssertEquals("When no transition period and phase4, false", false, supDoc.AutomaticSequenceNumberEnabled_Exposed);
				}
			});
		}

		public void TestAutomaticSequenceNumberEnabled_Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = nctsHeader.Bills.AddNew();
			var supDoc = Factory.New<NctsSupportingDocumentForTest>();
			bill.SupportingDocuments.Add(supDoc);

			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					AssertEquals("When transition period and phase5, true", true, supDoc.AutomaticSequenceNumberEnabled_Exposed);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					AssertEquals("When transition period and phase4, false", false, supDoc.AutomaticSequenceNumberEnabled_Exposed);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
				{
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					AssertEquals("When no transition period and phase5, true", true, supDoc.AutomaticSequenceNumberEnabled_Exposed);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					AssertEquals("When no transition period and phase4, false", false, supDoc.AutomaticSequenceNumberEnabled_Exposed);
				}
			});
		}

		public void TestResetCSI_LineNoWhenChangingCode_Phase5_ParentItem_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = bill.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 before setting it", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 before setting it", 0, supportingDocument6.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 8;
					supportingDocument2.CSI_LineNo = 5;
					supportingDocument3.CSI_LineNo = 6;
					supportingDocument4.CSI_LineNo = 4;
					supportingDocument5.CSI_LineNo = 3;
					supportingDocument6.CSI_LineNo = 7;

					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before changing CSI_Code", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before changing CSI_Code", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before changing CSI_Code", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before changing CSI_Code", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before changing CSI_Code", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before changing CSI_Code", 7, supportingDocument6.CSI_LineNo);

					supportingDocument5.CSI_Code = "AAA";
					AssertEquals("When changing CSI_Code in one document, supportingDocument1.CSI_LineNo is not 0 after changing CSI_Code", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument2.CSI_LineNo is not 0 after changing CSI_Code", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument3.CSI_LineNo is not 0 after changing CSI_Code", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument4.CSI_LineNo is not 0 after changing CSI_Code", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument5.CSI_LineNo is 0 after changing CSI_Code", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument6.CSI_LineNo is 0 after changing CSI_Code", 0, supportingDocument6.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoWhenChangingCode_Phase5_ParentItem_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = bill.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before changing CSI_Code", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before changing CSI_Code", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before changing CSI_Code", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before changing CSI_Code", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before changing CSI_Code", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before changing CSI_Code", 2, supportingDocument6.CSI_LineNo);

					supportingDocument5.CSI_Code = "AAA";
					AssertEquals("When changing CSI_Code in one document, supportingDocument1.CSI_LineNo is not 0 after changing CSI_Code", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument2.CSI_LineNo is not 0 after changing CSI_Code", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument3.CSI_LineNo is not 0 after changing CSI_Code", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument4.CSI_LineNo is not 0 after changing CSI_Code", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument5.CSI_LineNo is not 0 after changing CSI_Code", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument6.CSI_LineNo is not 0 after changing CSI_Code", 2, supportingDocument6.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoWhenChangingCode_Phase5_ParentBill_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = bill.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 before setting it", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 before setting it", 0, supportingDocument6.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 8;
					supportingDocument2.CSI_LineNo = 5;
					supportingDocument3.CSI_LineNo = 6;
					supportingDocument4.CSI_LineNo = 4;
					supportingDocument5.CSI_LineNo = 3;
					supportingDocument6.CSI_LineNo = 7;

					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before changing CSI_Code", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before changing CSI_Code", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before changing CSI_Code", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before changing CSI_Code", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before changing CSI_Code", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before changing CSI_Code", 7, supportingDocument6.CSI_LineNo);

					supportingDocument3.CSI_Code = "AAA";
					AssertEquals("When changing CSI_Code in one document, supportingDocument1.CSI_LineNo is not 0 after changing CSI_Code", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument2.CSI_LineNo is not 0 after changing CSI_Code", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument3.CSI_LineNo is 0 after changing CSI_Code", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument4.CSI_LineNo is 0 after changing CSI_Code", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument5.CSI_LineNo is 0 after changing CSI_Code", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument6.CSI_LineNo is 0 after changing CSI_Code", 0, supportingDocument6.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoWhenChangingCode_Phase5_ParentBill_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = bill.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before changing CSI_Code", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before changing CSI_Code", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before changing CSI_Code", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before changing CSI_Code", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before changing CSI_Code", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before changing CSI_Code", 2, supportingDocument6.CSI_LineNo);

					supportingDocument3.CSI_Code = "AAA";
					AssertEquals("When changing CSI_Code in one document, supportingDocument1.CSI_LineNo is not 0 after changing CSI_Code", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument2.CSI_LineNo is not 0 after changing CSI_Code", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument3.CSI_LineNo is not 0 after changing CSI_Code", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument4.CSI_LineNo is not 0 after changing CSI_Code", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument5.CSI_LineNo is not 0 after changing CSI_Code", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument6.CSI_LineNo is not 0 after changing CSI_Code", 2, supportingDocument6.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoWhenChangingCode_Phase5_ParentHeader_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = bill.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 before setting it", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 before setting it", 0, supportingDocument6.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 8;
					supportingDocument2.CSI_LineNo = 5;
					supportingDocument3.CSI_LineNo = 6;
					supportingDocument4.CSI_LineNo = 4;
					supportingDocument5.CSI_LineNo = 3;
					supportingDocument6.CSI_LineNo = 7;

					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before changing CSI_Code", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before changing CSI_Code", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before changing CSI_Code", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before changing CSI_Code", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before changing CSI_Code", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before changing CSI_Code", 7, supportingDocument6.CSI_LineNo);

					supportingDocument1.CSI_Code = "AAA";
					AssertEquals("When changing CSI_Code in one document, supportingDocument1.CSI_LineNo is 0 after changing CSI_Code", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument2.CSI_LineNo is 0 after changing CSI_Code", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument3.CSI_LineNo is 0 after changing CSI_Code", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument4.CSI_LineNo is 0 after changing CSI_Code", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument5.CSI_LineNo is 0 after changing CSI_Code", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument6.CSI_LineNo is 0 after changing CSI_Code", 0, supportingDocument6.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoWhenChangingCode_Phase5_ParentHeader_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = bill.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before changing CSI_Code", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before changing CSI_Code", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before changing CSI_Code", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before changing CSI_Code", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before changing CSI_Code", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before changing CSI_Code", 2, supportingDocument6.CSI_LineNo);

					supportingDocument1.CSI_Code = "AAA";
					AssertEquals("When changing CSI_Code in one document, supportingDocument1.CSI_LineNo is not 0 after changing CSI_Code", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument2.CSI_LineNo is not 0 after changing CSI_Code", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument3.CSI_LineNo is not 0 after changing CSI_Code", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument4.CSI_LineNo is not 0 after changing CSI_Code", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument5.CSI_LineNo is not 0 after changing CSI_Code", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When changing CSI_Code in one document, supportingDocument6.CSI_LineNo is not 0 after changing CSI_Code", 2, supportingDocument6.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoWhenChangingCode_Phase4_ParentItem()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument3 = goodsItem.SupportingDocuments.AddNew();
			var supportingDocument4 = goodsItem.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);

				supportingDocument1.CSI_LineNo = 8;
				supportingDocument2.CSI_LineNo = 5;
				supportingDocument3.CSI_LineNo = 6;
				supportingDocument4.CSI_LineNo = 4;

				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before changing CSI_Code", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before changing CSI_Code", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before changing CSI_Code", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before changing CSI_Code", 4, supportingDocument4.CSI_LineNo);

				supportingDocument3.CSI_Code = "AAA";
				AssertEquals("When changing CSI_Code in one document, supportingDocument1.CSI_LineNo is not 0 after changing CSI_Code", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("When changing CSI_Code in one document, supportingDocument2.CSI_LineNo is not 0 after changing CSI_Code", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("When changing CSI_Code in one document, supportingDocument3.CSI_LineNo is not 0 after changing CSI_Code", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("When changing CSI_Code in one document, supportingDocument4.CSI_LineNo is not 0 after changing CSI_Code", 4, supportingDocument4.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoWhenChangingCode_Phase4_ParentHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument3 = goodsItem.SupportingDocuments.AddNew();
			var supportingDocument4 = goodsItem.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);

				supportingDocument1.CSI_LineNo = 8;
				supportingDocument2.CSI_LineNo = 5;
				supportingDocument3.CSI_LineNo = 6;
				supportingDocument4.CSI_LineNo = 4;

				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before changing CSI_Code", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before changing CSI_Code", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before changing CSI_Code", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before changing CSI_Code", 4, supportingDocument4.CSI_LineNo);

				supportingDocument1.CSI_Code = "AAA";
				AssertEquals("When changing CSI_Code in one document, supportingDocument1.CSI_LineNo is not 0 after changing CSI_Code", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("When changing CSI_Code in one document, supportingDocument2.CSI_LineNo is not 0 after changing CSI_Code", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("When changing CSI_Code in one document, supportingDocument3.CSI_LineNo is not 0 after changing CSI_Code", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("When changing CSI_Code in one document, supportingDocument4.CSI_LineNo is not 0 after changing CSI_Code", 4, supportingDocument4.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentItem_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = bill.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 before setting it", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 before setting it", 0, supportingDocument6.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 8;
					supportingDocument2.CSI_LineNo = 5;
					supportingDocument3.CSI_LineNo = 6;
					supportingDocument4.CSI_LineNo = 4;
					supportingDocument5.CSI_LineNo = 3;
					supportingDocument6.CSI_LineNo = 7;

					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before saving", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before saving", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before saving", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before saving", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before saving", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before saving", 7, supportingDocument6.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after saving", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after saving", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after saving", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after saving", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after saving", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 after saving", 7, supportingDocument6.CSI_LineNo);

					var supportingDocument7 = goodsItem.SupportingDocuments.AddNew();
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument3.CSI_LineNo is not 0 after saving", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument4.CSI_LineNo is not 0 after saving", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 after saving", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 after saving", 0, supportingDocument6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument7.CSI_LineNo is 0 after saving, new item", 0, supportingDocument7.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 1;
					supportingDocument2.CSI_LineNo = 2;
					supportingDocument3.CSI_LineNo = 3;
					supportingDocument4.CSI_LineNo = 4;
					supportingDocument5.CSI_LineNo = 5;
					supportingDocument6.CSI_LineNo = 6;
					supportingDocument7.CSI_LineNo = 7;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after second saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after second saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after second saving", 3, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after second saving", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after second saving", 5, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 after second saving", 6, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument7.CSI_LineNo is not 0 after second saving", 7, supportingDocument7.CSI_LineNo);

					supportingDocument5.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument3.CSI_LineNo is not 0 after saving", 3, supportingDocument3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument4.CSI_LineNo is not 0 after saving", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 after saving, the item changed", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 after saving", 0, supportingDocument6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument7.CSI_LineNo is 0 after saving", 0, supportingDocument7.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentItem_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = bill.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before saving", 2, supportingDocument6.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 after saving", 2, supportingDocument6.CSI_LineNo);

					var supportingDocument7 = goodsItem.SupportingDocuments.AddNew();
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument3.CSI_LineNo is not 0 after saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument4.CSI_LineNo is not 0 after saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument5.CSI_LineNo is not 0 after saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument6.CSI_LineNo is not 0 after saving", 2, supportingDocument6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument7.CSI_LineNo is not 0 after saving, new item", 3, supportingDocument7.CSI_LineNo);

					supportingDocument7.CSI_LineNo = 5;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after second saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after second saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after second saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after second saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after second saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 after second saving", 2, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument7.CSI_LineNo is not 0 after second saving", 5, supportingDocument7.CSI_LineNo);

					supportingDocument6.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument3.CSI_LineNo is not 0 after saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument4.CSI_LineNo is not 0 after saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument5.CSI_LineNo is not 0 after saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 after saving, the item changed", 0, supportingDocument6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument7.CSI_LineNo is not 0 after saving", 5, supportingDocument7.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentBill_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = bill.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 before setting it", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 before setting it", 0, supportingDocument6.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 8;
					supportingDocument2.CSI_LineNo = 5;
					supportingDocument3.CSI_LineNo = 6;
					supportingDocument4.CSI_LineNo = 4;
					supportingDocument5.CSI_LineNo = 3;
					supportingDocument6.CSI_LineNo = 7;

					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before saving", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before saving", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before saving", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before saving", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before saving", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before saving", 7, supportingDocument6.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after saving", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after saving", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after saving", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after saving", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after saving", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 after saving", 7, supportingDocument6.CSI_LineNo);

					var supportingDocument7 = bill.SupportingDocuments.AddNew();
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 after saving", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 after saving", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 after saving", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 after saving", 0, supportingDocument6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument7.CSI_LineNo is 0 after saving, new item", 0, supportingDocument7.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 1;
					supportingDocument2.CSI_LineNo = 2;
					supportingDocument3.CSI_LineNo = 3;
					supportingDocument4.CSI_LineNo = 4;
					supportingDocument5.CSI_LineNo = 5;
					supportingDocument6.CSI_LineNo = 6;
					supportingDocument7.CSI_LineNo = 7;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after second saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after second saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after second saving", 3, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after second saving", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after second saving", 5, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 after second saving", 6, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument7.CSI_LineNo is not 0 after second saving", 7, supportingDocument7.CSI_LineNo);

					supportingDocument3.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 after saving, the item changed", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 after saving", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 after saving", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 after saving", 0, supportingDocument6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument7.CSI_LineNo is 0 after saving", 0, supportingDocument7.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentBill_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = bill.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before saving", 2, supportingDocument6.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 after saving", 2, supportingDocument6.CSI_LineNo);

					var supportingDocument7 = bill.SupportingDocuments.AddNew();
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument3.CSI_LineNo is not 0 after saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument4.CSI_LineNo is not 0 after saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument5.CSI_LineNo is not 0 after saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument6.CSI_LineNo is not 0 after saving", 2, supportingDocument6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument7.CSI_LineNo is not 0 after saving, new item", 3, supportingDocument7.CSI_LineNo);

					supportingDocument7.CSI_LineNo = 5;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after second saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after second saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after second saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after second saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after second saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 after second saving", 2, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument7.CSI_LineNo is not 0 after second saving", 5, supportingDocument7.CSI_LineNo);

					supportingDocument3.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 after saving, the item changed", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument4.CSI_LineNo is not 0 after saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument5.CSI_LineNo is not 0 after saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument6.CSI_LineNo is not 0 after saving", 2, supportingDocument6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument7.CSI_LineNo is not 0 after saving", 5, supportingDocument7.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentHeader_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = bill.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 before setting it", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 before setting it", 0, supportingDocument6.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 8;
					supportingDocument2.CSI_LineNo = 5;
					supportingDocument3.CSI_LineNo = 6;
					supportingDocument4.CSI_LineNo = 4;
					supportingDocument5.CSI_LineNo = 3;
					supportingDocument6.CSI_LineNo = 7;

					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before saving", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before saving", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before saving", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before saving", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before saving", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before saving", 7, supportingDocument6.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after saving", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after saving", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after saving", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after saving", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after saving", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 after saving", 7, supportingDocument6.CSI_LineNo);

					var supportingDocument7 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 after saving", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 after saving", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 after saving", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 after saving", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 after saving", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 after saving", 0, supportingDocument6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument7.CSI_LineNo is 0 after saving, new item", 0, supportingDocument7.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 1;
					supportingDocument2.CSI_LineNo = 2;
					supportingDocument3.CSI_LineNo = 3;
					supportingDocument4.CSI_LineNo = 4;
					supportingDocument5.CSI_LineNo = 5;
					supportingDocument6.CSI_LineNo = 6;
					supportingDocument7.CSI_LineNo = 7;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after second saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after second saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after second saving", 3, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after second saving", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after second saving", 5, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 after second saving", 6, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument7.CSI_LineNo is not 0 after second saving", 7, supportingDocument7.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 after saving, the item changed", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 after saving", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 after saving", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 after saving", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 after saving", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 after saving", 0, supportingDocument6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument7.CSI_LineNo is 0 after saving", 0, supportingDocument7.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentHeader_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = bill.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before saving", 2, supportingDocument6.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 after saving", 2, supportingDocument6.CSI_LineNo);

					var supportingDocument7 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument3.CSI_LineNo is not 0 after saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument4.CSI_LineNo is not 0 after saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument5.CSI_LineNo is not 0 after saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument6.CSI_LineNo is not 0 after saving", 2, supportingDocument6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument7.CSI_LineNo is not 0 after saving, new item", 3, supportingDocument7.CSI_LineNo);

					supportingDocument7.CSI_LineNo = 5;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after second saving", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after second saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after second saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after second saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after second saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 after second saving", 2, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument7.CSI_LineNo is not 0 after second saving", 5, supportingDocument7.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 after saving, the item changed", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument3.CSI_LineNo is not 0 after saving", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument4.CSI_LineNo is not 0 after saving", 2, supportingDocument4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument5.CSI_LineNo is not 0 after saving", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument6.CSI_LineNo is not 0 after saving", 2, supportingDocument6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument7.CSI_LineNo is not 0 after saving", 5, supportingDocument7.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase4_ParentItem()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument3 = goodsItem.SupportingDocuments.AddNew();
			var supportingDocument4 = goodsItem.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);

				supportingDocument1.CSI_LineNo = 8;
				supportingDocument2.CSI_LineNo = 5;
				supportingDocument3.CSI_LineNo = 6;
				supportingDocument4.CSI_LineNo = 4;

				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before saving", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before saving", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before saving", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before saving", 4, supportingDocument4.CSI_LineNo);

				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after saving", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after saving", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after saving", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after saving", 4, supportingDocument4.CSI_LineNo);

				var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
				Factory.Save();
				AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument3.CSI_LineNo is not 0 after saving", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument4.CSI_LineNo is not 0 after saving", 4, supportingDocument4.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 after saving, new item", 0, supportingDocument5.CSI_LineNo);

				supportingDocument1.CSI_LineNo = 1;
				supportingDocument2.CSI_LineNo = 2;
				supportingDocument3.CSI_LineNo = 3;
				supportingDocument4.CSI_LineNo = 4;
				supportingDocument5.CSI_LineNo = 5;
				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after second saving", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after second saving", 2, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after second saving", 3, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after second saving", 4, supportingDocument4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after second saving", 5, supportingDocument5.CSI_LineNo);

				supportingDocument4.CSI_LineNo = 0;
				Factory.Save();
				AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 2, supportingDocument2.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument3.CSI_LineNo is not 0 after saving", 3, supportingDocument3.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 after saving, the item changed", 0, supportingDocument4.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument5.CSI_LineNo is not 0 after saving", 5, supportingDocument5.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnSaving_Phase4_ParentHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument3 = goodsItem.SupportingDocuments.AddNew();
			var supportingDocument4 = goodsItem.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);

				supportingDocument1.CSI_LineNo = 8;
				supportingDocument2.CSI_LineNo = 5;
				supportingDocument3.CSI_LineNo = 6;
				supportingDocument4.CSI_LineNo = 4;

				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before saving", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before saving", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before saving", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before saving", 4, supportingDocument4.CSI_LineNo);

				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after saving", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after saving", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after saving", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after saving", 4, supportingDocument4.CSI_LineNo);

				var supportingDocument5 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				Factory.Save();
				AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument2.CSI_LineNo is not 0 after saving", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument3.CSI_LineNo is not 0 after saving", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument4.CSI_LineNo is not 0 after saving", 4, supportingDocument4.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 after saving, new item", 0, supportingDocument5.CSI_LineNo);

				supportingDocument1.CSI_LineNo = 1;
				supportingDocument2.CSI_LineNo = 2;
				supportingDocument3.CSI_LineNo = 3;
				supportingDocument4.CSI_LineNo = 4;
				supportingDocument5.CSI_LineNo = 5;
				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 after second saving", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 after second saving", 2, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 after second saving", 3, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 after second saving", 4, supportingDocument4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 after second saving", 5, supportingDocument5.CSI_LineNo);

				supportingDocument2.CSI_LineNo = 0;
				Factory.Save();
				AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument1.CSI_LineNo is not 0 after saving", 1, supportingDocument1.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 after saving, the item changed", 0, supportingDocument2.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument3.CSI_LineNo is not after saving", 3, supportingDocument3.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument4.CSI_LineNo is not 0 after saving", 4, supportingDocument4.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, supportingDocument5.CSI_LineNo is not 0 after saving", 5, supportingDocument5.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentItem_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = bill.SupportingDocuments.AddNew();
				var supportingDocument6 = bill.SupportingDocuments.AddNew();
				var supportingDocument7 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument8 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument9 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 before setting it", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 before setting it", 0, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument7.CSI_LineNo is 0 before setting it", 0, supportingDocument7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument8.CSI_LineNo is 0 before setting it", 0, supportingDocument8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument9.CSI_LineNo is 0 before setting it", 0, supportingDocument9.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 8;
					supportingDocument2.CSI_LineNo = 5;
					supportingDocument3.CSI_LineNo = 6;
					supportingDocument4.CSI_LineNo = 4;
					supportingDocument5.CSI_LineNo = 3;
					supportingDocument6.CSI_LineNo = 7;
					supportingDocument7.CSI_LineNo = 2;
					supportingDocument8.CSI_LineNo = 1;
					supportingDocument9.CSI_LineNo = 9;

					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before deleting a document", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before deleting a document", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before deleting a document", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before deleting a document", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before deleting a document", 7, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument7.CSI_LineNo is not 0 before deleting a document", 2, supportingDocument7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument8.CSI_LineNo is not 0 before deleting a document", 1, supportingDocument8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument9.CSI_LineNo is not 0 before deleting a document", 9, supportingDocument9.CSI_LineNo);

					supportingDocument7.Delete();
					AssertEquals("supportingDocument1.CSI_LineNo is not 0 after deleting a document, when not in database", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument4.CSI_LineNo is not 0 after deleting a document, when not in database", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("supportingDocument5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, supportingDocument6.CSI_LineNo);
					AssertEquals("supportingDocument8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, supportingDocument8.CSI_LineNo);
					AssertEquals("supportingDocument9.CSI_LineNo is not 0 after deleting a document, when not in database", 9, supportingDocument9.CSI_LineNo);

					Factory.Save();
					supportingDocument8.Delete();
					AssertEquals("supportingDocument1.CSI_LineNo is not 0 after deleting a document, when in database", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when in database", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when in database", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument4.CSI_LineNo is not 0 after deleting a document, when in database", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("supportingDocument5.CSI_LineNo is not 0 after deleting a document, when in database", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when in database", 7, supportingDocument6.CSI_LineNo);
					AssertEquals("supportingDocument9.CSI_LineNo is 0 after deleting a document, when in database", 0, supportingDocument9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentItem_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = bill.SupportingDocuments.AddNew();
				var supportingDocument6 = bill.SupportingDocuments.AddNew();
				var supportingDocument7 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument8 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument9 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before deleting a document", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before deleting a document", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before deleting a document", 1, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before deleting a document", 2, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument7.CSI_LineNo is not 0 before deleting a document", 1, supportingDocument7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument8.CSI_LineNo is not 0 before deleting a document", 2, supportingDocument8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument9.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument9.CSI_LineNo);

					supportingDocument7.Delete();
					AssertEquals("supportingDocument1.CSI_LineNo is not 0 after deleting a document, when not in database", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when not in database", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when not in database", 3, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument4.CSI_LineNo is not 0 after deleting a document, when not in database", 1, supportingDocument4.CSI_LineNo);
					AssertEquals("supportingDocument5.CSI_LineNo is not 0 after deleting a document, when not in database", 2, supportingDocument5.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when not in database", 3, supportingDocument6.CSI_LineNo);
					AssertEquals("supportingDocument8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, supportingDocument8.CSI_LineNo);
					AssertEquals("supportingDocument9.CSI_LineNo is not 0 after deleting a document, when not in database", 2, supportingDocument9.CSI_LineNo);

					Factory.Save();
					supportingDocument8.Delete();
					AssertEquals("supportingDocument1.CSI_LineNo is not 0 after deleting a document, when in database", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when in database", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when in database", 3, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument4.CSI_LineNo is not 0 after deleting a document, when in database", 1, supportingDocument4.CSI_LineNo);
					AssertEquals("supportingDocument5.CSI_LineNo is not 0 after deleting a document, when in database", 2, supportingDocument5.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when in database", 3, supportingDocument6.CSI_LineNo);
					AssertEquals("supportingDocument9.CSI_LineNo is not 0 after deleting a document, when in database", 1, supportingDocument9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentBill_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = bill.SupportingDocuments.AddNew();
				var supportingDocument6 = bill.SupportingDocuments.AddNew();
				var supportingDocument7 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument8 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument9 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 before setting it", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 before setting it", 0, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument7.CSI_LineNo is 0 before setting it", 0, supportingDocument7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument8.CSI_LineNo is 0 before setting it", 0, supportingDocument8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument9.CSI_LineNo is 0 before setting it", 0, supportingDocument9.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 8;
					supportingDocument2.CSI_LineNo = 5;
					supportingDocument3.CSI_LineNo = 6;
					supportingDocument4.CSI_LineNo = 4;
					supportingDocument5.CSI_LineNo = 3;
					supportingDocument6.CSI_LineNo = 7;
					supportingDocument7.CSI_LineNo = 2;
					supportingDocument8.CSI_LineNo = 1;
					supportingDocument9.CSI_LineNo = 9;

					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before deleting a document", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before deleting a document", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before deleting a document", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before deleting a document", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before deleting a document", 7, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument7.CSI_LineNo is not 0 before deleting a document", 2, supportingDocument7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument8.CSI_LineNo is not 0 before deleting a document", 1, supportingDocument8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument9.CSI_LineNo is not 0 before deleting a document", 9, supportingDocument9.CSI_LineNo);

					supportingDocument4.Delete();
					AssertEquals("supportingDocument1.CSI_LineNo is not 0 after deleting a document, when not in database", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, supportingDocument6.CSI_LineNo);
					AssertEquals("supportingDocument7.CSI_LineNo is not 0 after deleting a document, when not in database", 2, supportingDocument7.CSI_LineNo);
					AssertEquals("supportingDocument8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, supportingDocument8.CSI_LineNo);
					AssertEquals("supportingDocument9.CSI_LineNo is not 0 after deleting a document, when not in database", 9, supportingDocument9.CSI_LineNo);

					Factory.Save();
					supportingDocument5.Delete();
					AssertEquals("supportingDocument1.CSI_LineNo is not 0 after deleting a document, when in database", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when in database", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when in database", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is 0 after deleting a document, when in database", 0, supportingDocument6.CSI_LineNo);
					AssertEquals("supportingDocument7.CSI_LineNo is 0 after deleting a document, when in database", 0, supportingDocument7.CSI_LineNo);
					AssertEquals("supportingDocument8.CSI_LineNo is 0 after deleting a document, when in database", 0, supportingDocument8.CSI_LineNo);
					AssertEquals("supportingDocument9.CSI_LineNo is 0 after deleting a document, when in database", 0, supportingDocument9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentBill_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = bill.SupportingDocuments.AddNew();
				var supportingDocument6 = bill.SupportingDocuments.AddNew();
				var supportingDocument7 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument8 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument9 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before deleting a document", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before deleting a document", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before deleting a document", 1, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before deleting a document", 2, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument7.CSI_LineNo is not 0 before deleting a document", 1, supportingDocument7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument8.CSI_LineNo is not 0 before deleting a document", 2, supportingDocument8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument9.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument9.CSI_LineNo);

					supportingDocument4.Delete();
					AssertEquals("supportingDocument1.CSI_LineNo is not 0 after deleting a document, when not in database", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when not in database", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when not in database", 3, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument5.CSI_LineNo is not 0 after deleting a document, when not in database", 1, supportingDocument5.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when not in database", 2, supportingDocument6.CSI_LineNo);
					AssertEquals("supportingDocument7.CSI_LineNo is not 0 after deleting a document, when not in database", 1, supportingDocument7.CSI_LineNo);
					AssertEquals("supportingDocument8.CSI_LineNo is not 0 after deleting a document, when not in database", 2, supportingDocument8.CSI_LineNo);
					AssertEquals("supportingDocument9.CSI_LineNo is not 0 after deleting a document, when not in database", 3, supportingDocument9.CSI_LineNo);

					Factory.Save();
					supportingDocument5.Delete();
					AssertEquals("supportingDocument1.CSI_LineNo is not 0 after deleting a document, when in database", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when in database", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when in database", 3, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when in database", 1, supportingDocument6.CSI_LineNo);
					AssertEquals("supportingDocument7.CSI_LineNo is not 0 after deleting a document, when in database", 1, supportingDocument7.CSI_LineNo);
					AssertEquals("supportingDocument8.CSI_LineNo is not 0 after deleting a document, when in database", 2, supportingDocument8.CSI_LineNo);
					AssertEquals("supportingDocument9.CSI_LineNo is not 0 after deleting a document, when in database", 3, supportingDocument9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentHeader_TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = bill.SupportingDocuments.AddNew();
				var supportingDocument6 = bill.SupportingDocuments.AddNew();
				var supportingDocument7 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument8 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument9 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument5.CSI_LineNo is 0 before setting it", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument6.CSI_LineNo is 0 before setting it", 0, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument7.CSI_LineNo is 0 before setting it", 0, supportingDocument7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument8.CSI_LineNo is 0 before setting it", 0, supportingDocument8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, supportingDocument9.CSI_LineNo is 0 before setting it", 0, supportingDocument9.CSI_LineNo);

					supportingDocument1.CSI_LineNo = 8;
					supportingDocument2.CSI_LineNo = 5;
					supportingDocument3.CSI_LineNo = 6;
					supportingDocument4.CSI_LineNo = 4;
					supportingDocument5.CSI_LineNo = 3;
					supportingDocument6.CSI_LineNo = 7;
					supportingDocument7.CSI_LineNo = 2;
					supportingDocument8.CSI_LineNo = 1;
					supportingDocument9.CSI_LineNo = 9;

					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before deleting a document", 8, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before deleting a document", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before deleting a document", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before deleting a document", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before deleting a document", 7, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument7.CSI_LineNo is not 0 before deleting a document", 2, supportingDocument7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument8.CSI_LineNo is not 0 before deleting a document", 1, supportingDocument8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument9.CSI_LineNo is not 0 before deleting a document", 9, supportingDocument9.CSI_LineNo);

					supportingDocument1.Delete();
					AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, supportingDocument2.CSI_LineNo);
					AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument4.CSI_LineNo is not 0 after deleting a document, when not in database", 4, supportingDocument4.CSI_LineNo);
					AssertEquals("supportingDocument5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, supportingDocument5.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, supportingDocument6.CSI_LineNo);
					AssertEquals("supportingDocument7.CSI_LineNo is not 0 after deleting a document, when not in database", 2, supportingDocument7.CSI_LineNo);
					AssertEquals("supportingDocument8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, supportingDocument8.CSI_LineNo);
					AssertEquals("supportingDocument9.CSI_LineNo is not 0 after deleting a document, when not in database", 9, supportingDocument9.CSI_LineNo);

					Factory.Save();
					supportingDocument2.Delete();
					AssertEquals("supportingDocument3.CSI_LineNo is 0 after deleting a document, when in database", 0, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument4.CSI_LineNo is 0 after deleting a document, when in database", 0, supportingDocument4.CSI_LineNo);
					AssertEquals("supportingDocument5.CSI_LineNo is 0 after deleting a document, when in database", 0, supportingDocument5.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is 0 after deleting a document, when in database", 0, supportingDocument6.CSI_LineNo);
					AssertEquals("supportingDocument7.CSI_LineNo is 0 after deleting a document, when in database", 0, supportingDocument7.CSI_LineNo);
					AssertEquals("supportingDocument8.CSI_LineNo is 0 after deleting a document, when in database", 0, supportingDocument8.CSI_LineNo);
					AssertEquals("supportingDocument9.CSI_LineNo is 0 after deleting a document, when in database", 0, supportingDocument9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentHeader_NoTransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				var bill = nctsHeader.Bills.AddNew();
				var goodsItem = bill.GoodsItems.AddNew();

				var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument3 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
				var supportingDocument4 = bill.SupportingDocuments.AddNew();
				var supportingDocument5 = bill.SupportingDocuments.AddNew();
				var supportingDocument6 = bill.SupportingDocuments.AddNew();
				var supportingDocument7 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument8 = goodsItem.SupportingDocuments.AddNew();
				var supportingDocument9 = goodsItem.SupportingDocuments.AddNew();

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before deleting a document", 1, supportingDocument1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before deleting a document", 2, supportingDocument2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before deleting a document", 1, supportingDocument4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before deleting a document", 2, supportingDocument5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument7.CSI_LineNo is not 0 before deleting a document", 1, supportingDocument7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument8.CSI_LineNo is not 0 before deleting a document", 2, supportingDocument8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument9.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument9.CSI_LineNo);

					supportingDocument1.Delete();
					AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when not in database", 1, supportingDocument2.CSI_LineNo);
					AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when not in database", 2, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument4.CSI_LineNo is not 0 after deleting a document, when not in database", 1, supportingDocument4.CSI_LineNo);
					AssertEquals("supportingDocument5.CSI_LineNo is not 0 after deleting a document, when not in database", 2, supportingDocument5.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when not in database", 3, supportingDocument6.CSI_LineNo);
					AssertEquals("supportingDocument7.CSI_LineNo is not 0 after deleting a document, when not in database", 1, supportingDocument7.CSI_LineNo);
					AssertEquals("supportingDocument8.CSI_LineNo is not 0 after deleting a document, when not in database", 2, supportingDocument8.CSI_LineNo);
					AssertEquals("supportingDocument9.CSI_LineNo is not 0 after deleting a document, when not in database", 3, supportingDocument9.CSI_LineNo);

					Factory.Save();
					supportingDocument2.Delete();
					AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when in database", 1, supportingDocument3.CSI_LineNo);
					AssertEquals("supportingDocument4.CSI_LineNo is not 0 after deleting a document, when in database", 1, supportingDocument4.CSI_LineNo);
					AssertEquals("supportingDocument5.CSI_LineNo is not 0 after deleting a document, when in database", 2, supportingDocument5.CSI_LineNo);
					AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when in database", 3, supportingDocument6.CSI_LineNo);
					AssertEquals("supportingDocument7.CSI_LineNo is not 0 after deleting a document, when in database", 1, supportingDocument7.CSI_LineNo);
					AssertEquals("supportingDocument8.CSI_LineNo is not 0 after deleting a document, when in database", 2, supportingDocument8.CSI_LineNo);
					AssertEquals("supportingDocument9.CSI_LineNo is not 0 after deleting a document, when in database", 3, supportingDocument9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase4_ParentItem()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument3 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument4 = goodsItem.SupportingDocuments.AddNew();
			var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
			var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument6.CSI_LineNo);

				supportingDocument1.CSI_LineNo = 8;
				supportingDocument2.CSI_LineNo = 5;
				supportingDocument3.CSI_LineNo = 6;
				supportingDocument4.CSI_LineNo = 4;
				supportingDocument5.CSI_LineNo = 3;
				supportingDocument6.CSI_LineNo = 7;

				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before deleting a document", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before deleting a document", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before deleting a document", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before deleting a document", 4, supportingDocument4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before deleting a document", 7, supportingDocument6.CSI_LineNo);

				supportingDocument4.Delete();
				AssertEquals("supportingDocument1.CSI_LineNo is not 0 after deleting a document, when not in database", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("supportingDocument5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, supportingDocument5.CSI_LineNo);
				AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, supportingDocument6.CSI_LineNo);

				Factory.Save();
				supportingDocument5.Delete();
				AssertEquals("supportingDocument1.CSI_LineNo is not 0 after deleting a document, when in database", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when in database", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when in database", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when in database", 7, supportingDocument6.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnDelete_Phase4_ParentHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var supportingDocument1 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument2 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument3 = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			var supportingDocument4 = goodsItem.SupportingDocuments.AddNew();
			var supportingDocument5 = goodsItem.SupportingDocuments.AddNew();
			var supportingDocument6 = goodsItem.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument1.CSI_LineNo is 0 before setting it", 0, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument2.CSI_LineNo is 0 before setting it", 0, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument3.CSI_LineNo is 0 before setting it", 0, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, supportingDocument4.CSI_LineNo is 0 before setting it", 0, supportingDocument6.CSI_LineNo);

				supportingDocument1.CSI_LineNo = 8;
				supportingDocument2.CSI_LineNo = 5;
				supportingDocument3.CSI_LineNo = 6;
				supportingDocument4.CSI_LineNo = 4;
				supportingDocument5.CSI_LineNo = 3;
				supportingDocument6.CSI_LineNo = 7;

				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument1.CSI_LineNo is not 0 before deleting a document", 8, supportingDocument1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument2.CSI_LineNo is not 0 before deleting a document", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument3.CSI_LineNo is not 0 before deleting a document", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument4.CSI_LineNo is not 0 before deleting a document", 4, supportingDocument4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument5.CSI_LineNo is not 0 before deleting a document", 3, supportingDocument5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, supportingDocument6.CSI_LineNo is not 0 before deleting a document", 7, supportingDocument6.CSI_LineNo);

				supportingDocument1.Delete();
				AssertEquals("supportingDocument2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, supportingDocument2.CSI_LineNo);
				AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("supportingDocument4.CSI_LineNo is not 0 after deleting a document, when not in database", 4, supportingDocument4.CSI_LineNo);
				AssertEquals("supportingDocument5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, supportingDocument5.CSI_LineNo);
				AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, supportingDocument6.CSI_LineNo);

				Factory.Save();
				supportingDocument2.Delete();
				AssertEquals("supportingDocument3.CSI_LineNo is not 0 after deleting a document, when in database", 6, supportingDocument3.CSI_LineNo);
				AssertEquals("supportingDocument4.CSI_LineNo is not 0 after deleting a document, when in database", 4, supportingDocument4.CSI_LineNo);
				AssertEquals("supportingDocument5.CSI_LineNo is not 0 after deleting a document, when in database", 3, supportingDocument5.CSI_LineNo);
				AssertEquals("supportingDocument6.CSI_LineNo is not 0 after deleting a document, when in database", 7, supportingDocument6.CSI_LineNo);
			});
		}

		public void TestCloneCSI_LineNo()
		{
			supportingDocument.CSI_LineNo = 5;
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("TransitPeriod: Test CSI_LineNo of the original document", 5, supportingDocument.CSI_LineNo);
					var clonedSupportingDocument = (NctsSupportingDocument)supportingDocument.Clone();
					AssertEquals("TransitPeriod: Test CSI_LineNo is not cloned when cloning document", ZInt.Zero, clonedSupportingDocument.CSI_LineNo);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("Test CSI_LineNo of the original document", 5, supportingDocument.CSI_LineNo);
					var clonedSupportingDocument = (NctsSupportingDocument)supportingDocument.Clone();
					AssertEquals("Test CSI_LineNo is not cloned when cloning document", 5, clonedSupportingDocument.CSI_LineNo);
				}
			});
		}

		public void TestReadOnlyProviderType()
		{
			var (arrivalSupportingDocument1, arrivalSupportingDocument2) = CreateAndGetSupportingDocumentForTest(NctsMovementType.Codes.Arrival);
			var (departureSupportingDocument1, departureSupportingDocument2) = CreateAndGetSupportingDocumentForTest(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				AssertType<NctsSupportingDocumentPhase5ArrivalReadOnlyProvider>("When Ncts IsArrival Phase 5 and Parent is bill, NctsSupportingDocumentPhase5ArrivalReadOnlyProvider is ES", arrivalSupportingDocument1.GetNewReadOnlyProvider());
				AssertType<NctsSupportingDocumentPhase5ArrivalReadOnlyProvider>("When Ncts IsArrival Phase 5 and Parent is goodsItem, NctsSupportingDocumentPhase5ArrivalReadOnlyProvider is ES", arrivalSupportingDocument2.GetNewReadOnlyProvider());

				AssertEquals("When Ncts is not IsArrival Phase 5 but Parent is bill, SupportingDocumentReadOnlyProvider is EU", "Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocumentPhase5DepartureReadOnlyProvider", departureSupportingDocument1.GetNewReadOnlyProvider().GetType().FullName);
				AssertEquals("When Ncts is not IsArrival Phase 5 but Parent is goodsItem, SupportingDocumentReadOnlyProvider is EU", "Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocumentPhase5DepartureReadOnlyProvider", departureSupportingDocument2.GetNewReadOnlyProvider().GetType().FullName);
			});
		}

		public void TestCloneSupportingDocumentWithCSI_Lineno()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty SupportingDocument list", 0, goodsItem.SupportingDocuments.Count);

				var supDoc1 = goodsItem.SupportingDocuments.AddNew();
				supDoc1.CSI_Code = "9001";
				supDoc1.CSI_LineNo = 1;

				var supDoc4 = goodsItem.SupportingDocuments.AddNew();
				supDoc4.CSI_Code = "5004";
				supDoc4.CSI_LineNo = 4;

				var supDoc3 = goodsItem.SupportingDocuments.AddNew();
				supDoc3.CSI_Code = "A003";
				supDoc3.CSI_LineNo = 3;

				var supDoc2 = goodsItem.SupportingDocuments.AddNew();
				supDoc2.CSI_Code = "Y001";
				supDoc2.CSI_LineNo = 2;
				Factory.Save();

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					var documents = goodsItem.SupportingDocuments;
					var clonedSupportingDocument1 = (NctsSupportingDocument)supDoc1.Clone();
					var clonedSupportingDocument2 = (NctsSupportingDocument)supDoc2.Clone();
					var clonedSupportingDocument3 = (NctsSupportingDocument)supDoc3.Clone();
					var clonedSupportingDocument4 = (NctsSupportingDocument)supDoc4.Clone();

					AssertEquals("FinalPeriod: Expected filled SupportingDocument", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled SupportingDocuments ordered Name", new ZString[] { "9001", "5004", "A003", "Y001" }, documents.Select(x => x.CSI_Code));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled SupportingDocuments ordered SequenceNumber", new ZInt[] { 1, 4, 3, 2 }, documents.Select(x => x.CSI_LineNo));

					AssertEquals("Cloned SupportingDocuments 1 CSI_Code", supDoc1.CSI_Code, clonedSupportingDocument1.CSI_Code);
					AssertEquals("Cloned SupportingDocuments 2 CSI_Code", supDoc2.CSI_Code, clonedSupportingDocument2.CSI_Code);
					AssertEquals("Cloned SupportingDocuments 3 CSI_Code", supDoc3.CSI_Code, clonedSupportingDocument3.CSI_Code);
					AssertEquals("Cloned SupportingDocuments 4 CSI_Code", supDoc4.CSI_Code, clonedSupportingDocument4.CSI_Code);
					AssertEquals("Cloned SupportingDocuments 1 CSI_LineNo", supDoc1.CSI_LineNo, clonedSupportingDocument1.CSI_LineNo);
					AssertEquals("Cloned SupportingDocuments 2 CSI_LineNo", supDoc2.CSI_LineNo, clonedSupportingDocument2.CSI_LineNo);
					AssertEquals("Cloned SupportingDocuments 3 CSI_LineNo", supDoc3.CSI_LineNo, clonedSupportingDocument3.CSI_LineNo);
					AssertEquals("Cloned SupportingDocuments 4 CSI_LineNo", supDoc4.CSI_LineNo, clonedSupportingDocument4.CSI_LineNo);
				}
			});
		}

		(NctsSupportingDocumentForTest, NctsSupportingDocumentForTest) CreateAndGetSupportingDocumentForTest(ZString movementType)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(movementType);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var nctsBill = nctsHeader.Bills.AddNew();
			var supportingDocument1 = Factory.New<NctsSupportingDocumentForTest>();
			var supportingDocument2 = Factory.New<NctsSupportingDocumentForTest>();

			supportingDocument1.AttachToParent(nctsBill);

			BusinessObject goodsItem = movementType.Equals(NctsMovementType.Codes.Arrival) ? nctsBill.ArrivalGoodsItems.AddNew() : nctsBill.GoodsItems.AddNew();
			supportingDocument2.AttachToParent(goodsItem);

			return (supportingDocument1, supportingDocument2);
		}

		protected override IEnumerable<NctsSupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var supportingDoc = goodsItem.SupportingDocuments.AddNew();
			yield return supportingDoc;
		}

		protected override BusinessObject GetNewBusinessObject() => supportingDocument;

		protected override void SetUp()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			supportingDocument = goodsItem.SupportingDocuments.AddNew();
		}
		NctsSupportingDocument supportingDocument;

		class NctsSupportingDocumentForTest : NctsSupportingDocument
		{
			public NctsSupportingDocumentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public void AttachToParent(BusinessObject parent)
			{
				CSI_ParentTableCode = parent.TablePrefix;
				CSI_ParentID = parent.PK;
			}

			public new ISupportingDocumentReadOnlyConditions GetNewReadOnlyProvider() => base.GetNewReadOnlyProvider();

			public bool AutomaticSequenceNumberEnabled_Exposed => AutomaticSequenceNumberEnabled;

			public bool CSI_LineNo_ReadOnly_Exposed => CSI_LineNo_ReadOnly;
		}
	}
}
