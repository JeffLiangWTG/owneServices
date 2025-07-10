using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(DfiaImportItemDetailCollection))]
sealed class DfiaImportItemDetailCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<DfiaImportItemDetail>
{
	public void TestMaxCountValidation()
	{
		const string expectedMessage = "The maximum number of 9999 Duty Free Import Authorization has been exceeded.";
		var collection = ImportDetails;

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
		var importItem = ImportDetails.AddNew();
		AssertEquals("CSI_Type", CusSupportingInfoTypeList.Codes.DutyFreeImportAuthorization, importItem.CSI_Type);
	}

	protected override CusSupportingInfoCollection<DfiaImportItemDetail> GetCusSupportingInfoCollection() => ImportDetails;

	DfiaExportItemDetail ExportItem => exportItem ??= Factory.New<JobComInvoiceLine>().DfiaExportItemDetails.AddNew();
	DfiaExportItemDetail exportItem;

	DfiaImportItemDetailCollection ImportDetails => importDetails ??= ExportItem.DfiaImportItemDetails;
	DfiaImportItemDetailCollection importDetails;
}
