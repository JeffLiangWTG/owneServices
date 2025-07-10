using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(PreviousDocument))]
	sealed class PreviousDocumentBaseOnlyTest : PreviousDocumentTest<PreviousDocument>
	{
		public void TestMaxLength()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			CombineAssertions(() =>
			{
				AssertEquals(4, previousDocument.CSI_CodeInfo.MaxLength);
				AssertEquals(70, previousDocument.CSI_ReferenceNumberInfo.MaxLength);
			});
		}

		public void TestCaptionResourceString()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			CombineAssertions(() =>
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_CodeInfo, (string[])null, "Type", "Type", "Type", "Previous document type.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_ReferenceNumberInfo, (string[])null, "Reference Number", "Ref. No.", "Reference No.", "Previous document reference number.");
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.DocumentDescriptionInfo, (string[])null, "Description", "Desc.", "Description", "Previous document type description.");
			});
		}

		public void TestPreviousDocumentDescription()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var documents = header.PreviousDocuments;
			var previousDocument = documents.AddNew();
			AssertEquals(string.Empty, previousDocument.DocumentDescription);

			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "DC40E", "270", "Delivery note", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "DC40I", "271", "Packing list", yesterday, yesterday);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, "DC40I", "325", "Proforma invoice", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, "DC40I", "235", "Container list", yesterday, tomorrow);
			Factory.Save();

			CombineAssertions(() =>
			{
				previousDocument.CSI_Code = "270";
				AssertEquals(string.Empty, previousDocument.DocumentDescription);
				previousDocument.CSI_Code = "271";
				AssertEquals(string.Empty, previousDocument.DocumentDescription);
				previousDocument.CSI_Code = "325";
				AssertEquals(string.Empty, previousDocument.DocumentDescription);
				previousDocument.CSI_Code = "235";
				AssertEquals("Container list", previousDocument.DocumentDescription);
			});
		}
	}

	public abstract class PreviousDocumentTest<T> : CusSupportingInfoTest<T>
		where T : PreviousDocument
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			return header.PreviousDocuments.AddNew();
		}

		protected override IEnumerable<T> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			yield return (T)header.PreviousDocuments.AddNew();

			var bill = header.Bills.AddNew();
			yield return (T)bill.PreviousDocuments.AddNew();

			var packedItem = bill.PackedItems.AddNew();
			yield return (T)packedItem.PreviousDocuments.AddNew();
		}
	}
}
