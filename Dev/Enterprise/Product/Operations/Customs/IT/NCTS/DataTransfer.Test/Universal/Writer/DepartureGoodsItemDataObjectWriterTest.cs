using System.Linq;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Testing.Core.Writing;

namespace Enterprise.Customs.IT.NCTS.DataTransfer.Testing;

sealed class DepartureGoodsItemDataObjectWriterTest : DataObjectWriterTest
{
	public void TestPopulateRemarksSupportingInfo()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
		goodsItem.Remarks = "ABCDEF";

		var writer = new NctsHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, nctsHeader)));
		var dataObject = writer.GetDataObject(nctsHeader);

		var invoices = dataObject.CommercialInfo.CommercialInvoiceCollection;
		AssertEquals("CommercialInvoiceCollection Count", 1, invoices.Count);
		var invoiceLines = dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection;
		AssertEquals("CommercialInvoiceLineCollection Count", 1, invoiceLines.Count);
		var remarksCustomsSupportingInfoList = invoiceLines[0].CustomsSupportingInformationCollection.Where(x => x.Category.Code.GetValueOrDefault() == "REM").ToArray();
		AssertEquals("'REM' CustomsSupportingInformation Length", 1, remarksCustomsSupportingInfoList.Length);
		CombineAssertions(() =>
		{
			var remarksSupportingInfo = remarksCustomsSupportingInfoList[0];
			AssertEquals("Category.Description", "Remarks", remarksSupportingInfo.Category.Description);
			AssertEquals("Description", "ABCDEF", remarksSupportingInfo.Description);
		});
	}
}
