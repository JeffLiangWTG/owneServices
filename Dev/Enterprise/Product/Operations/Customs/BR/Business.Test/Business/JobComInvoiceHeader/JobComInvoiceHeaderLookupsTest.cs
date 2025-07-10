using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			JobComInvoiceHeader parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(parent.Lookups.Invoice, parent);
		}

		public void TestValuationCodeList()
		{
			JobComInvoiceHeader parent = Factory.New<JobComInvoiceHeader>();
			AssertNotNull(parent.Lookups.ValuationCodeList);
			AssertType<ValuationCodeList>(parent.Lookups.ValuationCodeList);
			AssertEquals(6, parent.Lookups.ValuationCodeList.Count);
		}

		public void TestRelatedIndicatorList()
		{
			JobComInvoiceHeader parent = Factory.New<JobComInvoiceHeader>();
			AssertNotNull(parent.Lookups.RelatedIndicatorList);
			AssertType<RelatedIndicatorList>(parent.Lookups.RelatedIndicatorList);
			AssertEquals(3, parent.Lookups.RelatedIndicatorList.Count);
		}

		public void TestSupplierList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();

			using (BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var pivotFilterObj = invoiceHeader.Lookups.SupplierList.FilterBusinessObjectDefaults;
				AssertEquals("Filters count should be", 2, pivotFilterObj.Count);

				Assert("SupplierList has not 'Is Foreign Operator' filter", !pivotFilterObj.Cast<FilterBusinessObjectDefault>().ToList().Any(f => f.FilterName == "Is Foreign Operator"));
			}

			Factory.ClearCachedValue<ConsignorCollection>("BR|JobComInvoiceHeaderLookups|SupplierList_Y");

			using (BRCustomsDataRegistry.Instance.EnableForeignOperator.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var pivotFilterObj = invoiceHeader.Lookups.SupplierList.FilterBusinessObjectDefaults;
				AssertEquals("Filters count should be", 3, pivotFilterObj.Count);

				var foreignOperatorFilter = pivotFilterObj.Cast<FilterBusinessObjectDefault>().ToList().First(f => f.FilterName == "Is Foreign Operator");
				AssertNotNull("SupplierList has 'Is Foreign Operator' filter", foreignOperatorFilter);
				Assert("'Is Foreign Operator' filter is removable", foreignOperatorFilter.IsRemovable);
				AssertEquals("'Is Foreign Operator' filter value is YES", OrgConstants.FilterControl.IsForeignOperator.Code.Yes, foreignOperatorFilter.Value);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				pivotFilterObj = invoiceHeader.Lookups.SupplierList.FilterBusinessObjectDefaults;

				AssertEquals("Filters count should be", 2, pivotFilterObj.Count);
				Assert("SupplierList has not 'Is Foreign Operator' filter", !pivotFilterObj.Cast<FilterBusinessObjectDefault>().Any(f => f.FilterName == "Is Foreign Operator"));
			}
		}
	}
}
