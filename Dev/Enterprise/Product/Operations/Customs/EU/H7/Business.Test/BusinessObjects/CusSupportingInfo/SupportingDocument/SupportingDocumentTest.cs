using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(SupportingDocument))]
	sealed class SupportingDocumentBaseOnlyTest : SupportingDocumentTest<SupportingDocument>
	{
		public void TestMaxLength()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			CombineAssertions(() =>
			{
				AssertEquals(4, supportingDocument.CSI_CodeInfo.MaxLength);
				AssertEquals(70, supportingDocument.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCaptionResourceString()
		{
			var supportingDocument = Factory.New<SupportingDocument>();
			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(supportingDocument.CSI_CodeInfo, (string[])null, "Type", "Type", "Type", "Supporting document type.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(supportingDocument.CSI_ReferenceNumberInfo, (string[])null, "Reference Number", "Ref. No.", "Reference No.", "Supporting document reference number.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(supportingDocument.DocumentDescriptionInfo, (string[])null, "Description", "Desc.", "Description", "Supporting document type description.");
			});
		}

		public void TestSupportingDocumentDescription()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var documents = header.SupportingDocuments;
			var supportingDocument = documents.AddNew();
			AssertEquals(string.Empty, supportingDocument.DocumentDescription);

			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "DC40E", "270", "Delivery note", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "DC44I", "271", "Packing list", yesterday, yesterday);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, "DC44I", "325", "Proforma invoice", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "DC44I", "N952", "TIR Carnet", yesterday, tomorrow);
			Factory.Save();

			CombineAssertions(() =>
			{
				supportingDocument.CSI_Code = "270";
				AssertEquals(string.Empty, supportingDocument.DocumentDescription);
				supportingDocument.CSI_Code = "271";
				AssertEquals(string.Empty, supportingDocument.DocumentDescription);
				supportingDocument.CSI_Code = "325";
				AssertEquals(string.Empty, supportingDocument.DocumentDescription);
				supportingDocument.CSI_Code = "N952";
				AssertEquals("TIR Carnet", supportingDocument.DocumentDescription);
			});
		}

		public void TestCSL_ReferenceNumberMaxLength()
		{
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(SupportingDocument), nameof(SupportingDocument.CSI_ReferenceNumber), false, r => r.MaxLength == 70);
		}
	}

	public abstract class SupportingDocumentTest<T> : CusSupportingInfoTest<T>
		where T : SupportingDocument
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			return header.SupportingDocuments.AddNew();
		}

		protected override IEnumerable<T> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			yield return (T)header.SupportingDocuments.AddNew();

			var bill = header.Bills.AddNew();
			yield return (T)bill.SupportingDocuments.AddNew();

			var packedItem = bill.PackedItems.AddNew();
			yield return (T)packedItem.SupportingDocuments.AddNew();
		}
	}
}
