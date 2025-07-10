using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DfiaExportItemDetailCollection))]
sealed class DfiaExportItemDetailCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<DfiaExportItemDetail>
{
	public void TestMaxCountValidation()
	{
		const string expectedMessage = "The maximum number of 9999 Duty Free Import Authorization has been exceeded.";
		var collection = ExportDetails;

		for (var i = 0; i < 9999; i++)
		{
			collection.AddNew();
		}

		CombineAssertions(() =>
		{
			AssertEquals("Maximum allowed", false, collection.HasErrors());
			AssertNoRowMessageError(collection.Last(), expectedMessage);

			var item = collection.AddNew();
			AssertHasRowMessageError(item, expectedMessage);
		});
	}

	public void TestDefaultCSI_Type()
	{
		var exportItemDetails = ExportDetails.AddNew();
		AssertEquals("CSI_Type", CusSupportingInfoTypeList.Codes.DutyFreeImportAuthorization, exportItemDetails.CSI_Type);
	}

	protected override CusSupportingInfoCollection<DfiaExportItemDetail> GetCusSupportingInfoCollection() => ExportDetails;

	DfiaExportItemDetailCollection ExportDetails => exportDetails ??= Factory.New<JobComInvoiceLine>().DfiaExportItemDetails;
	DfiaExportItemDetailCollection exportDetails;
}
