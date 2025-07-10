using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefCusCodeListTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsBillAdditionalDocument))]
	sealed class NctsBillAdditionalDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<NctsBillAdditionalDocument>
	{
		public void TestCSI_Code_Phase5Caption()
		{
			NCTSTestHelper.AssertCaptions(additionalDocument.CSI_CodeInfo, NctsHeader.Phase5CaptionKey, "Document Type", "Doc. Type", "Type");
		}

		public void TestCSI_ReferenceNumber_MaxLength()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("Departure Movement, in Phase5 Transition Period", 35, additionalDocument.CSI_ReferenceNumberInfo.MaxLength);
				}
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("Departure Movement, outside Phase5 Transition Period", 70, additionalDocument.CSI_ReferenceNumberInfo.MaxLength);
				}

				var arrivalAdditionalDocument = CreatePhase5ArrivalHeaderAndAdditionalDocument();
				AssertEquals("Phase 5 Arrival", AutoCusSupportingInfo.Schema.CSI_ReferenceNumberMaxLength, arrivalAdditionalDocument.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCSI_Description_MaxLength_Departure() => CombineAssertions(() =>
		{
			additionalDocument.Parent.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertEquals($"BH_ApplicationCode='{additionalDocument.Parent.Header.BH_ApplicationCode}'", 512, additionalDocument.CSI_DescriptionInfo.MaxLength);

			additionalDocument.Parent.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals($"BH_ApplicationCode='{additionalDocument.Parent.Header.BH_ApplicationCode}'", 70, additionalDocument.CSI_DescriptionInfo.MaxLength);
		});

		public void TestCSI_Description_MaxLength_Arrival() => CombineAssertions(() =>
		{
			var additionalDocument = CreatePhase5ArrivalHeaderAndAdditionalDocument();
			AssertEquals($"BH_ApplicationCode='{additionalDocument.Parent.Header.BH_ApplicationCode}'", 512, additionalDocument.CSI_DescriptionInfo.MaxLength);

			additionalDocument.Parent.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals($"BH_ApplicationCode='{additionalDocument.Parent.Header.BH_ApplicationCode}'", 70, additionalDocument.CSI_DescriptionInfo.MaxLength);
		});

		public void TestLookups()
		{
			AssertType<NctsBillAdditionalDocumentLookups>(additionalDocument.Lookups);
		}

		public void TestValidation()
		{
			AssertType<NctsBillAdditionalDocumentValidation>(additionalDocument.Validation);
		}

		public void TestSequenceNumber()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var additionalDocument = bill.AdditionalDocuments.AddNew();
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			CombineAssertions(() =>
			{
				AssertEquals("SequenceNumber of additionalDocument is 1", 1, additionalDocument.CSI_LineNo);

				var additionalDocument2 = bill.AdditionalDocuments.AddNew();
				additionalDocument2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("SequenceNumber of additionalDocument2 is 2", 2, additionalDocument2.CSI_LineNo);

				additionalDocument.Delete();
				AssertEquals("additionalDocument is deleted, SequenceNumber of additionalDocument2 is 1", 1, additionalDocument2.CSI_LineNo);
			});
		}

		public void TestSequenceNumber_DifferentSubTypes()
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

		public void TestSequenceNumber_WithOutParentTableCode()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();

			CombineAssertions(() =>
			{
				var additionalInfo1 = bill.AdditionalDocuments.AddNew();
				additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("SequenceNumber of additionalInfo (REF) is 1", 1, additionalInfo1.CSI_LineNo);

				var additionalInfo2 = bill.AdditionalDocuments.AddNew();
				additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("SequenceNumber of additionalInfo2 (REF) is 2", 2, additionalInfo2.CSI_LineNo);

				additionalInfo1.CSI_LineNo = 0;
				additionalInfo1.CSI_ParentTableCode = ZString.Empty;
				additionalInfo2.CSI_ParentTableCode = ZString.Empty;
				AssertEquals("additionalInfo CSI_ParentTableCode is empty, SequenceNumber of additionalInfo1 (REF) is 0", 0, additionalInfo1.CSI_LineNo);
				AssertEquals("additionalInfo CSI_ParentTableCode is empty, SequenceNumber of additionalInfo2 (INF) is 2", 2, additionalInfo2.CSI_LineNo);
			});
		}

		public void TestGetCodeTypeBySubType()
		{
			CombineAssertions(() =>
			{
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("When subType is REF, Code type should be AR44N", RefCusCodeListTypes.Codes.Code_AR44N, additionalDocument.GetCodeTypeBySubType());

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				AssertEquals("When subType is INF, Code type should be AI44N", RefCusCodeListTypes.Codes.Code_AI44N, additionalDocument.GetCodeTypeBySubType());

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				AssertEquals("When subType is REF, Code type should be TD44N", RefCusCodeListTypes.Codes.Code_TD44N, additionalDocument.GetCodeTypeBySubType());

				additionalDocument.CSI_SubType = "XXX";
				AssertEquals("When subType is invalid, Code type should be empty", ZString.Empty, additionalDocument.GetCodeTypeBySubType());
			});
		}

		public void TestRefCusCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_TD44N, "CusCodeTypeTD44N");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AI44N, "CusCodeTypeAI44N");

			AsssertRefCusCode(AdditionalInfoSubTypeList.Codes.AdditionalReference);
			AsssertRefCusCode(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
			AsssertRefCusCode(AdditionalInfoSubTypeList.Codes.TransportDocument);

			void AsssertRefCusCode(ZString subType)
			{
				additionalDocument.CSI_SubType = subType;
				var codeType = additionalDocument.GetCodeTypeBySubType();

				var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, codeType + "01", codeType + "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelTypes.House);

				var refCusCodeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, codeType + "02", codeType + "02 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				Factory.Save();

				CombineAssertions(() =>
				{
					AssertNull("CSI_Code empty", additionalDocument.RefCusCode);

					additionalDocument.CSI_Code = codeType + "01";
					AssertEquals("CSI_Code = '01'", codeType + "01 DES", additionalDocument.RefCusCode.ZZD_Description);

					additionalDocument.CSI_Code = codeType + "02";
					AssertNull("No 'Level-House' attribute", additionalDocument.RefCusCode);

					additionalDocument.CSI_Code = codeType + "XY";
					AssertNull("CSI_Code invalid", additionalDocument.RefCusCode);
				});
			}
		}

		public void TestRefCusCode_InvalidCSI_SubType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");
			helper.CreateNewOrGetExistingCusCodeType("INV", "CusCodeTypeINV");

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListTypes.Codes.Code_AR44N, "AR44N01", "AR44N01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelTypes.House);

			var refCusCodeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "INV", "INV01", "INV01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList2.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelTypes.House);

			Factory.Save();

			CombineAssertions(() =>
			{
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalDocument.CSI_Code = "AR44N01";
				AssertNotNull("Valid CSI_Code, RefCusCode isn't null", additionalDocument.RefCusCode);

				additionalDocument.CSI_SubType = "INV";
				additionalDocument.CSI_Code = "INV01";
				AssertNull("When subType is not valid, RefCusCode is null", additionalDocument.RefCusCode);
			});
		}

		public void TestSetUnloadedStateFromDocType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");

			var refCusCodeList1 = helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, RefCusCodeListTypes.Codes.Code_AR44N + "01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, "Level", RefCusCodeListLevelTypes.House);
			Factory.Save();
			additionalDocument.CSI_Code = refCusCodeList1.ZZD_Code;
			AssertEquals("A valid CSI_Code has been entered, so the unloaded state should be set to NEW", "NEW", additionalDocument.CSI_Status);

			additionalDocument.CSI_Status = ZString.Empty;
			additionalDocument.CSI_Code = "INVALID";
			AssertEquals("An invalid CSI_Code has been entered, so the unloaded state should not be set", ZString.Empty, additionalDocument.CSI_Status);

			additionalDocument.CSI_Status = "NEW";
			additionalDocument.CSI_Code = ZString.Empty;
			AssertEquals("The CSI_Code was cleared, so the unloaded state should also be cleared", ZString.Empty, additionalDocument.CSI_Status);

			additionalDocument.CSI_Status = "XXX";
			additionalDocument.CSI_Code = "INVALID";
			AssertEquals("An invalid CSI_Code has been entered, so the unloaded state should not be changed", "XXX", additionalDocument.CSI_Status);
		}

		public void TestSetKindFromDocType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_AR44N, "CusCodeTypeAR44N");

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.Code_AR44N, RefCusCodeListTypes.Codes.Code_AR44N + "01", "01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(refCusCodeList1.PK, "Level", RefCusCodeListLevelTypes.House);
			Factory.Save();

			additionalDocument.CSI_Code = refCusCodeList1.ZZD_Code;
			AssertEquals("A valid CSI_Code has been entered, so the kind should be set to REF", "REF", additionalDocument.CSI_SubType);

			additionalDocument.CSI_SubType = ZString.Empty;
			additionalDocument.CSI_Code = "INVALID";
			AssertEquals("An invalid CSI_Code has been entered, so the kind should not be set", ZString.Empty, additionalDocument.CSI_SubType);

			additionalDocument.CSI_SubType = "REF";
			additionalDocument.CSI_Code = ZString.Empty;
			AssertEquals("The CSI_Code was cleared, so the kind should also be cleared", ZString.Empty, additionalDocument.CSI_SubType);

			additionalDocument.CSI_SubType = "XXX";
			additionalDocument.CSI_Code = "INVALID";
			AssertEquals("An invalid CSI_Code has been entered, so the kind should not be changed", "XXX", additionalDocument.CSI_SubType);
		}

		public void TestBM_CSI_Code_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(additionalDocument.CSI_CodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Document Type", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Doc. Type", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Type", captionResourceString.ShortCaption);
			});
		}

		public void TestCSI_ReferenceNumber_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(additionalDocument.CSI_ReferenceNumberInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Reference Number of the additional document", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Reference Number", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Reference Number", captionResourceString.ShortCaption);
			});
		}

		public void TestCSI_Description_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(additionalDocument.CSI_DescriptionInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Description", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Description", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Descr.", captionResourceString.ShortCaption);
			});
		}

		public void TestCSI_LineNo_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(additionalDocument.CSI_LineNoInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Sequence Number", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Sequence No.", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Seq.No.", captionResourceString.ShortCaption);
			});
		}

		public void TestCSI_Status_Caption()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(additionalDocument.CSI_StatusInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "State of unloading", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Unloaded State", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Unloaded state", captionResourceString.ShortCaption);
			});
		}

		public void TestUnloadingRemarksAreFullyAcceptedByCustoms()
		{
			var additionalDocument = CreatePhase5ArrivalHeaderAndAdditionalDocument();
			var arrivalMovementHeader = additionalDocument.Parent.MovementDetail.ArrivalMoveHeader;

			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((NctsBillAdditionalDocument)x).FieldsReadOnlyForPhase5Arrival, additionalDocument);
		}

		public void TestUnloadingRemarksSentToCustoms()
		{
			var additionalDocument = CreatePhase5ArrivalHeaderAndAdditionalDocument();
			CombineAssertions(() =>
			{
				AssertEquals("Not Sent", false, additionalDocument.FieldsReadOnlyForPhase5Arrival);
				var movementDetail = additionalDocument.Parent.MovementDetail;
				movementDetail.MoveHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("Sent", true, additionalDocument.FieldsReadOnlyForPhase5Arrival);
			});
		}

		public void TestAutomaticSequenceNumberEnabled()
		{
			CombineAssertions(() =>
			{
				var bill = additionalDocument.Parent;
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("First Line", 1, (int)additionalDocument.CSI_LineNo);
				var secondLine = bill.AdditionalDocuments.AddNew();
				secondLine.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("Second Line", 2, (int)secondLine.CSI_LineNo);
				var thirdLine = bill.AdditionalDocuments.AddNew();
				thirdLine.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("Third Line", 3, (int)thirdLine.CSI_LineNo);
				secondLine.Delete();
				AssertEquals("First Line same as second deleted", 1, (int)additionalDocument.CSI_LineNo);
				AssertEquals("Third Line renumbered as second deleted", 2, (int)thirdLine.CSI_LineNo);
				var newThirdLine = bill.AdditionalDocuments.AddNew();
				newThirdLine.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				AssertEquals("New Third added", 3, (int)newThirdLine.CSI_LineNo);
			});
		}

		public void TestAutomaticSequenceNumberDisabled()
		{
			CombineAssertions(() =>
			{
				var firstLine = Factory.New<NctsBillAdditionalDocumentForTest>();
				var bill = additionalDocument.Parent;
				bill.AdditionalDocuments.RemoveAndDeleteAll();
				firstLine.AttachToParent(bill);
				AssertEquals("First Line", ZShort.Zero, firstLine.CSI_LineNo);
				firstLine.CSI_LineNo = 20;
				var secondLine = Factory.New<NctsBillAdditionalDocumentForTest>();
				secondLine.AttachToParent(bill);
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
				AssertType<NctsBillsDepartureAdditionalDocumentReadOnlyProvider>("When Ncts IsDeparture Phase 4, AdditionalDocumentReadOnlyProvider",
					CreateHouseBillsAdditionalDocument(
						CusInBondApplicationCodeList.Codes.NCTS4,
						NctsMovementType.Codes.Departure).GetNewReadOnlyProvider());

				AssertType<NctsBillsArrivalAdditionalDocumentReadOnlyProvider>("When Ncts IsArrival Phase 4, AdditionalDocumentReadOnlyProvider",
					CreateHouseBillsAdditionalDocument(
						CusInBondApplicationCodeList.Codes.NCTS4,
						NctsMovementType.Codes.Arrival).GetNewReadOnlyProvider());

				AssertType<NctsBillsDepartureAdditionalDocumentReadOnlyProvider>("When Ncts IsDeparture Phase 5, AdditionalDocumentReadOnlyProvider",
					CreateHouseBillsAdditionalDocument(
						CusInBondApplicationCodeList.Codes.NCTS5,
						NctsMovementType.Codes.Departure).GetNewReadOnlyProvider());

				AssertType<NctsBillsArrivalAdditionalDocumentReadOnlyProvider>("When Ncts IsArrival Phase 5, AdditionalDocumentReadOnlyProvider",
					CreateHouseBillsAdditionalDocument(
						CusInBondApplicationCodeList.Codes.NCTS5,
						NctsMovementType.Codes.Arrival).GetNewReadOnlyProvider());
			});
		}

		public void TestClearReadOnlyProperties()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertClearReadOnlyProperties(referenceNumberReadOnly: true, descriptionReadOnly: true);
			AssertClearReadOnlyProperties(referenceNumberReadOnly: false, descriptionReadOnly: false);
			AssertClearReadOnlyProperties(referenceNumberReadOnly: true, descriptionReadOnly: false);
			AssertClearReadOnlyProperties(referenceNumberReadOnly: false, descriptionReadOnly: true);
		}

		protected override IEnumerable<NctsBillAdditionalDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			yield return bill.AdditionalDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => additionalDocument;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsBill = nctsHeader.Bills.AddNew();
			additionalDocument = nctsBill.AdditionalDocuments.AddNew();
		}

		NctsHeader nctsHeader;
		NctsBill nctsBill;
		NctsBillAdditionalDocument additionalDocument;

		#region Implementation

		NctsBillAdditionalDocumentForTest CreateHouseBillsAdditionalDocument(string nctsPhase, string movementType)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(movementType);
			nctsHeader.BH_ApplicationCode = nctsPhase;

			var nctsBill = nctsHeader.Bills.AddNew();
			var additionalDocument = Factory.New<NctsBillAdditionalDocumentForTest>();
			additionalDocument.AttachToParent(nctsBill);
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			return additionalDocument;
		}

		NctsBillAdditionalDocument CreatePhase5ArrivalHeaderAndAdditionalDocument()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var additionalDocument = bill.AdditionalDocuments.AddNew();
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

			return additionalDocument;
		}

		void AssertClearReadOnlyProperties(bool referenceNumberReadOnly, bool descriptionReadOnly)
		{
			const string expectedReferenceNumber = "MyReferenceNumber";
			const string expectedDescription = "MyDescription";

			var additionalDocumentTest = Factory.New<NctsBillAdditionalDocumentForClearFieldsTest>();
			additionalDocumentTest.AttachToParent(nctsBill);

			var additionalDocumentReadOnlyProviderMock = new Mock<IAdditionalDocumentReadOnlyProvider>();
			additionalDocumentReadOnlyProviderMock.Setup(x => x.ReferenceNumberReadOnly).Returns(referenceNumberReadOnly);
			additionalDocumentReadOnlyProviderMock.Setup(x => x.DescriptionReadOnly).Returns(descriptionReadOnly);

			additionalDocumentTest.SetReadOnlyProvider(additionalDocumentReadOnlyProviderMock.Object);
			additionalDocumentTest.CSI_ReferenceNumber = expectedReferenceNumber;
			additionalDocumentTest.CSI_Description = expectedDescription;
			additionalDocumentTest.CSI_Code = "ref";
			additionalDocumentTest.CSI_SubType = "ref";
			CombineAssertions($"when referenceNumberReadOnly={referenceNumberReadOnly}, descriptionReadOnly={descriptionReadOnly}", () =>
			{
				AssertEquals("CSI_ReferenceNumber is cleared?", !referenceNumberReadOnly ? expectedReferenceNumber : string.Empty, additionalDocumentTest.CSI_ReferenceNumber);
				AssertEquals("CSI_Description is cleared?", !descriptionReadOnly ? expectedDescription : string.Empty, additionalDocumentTest.CSI_Description);
			});
		}

		#endregion
	}

	sealed class NctsBillAdditionalDocumentForTest : NctsBillAdditionalDocument
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

		protected override bool AutomaticSequenceNumberEnabled => false;
	}

	sealed class NctsBillAdditionalDocumentForClearFieldsTest : NctsBillAdditionalDocument
	{
		public NctsBillAdditionalDocumentForClearFieldsTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void AttachToParent(BusinessObject parent)
		{
			CSI_ParentTableCode = parent.TablePrefix;
			CSI_ParentID = parent.PK;
		}

		public void SetReadOnlyProvider(IAdditionalDocumentReadOnlyProvider additionalDocumentReadOnlyProvider)
		{
			this.additionalDocumentReadOnlyProvider = additionalDocumentReadOnlyProvider;
			ResetReadOnlyProvider();
		}

		protected override IAdditionalDocumentReadOnlyProvider GetNewReadOnlyProvider()
			=> additionalDocumentReadOnlyProvider;

		IAdditionalDocumentReadOnlyProvider additionalDocumentReadOnlyProvider;
	}
}
