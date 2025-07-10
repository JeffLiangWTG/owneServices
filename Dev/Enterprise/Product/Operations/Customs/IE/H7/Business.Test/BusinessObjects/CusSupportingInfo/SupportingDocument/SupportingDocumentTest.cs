using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(SupportingDocument))]
	sealed class SupportingDocumentTest : EU.H7.Business.Testing.SupportingDocumentTest<SupportingDocument>
	{
		public void TestSupportingDocumentDescription()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV2";
			var documents = header.SupportingDocuments;
			var supportingDocument = documents.AddNew();
			AssertEquals(string.Empty, supportingDocument.DocumentDescription);

			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC40E", "270", "Delivery note", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC44I", "271", "Packing list", yesterday, yesterday);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, "DC44I", "325", "Proforma invoice", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC44I", "N952", "TIR Carnet", yesterday, tomorrow);
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
	}
}
