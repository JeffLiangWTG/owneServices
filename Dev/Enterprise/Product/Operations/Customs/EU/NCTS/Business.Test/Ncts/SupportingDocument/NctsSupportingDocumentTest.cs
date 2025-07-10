using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Moq;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsSupportingDocument))]
	sealed class NctsSupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<NctsSupportingDocument>
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Supporting Document", supportingDocument.HumanReadableName);
		}

		public void TestLookups_Phase4()
		{
			AssertType<NctsSupportingDocumentPhase4Lookups>(Factory.New<NctsSupportingDocumentPhase4ForTest>().Lookups);
		}

		public void TestLookups_Phase5()
		{
			AssertType<NctsSupportingDocumentPhase5Lookups>(supportingDocument.Lookups);
		}

		public void TestValidation_Phase4()
		{
			AssertType<NctsSupportingDocumentPhase4Validation>(Factory.New<NctsSupportingDocumentPhase4ForTest>().Validation);
		}

		public void TestValidation_Phase5Arrival()
		{
			AssertType<NctsSupportingDocumentPhase5ArrivalValidation>(Factory.New<NctsArrivalSupportingDocumentPhase5ForTest>().Validation);
		}

		public void TestValidation_Phase5Departure()
		{
			AssertType<NctsSupportingDocumentPhase5DepartureValidation>(supportingDocument.Validation);
		}

		public void TestReadOnlyProvider_Phase4()
		{
			AssertType<NctsSupportingDocumentPhase4ReadOnlyProvider>(Factory.New<NctsSupportingDocumentPhase4ForTest>().ReadOnlyProvider);
		}

		public void TestReadOnlyProvider_Phase5Arrival()
		{
			AssertType<NctsSupportingDocumentPhase5ArrivalReadOnlyProvider>(Factory.New<NctsArrivalSupportingDocumentPhase5ForTest>().ReadOnlyProvider);
		}

		public void TestReadOnlyProvider_Phase5Departure()
		{
			AssertType<NctsSupportingDocumentPhase5DepartureReadOnlyProvider>(Factory.New<NctsDepartureSupportingDocumentPhase5ForTest>().ReadOnlyProvider);
		}

		public void TestValidationDeciderPhase4Departure()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertNull(supportingDocument.ValidationDecider);
		}

		public void TestValidationDeciderPhase5Departure()
		{
			AssertType<NctsSupportingDocumentDeparturePhase5ValidationDecider>(supportingDocument.ValidationDecider);
		}

		public void TestValidationDeciderPhase5Arrival()
		{
			AssertNull(CreatePhase5ArrivalHeaderAndSupportingDocument().ValidationDecider);
		}

		public void TestPropertiesReadOnly()
		{
			var readOnlyProvider = new Mock<ISupportingDocumentReadOnlyConditions>();
			var supportingDocument = Factory.New<NctsArrivalSupportingDocumentPhase5ForTest>();
			supportingDocument.ReadOnlyProiderMock = readOnlyProvider;

			CombineAssertions(() =>
			{
				readOnlyProvider.Setup(x => x.CSI_LineNo_ReadOnly).Returns(true);
				readOnlyProvider.Setup(x => x.CSI_Status_ReadOnly).Returns(true);
				readOnlyProvider.Setup(x => x.CSI_Code_ReadOnly).Returns(true);
				readOnlyProvider.Setup(x => x.CSI_ReferenceNumber_ReadOnly).Returns(true);
				readOnlyProvider.Setup(x => x.CSI_ReferenceNumber2_ReadOnly).Returns(true);
				readOnlyProvider.Setup(x => x.CSI_Description_ReadOnly).Returns(true);
				readOnlyProvider.Setup(x => x.CSI_ItemNumber_ReadOnly).Returns(true);
				AssertEquals("CSI_LineNo: ReadOnly", true, supportingDocument.CSI_LineNoInfo.ReadOnly);
				AssertEquals("CSI_Status: ReadOnly", true, supportingDocument.CSI_StatusInfo.ReadOnly);
				AssertEquals("CSI_Code: ReadOnly", true, supportingDocument.CSI_CodeInfo.ReadOnly);
				AssertEquals("CSI_ReferenceNumber: ReadOnly", true, supportingDocument.CSI_ReferenceNumberInfo.ReadOnly);
				AssertEquals("CSI_ReferenceNumber2: ReadOnly", true, supportingDocument.CSI_ReferenceNumber2Info.ReadOnly);
				AssertEquals("CSI_Description: ReadOnly", true, supportingDocument.CSI_DescriptionInfo.ReadOnly);
				AssertEquals("CSI_ItemNumber: ReadOnly", true, supportingDocument.CSI_ItemNumberInfo.ReadOnly);

				readOnlyProvider.Setup(x => x.CSI_LineNo_ReadOnly).Returns(false);
				readOnlyProvider.Setup(x => x.CSI_Status_ReadOnly).Returns(false);
				readOnlyProvider.Setup(x => x.CSI_Code_ReadOnly).Returns(false);
				readOnlyProvider.Setup(x => x.CSI_ReferenceNumber_ReadOnly).Returns(false);
				readOnlyProvider.Setup(x => x.CSI_ReferenceNumber2_ReadOnly).Returns(false);
				readOnlyProvider.Setup(x => x.CSI_Description_ReadOnly).Returns(false);
				readOnlyProvider.Setup(x => x.CSI_ItemNumber_ReadOnly).Returns(false);
				AssertEquals("CSI_LineNo: not ReadOnly", false, supportingDocument.CSI_LineNoInfo.ReadOnly);
				AssertEquals("CSI_Status: not ReadOnly", false, supportingDocument.CSI_StatusInfo.ReadOnly);
				AssertEquals("CSI_Code: not ReadOnly", false, supportingDocument.CSI_CodeInfo.ReadOnly);
				AssertEquals("CSI_ReferenceNumber: not ReadOnly", false, supportingDocument.CSI_ReferenceNumberInfo.ReadOnly);
				AssertEquals("CSI_ReferenceNumber2: not ReadOnly", false, supportingDocument.CSI_ReferenceNumber2Info.ReadOnly);
				AssertEquals("CSI_Description: not ReadOnly", false, supportingDocument.CSI_DescriptionInfo.ReadOnly);
				AssertEquals("CSI_ItemNumber: not ReadOnly", false, supportingDocument.CSI_ItemNumberInfo.ReadOnly);
			});
		}

		public void TestRefCusCode_Phase4()
		{
			CreateRefCusCodeListForTest();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			supportingDocument.CSI_Code = "ABC";
			var refCusCode = supportingDocument.RefCusCode;

			CombineAssertions(() =>
			{
				AssertEquals("DataGrouping is the same as configured inside supporting document", "LV", refCusCode.ZZD_CountryOrGrouping);
				AssertEquals("Document code is the same as configured inside supporting document", "ABC", refCusCode.ZZD_Code);

				supportingDocument.CSI_Code = "DEF";
				AssertNull("Recalculated", supportingDocument.RefCusCode);
			});
		}

		public void TestRefCusCode_Phase5_NoAttribute()
		{
			CreateRefCusCodeListForTest();
			supportingDocument.CSI_Code = "ABC";
			AssertNull(supportingDocument.RefCusCode);
		}

		public void TestRefCusCode_Phase5_HasAttribute()
		{
			var (refCusCodeList, helper) = CreateRefCusCodeListForTest();

			helper.CreateCusCodeListAttribute(refCusCodeList.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header);
			Factory.Save();
			supportingDocument.CSI_Code = "ABC";
			AssertEquals("ABC", supportingDocument.RefCusCode.ZZD_Code);
		}

		public void TestCSI_Code_ClearReadOnlyProperties()
		{
			var readOnlyProvider = new Mock<ISupportingDocumentReadOnlyConditions>();
			var supportingDocument = Factory.New<NctsArrivalSupportingDocumentPhase5ForTest>();
			supportingDocument.ReadOnlyProiderMock = readOnlyProvider;
			CombineAssertions(() =>
			{
				readOnlyProvider.Setup(x => x.CSI_ReferenceNumber_ReadOnly).Returns(true);
				readOnlyProvider.Setup(x => x.CSI_ReferenceNumber2_ReadOnly).Returns(true);
				readOnlyProvider.Setup(x => x.CSI_ItemNumber_ReadOnly).Returns(true);
				supportingDocument.CSI_ReferenceNumber = "REF";
				supportingDocument.CSI_ItemNumber = 123;
				supportingDocument.CSI_ReferenceNumber2 = "REF2";
				supportingDocument.CSI_Code = "1";
				AssertEquals("CSI_ReferenceNumber read-only", ZString.Empty, supportingDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_ItemNumber read-only", ZShort.Zero, supportingDocument.CSI_ItemNumber);
				AssertEquals("CSI_ReferenceNumber2 read-only", ZString.Empty, supportingDocument.CSI_ReferenceNumber2);

				readOnlyProvider.Setup(x => x.CSI_ReferenceNumber_ReadOnly).Returns(false);
				readOnlyProvider.Setup(x => x.CSI_ReferenceNumber2_ReadOnly).Returns(false);
				readOnlyProvider.Setup(x => x.CSI_ItemNumber_ReadOnly).Returns(false);
				supportingDocument.CSI_ReferenceNumber = "REF";
				supportingDocument.CSI_ItemNumber = 123;
				supportingDocument.CSI_ReferenceNumber2 = "REF2";
				supportingDocument.CSI_Code = "2";
				AssertEquals("CSI_ReferenceNumber not read-only", "REF", supportingDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_ItemNumber not read-only", (ZShort)123, supportingDocument.CSI_ItemNumber);
				AssertEquals("CSI_ReferenceNumber2 not read-only", "REF2", supportingDocument.CSI_ReferenceNumber2);
			});
		}

		public void TestCSI_Code_Caption()
		{
			NCTSTestHelper.AssertCaptions(supportingDocument.CSI_CodeInfo, "Document Type", "Doc. Type", "Type");
		}

		public void TestSequenceNumber_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			CombineAssertions(() =>
			{
				var supportingDocument = goodsItem.SupportingDocuments.AddNew();
				AssertEquals("SequenceNumber of supportingDocument is 0", 0, supportingDocument.CSI_LineNo);

				var supportingDocument2 = goodsItem.SupportingDocuments.AddNew();
				AssertEquals("SequenceNumber of supportingDocument2 is 0", 0, supportingDocument2.CSI_LineNo);
			});
		}

		public void TestSequenceNumber_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			CombineAssertions(() =>
			{
				var supportingDocument = goodsItem.SupportingDocuments.AddNew();
				AssertEquals("SequenceNumber of supportingDocument is 1", 1, supportingDocument.CSI_LineNo);

				var supportingDocument2 = goodsItem.SupportingDocuments.AddNew();
				AssertEquals("SequenceNumber of supportingDocument2 is 2", 2, supportingDocument2.CSI_LineNo);

				supportingDocument.Delete();
				AssertEquals("supportingDocument is deleted, SequenceNumber of supportingDocument2 is 1", 1, supportingDocument2.CSI_LineNo);
			});
		}

		public void TestSequenceNumber_WithOutParentTableCode_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();
			CombineAssertions(() =>
			{
				var supportingDocument1 = goodsItem.SupportingDocuments.AddNew();
				AssertEquals("SequenceNumber of supportingDocument is 1", 1, supportingDocument1.CSI_LineNo);

				var supportingDocument2 = goodsItem.SupportingDocuments.AddNew();
				AssertEquals("SequenceNumber of supportingDocument2 is 2", 2, supportingDocument2.CSI_LineNo);

				supportingDocument1.CSI_LineNo = 0;
				supportingDocument1.CSI_ParentTableCode = ZString.Empty;
				supportingDocument2.CSI_ParentTableCode = ZString.Empty;
				AssertEquals("supportingDocument CSI_ParentTableCode is empty, SequenceNumber of supportingDocument1 (REF) is 0", 0, supportingDocument1.CSI_LineNo);
				AssertEquals("supportingDocument CSI_ParentTableCode is empty, SequenceNumber of supportingDocument2 (INF) is 2", 2, supportingDocument2.CSI_LineNo);
			});
		}

		public void TestCSI_ReferenceNumber_Caption()
		{
			NCTSTestHelper.AssertCaptions(supportingDocument.CSI_ReferenceNumberInfo, "Reference Number", "Reference No.", "Reference");
		}

		public void TestCSI_ReferenceNumber2_Caption()
		{
			NCTSTestHelper.AssertCaptions(supportingDocument.CSI_ReferenceNumber2Info, "Complement of Information", "Complement Info.", "Complement");
		}

		public void TestCSI_ItemNumber_Caption()
		{
			NCTSTestHelper.AssertCaptions(supportingDocument.CSI_ItemNumberInfo, "Item Number", "Item No.", "Item");
		}

		public void TestCSI_LineNo_Caption()
		{
			NCTSTestHelper.AssertCaptions(supportingDocument.CSI_LineNoInfo, "Sequence Number", "Sequence No.", "Seq. No.");
		}

		public void TestCSI_Status_Caption()
		{
			NCTSTestHelper.AssertCaptions(supportingDocument.CSI_StatusInfo, NctsHeader.Phase5CaptionKey, "Unloaded State", string.Empty, "State");
		}

		public void TestCSI_ReferenceNumber_MaxLength()
		{
			CombineAssertions(() =>
			{
				var nctsBillSupportingDocument = nctsHeader.Bills.AddNew().SupportingDocuments.AddNew();
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("Phase 5 Departure in Transition Period", 35, supportingDocument.CSI_ReferenceNumberInfo.MaxLength);
					AssertEquals("Phase 5 Departure Header in Transition Period", 35, nctsBillSupportingDocument.CSI_ReferenceNumberInfo.MaxLength);
				}
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("Phase 5 Departure outside Transition Period", 70, supportingDocument.CSI_ReferenceNumberInfo.MaxLength);
					AssertEquals("Phase 5 Departure Header outside Transition Period", 70, nctsBillSupportingDocument.CSI_ReferenceNumberInfo.MaxLength);
				}

				var arrivalSupportingDocument = CreatePhase5ArrivalHeaderAndSupportingDocument();
				AssertEquals("Phase 5 Arrival", AutoCusSupportingInfo.Schema.CSI_ReferenceNumberMaxLength, arrivalSupportingDocument.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber2_MaxLength()
		{
			AssertEquals(100, supportingDocument.CSI_ReferenceNumber2Info.MaxLength);
		}

		public void TestCSI_Code_ClearCSI_Status()
		{
			supportingDocument.CSI_Code = "1";
			AssertClearCSI_StatusForCSI_Code(ZString.Empty, ZString.Empty, ZString.Empty, true);
		}

		public void TestCSI_Code_ClearCSI_Status_CSI_CodeNotEmpty()
		{
			supportingDocument.CSI_Code = "1";
			AssertClearCSI_StatusForCSI_Code(ZString.Empty, ZString.Empty, "2", false);
		}

		public void TestCSI_Code_ClearCSI_Status_CSI_ReferenceNumberNotEmpty()
		{
			supportingDocument.CSI_Code = "1";
			AssertClearCSI_StatusForCSI_Code("REF", ZString.Empty, ZString.Empty, true);
		}

		public void TestCSI_Code_ClearCSI_Status_CSI_ReferenceNumber2NotEmpty()
		{
			supportingDocument.CSI_Code = "1";
			AssertClearCSI_StatusForCSI_Code(ZString.Empty, "REF", ZString.Empty, true);
		}

		public void TestCSI_ReferenceNumber_ClearCSI_Status()
		{
			supportingDocument.CSI_ReferenceNumber = "REF";
			AssertClearCSI_StatusForCSI_ReferenceNumber(ZString.Empty, ZString.Empty, ZString.Empty, true);
		}

		public void TestCSI_ReferenceNumber_ClearCSI_Status_CSI_CodeNotEmpty()
		{
			supportingDocument.CSI_ReferenceNumber = "REF";
			AssertClearCSI_StatusForCSI_ReferenceNumber(ZString.Empty, ZString.Empty, "1", false);
		}

		public void TestCSI_ReferenceNumber_ClearCSI_Status_CSI_ReferenceNumber2NotEmpty()
		{
			supportingDocument.CSI_ReferenceNumber = "REF";
			AssertClearCSI_StatusForCSI_ReferenceNumber(ZString.Empty, "REF", ZString.Empty, false);
		}

		public void TestCSI_ReferenceNumber_ClearCSI_Status_CSI_ReferenceNumberNotEmpty()
		{
			supportingDocument.CSI_ReferenceNumber = "REF";
			AssertClearCSI_StatusForCSI_ReferenceNumber("REF2", ZString.Empty, ZString.Empty, false);
		}

		public void TestCSI_ReferenceNumber2_ClearCSI_Status()
		{
			supportingDocument.CSI_ReferenceNumber2 = "REF";
			AssertClearCSI_StatusForCSI_ReferenceNumber2(ZString.Empty, ZString.Empty, ZString.Empty, true);
		}

		public void TestCSI_ReferenceNumber2_ClearCSI_Status_CSI_CodeNotEmpty()
		{
			supportingDocument.CSI_ReferenceNumber2 = "REF";
			AssertClearCSI_StatusForCSI_ReferenceNumber2(ZString.Empty, ZString.Empty, "1", false);
		}

		public void TestCSI_ReferenceNumber2_ClearCSI_Status_CSI_ReferenceNumberNotEmpty()
		{
			supportingDocument.CSI_ReferenceNumber2 = "REF";
			AssertClearCSI_StatusForCSI_ReferenceNumber2("REF", ZString.Empty, ZString.Empty, false);
		}

		public void TestCSI_ReferenceNumber2_ClearCSI_Status_CSI_ReferenceNumber2NotEmpty()
		{
			supportingDocument.CSI_ReferenceNumber2 = "REF";
			AssertClearCSI_StatusForCSI_ReferenceNumber2(ZString.Empty, "REF2", ZString.Empty, false);
		}

		public void TestCanDelete()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Departure", true, supportingDocument.CanDelete);

				var arrivalSupportingDocument = CreatePhase5ArrivalHeaderAndSupportingDocument();
				AssertEquals("Arrival, Status != DEC", true, arrivalSupportingDocument.CanDelete);

				arrivalSupportingDocument.CSI_Status = SupportingDocumentStatusList.Codes.DEC;
				AssertEquals("Arrival, Status == DEC", false, arrivalSupportingDocument.CanDelete);
			});
		}

		public void TestReasonForNotAbleToDelete()
		{
			AssertEquals("Cannot delete Documents from Customs.", supportingDocument.ReasonForNotAbleToDelete);
		}

		public void TestHeader()
		{
			CombineAssertions(() =>
			{
				AssertSame("Header is returned correctly when Parent is NctsHeader", nctsHeader, supportingDocument.Header);

				var bill = nctsHeader.Bills.AddNew();
				var supportingDocumentForBill = bill.SupportingDocuments.AddNew();
				AssertSame("Header is returned correctly when Parent is NctsBill", nctsHeader, supportingDocumentForBill.Header);

				var goodsItem = bill.GoodsItems.AddNew();
				var supportingDocumentForGoodsItem = goodsItem.SupportingDocuments.AddNew();
				AssertSame("Header is returned correctly when Parent is NctsCommonCargoDesc", nctsHeader, supportingDocumentForGoodsItem.Header);

				var nctsHeaderArrival = Factory.New<NctsHeader>();
				nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
				nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var arrivalMovementHeader = nctsHeaderArrival.ArrivalMovementHeader;
				var supportingDocumentForArrivalMovHeader = arrivalMovementHeader.SupportingDocuments.AddNew();
				AssertSame("Header is returned correctly when Parent is NctsArrivalMovementHeader", nctsHeaderArrival, supportingDocumentForArrivalMovHeader.Header);
			});
		}

		protected override IEnumerable<NctsSupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.ArrivalGoodsItems.AddNew();
			yield return arrivalMovementHeader.SupportingDocuments.AddNew();
			yield return goodsItem.SupportingDocuments.AddNew();
			yield return bill.SupportingDocuments.AddNew();
		}

		protected override void LoadParentIfNeeded(BusinessObjectFactory factory, NctsSupportingDocument bizObj)
		{
			base.LoadParentIfNeeded(factory, bizObj);
			if (bizObj.Parent is NctsHeader _)
			{
				factory.Load<NctsHeader>(bizObj.CSI_ParentID);
			}
			else if (bizObj.Parent is NctsBill _)
			{
				factory.Load<NctsBill>(bizObj.CSI_ParentID);
			}
			else if (bizObj.Parent is NctsArrivalMovementHeader _)
			{
				factory.Load<NctsArrivalMovementHeader>(bizObj.CSI_ParentID);
			}
			else if (bizObj.Parent is NctsCommonCargoDesc _)
			{
				factory.Load<NctsCommonCargoDesc>(bizObj.CSI_ParentID);
			}
		}

		protected override BusinessObject GetNewBusinessObject() => supportingDocument;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			supportingDocument = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
		}
		NctsHeader nctsHeader;
		NctsSupportingDocument supportingDocument;

		(RefCusCodeList, UniversalReferenceTestDataHelper) CreateRefCusCodeListForTest()
		{
			var nctsCodeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupingEU = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			var dataGroupingLV = helper.CreateNewOrGetExistingDataGrouping("LV", "Latvia", dataGroupingEU);
			helper.CreateNewOrGetExistingCusCodeType(nctsCodeType, "Ncts Supporting Document Type");
			var refCusCodeList = helper.CreateCusCodeList(dataGroupingLV.ZZZ_DataGrouping, nctsCodeType, "ABC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			return (refCusCodeList, helper);
		}

		NctsSupportingDocument CreatePhase5ArrivalHeaderAndSupportingDocument()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			CusInBondMoveDetail movementDetail = header.ArrivalMovementHeader.MovementDetails.AddNew();
			var bill = header.Bills.AddNew();
			movementDetail.B9_B0 = bill.PK;
			var supportingDocument = bill.ArrivalGoodsItems.AddNew().SupportingDocuments.AddNew();
			return supportingDocument;
		}

		void AssertClearCSI_StatusForCSI_Code(ZString referenceNumber, ZString referenceNumber2, ZString code, bool expectedStatusCleared)
		{
			supportingDocument.CSI_ReferenceNumber = referenceNumber;
			supportingDocument.CSI_ReferenceNumber2 = referenceNumber2;
			supportingDocument.CSI_Status = SupportingDocumentStatusList.Codes.NEW;
			supportingDocument.CSI_Code = code;
			var result = expectedStatusCleared ? string.Empty : SupportingDocumentStatusList.Codes.NEW;
			AssertEquals(result, supportingDocument.CSI_Status);
		}

		void AssertClearCSI_StatusForCSI_ReferenceNumber(ZString referenceNumber, ZString referenceNumber2, ZString code, bool expectedStatusCleared)
		{
			supportingDocument.CSI_ReferenceNumber2 = referenceNumber2;
			supportingDocument.CSI_Status = SupportingDocumentStatusList.Codes.NEW;
			supportingDocument.CSI_Code = code;
			supportingDocument.CSI_ReferenceNumber = referenceNumber;
			var result = expectedStatusCleared ? string.Empty : SupportingDocumentStatusList.Codes.NEW;
			AssertEquals(result, supportingDocument.CSI_Status);
		}

		void AssertClearCSI_StatusForCSI_ReferenceNumber2(ZString referenceNumber, ZString referenceNumber2, ZString code, bool expectedStatusCleared)
		{
			supportingDocument.CSI_ReferenceNumber = referenceNumber;
			supportingDocument.CSI_Status = SupportingDocumentStatusList.Codes.NEW;
			supportingDocument.CSI_Code = code;
			supportingDocument.CSI_ReferenceNumber2 = referenceNumber2;
			var result = expectedStatusCleared ? string.Empty : SupportingDocumentStatusList.Codes.NEW;
			AssertEquals(result, supportingDocument.CSI_Status);
		}
	}

	sealed class NctsDepartureSupportingDocumentPhase5ForTest : NctsSupportingDocument
	{
		public NctsDepartureSupportingDocumentPhase5ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			CSI_ParentID = nctsHeader.PK;
			CSI_ParentTableCode = nctsHeader.TablePrefix;
		}

		internal ISupportingDocumentReadOnlyConditions ReadOnlyProvider => GetNewReadOnlyProvider();
	}

	sealed class NctsArrivalSupportingDocumentPhase5ForTest : NctsSupportingDocument
	{
		public NctsArrivalSupportingDocumentPhase5ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
			CSI_ParentID = arrivalMovementHeader.PK;
			CSI_ParentTableCode = arrivalMovementHeader.TablePrefix;
		}

		internal ISupportingDocumentReadOnlyConditions ReadOnlyProvider => GetNewReadOnlyProvider();

		internal Mock<ISupportingDocumentReadOnlyConditions> ReadOnlyProiderMock;

		protected override ISupportingDocumentReadOnlyConditions GetNewReadOnlyProvider() => ReadOnlyProiderMock?.Object ?? base.GetNewReadOnlyProvider();
	}

	sealed class NctsSupportingDocumentPhase4ForTest : NctsSupportingDocument
	{
		public NctsSupportingDocumentPhase4ForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			CSI_ParentID = nctsHeader.PK;
			CSI_ParentTableCode = nctsHeader.TablePrefix;
		}

		internal ISupportingDocumentReadOnlyConditions ReadOnlyProvider => GetNewReadOnlyProvider();
	}
}
