using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEDocIsMissing()
		{
			const string expectedErrorMessage = "There is no eDoc attached to the supporting document.";
			var supportingDocument = bill.SupportingDocuments.AddNew();
			var validation = supportingDocument.Validation;
			var docManagerInfo = header.DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("Hello world"), "Invoice1.pdf", "CIV");
			supportingDocument.CSI_Status = RequestedSupportingStatusList.Codes.CAN;
			validation.ValidateEDoc();
			AssertNoWarning("Should not have warning message", supportingDocument.EDocInfo, expectedErrorMessage);
			supportingDocument.CSI_Status = RequestedSupportingStatusList.Codes.REQ;
			validation.ValidateEDoc();
			AssertHasWarning("Should have warning message", supportingDocument.EDocInfo, expectedErrorMessage);
			supportingDocument.EDoc = eDoc1.UniqueKey;
			validation.ValidateEDoc();
			AssertNoWarning("Should not have warning message", supportingDocument.EDocInfo, expectedErrorMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "785";
			bill = header.Bills.AddNew();
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction?.Dispose();
		}

		AsycudaBill bill;
		IDisposable disposableAction;
		AsycudaManifestHeader header;
	}
}
