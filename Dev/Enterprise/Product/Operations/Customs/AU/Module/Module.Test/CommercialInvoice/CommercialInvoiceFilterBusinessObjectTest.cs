using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CommercialInvoiceFilterBusinessObject))]
	sealed class CommercialInvoiceFilterBusinessObjectTest : Customs.Module.Testing.CommercialInvoiceFilterBusinessObjectTest
	{
		public void TestGetExporterReferenceQuery()
		{
			var invoiceHeader1 = Factory.New<JobComInvoiceHeader>();
			invoiceHeader1.JZ_MessageType = JobMessageTypeList.Codes.Quarantine;
			invoiceHeader1.JZ_ExporterReference = "123456";
			var invoiceHeader2 = Factory.New<JobComInvoiceHeader>();
			invoiceHeader2.JZ_MessageType = JobMessageTypeList.Codes.Quarantine;
			invoiceHeader2.JZ_ExporterReference = "";
			var invoiceHeader3 = Factory.New<JobComInvoiceHeader>();
			invoiceHeader3.JZ_ExporterReference = "123456";
			var invoiceHeader4 = Factory.New<JobComInvoiceHeader>();
			invoiceHeader4.JZ_ExporterReference = "";
			Factory.Save();
			var filterBizObj = new CommercialInvoiceFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizObj[Core.Constants.AUCustoms.CommercialInvoiceFilter.ExporterReference];
			filter.Property = "123456";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			var invoices = NewFactory().Load<JobComInvoiceHeader>(filterBizObj.Filter);
			AssertEquals(1, invoices.Length);
			AssertEquals(invoiceHeader1.PK, invoices[0].PK);
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			invoices = NewFactory().Load<JobComInvoiceHeader>(filterBizObj.Filter);
			AssertEquals(1, invoices.Length);
			AssertEquals(invoiceHeader2.PK, invoices[0].PK);
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			invoices = NewFactory().Load<JobComInvoiceHeader>(filterBizObj.Filter);
			AssertEquals(1, invoices.Length);
			AssertEquals(invoiceHeader1.PK, invoices[0].PK);
		}

		public void TestGetPermitNumberQuery()
		{
			var invoiceHeader1 = Factory.New<JobComInvoiceHeader>();
			invoiceHeader1.JZ_StandAloneInvoiceDirection = AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader1QuarantineExDocHeader = invoiceHeader1.QuarantineExDocHeader;
			invoiceHeader1QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			invoiceHeader1QuarantineExDocHeader.QH_RequestForPermitNumber = "0001";

			var invoiceHeader2 = Factory.New<JobComInvoiceHeader>();
			invoiceHeader2.JZ_StandAloneInvoiceDirection = AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader2QuarantineExDocHeader = invoiceHeader2.QuarantineExDocHeader;
			invoiceHeader2QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Meat;
			invoiceHeader2QuarantineExDocHeader.QH_RequestForPermitNumber = "0002";

			var invoiceHeader3 = Factory.New<JobComInvoiceHeader>();
			invoiceHeader3.JZ_StandAloneInvoiceDirection = AUJobMessageTypeList.Codes.Quarantine;
			var invoiceHeader3QuarantineExDocHeader = invoiceHeader3.QuarantineExDocHeader;
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = invoiceHeader3QuarantineExDocHeader.PK;
			entryNumber.CE_ParentTable = "QuarantineExDocHeader";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumber.CE_EntryType = CusEntryNumber.EntryType.ActualArrivalStatus;
			entryNumber.CE_EntryNum = "0001";

			var invoiceHeader4 = Factory.New<JobComInvoiceHeader>();
			invoiceHeader4.JZ_StandAloneInvoiceDirection = AUJobMessageTypeList.Codes.Import;
			Factory.Save();

			var filterBizObj = new CommercialInvoiceFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizObj[Core.Constants.AUCustoms.CommercialInvoiceFilter.REXNumber];
			filter.Property = "0001";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var invoices = NewFactory().Load<JobComInvoiceHeader>(filterBizObj.Filter);
			AssertEquals(1, invoices.Length);
			AssertEquals("Should only query the matched invoice header from RFS entry.", invoiceHeader1.PK, invoices[0].PK);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			invoices = NewFactory().Load<JobComInvoiceHeader>(filterBizObj.Filter);
			AssertEquals(1, invoices.Length);
			AssertEquals("Should only query the matched invoice header which doesn't have a valid RFS number.", invoiceHeader3.PK, invoices[0].PK);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			invoices = NewFactory().Load<JobComInvoiceHeader>(filterBizObj.Filter);
			AssertEquals(2, invoices.Length);
			AssertContainsExactElementsInAnyOrder("Should only query these matched invoice headers which has a valid RFS number.", new[] { invoiceHeader1.PK, invoiceHeader2.PK }, invoices.Select(c => c.PK));
		}
	}
}
