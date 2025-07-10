using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(InvoiceBatchFilterBusinessObject))]
	public class InvoiceBatchFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestTransactionNumberFilter()
		{
			Header1.IsManuallySetTransactionNumber_ForTestOnly = true;
			Header2.IsManuallySetTransactionNumber_ForTestOnly = true;

			Header1.AH_TransactionNum = "11111111";
			Header2.AH_TransactionNum = "22222222";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterBO["Batch Number"];

			filter.Property = "11111111";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Header1", FilterCollection.Contains(Header1));
			Assert("Expecting collection not to contain Header2", !FilterCollection.Contains(Header2));

			filter.Property = "2222";
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Header1", !FilterCollection.Contains(Header1));
			Assert("Expecting collection not to contain Header2", !FilterCollection.Contains(Header2));

			filter.Property = "33333333";
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Header1", !FilterCollection.Contains(Header1));
			Assert("Expecting collection not to contain Header2", !FilterCollection.Contains(Header2));
		}

		public void TestDebtorFilter()
		{
			Header1.AH_OH = Debtor1.PK;
			Header2.AH_OH = Debtor2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Debtor"];

			filter.Property = Debtor1.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Header1", FilterCollection.Contains(Header1));
			Assert("Expecting collection not to contain Header2", !FilterCollection.Contains(Header2));

			filter.Property = Debtor3.PK;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Header1", !FilterCollection.Contains(Header1));
			Assert("Expecting collection not to contain Header2", !FilterCollection.Contains(Header2));
		}

		public void TestPostDateFilter()
		{
			Header1.AH_PostDate = new ZDateTime(2000, 1, 1, 11, 0, 0);      // 2 Jan 11:00
			Header2.AH_PostDate = new ZDateTime(2000, 2, 2, 22, 0, 0);      // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Post Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Header1", !FilterCollection.Contains(Header1));
			Assert("Expecting collection to contain Header2", FilterCollection.Contains(Header2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Header1", FilterCollection.Contains(Header1));
			Assert("Expecting collection not to contain Header2", !FilterCollection.Contains(Header2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Header1", FilterCollection.Contains(Header1));
			Assert("Expecting collection to contain Header2", FilterCollection.Contains(Header2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Header1", FilterCollection.Contains(Header1));
			Assert("Expecting collection not to contain Header2", !FilterCollection.Contains(Header2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Header1", !FilterCollection.Contains(Header1));
			Assert("Expecting collection not to contain Header2", !FilterCollection.Contains(Header2));
		}

		public void TestTransactionDateFilter()
		{
			Header1.AH_InvoiceDate = new ZDateTime(2000, 1, 1, 11, 0, 0);       // 2 Jan 11:00
			Header2.AH_InvoiceDate = new ZDateTime(2000, 2, 2, 22, 0, 0);       // 2 Feb 22:00

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterBO["Transaction Date"];

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 2, 2);
			filter.Property2 = ZDateTime.Empty;
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Header1", !FilterCollection.Contains(Header1));
			Assert("Expecting collection to contain Header2", FilterCollection.Contains(Header2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Header1", FilterCollection.Contains(Header1));
			Assert("Expecting collection not to contain Header2", !FilterCollection.Contains(Header2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 2, 2);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Header1", FilterCollection.Contains(Header1));
			Assert("Expecting collection to contain Header2", FilterCollection.Contains(Header2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 1);
			filter.Property2 = new ZDateTime(2000, 1, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection to contain Header1", FilterCollection.Contains(Header1));
			Assert("Expecting collection not to contain Header2", !FilterCollection.Contains(Header2));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2000, 1, 2);
			filter.Property2 = new ZDateTime(2000, 2, 1);
			filter.IsActive = true;

			FilterCollection.Load(FilterBO.Filter);

			Assert("Expecting collection not to contain Header1", !FilterCollection.Contains(Header1));
			Assert("Expecting collection not to contain Header2", !FilterCollection.Contains(Header2));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new InvoiceBatchFilterBusinessObject();
		}

		OrgHeader Debtor1;
		OrgHeader Debtor2;
		OrgHeader Debtor3;

		InvoiceBatchHeader Header1;
		InvoiceBatchHeader Header2;

		InvoiceBatchHeaderCollection FilterCollection;
		InvoiceBatchFilterBusinessObject FilterBO;

		protected override void SetUp()
		{
			base.SetUp();

			Debtor1 = Factory.NewWithValidTestData<OrgHeader>();
			Debtor2 = Factory.NewWithValidTestData<OrgHeader>();
			Debtor3 = Factory.NewWithValidTestData<OrgHeader>();

			Debtor1.CompanyData.OB_IsDebtor = true;
			Debtor2.CompanyData.OB_IsDebtor = true;
			Debtor3.CompanyData.OB_IsDebtor = true;

			Header1 = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			Header2 = Factory.NewWithValidTestData<InvoiceBatchHeader>();

			FilterCollection = new InvoiceBatchHeaderCollection(Factory);
			FilterBO = (InvoiceBatchFilterBusinessObject)GetNewFilterStripBusinessObject();
		}
	}
}
