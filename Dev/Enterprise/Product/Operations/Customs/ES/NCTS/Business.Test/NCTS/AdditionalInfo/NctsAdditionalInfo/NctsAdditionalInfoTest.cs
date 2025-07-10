using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsAdditionalInfo))]
	public class NctsAdditionalInfoTest : EU.NCTS.Business.Testing.NctsAdditionalInfoTest<NctsAdditionalInfo>
	{
		public void TestLookupsTypePhase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var additionalInfo = goodsItem.AdditionalInfos.AddNew();
			AssertType<NctsAdditionalInfoLookups>(additionalInfo.Lookups);
		}

		public void TestLookupsTypePhase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var additionalInfo = goodsItem.AdditionalInfos.AddNew();
			AssertType<NctsAdditionalInfoPhase5Lookups>(additionalInfo.Lookups);
		}

		public void TestCSI_LineNo_ReadOnly()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var additionalInfo = Factory.New<NctsAdditionalInfoForTest>();
			goodsItem.AdditionalInfos.Add(additionalInfo);
			AssertEquals("CSI_LineNo_ReadOnly for ES, true", true, additionalInfo.CSI_LineNo_ReadOnly_Exposed);
		}

		public void TestDefaultSubType()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("default subtype for parent goodsItem is empty", ZString.Empty, goodsItem.AdditionalInfos.AddNew().CSI_SubType);
				AssertEquals("default subtype for parent header is empty", ZString.Empty, nctsHeader.AdditionalDocuments.AddNew().CSI_SubType);
			});
		}

		public void TestAutomaticSequenceNumberEnabled_Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var additionalInfo = Factory.New<NctsAdditionalInfoForTest>();
			goodsItem.AdditionalInfos.Add(additionalInfo);

			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					AssertEquals("When transition period and phase5, false", false, additionalInfo.AutomaticSequenceNumberEnabled_Exposed);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					AssertEquals("When transition period and phase4, false", false, additionalInfo.AutomaticSequenceNumberEnabled_Exposed);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
				{
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					AssertEquals("When no transition period and phase5, true", true, additionalInfo.AutomaticSequenceNumberEnabled_Exposed);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					AssertEquals("When no transition period and phase4, false", false, additionalInfo.AutomaticSequenceNumberEnabled_Exposed);
				}
			});
		}

		public void TestAutomaticSequenceNumberEnabled_Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var additionalInfo = Factory.New<NctsAdditionalInfoForTest>();
			nctsHeader.ArrivalMovementHeader.AdditionalDocuments.Add(additionalInfo);

			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					AssertEquals("When transition period and phase5, true", true, additionalInfo.AutomaticSequenceNumberEnabled_Exposed);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					AssertEquals("When transition period and phase4, false", false, additionalInfo.AutomaticSequenceNumberEnabled_Exposed);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
							Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
				{
					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					AssertEquals("When no transition period and phase5, true", true, additionalInfo.AutomaticSequenceNumberEnabled_Exposed);

					nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					AssertEquals("When no transition period and phase4, false", false, additionalInfo.AutomaticSequenceNumberEnabled_Exposed);
				}
			});
		}

		public void TestResetCSI_LineNoWhenChangingSubType_Phase5_ParentItem_TransitionPeriod()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "INF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "REF";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "TRA";

				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "INF";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "REF";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "TRA";

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "INF";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "REF";
				var additionalInfo10 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo10.CSI_SubType = "REF";
				var additionalInfo11 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo11.CSI_SubType = "TRA";
				var additionalInfo12 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo12.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 before setting it", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo10.CSI_LineNo is 0 before setting it", 0, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo11.CSI_LineNo is 0 before setting it", 0, additionalInfo11.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo12.CSI_LineNo is 0 before setting it", 0, additionalInfo12.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;
					additionalInfo9.CSI_LineNo = 9;
					additionalInfo10.CSI_LineNo = 10;
					additionalInfo11.CSI_LineNo = 11;
					additionalInfo12.CSI_LineNo = 12;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before changing a SubType", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before changing a SubType", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before changing a SubType", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before changing a SubType", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before changing a SubType", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before changing a SubType", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before changing a SubType", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before changing a SubType", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before changing a SubType", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before changing a SubType", 11, additionalInfo11.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo12.CSI_LineNo is not 0 before changing a SubType", 12, additionalInfo12.CSI_LineNo);

					additionalInfo9.CSI_SubType = "INF";
					AssertEquals("When changing a SubType from REF to INF, additionalInfo1.CSI_LineNo is not 0 after changing a SubType", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo2.CSI_LineNo is not 0 after changing a SubType", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo3.CSI_LineNo is not 0 after changing a SubType", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo4.CSI_LineNo is not 0 after changing a SubType", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo5.CSI_LineNo is not 0 after changing a SubType", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo6.CSI_LineNo is not 0 after changing a SubType", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo7.CSI_LineNo is 0 after changing a SubType", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo8.CSI_LineNo is 0 after changing a SubType", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo9.CSI_LineNo is 0 after changing a SubType", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo10.CSI_LineNo is 0 after changing a SubType", 0, additionalInfo10.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo11.CSI_LineNo is not 0 after changing a SubType", 11, additionalInfo11.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo12.CSI_LineNo is not 0 after changing a SubType", 12, additionalInfo12.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoWhenChangingSubType_Phase5_ParentItem_NoTransitionPeriod()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "INF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "REF";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "TRA";

				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "INF";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "REF";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "TRA";

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "INF";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "REF";
				var additionalInfo10 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo10.CSI_SubType = "REF";
				var additionalInfo11 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo11.CSI_SubType = "TRA";
				var additionalInfo12 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo12.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before changing a SubType", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before changing a SubType", 2, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo11.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo12.CSI_LineNo is not 0 before changing a SubType", 2, additionalInfo12.CSI_LineNo);

					additionalInfo9.CSI_SubType = "INF";
					AssertEquals("When changing a SubType from REF to INF, additionalInfo1.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo2.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo3.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo4.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo5.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo6.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo7.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo8.CSI_LineNo is not 0 after changing a SubType", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo9.CSI_LineNo is not 0 after changing a SubType", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo10.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo11.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo11.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo12.CSI_LineNo is not 0 after changing a SubType", 2, additionalInfo12.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoWhenChangingSubType_Phase5_ParentHeader_TransitionPeriod()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "INF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "REF";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "TRA";

				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "INF";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "REF";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "TRA";

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "INF";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "REF";
				var additionalInfo10 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo10.CSI_SubType = "REF";
				var additionalInfo11 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo11.CSI_SubType = "TRA";
				var additionalInfo12 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo12.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 before setting it", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo10.CSI_LineNo is 0 before setting it", 0, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo11.CSI_LineNo is 0 before setting it", 0, additionalInfo11.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo12.CSI_LineNo is 0 before setting it", 0, additionalInfo12.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;
					additionalInfo9.CSI_LineNo = 9;
					additionalInfo10.CSI_LineNo = 10;
					additionalInfo11.CSI_LineNo = 11;
					additionalInfo12.CSI_LineNo = 12;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before changing a SubType", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before changing a SubType", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before changing a SubType", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before changing a SubType", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before changing a SubType", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before changing a SubType", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before changing a SubType", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before changing a SubType", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before changing a SubType", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before changing a SubType", 11, additionalInfo11.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo12.CSI_LineNo is not 0 before changing a SubType", 12, additionalInfo12.CSI_LineNo);

					additionalInfo2.CSI_SubType = "INF";
					AssertEquals("When changing a SubType from REF to INF, additionalInfo1.CSI_LineNo is 0 after changing a SubType", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo2.CSI_LineNo is 0 after changing a SubType", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo3.CSI_LineNo is not 0 after changing a SubType", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo4.CSI_LineNo is 0 after changing a SubType", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo5.CSI_LineNo is 0 after changing a SubType", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo6.CSI_LineNo is not 0 after changing a SubType", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo7.CSI_LineNo is 0 after changing a SubType", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo8.CSI_LineNo is 0 after changing a SubType", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo9.CSI_LineNo is 0 after changing a SubType", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo10.CSI_LineNo is 0 after changing a SubType", 0, additionalInfo10.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo11.CSI_LineNo is not 0 after changing a SubType", 11, additionalInfo11.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo12.CSI_LineNo is not 0 after changing a SubType", 12, additionalInfo12.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoWhenChangingSubType_Phase5_ParentHeader_NoTransitionPeriod()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "INF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "REF";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "TRA";

				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "INF";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "REF";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "TRA";

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "INF";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "REF";
				var additionalInfo10 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo10.CSI_SubType = "REF";
				var additionalInfo11 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo11.CSI_SubType = "TRA";
				var additionalInfo12 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo12.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before changing a SubType", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before changing a SubType", 2, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo11.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo12.CSI_LineNo is not 0 before changing a SubType", 2, additionalInfo12.CSI_LineNo);

					additionalInfo2.CSI_SubType = "INF";
					AssertEquals("When changing a SubType from REF to INF, additionalInfo1.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo2.CSI_LineNo is not 0 after changing a SubType", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo3.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo4.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo5.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo6.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo7.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo8.CSI_LineNo is not 0 after changing a SubType", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo9.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo9.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo10.CSI_LineNo is not 0 after changing a SubType", 2, additionalInfo10.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo11.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo11.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo12.CSI_LineNo is not 0 after changing a SubType", 2, additionalInfo12.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoWhenChangingSubType_Phase4_ParentItem()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "INF";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "REF";
			var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo3.CSI_SubType = "TRA";

			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "INF";
			var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo5.CSI_SubType = "INF";
			var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo6.CSI_SubType = "REF";
			var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo7.CSI_SubType = "REF";
			var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo8.CSI_SubType = "TRA";
			var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo9.CSI_SubType = "TRA";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 before setting it", 0, additionalInfo9.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;
				additionalInfo7.CSI_LineNo = 2;
				additionalInfo8.CSI_LineNo = 1;
				additionalInfo9.CSI_LineNo = 9;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before changing a SubType", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before changing a SubType", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before changing a SubType", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before changing a SubType", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before changing a SubType", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before changing a SubType", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before changing a SubType", 2, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo8.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before changing a SubType", 9, additionalInfo9.CSI_LineNo);

				additionalInfo6.CSI_SubType = "INF";
				AssertEquals("When changing a SubType from REF to INF, additionalInfo1.CSI_LineNo is not 0 after changing a SubType", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo2.CSI_LineNo is not 0 after changing a SubType", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo3.CSI_LineNo is not 0 after changing a SubType", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo4.CSI_LineNo is not 0 after changing a SubType", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo5.CSI_LineNo is not 0 after changing a SubType", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo6.CSI_LineNo is not 0 after changing a SubType", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo7.CSI_LineNo is not 0 after changing a SubType", 2, additionalInfo7.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo8.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo8.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo9.CSI_LineNo is not 0 after changing a SubType", 9, additionalInfo9.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoWhenChangingSubType_Phase4_ParentHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "INF";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "REF";
			var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo3.CSI_SubType = "TRA";

			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "INF";
			var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo5.CSI_SubType = "INF";
			var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo6.CSI_SubType = "REF";
			var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo7.CSI_SubType = "REF";
			var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo8.CSI_SubType = "TRA";
			var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo9.CSI_SubType = "TRA";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 before setting it", 0, additionalInfo9.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;
				additionalInfo7.CSI_LineNo = 2;
				additionalInfo8.CSI_LineNo = 1;
				additionalInfo9.CSI_LineNo = 9;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before changing a SubType", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before changing a SubType", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before changing a SubType", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before changing a SubType", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before changing a SubType", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before changing a SubType", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before changing a SubType", 2, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before changing a SubType", 1, additionalInfo8.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before changing a SubType", 9, additionalInfo9.CSI_LineNo);

				additionalInfo2.CSI_SubType = "INF";
				AssertEquals("When changing a SubType from REF to INF, additionalInfo1.CSI_LineNo is not 0 after changing a SubType", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo2.CSI_LineNo is not 0 after changing a SubType", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo3.CSI_LineNo is not 0 after changing a SubType", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo4.CSI_LineNo is not 0 after changing a SubType", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo5.CSI_LineNo is not 0 after changing a SubType", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo6.CSI_LineNo is not 0 after changing a SubType", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo7.CSI_LineNo is not 0 after changing a SubType", 2, additionalInfo7.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo8.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo8.CSI_LineNo);
				AssertEquals("When changing a SubType from REF to INF, additionalInfo9.CSI_LineNo is not 0 after changing a SubType", 9, additionalInfo9.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentItem_TransitionPeriod_INF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "INF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "INF";
				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "INF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "INF";
				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = "INF";
				var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo6.CSI_SubType = "INF";

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "REF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before saving", 1, additionalInfo8.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);

					var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
					additionalInfo9.CSI_SubType = "INF";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving, new item", 0, additionalInfo9.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 1;
					additionalInfo2.CSI_LineNo = 2;
					additionalInfo3.CSI_LineNo = 3;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 5;
					additionalInfo6.CSI_LineNo = 6;
					additionalInfo7.CSI_LineNo = 7;
					additionalInfo8.CSI_LineNo = 8;
					additionalInfo9.CSI_LineNo = 9;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 5, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 6, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after second saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 after second saving", 9, additionalInfo9.CSI_LineNo);

					additionalInfo5.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 not after saving", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 not after saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving", 0, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentItem_NoTransitionPeriod_INF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "INF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "INF";
				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "INF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "INF";
				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = "INF";
				var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo6.CSI_SubType = "INF";

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "REF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before saving", 1, additionalInfo8.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);

					var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
					additionalInfo9.CSI_SubType = "INF";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving, new item", 3, additionalInfo9.CSI_LineNo);

					additionalInfo9.CSI_LineNo = 5;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after second saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 after second saving", 5, additionalInfo9.CSI_LineNo);

					additionalInfo5.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving", 5, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentHeader_TransitionPeriod_INF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "INF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "INF";
				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "INF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "INF";
				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = "INF";
				var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo6.CSI_SubType = "INF";

				var additionalInfo7 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "REF";
				var additionalInfo8 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo8.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before saving", 1, additionalInfo8.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);

					var additionalInfo9 = nctsHeader.AdditionalDocuments.AddNew();
					additionalInfo9.CSI_SubType = "INF";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 after saving", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 after saving", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving, new item", 0, additionalInfo9.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 1;
					additionalInfo2.CSI_LineNo = 2;
					additionalInfo3.CSI_LineNo = 3;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 5;
					additionalInfo6.CSI_LineNo = 6;
					additionalInfo7.CSI_LineNo = 7;
					additionalInfo8.CSI_LineNo = 8;
					additionalInfo9.CSI_LineNo = 9;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 5, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 6, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after second saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 after second saving", 9, additionalInfo9.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 after saving", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving", 0, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentHeader_NoTransitionPeriod_INF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "INF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "INF";
				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "INF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "INF";
				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = "INF";
				var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo6.CSI_SubType = "INF";

				var additionalInfo7 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "REF";
				var additionalInfo8 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo8.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before saving", 1, additionalInfo8.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);

					var additionalInfo9 = nctsHeader.AdditionalDocuments.AddNew();
					additionalInfo9.CSI_SubType = "INF";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving, new item", 3, additionalInfo9.CSI_LineNo);

					additionalInfo9.CSI_LineNo = 5;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after second saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 after second saving", 5, additionalInfo9.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving", 5, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentItem_TransitionPeriod_REF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "REF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "REF";
				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "REF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "REF";
				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = "REF";
				var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo6.CSI_SubType = "REF";

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before saving", 1, additionalInfo8.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);

					var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
					additionalInfo9.CSI_SubType = "REF";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving, new item", 0, additionalInfo9.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 1;
					additionalInfo2.CSI_LineNo = 2;
					additionalInfo3.CSI_LineNo = 3;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 5;
					additionalInfo6.CSI_LineNo = 6;
					additionalInfo7.CSI_LineNo = 7;
					additionalInfo8.CSI_LineNo = 8;
					additionalInfo9.CSI_LineNo = 9;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 5, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 6, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after second saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 after second saving", 9, additionalInfo9.CSI_LineNo);

					additionalInfo5.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 not after saving", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 not after saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving", 0, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentItem_NoTransitionPeriod_REF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "REF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "REF";
				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "REF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "REF";
				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = "REF";
				var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo6.CSI_SubType = "REF";

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before saving", 1, additionalInfo8.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);

					var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
					additionalInfo9.CSI_SubType = "REF";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving, new item", 3, additionalInfo9.CSI_LineNo);

					additionalInfo9.CSI_LineNo = 5;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after second saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 after second saving", 5, additionalInfo9.CSI_LineNo);

					additionalInfo5.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving", 5, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentHeader_TransitionPeriod_REF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "REF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "REF";
				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "REF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "REF";
				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = "REF";
				var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo6.CSI_SubType = "REF";

				var additionalInfo7 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo8.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before saving", 1, additionalInfo8.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);

					var additionalInfo9 = nctsHeader.AdditionalDocuments.AddNew();
					additionalInfo9.CSI_SubType = "REF";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 after saving", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 after saving", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving, new item", 0, additionalInfo9.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 1;
					additionalInfo2.CSI_LineNo = 2;
					additionalInfo3.CSI_LineNo = 3;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 5;
					additionalInfo6.CSI_LineNo = 6;
					additionalInfo7.CSI_LineNo = 7;
					additionalInfo8.CSI_LineNo = 8;
					additionalInfo9.CSI_LineNo = 9;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 5, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 6, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after second saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 after second saving", 9, additionalInfo9.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 after saving", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving", 0, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentHeader_NoTransitionPeriod_REF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "REF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "REF";
				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "REF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "REF";
				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = "REF";
				var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo6.CSI_SubType = "REF";

				var additionalInfo7 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo8.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before saving", 1, additionalInfo8.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);

					var additionalInfo9 = nctsHeader.AdditionalDocuments.AddNew();
					additionalInfo9.CSI_SubType = "REF";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving, new item", 3, additionalInfo9.CSI_LineNo);

					additionalInfo9.CSI_LineNo = 5;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after second saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 after second saving", 5, additionalInfo9.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving", 5, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentItem_TransitionPeriod_TRA()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "TRA";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "TRA";
				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "TRA";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "TRA";
				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = "TRA";
				var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo6.CSI_SubType = "TRA";

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "REF";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before saving", 1, additionalInfo8.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);

					var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
					additionalInfo9.CSI_SubType = "TRA";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving, new item", 0, additionalInfo9.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 1;
					additionalInfo2.CSI_LineNo = 2;
					additionalInfo3.CSI_LineNo = 3;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 5;
					additionalInfo6.CSI_LineNo = 6;
					additionalInfo7.CSI_LineNo = 7;
					additionalInfo8.CSI_LineNo = 8;
					additionalInfo9.CSI_LineNo = 9;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 5, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 6, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after second saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 after second saving", 9, additionalInfo9.CSI_LineNo);

					additionalInfo5.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 not after saving", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 not after saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving", 0, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentItem_NoTransitionPeriod_TRA()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "TRA";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "TRA";
				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "TRA";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "TRA";
				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = "TRA";
				var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo6.CSI_SubType = "TRA";

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "REF";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before saving", 1, additionalInfo8.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);

					var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
					additionalInfo9.CSI_SubType = "TRA";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving, new item", 3, additionalInfo9.CSI_LineNo);

					additionalInfo9.CSI_LineNo = 5;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after second saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 after second saving", 5, additionalInfo9.CSI_LineNo);

					additionalInfo5.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving", 5, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentHeader_TransitionPeriod_TRA()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "TRA";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "TRA";
				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "TRA";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "TRA";
				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = "TRA";
				var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo6.CSI_SubType = "TRA";

				var additionalInfo7 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo8.CSI_SubType = "REF";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before saving", 1, additionalInfo8.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);

					var additionalInfo9 = nctsHeader.AdditionalDocuments.AddNew();
					additionalInfo9.CSI_SubType = "TRA";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 after saving", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 after saving", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving, new item", 0, additionalInfo9.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 1;
					additionalInfo2.CSI_LineNo = 2;
					additionalInfo3.CSI_LineNo = 3;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 5;
					additionalInfo6.CSI_LineNo = 6;
					additionalInfo7.CSI_LineNo = 7;
					additionalInfo8.CSI_LineNo = 8;
					additionalInfo9.CSI_LineNo = 9;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 5, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 6, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after second saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 after second saving", 9, additionalInfo9.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 after saving", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving", 0, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentHeader_NoTransitionPeriod_TRA()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "TRA";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "TRA";
				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "TRA";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "TRA";
				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = "TRA";
				var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo6.CSI_SubType = "TRA";

				var additionalInfo7 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo8.CSI_SubType = "REF";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before saving", 1, additionalInfo8.CSI_LineNo);

					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);

					var additionalInfo9 = nctsHeader.AdditionalDocuments.AddNew();
					additionalInfo9.CSI_SubType = "TRA";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving, new item", 3, additionalInfo9.CSI_LineNo);

					additionalInfo9.CSI_LineNo = 5;
					Factory.Save();
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 after second saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 after second saving", 5, additionalInfo9.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving", 5, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase4_ParentItem_INF()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "INF";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "INF";
			var additionalInfo3 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo3.CSI_SubType = "INF";
			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "INF";

			var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo5.CSI_SubType = "REF";
			var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo6.CSI_SubType = "TRA";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 7, additionalInfo6.CSI_LineNo);

				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				Factory.Save();
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 after saving, new item", 0, additionalInfo7.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 1;
				additionalInfo2.CSI_LineNo = 2;
				additionalInfo3.CSI_LineNo = 3;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 5;
				additionalInfo6.CSI_LineNo = 6;
				additionalInfo7.CSI_LineNo = 7;
				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 3, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 5, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 6, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 7, additionalInfo7.CSI_LineNo);

				additionalInfo4.CSI_LineNo = 0;
				Factory.Save();
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 3, additionalInfo3.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 5, additionalInfo5.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 6, additionalInfo6.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnSaving_Phase4_ParentHeader_INF()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "INF";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "INF";
			var additionalInfo3 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo3.CSI_SubType = "INF";
			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "INF";

			var additionalInfo5 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo5.CSI_SubType = "REF";
			var additionalInfo6 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo6.CSI_SubType = "TRA";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 7, additionalInfo6.CSI_LineNo);

				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);

				var additionalInfo7 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				Factory.Save();
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 after saving, new item", 0, additionalInfo7.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 1;
				additionalInfo2.CSI_LineNo = 2;
				additionalInfo3.CSI_LineNo = 3;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 5;
				additionalInfo6.CSI_LineNo = 6;
				additionalInfo7.CSI_LineNo = 7;
				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 3, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 5, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 6, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 7, additionalInfo7.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 0;
				Factory.Save();
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 3, additionalInfo3.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 5, additionalInfo5.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 6, additionalInfo6.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnSaving_Phase4_ParentItem_REF()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "REF";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "REF";
			var additionalInfo3 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo3.CSI_SubType = "REF";
			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "REF";

			var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo5.CSI_SubType = "INF";
			var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo6.CSI_SubType = "TRA";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 7, additionalInfo6.CSI_LineNo);

				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "REF";
				Factory.Save();
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 after saving, new item", 0, additionalInfo7.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 1;
				additionalInfo2.CSI_LineNo = 2;
				additionalInfo3.CSI_LineNo = 3;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 5;
				additionalInfo6.CSI_LineNo = 6;
				additionalInfo7.CSI_LineNo = 7;
				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 3, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 5, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 6, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 7, additionalInfo7.CSI_LineNo);

				additionalInfo4.CSI_LineNo = 0;
				Factory.Save();
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 3, additionalInfo3.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 5, additionalInfo5.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 6, additionalInfo6.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnSaving_Phase4_ParentHeader_REF()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "REF";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "REF";
			var additionalInfo3 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo3.CSI_SubType = "REF";
			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "REF";

			var additionalInfo5 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo5.CSI_SubType = "INF";
			var additionalInfo6 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo6.CSI_SubType = "TRA";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 7, additionalInfo6.CSI_LineNo);

				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);

				var additionalInfo7 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "REF";
				Factory.Save();
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 after saving, new item", 0, additionalInfo7.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 1;
				additionalInfo2.CSI_LineNo = 2;
				additionalInfo3.CSI_LineNo = 3;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 5;
				additionalInfo6.CSI_LineNo = 6;
				additionalInfo7.CSI_LineNo = 7;
				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 3, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 5, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 6, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 7, additionalInfo7.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 0;
				Factory.Save();
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 3, additionalInfo3.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 5, additionalInfo5.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 6, additionalInfo6.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnSaving_Phase4_ParentItem_TRA()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "TRA";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "TRA";
			var additionalInfo3 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo3.CSI_SubType = "TRA";
			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "TRA";

			var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo5.CSI_SubType = "INF";
			var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo6.CSI_SubType = "REF";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 7, additionalInfo6.CSI_LineNo);

				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);

				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "TRA";
				Factory.Save();
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 after saving, new item", 0, additionalInfo7.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 1;
				additionalInfo2.CSI_LineNo = 2;
				additionalInfo3.CSI_LineNo = 3;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 5;
				additionalInfo6.CSI_LineNo = 6;
				additionalInfo7.CSI_LineNo = 7;
				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 3, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 5, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 6, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 7, additionalInfo7.CSI_LineNo);

				additionalInfo4.CSI_LineNo = 0;
				Factory.Save();
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 3, additionalInfo3.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 5, additionalInfo5.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 6, additionalInfo6.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnSaving_Phase4_ParentHeader_TRA()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "TRA";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "TRA";
			var additionalInfo3 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo3.CSI_SubType = "TRA";
			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "TRA";

			var additionalInfo5 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo5.CSI_SubType = "REF";
			var additionalInfo6 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo6.CSI_SubType = "INF";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before saving", 7, additionalInfo6.CSI_LineNo);

				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);

				var additionalInfo7 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "TRA";
				Factory.Save();
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 after saving, new item", 0, additionalInfo7.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 1;
				additionalInfo2.CSI_LineNo = 2;
				additionalInfo3.CSI_LineNo = 3;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 5;
				additionalInfo6.CSI_LineNo = 6;
				additionalInfo7.CSI_LineNo = 7;
				Factory.Save();
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 after second saving", 1, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 after second saving", 2, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 after second saving", 3, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 after second saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 after second saving", 5, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 after second saving", 6, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 after second saving", 7, additionalInfo7.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 0;
				Factory.Save();
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is not 0 after saving", 3, additionalInfo3.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 5, additionalInfo5.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 6, additionalInfo6.CSI_LineNo);
				AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentItem_TransitionPeriod_INF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "INF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "INF";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "INF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "INF";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "INF";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "INF";
				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "INF";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "INF";

				var additionalInfo10 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo10.CSI_SubType = "REF";
				var additionalInfo11 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo11.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 before setting it", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo10.CSI_LineNo is 0 before setting it", 0, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo11.CSI_LineNo is 0 before setting it", 0, additionalInfo11.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;
					additionalInfo9.CSI_LineNo = 9;
					additionalInfo10.CSI_LineNo = 10;
					additionalInfo11.CSI_LineNo = 11;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before deleting a document", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before deleting a document", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before deleting a document", 11, additionalInfo11.CSI_LineNo);

					additionalInfo7.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 11, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo8.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when in database", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 11, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentItem_NoTransitionPeriod_INF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "INF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "INF";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "INF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "INF";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "INF";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "INF";
				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "INF";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "INF";

				var additionalInfo10 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo10.CSI_SubType = "REF";
				var additionalInfo11 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo11.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo11.CSI_LineNo);

					additionalInfo7.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo8.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentHeader_TransitionPeriod_INF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "INF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "INF";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "INF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "INF";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "INF";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "INF";
				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "INF";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "INF";

				var additionalInfo10 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo10.CSI_SubType = "REF";
				var additionalInfo11 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo11.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 before setting it", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo10.CSI_LineNo is 0 before setting it", 0, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo11.CSI_LineNo is 0 before setting it", 0, additionalInfo11.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;
					additionalInfo9.CSI_LineNo = 9;
					additionalInfo10.CSI_LineNo = 10;
					additionalInfo11.CSI_LineNo = 11;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before deleting a document", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before deleting a document", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before deleting a document", 11, additionalInfo11.CSI_LineNo);

					additionalInfo1.Delete();
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 11, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo2.Delete();
					AssertEquals("additionalInfo3.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 11, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentHeader_NoTransitionPeriod_INF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "INF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "INF";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "INF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "INF";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "INF";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "INF";
				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "INF";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "INF";

				var additionalInfo10 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo10.CSI_SubType = "REF";
				var additionalInfo11 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo11.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo11.CSI_LineNo);

					additionalInfo1.Delete();
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo2.Delete();
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentItem_TransitionPeriod_REF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "REF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "REF";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "REF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "REF";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "REF";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "REF";
				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "REF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "REF";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "REF";

				var additionalInfo10 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo10.CSI_SubType = "INF";
				var additionalInfo11 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo11.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 before setting it", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo10.CSI_LineNo is 0 before setting it", 0, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo11.CSI_LineNo is 0 before setting it", 0, additionalInfo11.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;
					additionalInfo9.CSI_LineNo = 9;
					additionalInfo10.CSI_LineNo = 10;
					additionalInfo11.CSI_LineNo = 11;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before deleting a document", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before deleting a document", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before deleting a document", 11, additionalInfo11.CSI_LineNo);

					additionalInfo7.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 11, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo8.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when in database", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 11, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentItem_NoTransitionPeriod_REF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "REF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "REF";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "REF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "REF";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "REF";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "REF";
				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "REF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "REF";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "REF";

				var additionalInfo10 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo10.CSI_SubType = "INF";
				var additionalInfo11 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo11.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo11.CSI_LineNo);

					additionalInfo7.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo8.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentHeader_TransitionPeriod_REF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "REF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "REF";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "REF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "REF";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "REF";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "REF";
				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "REF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "REF";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "REF";

				var additionalInfo10 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo10.CSI_SubType = "INF";
				var additionalInfo11 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo11.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 before setting it", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo10.CSI_LineNo is 0 before setting it", 0, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo11.CSI_LineNo is 0 before setting it", 0, additionalInfo11.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;
					additionalInfo9.CSI_LineNo = 9;
					additionalInfo10.CSI_LineNo = 10;
					additionalInfo11.CSI_LineNo = 11;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before deleting a document", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before deleting a document", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before deleting a document", 11, additionalInfo11.CSI_LineNo);

					additionalInfo1.Delete();
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 11, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo2.Delete();
					AssertEquals("additionalInfo3.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 11, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentHeader_NoTransitionPeriod_REF()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "REF";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "REF";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "REF";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "REF";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "REF";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "REF";
				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "REF";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "REF";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "REF";

				var additionalInfo10 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo10.CSI_SubType = "INF";
				var additionalInfo11 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo11.CSI_SubType = "TRA";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo11.CSI_LineNo);

					additionalInfo1.Delete();
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo2.Delete();
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentItem_TransitionPeriod_TRA()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "TRA";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "TRA";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "TRA";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "TRA";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "TRA";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "TRA";
				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "TRA";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "TRA";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "TRA";

				var additionalInfo10 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo10.CSI_SubType = "INF";
				var additionalInfo11 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo11.CSI_SubType = "REF";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 before setting it", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo10.CSI_LineNo is 0 before setting it", 0, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo11.CSI_LineNo is 0 before setting it", 0, additionalInfo11.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;
					additionalInfo9.CSI_LineNo = 9;
					additionalInfo10.CSI_LineNo = 10;
					additionalInfo11.CSI_LineNo = 11;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before deleting a document", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before deleting a document", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before deleting a document", 11, additionalInfo11.CSI_LineNo);

					additionalInfo7.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 11, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo8.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when in database", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 11, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentItem_NoTransitionPeriod_TRA()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "TRA";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "TRA";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "TRA";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "TRA";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "TRA";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "TRA";
				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "TRA";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "TRA";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "TRA";

				var additionalInfo10 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo10.CSI_SubType = "INF";
				var additionalInfo11 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo11.CSI_SubType = "REF";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo11.CSI_LineNo);

					additionalInfo7.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo8.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentHeader_TransitionPeriod_TRA()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "TRA";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "TRA";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "TRA";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "TRA";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "TRA";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "TRA";
				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "TRA";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "TRA";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "TRA";

				var additionalInfo10 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo10.CSI_SubType = "INF";
				var additionalInfo11 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo11.CSI_SubType = "REF";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 before setting it", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo10.CSI_LineNo is 0 before setting it", 0, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo 0, additionalInfo11.CSI_LineNo is 0 before setting it", 0, additionalInfo11.CSI_LineNo);

					additionalInfo1.CSI_LineNo = 8;
					additionalInfo2.CSI_LineNo = 5;
					additionalInfo3.CSI_LineNo = 6;
					additionalInfo4.CSI_LineNo = 4;
					additionalInfo5.CSI_LineNo = 3;
					additionalInfo6.CSI_LineNo = 7;
					additionalInfo7.CSI_LineNo = 2;
					additionalInfo8.CSI_LineNo = 1;
					additionalInfo9.CSI_LineNo = 9;
					additionalInfo10.CSI_LineNo = 10;
					additionalInfo11.CSI_LineNo = 11;

					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before deleting a document", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before deleting a document", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before deleting a document", 11, additionalInfo11.CSI_LineNo);

					additionalInfo1.Delete();
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 4, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 11, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo2.Delete();
					AssertEquals("additionalInfo3.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 11, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentHeader_NoTransitionPeriod_TRA()
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

				var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = "TRA";
				var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = "TRA";
				var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = "TRA";
				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = "TRA";
				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = "TRA";
				var additionalInfo6 = bill.AdditionalDocuments.AddNew();
				additionalInfo6.CSI_SubType = "TRA";
				var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo7.CSI_SubType = "TRA";
				var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo8.CSI_SubType = "TRA";
				var additionalInfo9 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo9.CSI_SubType = "TRA";

				var additionalInfo10 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo10.CSI_SubType = "INF";
				var additionalInfo11 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInfo11.CSI_SubType = "REF";

				CombineAssertions(() =>
				{
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo9.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo10.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo11.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo11.CSI_LineNo);

					additionalInfo1.Delete();
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo2.Delete();
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase4_ParentItem_INF()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "INF";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "INF";
			var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo3.CSI_SubType = "INF";
			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "INF";
			var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo5.CSI_SubType = "INF";
			var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo6.CSI_SubType = "INF";

			var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo7.CSI_SubType = "REF";
			var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo8.CSI_SubType = "TRA";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;
				additionalInfo7.CSI_LineNo = 2;
				additionalInfo8.CSI_LineNo = 1;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo8.CSI_LineNo);

				additionalInfo4.Delete();
				AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);

				Factory.Save();
				additionalInfo5.Delete();
				AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 7, additionalInfo6.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnDelete_Phase4_ParentHeader_INF()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "INF";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "INF";
			var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo3.CSI_SubType = "INF";
			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "INF";
			var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo5.CSI_SubType = "INF";
			var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo6.CSI_SubType = "INF";

			var additionalInfo7 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo7.CSI_SubType = "REF";
			var additionalInfo8 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo8.CSI_SubType = "TRA";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;
				additionalInfo7.CSI_LineNo = 2;
				additionalInfo8.CSI_LineNo = 1;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo8.CSI_LineNo);

				additionalInfo1.Delete();
				AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);

				Factory.Save();
				additionalInfo2.Delete();
				AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when in database", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 7, additionalInfo6.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnDelete_Phase4_ParentItem_REF()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "REF";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "REF";
			var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo3.CSI_SubType = "REF";
			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "REF";
			var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo5.CSI_SubType = "REF";
			var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo6.CSI_SubType = "REF";

			var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo7.CSI_SubType = "INF";
			var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo8.CSI_SubType = "TRA";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;
				additionalInfo7.CSI_LineNo = 2;
				additionalInfo8.CSI_LineNo = 1;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo8.CSI_LineNo);

				additionalInfo4.Delete();
				AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);

				Factory.Save();
				additionalInfo5.Delete();
				AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 7, additionalInfo6.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnDelete_Phase4_ParentHeader_REF()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "REF";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "REF";
			var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo3.CSI_SubType = "REF";
			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "REF";
			var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo5.CSI_SubType = "REF";
			var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo6.CSI_SubType = "REF";

			var additionalInfo7 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo7.CSI_SubType = "INF";
			var additionalInfo8 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo8.CSI_SubType = "TRA";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;
				additionalInfo7.CSI_LineNo = 2;
				additionalInfo8.CSI_LineNo = 1;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo8.CSI_LineNo);

				additionalInfo1.Delete();
				AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);

				Factory.Save();
				additionalInfo2.Delete();
				AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when in database", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 7, additionalInfo6.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnDelete_Phase4_ParentItem_TRA()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "TRA";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "TRA";
			var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo3.CSI_SubType = "TRA";
			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "TRA";
			var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo5.CSI_SubType = "TRA";
			var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo6.CSI_SubType = "TRA";

			var additionalInfo7 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo7.CSI_SubType = "INF";
			var additionalInfo8 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo8.CSI_SubType = "REF";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;
				additionalInfo7.CSI_LineNo = 2;
				additionalInfo8.CSI_LineNo = 1;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo8.CSI_LineNo);

				additionalInfo4.Delete();
				AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);

				Factory.Save();
				additionalInfo5.Delete();
				AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 7, additionalInfo6.CSI_LineNo);
			});
		}

		public void TestResetCSI_LineNoOnDelete_Phase4_ParentHeader_TRA()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_CustomsStatus = "PRE";
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var additionalInfo1 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo1.CSI_SubType = "TRA";
			var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo2.CSI_SubType = "TRA";
			var additionalInfo3 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo3.CSI_SubType = "TRA";
			var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo4.CSI_SubType = "TRA";
			var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo5.CSI_SubType = "TRA";
			var additionalInfo6 = goodsItem.AdditionalInfos.AddNew();
			additionalInfo6.CSI_SubType = "TRA";

			var additionalInfo7 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo7.CSI_SubType = "INF";
			var additionalInfo8 = nctsHeader.AdditionalDocuments.AddNew();
			additionalInfo8.CSI_SubType = "REF";

			CombineAssertions(() =>
			{
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo1.CSI_LineNo is 0 before setting it", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo2.CSI_LineNo is 0 before setting it", 0, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 before setting it", 0, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 before setting it", 0, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 before setting it", 0, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 before setting it", 0, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 before setting it", 0, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo 0, additionalInfo8.CSI_LineNo is 0 before setting it", 0, additionalInfo8.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 8;
				additionalInfo2.CSI_LineNo = 5;
				additionalInfo3.CSI_LineNo = 6;
				additionalInfo4.CSI_LineNo = 4;
				additionalInfo5.CSI_LineNo = 3;
				additionalInfo6.CSI_LineNo = 7;
				additionalInfo7.CSI_LineNo = 2;
				additionalInfo8.CSI_LineNo = 1;

				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo1.CSI_LineNo is not 0 before deleting a document", 8, additionalInfo1.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo2.CSI_LineNo is not 0 before deleting a document", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo3.CSI_LineNo is not 0 before deleting a document", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo4.CSI_LineNo is not 0 before deleting a document", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo5.CSI_LineNo is not 0 before deleting a document", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo6.CSI_LineNo is not 0 before deleting a document", 7, additionalInfo6.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo7.CSI_LineNo is not 0 before deleting a document", 2, additionalInfo7.CSI_LineNo);
				AssertEquals("When all docs have CSI_LineNo not 0, additionalInfo8.CSI_LineNo is not 0 before deleting a document", 1, additionalInfo8.CSI_LineNo);

				additionalInfo1.Delete();
				AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
				AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when not in database", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);

				Factory.Save();
				additionalInfo2.Delete();
				AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 6, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo4.CSI_LineNo is not 0 after deleting a document, when in database", 4, additionalInfo4.CSI_LineNo);
				AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo5.CSI_LineNo);
				AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 7, additionalInfo6.CSI_LineNo);
			});
		}

		public void TestCloneCSI_LineNo()
		{
			var additionalInfo = (NctsAdditionalInfo)GetNewBusinessObject();
			additionalInfo.CSI_LineNo = 5;
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("TransitPeriod: Test CSI_LineNo of the original document", 5, additionalInfo.CSI_LineNo);
					var clonedAdditionalInfo = (NctsAdditionalInfo)additionalInfo.Clone();
					AssertEquals("TransitPeriod: Test CSI_LineNo is not cloned when cloning document", ZInt.Zero, clonedAdditionalInfo.CSI_LineNo);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("Test CSI_LineNo of the original document", 5, additionalInfo.CSI_LineNo);
					var clonedAdditionalInfo = (NctsAdditionalInfo)additionalInfo.Clone();
					AssertEquals("Test CSI_LineNo is not cloned when cloning document", 5, clonedAdditionalInfo.CSI_LineNo);
				}
			});
		}

		public void TestCloneAdditionalInfoWithCSI_Lineno()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty AdditionalInfos list", 0, goodsItem.AdditionalInfos.Count);

				var addInf1 = goodsItem.AdditionalInfos.AddNew();
				addInf1.CSI_Code = "9001";
				addInf1.CSI_LineNo = 1;

				var addInf4 = goodsItem.AdditionalInfos.AddNew();
				addInf4.CSI_Code = "5004";
				addInf4.CSI_LineNo = 4;

				var addInf3 = goodsItem.AdditionalInfos.AddNew();
				addInf3.CSI_Code = "A003";
				addInf3.CSI_LineNo = 3;

				var addInf2 = goodsItem.AdditionalInfos.AddNew();
				addInf2.CSI_Code = "Y001";
				addInf2.CSI_LineNo = 2;
				Factory.Save();

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					var documents = goodsItem.AdditionalInfos;
					var clonedAdditionalInfos1 = (NctsAdditionalInfo)addInf1.Clone();
					var clonedAdditionalInfos2 = (NctsAdditionalInfo)addInf2.Clone();
					var clonedAdditionalInfos3 = (NctsAdditionalInfo)addInf3.Clone();
					var clonedAdditionalInfos4 = (NctsAdditionalInfo)addInf4.Clone();

					AssertEquals("FinalPeriod: Expected filled AdditionalInfos", 4, documents.Count);
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInfos ordered Name", new ZString[] { "9001", "5004", "A003", "Y001" }, documents.Select(x => x.CSI_Code));
					AssertContainsExactElementsInExactOrder("FinalPeriod: Expected filled AdditionalInfos ordered SequenceNumber", new ZInt[] { 1, 4, 3, 2 }, documents.Select(x => x.CSI_LineNo));

					AssertEquals("Cloned AdditionalInfos 1 CSI_Code", addInf1.CSI_Code, clonedAdditionalInfos1.CSI_Code);
					AssertEquals("Cloned AdditionalInfos 2 CSI_Code", addInf2.CSI_Code, clonedAdditionalInfos2.CSI_Code);
					AssertEquals("Cloned AdditionalInfos 3 CSI_Code", addInf3.CSI_Code, clonedAdditionalInfos3.CSI_Code);
					AssertEquals("Cloned AdditionalInfos 4 CSI_Code", addInf4.CSI_Code, clonedAdditionalInfos4.CSI_Code);
					AssertEquals("Cloned AdditionalInfos 1 CSI_LineNo", addInf1.CSI_LineNo, clonedAdditionalInfos1.CSI_LineNo);
					AssertEquals("Cloned AdditionalInfos 2 CSI_LineNo", addInf2.CSI_LineNo, clonedAdditionalInfos2.CSI_LineNo);
					AssertEquals("Cloned AdditionalInfos 3 CSI_LineNo", addInf3.CSI_LineNo, clonedAdditionalInfos3.CSI_LineNo);
					AssertEquals("Cloned AdditionalInfos 4 CSI_LineNo", addInf4.CSI_LineNo, clonedAdditionalInfos4.CSI_LineNo);
				}
			});
		}

		public void TestReadOnlyProviderType()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var additionalDocument = Factory.New<NctsAdditionalInfoForTest>();
			var movementHeaderDocument = Factory.New<NctsAdditionalInfoForTest>();

			var bill = header.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			additionalDocument.AttachToParent(goodsItem);
			movementHeaderDocument.AttachToParent(header.ArrivalMovementHeader);

			var header2 = Factory.New<NctsHeader>();
			header2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header2.SetMovementType(NctsMovementType.Codes.Arrival);
			var otherDocument = Factory.New<NctsAdditionalInfoForTest>();
			otherDocument.AttachToParent(header2.ArrivalMovementHeader);

			CombineAssertions(() =>
			{
				AssertType<NctsArrivalAdditionalDocumentReadOnlyProvider>("When Ncts IsArrival Phase 5 and Parent is goodsItem, AdditionalDocumentReadOnlyProvider is ES", additionalDocument.GetNewReadOnlyProvider());
				AssertType<NctsArrivalAdditionalDocumentReadOnlyProvider>("When Ncts IsArrival Phase 5 and Parent is movementHeader, AdditionalDocumentReadOnlyProvider is ES", movementHeaderDocument.GetNewReadOnlyProvider());
				AssertEquals("When Ncts is not IsArrival Phase 5, AdditionalDocumentReadOnlyProvider is EU", "Enterprise.Customs.EU.NCTS.Business.NctsHeaderArrivalAdditionalDocumentReadOnlyProvider", otherDocument.GetNewReadOnlyProvider().GetType().FullName);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			return goodsItem.AdditionalInfos.AddNew();
		}

		class NctsAdditionalInfoForTest : NctsAdditionalInfo
		{
			public NctsAdditionalInfoForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public void AttachToParent(BusinessObject parent)
			{
				CSI_ParentTableCode = parent.TablePrefix;
				CSI_ParentID = parent.PK;
			}

			public new IAdditionalDocumentReadOnlyProvider GetNewReadOnlyProvider() => base.GetNewReadOnlyProvider();

			public bool AutomaticSequenceNumberEnabled_Exposed => AutomaticSequenceNumberEnabled;

			public bool CSI_LineNo_ReadOnly_Exposed => CSI_LineNo_ReadOnly;
		}
	}
}
