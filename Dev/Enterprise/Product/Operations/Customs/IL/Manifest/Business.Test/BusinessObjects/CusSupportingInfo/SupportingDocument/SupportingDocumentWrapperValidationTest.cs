using System;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(SupportingDocumentWrapperValidation))]
	sealed class SupportingDocumentWrapperValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckIsSelected()
		{
			const string expectedErrorMessage = "The selected record is not related to an eDoc, please update the record from the Supporting Documents tab.";
			var supportingDocument = bill.SupportingDocuments.AddNew();
			var docManagerInfo = header.DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("Hello world"), "Invoice1.pdf", "CIV");
			var supportingDocumentWrapper = new SupportingDocumentWrapper(supportingDocument);
			supportingDocument.CSI_Status = RequestedSupportingStatusList.Codes.REQ;
			
			supportingDocumentWrapper.Validation.ValidateIsSelected();
			AssertNoError("Should not have error message", supportingDocumentWrapper.IsSelectedInfo, expectedErrorMessage);
			supportingDocumentWrapper.IsSelected = true;
			supportingDocumentWrapper.Validation.ValidateIsSelected();
			AssertHasError("Should have error message", supportingDocumentWrapper.IsSelectedInfo, expectedErrorMessage);
			supportingDocument.EDoc = eDoc1.UniqueKey;
			supportingDocumentWrapper.Validation.ValidateIsSelected();
			AssertNoError("Should not have error message", supportingDocumentWrapper.IsSelectedInfo, expectedErrorMessage);
		}

		public void TestAutoValidationType()
		{
			var supportingDocument = bill.SupportingDocuments.AddNew();
			var supportingDocumentWrapper = new SupportingDocumentWrapper(supportingDocument);

			AssertEquals(typeof(SupportingDocumentWrapperValidation), supportingDocumentWrapper.Validation.AutoValidationType);
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
