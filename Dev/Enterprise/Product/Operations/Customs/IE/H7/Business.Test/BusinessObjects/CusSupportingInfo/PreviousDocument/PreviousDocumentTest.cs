using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(PreviousDocument))]
	sealed class PreviousDocumentTest : EU.H7.Business.Testing.PreviousDocumentTest<PreviousDocument>
	{
		public void TestPreviousDocumentDescription()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV2";
			var documents = header.PreviousDocuments;
			var previousDocument = documents.AddNew();
			AssertEquals(string.Empty, previousDocument.DocumentDescription);

			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC40E", "270", "Delivery note", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC40I", "271", "Packing list", yesterday, yesterday);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, "DC40I", "325", "Proforma invoice", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "DC40I", "235", "Container list", yesterday, tomorrow);
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
}
