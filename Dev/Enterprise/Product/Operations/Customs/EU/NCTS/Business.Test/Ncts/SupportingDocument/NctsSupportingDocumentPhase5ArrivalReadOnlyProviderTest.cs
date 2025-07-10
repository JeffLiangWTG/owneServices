using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsSupportingDocumentPhase5ArrivalReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestCSI_LineNo_ReadOnly()
		{
			AssertEquals(true, provider.CSI_LineNo_ReadOnly);
		}

		public void TestCSI_Status_ReadOnly()
		{
			AssertEquals(true, provider.CSI_Status_ReadOnly);
		}

		public void TestCSI_Code_ReadOnly_CSIStatusIsDEC()
		{
			AssertReadOnlyWhenCSI_StatusIsDEC((x) => x.CSI_Code_ReadOnly);
		}

		public void TestCSI_Code_ReadOnly_UnloadingRemarksAreFullyAccepted()
		{
			var arrivalMovementHeader = ((NctsBill)supportingDocument.Parent).MovementDetail.ArrivalMoveHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((ISupportingDocumentReadOnlyConditions)x).CSI_Code_ReadOnly, provider);
		}

		public void TestCSI_Code_ReadOnly_SentToCustoms()
		{
			AssertReadOnlyForSentToCustoms((x) => x.CSI_Code_ReadOnly);
		}

		public void TestCSI_Referencenumber_ReadOnly_CSIStatusIsDEC()
		{
			AssertReadOnlyWhenCSI_StatusIsDEC((x) => x.CSI_ReferenceNumber_ReadOnly);
		}

		public void TestCSI_Referencenumber_ReadOnly_UnloadingRemarksAreFullyAccepted()
		{
			var arrivalMovementHeader = ((NctsBill)supportingDocument.Parent).MovementDetail.ArrivalMoveHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((ISupportingDocumentReadOnlyConditions)x).CSI_ReferenceNumber_ReadOnly, provider);
		}

		public void TestCSI_Referencenumber_ReadOnly_SentToCustoms()
		{
			AssertReadOnlyForSentToCustoms((x) => x.CSI_ReferenceNumber_ReadOnly);
		}

		public void TestCSI_Referencenumber2_ReadOnly_CSIStatusIsDEC()
		{
			AssertReadOnlyWhenCSI_StatusIsDEC((x) => x.CSI_ReferenceNumber2_ReadOnly);
		}

		public void TestCSI_Referencenumber2_ReadOnly_UnloadingRemarksAreFullyAccepted()
		{
			var arrivalMovementHeader = ((NctsBill)supportingDocument.Parent).MovementDetail.ArrivalMoveHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((ISupportingDocumentReadOnlyConditions)x).CSI_ReferenceNumber2_ReadOnly, provider);
		}

		public void TestCSI_Referencenumber2_ReadOnly_SentToCustoms()
		{
			AssertReadOnlyForSentToCustoms((x) => x.CSI_ReferenceNumber2_ReadOnly);
		}

		public void TestCSI_Description_ReadOnly_CSIStatusIsDEC()
		{
			AssertReadOnlyWhenCSI_StatusIsDEC((x) => x.CSI_Description_ReadOnly);
		}

		public void TestCSI_Description_ReadOnly_UnloadingRemarksAreFullyAccepted()
		{
			var arrivalMovementHeader = ((NctsBill)supportingDocument.Parent).MovementDetail.ArrivalMoveHeader;
			NCTSTestHelper.AssertUnloadingRemarksReadOnlyForLockedDeclaration(arrivalMovementHeader, x => ((ISupportingDocumentReadOnlyConditions)x).CSI_Description_ReadOnly, provider);
		}
		public void TestCSI_Description_ReadOnly_SentToCustoms()
		{
			AssertReadOnlyForSentToCustoms((x) => x.CSI_Description_ReadOnly);
		}

		public void TestCSI_ItemNumber_ReadOnly()
		{
			NCTSTestHelper.AssertIsReadOnlyWhenAttributeIsMissing(Factory, UniversalReferenceConstants.RefCusCodeListLevelTypes.House, provider, supportingDocument, (x) => x.CSI_ItemNumber_ReadOnly, "ItemNumber");
		}

		public void TestCSI_Code_ReadOnly_StatusIsMIS()
		{
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.CSI_Code_ReadOnly);
		}

		public void TestCSI_ReferenceNumber_ReadOnly_StatusIsMIS()
		{
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.CSI_ReferenceNumber_ReadOnly);
		}

		public void TestCSI_ReferenceNumber2_ReadOnly_StatusIsMIS()
		{
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.CSI_ReferenceNumber2_ReadOnly);
		}

		public void TestCSI_Description_ReadOnly_StatusIsMIS()
		{
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.CSI_Description_ReadOnly);
		}

		public void TestCSI_ItemNumber_ReadOnly_StatusIsMIS()
		{
			var (refCusCodeList1, _, _) = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTest(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS, UniversalReferenceConstants.RefCusCodeListLevelTypes.House, Factory, "ItemNumber");
			supportingDocument.CSI_Code = refCusCodeList1.ZZD_Code;
			AssertReadOnlyWhenCSI_StatusIsMIS((x) => x.CSI_ItemNumber_ReadOnly);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = nctsHeader.Bills.AddNew();
			supportingDocument = bill.SupportingDocuments.AddNew();
			provider = new NctsSupportingDocumentPhase5ArrivalReadOnlyProvider(supportingDocument);
		}
		ISupportingDocumentReadOnlyConditions provider;
		NctsSupportingDocument supportingDocument;

		void AssertReadOnlyForCSI_StatusValue(string csiStatus, Func<ISupportingDocumentReadOnlyConditions, bool> readOnly)
		{
			CombineAssertions(() =>
			{
				supportingDocument.CSI_Status = ZString.Empty;
				AssertEquals("CSI_Status empty", expected: false, readOnly(provider));

				supportingDocument.CSI_Status = csiStatus;
				provider = new NctsSupportingDocumentPhase5ArrivalReadOnlyProvider(supportingDocument);
				AssertEquals($"CSI_Status = '{csiStatus}'", expected: true, readOnly(provider));

				supportingDocument.CSI_Status = SupportingDocumentStatusList.Codes.NEW;
				provider = new NctsSupportingDocumentPhase5ArrivalReadOnlyProvider(supportingDocument);
				AssertEquals("CSI_Status = 'NEW'", expected: false, readOnly(provider));
			});
		}

		void AssertReadOnlyWhenCSI_StatusIsDEC(Func<ISupportingDocumentReadOnlyConditions, bool> readOnly)
			=> AssertReadOnlyForCSI_StatusValue(SupportingDocumentStatusList.Codes.DEC, readOnly);

		void AssertReadOnlyWhenCSI_StatusIsMIS(Func<ISupportingDocumentReadOnlyConditions, bool> readOnly)
			=> AssertReadOnlyForCSI_StatusValue(SupportingDocumentStatusList.Codes.MIS, readOnly);

		void AssertReadOnlyForSentToCustoms(Func<ISupportingDocumentReadOnlyConditions, bool> readOnly)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Not ReadOnly", expected: false, readOnly(provider));

				var bill = (NctsBill)supportingDocument.Parent;
				var movementHeader = bill.MovementDetail.MoveHeader;
				movementHeader.BM_MessageStatus = "SNT";
				provider = new NctsSupportingDocumentPhase5ArrivalReadOnlyProvider(supportingDocument);
				AssertEquals("ReadOnly", expected: true, readOnly(provider));
			});
		}
	}
}
