using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefCusCodeListAttributeTypes = Enterprise.Customs.Universal.RefCusCodeListAttributeTypes;
using RefCusCodeListTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(NctsAdditionalInfo))]
	public abstract class NctsAdditionalInfoTest<T> : Customs.Business.Testing.CusSupportingInfoTest<T>
		where T : NctsAdditionalInfo
	{
		protected override IEnumerable<T> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var departureHeaderPhase5 = factory.New<NctsHeader>();
			departureHeaderPhase5.SetMovementType(NctsMovementType.Codes.Departure);
			if (departureHeaderPhase5.AdditionalDocuments.AddNew() is T departureHeaderAdditionalDocument)
			{
				yield return departureHeaderAdditionalDocument;
			}
			if (departureHeaderPhase5.Bills.AddNew().GoodsItems.AddNew().AdditionalInfos.AddNew() is T departureBillGoodsItemAdditionalDocument)
			{
				yield return departureBillGoodsItemAdditionalDocument;
			}

			var departureHeaderPhase4 = factory.New<NctsHeader>();
			departureHeaderPhase4.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			departureHeaderPhase4.SetMovementType(NctsMovementType.Codes.Departure);
			if (departureHeaderPhase4.MovementHeader.GoodsItems.AddNew().AdditionalInfos.AddNew() is T departureMoveHeaderGoodsItemAdditionalDocument)
			{
				yield return departureMoveHeaderGoodsItemAdditionalDocument;
			}

			var arrivalHeaderPhase5 = factory.New<NctsHeader>();
			arrivalHeaderPhase5.SetMovementType(NctsMovementType.Codes.Arrival);
			if (arrivalHeaderPhase5.Bills.AddNew().ArrivalGoodsItems.AddNew().AdditionalInfos.AddNew() is T arrivalBillGoodsItemAdditionalDocument)
			{
				yield return arrivalBillGoodsItemAdditionalDocument;
			}

			if (arrivalHeaderPhase5.ArrivalMovementHeader.AdditionalDocuments.AddNew() is T arrivalMovementHeaderAdditionalDocument)
			{
				yield return arrivalMovementHeaderAdditionalDocument;
			}
		}
	}

	[TestedType(typeof(NctsAdditionalInfo))]
	sealed class NctsAdditionalInfoBaseOnlyTest : NctsAdditionalInfoTest<NctsAdditionalInfo>
	{
		public void TestCSI_ItemNumber_Caption()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory);
			AssertEquals("Item Number", DataBoundResourceStrings.GetDataForProperty(additionalInfo.CSI_ItemNumberInfo).Caption);
		}

		public void TestCSI_Status_Caption()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory);
			NCTSTestHelper.AssertCaptions(additionalInfo.CSI_StatusInfo, NctsHeader.Phase5CaptionKey, "State of Unloading", "Unloaded State", "Unloaded State");
		}

		public void TestDefaultStatus()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory);
			AssertEquals("NEW", additionalInfo.CSI_Status);
		}

		public void TestDefaultSubType()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory);
			AssertEquals("REF", additionalInfo.CSI_SubType);
		}

		public void TestLookups_Phase4()
		{
			(var nctsHeader, _, _, var additionalInfo, _) = CreateData(Factory);
			AssertType<NctsAdditionalInfoPhase4Lookups>(additionalInfo.Lookups);
		}

		public void TestLookups_Phase5()
		{
			(var nctsHeader, _, _, var additionalInfo, _) = CreateData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);
			AssertType<NctsAdditionalInfoPhase5Lookups>(additionalInfo.Lookups);
		}

		public void TestValidation()
		{
			(var nctsHeader, _, _, var additionalInfo, _) = CreateData(Factory);
			AssertType<NctsAdditionalInfoValidation>(additionalInfo.Validation);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsAdditionalInfoPhase5Validation>(additionalInfo.Validation);
		}

		public void TestCSI_SubType_Caption()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory);
			NCTSTestHelper.AssertCaptions(additionalInfo.CSI_SubTypeInfo, "Kind of Document", "Doc. Kind", "Kind");
		}

		public void TestCSI_Code_Phase4Caption()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory);
			NCTSTestHelper.AssertCaptions(additionalInfo.CSI_CodeInfo, NctsHeader.Phase4CaptionKey, "Code", string.Empty, string.Empty);
		}

		public void TestCSI_Code_Phase5Caption()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);
			NCTSTestHelper.AssertCaptions(additionalInfo.CSI_CodeInfo, NctsHeader.Phase5CaptionKey, "Document Type", "Doc. Type", "Type");
		}

		public void TestSequenceNumber()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			var additionalInfo = goodsItem.AdditionalInfos.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("SequenceNumber of additionalInfo is 1", 1, additionalInfo.CSI_LineNo);

				var additionalInfo2 = goodsItem.AdditionalInfos.AddNew();
				AssertEquals("SequenceNumber of additionalInfo2 is 2", 2, additionalInfo2.CSI_LineNo);

				additionalInfo.Delete();
				AssertEquals("additionalInfo is deleted, SequenceNumber of additionalInfo2 is 1", 1, additionalInfo2.CSI_LineNo);
			});
		}

		public void TestSequenceNumber_DifferentSubTypes_NctsCommonCargoDescParent()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var additionalInfo = goodsItem.AdditionalInfos.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("SequenceNumber of additionalInfo (REF) is 1", 1, additionalInfo.CSI_LineNo);

				var additionalInfo2 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("SequenceNumber of additionalInfo2 (INF) is 1", 1, additionalInfo2.CSI_LineNo);

				var additionalInfo3 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("SequenceNumber of additionalInfo3 (INF) is 2", 2, additionalInfo3.CSI_LineNo);

				var additionalInfo4 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo4.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("SequenceNumber of additionalInfo4 (REF) is 2", 2, additionalInfo4.CSI_LineNo);

				var additionalInfo5 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals("SequenceNumber of additionalInfo5 (TRA) is 1", 1, additionalInfo5.CSI_LineNo);

				additionalInfo.Delete();
				AssertEquals("additionalInfo (REF) is deleted, SequenceNumber of additionalInfo4 (REF) is 1", 1, additionalInfo4.CSI_LineNo);
				AssertEquals("additionalInfo (REF) is deleted, SequenceNumber of additionalInfo2 (INF) is 1", 1, additionalInfo2.CSI_LineNo);
				AssertEquals("additionalInfo (REF) is deleted, SequenceNumber of additionalInfo3 (INF) is 2", 2, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo (REF) is deleted, SequenceNumber of additionalInfo5 (TRA) is 1", 1, additionalInfo5.CSI_LineNo);
			});
		}

		public void TestSequenceNumber_WithOutParentTableCode_NctsCommonCargoDescParent()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var additionalInfo1 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("SequenceNumber of additionalInfo1 (REF) is 1", 1, additionalInfo1.CSI_LineNo);

				var additionalInfo2 = goodsItem.AdditionalInfos.AddNew();
				additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("SequenceNumber of additionalInfo2 (REF) is 2", 2, additionalInfo2.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 0;
				additionalInfo1.CSI_ParentTableCode = ZString.Empty;
				additionalInfo2.CSI_ParentTableCode = ZString.Empty;
				AssertEquals("additionalInfo CSI_ParentTableCode is empty, SequenceNumber of additionalInfo1 (REF) is 0", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("additionalInfo CSI_ParentTableCode is empty, SequenceNumber of additionalInfo2 (REF) is 2", 2, additionalInfo2.CSI_LineNo);
			});
		}

		public void TestSequenceNumber_DifferentSubTypes_NctsHeaderParent()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();

			CombineAssertions(() =>
			{
				var additionalInfo = bill.AdditionalDocuments.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("SequenceNumber of additionalInfo (REF) is 1", 1, additionalInfo.CSI_LineNo);

				var additionalInfo2 = bill.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("SequenceNumber of additionalInfo2 (INF) is 1", 1, additionalInfo2.CSI_LineNo);

				var additionalInfo3 = bill.AdditionalDocuments.AddNew();
				additionalInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("SequenceNumber of additionalInfo3 (INF) is 2", 2, additionalInfo3.CSI_LineNo);

				var additionalInfo4 = bill.AdditionalDocuments.AddNew();
				additionalInfo4.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("SequenceNumber of additionalInfo4 (REF) is 2", 2, additionalInfo4.CSI_LineNo);

				var additionalInfo5 = bill.AdditionalDocuments.AddNew();
				additionalInfo5.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals("SequenceNumber of additionalInfo5 (TRA) is 1", 1, additionalInfo5.CSI_LineNo);

				additionalInfo.Delete();
				AssertEquals("additionalInfo (REF) is deleted, SequenceNumber of additionalInfo4 (REF) is 1", 1, additionalInfo4.CSI_LineNo);
				AssertEquals("additionalInfo (REF) is deleted, SequenceNumber of additionalInfo2 (INF) is 1", 1, additionalInfo2.CSI_LineNo);
				AssertEquals("additionalInfo (REF) is deleted, SequenceNumber of additionalInfo3 (INF) is 2", 2, additionalInfo3.CSI_LineNo);
				AssertEquals("additionalInfo (REF) is deleted, SequenceNumber of additionalInfo5 (TRA) is 1", 1, additionalInfo5.CSI_LineNo);
			});
		}

		public void TestSequenceNumber_WithOutParentTableCode_NctsHeaderParent()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();

			CombineAssertions(() =>
			{
				var additionalInfo1 = bill.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("SequenceNumber of additionalInfo1 (REF) is 1", 1, additionalInfo1.CSI_LineNo);

				var additionalInfo2 = bill.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("SequenceNumber of additionalInfo2 (REF) is 2", 2, additionalInfo2.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 0;
				additionalInfo1.CSI_ParentTableCode = ZString.Empty;
				additionalInfo2.CSI_ParentTableCode = ZString.Empty;
				AssertEquals("additionalInfo CSI_ParentTableCode is empty, SequenceNumber of additionalInfo1 (REF) is 0", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("additionalInfo CSI_ParentTableCode is empty, SequenceNumber of additionalInfo2 (REF) is 2", 2, additionalInfo2.CSI_LineNo);
			});
		}

		public void TestCSI_ReferenceNumber_Caption()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory);
			NCTSTestHelper.AssertCaptions(additionalInfo.CSI_ReferenceNumberInfo, "Reference Number", "Reference No.", "Reference");
		}

		public void TestCSI_ReferenceNumber_MaxLength()
		{
			CombineAssertions(() =>
			{
				(var nctsHeader, _, _, var additionalInfo, _) = CreateData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("Phase 5 Departure in Transition Period", 35, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
				}
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("Phase 5 Departure outside Transition Period", 70, additionalInfo.CSI_ReferenceNumberInfo.MaxLength);
				}

				var arrivalAdditionalInfo = CreatePhase5ArrivalHeaderAndAdditionalDocument();
				AssertEquals("Phase 5 Arrival", AutoCusSupportingInfo.Schema.CSI_ReferenceNumberMaxLength, arrivalAdditionalInfo.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_Description_Phase4Caption()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory);
			NCTSTestHelper.AssertCaptions(additionalInfo.CSI_DescriptionInfo, NctsHeader.Phase4CaptionKey, "Description", string.Empty, string.Empty);
		}

		public void TestCSI_Description_Phase5Caption()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);
			NCTSTestHelper.AssertCaptions(additionalInfo.CSI_DescriptionInfo, NctsHeader.Phase5CaptionKey, "Description", "Description", "Descr.");
		}

		public void TestCSI_Description_Caption_Header()
		{
			(_, var additionalInfoHeader, _, _, _) = CreateData(Factory);
			NCTSTestHelper.AssertCaptions(additionalInfoHeader.CSI_DescriptionInfo, NctsHeader.Phase4CaptionKey, "Description", string.Empty, string.Empty);
		}

		public void TestCSI_DescriptionMaxLength_Phase4()
		{
			(var nctsHeader, _, _, var additionalInfo, _) = CreateData(Factory);
			AssertEquals("Goods Item CSI_DescriptionMaxLength", 70, additionalInfo.CSI_DescriptionInfo.MaxLength);
		}

		public void TestCSI_DescriptionMaxLength_Phase5() => CombineAssertions(() =>
		{
			(_, var additionalInfoHeader, _, var additionalInfo, var additionalInfoBill) = CreateData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);
			AssertEquals("Header CSI_DescriptionMaxLength", 512, additionalInfoHeader.CSI_DescriptionInfo.MaxLength);
			AssertEquals("Bill CSI_DescriptionMaxLength", 512, additionalInfoBill.CSI_DescriptionInfo.MaxLength);
			AssertEquals("Goods Item CSI_DescriptionMaxLength", 512, additionalInfo.CSI_DescriptionInfo.MaxLength);
		});

		public void TestCSI_LineNo_Phase5Caption()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);
			NCTSTestHelper.AssertCaptions(additionalInfo.CSI_LineNoInfo, NctsHeader.Phase5CaptionKey, "Sequence Number", "Sequence No.", "Seq.No.");
		}

		public void TestCSI_Status_Phase5Caption()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);
			NCTSTestHelper.AssertCaptions(additionalInfo.CSI_StatusInfo, NctsHeader.Phase5CaptionKey, "State of Unloading", "Unloaded State", "Unloaded State");
		}

		public void TestMultipleKeysToUse_Phase4()
		{
			(var nctsHeader, _, _, var additionalInfo, _) = CreateData(Factory);
			AssertSequencesEqual("Header.BH_ApplicationCode is NCT", new[] { NctsHeader.Phase4CaptionKey }, additionalInfo.MultipleKeysToUse);
		}

		public void TestMultipleKeysToUse_Phase5Departure()
		{
			(var nctsHeader, _, _, var additionalInfo, _) = CreateData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);
			AssertSequencesEqual("Header.BH_ApplicationCode is NC5 and header is departure", new[] { NctsHeader.Phase5DepartureCaptionKey, NctsHeader.Phase5CaptionKey }, additionalInfo.MultipleKeysToUse);
		}

		public void TestMultipleKeysToUse_Phase5Arrival()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			var additionalInfo = goodsItem.AdditionalInfos.AddNew();

			AssertSequencesEqual("Header.BH_ApplicationCode is NC5 and header is arrival", new[] { NctsHeader.Phase5CaptionKey }, additionalInfo.MultipleKeysToUse);
		}

		public void TestGoodsItem()
		{
			(var nctsHeader, _, _, var additionalInfo, _) = CreateData(Factory);
			AssertEquals(additionalInfo.ParentAsGoodsItem.PK, nctsHeader.Bills[0].GoodsItems[0].PK);
		}

		public void TestCodeListType()
		{
			CombineAssertions(() =>
			{
				(var nctsHeader, _, _, var additionalInfo, _) = CreateData(Factory);

				AssertEquals("Phase 4", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, additionalInfo.CodeListType);

				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals($"Phase 5, SubType={additionalInfo.CSI_SubType}", RefCusCodeListTypes.Codes.Code_AR44N, additionalInfo.CodeListType);

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals($"Phase 5, SubType={additionalInfo.CSI_SubType}", RefCusCodeListTypes.Codes.Code_AI44N, additionalInfo.CodeListType);
			});
		}

		public void TestParentAsNctsHeader()
		{
			(var nctsHeader, var additionalInfoHeader, _, _, _) = CreateData(Factory);
			AssertSame(nctsHeader, additionalInfoHeader.ParentAsNctsHeader);
		}

		public void TestParentAsGoodsItem()
		{
			(_, _, var goodsItem, var additionalInfo, _) = CreateData(Factory);
			AssertSame(goodsItem, additionalInfo.ParentAsGoodsItem);
		}

		public void TestParentAsArrivalMovementHeader()
		{
			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var arrivalMovementHeader = arrivalNctsHeader.ArrivalMovementHeader;
			var arrivalAdditionalInfo = arrivalMovementHeader.AdditionalDocuments.AddNew();

			AssertSame(arrivalMovementHeader, arrivalAdditionalInfo.ParentAsArrivalMovementHeader);
		}

		public void TestSetUnloadedStateFromDocType()
		{
			(var nctsHeader, _, var goodsItem, var additionalInfo, _) = CreateData(Factory);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");

			var refCusCodeList1 = helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, RefCusCodeListTypes.Codes.Code_AR44N + "01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, "Level", RefCusCodeListLevelTypes.Item);
			Factory.Save();

			CombineAssertions(() =>
			{
				additionalInfo.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals("A valid CSI_Code has been entered, so the unloaded state should be set to NEW", "NEW", additionalInfo.CSI_Status);

				additionalInfo.CSI_Status = ZString.Empty;
				additionalInfo.CSI_Code = "INVALID";
				AssertEquals("An invalid CSI_Code has been entered, so the unloaded state should not be set", ZString.Empty, additionalInfo.CSI_Status);

				additionalInfo.CSI_Status = "NEW";
				additionalInfo.CSI_Code = ZString.Empty;
				AssertEquals("The CSI_Code was cleared, so the unloaded state should also be cleared", ZString.Empty, additionalInfo.CSI_Status);

				additionalInfo.CSI_Status = "XXX";
				additionalInfo.CSI_Code = "INVALID";
				AssertEquals("An invalid CSI_Code has been entered, so the unloaded state should not be changed", "XXX", additionalInfo.CSI_Status);
			});
		}

		public void TestSetKindFromDocType()
		{
			(var nctsHeader, _, _, var additionalInfo, _) = CreateData(Factory);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");

			var refCusCodeList1 = helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, RefCusCodeListTypes.Codes.Code_AR44N + "01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, "Level", RefCusCodeListLevelTypes.Item);
			Factory.Save();

			CombineAssertions(() =>
			{
				additionalInfo.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals("A valid CSI_Code has been entered, so the kind should be set to REF", "REF", additionalInfo.CSI_SubType);

				additionalInfo.CSI_SubType = ZString.Empty;
				additionalInfo.CSI_Code = "INVALID";
				AssertEquals("An invalid CSI_Code has been entered, so the kind should not be set", ZString.Empty, additionalInfo.CSI_SubType);

				additionalInfo.CSI_SubType = "REF";
				additionalInfo.CSI_Code = ZString.Empty;
				AssertEquals("The CSI_Code was cleared, so the kind should also be cleared", ZString.Empty, additionalInfo.CSI_SubType);

				additionalInfo.CSI_SubType = "XXX";
				additionalInfo.CSI_Code = "INVALID";
				AssertEquals("An invalid CSI_Code has been entered, so the kind should not be changed", "XXX", additionalInfo.CSI_SubType);
			});
		}

		public void TestCSI_Description_Caption()
		{
			(_, _, _, var additionalInfo, _) = CreateData(Factory);
			NCTSTestHelper.AssertCaptions(additionalInfo.CSI_DescriptionInfo, "Description", string.Empty, string.Empty);
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms()
		{
			var additionalDocument = CreatePhase5ArrivalHeaderAndAdditionalDocument();

			var goodsItem = (NctsArrivalCargoDesc)additionalDocument.Parent;
			var bill = goodsItem.Bill;
			var arrivalMovementHeader = bill.MovementDetail.ArrivalMoveHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsAdditionalInfo)x).FieldsReadOnlyForPhase5Arrival, additionalDocument);
		}

		public void TestAutomaticSequenceNumberEnabled()
		{
			CombineAssertions(() =>
			{
				(_, _, _, var additionalInfo, _) = CreateData(Factory);
				var goodsItem = additionalInfo.ParentAsGoodsItem;
				AssertEquals("First Line", 1, (int)additionalInfo.CSI_LineNo);
				var secondLine = goodsItem.AdditionalInfos.AddNew();
				AssertEquals("Second Line", 2, (int)secondLine.CSI_LineNo);
				var thirdLine = goodsItem.AdditionalInfos.AddNew();
				AssertEquals("Third Line", 3, (int)thirdLine.CSI_LineNo);
				secondLine.Delete();
				AssertEquals("First Line same as second deleted", 1, (int)additionalInfo.CSI_LineNo);
				AssertEquals("Third Line renumbered as second deleted", 2, (int)thirdLine.CSI_LineNo);
				var newThirdLine = goodsItem.AdditionalInfos.AddNew();
				AssertEquals("New Third added", 3, (int)newThirdLine.CSI_LineNo);
			});
		}

		public void TestAutomaticSequenceNumberDisabled()
		{
			CombineAssertions(() =>
			{
				(_, _, var goodsItem, _, _) = CreateData(Factory);
				var firstLine = Factory.New<NctsAdditionalInfoForTest>();
				goodsItem.AdditionalInfos.RemoveAndDeleteAll();
				firstLine.AttachToParent(goodsItem);
				AssertEquals("First Line", ZShort.Zero, firstLine.CSI_LineNo);
				firstLine.CSI_LineNo = 20;
				var secondLine = Factory.New<NctsAdditionalInfoForTest>();
				secondLine.AttachToParent(goodsItem);
				AssertEquals("Second Line", ZShort.Zero, secondLine.CSI_LineNo);
				secondLine.CSI_LineNo = 35;
				firstLine.Delete();
				AssertEquals("Second Line remains the same as the first line is deleted", 35, (int)secondLine.CSI_LineNo);
			});
		}

		public void TestReadOnlyProviderType()
		{
			CombineAssertions(() =>
			{
				AssertType<NctsHeaderDepartureAdditionalDocumentReadOnlyProvider>(
					"When Ncts IsDeparture Phase 4, Parent is header, AdditionalDocumentReadOnlyProvider",
					CreateHeaderAdditionalDocument(
						CusInBondApplicationCodeList.Codes.NCTS4,
						NctsMovementType.Codes.Departure).GetNewReadOnlyProvider());

				AssertType<NctsHeaderArrivalAdditionalDocumentReadOnlyProvider>(
					"When Ncts IsArrival Phase 4, Parent is movement header, AdditionalDocumentReadOnlyProvider",
					CreateHeaderAdditionalDocument(
						CusInBondApplicationCodeList.Codes.NCTS4,
						NctsMovementType.Codes.Arrival).GetNewReadOnlyProvider());

				AssertType<NctsGoodsItemsDepartureAdditionalDocumentReadOnlyProvider>(
					"When Ncts IsDeparture Phase 4, Parent is goodsItem, AdditionalDocumentReadOnlyProvider",
					CreateHeaderAdditionalDocument(
						CusInBondApplicationCodeList.Codes.NCTS4,
						NctsMovementType.Codes.Departure,
						parentIsNctsHeader: false).GetNewReadOnlyProvider());

				AssertType<NctsGoodsItemsArrivalAdditionalDocumentReadOnlyProvider>(
					"When Ncts IsArrival Phase 4, Parent is goodsItem, AdditionalDocumentReadOnlyProvider",
					CreateHeaderAdditionalDocument(
						CusInBondApplicationCodeList.Codes.NCTS4,
						NctsMovementType.Codes.Arrival,
						parentIsNctsHeader: false).GetNewReadOnlyProvider());

				AssertType<NctsHeaderDepartureAdditionalDocumentReadOnlyProvider>(
					"When Ncts IsDeparture Phase 5, Parent is header, AdditionalDocumentReadOnlyProvider",
					CreateHeaderAdditionalDocument(
						CusInBondApplicationCodeList.Codes.NCTS5,
						NctsMovementType.Codes.Departure).GetNewReadOnlyProvider());

				AssertType<NctsHeaderArrivalAdditionalDocumentReadOnlyProvider>(
					"When Ncts IsArrival Phase 5, Parent is movement header, AdditionalDocumentReadOnlyProvider",
					CreateHeaderAdditionalDocument(
						CusInBondApplicationCodeList.Codes.NCTS5,
						NctsMovementType.Codes.Arrival).GetNewReadOnlyProvider());

				AssertType<NctsGoodsItemsDepartureAdditionalDocumentReadOnlyProvider>(
					"When Ncts IsDeparture Phase 5, Parent is goodsItem, AdditionalDocumentReadOnlyProvider",
					CreateHeaderAdditionalDocument(
						CusInBondApplicationCodeList.Codes.NCTS5,
						NctsMovementType.Codes.Departure,
						parentIsNctsHeader: false).GetNewReadOnlyProvider());

				AssertType<NctsGoodsItemsArrivalAdditionalDocumentReadOnlyProvider>(
					"When Ncts IsArrival Phase 5, Parent is goodsItem, AdditionalDocumentReadOnlyProvider",
					CreateHeaderAdditionalDocument(
						CusInBondApplicationCodeList.Codes.NCTS5,
						NctsMovementType.Codes.Arrival,
						parentIsNctsHeader: false).GetNewReadOnlyProvider());
			});
		}

		public void TestGetRefCusCodeListByCodeType()
		{
			(var nctsHeader, _, _, var additionalInfo, _) = CreateData(Factory);
			var goodItem = (NctsDepartureCargoDesc)additionalInfo.Parent;
			goodItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			goodItem.Header.BH_HeaderType = NctsMovementType.Codes.Arrival;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");

			var refCusCodeList1 = helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, RefCusCodeListTypes.Codes.Code_AR44N + "01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, "Level", RefCusCodeListLevelTypes.Item);
			Factory.Save();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			Factory.Save();

			CombineAssertions(() =>
			{
				additionalInfo.CSI_Code = refCusCodeList1.ZZD_Code;

				var list = additionalInfo.GetRefCusCodeListByCodeType(RefCusCodeListTypes.Codes.Code_AR44N);
				AssertEquals($"RefCusCodeList contains values, when filter is empty", RefCusCodeListTypes.Codes.Code_AR44N + "01", list.ZZD_Code);

				list = additionalInfo.GetRefCusCodeListByCodeType(RefCusCodeListTypes.Codes.Code_AR44N, new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Level, SQLComparisonOperator.Equal, UniversalReferenceConstants.RefCusCodeListLevelTypes.Item) });
				AssertEquals($"RefCusCodeList contains values, when filtering by Item Level", RefCusCodeListTypes.Codes.Code_AR44N + "01", list.ZZD_Code);

				list = additionalInfo.GetRefCusCodeListByCodeType(RefCusCodeListTypes.Codes.Code_AR44N, new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Level, SQLComparisonOperator.Equal, UniversalReferenceConstants.RefCusCodeListLevelTypes.Header) });
				AssertNull($"RefCusCodeList is null, when filtering by Header Level", list);

				list = additionalInfo.GetRefCusCodeListByCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010);
				AssertNull($"RefCusCodeList is null, when CodeType is wrong", list);

				additionalInfo.CSI_Code = ZString.Empty;

				list = additionalInfo.GetRefCusCodeListByCodeType(RefCusCodeListTypes.Codes.Code_AR44N);
				AssertNull($"RefCusCodeList is null, when CSI_Code is empty", list);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			return goodsItem.AdditionalInfos.AddNew();
		}

		static (NctsHeader nctsHeader, NctsAdditionalInfo additionalInfoHeader, NctsDepartureCargoDesc goodsItem, NctsAdditionalInfo additionalInfo, NctsBillAdditionalDocument additionalInfoBill) CreateData(BusinessObjectFactory factory, string phase = CusInBondApplicationCodeList.Codes.NCTS4)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = phase;
			var additionalInfoHeader = nctsHeader.AdditionalDocuments.AddNew();
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			var additionalInfo = goodsItem.AdditionalInfos.AddNew();
			var additionalInfoBill = bill.AdditionalDocuments.AddNew();
			return (nctsHeader, additionalInfoHeader, goodsItem, additionalInfo, additionalInfoBill);
		}

		#region Implementation

		NctsAdditionalInfoForTest CreateHeaderAdditionalDocument(string phase, string movementType, bool parentIsNctsHeader = true)
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(movementType);
			header.BH_ApplicationCode = phase;
			var additionalDocument = Factory.New<NctsAdditionalInfoForTest>();

			var parent = (BusinessObject)header;
			if (!parentIsNctsHeader)
			{
				var bill = header.Bills.AddNew();
				var goodsItem = (movementType == NctsMovementType.Codes.Arrival)
					? (NctsCommonCargoDesc)bill.ArrivalGoodsItems.AddNew()
					: bill.GoodsItems.AddNew();
				parent = goodsItem;
			}
			else
			{
				if (movementType == NctsMovementType.Codes.Arrival)
				{
					parent = header.ArrivalMovementHeader;
				}
			}

			additionalDocument.AttachToParent(parent);
			return additionalDocument;
		}

		NctsAdditionalInfo CreatePhase5ArrivalHeaderAndAdditionalDocument()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var movementDetail = header.ArrivalMovementHeader.MovementDetails.AddNew();
			var bill = header.Bills.AddNew();
			movementDetail.B9_B0 = bill.PK;
			var additionalDocument = bill.ArrivalGoodsItems.AddNew().AdditionalInfos.AddNew();
			additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			return additionalDocument;
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

			protected override bool AutomaticSequenceNumberEnabled => false;
		}

		#endregion
	}
}
