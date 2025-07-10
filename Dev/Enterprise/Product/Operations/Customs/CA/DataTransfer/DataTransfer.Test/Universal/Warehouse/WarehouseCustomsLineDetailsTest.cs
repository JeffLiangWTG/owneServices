using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineDetailsTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var invoiceLine = CreateInvoiceLine();
			var fallbackDetail = new WarehouseCustomsFallbackDetail() { IsExWarehouse = false, InvoiceLineAddInfosApplicableForInwardWarehousing = new List<string>(new[] { "99TariffCode", "RN_NKExport" }) };
			var lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "12345000000023", 2, "12345000000012", 3, "99TariffCode=9902*RN_NKExport=US");

			fallbackDetail.IsExWarehouse = true;
			lineDetails = new WarehouseCustomsLineDetails(Factory, invoiceLine, fallbackDetail);
			AssertWarehouseCustomsLineDetails(lineDetails, "12345000000023", 2, "12345000000012", 3, "");
		}

		CommercialInvoiceLine CreateInvoiceLine()
		{
			var invoiceLine = new CommercialInvoiceLine()
			{
				LineNo = 1,
				EntryLineNumber = 2,
				EntryNumber = "12345000000023",
				PreviousEntryNumber = "12345000000012",
				PreviousEntryLineNumber = 3,
				AddInfoCollection = new List<UniversalAddInfo>(new[]
				{
					new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.CA_99TariffCode.Substring(3), Value = "9902" },
					new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.CA_RN_NKExport.Substring(3), Value = "US" }
				})
			};
			return invoiceLine;
		}

		internal static void AssertWarehouseCustomsLineDetails(IWarehouseCustomsLineDetails lineDetails, ZString entryNumber, ZShort entryLineNumber, ZString previousEntryNumber, ZShort previousEntryLineNumber, ZString addInfos)
		{
			AssertEquals("EntryNumber", entryNumber, lineDetails.EntryNumber);
			AssertEquals("EntryLineNumber", entryLineNumber, lineDetails.EntryLineNumber);
			AssertEquals("PreviousEntryNumber", previousEntryNumber, lineDetails.PreviousEntryNumber);
			AssertEquals("PreviousEntryLineNumber", previousEntryLineNumber, lineDetails.PreviousEntryLineNumber);
			AssertEquals("AddInfos", addInfos, lineDetails.AddInfos);
		}
	}
}
