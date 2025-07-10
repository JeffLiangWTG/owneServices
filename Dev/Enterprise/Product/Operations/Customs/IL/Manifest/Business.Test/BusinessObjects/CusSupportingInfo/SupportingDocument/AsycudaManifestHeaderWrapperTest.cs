using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderWrapper))]
	sealed class AsycudaManifestHeaderWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSupportingDocuments()
		{
			var headerWrapper = (AsycudaManifestHeaderWrapper)GetNewBusinessObject();

			AssertType<SupportingDocumentWrappersCollection>(headerWrapper.SupportingDocuments);
			AssertEquals("SupportingDocuments should have three items.", 3, headerWrapper.SupportingDocuments.Count);
			AssertContainsExactElementsInAnyOrder(headerWrapper.SupportingDocuments.Cast<SupportingDocumentWrapper>().Select(x => x.SupportingDocument.CSI_Code), new List<string> { "code1", "code2", "code6" });
		}

		public void TestRunPreSaveValidationCore()
		{
			var headerWrapper = (AsycudaManifestHeaderWrapper)GetNewBusinessObject();
			headerWrapper.RunPreSaveValidation();

			Assert("An error should be added if no supporting document is selected.", headerWrapper.HasErrors);
			Assert(headerWrapper.RowErrors.Any(e => e.Message.Contains("Please select at least one valid supporting document to send the message.")));

			var headerWrapper2 = (AsycudaManifestHeaderWrapper)GetNewBusinessObject();
			headerWrapper2.SupportingDocuments.Cast<SupportingDocumentWrapper>().First().IsSelected = true;
			headerWrapper2.RunPreSaveValidation();
			Assert(!headerWrapper2.RowErrors.Any(e => e.Message.Contains("Please select at least one valid supporting document to send the message.")));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var supportingDocument1 = bill.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "code1";

			var supportingDocument2 = bill.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "code2";
			supportingDocument2.CSI_Status = "REQ";

			var supportingDocument3 = bill.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "code3";
			supportingDocument3.CSI_Status = "REJ";

			var supportingDocument4 = bill.SupportingDocuments.AddNew();
			supportingDocument4.CSI_Code = "code4";
			supportingDocument4.CSI_Status = "VAL";

			var supportingDocument5 = bill.SupportingDocuments.AddNew();
			supportingDocument5.CSI_Code = "code5";
			supportingDocument5.CSI_Status = "SNT";

			var supportingDocument6 = bill.SupportingDocuments.AddNew();
			supportingDocument6.CSI_Code = "code6";
			supportingDocument6.CSI_Status = "FAL";

			return new AsycudaManifestHeaderWrapper(header);
		}
	}
}
