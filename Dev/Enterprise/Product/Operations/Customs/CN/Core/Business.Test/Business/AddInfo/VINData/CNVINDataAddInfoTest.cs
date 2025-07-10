using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CNVINDataAddInfo))]
	class CNVINDataAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var testItem = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { })
				.InvoiceLine.VINDataCollection.AddNew();
			var result = new CNVINDataAddInfo(testItem.B7_AddInfoDataInfo);
			return result;
		}
	}
}
