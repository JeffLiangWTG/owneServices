using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefCusCodeListAttributeTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListAttributeTypes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CommonPreviousDocument))]
	sealed class CommonPreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<CommonPreviousDocument>
	{
		public void TestCSI_LineNo_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_LineNoInfo, "Sequence Number", "Sequence No.", "Seq. No.");
		}

		public void TestCSI_Status_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_StatusInfo, "State of Unloading", "Unloaded State", string.Empty);
		}

		public void TestCSI_Code_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_CodeInfo, "Document Type", "Doc. Type", "Type");
		}

		public void TestCSI_Code_ClearReadOnlyProperties()
		{
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(Codes.PreviousDocumentOfNCTS, RefCusCodeListLevelTypes.House, Factory, RefCusCodeListAttributeTypes.Reference, RefCusCodeListAttributeTypes.Complement);
			previousDocument.CSI_ReferenceNumber = "REF";
			previousDocument.CSI_ReferenceNumber2 = "REF2";

			CombineAssertions(() =>
			{
				previousDocument.CSI_Code = refCusCodeList1.ZZD_Code;
				AssertEquals("TypeCode has attribute 'Reference' = 'Y', CSI_ReferenceNumber not read-only", "REF", previousDocument.CSI_ReferenceNumber);
				AssertEquals("TypeCode has attribute 'Complement' = 'Y', CSI_ReferenceNumber2 not read-only", "REF2", previousDocument.CSI_ReferenceNumber2);

				previousDocument.CSI_Code = refCusCodeList2.ZZD_Code;
				AssertEquals("TypeCode has attribute 'Reference' = 'N', CSI_ReferenceNumber not read-only", "REF", previousDocument.CSI_ReferenceNumber);
				AssertEquals("TypeCode has attribute 'Complement' = 'N', CSI_ReferenceNumber2 not read-only", "REF2", previousDocument.CSI_ReferenceNumber2);

				previousDocument.CSI_Code = refCusCodeList3.ZZD_Code;
				AssertEquals("TypeCode doesn't have attribute 'Reference', CSI_ReferenceNumber read-only", ZString.Empty, previousDocument.CSI_ReferenceNumber);
				AssertEquals("TypeCode doesn't have attribute 'Complement', CSI_ReferenceNumber2 read-only", ZString.Empty, previousDocument.CSI_ReferenceNumber2);
			});
		}

		public void TestRefCusCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(Codes.PreviousDocumentOfNCTS, "CusCodeType");

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Codes.PreviousDocumentOfNCTS, "PD1", "PD1 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelTypes.House);

			var refCusCodeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Codes.PreviousDocumentOfNCTS, "PD2", "PD2 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNull("CSI_Code empty", previousDocument.RefCusCode);

				previousDocument.CSI_Code = "PD1";
				AssertEquals("CSI_Code = 'PD1'", "PD1 DES", previousDocument.RefCusCode.ZZD_Description);

				previousDocument.CSI_Code = "PD2";
				AssertNull("No 'Level-House' attribute", previousDocument.RefCusCode);

				previousDocument.CSI_Code = "XYZ";
				AssertNull("CSI_Code invalid", previousDocument.RefCusCode);
			});
		}

		public void TestCSI_ReferenceNumber_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_ReferenceNumberInfo, "Reference Number", "Reference No.", "Reference");
		}

		public void TestCSI_ReferenceNumber2_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_ReferenceNumber2Info, "Complement of Information", "Complement Info.", "Complement");
		}

		public void TestSequenceNumber_Phase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			CombineAssertions(() =>
			{
				var previousDocument = nctsHeader.PreviousDocuments.AddNew();
				AssertEquals("SequenceNumber of previousDocument is 1", 1, previousDocument.CSI_LineNo);

				var previousDocument2 = nctsHeader.PreviousDocuments.AddNew();
				AssertEquals("SequenceNumber of previousDocument2 is 2", 2, previousDocument2.CSI_LineNo);

				previousDocument.Delete();
				AssertEquals("previousDocument is deleted, SequenceNumber of previousDocument2 is 1", 1, previousDocument2.CSI_LineNo);
			});
		}

		public void TestCSI_LineNo_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Phase 5 Arrival", true, Phase5ArrivalPreviousDocument.CSI_LineNoInfo.ReadOnly);
				AssertEquals("Phase 5 Departure", false, Phase5DeparturePreviousDocument.CSI_LineNoInfo.ReadOnly);
			});
		}

		public void TestCSI_Status_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Phase 5 Arrival", true, Phase5ArrivalPreviousDocument.CSI_StatusInfo.ReadOnly);
				AssertEquals("Phase 5 Departure", false, Phase5DeparturePreviousDocument.CSI_StatusInfo.ReadOnly);
			});
		}

		public void TestCSI_Code_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Phase 5 Arrival", true, Phase5ArrivalPreviousDocument.CSI_CodeInfo.ReadOnly);
				AssertEquals("Phase 5 Departure", false, Phase5DeparturePreviousDocument.CSI_CodeInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertPropertyIsReadOnlyWhenAttributeIsMissing(Phase5DeparturePreviousDocument, Phase5DeparturePreviousDocument.CSI_ReferenceNumberInfo, RefCusCodeListAttributeTypes.Reference, readOnlyWhenCodeNotInList: false);
				AssertEquals("Phase 5 Arrival", true, Phase5ArrivalPreviousDocument.CSI_ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber2_ReadOnly()
		{
			CombineAssertions(() =>
			{
				AssertPropertyIsReadOnlyWhenAttributeIsMissing(Phase5DeparturePreviousDocument, Phase5DeparturePreviousDocument.CSI_ReferenceNumber2Info, RefCusCodeListAttributeTypes.Complement, readOnlyWhenCodeNotInList: false);
				AssertEquals("Phase 5 Arrival", true, Phase5ArrivalPreviousDocument.CSI_ReferenceNumber2Info.ReadOnly);
			});
		}

		public void TestCSI_ItemNumber_Caption()
		{
			NCTSTestHelper.AssertCaptions(previousDocument.CSI_ItemNumberInfo, "Item Number", "", "");
		}

		public void TestCSI_Code_MaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Phase 5 Arrival", AutoCusSupportingInfo.Schema.CSI_CodeMaxLength, Phase5ArrivalPreviousDocument.CSI_CodeInfo.MaxLength);
				AssertEquals("Phase 5 Arrival Header", 4, Phase5ArrivalPreviousDocumentHeader.CSI_CodeInfo.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber_MaxLength()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("Phase 5 Departure in Transition Period", 35, Phase5DeparturePreviousDocument.CSI_ReferenceNumberInfo.MaxLength);
					AssertEquals("Phase 5 Departure Header in Transition Period", 35, Phase5DeparturePreviousDocumentHeader.CSI_ReferenceNumberInfo.MaxLength);
				}
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("Phase 5 Departure outside Transition Period", 70, Phase5DeparturePreviousDocument.CSI_ReferenceNumberInfo.MaxLength);
					AssertEquals("Phase 5 Departure Header outside Transition Period", 70, Phase5DeparturePreviousDocumentHeader.CSI_ReferenceNumberInfo.MaxLength);
				}
				AssertEquals("Phase 5 Arrival", AutoCusSupportingInfo.Schema.CSI_ReferenceNumberMaxLength, Phase5ArrivalPreviousDocument.CSI_ReferenceNumberInfo.MaxLength);
				AssertEquals("Phase 5 Arrival Header", 70, Phase5ArrivalPreviousDocumentHeader.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_ReferenceNumber2_MaxLength()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Phase 5 Arrival", AutoCusSupportingInfo.Schema.CSI_ReferenceNumber2MaxLength, Phase5ArrivalPreviousDocument.CSI_ReferenceNumber2Info.MaxLength);
				AssertEquals("Phase 5 Arrival Header", 35, Phase5ArrivalPreviousDocumentHeader.CSI_ReferenceNumber2Info.MaxLength);
			});
		}

		public void TestLookups()
		{
			AssertType<CommonPreviousDocumentLookups>(previousDocument.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CommonPreviousDocumentValidation>(Phase5ArrivalPreviousDocument.Validation);
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms()
		{
			var previousDocument = CreatePhase5ArrivalHeaderAndPreviousDocument();
			var arrivalMovementHeader = ((NctsBill)previousDocument.Parent).MovementDetail.ArrivalMoveHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => previousDocument.FieldsReadOnlyForPhase5Arrival, previousDocument);
		}

		public void TestCSIStatus_ReadOnlyForAcceptedUnloading()
		{
			var previousDocument = CreatePhase5ArrivalHeaderAndPreviousDocument();
			CombineAssertions(() =>
			{
				AssertEquals("This field should never be Editable for NCTS arrival phase 5", true, previousDocument.CSI_StatusInfo.ReadOnly);
				var movementDetail = ((NctsBill)previousDocument.Parent).MovementDetail;
				movementDetail.MoveHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				AssertEquals("ReadOnly", true, previousDocument.CSI_StatusInfo.ReadOnly);
			});
		}

		public void TestCSI_Code_ReadOnlyForAcceptedUnloading()
		{
			var previousDocument = CreatePhase5ArrivalHeaderAndPreviousDocument();
			CombineAssertions(() =>
			{
				AssertEquals("This field should never be editable", true, previousDocument.CSI_CodeInfo.ReadOnly);
				var movementDetail = ((NctsBill)previousDocument.Parent).MovementDetail;
				movementDetail.MoveHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				AssertEquals("ReadOnly", true, previousDocument.CSI_CodeInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber_ReadOnlyForAcceptedUnloading()
		{
			var previousDocument = CreatePhase5ArrivalHeaderAndPreviousDocument();
			CombineAssertions(() =>
			{
				AssertEquals("This field should never be Editable for NCTS arrival phase 5", true, previousDocument.CSI_ReferenceNumberInfo.ReadOnly);
				var movementDetail = ((NctsBill)previousDocument.Parent).MovementDetail;
				movementDetail.MoveHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				AssertEquals("ReadOnly", true, previousDocument.CSI_ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber2_ReadOnlyForAcceptedUnloading()
		{
			var previousDocument = CreatePhase5ArrivalHeaderAndPreviousDocument();
			CombineAssertions(() =>
			{
				AssertEquals("This field should never be Editable for NCTS arrival phase 5", true, previousDocument.CSI_ReferenceNumber2Info.ReadOnly);
				var movementDetail = ((NctsBill)previousDocument.Parent).MovementDetail;
				movementDetail.MoveHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				AssertEquals("ReadOnly", true, previousDocument.CSI_ReferenceNumber2Info.ReadOnly);
			});
		}

		public void TestCSI_Description_ReadOnlyForAcceptedUnloading()
		{
			var previousDocument = CreatePhase5ArrivalHeaderAndPreviousDocument();
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(((NctsBill)previousDocument.Parent).Header.ArrivalMovementHeader, x => previousDocument.CSI_DescriptionInfo.ReadOnly, previousDocument);
		}

		public void TestUnloadingRemarksAreSentToCustoms()
		{
			var previousDocument = CreatePhase5ArrivalHeaderAndPreviousDocument();
			CombineAssertions(() =>
			{
				AssertEquals("Not Sent", false, previousDocument.FieldsReadOnlyForPhase5Arrival);
				var movementDetail = ((NctsBill)previousDocument.Parent).MovementDetail;
				movementDetail.MoveHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, previousDocument.FieldsReadOnlyForPhase5Arrival);
			});
		}

		public void TestCSIStatus_ReadOnlyForSentToCustoms()
		{
			var previousDocument = CreatePhase5ArrivalHeaderAndPreviousDocument();
			CombineAssertions(() =>
			{
				AssertEquals("This field should never be Editable for NCTS arrival phase 5", true, previousDocument.CSI_StatusInfo.ReadOnly);
				var movementDetail = ((NctsBill)previousDocument.Parent).MovementDetail;
				movementDetail.MoveHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, previousDocument.CSI_StatusInfo.ReadOnly);
			});
		}

		public void TestCSI_Code_ReadOnlyForSentToCustoms()
		{
			var previousDocument = CreatePhase5ArrivalHeaderAndPreviousDocument();
			CombineAssertions(() =>
			{
				AssertEquals("This field should never be editable", true, previousDocument.CSI_CodeInfo.ReadOnly);
				var movementDetail = ((NctsBill)previousDocument.Parent).MovementDetail;
				movementDetail.MoveHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, previousDocument.CSI_CodeInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber_ReadOnlyForSentToCustoms()
		{
			var previousDocument = CreatePhase5ArrivalHeaderAndPreviousDocument();
			CombineAssertions(() =>
			{
				AssertEquals("This field should never be Editable for NCTS arrival phase 5", true, previousDocument.CSI_ReferenceNumberInfo.ReadOnly);
				var movementDetail = ((NctsBill)previousDocument.Parent).MovementDetail;
				movementDetail.MoveHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, previousDocument.CSI_ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber2_ReadOnlyForSentToCustoms()
		{
			var previousDocument = CreatePhase5ArrivalHeaderAndPreviousDocument();
			CombineAssertions(() =>
			{
				AssertEquals("This field should never be Editable for NCTS arrival phase 5", true, previousDocument.CSI_ReferenceNumber2Info.ReadOnly);
				var movementDetail = ((NctsBill)previousDocument.Parent).MovementDetail;
				movementDetail.MoveHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, previousDocument.CSI_ReferenceNumber2Info.ReadOnly);
			});
		}

		public void TestCSI_Description_ReadOnlyForSentToCustoms()
		{
			var previousDocument = CreatePhase5ArrivalHeaderAndPreviousDocument();
			CombineAssertions(() =>
			{
				AssertEquals("Editable", false, previousDocument.CSI_DescriptionInfo.ReadOnly);
				var movementDetail = ((NctsBill)previousDocument.Parent).MovementDetail;
				movementDetail.MoveHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("ReadOnly", true, previousDocument.CSI_DescriptionInfo.ReadOnly);
			});
		}

		public void TestIsInPhase5DepartureHouseConsignment()
		{
			var headerArrival = Factory.New<NctsHeader>();
			headerArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			headerArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var billArrival = headerArrival.Bills.AddNew();
			var headerPreviousDocumentArrival = headerArrival.PreviousDocuments.AddNew();
			var billPreviousDocumentArrival = billArrival.PreviousDocuments.AddNew();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var headerPreviousDocument = header.PreviousDocuments.AddNew();
			var billPreviousDocument = bill.PreviousDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Arrival headerPreviousDocument", false, headerPreviousDocumentArrival.IsInPhase5DepartureHouseConsignment);
				AssertEquals("Arrival billPreviousDocument", false, billPreviousDocumentArrival.IsInPhase5DepartureHouseConsignment);

				AssertEquals("Departure headerPreviousDocument", false, headerPreviousDocument.IsInPhase5DepartureHouseConsignment);
				AssertEquals("Departure billPreviousDocument", true, billPreviousDocument.IsInPhase5DepartureHouseConsignment);
			});
		}

		public void TestIsNCTSPreviousDocument() => CombineAssertions(() =>
		{
			previousDocument.CSI_Code = ZString.Empty;
			AssertEquals($"CSI_Code={previousDocument.CSI_Code}", false, previousDocument.IsNCTSPreviousDocument);
			previousDocument.CSI_Code = "N123";
			AssertEquals($"CSI_Code={previousDocument.CSI_Code}", true, previousDocument.IsNCTSPreviousDocument);
			previousDocument.CSI_Code = "X123";
			AssertEquals($"CSI_Code={previousDocument.CSI_Code}", false, previousDocument.IsNCTSPreviousDocument);
		});

		public void TestHumanReadableName()
		{
			AssertEquals("Previous Document", previousDocument.HumanReadableName);
		}

		public void TestCommonPreviousDocumentValidationDecider_Departure() => AssertType<CommonPreviousDocumentDepartureValidationDecider>(GetNewPreviousDocument().ValidationDecider);

		public void TestCommonPreviousDocumentValidationDecider_Arrival() => AssertType<CommonPreviousDocumentArrivalValidationDecider>(CreatePhase5ArrivalHeaderAndPreviousDocument().ValidationDecider);

		public void TestCSI_LineNo_RecalculatedWhenPreviousDocumentIsDeleted()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();

			var doc1 = bill.PreviousDocuments.AddNew();
			doc1.CSI_ReferenceNumber = "Doc 1";
			doc1.CSI_ItemNumber = 1;

			var doc2 = bill.PreviousDocuments.AddNew();
			doc2.CSI_ReferenceNumber = "Doc 2";
			doc2.CSI_ItemNumber = 2;

			var doc3 = bill.PreviousDocuments.AddNew();
			doc3.CSI_ReferenceNumber = "Doc 3";
			doc3.CSI_ItemNumber = 3;

			AssertEquals("Doc 1 CSI_LineNo", 1, doc1.CSI_LineNo);
			AssertEquals("Doc 2 CSI_LineNo", 2, doc2.CSI_LineNo);
			AssertEquals("Doc 3 CSI_LineNo", 3, doc3.CSI_LineNo);

			bill.PreviousDocuments.RemoveAndDelete(doc2);

			AssertEquals("Doc 1 CSI_LineNo, when Doc 2 is deleted", 1, doc1.CSI_LineNo);
			AssertEquals("Doc 3 CSI_LineNo, when Doc 2 is deleted", 2, doc3.CSI_LineNo);

			bill.PreviousDocuments.RemoveAndDelete(doc1);

			AssertEquals("Doc 3 CSI_LineNo, when Doc 1 is deleted", 1, doc3.CSI_LineNo);
		}

		CommonPreviousDocument CreatePhase5ArrivalHeaderAndPreviousDocument()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var movementDetail = header.ArrivalMovementHeader.MovementDetails.AddNew();
			movementDetail.B9_B0 = bill.PK;
			var previousDocument = bill.PreviousDocuments.AddNew();
			return previousDocument;
		}

		protected override IEnumerable<CommonPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = nctsHeader.Bills.AddNew();
			yield return bill.PreviousDocuments.AddNew();
			yield return nctsHeader.PreviousDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => previousDocument;

		CommonPreviousDocument GetNewPreviousDocument()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			return bill.PreviousDocuments.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			previousDocument = GetNewPreviousDocument();
		}
		CommonPreviousDocument previousDocument;

		void AssertPropertyIsReadOnlyWhenAttributeIsMissing(CommonPreviousDocument previousDocument, ZPropertyInfo propertyInfo, string attributeName, bool readOnlyWhenCodeNotInList = true)
		{
			var parentHeader = ((NctsBill)previousDocument.Parent).Header;
			var (refCusCodeList1, refCusCodeList2, refCusCodeList3) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(Codes.PreviousDocumentOfNCTS, RefCusCodeListLevelTypes.House, Factory, attributeName);

			previousDocument.CSI_Code = refCusCodeList1.ZZD_Code;
			AssertEquals($"NCTS Phase {parentHeader.BH_ApplicationCode} MovementType {parentHeader.BH_HeaderType}, TypeCode has attribute '{attributeName}' = 'Y'", false, propertyInfo.ReadOnly);

			previousDocument.CSI_Code = refCusCodeList2.ZZD_Code;
			AssertEquals($"NCTS Phase {parentHeader.BH_ApplicationCode} MovementType {parentHeader.BH_HeaderType}, TypeCode has attribute '{attributeName}' = 'N'", false, propertyInfo.ReadOnly);

			previousDocument.CSI_Code = refCusCodeList3.ZZD_Code;
			AssertEquals($"NCTS Phase {parentHeader.BH_ApplicationCode} MovementType {parentHeader.BH_HeaderType}, TypeCode doesn't have attribute '{attributeName}'", true, propertyInfo.ReadOnly);

			previousDocument.CSI_Code = ZString.Empty;
			AssertEquals($"NCTS Phase {parentHeader.BH_ApplicationCode} MovementType {parentHeader.BH_HeaderType}, TypeCode is empty", true, propertyInfo.ReadOnly);

			previousDocument.CSI_Code = "AAAA";
			AssertEquals($"NCTS Phase {parentHeader.BH_ApplicationCode} MovementType {parentHeader.BH_HeaderType}, TypeCode is not in the list", readOnlyWhenCodeNotInList, propertyInfo.ReadOnly);

			refCusCodeList1.Delete();
			refCusCodeList2.Delete();
			refCusCodeList3.Delete();
		}

		CommonPreviousDocument CreatePreviousDocument(string nctsHeaderApplicationCode, string nctsHeaderMovementType, string parentname = "bills")
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = nctsHeaderApplicationCode;
			nctsHeader.SetMovementType(nctsHeaderMovementType);
			return parentname == "NctsHeader" ? nctsHeader.PreviousDocuments.AddNew() : nctsHeader.Bills.AddNew().PreviousDocuments.AddNew();
		}

		CommonPreviousDocument Phase5ArrivalPreviousDocument => phase5ArrivalPreviousDocument ?? (phase5ArrivalPreviousDocument = CreatePreviousDocument(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival));
		CommonPreviousDocument phase5ArrivalPreviousDocument;
		CommonPreviousDocument Phase5DeparturePreviousDocument => phase5DeparturePreviousDocument ?? (phase5DeparturePreviousDocument = CreatePreviousDocument(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure));
		CommonPreviousDocument phase5DeparturePreviousDocument;
		CommonPreviousDocument Phase5ArrivalPreviousDocumentHeader => phase5ArrivalPreviousDocumentHeader ?? (phase5ArrivalPreviousDocumentHeader = CreatePreviousDocument(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Arrival, "NctsHeader"));
		CommonPreviousDocument phase5ArrivalPreviousDocumentHeader;
		CommonPreviousDocument Phase5DeparturePreviousDocumentHeader => phase5DeparturePreviousDocumentHeader ?? (phase5DeparturePreviousDocument = CreatePreviousDocument(CusInBondApplicationCodeList.Codes.NCTS5, NctsMovementType.Codes.Departure, "NctsHeader"));
		readonly CommonPreviousDocument phase5DeparturePreviousDocumentHeader;
	}
}
