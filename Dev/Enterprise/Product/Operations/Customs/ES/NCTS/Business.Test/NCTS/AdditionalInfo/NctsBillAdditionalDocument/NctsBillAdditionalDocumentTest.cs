using System.Collections.Generic;
using System.Data;
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
	[TestedType(typeof(NctsBillAdditionalDocument))]
	class NctsBillAdditionalDocumentTest : CusSupportingInfoTest<NctsBillAdditionalDocument>
	{
		public void TestAutomaticSequenceNumberEnabled_Departure()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			var additionalInfo = Factory.New<NctsBillAdditionalDocumentForTest>();
			bill.AdditionalDocuments.Add(additionalInfo);

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
			var bill = nctsHeader.Bills.AddNew();
			var additionalInfo = Factory.New<NctsBillAdditionalDocumentForTest>();
			bill.AdditionalDocuments.Add(additionalInfo);

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

		public void TestResetCSI_LineNoWhenChangingSubType_Phase5_ParentBill_TransitionPeriod()
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

					additionalInfo5.CSI_SubType = "INF";
					AssertEquals("When changing a SubType from REF to INF, additionalInfo1.CSI_LineNo is not 0 after changing a SubType", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo2.CSI_LineNo is not 0 after changing a SubType", 5, additionalInfo2.CSI_LineNo);
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

		public void TestResetCSI_LineNoWhenChangingSubType_Phase5_ParentBill_NoTransitionPeriod()
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

					additionalInfo5.CSI_SubType = "INF";
					AssertEquals("When changing a SubType from REF to INF, additionalInfo1.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo2.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo3.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo4.CSI_LineNo is not 0 after changing a SubType", 1, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a SubType from REF to INF, additionalInfo5.CSI_LineNo is not 0 after changing a SubType", 2, additionalInfo5.CSI_LineNo);
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

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentBill_TransitionPeriod_INF()
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

				var additionalInfo7 = bill.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "REF";
				var additionalInfo8 = bill.AdditionalDocuments.AddNew();
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

					var additionalInfo9 = bill.AdditionalDocuments.AddNew();
					additionalInfo9.CSI_SubType = "INF";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 after saving, new item", 0, additionalInfo9.CSI_LineNo);

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

					additionalInfo3.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving", 0, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentBill_NoTransitionPeriod_INF()
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

				var additionalInfo7 = bill.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "REF";
				var additionalInfo8 = bill.AdditionalDocuments.AddNew();
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

					var additionalInfo9 = bill.AdditionalDocuments.AddNew();
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

					additionalInfo3.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving", 5, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentBill_TransitionPeriod_REF()
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

				var additionalInfo7 = bill.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = bill.AdditionalDocuments.AddNew();
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

					var additionalInfo9 = bill.AdditionalDocuments.AddNew();
					additionalInfo9.CSI_SubType = "REF";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 after saving, new item", 0, additionalInfo9.CSI_LineNo);

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

					additionalInfo3.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving", 0, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentBill_NoTransitionPeriod_REF()
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

				var additionalInfo7 = bill.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = bill.AdditionalDocuments.AddNew();
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

					var additionalInfo9 = bill.AdditionalDocuments.AddNew();
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

					additionalInfo3.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving", 5, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentBill_TransitionPeriod_TRA()
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

				var additionalInfo7 = bill.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = bill.AdditionalDocuments.AddNew();
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

					var additionalInfo9 = bill.AdditionalDocuments.AddNew();
					additionalInfo9.CSI_SubType = "TRA";
					Factory.Save();
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When adding a new doc with CSI_LineNo 0, additionalInfo7.CSI_LineNo is 0 after saving, new item", 0, additionalInfo9.CSI_LineNo);

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

					additionalInfo3.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is 0 after saving", 0, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is 0 after saving", 0, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is 0 after saving", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 7, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 8, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is 0 after saving", 0, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnSaving_Phase5_ParentBill_NoTransitionPeriod_TRA()
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

				var additionalInfo7 = bill.AdditionalDocuments.AddNew();
				additionalInfo7.CSI_SubType = "INF";
				var additionalInfo8 = bill.AdditionalDocuments.AddNew();
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

					var additionalInfo9 = bill.AdditionalDocuments.AddNew();
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

					additionalInfo3.CSI_LineNo = 0;
					Factory.Save();
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo1.CSI_LineNo is not 0 after saving", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo2.CSI_LineNo is not 0 after saving", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo3.CSI_LineNo is 0 after saving, the item changed", 0, additionalInfo3.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo4.CSI_LineNo is not 0 after saving", 2, additionalInfo4.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo5.CSI_LineNo is not 0 after saving", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo6.CSI_LineNo is not 0 after saving", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo7.CSI_LineNo is not 0 after saving", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo8.CSI_LineNo is not 0 after saving", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("When changing a doc and setting CSI_LineNo 0, additionalInfo9.CSI_LineNo is not 0 after saving", 5, additionalInfo9.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentBill_TransitionPeriod_INF()
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

				var additionalInfo10 = bill.AdditionalDocuments.AddNew();
				additionalInfo10.CSI_SubType = "REF";
				var additionalInfo11 = bill.AdditionalDocuments.AddNew();
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

					additionalInfo4.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 11, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo5.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 11, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentBill_NoTransitionPeriod_INF()
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

				var additionalInfo10 = bill.AdditionalDocuments.AddNew();
				additionalInfo10.CSI_SubType = "REF";
				var additionalInfo11 = bill.AdditionalDocuments.AddNew();
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

					additionalInfo4.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo5.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentBill_TransitionPeriod_REF()
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

				var additionalInfo10 = bill.AdditionalDocuments.AddNew();
				additionalInfo10.CSI_SubType = "INF";
				var additionalInfo11 = bill.AdditionalDocuments.AddNew();
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

					additionalInfo4.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 11, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo5.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 11, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentBill_NoTransitionPeriod_REF()
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

				var additionalInfo10 = bill.AdditionalDocuments.AddNew();
				additionalInfo10.CSI_SubType = "INF";
				var additionalInfo11 = bill.AdditionalDocuments.AddNew();
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

					additionalInfo4.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo5.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentBill_TransitionPeriod_TRA()
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

				var additionalInfo10 = bill.AdditionalDocuments.AddNew();
				additionalInfo10.CSI_SubType = "INF";
				var additionalInfo11 = bill.AdditionalDocuments.AddNew();
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

					additionalInfo4.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 7, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 9, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 11, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo5.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 8, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 5, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 6, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is 0 after deleting a document, when in database", 0, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 10, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 11, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestResetCSI_LineNoOnDelete_Phase5_ParentBill_NoTransitionPeriod_TRA()
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

				var additionalInfo10 = bill.AdditionalDocuments.AddNew();
				additionalInfo10.CSI_SubType = "INF";
				var additionalInfo11 = bill.AdditionalDocuments.AddNew();
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

					additionalInfo4.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo5.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo5.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when not in database", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when not in database", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when not in database", 1, additionalInfo11.CSI_LineNo);

					Factory.Save();
					additionalInfo5.Delete();
					AssertEquals("additionalInfo1.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo1.CSI_LineNo);
					AssertEquals("additionalInfo2.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo2.CSI_LineNo);
					AssertEquals("additionalInfo3.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo3.CSI_LineNo);
					AssertEquals("additionalInfo6.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo6.CSI_LineNo);
					AssertEquals("additionalInfo7.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo7.CSI_LineNo);
					AssertEquals("additionalInfo8.CSI_LineNo is not 0 after deleting a document, when in database", 2, additionalInfo8.CSI_LineNo);
					AssertEquals("additionalInfo9.CSI_LineNo is not 0 after deleting a document, when in database", 3, additionalInfo9.CSI_LineNo);
					AssertEquals("additionalInfo10.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo10.CSI_LineNo);
					AssertEquals("additionalInfo11.CSI_LineNo is not 0 after deleting a document, when in database", 1, additionalInfo11.CSI_LineNo);
				});
			}
		}

		public void TestCloneCSI_LineNo()
		{
			additionalDocument.CSI_LineNo = 5;
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("TransitPeriod: Test CSI_LineNo of the original document", 5, additionalDocument.CSI_LineNo);
					var clonedAdditionalInfo = (NctsBillAdditionalDocument)additionalDocument.Clone();
					AssertEquals("TransitPeriod: Test CSI_LineNo is not cloned when cloning document", ZInt.Zero, clonedAdditionalInfo.CSI_LineNo);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("Test CSI_LineNo of the original document", 5, additionalDocument.CSI_LineNo);
					var clonedAdditionalInfo = (NctsBillAdditionalDocument)additionalDocument.Clone();
					AssertEquals("Test CSI_LineNo is not cloned when cloning document", 5, clonedAdditionalInfo.CSI_LineNo);
				}
			});
		}

		public void TestReadOnlyProviderType()
		{
			var arrivalAdditionalDocument = CreateAndGetAdditionalDocumentForTest(NctsMovementType.Codes.Arrival);
			var departureAdditionalDocument = CreateAndGetAdditionalDocumentForTest(NctsMovementType.Codes.Departure);

			CombineAssertions(() =>
			{
				AssertType<NctsBillsArrivalAdditionalDocumentReadOnlyProvider>("When Ncts IsArrival Phase 5, ES AdditionalDocumentReadOnlyProvider", arrivalAdditionalDocument.GetNewReadOnlyProvider());
				AssertEquals("When Ncts is not IsArrival Phase 5, AdditionalDocumentReadOnlyProvider is EU", "Enterprise.Customs.EU.NCTS.Business.NctsBillsDepartureAdditionalDocumentReadOnlyProvider", departureAdditionalDocument.GetNewReadOnlyProvider().GetType().FullName);
			});
		}

		NctsBillAdditionalDocumentForTest CreateAndGetAdditionalDocumentForTest(ZString movementType)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(movementType);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var nctsBill = nctsHeader.Bills.AddNew();
			var additionalDocument = Factory.New<NctsBillAdditionalDocumentForTest>();
			additionalDocument.AttachToParent(nctsBill);

			return additionalDocument;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => additionalDocument;

		protected override IEnumerable<NctsBillAdditionalDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			yield return bill.AdditionalDocuments.AddNew();
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, NctsBillAdditionalDocument bizObj)
		{
			base.LoadParentIfNeeded(factory, bizObj);
			factory.Load<NctsBill>(bizObj.CSI_ParentID);
		}

		protected override BusinessObject GetNewBusinessObject() => additionalDocument;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			bill = nctsHeader.Bills.AddNew();
			additionalDocument = bill.AdditionalDocuments.AddNew();
		}
		NctsHeader nctsHeader;
		NctsBill bill;
		NctsBillAdditionalDocument additionalDocument;

		class NctsBillAdditionalDocumentForTest : NctsBillAdditionalDocument
		{
			public NctsBillAdditionalDocumentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public void AttachToParent(BusinessObject parent)
			{
				CSI_ParentTableCode = parent.TablePrefix;
				CSI_ParentID = parent.PK;
			}

			public new IAdditionalDocumentReadOnlyProvider GetNewReadOnlyProvider() => base.GetNewReadOnlyProvider();

			protected override bool AutomaticSequenceNumberEnabled => true;

			public bool AutomaticSequenceNumberEnabled_Exposed => base.AutomaticSequenceNumberEnabled;
		}
	}
}
