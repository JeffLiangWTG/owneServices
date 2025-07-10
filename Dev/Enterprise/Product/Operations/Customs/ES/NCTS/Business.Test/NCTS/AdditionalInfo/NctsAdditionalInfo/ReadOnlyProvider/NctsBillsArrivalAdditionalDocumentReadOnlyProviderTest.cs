using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsBillsArrivalAdditionalDocumentReadOnlyProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("When additional document is null", ()
				=> new NctsBillsArrivalAdditionalDocumentReadOnlyProvider(additionalDocument: null, baseReadOnlyProvider: null));

				AssertExceptionThrown<ArgumentNullException>("When baseReadOnlyProvider is null", ()
					=> new NctsBillsArrivalAdditionalDocumentReadOnlyProvider(additionalDocument: additionalDocument, baseReadOnlyProvider: null));
			});
		}

		public void TestCSI_Status_ReadOnly()
		{
			additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			CombineAssertions(() =>
			{
				AssertEquals("When Status is New, CSI_Status is readonly", true, additionalDocument.CSI_StatusInfo.ReadOnly);

				additionalDocument.CSI_Status = NctsUnloadedStateList.Codes.DEC;
				AssertEquals("When Status is not New, Unloading Remarks is not readonly and Unloading Remarks are not accepted then CSI_Status is not readonly", false, additionalDocument.CSI_StatusInfo.ReadOnly);

				nctsHeader.CommonMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("When Status is not New but Unloading Remarks is sent, CSI_Status is readonly", true, additionalDocument.CSI_StatusInfo.ReadOnly);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
			var nctsBill = nctsHeader.Bills.AddNew();

			additionalDocument = nctsBill.AdditionalDocuments.AddNew();
		}
		NctsHeader nctsHeader;
		NctsBillAdditionalDocument additionalDocument;
	}
}
