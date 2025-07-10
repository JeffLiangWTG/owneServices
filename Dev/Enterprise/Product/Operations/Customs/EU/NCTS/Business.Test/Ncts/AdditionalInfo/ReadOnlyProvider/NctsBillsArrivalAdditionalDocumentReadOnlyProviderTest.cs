using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;
using RefCusCodeListAttributeTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListAttributeTypes;
using RefCusCodeListTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillsArrivalAdditionalDocumentReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NctsBillsArrivalAdditionalDocumentReadOnlyProvider(additionalDocument: null));
		}

		public void TestCSI_ReferenceNumberReadOnly_ForAcceptedUnloading()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalDocument.CSI_Code = GetRefCusCodeListForTest(RefCusCodeListAttributeTypes.Reference).ZZD_Code;

			AssertEquals("CSI_ReferenceNumber isn't editable", false, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
			var movementDetail = additionalDocument.Parent.MovementDetail;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(
				movementDetail.ArrivalMoveHeader,
				x => ((NctsBillAdditionalDocument)x).CSI_ReferenceNumberInfo.ReadOnly,
				additionalDocument);
		}

		public void TestCSI_ReferenceNumberReadOnly_ForMessageSent()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalDocument.CSI_Code = GetRefCusCodeListForTest(RefCusCodeListAttributeTypes.Reference).ZZD_Code;

			CombineAssertions("CSI_ReferenceNumber ReadOnly", () =>
			{
				AssertEquals("When BM_MessageStatus isn't SentToCustom", false, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
				var movementDetail = additionalDocument.Parent.MovementDetail;
				movementDetail.MoveHeader.BM_MessageStatus = NctsMessageStatusList.Codes.SentToCustoms;
				AssertEquals("When BM_MessageStatus is SentToCustom", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_ReferenceNumber2ReadOnly_ForAcceptedUnloading()
		{
			additionalDocument.CSI_Code = GetRefCusCodeListForTest(RefCusCodeListAttributeTypes.Reference).ZZD_Code;

			AssertEquals("CSI_ReferenceNumber2 isn't editable", false, additionalDocument.CSI_ReferenceNumber2Info.ReadOnly);
			var movementDetail = additionalDocument.Parent.MovementDetail;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(
				movementDetail.ArrivalMoveHeader,
				x => ((NctsBillAdditionalDocument)x).CSI_ReferenceNumber2Info.ReadOnly,
				additionalDocument);
		}

		public void TestCSI_ReferenceNumber2ReadOnly_ForMessageSent()
		{
			additionalDocument.CSI_Code = GetRefCusCodeListForTest(RefCusCodeListAttributeTypes.Reference).ZZD_Code;
			CombineAssertions("CSI_ReferenceNumber2 ReadOnly", () =>
			{
				AssertEquals("When BM_MessageStatus isn't Sent", false, additionalDocument.CSI_ReferenceNumber2Info.ReadOnly);
				var movementDetail = additionalDocument.Parent.MovementDetail;
				movementDetail.MoveHeader.BM_MessageStatus = NctsMessageStatusList.Codes.SentToCustoms;
				AssertEquals("When BM_MessageStatus is SentToCustom", true, additionalDocument.CSI_ReferenceNumber2Info.ReadOnly);
			});
		}

		public void TestCSI_DescriptionReadOnly_ForAcceptedUnloading()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalDocument.CSI_Code = GetRefCusCodeListForTest(RefCusCodeListAttributeTypes.Complement).ZZD_Code;

			AssertEquals("CSI_Description isn't editable", false, additionalDocument.CSI_DescriptionInfo.ReadOnly);
			var movementDetail = additionalDocument.Parent.MovementDetail;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(
				movementDetail.ArrivalMoveHeader,
				x => ((NctsBillAdditionalDocument)x).CSI_DescriptionInfo.ReadOnly,
				additionalDocument);
		}

		public void TestCSI_DescriptionReadOnly_ForMessageSent()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalDocument.CSI_Code = GetRefCusCodeListForTest(RefCusCodeListAttributeTypes.Complement).ZZD_Code;
			CombineAssertions("CSI_Description ReadOnly", () =>
			{
				AssertEquals("When BM_MessageStatus isn't Sent", false, additionalDocument.CSI_DescriptionInfo.ReadOnly);
				var movementDetail = additionalDocument.Parent.MovementDetail;
				movementDetail.MoveHeader.BM_MessageStatus = NctsMessageStatusList.Codes.SentToCustoms;
				AssertEquals("When BM_MessageStatus is Sent", true, additionalDocument.CSI_ReferenceNumberInfo.ReadOnly);
			});
		}

		public void TestCSI_StatusReadOnly()
		{
			AssertEquals("CSI_Status is read-only", true, additionalDocument.CSI_StatusInfo.ReadOnly);
		}

		public void TestCSI_CodeReadOnly()
		{
			CombineAssertions("CSI_Code ReadOnly", () =>
			{
				additionalDocument.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.DEC;
				AssertEquals("When CSI_Status = DEC", true, additionalDocument.CSI_CodeInfo.ReadOnly);

				additionalDocument.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.NEW;
				AssertEquals("When CSI_Status = New", false, additionalDocument.CSI_CodeInfo.ReadOnly);
			});
		}

		public void TestCSI_CodeReadOnly_ForMessageSent()
		{
			CombineAssertions("CSI_Code ReadOnly", () =>
			{
				AssertEquals("When BM_MessageStatus is not Sent", false, additionalDocument.CSI_CodeInfo.ReadOnly);
				var movementHeader = additionalDocument.Parent.Header.ArrivalMovementHeader;
				movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("When BM_MessageStatus is Sent", true, additionalDocument.CSI_CodeInfo.ReadOnly);
			});
		}

		public void TestCSI_CodeReadOnly_ForAcceptedUnloading()
		{
			AssertEquals("CSI_Code isn't editable", false, additionalDocument.CSI_CodeInfo.ReadOnly);
			var movementHeader = additionalDocument.Parent.Header.ArrivalMovementHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(
				movementHeader,
				x => ((NctsBillAdditionalDocument)x).CSI_CodeInfo.ReadOnly,
				additionalDocument);
		}

		public void TestCSI_SubTypeReadOnly()
		{
			CombineAssertions("CSI_SubType ReadOnly", () =>
			{
				additionalDocument.CSI_Status = ZString.Empty;
				AssertEquals("When CSI_Status is Empty", false, additionalDocument.CSI_SubTypeInfo.ReadOnly);

				additionalDocument.CSI_Status = "NEW";
				AssertEquals("When CSI_Status is NEW", false, additionalDocument.CSI_SubTypeInfo.ReadOnly);

				additionalDocument.CSI_Status = "DEC";
				AssertEquals("When CSI_Status is DEC", true, additionalDocument.CSI_SubTypeInfo.ReadOnly);
			});
		}

		public void TestCSI_SubTypeReadOnly_ForAcceptedUnloading()
		{
			additionalDocument.CSI_Code = GetRefCusCodeListForTest(RefCusCodeListAttributeTypes.Complement).ZZD_Code;

			AssertEquals("CSI_SubType isn't editable", false, additionalDocument.CSI_SubTypeInfo.ReadOnly);
			var movementDetail = additionalDocument.Parent.MovementDetail;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(
				movementDetail.ArrivalMoveHeader,
				x => ((NctsBillAdditionalDocument)x).CSI_SubTypeInfo.ReadOnly,
				additionalDocument);
		}

		public void TestCSI_SubTypeReadOnly_ForMessageSent()
		{
			additionalDocument.CSI_Code = GetRefCusCodeListForTest(RefCusCodeListAttributeTypes.Complement).ZZD_Code;
			CombineAssertions("CSI_SubType ReadOnly", () =>
			{
				AssertEquals("When BM_MessageStatus is not sent", false, additionalDocument.CSI_SubTypeInfo.ReadOnly);
				var movementDetail = additionalDocument.Parent.MovementDetail;
				movementDetail.MoveHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("When BM_MessageStatus is sent", true, additionalDocument.CSI_SubTypeInfo.ReadOnly);
			});
		}

		public void TestCSI_LineNoReadOnly()
		{
			AssertEquals("CSI_LineNo is read-only", true, additionalDocument.CSI_LineNoInfo.ReadOnly);
		}

		public void TestAdditionalInfoReadOnly_StatusIsMIS()
		{
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.AdditionalInfoReadOnly);
		}

		public void TestReferenceNumberReadOnly_StatusIsMIS()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalDocument.CSI_Code = GetRefCusCodeListForTest(RefCusCodeListAttributeTypes.Reference).ZZD_Code;
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.ReferenceNumberReadOnly);
		}

		public void TestReferenceNumber2ReadOnly_StatusIsMIS()
		{
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.ReferenceNumber2ReadOnly);
		}

		public void TestDescriptionReadOnly_StatusIsMIS()
		{
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalDocument.CSI_Code = GetRefCusCodeListForTest(RefCusCodeListAttributeTypes.Complement).ZZD_Code;
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.DescriptionReadOnly);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			var nctsBill = nctsHeader.Bills.AddNew();
			additionalDocument = nctsBill.AdditionalDocuments.AddNew();
		}

		NctsHeader nctsHeader;
		NctsBillAdditionalDocument additionalDocument;

		#endregion

		void AssertReadOnlyWhenCSI_StatusIsMIS(Func<IAdditionalDocumentReadOnlyProvider, bool> readOnly)
		{
			CombineAssertions(() =>
			{
				var provider = new NctsBillsArrivalAdditionalDocumentReadOnlyProvider(additionalDocument);
				additionalDocument.CSI_Status = ZString.Empty;
				AssertEquals("CSI_Status empty", expected: false, readOnly(provider));

				additionalDocument.CSI_Status = SupportingDocumentStatusList.Codes.MIS;
				provider = new NctsBillsArrivalAdditionalDocumentReadOnlyProvider(additionalDocument);
				AssertEquals("CSI_Status = 'MIS'", expected: true, readOnly(provider));

				additionalDocument.CSI_Status = SupportingDocumentStatusList.Codes.NEW;
				provider = new NctsBillsArrivalAdditionalDocumentReadOnlyProvider(additionalDocument);
				AssertEquals("CSI_Status = 'NEW'", expected: false, readOnly(provider));
			});
		}

		Universal.RefCusCodeList GetRefCusCodeListForTest(string attributeName)
		{
			var (refCusCodeList1, _, _) =
				CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(
					RefCusCodeListTypes.Codes.Code_AI44N,
					RefCusCodeListLevelTypes.House,
					Factory,
					attributeName);
			return refCusCodeList1;
		}
	}
}
